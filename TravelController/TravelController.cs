﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.GPS;
using TravelsClient;
using System.Threading;

namespace TravelController
{
    public class TravelController : ITravelController, IDisposable
    {
        public event MetricsUpdatedEventHandler MetricsUpdated;

        private enum States { NotStarted, FindingOpenedTravel, ToOpenNewTravel, CreatingNewTravel, ExportingPoints, Ready }

        private readonly IHostController hc;
        private readonly AsyncClient client;
        private readonly List<TravelPoint> bufferedPoints = new List<TravelPoint>();
        private readonly GPSLogFilter logFilter;
        private readonly IHostTimer timer;

        private const int minimalExportRateMs = 5000;
        private readonly int exportRateMs;
        private const int preparingRateMs = 5000;

        private Travel travel;

        private LockingProperty<States> state = new LockingProperty<States>(States.NotStarted);

        private volatile int metricsBufferedPoints;
        private volatile int metricsSendedPoints;
        private bool metricsError = true;

        private double travelDistance;
        private GPRMC firstGprmc;
        private GPRMC previousGprmc;

        private bool disposed;
        private string requestNewTrawel;
        private volatile int requestCustomPoint;

        public int BufferedPoints
        {
            get { return metricsBufferedPoints; }
        }
        public int SendedPoints
        {
            get { return metricsSendedPoints; }
        }
        public TimeSpan TravelTime
        {
            get 
            {
                if (firstGprmc == null || previousGprmc == null)
                    return TimeSpan.FromMinutes(0);

                return (previousGprmc.Time - firstGprmc.Time);
            }
        }
        public double TravelDistance
        {
            get { return travelDistance; }
        }

        public TravelController(IHostController hc)
        {
            if (hc == null)
                throw new ArgumentNullException("hc");

            this.hc = hc;

            logFilter = new GPSLogFilter(hc.Config.GetDouble(ConfigNames.TravelServiceGPSFilterDistanceToSpeedRatio), 
                                         hc.Config.GetInt(ConfigNames.TravelServiceGPSFilterDeadZoneMeters));

            this.exportRateMs = Math.Max(minimalExportRateMs, hc.Config.GetInt(ConfigNames.TravelServiceExportRateSeconds) * 1000);

            if ((client = CreateClient(hc.Config, hc.Logger)) != null)
            {
                hc.GetController<IGPSController>().GPRMCReseived += GPRMCReseived;
                this.timer = hc.CreateTimer(preparingRateMs, Operate, true);
            }
        }

        private void FindOpenedTravel()
        {
            if (client.TryFindActiveTravelAsync(result =>
                {
                    if (result.Success)
                    {
                        if (result.Travel != null)
                        {
                            var minutesGapToOpenNewTravel = hc.Config.GetInt(ConfigNames.TravelServiceMinutesGapToOpenNewTravel);

                            if (result.Travel.Closed)
                            {
                                this.travel = null;
                                state.Value = States.ToOpenNewTravel;
                                hc.Logger.Log(this, string.Format("Found travel is closed. Id={0}", result.Travel.ID), LogLevels.Warning);
                            }
                            else if (result.Travel.EndTime.AddMinutes(minutesGapToOpenNewTravel) <= DateTime.Now.ToUniversalTime())
                            {
                                this.travel = null;
                                state.Value = States.ToOpenNewTravel;
                                hc.Logger.Log(this, string.Format("Found travel Id={0} was closed more than {1} minutes ago. New travel will be created.", result.Travel.ID, minutesGapToOpenNewTravel), LogLevels.Info);
                            }
                            else
                            {
                                this.travel = result.Travel;
                                state.Value = States.Ready;
                                hc.Logger.Log(this, string.Format("Opened travel found, Id={0}", result.Travel.ID), LogLevels.Info);
                            }
                        }
                        else
                        {
                            this.travel = null;
                            state.Value = States.ToOpenNewTravel;
                            hc.Logger.Log(this, "Opened travel not found", LogLevels.Info);
                        }

                        metricsError = false;
                    }
                    else
                    {
                        hc.Logger.Log(this, "Error while finding opened travel:", LogLevels.Warning);
                        hc.Logger.Log(this, result.Error, LogLevels.Warning);
                        state.Value = States.NotStarted;
                        metricsError = true;
                    }
                }))
            {
                hc.Logger.LogIfDebug(this, "FindOpenedTravel operation started.");
                state.Value = States.FindingOpenedTravel;
                metricsError = false;
            }
            else
            {
                hc.Logger.Log(this, "Unable to start FindOpenedTravel operation", LogLevels.Warning);
                metricsError = true;
            }
        }

        private bool OpenNewTravel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = CreateNewTravelName();

            if (client.TryOpenNewTravel(name, result => 
            {
                if (result.Success)
                {
                    this.travel = result.Travel;
                    state.Value = States.Ready;
                    hc.Logger.Log(this, string.Format("New travel was opened succesfully, Id={0}", result.Travel.ID), LogLevels.Info);
                    metricsError = false;
                    requestNewTrawel = null;
                }
                else
                {
                    this.travel = null;
                    state.Value = States.NotStarted;
                    hc.Logger.Log(this, "Error while opening new travel:", LogLevels.Warning);
                    hc.Logger.Log(this, result.Error, LogLevels.Warning);
                    metricsError = true;
                }
            }))
            {
                state.Value = States.CreatingNewTravel;
                hc.Logger.LogIfDebug(this, "OpenNewTravel operation started");
                metricsError = false;
                return true;
            }
            else
            {
                hc.Logger.Log(this, "Unable to start OpenNewTravel operation", LogLevels.Warning);
                metricsError = true;
                return false;
            }
        }

        private string CreateNewTravelName()
        {
            return string.Concat("Auto at", DateTime.Now.AddHours(hc.Config.GetInt(ConfigNames.SystemTimeLocalZone)).ToString("HH:mm"));
        }

        public void RequestNewTravel(string name)
        {
            requestNewTrawel = name;
            state.Value = States.NotStarted;
            timer.Span = preparingRateMs;
        }

        public void MarkCurrentPositionWithCustomPoint()
        {
            //4 tries to add custom points (if current GPRMC is not active)
            requestCustomPoint = 4;
        }

        private void Operate()
        {
            if (disposed)
            {
                return;
            }
            else if (!hc.Config.IsSystemTimeValid || !hc.Config.IsInternetConnected)
            {
                hc.Logger.LogIfDebug(this, string.Format("Skipping Operation because of precondition failed: SystemTimeIsValid = {0} and InternetIsConnected = {1}",
                                                                     hc.Config.IsSystemTimeValid, hc.Config.IsInternetConnected), LogLevels.Warning);
                state.Value = States.NotStarted;
                travel = null;
                metricsError = true;
            }
            else
            {
                switch (state.Value)
                {
                    case States.NotStarted:

                        if (!string.IsNullOrWhiteSpace(requestNewTrawel))
                        {
                            timer.Span = preparingRateMs;
                            state.Value = States.ToOpenNewTravel;
                        }
                        else
                        {
                            int count = 0;

                            lock (bufferedPoints)
                            {
                                count = bufferedPoints.Count;
                            }

                            var minPointsToStart = hc.Config.GetInt(ConfigNames.TravelServiceMinPointsCountToStart);

                            if (bufferedPoints.Count >= minPointsToStart)
                            {
                                timer.Span = preparingRateMs;
                                FindOpenedTravel();
                            }
                            else
                            {
                                hc.Logger.Log(this, string.Concat("Skipping export because points < ", minPointsToStart), LogLevels.Info);
                            }
                        }
                        
                        break;

                    case States.ToOpenNewTravel:
                        OpenNewTravel(null);
                        break;

                    case States.Ready:
                        timer.Span = exportRateMs;
                        ExportPoints();
                        break;

                    default:
                        hc.Logger.LogIfDebug(this, string.Format("Skipping sheduled operation because of current state is {0}", state.Value));
                        break;
                }

                metricsError = false;
            }

            UpdateMetrics();
        }

        private void UpdateMetrics()
        {
            var metrics = new Metrics("Travel Controller", 5);
            metrics.Add(0, "", travel != null ? travel.Name : "NO TRAVEL");
            metrics.Add(1, "State", state.Value);
			metrics.Add(2, "Sended points", metricsSendedPoints);
            metrics.Add(3, "Buffered points", metricsBufferedPoints);
            metrics.Add(4, "_is_error", metricsError);

            var handler = MetricsUpdated;
            if (handler != null)
                handler(this, metrics);
        }

        private void ExportPoints()
        {
            if (state.Value != States.Ready)
                return;

            var pointsToExport = new List<TravelPoint>();

            lock (bufferedPoints)
            {
                pointsToExport.AddRange(bufferedPoints);
                bufferedPoints.Clear();
                metricsBufferedPoints = 0;
            }

            if (pointsToExport.Any())
            {
                hc.Logger.LogIfDebug(this, string.Format("{0} point(s) are ready to export", pointsToExport.Count));

                if (client.TryAddTravelPoints(pointsToExport, travel, result =>
                    {
                        if (result.Success)
                        {
                            metricsSendedPoints += pointsToExport.Count;
                            hc.Logger.LogIfDebug(this, "Point(s) were exported succesfully");
                            metricsError = false;
                        }
                        else
                        {
                            lock (bufferedPoints)
                            {
                                bufferedPoints.AddRange(pointsToExport);
                                metricsBufferedPoints = bufferedPoints.Count;
                            }
                            hc.Logger.Log(this, "Attempt to add Travel points was failed.", LogLevels.Warning);
                            metricsError = false;
                        }

                        state.Value = States.Ready;
                        UpdateMetrics();
                    }))
                {
                    state.Value = States.ExportingPoints;
                    hc.Logger.LogIfDebug(this, "ExportPoints operation started");
                    metricsError = false;
                }
                else
                {
                    hc.Logger.Log(this, "Unable to start ExportPoints operation", LogLevels.Warning);
                    metricsError = true;
                }
            }
        }

        private void GPRMCReseived(GPRMC gprmc)
        {
            if (!disposed)
            {
                if (gprmc.Active)
                {
                    ProcessStatistics(gprmc);

                    if (requestCustomPoint > 0 || logFilter.Match(gprmc))
                    {
                        lock (bufferedPoints)
                        {
                            bufferedPoints.Add(new TravelPoint
                            {
                                Type = requestCustomPoint > 0 ? TravelPointTypes.ManualTrackPoint : TravelPointTypes.AutoTrackPoint,
                                Lat = gprmc.Location.Lat.Degrees,
                                Lon = gprmc.Location.Lon.Degrees,
                                Speed = gprmc.Speed,
                                Time = gprmc.Time.ToUniversalTime()
                            });

                            metricsBufferedPoints = bufferedPoints.Count;
                        }

                        hc.Logger.LogIfDebug(this, "GPRMC was added to Travel Controller");
                        UpdateMetrics();
                        requestCustomPoint = 0;
                    }
                }
                else if (requestCustomPoint > 0)
                {
                    requestCustomPoint--;
                }
            }
        }

        private void ProcessStatistics(GPRMC gprmc)
        {
            if (firstGprmc == null)
            {
                firstGprmc = gprmc;
            }
            else
            {
                travelDistance += Interfaces.GPS.Helpers.GetDistance(previousGprmc.Location, gprmc.Location);
            }

            previousGprmc = gprmc;
        }

        private AsyncClient CreateClient(IConfig config, ILogger logger)
        {
            try
            {
                if (config == null)
                    throw new ArgumentNullException("config");

                var serverUrl = config.GetString(ConfigNames.TravelServiceUrl);
                var vehicleId = config.GetString(ConfigNames.TravelServiceVehicle);
                var key = config.GetString(ConfigNames.TravelServiceKey);

                logger.LogIfDebug(this, string.Format("Travel client created successfully for URL {0}, vehicle Id={1}", serverUrl, vehicleId));

                return new AsyncClient(new Uri(serverUrl), key, vehicleId, logger);
            }
            catch (Exception ex)
            {
                logger.Log(this, "Unable to create TravelClient", LogLevels.Error);
                logger.Log(this, ex);
                return null;
            }
        }

        public void Dispose()
        {
            disposed = true;
            timer.Dispose();

            if (bufferedPoints.Count > 0 && state.Value == States.Ready && hc.Config.IsSystemTimeValid && hc.Config.IsInternetConnected)
            {
                hc.Logger.Log(this, "Trying to send buffered points...", LogLevels.Info);

                ExportPoints();

                while (state.Value == States.ExportingPoints)
                {
                    Thread.Sleep(1000);
                }

                hc.Logger.Log(this, "Buffered points were successfully sended...", LogLevels.Info);
            }
        }
    }
}

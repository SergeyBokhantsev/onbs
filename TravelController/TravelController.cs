using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.GPS;
using TravelsClient;
using System.Threading;
using UIModels.Dialogs;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TravelControllerNamespace
{
    public class TravelMetricsProvider : MetricsProvider
    {
        public GenericMetric<string> Travel = new GenericMetric<string>("Travel name", "NO TRAVEL");
        public GenericMetric<TravelController.States> State = new GenericMetric<TravelController.States>("State", TravelController.States.NotStarted);
        public GenericMetric<int> SendedPoints = new GenericMetric<int>("Sended points", 0);
        public GenericMetric<int> BufferedPoints = new GenericMetric<int>("Buffered points", 0);

        public TravelMetricsProvider(ILogger logger)
            : base(logger, "Travel Controller")
        {
            Initialize(Travel, State, SendedPoints, BufferedPoints);
        }
    }

    public class TravelController : ITravelController
    {
        private const string storedPointsFileName = "StoredTravelPoints.bin";

        public enum States { NotStarted, FindingOpenedTravel, ToOpenNewTravel, CreatingNewTravel, ExportingPoints, Ready }

        private readonly IHostController hc;
        private readonly Client client;
        private readonly List<TravelPoint> bufferedPoints;
        private bool locallyStoredPointsLoaded;
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
        private volatile int exportErrorCount;

        private readonly TravelMetricsProvider metricsProvider;

        private double travelDistance;
        private GPRMC firstGprmc;
        private GPRMC previousGprmc;

        private bool disposed;
        private volatile int requestCustomPoint;

        private int exportBatchSize;

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

            metricsProvider = new TravelMetricsProvider(hc.Logger);
            hc.MetricsService.RegisterProvider(metricsProvider);

            exportBatchSize = hc.Config.GetInt(ConfigNames.TravelServiceExportBatchSize);

            bufferedPoints = LoadPointsLocally() ?? new List<TravelPoint>();

            logFilter = new GPSLogFilter(hc.Config.GetDouble(ConfigNames.TravelServiceGPSFilterDistanceToSpeedRatio),
                                         hc.Config.GetInt(ConfigNames.TravelServiceGPSFilterDeadZoneMeters),
                                         hc.Config.GetInt(ConfigNames.TravelServiceGPSFilterDeadZoneSpeed));

            this.exportRateMs = Math.Max(minimalExportRateMs, hc.Config.GetInt(ConfigNames.TravelServiceExportRateSeconds) * 1000);

            if ((client = CreateClient(hc.Config, hc.Logger)) != null)
            {
                hc.GetController<IGPSController>().GPRMCReseived += GPRMCReseived;
                this.timer = hc.CreateTimer(preparingRateMs, Operate, true, true, "travel operational timer");
            }
        }

        private async void FindOpenedTravel()
        {
            metricsError = false;

            state.Value = States.FindingOpenedTravel;

            var result = await client.FindActiveTravelAsync();

            if (result.Success)
            {
                if (result.Value.Closed)
                {
                    this.travel = null;
                    state.Value = States.ToOpenNewTravel;
                    hc.Logger.Log(this, string.Format("Found travel is closed. Id={0}", result.Value.ID), LogLevels.Warning);
                }
                else
                {
                    if (locallyStoredPointsLoaded)
                    {
                        this.travel = result.Value;
                        state.Value = States.Ready;
                        hc.Logger.Log(this, string.Format("Continuing travel Id={0} because some not pushed points exists", result.Value.ID), LogLevels.Info);
                    }
                    else
                    {
                        var minutesGapToOpenNewTravel = hc.Config.GetInt(ConfigNames.TravelServiceMinutesGapToOpenNewTravel);

                        if (result.Value.EndTime.AddMinutes(minutesGapToOpenNewTravel) > DateTime.Now)
                        {
                            this.travel = result.Value;
                            state.Value = States.Ready;
                            hc.Logger.Log(this, string.Format("Continuing travel Id={0} because existing travel time match", result.Value.ID), LogLevels.Info);
                        }
                        else
                        {
                            var dRes = await hc.GetController<IUIController>().ShowDialogAsync(new YesNoDialog("Travel exist", "Start new travel or continue last one?", "(Y)Start new", "(N)Continue", hc, 60000, Interfaces.UI.DialogResults.Yes));

                            if (dRes == Interfaces.UI.DialogResults.No)
                            {
                                this.travel = result.Value;
                                state.Value = States.Ready;
                                hc.Logger.Log(this, string.Format("Continuing travel Id={0} because of user confirmation", result.Value.ID), LogLevels.Info);
                            }
                            else
                            {
                                this.travel = null;
                                state.Value = States.ToOpenNewTravel;
                                hc.Logger.Log(this, string.Format("Found travel Id={0} but user had declined it. New travel will be created.", result.Value.ID), LogLevels.Info);
                            }
                        }
                    }
                }
            }
            else if (result.HttpCode == System.Net.HttpStatusCode.NotFound)
            {
                this.travel = null;
                state.Value = States.ToOpenNewTravel;
                hc.Logger.Log(this, "Opened travel not found", LogLevels.Info);
            }
            else
            {
                hc.Logger.Log(this, "Error while finding opened travel:", LogLevels.Warning);
                hc.Logger.Log(this, result.ErrorMessage, LogLevels.Warning);
                state.Value = States.NotStarted;
                metricsError = true;
            }
        }

        private async void OpenNewTravel()
        {
            state.Value = States.CreatingNewTravel;

            var result = await client.OpenTravelAsync(CreateNewTravelName());

            if (result.Success)
            {
                this.travel = result.Value;
                state.Value = States.Ready;
                hc.Logger.Log(this, string.Format("New travel was opened succesfully, Id={0}", result.Value.ID), LogLevels.Info);
                metricsError = false;
            }
            else
            {
                this.travel = null;
                state.Value = States.NotStarted;
                hc.Logger.Log(this, "Error while opening new travel:", LogLevels.Warning);
                hc.Logger.Log(this, result.ErrorMessage, LogLevels.Warning);
                metricsError = true;
            }
        }

        private string CreateNewTravelName()
        {
            return string.Concat("Auto at", DateTime.Now.AddHours(hc.Config.GetInt(ConfigNames.SystemTimeLocalZone)).ToString("HH:mm"));
        }

        public void MarkCurrentPositionWithCustomPoint()
        {
            //4 tries to add custom points (if current GPRMC is not active)
            requestCustomPoint = 4;
        }

        private async void Operate(IHostTimer timer)
        {
            if (disposed)
            {
                return;
            }
            else if (!hc.Config.IsSystemTimeValid || !hc.Config.IsInternetConnected)
            {
                hc.Logger.LogIfDebug(this, string.Format("Skipping Operation because of precondition failed: SystemTimeIsValid = {0} and InternetIsConnected = {1}",
                                                                     hc.Config.IsSystemTimeValid, hc.Config.IsInternetConnected));
                metricsError = true;
            }
            else
            {
                switch (state.Value)
                {
                    case States.NotStarted:

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
                            hc.Logger.Log(this, string.Concat("Skipping export because points < ", minPointsToStart), LogLevels.Debug);
                        }
                        break;

                    case States.ToOpenNewTravel:
                        OpenNewTravel();
                        break;

                    case States.Ready:
                        timer.Span = exportRateMs;
                        await ExportPoints();
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
            metricsProvider.OpenBatch();

            metricsProvider.SummaryState = metricsError ? ColoredStates.Red : ColoredStates.Normal;

            metricsProvider.Travel.Set(travel != null ? travel.Name : "NO TRAVEL");
            metricsProvider.State.Set(state.Value);
            metricsProvider.SendedPoints.Set(metricsSendedPoints);
            metricsProvider.BufferedPoints.Set(metricsBufferedPoints);

            metricsProvider.CommitBatch();
        }

        private async Task<bool> ExportPoints()
        {
            var isSended = false;

            if (state.Value != States.Ready)
                return isSended;

            state.Value = States.ExportingPoints;

            var pointsToExport = new List<TravelPoint>();
            
            lock (bufferedPoints)
            {
                if (bufferedPoints.Any())
                {
                    int batchSize = Math.Min(10, bufferedPoints.Count);
                    pointsToExport.AddRange(bufferedPoints.Take(batchSize));
                    bufferedPoints.RemoveRange(0, batchSize);
                    metricsBufferedPoints = bufferedPoints.Count;
                }
            }

            if (pointsToExport.Any())
            {
                hc.Logger.LogIfDebug(this, string.Format("{0} point(s) are ready to export", pointsToExport.Count));

                var result = await client.AddTravelPointAsync(pointsToExport, travel);

                if (result.Success)
                {
                    metricsSendedPoints += pointsToExport.Count;
                    hc.Logger.LogIfDebug(this, "Point(s) were exported succesfully");
                    metricsError = false;
                    exportErrorCount = 0;
                    isSended = true;
                }
                else
                {
                    lock (bufferedPoints)
                    {
                        bufferedPoints.AddRange(pointsToExport);
                        metricsBufferedPoints = bufferedPoints.Count;
                        //DumpPoints();
                    }

                    hc.Logger.Log(this, string.Concat("Attempt to add Travel points was failed: ", result.ErrorMessage), LogLevels.Warning);

                    metricsError = false;

                    exportErrorCount++;
                    if (exportErrorCount > 2)
                    {
                        lock (bufferedPoints)
                        {
                            hc.Logger.Log(this, string.Format("Clearing following {0} points because of repeating failings...", bufferedPoints.Count), LogLevels.Warning);
                            bufferedPoints.Clear();
                        }
                    }
                }

                UpdateMetrics();
            }

            state.Value = States.Ready;

            return isSended;
        }

        private void DumpPoints()
        {
            StringBuilder body = new StringBuilder();

            lock (bufferedPoints)
            {
                foreach (var tp in bufferedPoints)
                {
                    var lat = string.Concat("LAT=", tp.Lat);
                    var lon = string.Concat("LON=", tp.Lon);
                    var speed = string.Concat("SPEED=", tp.Speed);
                    var type = string.Concat("TYPE=", (int)tp.Type);
                    var time = string.Concat("TIME=", tp.Time.ToFileTimeUtc());
                    var descr = string.Concat("DESCR=", tp.Description);

                    body.Append(string.Join(";", lat, lon, speed, type, time, descr));
                    body.Append("|");
                }
            }

            if (body.Length > 0)
            {
                body.Remove(body.Length - 1, 1);

                var dumpFilePath = Path.Combine(hc.Config.DataFolder, Guid.NewGuid().ToString() + ".txt");
                File.WriteAllText(dumpFilePath, body.ToString());

                hc.Logger.Log(this, string.Concat("TPoints dumped to ", dumpFilePath), LogLevels.Info);
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

        private Client CreateClient(IConfig config, ILogger logger)
        {
            try
            {
                if (config == null)
                    throw new ArgumentNullException("config");

                var serverUrl = config.GetString(ConfigNames.TravelServiceUrl);
                var vehicleId = config.GetString(ConfigNames.TravelServiceVehicle);
                var key = config.GetString(ConfigNames.TravelServiceKey);

                logger.LogIfDebug(this, string.Format("Travel client created successfully for URL {0}, vehicle Id={1}", serverUrl, vehicleId));

                return new Client(new Uri(serverUrl), key, vehicleId, logger);
            }
            catch (Exception ex)
            {
                logger.Log(this, "Unable to create TravelClient", LogLevels.Error);
                logger.Log(this, ex);
                return null;
            }
        }

        public async Task ShutdownAsync()
        {
            if (!disposed)
            {
                disposed = true;
                hc.GetController<IGPSController>().GPRMCReseived -= GPRMCReseived;
                timer.Dispose();

                hc.MetricsService.UnregisterProvider(metricsProvider);

                var lastKnownGprmc = logFilter.LastKnownLocation;

                if (lastKnownGprmc != null && lastKnownGprmc.Active)
                {
                    lock (bufferedPoints)
                    {
                        if (!bufferedPoints.Any()
                             || bufferedPoints.Last().Lat != lastKnownGprmc.Location.Lat.Degrees
                             || bufferedPoints.Last().Lon != lastKnownGprmc.Location.Lon.Degrees)
                        {
                            bufferedPoints.Add(new TravelPoint
                            {
                                Type = TravelPointTypes.AutoTrackPoint,
                                Lat = lastKnownGprmc.Location.Lat.Degrees,
                                Lon = lastKnownGprmc.Location.Lon.Degrees,
                                Speed = lastKnownGprmc.Speed,
                                Time = lastKnownGprmc.Time.ToUniversalTime()
                            });
                        }
                    }
                }

                if (bufferedPoints.Count > 0)
                {
                    var toSavePointsLocally = state.Value != States.Ready || !hc.Config.IsSystemTimeValid || !hc.Config.IsInternetConnected;

                    if (!toSavePointsLocally)
                    {
                        hc.Logger.Log(this, "Trying to send buffered points...", LogLevels.Info);

                        var sendTask = ExportPoints();
                        await Task.WhenAny(sendTask, Task.Delay(15000));

                        if (sendTask.IsCompleted && sendTask.Result)
                            hc.Logger.Log(this, "Buffered points were successfully sended.", LogLevels.Info);
                        else
                        {
                            client.Cancel();
                            hc.Logger.Log(this, "Buffered points were not sended in 15 seconds.", LogLevels.Warning);
                            toSavePointsLocally = true;
                        }
                    }

                    if (toSavePointsLocally)
                    {
                        hc.Logger.Log(this, "Saving buffered point to a file.", LogLevels.Info);
                        SavePointsLocally();
                    }
                }
            }
        }

        private void SavePointsLocally()
        {
            var filePath = Path.Combine(hc.Config.DataFolder, storedPointsFileName);

            lock (bufferedPoints)
            {
                if (bufferedPoints.Count > 0)
                {
                    using (var fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var serializer = new BinaryFormatter();
                        serializer.Serialize(fs, bufferedPoints);
                    }

                    hc.Logger.Log(this, string.Concat("Buffered points were successfully saved to ", filePath), LogLevels.Info);
                }
                else
                {
                    hc.Logger.Log(this, "No points to save...", LogLevels.Info);
                }
            }
        }

        private List<TravelPoint> LoadPointsLocally()
        {
            List<TravelPoint> res = null;

            try
            {
                var filePath = Path.Combine(hc.Config.DataFolder, storedPointsFileName);

                if (File.Exists(filePath))
                {
                    using (var fs = File.OpenRead(filePath))
                    {
                        var serializer = new BinaryFormatter();
                        res = serializer.Deserialize(fs) as List<TravelPoint>;
                    }

                    File.Delete(filePath);

                    locallyStoredPointsLoaded = true;
                    hc.Logger.Log(this, string.Concat("Buffered points were successfully loaded from ", filePath), LogLevels.Info);
                }
            }
            catch (Exception ex)
            {
                hc.Logger.Log(this, ex);
            }

            return res;
        }
    }
}

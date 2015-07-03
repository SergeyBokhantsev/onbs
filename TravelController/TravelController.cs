using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.GPS;
using TravelsClient;

namespace TravelController
{
    public class TravelController : ITravelController
    {
        public event MetricsUpdatedEventHandler MetricsUpdated;

        private enum States { NotStarted, FindingOpenedTravel, OpenedTravelNotFound, CreatingNewTravel, ExportingPoints, Ready }

        private readonly IHostController hc;
        private readonly AsyncClient client;
        private readonly List<TravelPoint> bufferedPoints = new List<TravelPoint>();
        private readonly GPSLogFilter logFilter;
        private readonly IDispatcherTimer timer;

        private const int minimalExportRateMs = 5000;
        private readonly int exportRateMs;

        private Travel travel;

        private LockingProperty<States> state = new LockingProperty<States>(States.NotStarted);

        private volatile int metricsBufferedPoints;
        private volatile int metricsSendedPoints;

        public TravelController(IHostController hc)
        {
            if (hc == null)
                throw new ArgumentNullException("hc");

            this.hc = hc;

            logFilter = new GPSLogFilter(hc.Config.GetInt(ConfigNames.TravelServiceGPSFilterMaxGapSeconds), hc.Config.GetInt(ConfigNames.TravelServiceGPSFilterMaxGapMeters));

            this.exportRateMs = Math.Max(minimalExportRateMs, hc.Config.GetInt(ConfigNames.TravelServiceExportRateSeconds) * 1000);

            if ((client = CreateClient(hc.Config, hc.Logger)) != null)
            {
                hc.GetController<IGPSController>().GPRMCReseived += GPRMCReseived;
                this.timer = hc.Dispatcher.CreateTimer(3000, Operate);
                hc.Dispatcher.Invoke(this, null, (s, e) => timer.Enabled = true);
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
                                state.Value = States.OpenedTravelNotFound;
                                hc.Logger.Log(this, string.Format("Found travel is closed. Id={0}", result.Travel.ID), LogLevels.Warning);
                            }
                            else if (result.Travel.EndTime.AddMinutes(minutesGapToOpenNewTravel) <= DateTime.Now.ToUniversalTime())
                            {
                                this.travel = null;
                                state.Value = States.OpenedTravelNotFound;
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
                            state.Value = States.OpenedTravelNotFound;
                            hc.Logger.Log(this, "Opened travel not found", LogLevels.Info);
                        }
                    }
                    else
                    {
                        hc.Logger.Log(this, "Error while finding opened travel:", LogLevels.Warning);
                        hc.Logger.Log(this, result.Error, LogLevels.Warning);
                        state.Value = States.OpenedTravelNotFound;
                    }
                }))
            {
                hc.Logger.LogIfDebug(this, "FindOpenedTravel operation started.");
                state.Value = States.FindingOpenedTravel;
            }
            else
            {
                hc.Logger.Log(this, "Unable to start FindOpenedTravel operation", LogLevels.Warning);
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
                }
                else
                {
                    this.travel = null;
                    state.Value = States.NotStarted;
                    hc.Logger.Log(this, "Error while opening new travel:", LogLevels.Warning);
                    hc.Logger.Log(this, result.Error, LogLevels.Warning);
                }
            }))
            {
                state.Value = States.CreatingNewTravel;
                hc.Logger.LogIfDebug(this, "OpenNewTravel operation started");
                return true;
            }
            else
            {
                hc.Logger.Log(this, "Unable to start OpenNewTravel operation", LogLevels.Warning);
                return false;
            }
        }

        private string CreateNewTravelName()
        {
            return DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        private void Operate(object sender, EventArgs e)
        {
            if (!hc.Config.IsSystemTimeValid || !hc.Config.IsInternetConnected)
            {
                hc.Logger.LogIfDebug(this, string.Format("Skipping Operation because of precondition failed: SystemTimeIsValid = {0} and InternetIsConnected = {1}", 
                    hc.Config.IsSystemTimeValid, hc.Config.IsInternetConnected), LogLevels.Warning);
                return;
            }

            switch(state.Value)
            {
                case States.NotStarted:
                    FindOpenedTravel();
                    break;

                case States.OpenedTravelNotFound:
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

            UpdateMetrics();
        }

        private void UpdateMetrics()
        {
            var metrics = new Metrics("Travel Controller", 3);
            metrics.Add(0, "", travel != null ? travel.Name : "NO TRAVEL");
            metrics.Add(1, "Sended points", metricsSendedPoints);
            metrics.Add(2, "Buffered points", metricsBufferedPoints);

            var handler = MetricsUpdated;
            if (handler != null)
                handler(this, metrics);
        }

        private void ExportPoints()
        {
            if (state.Value != States.Ready)
                return;

            var pointsToExport = new List<TravelPoint>();
            var newPoints = logFilter.WithdrawPoints();

            if (newPoints != null && newPoints.Any())
            {
                pointsToExport.AddRange(newPoints.Select(gprmc => new TravelPoint
                {
                    Type = TravelPointTypes.AutoTrackPoint,
                    Lat = gprmc.Location.Lat.Degrees,
                    Lon = gprmc.Location.Lon.Degrees,
                    Speed = gprmc.Speed,
                    Time = gprmc.Time.ToUniversalTime()
                }));
            }

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
                        }
                        else
                        {
                            lock (bufferedPoints)
                            {
                                bufferedPoints.AddRange(pointsToExport);
                                metricsBufferedPoints = bufferedPoints.Count;
                            }
                            hc.Logger.Log(this, "Attempt to add Travel points was failed.", LogLevels.Warning);
                        }

                        state.Value = States.Ready;
                    }))
                {
                    state.Value = States.ExportingPoints;
                    hc.Logger.LogIfDebug(this, "ExportPoints operation started");
                }
                else
                {
                    hc.Logger.Log(this, "Unable to start ExportPoints operation", LogLevels.Warning);
                }
            }
        }

        private void GPRMCReseived(GPRMC gprmc)
        {
           // if (gprmc.Active)
            {
                logFilter.Log(gprmc);
                hc.Logger.LogIfDebug(this, "GPRMC was provided to Travel Controller");
            }
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

        
    }
}

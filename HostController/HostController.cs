using Interfaces;
using System;
using System.Reflection;
using System.IO;
using System.Text;
using LogLib;
using System.Linq;
using System.Net;
using System.Threading;
using Interfaces.UI;
using Interfaces.Relays;
using HostController.Lin;
using HostController.Win;
using Interfaces.MiniDisplay;
using Implementation.MiniDisplay;
using System.Threading.Tasks;
using Interfaces.GPS;
using System.Collections.Generic;
using ProcessRunnerNamespace;
using TravelControllerNamespace;

namespace HostController
{
    public class HostController : IHostController
	{
        private string logFolder;
        
        private GPSD.Net.GPSD gpsd;
        //private InternetConnectionKeeper netKeeper;

        private ModemConnectionKeeper.ConnectionKeeper connectionKeeper;
        private ModemConnectionKeeper.Pinger pinger;

        private readonly InterlockedGuard systemTimeCorrectorGuard = new InterlockedGuard();

        private Configuration config;
        private UIController.UIController uiController;
        private InputController.InputController inputController;
        private ArduinoController.ArduinoController arduController;
        private GPSController.GPSController gpsController;
        private IAutomationController automationController;
        private TravelController travelController;
        private Elm327Controller.Elm327Controller elm327Controller;
        private MiniDisplayController miniDisplayController;
        private DashCamController.DashCamController dashCamController;

        private HostSynchronizationContext syncContext;
        private HostTimersController timersController;

        private TravelsClient.OnlineLogger onlineLogger;

		private int appliedTimeProviderPriority;

        private ISpeakService speakService;

        private DropboxService.DropboxService remoteStorageService;

        private MetricsService metricsService;

        private List<object> jobs = new List<object>();

        private ConsoleLoggerWrapper logger;

		private bool shutdownRun;

        public IConfig Config
        {
            get
            {
                return config;
            }
        }

        public ILogger Logger
        {
            get
            {
                return logger;
            }
        }
        public ONBSSyncContext SyncContext
        {
            get
            {
                return syncContext;
            }
        }

        public ISpeakService SpeakService
        {
            get
            {
                return speakService;
            }
        }

        public IRemoteStorageService RemoteStorageService
        {
            get
            {
                return remoteStorageService;
            }
        }

        public IMetricsService MetricsService
        {
            get
            {
                return metricsService;
            }
        }

        private Environments ResolveEnvironment()
        {
            var os = Environment.OSVersion;
            var pid = os.Platform;
            switch (pid)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return Environments.Win;

                case PlatformID.Unix:
                    return Environments.RPi;

                default:
                    throw new Exception("Unknown environment");
            }
        }

        public void Run()
        {            
			ThreadPool.SetMinThreads (20, 20);

            CreateConfig();

            CreateLogger();

            syncContext = new HostSynchronizationContext(Logger);

            timersController = new HostTimersController(syncContext, config);

            syncContext.Post(o => TryInitialize(), null, "HostController.TryInitialize()");
            syncContext.Pump();
        }

        public T GetController<T>() 
            where T : class, IController
        {
            if (typeof(T) == typeof(IUIController))
                return uiController as T;
            if (typeof(T) == typeof(IInputController))
                return inputController as T;
            if (typeof(T) == typeof(IArduinoController))
                return arduController as T;
            if (typeof(T) == typeof(IGPSController))
                return gpsController as T;
            if (typeof(T) == typeof(IAutomationController))
                return automationController as T;
            if (typeof(T) == typeof(ITravelController))
                return travelController as T;
            if (typeof(T) == typeof(IElm327Controller))
                return elm327Controller as T;
            if (typeof(T) == typeof(IMiniDisplayController))
                return miniDisplayController as T;
            if (typeof(T) == typeof(IDashCamController))
                return dashCamController as T;

            throw new NotImplementedException(typeof(T).ToString());
        }

        private void CreateConfig()
        {
            var env = ResolveEnvironment();
            ConfigValuesResolver configResolver;

            switch (env)
            {
                case Environments.Win:
                    configResolver = new MockConfigResolver();
                    break;

                case Environments.RPi:
                    configResolver = new RPiConfigResolver();
                    break;

                default:
                    throw new NotImplementedException();
            }

            config = new Configuration(configResolver) {Environment = env};
            config.IsSystemTimeValid = config.GetBool(ConfigNames.SystemTimeValidByDefault) || DateTime.Now.Year >= 2015;
        }

        private void CreateLogger()
        {
            var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrWhiteSpace(workingDir))
                throw new Exception("Working directory cannot be resolved");

            try
            {
                logFolder = Path.Combine(workingDir, Config.GetString(ConfigNames.LogFolder));
            }
            catch
            {
                logFolder = Path.Combine(workingDir, "Logs");
            }

            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            onlineLogger = new TravelsClient.OnlineLogger(Config, new Lazy<SynchronizationContext>(() => SyncContext));
            
            logger = new ConsoleLoggerWrapper(new ILogger[] { new GeneralLogger(Config), onlineLogger}, Config);
            logger.Log(this, "--- Logging initiated ---", LogLevels.Info);
            logger.Log(this, string.Format("Environment: {0}", Config.Environment), LogLevels.Info);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    var exception = e.ExceptionObject as Exception;
                    var exceptionFileName = WriteUnhandledException(exception);
                    Logger.Log(this, exception);
                    Logger.Log(this, string.Format("Unhandled exception occured, terminating application. Exception details logged to '{0}'", exceptionFileName), LogLevels.Fatal);

				if (!shutdownRun)
                    Shutdown(HostControllerShutdownModes.UnhandledException).Wait();
                };
        }

        private string WriteUnhandledException(Exception exception)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString());
                sb.Append(Environment.NewLine);
                sb.Append("EXCEPTION: ");
                sb.Append(exception != null ? exception.Message : "NULL");
                sb.Append(Environment.NewLine);
                sb.Append("CALL STACK: ");
                sb.Append(exception != null ? exception.StackTrace : "NULL");

                var fileName = string.Concat("EXEPTION_", DateTime.Now.ToFileTime().ToString(), "_", Guid.NewGuid().ToString(), ".txt");
                var filePath = Path.Combine(logFolder, fileName);
                File.WriteAllText(filePath, sb.ToString());

                return filePath;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task TryInitialize()
        {
            try
            {
                await Initialize();
            }
            catch (Exception ex)
            {
                Logger.Log(this, ex);
                syncContext.Stop();
            }
        }

        private void InitConnectionKeeper()
        {

        }

        private async Task Initialize()
        {
            ServicePointManager.ServerCertificateValidationCallback = (s1, s2, s3, s4) => true;

            metricsService = new MetricsService(Logger);

            //logger.Metrics = new LogMetrics("Warnings", LogLevels.Warning, 5);
            //metricsService.RegisterProvider(logger.Metrics);

            this.speakService = new SpeakService(Logger, Config);

			this.remoteStorageService = new DropboxService.DropboxService();

            var connectionMetrics = new ModemConnectionKeeper.ConnectionMetricsProvider(Logger);
            MetricsService.RegisterProvider(connectionMetrics);

            connectionKeeper = new ModemConnectionKeeper.ConnectionKeeper(Config, Logger) { Metrics = connectionMetrics };

            pinger = new ModemConnectionKeeper.Pinger(
                config.GetString("PingHost"),
                config.GetInt("PingInterval"),
                config.GetInt("PingRequestTimeout"),
                Logger)
                {
                     Metrics = connectionMetrics
                };

            if (Config.Environment == Environments.RPi)
            {
                pinger.ConnectionStatus += OnInternetConnectionStatus;
            }
            else
            {
                OnInternetConnectionStatus(true);
            }

            pinger.Start();

            //netKeeper = new InternetConnectionKeeper(Config, Logger);
            //netKeeper.InternetConnectionStatus += OnInternetConnectionStatus;
            //netKeeper.InternetTime += CheckSystemTimeFromInternet;
            //netKeeper.RestartNeeded += InetKeeperRestartNeeded;
            //netKeeper.StartChecking();

            inputController = new InputController.InputController(Logger, Config);

            StartJob(typeof(Jobs.CheckUserInputIdle), new object[] { this as IHostController });

            elm327Controller = new Elm327Controller.Elm327Controller(this);

            IPort arduPort = null;

            if (config.Environment == Environments.Win)
            {
                arduPort = new ArduinoEmulatorPort();
                await Task.Delay(2000); // to allow connect to emulator;
            }
            else
            {
                arduPort = new SerialArduPort(Logger, Config);
            }

            arduController = new ArduinoController.ArduinoController(arduPort, this);
            arduController.RegisterFrameAcceptor(inputController);

            miniDisplayController = new MiniDisplayController(Logger);
			arduController.RegisterFrameProvider(miniDisplayController);
            miniDisplayController.Graphics.Cls();
            miniDisplayController.Graphics.Invert(false);
            miniDisplayController.Graphics.Brightness(255);
            await miniDisplayController.WaitQueueFlushes();

            var gpsCtrl = new GPSController.GPSController(Config, SyncContext, Logger, MetricsService);
            gpsCtrl.GPRMCReseived += GPRMCReceived;

            arduController.RegisterFrameAcceptor(gpsCtrl);
            gpsController = gpsCtrl;
            gpsd = new GPSD.Net.GPSD(gpsController, Config, Logger);
            gpsd.Start();

            automationController = new AutomationController.AutomationController(this);

            travelController = new TravelController(this);

            var map = new ApplicationMap(Path.Combine(config.DataFolder, "application.xml"));

            uiController = new UIController.UIController(
                Config.GetString(ConfigNames.UIHostAssemblyName), 
                Config.GetString(ConfigNames.UIHostClass), 
                map,
                this, (pdescr, viewModelName, modelArgument) => UIModels.ModelBase.CreateModel(this, pdescr, viewModelName, modelArgument));

            uiController.DialogPending += value =>
            {
                config.IsMessagePending = value;
                if (value)
                    arduController.Beep(100, 50, 2);
            };

            uiController.DialogShown += dialog =>
            {
                config.IsMessageShown = null != dialog;

                if (null != dialog)
                {
                    miniDisplayController.ResetQueue();
                    miniDisplayController.Graphics.Delay(500);
                    miniDisplayController.Graphics.Cls();
                    miniDisplayController.Graphics.SetFont(Fonts.Small);
                    var caption = string.IsNullOrWhiteSpace(dialog.Caption) ? "NO CAPTION" : 
                        dialog.Caption.Length > 10 ? dialog.Caption.Substring(0, 10) : dialog.Caption;
                    miniDisplayController.Graphics.Print(0, 10, caption, TextAlingModes.Center);

                    int buttonCount = 0;
                    foreach (var btn in dialog.Buttons)
                    {
                        miniDisplayController.Graphics.Print(0, (byte)(20 + buttonCount * 10), btn.Value, TextAlingModes.Center);
                        buttonCount++;
                    }

                    miniDisplayController.Graphics.Update();
                }
            };

            uiController.PageChanging += (string descriptorName, string viewName) =>
            {
                miniDisplayController.ResetQueue();
                miniDisplayController.Graphics.Delay(500);
                miniDisplayController.Graphics.Cls();
                miniDisplayController.Graphics.SetFont(Fonts.Small);
                miniDisplayController.Graphics.Print(0, 10, "PAGE", TextAlingModes.Center);
                miniDisplayController.Graphics.Print(0, 25, descriptorName ?? "NO DESCR", TextAlingModes.Center);
                miniDisplayController.Graphics.Print(0, 40, viewName?? "def. view", TextAlingModes.Center);
                miniDisplayController.Graphics.Update();
            };

            uiController.ShowDefaultPage();

            if (config.Environment == Environments.RPi)
                gpsCtrl.GPRMCReseived += CheckSystemTimeFromGPS;

            dashCamController = new DashCamController.DashCamController(this);
            
            StartTimers();

            arduController.GetArduinoTime(t => syncContext.Post(async tt => await CheckSystemTimeFromArduino((DateTime)tt), t, "CheckSystemTimeFromArduino"));

            StartJob(typeof(Jobs.DimLightningCheckerJob), new object[] { this as IHostController, config });
            StartJob(typeof(Jobs.UploadLog), new object[] { this, onlineLogger });
            StartJob(typeof(Jobs.PhotoJob), new object[] { this });

            await SpeakService.Speak("System started");
        }

        private void StartJob(Type type, object[] constructorArgs)
        {
            try
            {
                var constructor = type.GetConstructor(constructorArgs.Select(p => p.GetType()).ToArray());
                if (constructor != null)
                {
                    var job = constructor.Invoke(constructorArgs);
                    jobs.Add(job);
                    Logger.Log(this, string.Format("Job of type {0} started", type), LogLevels.Info);
                }
                else
                {
                    Logger.Log(this, string.Format("Unable to start job of type {0}, because no apropriate constructor found.", type), LogLevels.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(this, string.Format("Unable to start job of type {0}, because of unexpected error", type), LogLevels.Warning);
                Logger.Log(this, ex);
            }
        }

        private async void GPRMCReceived(GPRMC gprmc)
        {
            if (config.IsGPSLock != gprmc.Active)
            {
                config.IsGPSLock = gprmc.Active;
                await SpeakService.Speak(gprmc.Active ? "GPS lock" : "GPS signal has been lost");
            }
        }

        private async void OnInternetConnectionStatus(bool status)
        {
            if (config.IsInternetConnected != status)
            {
                config.IsInternetConnected = status;
                await SpeakService.Speak(status ? "Internet connected" : "Internet connection has been lost");
            }
        }

        //private void InetKeeperRestartNeeded()
        //{
        //    netKeeper.RestartNeeded -= InetKeeperRestartNeeded;

        //    var dialog = new UIModels.Dialogs.YesNoDialog("Restart", "Failed to get internet connection. Restart now?", "Yes", "No", this, 30000, DialogResults.Yes);
        //    dialog.Closed += dr => 
        //    {
        //        if (dr == DialogResults.Yes)
        //        {
        //            Logger.Log(this, "Begin restart because of Internet keeper request...", LogLevels.Warning);
        //            SyncContext.Post(async o => await Shutdown(HostControllerShutdownModes.Restart), null, "Shutdown");
        //        }
        //    };

        //    uiController.ShowDialog(dialog);
        //}

        private async Task CheckSystemTimeFromArduino(DateTime time)
        {
            Logger.Log(this, string.Concat("CheckSystemTimeFromArduino handler called with proposed time ", time), LogLevels.Info);

            await systemTimeCorrectorGuard.ExecuteIfFreeAsync(() => Task.Run(() =>
            {
                Logger.Log(this, "Executing CheckSystemTimeFromArduino handler async...", LogLevels.Info);

				if (appliedTimeProviderPriority > 1)
				{
					Logger.Log(this, "Time already settet by more priority provider, ignoring.", LogLevels.Info);
					return;
				}

                if (new SystemTimeCorrector(Config, Logger).IsSystemTimeValid(time))
                {
                    config.IsSystemTimeValid = true;
					appliedTimeProviderPriority = 1;
                }
            }),
            ex => Logger.Log(this, ex));
        }

        private async void CheckSystemTimeFromGPS(Interfaces.GPS.GPRMC gprmc)
        {
			if (!gprmc.Active)
				return;

            Logger.Log(this, string.Concat("CheckSystemTimeFromGPS handler called with proposed time ", gprmc.Time), LogLevels.Info);

            await systemTimeCorrectorGuard.ExecuteIfFreeAsync(() => Task.Run(() =>
            {
                Logger.Log(this, "Executing CheckSystemTimeFromGPS handler async...", LogLevels.Info);

				if (appliedTimeProviderPriority > 3)
				{
					Logger.Log(this, "Time already settet by more priority provider, ignoring.", LogLevels.Info);
					return;
				}

                if (new SystemTimeCorrector(Config, Logger).IsSystemTimeValid(gprmc.Time))
                {
                    config.IsSystemTimeValid = true;
                    DisconnectSystemTimeChecking();

                    SyncContext.Post(s => arduController.SetTimeToArduino(), null, "Set GPS Time To Arduino");

					appliedTimeProviderPriority = 3;
                }
            }),
            ex => Logger.Log(this, ex));
        }

        private async void CheckSystemTimeFromInternet(DateTime inetTime)
        {
            Logger.Log(this, string.Concat("CheckSystemTimeFromInternet handler called with proposed time ", inetTime), LogLevels.Info);

            await systemTimeCorrectorGuard.ExecuteIfFreeAsync(() => Task.Run(() =>
            {
                Logger.Log(this, "Executing CheckSystemTimeFromInternet handler async...", LogLevels.Info);

				if (appliedTimeProviderPriority > 2)
				{
					Logger.Log(this, "Time already settet by more priority provider, ignoring.", LogLevels.Info);
					return;
				}

                if (new SystemTimeCorrector(Config, Logger).IsSystemTimeValid(inetTime))
                {
                    config.IsSystemTimeValid = true;
                    DisconnectSystemTimeChecking();

                    SyncContext.Post(s => arduController.SetTimeToArduino(), null, "Set inet Time To Arduino");

					appliedTimeProviderPriority = 2;
                }
            }),
            ex => Logger.Log(this, ex));
        }

        private void DisconnectSystemTimeChecking()
        {
            gpsController.GPRMCReseived -= CheckSystemTimeFromGPS;
            //netKeeper.InternetTime -= CheckSystemTimeFromInternet;
            Logger.Log(this, "SystemTimeCorrector has been disconnected.", LogLevels.Info);
        }

        public async Task Shutdown(HostControllerShutdownModes mode)
        {
			shutdownRun = true;

            try
            {
                arduController.RelayService.Enable(Relay.Relay3);
                await Task.Delay(400);
                arduController.RelayService.Disable(Relay.Relay3);
                //await Task.Delay(200);

                dashCamController.Dispose();

                await Task.Delay(200);

                var shutdownModel = uiController.ShowPage("ShutdownProgress", null, null) as UIModels.ShutdownProgressModel;

                Action<string> showLine = line => { if (shutdownModel != null) { shutdownModel.AddLine(line); } };

                showLine(string.Format("Begin shutdown in {0} mode", mode));
                Logger.Log(this, string.Format("Begin shutdown in {0} mode", mode), LogLevels.Info);

                showLine("Disposing ELM327 Controller");
                elm327Controller.Dispose();
                await Task.Delay(200);

                arduController.RelayService.Disable(Relay.OBD);
                await Task.Delay(100);
                arduController.RelayService.Disable(Relay.OBD);
                await Task.Delay(100);
                arduController.RelayService.Disable(Relay.Relay3);
                await Task.Delay(100);
                arduController.RelayService.Disable(Relay.Relay3);
                await Task.Delay(100);
                arduController.RelayService.Disable(Relay.Relay4);
                await Task.Delay(100);
                arduController.RelayService.Disable(Relay.Relay4);
                await Task.Delay(200);

                //arduController.RelayService.Enable(Relay.Relay3);
                //await Task.Delay(200);
                //arduController.RelayService.Disable(Relay.Relay3);
                //await Task.Delay(200);

                showLine("Disposing Travel Controller");
                await travelController.ShutdownAsync();
                await Task.Delay(200);             

                showLine("Stopping GPSD service");
                gpsd.Stop();
                await Task.Delay(200);

                showLine("Disconnecting time checking events");
                DisconnectSystemTimeChecking();
                await Task.Delay(200);

                showLine("Disabling GPS Controller");
                gpsController.Shutdown();
                await Task.Delay(200);

                if (mode != HostControllerShutdownModes.UnhandledException)
                {
                    try
                    {
                        showLine("Saving Configuration");
                        Config.Save();
                    }
                    catch (Exception ex)
                    {
                        showLine(ex.Message);
                        Logger.Log(this, ex);
                    }
                }

                Logger.Log(this, "--- Logging finished ---", LogLevels.Info);

                showLine("Flushing loggers");
                Logger.Flush();
                await Task.Delay(200);

                showLine("Flushing online log");

                await onlineLogger.DisableAndUpload(30000);

                showLine("Disposing InetKeeper");
                //netKeeper.Dispose();
                pinger.Dispose();
                connectionKeeper.Abort();
                await Task.Delay(200);

                showLine("Stopping UI...");

                miniDisplayController.ResetQueue();

                miniDisplayController.Dispose();
                await Task.Delay(200);

                uiController.Shutdown();
                await Task.Delay(200);

                arduController.StopPing();

                if (mode == HostControllerShutdownModes.Exit
                    || mode == HostControllerShutdownModes.Update)
                {
                    showLine("Sending HOLD POWER signal");
                    await arduController.HoldPower();
                }

                await Task.Delay(500);

                speakService.Shutdown();

                await arduController.Beep(100);

                showLine("Stopping timers");
                StopTimers();
                await Task.Delay(200);

                showLine("Stopping SyncContext");
            }
            catch (Exception ex)
            {
                WriteUnhandledException(ex);
                mode = HostControllerShutdownModes.UnhandledException;
            }

            switch (mode)
            {
                case HostControllerShutdownModes.Update:
                    {
                        try
                        {
                            await ProcessRunner.ExecuteToolAsync("launch update tool", (string str) => null as string, 10000,
                                Config.GetString(ConfigNames.SystemUpdateCommand),
                                string.Format(Config.GetString(ConfigNames.SystemUpdateArg), Config.DataFolder));
                        }
                        catch (Exception ex)
                        {
                            Logger.Log (this, ex);
                        }
                    }
                    break;

                case HostControllerShutdownModes.Restart:
                    {
                        try
                        {
                            await ProcessRunner.ExecuteToolAsync("restart", (string str) => null as string, 10000,
                                Config.GetString(ConfigNames.SystemRestartCommand),
                                Config.GetString(ConfigNames.SystemRestartArg));
                        }
                        catch (Exception ex)
                        {
                            Logger.Log (this, ex);
                        }
                    }
                    break;

                case HostControllerShutdownModes.Shutdown:
                    {
                        try
                        {
                            await ProcessRunner.ExecuteToolAsync("shutdown", (string str) => null as string, 10000,
                                Config.GetString(ConfigNames.SystemShutdownCommand),
                                Config.GetString(ConfigNames.SystemShutdownArg));
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(this, ex);
                        }
                    }
                    break;
            }

            Logger.Flush();

            syncContext.Stop();
        }

        public IHostTimer CreateTimer(int span, Action<IHostTimer> action, bool isEnabled, bool firstEventImmidiatelly, string name)
        {
            return timersController.CreateTimer(span, action, isEnabled, firstEventImmidiatelly, name);
        }

        private void StartTimers()
        {
            if (timersController == null)
                throw new InvalidOperationException("timersController is not created");

            timersController.Start();
        }

        private void StopTimers()
        {
            timersController.Dispose();
        }
    }
}


using Interfaces;
using System;
using System.Reflection;
using System.IO;
using System.Text;
using LogLib;
using System.Net;
using System.Threading;
using Interfaces.UI;
using Interfaces.Relays;
using HostController.Lin;
using HostController.Win;
using Interfaces.MiniDisplay;
using Implementation.MiniDisplay;
using System.Threading.Tasks;

namespace HostController
{
    public class HostController : IHostController, IProcessRunnerFactory
	{
        private string logFolder;
        
        private GPSD.Net.GPSD gpsd;
        private InternetConnectionKeeper netKeeper;

        private readonly InterlockedGuard systemTimeCorrectorGuard = new InterlockedGuard();

        private Configuration config;
        private UIController.UIController uiController;
        private IInputController inputController;
        private ArduinoController.ArduinoController arduController;
        private GPSController.GPSController gpsController;
        private IAutomationController automationController;
        private TravelController.TravelController travelController;
        private Elm327Controller.Elm327Controller elm327Controller;
        private MiniDisplayController miniDisplayController;

        private HostSynchronizationContext syncContext;
        private HostTimersController timersController;

        private TravelsClient.OnlineLogger onlineLogger;

        public IConfig Config
        {
            get
            {
                return config;
            }
        }

        public ILogger Logger
        {
            get;
            private set;
        }
        public ONBSSyncContext SyncContext
        {
            get
            {
                return syncContext;
            }
        }

        public IProcessRunnerFactory ProcessRunnerFactory
        {
            get
            {
                return this;
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
            CreateConfig();

            CreateLogger();

            syncContext = new HostSynchronizationContext(Logger);

            timersController = new HostTimersController(syncContext);

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
                    configResolver = new RPiConfigResolver(this);
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

            onlineLogger = new TravelsClient.OnlineLogger(Config);
            
            Logger = new ConsoleLoggerWrapper(new ILogger[] { new GeneralLogger(Config), onlineLogger});
            Logger.Log(this, "--- Logging initiated ---", LogLevels.Info);
            Logger.Log(this, string.Format("Environment: {0}", Config.Environment), LogLevels.Info);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    var exception = e.ExceptionObject as Exception;
                    var exceptionFileName = WriteUnhandledException(exception);
                    Logger.Log(this, exception);
                    Logger.Log(this, string.Format("Unhandled exception occured, terminating application. Exception details logged to '{0}'", exceptionFileName), LogLevels.Fatal);
                    Shutdown(HostControllerShutdownModes.UnhandledException);
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

        private async Task Initialize()
        {
            ServicePointManager.ServerCertificateValidationCallback = (s1, s2, s3, s4) => true;

            netKeeper = new InternetConnectionKeeper(Config, Logger, this);
            netKeeper.InternetConnectionStatus += connected => config.IsInternetConnected = connected;
            netKeeper.InternetTime += CheckSystemTimeFromInternet;
            netKeeper.RestartNeeded += InetKeeperRestartNeeded;
            netKeeper.StartChecking();

            inputController = new InputController.InputController(Logger);

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

            var gpsCtrl = new GPSController.GPSController(Config, SyncContext, Logger);
            gpsCtrl.GPRMCReseived += gprmc => config.IsGPSLock = gprmc.Active;

            arduController.RegisterFrameAcceptor(gpsCtrl);
            gpsController = gpsCtrl;
            gpsd = new GPSD.Net.GPSD(gpsController, Config, Logger);
            gpsd.Start();

            automationController = new AutomationController.AutomationController(this);

            travelController = new TravelController.TravelController(this);

            var map = new ApplicationMap(Path.Combine(config.DataFolder, "application.xml"));

            uiController = new UIController.UIController(
                Config.GetString(ConfigNames.UIHostAssemblyName), 
                Config.GetString(ConfigNames.UIHostClass), 
                map,
                this, (pdescr, viewModelName) => UIModels.ModelBase.CreateModel(this, pdescr, viewModelName));

            uiController.DialogPending += value =>
            {
                config.IsMessagePending = value;
                if (value)
                    arduController.Beep(100, 50, 2);
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
                gpsCtrl.GPRMCReseived += gprmc => CheckSystemTimeFromGPS(gprmc);

            // Timer for online log upoad
			CreateTimer(60000, ht => onlineLogger.Upload(false), true, false, "online logger timer");

            StartTimers();

            arduController.GetArduinoTime(t => syncContext.Post(tt => CheckSystemTimeFromArduino((DateTime)tt), t, "CheckSystemTimeFromArduino"));
        }

        private void InetKeeperRestartNeeded()
        {
            netKeeper.RestartNeeded -= InetKeeperRestartNeeded;

            var dialog = new UIModels.Dialogs.YesNoDialog("Restart", "Failed to get internet connection. Restart now?", "Yes", "No", this, 30000, DialogResults.Yes);
            dialog.Closed += dr => 
            {
                if (dr == DialogResults.Yes)
                {
                    Logger.Log(this, "Begin restart because of Internet keeper request...", LogLevels.Warning);
                    SyncContext.Post(o => Shutdown(HostControllerShutdownModes.Restart), null, "Shutdown");
                }
            };

            uiController.ShowDialog(dialog);
        }

        private void CheckSystemTimeFromArduino(DateTime time)
        {
            Logger.Log(this, string.Concat("CheckSystemTimeFromArduino handler called with proposed time ", time), LogLevels.Info);

            systemTimeCorrectorGuard.ExecuteIfFreeAsync(() =>
            {
                Logger.Log(this, "Executing CheckSystemTimeFromArduino handler async...", LogLevels.Info);

                if (new SystemTimeCorrector(Config, ProcessRunnerFactory, Logger).IsSystemTimeValid(time))
                {
                    config.IsSystemTimeValid = true;
                }
            },
            ex => Logger.Log(this, ex));
        }

        private void CheckSystemTimeFromGPS(Interfaces.GPS.GPRMC gprmc)
        {
			if (!gprmc.Active)
				return;

            Logger.Log(this, string.Concat("CheckSystemTimeFromGPS handler called with proposed time ", gprmc.Time), LogLevels.Info);

            systemTimeCorrectorGuard.ExecuteIfFreeAsync(() =>
            {
                Logger.Log(this, "Executing CheckSystemTimeFromGPS handler async...", LogLevels.Info);

                if (new SystemTimeCorrector(Config, ProcessRunnerFactory, Logger).IsSystemTimeValid(gprmc.Time))
                {
                    config.IsSystemTimeValid = true;
                    DisconnectSystemTimeChecking();
                    arduController.SetTimeToArduino();
                }
            },
            ex => Logger.Log(this, ex));
        }

        private void CheckSystemTimeFromInternet(DateTime inetTime)
        {
            Logger.Log(this, string.Concat("CheckSystemTimeFromInternet handler called with proposed time ", inetTime), LogLevels.Info);

            systemTimeCorrectorGuard.ExecuteIfFreeAsync(() =>
            {
                Logger.Log(this, "Executing CheckSystemTimeFromInternet handler async...", LogLevels.Info);

                if (new SystemTimeCorrector(Config, ProcessRunnerFactory, Logger).IsSystemTimeValid(inetTime))
                {
                    config.IsSystemTimeValid = true;
                    DisconnectSystemTimeChecking();
                    arduController.SetTimeToArduino();
                }
            },
            ex => Logger.Log(this, ex));
        }

        private void DisconnectSystemTimeChecking()
        {
            gpsController.GPRMCReseived -= CheckSystemTimeFromGPS;
            netKeeper.InternetTime -= CheckSystemTimeFromInternet;
            Logger.Log(this, "SystemTimeCorrector has been disconnected.", LogLevels.Info);
        }

        public async Task Shutdown(HostControllerShutdownModes mode)
        {
			arduController.StopPing();

            var shutdownModel = uiController.ShowPage("ShutdownProgress", null) as UIModels.ShutdownProgressModel;

            Action<string> showLine = line => { if (shutdownModel != null) { shutdownModel.AddLine(line); } };

            showLine(string.Format("Begin shutdown in {0} mode", mode));
            Logger.Log(this, string.Format("Begin shutdown in {0} mode", mode), LogLevels.Info);

            showLine("Disposing ELM327 Controller");
            elm327Controller.Dispose();

			arduController.RelayService.Disable(Relay.OBD);
			arduController.RelayService.Disable(Relay.Relay3);
			arduController.RelayService.Disable(Relay.Relay4);

			if (mode == HostControllerShutdownModes.Exit) 
			{
                await arduController.HoldPower();
			}

            showLine("Disposing Travel Controller");
            travelController.Dispose();

            showLine("Disposing InetKeeper");
			netKeeper.Dispose();

            showLine("Stopping GPSD service");
			gpsd.Stop();

            showLine("Disconnecting time checking events");
            DisconnectSystemTimeChecking();

            showLine("Disabling GPS Controller");
            gpsController.Shutdown();

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

            showLine("Flushing online log");
            onlineLogger.Upload(true);

            showLine("Stopping UI...");

            miniDisplayController.ResetQueue();

            miniDisplayController.Dispose();

            uiController.Shutdown();

			await arduController.Beep (100);

            showLine("Stopping timers");
            StopTimers();

            showLine("Stopping SyncContext");
            syncContext.Stop();

            switch (mode)
            {
                case HostControllerShutdownModes.Restart:
                    {
                        var processConfig = new ProcessConfig
                        {
                            ExePath = Config.GetString(ConfigNames.SystemRestartCommand),
                            Args = Config.GetString(ConfigNames.SystemRestartArg) 
                        };

                        ProcessRunnerFactory.Create(processConfig).Run();
                    }
                    break;

                case HostControllerShutdownModes.Shutdown:
                    {
                        var processConfig = new ProcessConfig
                        {
                            ExePath = Config.GetString(ConfigNames.SystemShutdownCommand),
                            Args = Config.GetString(ConfigNames.SystemShutdownArg)
                        };

                        ProcessRunnerFactory.Create(processConfig).Run();
                    }
                    break;
            }
        }

		public IProcessRunner Create(string appKey, object[] argumentParameters = null)
        {
			var args = Config.GetString (string.Concat (appKey, "_args"));

			if (argumentParameters != null)
				args = string.Format (args, argumentParameters);

            var processConfig = new ProcessConfig
            {
                ExePath = Config.GetString(string.Concat(appKey, "_exe")),
                Args = args,
                WaitForUI = Config.GetBool(string.Concat(appKey, "_wait_UI"))
            };

            return Create(processConfig);
        }

        public IProcessRunner Create(ProcessConfig processConfig)
        {
            if (Logger == null)
                throw new InvalidOperationException("Unable to create ProcessRunner before logger will be fully initialized");

            return new ProcessRunner.ProcessRunnerImpl(processConfig, Logger);
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


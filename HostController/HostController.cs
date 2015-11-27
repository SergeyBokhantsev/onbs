using Interfaces;
using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Text;
using LogLib;
using System.Net;
using System.Threading;
using Interfaces.UI;
using UIController;
using HostController.Lin;
using HostController.Win;

namespace HostController
{
    public class HostController : IHostController, IProcessRunnerFactory
	{
        private string logFolder;
        
        private GPSD.Net.GPSD gpsd;
        private InternetConnectionKeeper netKeeper;

        private Configuration config;
        private UIController.UIController uiController;
        private IInputController inputController;
        private IArduinoController arduController;
        private GPSController.GPSController gpsController;
        private IAutomationController automationController;
        private TravelController.TravelController travelController;
        private Elm327Controller.Elm327Controller elm327Controller;

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
        public SynchronizationContext SyncContext
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

            syncContext.Post(o => Initialize(), null);
            syncContext.Pump();
        }

        public T GetController<T>() 
            where T : class, IController
        {
            if (typeof(T).Equals(typeof(IUIController)))
                return uiController as T;
            else if (typeof(T).Equals(typeof(IInputController)))
                return inputController as T;
            else if (typeof(T).Equals(typeof(IArduinoController)))
                return arduController as T;
            else if (typeof(T).Equals(typeof(IGPSController)))
                return gpsController as T;
            else if (typeof(T).Equals(typeof(IAutomationController)))
                return automationController as T;
            else if (typeof(T).Equals(typeof(ITravelController)))
                return travelController as T;
            else if (typeof(T).Equals(typeof(IElm327Controller)))
                return elm327Controller as T;
            else
                throw new NotImplementedException(typeof(T).ToString());
        }

        private void CreateConfig()
        {
            var env = ResolveEnvironment();
            ConfigValuesResolver configResolver = null;

            switch (env)
            {
                case Environments.Win:
                    configResolver = new Win.MockConfigResolver();
                    break;

                case Environments.RPi:
                    configResolver = new Lin.RPiConfigResolver(this);
                    break;

                default:
                    throw new NotImplementedException();
            }

            config = new Configuration(configResolver);
            config.Environment = env;
            config.IsSystemTimeValid = config.GetBool(ConfigNames.SystemTimeValidByDefault);

            if (DateTime.Now.Year >= 2015)
                config.IsSystemTimeValid = true;
        }

        private void CreateLogger()
        {
            var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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
            var onlineLogTimer = CreateTimer(60000, ht => onlineLogger.Upload(false), true, false);

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

        private void Initialize()
        {
            ServicePointManager.ServerCertificateValidationCallback = (s1, s2, s3, s4) => true;

            netKeeper = new InternetConnectionKeeper(Config, Logger);
            netKeeper.InternetConnectionStatus += connected => config.IsInternetConnected = connected;
            netKeeper.InternetTime += CheckSystemTimeFromInternet;
            netKeeper.RestartNeeded += InetKeeperRestartNeeded;
            netKeeper.StartChecking();

            inputController = new InputController.InputController(Logger);

            elm327Controller = new Elm327Controller.Elm327Controller(this);

            var arduPort = config.Environment == Environments.Win ? new ArduinoEmulatorPort() as IPort : new SerialArduPort(Logger, Config) as IPort;
            arduController = new ArduinoController.ArduinoController(arduPort, this);
            arduController.RegisterFrameAcceptor(inputController);

            var gpsCtrl = new GPSController.GPSController(Config, SyncContext, Logger);
            
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
                this, (MappedPage pdescr, string viewModelName) => UIModels.ModelBase.CreateModel(this, pdescr, viewModelName));

            uiController.DialogPending += uiController_DialogPending;
            uiController.ShowDefaultPage();

            if (config.Environment == Environments.RPi)
                gpsCtrl.GPRMCReseived += CheckSystemTimeFromGPS;

            StartTimers();
        }

        void uiController_DialogPending(bool obj)
        {
           
        }

        void InetKeeperRestartNeeded()
        {
            netKeeper.RestartNeeded -= InetKeeperRestartNeeded;

            var dialog = new UIModels.Dialogs.YesNoDialog("Restart", "Failed to get internet connection. Restart now?", "Yes", "No", this, 30000, Interfaces.UI.DialogResults.Yes);
            dialog.Closed += dr => 
            {
                if (dr == Interfaces.UI.DialogResults.Yes)
                {
                    Logger.Log(this, "Begin restart because of Internet keeper request...", LogLevels.Warning);
                    SyncContext.Post(o => Shutdown(HostControllerShutdownModes.Restart), null);
                }
            };

            uiController.ShowDialog(dialog);
        }

        private void CheckSystemTimeFromGPS(Interfaces.GPS.GPRMC gprmc)
        {
            if (gprmc.Active && (config.IsSystemTimeValid = new SystemTimeCorrector(Config, ProcessRunnerFactory, Logger).IsSystemTimeValid(gprmc.Time)))
            {
                DisconnectSystemTimeChecking();
            }
        }

        private void CheckSystemTimeFromInternet(DateTime inetTime)
        {
            if (config.IsSystemTimeValid = new SystemTimeCorrector(Config, ProcessRunnerFactory, Logger).IsSystemTimeValid(inetTime))
            {
                DisconnectSystemTimeChecking();
            }
        }

        private void DisconnectSystemTimeChecking()
        {
            gpsController.GPRMCReseived -= CheckSystemTimeFromGPS;
            netKeeper.InternetTime -= CheckSystemTimeFromInternet;
            Logger.Log(this, "SystemTimeCorrector has been disconnected.", LogLevels.Info);
        }

        public void Shutdown(HostControllerShutdownModes mode)
        {
            var shutdownModel = uiController.ShowPage("ShutdownProgress", null) as UIModels.ShutdownProgressModel;

            shutdownModel.AddLine(string.Format("Begin shutdown in {0} mode", mode));
            Logger.Log(this, string.Format("Begin shutdown in {0} mode", mode), LogLevels.Info);

            shutdownModel.AddLine("Disposing ELM327 Controller");
            elm327Controller.Dispose();

            shutdownModel.AddLine("Disposing Travel Controller");
            travelController.Dispose();

            shutdownModel.AddLine("Disposing InetKeeper");
			netKeeper.Dispose();

            shutdownModel.AddLine("Stopping GPSD service");
			gpsd.Stop();

            shutdownModel.AddLine("Disconnecting time checking events");
            DisconnectSystemTimeChecking();

            shutdownModel.AddLine("Disabling GPS Controller");
            gpsController.Shutdown();

            shutdownModel.AddLine("Stopping timers");
            StopTimers();

            shutdownModel.AddLine("Stopping SyncContext");
            syncContext.Stop();

            if (mode != HostControllerShutdownModes.UnhandledException)
            {
                try
                {
                    shutdownModel.AddLine("Saving Configuration");
                    Config.Save();
                }
                catch (Exception ex)
                {
                    shutdownModel.AddLine(ex.Message);
                    Logger.Log(this, ex);
                }
            }

            Logger.Log(this, "--- Logging finished ---", LogLevels.Info);

            shutdownModel.AddLine("Flushing loggers");
            Logger.Flush();

            shutdownModel.AddLine("Flushing online log");
            onlineLogger.Upload(true);

            shutdownModel.AddLine("Stopping UI...");

            uiController.Shutdown();

            switch (mode)
            {
                case HostControllerShutdownModes.Restart:
                    {
                        var command = Config.GetString(ConfigNames.SystemRestartCommand);
                        var arg = Config.GetString(ConfigNames.SystemRestartArg);
                        ProcessRunnerFactory.Create(command, arg, false).Run();
                    }
                    break;

                case HostControllerShutdownModes.Shutdown:
                    {
                        var command = Config.GetString(ConfigNames.SystemShutdownCommand);
                        var arg = Config.GetString(ConfigNames.SystemShutdownArg);
                        ProcessRunnerFactory.Create(command, arg, false).Run();
                    }
                    break;
            }
        }

        public IProcessRunner Create(string appKey)
        {
            var appName = Config.GetString(string.Concat(appKey, "_exe"));
            var commandLine = Config.GetString(string.Concat(appKey, "_args"));
            var waitForUI = Config.GetBool(string.Concat(appKey, "_wait_UI"));

            return Create(appName, commandLine, waitForUI);
        }

        public IProcessRunner Create(string exePath, string args, bool waitForUI)
        {
            if (Logger == null)
                throw new InvalidOperationException("Unable to create ProcessRunner before logger will be fully initialized");

            return new ProcessRunner.ProcessRunnerImpl(exePath, args, waitForUI, Logger);
        }

        public IHostTimer CreateTimer(int span, Action<IHostTimer> action, bool isEnabled, bool firstEventImmidiatelly)
        {
            return timersController.CreateTimer(span, action, isEnabled, firstEventImmidiatelly);
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


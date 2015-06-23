using Interfaces;
using System;
using System.Threading.Tasks;
using System.Configuration;

namespace HostController
{
    public class HostController : IHostController, IProcessRunnerFactory
	{
        private GPSD.Net.GPSD gpsd;

        private Configuration config;
        private UIController.UIController uiController;
        private IInputController inputController;
        private IArduinoController arduController;
        private IGPSController gpsController;
        private IAutomationController automationController;

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
        public IDispatcher Dispatcher
        {
            get;
            private set;
        }

        public IProcessRunnerFactory ProcessRunnerFactory
        {
            get
            {
                return this;
            }
        }

        public void Run()
        {
            config = new Configuration();

            config.IsSystemTimeValid = config.GetBool(ConfigNames.SystemTimeValidByDefault);

			if (DateTime.Now.Year >= 2015)
				config.IsSystemTimeValid = true;

            CreateLogger();
            RunDispatcher();
            Shutdown();
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
            else
                throw new NotImplementedException(typeof(T).ToString());
        }

        private void CreateLogger()
        {
            Logger = new ConsoleLoggerWrapper(Config);
            Logger.Log(this, "--- Logging initiated ---", LogLevels.Info);
        }

        private void RunDispatcher()
        {
            Dispatcher = new Dispatcher(Logger);
            Dispatcher.Invoke(null, null, Initialize);
            Dispatcher.Run();
        }

        private void Initialize(object sender, EventArgs args)
        {
            inputController = new InputController.InputController(Logger);

            var useFakeArduPort = Config.GetBool(ConfigNames.ArduinoPortFake);
            var arduPort = useFakeArduPort ? new MockArduPort() as IPort : new SerialArduPort(Logger, Config) as IPort;
            arduController = new ArduinoController.ArduinoController(arduPort, Dispatcher, Logger);
            arduController.RegisterFrameAcceptor(inputController);

            var gpsCtrl = new GPSController.GPSController(Dispatcher, Logger);
            
            arduController.RegisterFrameAcceptor(gpsCtrl);
            gpsController = gpsCtrl;
            gpsd = new GPSD.Net.GPSD(gpsController, Config, Logger);
            gpsd.Start();

            automationController = new AutomationController.AutomationController(this);

            uiController = new UIController.UIController(Config.GetString(ConfigNames.UIHostAssemblyName), Config.GetString(ConfigNames.UIHostClass), this, () => new UIModels.MainPage(this));
            uiController.ShowDefaultPage();

            gpsCtrl.GPRMCReseived += CheckSystemTime;
        }

        private void CheckSystemTime(Interfaces.GPS.GPRMC gprmc)
        {
            if (gprmc.Active && (config.IsSystemTimeValid = new SystemTimeCorrector(Config, ProcessRunnerFactory, Logger).IsSystemTimeValid(gprmc.Time)))
            {
                gpsController.GPRMCReseived -= CheckSystemTime;
                Logger.Log(this, "SystemTimeCorrector has been disconnected.", LogLevels.Info);
            }
        }

        private void Shutdown()
        {
            Logger.Log(this, "Begin shutdown", LogLevels.Info);

            gpsController.GPRMCReseived -= CheckSystemTime;

            uiController.Shutdown();

            (Config as Configuration).Dispose();

            Logger.Log(this, "--- Logging finished ---", LogLevels.Info);
        }

        public IProcessRunner Create(string appKey)
        {
            var appName = Config.GetString(string.Concat(appKey, "_exe"));
            var commandLine = Config.GetString(string.Concat(appKey, "_args"));
            var useShellExecution = Config.GetBool(string.Concat(appKey, "_use_shell"));
            var waitForUI = Config.GetBool(string.Concat(appKey, "_wait_UI"));

            return Create(appName, commandLine, useShellExecution, waitForUI);
        }

        public IProcessRunner Create(string exePath, string args, bool useShellExecute, bool waitForUI)
        {
            return new ProcessRunner(exePath, args, useShellExecute, waitForUI, Logger);
        }
    }
}


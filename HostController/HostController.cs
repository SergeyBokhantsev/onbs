using Interfaces;
using System;
using System.Threading.Tasks;
using System.Configuration;

namespace HostController
{
    public class HostController : IHostController, IProcessRunnerFactory
	{
        private GPSD.Net.GPSD gpsd;

        private IUIController uiController;
        private IInputController inputController;
        private IArduinoController arduController;
        private IGPSController gpsController;
        private IAutomationController automationController;

        public IConfig Config
        {
            get;
            private set;
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
            Config = new Configuration();
            CreateLogger();
            RunDispatcher();
            Shutdown();
        }

        public T GetController<T>() where T : IController
        {
            if (typeof(T).Equals(typeof(IUIController)))
                return (T)uiController;
            else if (typeof(T).Equals(typeof(IInputController)))
                return (T)inputController;
            else if (typeof(T).Equals(typeof(IArduinoController)))
                return (T)arduController;
            else if (typeof(T).Equals(typeof(IGPSController)))
                return (T)gpsController;
            else if (typeof(T).Equals(typeof(IAutomationController)))
                return (T)automationController;
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

            var useFakeArduPort = Config.GetBool(Configuration.Names.ArduinoPortFake);
            var arduPort = useFakeArduPort ? new MockArduPort() as IPort : new SerialArduPort(Logger, Config) as IPort;
            arduController = new ArduinoController.ArduinoController(arduPort, Dispatcher, Logger);
            arduController.RegisterFrameAcceptor(inputController);

            var gpsCtrl = new GPSController.GPSController(Dispatcher, Logger);
            arduController.RegisterFrameAcceptor(gpsCtrl);
            gpsController = gpsCtrl;

            gpsd = new GPSD.Net.GPSD(gpsController, Config, Logger);
            gpsd.Start();

            automationController = new AutomationController.AutomationController(this);

            uiController = new UIController.UIController(Config.GetString(Configuration.Names.UIHostAssemblyName), Config.GetString(Configuration.Names.UIHostClass), this);
            uiController.ShowMainPage();
        }

        private void Shutdown()
        {
            Logger.Log(this, "Begin shutdown", LogLevels.Info);

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


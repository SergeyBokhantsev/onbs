using Interfaces;
using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Text;
using LogLib;

namespace HostController
{
    public class HostController : IHostController, IProcessRunnerFactory
	{
        private string logFolder;

        private GPSD.Net.GPSD gpsd;

        private Configuration config;
        private UIController.UIController uiController;
        private IInputController inputController;
        private IArduinoController arduController;
        private GPSController.GPSController gpsController;
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
            var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            try
            {
                logFolder = Path.Combine(workingDir, Config.GetString(ConfigNames.LogFolder));
            }
            catch (Exception ex)
            {
                logFolder = Path.Combine(workingDir, "Logs");
            }

            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            Logger = new ConsoleLoggerWrapper(new GeneralLogger(Config));
            Logger.Log(this, "--- Logging initiated ---", LogLevels.Info);

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

        private void RunDispatcher()
        {
            Dispatcher = new Dispatcher(Logger);
            Dispatcher.Invoke(this, null, Initialize);
            Dispatcher.Run();
        }

        private void Initialize(object sender, EventArgs args)
        {
            inputController = new InputController.InputController(Logger);

            var useFakeArduPort = Config.GetBool(ConfigNames.ArduinoPortFake);
            var arduPort = useFakeArduPort ? new MockArduPort() as IPort : new SerialArduPort(Logger, Config) as IPort;
            arduController = new ArduinoController.ArduinoController(arduPort, Dispatcher, Logger);
            arduController.RegisterFrameAcceptor(inputController);

            var gpsCtrl = new GPSController.GPSController(Config, Dispatcher, Logger);
            
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

        public void Shutdown(HostControllerShutdownModes mode)
        {
            Logger.Log(this, string.Format("Begin shutdown in {0} mode", mode), LogLevels.Info);

            gpsController.Shutdown();

            uiController.Shutdown();

            ((Dispatcher)Dispatcher).Exit();

            if (mode != HostControllerShutdownModes.UnhandledException)
                Config.Save();

            Logger.Log(this, "--- Logging finished ---", LogLevels.Info);

            Logger.Flush();

            switch (mode)
            {
                case HostControllerShutdownModes.Restart:
                    {
                        var command = Config.GetString(ConfigNames.SystemRestartCommand);
                        var arg = Config.GetString(ConfigNames.SystemRestartArg);
                        ProcessRunnerFactory.Create(command, arg, true, false).Run();
                    }
                    break;

                case HostControllerShutdownModes.Shutdown:
                    {
                        var command = Config.GetString(ConfigNames.SystemShutdownCommand);
                        var arg = Config.GetString(ConfigNames.SystemShutdownArg);
                        ProcessRunnerFactory.Create(command, arg, true, false).Run();
                    }
                    break;
            }
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


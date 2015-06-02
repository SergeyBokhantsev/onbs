﻿using Interfaces;
using System;
using System.Threading.Tasks;
using System.Configuration;

namespace HostController
{
	public class HostController : IHostController
	{
        private GPSD.Net.GPSD gpsd;

        private IUIController uiController;
        private IInputController inputController;
        private IArduinoController arduController;
        private IGPSController gpsController;

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

        public void Run()
        {
            Config = new Configuration();
            CreateLogger();
            RunDispatcher();
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

            arduController = new ArduinoController.ArduinoController(new SerialArduPort(Logger, Config), Dispatcher, Logger);
            arduController.RegisterFrameAcceptor(inputController);

            var gpsCtrl = new GPSController.GPSController(Logger);
            arduController.RegisterFrameAcceptor(gpsCtrl);
            gpsController = gpsCtrl;

            gpsd = new GPSD.Net.GPSD(gpsController, Logger);
            gpsd.Start();

            uiController = new UIController.UIController(Config.GetString(Configuration.Names.UIHostAssemblyName), Config.GetString(Configuration.Names.UIHostClass), this);
            uiController.ShowMainPage();
        }

        public IProcessRunner CreateProcessRunner(string appKey)
        {
            var appName = Config.GetString(string.Concat(appKey, "_exe"));
            var commandLine = Config.GetString(string.Concat(appKey, "_args"));
            var useShellExecution = Config.GetBool(string.Concat(appKey, "_use_shell"));
            var waitForUI = Config.GetBool(string.Concat(appKey, "_wait_UI"));

            return new ProcessRunner(appName, commandLine, useShellExecution, waitForUI, Logger);
        }
    }
}


using Interfaces;
using System;
using System.Threading.Tasks;

namespace HostController
{
	public class HostController : IHostController
	{
        private GPSD.Net.GPSD gpsd;

        private IUIController uiController;
        private IInputController inputController;
        private IArduinoController arduController;
        private IGPSController gpsController;

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
            CreateLogger();
            RunDispatcher();
        }

        public T GetController<T>() where T : IController
        {
            if (typeof(T).Equals(typeof(IUIController)))
                return (T)uiController;
            else
                throw new NotImplementedException(typeof(T).ToString());
        }

        private void CreateLogger()
        {
            Logger = new ConsoleLoggerWrapper();
            Logger.Log("--- Logging initiated ---", LogLevels.Info);
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

            arduController = new ArduinoController.ArduinoController(new MockArduPort(), Dispatcher, Logger);
            arduController.RegisterFrameAcceptor(inputController);

            var gpsCtrl = new GPSController.GPSController(Logger);
            arduController.RegisterFrameAcceptor(gpsCtrl);
            gpsController = gpsCtrl;

            gpsd = new GPSD.Net.GPSD(gpsController, Logger);
            gpsd.Start();

            uiController = new UIController.UIController("GtkApplication.dll", "GtkApplication.App", inputController, Logger, Dispatcher);
            uiController.ShowMainPage();
        }
    }
}


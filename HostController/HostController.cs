using Interfaces;
using System;
using System.Threading.Tasks;

namespace HostController
{
	public class HostController : IHostController
	{
        private IUIController uiController;
        private IInputController inputController;
        private IArduinoController arduController;

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
            arduController.FrameAcceptors.Add(inputController);

            uiController = new UIController.UIController("GtkApplication.dll", "GtkApplication.App", inputController, Logger, Dispatcher);
            uiController.ShowMainPage();
        }
    }
}


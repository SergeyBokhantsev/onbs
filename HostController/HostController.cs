using Interfaces;
using System;
using System.Threading.Tasks;

namespace HostController
{
	public class HostController : IHostController
	{
        private readonly IUIController uiController;

        public ILogger Logger
        {
            get;
            private set;
        }

		public HostController()
		{
            Logger = new ConsoleLoggerWrapper();

            uiController = new UIController.UIController("GtkApplication.dll", "GtkApplication.App", Logger);
            uiController.ShowMainPage();
		}

        public T GetController<T>() where T : IController
        {
            if (typeof(T).Equals(typeof(IUIController)))
                return (T)uiController;
            else
                throw new NotImplementedException(typeof(T).ToString());
        }
    }
}


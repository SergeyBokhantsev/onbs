using Interfaces;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UIController.Models;

namespace UIController
{
    public class UIController : IUIController
    {
        private ManualResetEvent hostWaiter = new ManualResetEvent(false);
        private IUIHost uiHost;
        private IPageModel current;
        private readonly IDispatcher dispatcher;
        private readonly ILogger logger;

        public UIController(string uiHostAssemblyPath, string uiHostClassName, IInputController inputController, ILogger logger, IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.logger = logger;

            var uit = new Thread(() => UIThread(uiHostAssemblyPath, uiHostClassName, logger));
            uit.IsBackground = true;
            uit.Name = "UI";
            uit.Start();
            
            if (!hostWaiter.WaitOne(10000) || uiHost == null)
                throw new Exception("Unable to start UI host.");

            inputController.ButtonPressed += ButtonPressed;
        }

        private void ButtonPressed(Buttons button, ButtonStates state)
        {
            var page = current;

            if (page != null)
                page.Button(button, state);
        }

        private void UIThread(string uiHostAssemblyPath, string uiHostClassName, ILogger logger)
        {
            var assembly = Assembly.LoadFrom(uiHostAssemblyPath);
            var appType = assembly.GetType(uiHostClassName);
            var appConstructor = appType.GetConstructor(new Type[] { typeof(ILogger) });
            uiHost = appConstructor.Invoke(new object[] { logger }) as IUIHost;
            hostWaiter.Set();
            uiHost.Run();
            logger.Log(this, "UI Host has exited", LogLevels.Info);
        }

        public void ShowMainPage()
        {
            ShowPage(new MainPage(dispatcher, logger));
        }

        private void ShowPage(IPageModel model)
        {
            if (current != null)
                current.Dispose();

            current = model;

            uiHost.ShowPage(current);
        }
    }
}

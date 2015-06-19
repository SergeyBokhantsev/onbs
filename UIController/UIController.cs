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
        private readonly string uiHostAssemblyPath;
        private readonly string uiHostClassName;

        private AutoResetEvent hostWaiter = new AutoResetEvent(false);
        private IUIHost uiHost;
        private IPageModel current;
        private readonly IHostController hostController;

        private bool shutdowning;

        public UIController(string uiHostAssemblyPath, string uiHostClassName, IHostController hostController)
        {
            this.uiHostAssemblyPath = uiHostAssemblyPath;
            this.uiHostClassName = uiHostClassName;
            this.hostController = hostController;

            StartUIThread();
            
            hostController.GetController<IInputController>().ButtonPressed += ButtonPressed;
        }

        private void ButtonPressed(Buttons button, ButtonStates state)
        {
            var page = current;

            if (page != null)
            {
                var args = new PageModelActionEventArgs(button.ToString(), state);
                page.Action(args);
            }
        }

        private void StartUIThread()
        {
            var uit = new Thread(() => UIThread(uiHostAssemblyPath, uiHostClassName, hostController.Logger));
            uit.IsBackground = true;
            uit.Name = "UI";
            uit.Start();

            if (!hostWaiter.WaitOne(10000) || uiHost == null)
                throw new Exception("Unable to start UI host.");
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

            if (!shutdowning)
            {
                logger.Log(this, "Restarting UI Host...", LogLevels.Info);
                StartUIThread();
                ShowMainPage();
            }
        }

        public void ShowMainPage()
        {
            ShowPage(new MainPage(hostController));
        }

        public void ShowPage(IPageModel model)
        {
            AssertHost();

            if (current != null)
                current.Dispose();

            current = model;

            uiHost.ShowPage(current);
        }

        public void Shutdown()
        {
            AssertHost();
            shutdowning = true;
            uiHost.Shutdown();
            uiHost = null;
        }

        private void AssertHost()
        {
            if (shutdowning || uiHost == null)
                throw new Exception("UI host was destroyed");
        }
    }
}

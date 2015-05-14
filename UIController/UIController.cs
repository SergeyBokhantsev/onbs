using Interfaces;
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

        public UIController(string uiHostAssemblyPath, string uiHostClassName, ILogger logger)
        {
            var uit = new Thread(() => UIThread(uiHostAssemblyPath, uiHostClassName, logger));
            uit.IsBackground = false;
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
        }

        public void ShowMainPage()
        {
            ShowPage(new MainPage());
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

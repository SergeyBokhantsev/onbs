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

namespace UIController
{
    public delegate IPageModel PageConstructorDelegate(MappedPage pageDescriptor, string viewModelName, object modelArgument);

    public class UIController : IUIController
    {
        public event Action<bool> DialogPending;
        public event Action<IDialogModel> DialogShown;

        public event Action<string, string> PageChanging;

        private readonly string uiHostAssemblyPath;
        private readonly string uiHostClassName;

        private AutoResetEvent hostWaiter = new AutoResetEvent(false);
        private IUIHost uiHost;
        private IPageModel current;
        private readonly Stack<IDialogModel> dialogs = new Stack<IDialogModel>();
        private IDialogModel currentDialog;
        private readonly IHostController hostController;
        private readonly PageConstructorDelegate pageConstructor;
        private int mainThreadId;

        private ApplicationMap map;

        private int mouseLocation;

        private bool shutdowning;

        public int UserIdleMinutes
        {
            get
            {
                return uiHost != null ? uiHost.UserIdleMinutes : 0;
            }
        }

        public UIController(string uiHostAssemblyPath, string uiHostClassName, ApplicationMap map, IHostController hostController, PageConstructorDelegate pageConstructor)
        {
            this.map = map;

            this.uiHostAssemblyPath = uiHostAssemblyPath;
            this.uiHostClassName = uiHostClassName;
            this.hostController = hostController;
            this.pageConstructor = pageConstructor;
            this.mainThreadId = Thread.CurrentThread.ManagedThreadId;

            hostController.CreateTimer(3 * 60 * 1000, t =>
                {
                   // hostController.ProcessRunnerFactory.Create(ConfigNames.XScreenForceOn).Run();
                    hostController.GetController<IAutomationController>().MouseMove(mouseLocation++, mouseLocation++);
                    if (mouseLocation > 3)
                        mouseLocation = 0;
                }, true, false, "periodic mouse mover");

            StartUIThread();
            
            hostController.GetController<IInputController>().ButtonPressed += ButtonPressed;
        }

        private void ButtonPressed(Buttons button, ButtonStates state)
        {
            lock (dialogs)
            {
				if (currentDialog != null && state == ButtonStates.Press)
                {
                    currentDialog.HardwareButtonClick(button);
                    return;
                }
            }

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
            var appConstructor = appType.GetConstructor(new Type[] { typeof(ILogger), typeof(ISessionConfig) });
            uiHost = appConstructor.Invoke(new object[] { logger, hostController.Config }) as IUIHost;
            hostWaiter.Set();
            uiHost.Run(hostController.Config.GetBool(ConfigNames.UIFullscreen));
            logger.Log(this, "UI Host has exited", LogLevels.Info);

            if (!shutdowning)
            {
                logger.Log(this, "Restarting UI Host...", LogLevels.Info);
                StartUIThread();
                hostController.SyncContext.Post(o => ShowDefaultPage(), null, "ShowDefaultPage");
            }
        }

        public void ShowDefaultPage()
        {
            ShowPage(map.DefaultPageName, map.DefaultPageViewName, null);
        }

        private IPageModel CreateModel(string descriptorName, object arg, string viewName = null)
        {
            var pageDescriptor = map.GetPage(descriptorName);

            if (pageDescriptor == null)
                throw new Exception(string.Format("Page descriptor '{0}' not found", descriptorName));

            return pageConstructor(pageDescriptor, viewName, arg);
        }

        public IPageModel ShowPage(string descriptorName, string viewName, object arg)
        {
            AssertThread();
            AssertHost();

            if (current != null)
                current.Dispose();

            IPageModel model = null;

            OnPageChanging(descriptorName, viewName);

            try
            {
                model = CreateModel(descriptorName, arg, viewName);
            }
            catch (Exception ex)
            {
                model = CreateModel("UnexpectedError", arg);
                dynamic unexpectedErrorModel = model;
                unexpectedErrorModel.AddLine(string.Concat("Cannot create model for descriptor: ", descriptorName));
                unexpectedErrorModel.AddLine(string.Concat("Exception: ", ex.Message));
                unexpectedErrorModel.AddLine(string.Concat("Inner exception: ", ex.InnerException != null ? ex.InnerException.Message : "Null"));
                unexpectedErrorModel.AddLine("Stack trace:");
                unexpectedErrorModel.AddLine(ex.StackTrace.Substring(0, Math.Min(ex.StackTrace.Length, 1200)));
            }

            current = model;

            uiHost.ShowPage(current);

            ProcessDialogsQueue();

            return model;
        }

        private void OnPageChanging(string descriptorName, string viewName)
        {
            var handler = PageChanging;
            if (handler != null)
                handler(descriptorName, viewName);
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

        private void AssertThread()
        {
            if (mainThreadId != Thread.CurrentThread.ManagedThreadId)
                throw new InvalidOperationException("UI operation should be performed from the original thread.");
        }

        public void ShowDialog(IDialogModel model)
        {
            lock (dialogs)
            {
                dialogs.Push(model);
                hostController.SyncContext.Post(o => ProcessDialogsQueue(), null, "UIController.ShowDialog -> ProcessDialogsQueue");
                ProcessDialogsQueue();
            }
        }

        public async Task<DialogResults> ShowDialogAsync(IDialogModel model)
        {
            return await Task.Run<DialogResults>(() =>
            {
                var result = DialogResults.None;

                using (var signal = new ManualResetEvent(false))
                {
                    model.Closed += dr => { result = dr; signal.Set(); };

                    ShowDialog(model);

                    signal.WaitOne();

                    return result;
                }
            });
        }

        private void ProcessDialogsQueue()
        {
            lock (dialogs)
            {
                if (currentDialog == null && dialogs.Any())
                {
                    OnDialogPending(true);

                    if ((current == null || !current.NoDialogsAllowed))
                    {
                        currentDialog = dialogs.Pop();

                        currentDialog.Closed += r =>
                        {
                            lock (dialogs)
                            {
                                OnDialogShown(null);
                                currentDialog = null;
                                OnDialogPending(false);
                                hostController.SyncContext.Post(o => ProcessDialogsQueue(), null);
                            }
                        };

                        uiHost.ShowDialog(currentDialog);
                        OnDialogShown(currentDialog);
                    }
                }
            }
        }

        private void OnDialogPending(bool pending)
        {
            hostController.SyncContext.Post(o => 
            {
                var handler = DialogPending;
                if (handler != null)
                    handler((bool)o);
            }, pending, "UIController.OnDialogPending -> ProcessDialogsQueue");
        }

        private void OnDialogShown(IDialogModel dialog)
        {
            hostController.SyncContext.Post(o =>
            {
                var handler = DialogShown;
                if (handler != null)
                    handler((IDialogModel)o);
            }, dialog, "UIController.OnDialogShown -> ProcessDialogsQueue");
        }
    }
}

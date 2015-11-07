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
    public class UIController : IUIController
    {
        public event Action<bool> DialogPending;

        private readonly string uiHostAssemblyPath;
        private readonly string uiHostClassName;

        private AutoResetEvent hostWaiter = new AutoResetEvent(false);
        private IUIHost uiHost;
        private IPageModel current;
        private readonly Stack<IDialogModel> dialogs = new Stack<IDialogModel>();
        private IDialogModel currentDialog;
        private readonly IHostController hostController;
        private readonly Func<IPageModel> startPageConstructor;
        private int mainThreadId;

        private int mouseLocation;

        private bool shutdowning;

        public UIController(string uiHostAssemblyPath, string uiHostClassName, IHostController hostController, Func<IPageModel> startPageConstructor)
        {
            this.uiHostAssemblyPath = uiHostAssemblyPath;
            this.uiHostClassName = uiHostClassName;
            this.hostController = hostController;
            this.startPageConstructor = startPageConstructor;
            this.mainThreadId = Thread.CurrentThread.ManagedThreadId;

            hostController.CreateTimer(3 * 60 * 1000, t =>
                {
                    hostController.ProcessRunnerFactory.Create(ConfigNames.XScreenForceOn).Run();
                    hostController.GetController<IAutomationController>().MouseMove(mouseLocation++, mouseLocation++);
                    if (mouseLocation > 3)
                        mouseLocation = 0;
                }, true, false);

            StartUIThread();
            
            hostController.GetController<IInputController>().ButtonPressed += ButtonPressed;
        }

        private void ButtonPressed(Buttons button, ButtonStates state)
        {
            lock (dialogs)
            {
                if (currentDialog != null)
                {
                    currentDialog.Action(new PageModelActionEventArgs(button.ToString(), state));
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
            var appConstructor = appType.GetConstructor(new Type[] { typeof(ILogger) });
            uiHost = appConstructor.Invoke(new object[] { logger }) as IUIHost;
            hostWaiter.Set();
            uiHost.Run(hostController.Config.GetBool(ConfigNames.UIFullscreen));
            logger.Log(this, "UI Host has exited", LogLevels.Info);

            if (!shutdowning)
            {
                logger.Log(this, "Restarting UI Host...", LogLevels.Info);
                StartUIThread();
                ShowDefaultPage();
            }
        }

        public void ShowDefaultPage()
        {
            ShowPage(startPageConstructor());
        }

        public void ShowPage(IPageModel model)
        {
            AssertThread();
            AssertHost();

            if (current != null)
                current.Dispose();

            current = model;

            uiHost.ShowPage(current);

            ProcessDialogsQueue();
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
                hostController.SyncContext.Post(o => ProcessDialogsQueue(), null);
                ProcessDialogsQueue();
            }
        }

        public async Task<DialogResults> ShowDialogAsync(IDialogModel model)
        {
            return await Task.Run<DialogResults>(() =>
            {
                var result = DialogResults.None;
                var signal = new ManualResetEvent(false);
                
                model.Closed += dr => { result = dr; signal.Set(); };

                ShowDialog(model);

                signal.WaitOne();

                return result;
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
                                currentDialog = null;
                                OnDialogPending(false);
                                hostController.SyncContext.Post(o => ProcessDialogsQueue(), null);
                            }
                        };

                        uiHost.ShowDialog(currentDialog);
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
            }, pending);
        }
    }
}

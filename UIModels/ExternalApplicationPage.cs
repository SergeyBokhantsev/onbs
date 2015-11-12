using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Input;
using System.Threading;
using UIController;

namespace UIModels
{
    public abstract class ExternalApplicationPage : ModelBase
    {
        protected IProcessRunner Runner
        {
            get;
            set;
        }

		protected ExternalApplicationPage(string viewName, IHostController hc, ApplicationMap map, IProcessRunner runner)
            : base(viewName, hc, map)
        {
            NoDialogsAllowed = true;

            Runner = runner;

            if (Runner != null)
            {
                Runner.Exited += RunnerExited;

                SetProperty("label_launch_info", string.Format("Launching {0}...", Runner.Name));
                SetProperty("is_error", "0");
            }
            else
            {
                SetProperty("label_launch_info", "Runner was not provided");
                SetProperty("is_error", "1");
            }

            SetProperty("button_exit_label", "Close and back");
        }

        void RunnerExited(bool unexpected)
        {
            hc.SyncContext.Post(o => hc.GetController<IUIController>().ShowDefaultPage(), null);
        }

        public virtual bool Run()
        {
            if (Runner == null)
                return false;

            try
            {
                Runner.Run();
                SetProperty("label_launch_info", string.Format("{0} now launched", Runner.Name));

                return true;
            }
            catch (Exception ex)
            {
                SetProperty("is_error", "1");
                SetProperty("label_launch_info", string.Format("Error launching {0}...{1}{2}", Runner.Name, Environment.NewLine, ex.Message));
                return false;
            }
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch (name)
            {
                case "Exit":
                    ExitRunner();
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }

        private void ExitRunner()
        {
            if (Runner != null)
            {
                Runner.Exit();
                Runner = null;
            }
            else
                RunnerExited(false);
        }
    }
}

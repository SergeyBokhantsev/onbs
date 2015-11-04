using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Input;
using System.Threading;

namespace UIModels
{
    public class ExternalApplicationPage : ModelBase
    {
        private readonly IProcessRunner runner;
        private readonly IUIController ui;

		public ExternalApplicationPage(string modelName, IProcessRunner runner, SynchronizationContext syncContext, ILogger logger, IUIController ui)
            : base(modelName, syncContext, logger)
        {
            this.runner = runner;
            this.ui = ui;

            if (runner != null)
            {
                runner.Exited += RunnerExited;

                SetProperty("label_launch_info", string.Format("Launching {0}...", runner.Name));
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
            syncContext.Post(o => ui.ShowDefaultPage(), null);
        }

        public virtual bool Run()
        {
            if (runner == null)
                return false;

            try
            {
                runner.Run();
                SetProperty("label_launch_info", string.Format("{0} now launched", runner.Name));

                return true;
            }
            catch (Exception ex)
            {
                SetProperty("is_error", "1");
                SetProperty("label_launch_info", string.Format("Error launching {0}...{1}{2}", runner.Name, Environment.NewLine, ex.Message));
                return false;
            }
        }

        protected override void DoAction(PageModelActionEventArgs args)
        {
            switch (args.ActionName)
            {
                case "Cancel":
                    if (args.State == ButtonStates.Press)
                    {
                        if (runner != null)
                            runner.Exit();
                        else
                            RunnerExited(false);
                    }
                    break;
            }
        }
    }
}

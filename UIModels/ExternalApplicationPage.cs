using Interfaces;
using Interfaces.UI;
using System;

namespace UIModels
{
    public abstract class ExternalApplicationPage : ModelBase
    {
		protected IProcessRunner Runner
        {
            get;
			private set;
        }

        protected ExternalApplicationPage(string viewName, IHostController hc, MappedPage pageDescriptor, IProcessRunner runner)
            : base(viewName, hc, pageDescriptor)
        {
            NoDialogsAllowed = true;

            Runner = runner;

            if (Runner != null)
            {
                Runner.Exited += RunnerExited;

                this.Disposing += (s, e) => Runner.Exit();

                SetProperty("label_launch_info", string.Format("Launching {0}...", Runner.ToString()));
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
			if (unexpected)
				Action(new PageModelActionEventArgs(ModelNames.ButtonCancel, 
				                                    Interfaces.Input.ButtonStates.Press));
        }

        public virtual bool Run()
        {
            if (Runner == null)
                return false;

            try
            {
                Runner.Run();
                SetProperty("label_launch_info", string.Format("{0} now launched", Runner.ToString()));

                return true;
            }
            catch (Exception ex)
            {
                hc.Logger.Log(this, ex);
                SetProperty("is_error", "1");
                SetProperty("label_launch_info", string.Format("Error launching {0}...{1}{2}", Runner.ToString(), Environment.NewLine, ex.Message));
                return false;
            }
        }
    }
}

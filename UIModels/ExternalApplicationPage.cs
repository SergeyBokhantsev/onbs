﻿using Interfaces;
using Interfaces.UI;
using ProcessRunnerNamespace;
using System;

namespace UIModels
{
    public abstract class ExternalApplicationPage : ModelBase
    {
		protected ProcessRunner Runner
        {
            get;
			private set;
        }

        protected ExternalApplicationPage(string viewName, IHostController hc, MappedPage pageDescriptor, ProcessRunner runner)
            : base(viewName, hc, pageDescriptor)
        {
            this.Disposing += ExternalApplicationPage_Disposing;

            NoDialogsAllowed = true;

            Runner = runner;

            if (Runner != null)
            {
                Runner.Exited += RunnerExited;

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

        void ExternalApplicationPage_Disposing(object sender, EventArgs e)
        {
            ProcessRunner.TryExitEndDispose(Runner);
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

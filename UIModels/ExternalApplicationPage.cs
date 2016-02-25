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

        //protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        //{
        //    switch (name)
        //    {
        //        case "Exit":
        //            ExitRunner();
        //            break;

        //        default:
        //            base.DoAction(name, actionArgs);
        //            break;
        //    }
        //}

        //private void ExitRunner()
        //{
        //    if (Runner != null)
        //    {
        //        Runner.Exit();
        //        Runner = null;
        //    }
        //    else
        //        RunnerExited(false);
        //}
    }
}

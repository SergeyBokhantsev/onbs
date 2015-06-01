using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIController.Models
{
    public class ExternalApplicationPage : ModelBase
    {
        private readonly IProcessRunner runner;
        private readonly IUIController ui;

        public ExternalApplicationPage(IProcessRunner runner, IDispatcher dispatcher, ILogger logger, IUIController ui)
            : base(typeof(ExternalApplicationPage).Name, dispatcher, logger)
        {
            if (runner == null)
                throw new ArgumentNullException("runner");

            this.runner = runner;
            this.ui = ui;

            runner.Exited += RunnerExited;

            SetProperty("label_launch_info", string.Format("Launching {0}...", runner.Name));   
        }

        void RunnerExited(bool unexpected)
        {
            dispatcher.Invoke(null, null, new EventHandler((s, e) => ui.ShowMainPage()));
        }

        public void Run()
        {
            try
            {
                runner.Run();
                SetProperty("label_launch_info", string.Format("{0} now launched", runner.Name));   
            }
            catch (Exception ex)
            {
                SetProperty("label_launch_info", string.Format("Error launching {0}...{1}{2}", runner.Name, Environment.NewLine, ex.Message));
            }
        }

        protected override void DoAction(PageModelActionEventArgs actionArgs)
        {
            if (actionArgs.ActionName == "close")
            {
                runner.Exit();
            }
        }

        protected override void OnCancelButton(Interfaces.Input.ButtonStates state)
        {
            DoAction(new PageModelActionEventArgs("close"));
        }
    }
}

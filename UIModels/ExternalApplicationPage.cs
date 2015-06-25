﻿using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Input;

namespace UIModels
{
    public class ExternalApplicationPage : ModelBase
    {
        private readonly IProcessRunner runner;
        private readonly IUIController ui;

		public ExternalApplicationPage(string modelName, IProcessRunner runner, IDispatcher dispatcher, ILogger logger, IUIController ui)
            : base(modelName, dispatcher, logger)
        {
            this.runner = runner;
            this.ui = ui;

            runner.Exited += RunnerExited;

            SetProperty("label_launch_info", string.Format("Launching {0}...", runner.Name));
            SetProperty("is_error", "0");
            SetProperty("button_exit_label", "Close and back");
        }

        void RunnerExited(bool unexpected)
        {
            dispatcher.Invoke(null, null, new EventHandler((s, e) => ui.ShowDefaultPage()));
        }

        public virtual void Run()
        {
            try
            {
                if (runner == null)
                    throw new Exception("Runner was not provided");

                runner.Run();
                SetProperty("label_launch_info", string.Format("{0} now launched", runner.Name));
            }
            catch (Exception ex)
            {
                SetProperty("is_error", "1");
                SetProperty("label_launch_info", string.Format("Error launching {0}...{1}{2}", runner.Name, Environment.NewLine, ex.Message));
            }
        }

        protected override void DoAction(PageModelActionEventArgs args)
        {
            switch (args.ActionName)
            {
                case "Cancel":
                    if (args.State == ButtonStates.Press)
                    {
                        runner.Exit();
                    }
                    break;
            }
        }
    }
}
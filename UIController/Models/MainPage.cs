using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace UIController.Models
{
    public class MainPage : ModelBase
    {
        private readonly IHostController hostController;

        public MainPage(IHostController hostController)
            :base(typeof(MainPage).Name, hostController.Dispatcher, hostController.Logger)
        {
            this.hostController = hostController;

            SetProperty("label_f1", "F1 to Navit");
        }

        protected override void DoAction(PageModelActionEventArgs actionArgs)
        {
            switch (actionArgs.ActionName)
            {
                case "navit":
                    var appName = hostController.
                    var runner = hostController.CreateProcessRunner()
                    break;
            }
        }

        protected override void OnF1Button(Interfaces.Input.ButtonStates state)
        {
            DoAction(new PageModelActionEventArgs("navit"));
        }
    }
}

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
                    var uiController = hostController.GetController<IUIController>();
                    var appName = hostController.Config.GetString("navit exe");
                    var appArgs = hostController.Config.GetString("navit args");
                    var runner = hostController.CreateProcessRunner(appName, null);
                    var page = new ExternalApplicationPage(runner, hostController.Dispatcher, hostController.Logger, uiController);                    
                    uiController.ShowPage(page);
                    page.Run();
                    break;
            }
        }

        protected override void OnF1Button(Interfaces.Input.ButtonStates state)
        {
            DoAction(new PageModelActionEventArgs("navit"));
        }
    }
}

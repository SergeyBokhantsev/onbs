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
        private const string navigationAppKey = "Navit";

        private readonly IHostController hostController;

        public MainPage(IHostController hostController)
            :base(typeof(MainPage).Name, hostController.Dispatcher, hostController.Logger)
        {
            this.hostController = hostController;

            SetProperty("label_f1", string.Format("F1 to {0}", navigationAppKey));
        }

        protected override void DoAction(PageModelActionEventArgs actionArgs)
        {
            switch (actionArgs.ActionName)
            {
                case "navigation":
                    var uiController = hostController.GetController<IUIController>();
                    var runner = hostController.CreateProcessRunner(navigationAppKey);
                    var page = new ExternalApplicationPage(runner, hostController.Dispatcher, hostController.Logger, uiController);                    
                    uiController.ShowPage(page);
                    page.Run();
                    break;
            }
        }

        protected override void OnF1Button(Interfaces.Input.ButtonStates state)
        {
            DoAction(new PageModelActionEventArgs("navigation"));
        }
    }
}

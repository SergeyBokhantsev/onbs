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
        public MainPage(IDispatcher dispatcher, ILogger logger)
            :base(typeof(MainPage).Name, dispatcher, logger)
        {
            SetProperty("welcome", "Welcome!");
        }

        protected override void DoAction(PageModelActionEventArgs actionArgs)
        {
            switch (actionArgs.ActionName)
            {
                case "start":
                    SetProperty("welcome", Guid.NewGuid().ToString());
                    break;
            }
        }

        protected override void OnAcceptButton(Interfaces.Input.ButtonStates state)
        {
            SetProperty("welcome", Guid.NewGuid().ToString());
        }
    }
}

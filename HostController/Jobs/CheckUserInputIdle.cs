using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace HostController.Jobs
{
    internal class CheckUserInputIdle
    {
        private readonly IHostController hc;

        private IOperationGuard locker = new InterlockedGuard();

        public CheckUserInputIdle(IHostController hc)
        {
            this.hc = Ensure.ArgumentIsNotNull(hc);
            hc.CreateTimer(10000, t => locker.ExecuteIfFree(Check), true, false, "CheckUserInputIddle timer");
        }

        private void Check()
        {
            var maxIdleTime = hc.Config.GetInt(ConfigNames.TurnOffAftrerInputIdleMinutes);

            if (hc.GetController<IInputController>().IddleMinutes >= maxIdleTime
                && hc.GetController<IUIController>().UserIdleMinutes >= maxIdleTime
                && hc.GetController<IGPSController>().IdleMinutes >= maxIdleTime)
            {
                hc.SyncContext.Post(async state => await ShowDialog(state), maxIdleTime, "CheckUserInputIdle Showdialog call");
            }
        }

        private async Task ShowDialog(object state)
        {
            int iddleTime = (int)state;

            var dr = await hc.GetController<IUIController>().ShowDialogAsync(new UIModels.Dialogs.OkDialog("Turn Off", "User is inactive, press cancel to continue", "Cancel", hc, 60000, DialogResults.Yes));

            if (dr == DialogResults.Yes)
            {
                hc.Logger.Log(this, string.Format("Turning off system because of user's inactivity for {0} minutes", iddleTime), LogLevels.Info);
                hc.SyncContext.Post(o => hc.Shutdown(HostControllerShutdownModes.Shutdown), null, "Shutdown call from CheckUserInputIddle");
            }
        }
    }
}

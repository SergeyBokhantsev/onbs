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

        private ManualResetGuard locker = new ManualResetGuard();

        public CheckUserInputIdle(IHostController hc)
        {
            this.hc = Ensure.ArgumentIsNotNull(hc);
            hc.CreateTimer(10000, t => locker.ExecuteIfFree(Check), true, false, "CheckUserInputIddle timer");
        }

        private void Check()
        {
            var maxIdleTime = hc.Config.GetInt(ConfigNames.TurnOffAftrerInputIdleMinutes);

            var inputIdleMinutes = hc.GetController<IInputController>().IddleMinutes;
            var uiIdleMinutes = hc.GetController<IUIController>().UserIdleMinutes;
            var gpsIdleMinutes = hc.GetController<IGPSController>().IdleMinutes;

            if ( inputIdleMinutes >= maxIdleTime
                && uiIdleMinutes >= maxIdleTime
                && gpsIdleMinutes >= maxIdleTime)
            {
                hc.Logger.Log(this, string.Format("System idling detected. Input idle for {0} minutes, UI idle for {1} minutes, GPS idle for {2} minutes", inputIdleMinutes, uiIdleMinutes, gpsIdleMinutes), LogLevels.Info);
                hc.SyncContext.Post(async state => await ShowDialog(state), maxIdleTime, "CheckUserInputIdle Showdialog call");
            }
            else
            {
                locker.Reset();
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
            else
            {
                locker.Reset();
            }
        }
    }
}

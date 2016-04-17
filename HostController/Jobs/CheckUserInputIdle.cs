using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostController.Jobs
{
    internal class CheckUserInputIdle
    {
        private readonly IHostController hc;

        private IOperationGuard locker = new InterlockedGuard();

        public CheckUserInputIdle(IHostController hc)
        {
            this.hc = Ensure.ArgumentIsNotNull(hc);
            hc.CreateTimer(10000, Check, true, false, "CheckUserInputIddle timer");
        }

        private void Check(IHostTimer obj)
        {
            locker.ExecuteIfFreeAsync(() =>
            {
                var maxIdleTime = hc.Config.GetInt(ConfigNames.TurnOffAftrerInputIdleMinutes);

                if (hc.GetController<IInputController>().IddleMinutes >= maxIdleTime
                    && hc.GetController<IUIController>().UserIdleMinutes >= maxIdleTime
                    && hc.GetController<IGPSController>().IdleMinutes >= maxIdleTime)
                {
                    var dialogTask = hc.GetController<IUIController>().ShowDialogAsync(new UIModels.Dialogs.OkDialog("Turn Off", "User is inactive, press cancel to continue", "Cancel", hc, 60000, DialogResults.Yes));

                    dialogTask.Wait();

                    if (dialogTask.Result == DialogResults.Yes)
                    {
                        hc.Logger.Log(this, string.Format("Turning off system because of user's inactivity for {0} minutes", maxIdleTime), LogLevels.Info);
                        hc.SyncContext.Post(o => hc.Shutdown(HostControllerShutdownModes.Shutdown), null, "Shutdown call from CheckUserInputIddle");
                    }
                }
            });
        }

    }
}

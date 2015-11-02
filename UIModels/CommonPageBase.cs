using Interfaces;
using Interfaces.GPS;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels
{
    public abstract class CommonPageBase : ModelBase
    {
        protected readonly IHostController hc;
        protected readonly IDispatcherTimer primaryTimer;
        protected readonly IDispatcherTimer secondaryTimer;

        protected bool Disposed { get; private set; }

        protected CommonPageBase(IHostController hc, string modelName)
            :base(modelName, hc.Dispatcher, hc.Logger)
        {
            if (hc == null)
                throw new ArgumentNullException("IHostController");

            this.hc = hc;
            this.Disposing += OnDisposing;

            onlyPressButtonEvents = true;

            primaryTimer = hc.Dispatcher.CreateTimer(1000, OnPrimaryTick);
            primaryTimer.Enabled = true;

            secondaryTimer = hc.Dispatcher.CreateTimer(60000, OnSecondaryTimer);
            secondaryTimer.Enabled = true;
        }

        protected virtual void OnPrimaryTick(object sender, EventArgs e)
        {
            if (Disposed)
                return;

            SetProperty("ard_status", hc.GetController<IArduinoController>().IsCommunicationOk);
            SetProperty("inet_status", hc.Config.IsInternetConnected);

            if (hc.Config.IsSystemTimeValid)
                SetProperty("time", DateTime.Now.AddHours(hc.Config.GetInt(ConfigNames.SystemTimeLocalZone)));
        }

        protected virtual void OnSecondaryTimer(object sender, EventArgs e)
        {
        }

        protected override void DoAction(Interfaces.UI.PageModelActionEventArgs args)
        {
            if (Disposed)
                return;

            switch (args.ActionName)
            {
                case ModelNames.ButtonCancel:
                    hc.GetController<IUIController>().ShowDefaultPage();
                    break;
            }
        }

        protected virtual void OnDisposing(object sender, EventArgs e)
        {
            primaryTimer.Dispose();
            secondaryTimer.Dispose();
            Disposed = true;
        }
    }
}

using Interfaces;
using Interfaces.GPS;
using Interfaces.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using UIModels.MultipurposeModels;

namespace UIModels
{
    public abstract class DrivePageBase : RotaryListModel<MappedActionBase>
    {
        protected readonly static Dictionary<string, object> crossPageProperties = new Dictionary<string, object>();

        protected readonly IHostTimer primaryTimer;
        protected readonly IHostTimer secondaryTimer;

        protected readonly IGPSController gpsController;

        private readonly List<ListItem<MappedActionBase>> rotaryItems = new List<ListItem<MappedActionBase>>(10);

        protected DrivePageBase(string viewName, IHostController hc, MappedPage pageDescriptor, int focusedIndex = 0)
            :base(viewName, hc, pageDescriptor, "list", 6, focusedIndex)
        {
            this.Disposing += OnDisposing;

            ReadCrossPageProps();

            gpsController = hc.GetController<IGPSController>();

            primaryTimer = hc.CreateTimer(1000, OnPrimaryTick, true, true, "common primary timer");
            secondaryTimer = hc.CreateTimer(60000, OnSecondaryTimer, true, true, "common secondary timer");

            foreach (var mappedAction in pageDescriptor.ButtonsMap)
            {
                if (mappedAction.ButtonActionName.StartsWith("RotaryItem_"))
                {
                    ListItem<MappedActionBase>.PrepareItem(hc.SyncContext, ref rotaryItems, mappedAction, (s, e) => Action(new PageModelActionEventArgs(mappedAction.ButtonActionName, Interfaces.Input.ButtonStates.Press)), mappedAction.Caption);
                }
            }

            SetProperty(ModelNames.ButtonPrevLabel, "<<");
            SetProperty(ModelNames.ButtonNextLabel, ">>");
        }

        private void ReadCrossPageProps()
        {
            foreach (var key in crossPageProperties.Keys)
                SetProperty(key, crossPageProperties[key]);
        }

        private void SaveCrossPageProps()
        {
            foreach (var key in crossPageProperties.Keys)
                crossPageProperties[key] = GetProperty<object>(key);
        }

        protected override IList<ListItem<MappedActionBase>> QueryItems(int skip, int take)
        {
            return rotaryItems.Skip(skip).Take(take).ToList();
        }

        protected virtual void OnPrimaryTick(IHostTimer timer)
        {
            if (Disposed)
                return;

            SetProperty("ard_status", hc.GetController<IArduinoController>().IsCommunicationOk);
            SetProperty("inet_status", hc.Config.IsInternetConnected);
            SetProperty("gps_status", hc.Config.IsGPSLock);            
            SetProperty("dim_light", hc.Config.IsDimLighting);
            SetProperty("warning_log", hc.Logger.LastWarningTime.AddSeconds(30) > DateTime.Now);

            if (hc.Config.IsSystemTimeValid)
                SetProperty("time", DateTime.Now.AddHours(hc.Config.GetInt(ConfigNames.SystemTimeLocalZone)));
        }

        protected virtual void OnSecondaryTimer(IHostTimer timer)
        {
        }

        protected virtual void OnDisposing(object sender, EventArgs e)
        {
            primaryTimer.Dispose();
            secondaryTimer.Dispose();
            SaveCrossPageProps();
        }
    }
}

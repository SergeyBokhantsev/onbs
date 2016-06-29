using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;
using OBD;
using UIModels.MiniDisplay;

namespace UIModels
{
    public class WelcomeModel : ModelBase
    {
        private readonly DriveMiniDisplayModel miniDisplayModel;

        private readonly IOperationGuard obdGuard = new InterlockedGuard();
        private readonly IOperationGuard minidisplayGuard = new TimedGuard(new TimeSpan(0, 0, 1));

        private readonly IElm327Controller elm;
        private readonly OBDProcessor obdProcessor;

        private readonly ITravelController tc;

        public WelcomeModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            tc = hc.GetController<ITravelController>();

            elm = hc.GetController<IElm327Controller>();
            obdProcessor = new OBDProcessor(elm);

            miniDisplayModel = new DriveMiniDisplayModel(hc, pageDescriptor.Name);

            var timer = hc.CreateTimer(1000, OnTimerTick, true, true, "welcome screen timer");

            this.Disposing += (s, e) => timer.Dispose();

            SetProperty("label_message", "Welcome. GREEN to start.");
        }

        private void OnTimerTick(IHostTimer timer)
        {
            if (!Disposed)
            {
                obdGuard.ExecuteIfFreeAsync(UpdateOBD);
                minidisplayGuard.ExecuteIfFree(UpdateMiniDisplay);
            }
        }

        private void UpdateMiniDisplay()
        {
            miniDisplayModel.BufferedPoints = tc.BufferedPoints;
            miniDisplayModel.SendedPoints = tc.SendedPoints;
            miniDisplayModel.Draw();
        }

        private void UpdateOBD()
        {
            var engineTemp = obdProcessor.GetCoolantTemp() ?? int.MinValue;
            miniDisplayModel.EngineTemp = engineTemp;

            Thread.Sleep(5000);
        }

        
    }
}

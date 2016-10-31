using Interfaces;
using Interfaces.UI;
using ProcessRunnerNamespace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UIModels
{
    public sealed class WebCamPage : ExternalApplicationPage
    {
        private const string WebcamControlCommand = "WebcamControlCommand";
        private const string WebcamControlColorArgs = "WebcamControlColorArgs";
        private const string WebcamControlContrastArgs = "WebcamControlContrastArgs";
		private const string WebcamControlBrightArgs = "WebcamControlBrightArgs";
        private const string WebcamControlColor = "WebcamControlColor";
        private const string WebcamControlContrast = "WebcamControlContrast";
		private const string WebcamControlBright = "WebcamControlBright";

        private readonly IOperationGuard guard = new InterlockedGuard();

        private bool configDirty;

        private int Color
        {
            get
            {
                return hc.Config.GetInt(WebcamControlColor);
            }
            set
            {
                if (value > 100)
                    value = 100;
                else if (value < 0)
                    value = 0;

                hc.Config.Set(WebcamControlColor, value);
                configDirty = true;
            }
        }

        private int Contrast
        {
            get
            {
                return hc.Config.GetInt(WebcamControlContrast);
            }
            set
            {
                if (value > 100)
                    value = 100;
                else if (value < 0)
                    value = 0;

                hc.Config.Set(WebcamControlContrast, value);
                configDirty = true;
            }
        }

		private int Bright
		{
			get
			{
				return hc.Config.GetInt(WebcamControlBright);
			}
			set
			{
				if (value > 100)
					value = 100;
				else if (value < 0)
					value = 0;

				hc.Config.Set(WebcamControlBright, value);
				configDirty = true;
			}
		}

        public WebCamPage(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor, ProcessRunner.ForInteractiveApp(hc.Config.GetString("cam_exe"), hc.Config.GetString("cam_args")))
        {
            this.Disposing += WebCamPageDisposing;
			Run();
        }

        public override bool Run()
        {
            if (base.Run())
            {
                Thread.Sleep(3000);
                UpdateColor(0).Wait();
                UpdateContrast(0).Wait();
                UpdateBright(0).Wait();
                return true;
            }
            else
                return false;
        }

        void WebCamPageDisposing(object sender, EventArgs e)
        {
            if (!guard.ExecuteIfFree(() =>
                {
                    if (configDirty)
                        hc.Config.Save();
                }))
            {
                Thread.Sleep(1000);
                WebCamPageDisposing(null, null);
            }
        }

        private async Task ExecuteWebCamCommand(string args)
        {
            await ProcessRunner.ExecuteToolAsync("UpdateColor", str => null as string, 3000, hc.Config.GetString(WebcamControlCommand), args);
        }

        private async Task UpdateColor(int delta)
        {
            if (delta > 0)
                Color = Color + delta;

            await ExecuteWebCamCommand(string.Format(hc.Config.GetString(WebcamControlColorArgs), Color));
        }

        private async Task UpdateContrast(int delta)
        {
            if (delta > 0)
                Contrast = Contrast + delta;

            await ExecuteWebCamCommand(string.Format(hc.Config.GetString(WebcamControlContrastArgs), Contrast));
        }

		private async Task UpdateBright(int delta)
		{
            if (delta > 0)
                Bright = Bright + delta;

            await ExecuteWebCamCommand(string.Format(hc.Config.GetString(WebcamControlBrightArgs), Bright));
		}

        protected override async Task DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch(name)
            {
                case "Color+":
                    await guard.ExecuteIfFreeAsync(() => UpdateColor(10));
                    break;

                case "Color-":
                    await guard.ExecuteIfFreeAsync(() => UpdateColor(-10));
                    break;

                case "Contrast+":
                    await guard.ExecuteIfFreeAsync(() => UpdateContrast(10));
                    break;

                case "Contrast-":
                    await guard.ExecuteIfFreeAsync(() => UpdateContrast(-10));
                    break;

                case "Bright+":
                    await guard.ExecuteIfFreeAsync(() => UpdateBright(10));
                    break;

                case "Bright-":
                    await guard.ExecuteIfFreeAsync(() => UpdateBright(-10));
                    break;

                default:
                    await base.DoAction(name, actionArgs);
                    break;
            }
        }
    }
}

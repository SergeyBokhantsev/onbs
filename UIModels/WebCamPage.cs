using Interfaces;
using Interfaces.Input;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UIController;

namespace UIModels
{
    public class WebCamPage : ExternalApplicationPage
    {
        private const string WebcamControlCommand = "WebcamControlCommand";
        private const string WebcamControlColorArgs = "WebcamControlColorArgs";
        private const string WebcamControlContrastArgs = "WebcamControlContrastArgs";
		private const string WebcamControlBrightArgs = "WebcamControlBrightArgs";
        private const string WebcamControlColor = "WebcamControlColor";
        private const string WebcamControlContrast = "WebcamControlContrast";
		private const string WebcamControlBright = "WebcamControlBright";

        private readonly object locker = new object();

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

                hc.Config.Set<int>(WebcamControlColor, value);
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

                hc.Config.Set<int>(WebcamControlContrast, value);
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

				hc.Config.Set<int>(WebcamControlBright, value);
				configDirty = true;
			}
		}

        public WebCamPage(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            : base(viewName, hc, pageDescriptor, hc.ProcessRunnerFactory.Create("cam"))
        {
            this.Disposing += WebCamPageDisposing;
        }

        public override bool Run()
        {
            if (base.Run())
            {
                Thread.Sleep(3000);
                UpdateColor();
                UpdateContrast();
                return true;
            }
            else
                return false;
        }

        void WebCamPageDisposing(object sender, EventArgs e)
        {
            lock (locker)
            {
                if (configDirty)
                    hc.Config.Save();
            }
        }

        private void UpdateColor()
        {
            var command = hc.Config.GetString(WebcamControlCommand);
            var arg = string.Format(hc.Config.GetString(WebcamControlColorArgs), Color);
            hc.ProcessRunnerFactory.Create(command, arg, false).Run();
            Thread.Sleep(300);
        }

        private void UpdateContrast()
        {
            var command = hc.Config.GetString(WebcamControlCommand);
            var arg = string.Format(hc.Config.GetString(WebcamControlContrastArgs), Contrast);
            hc.ProcessRunnerFactory.Create(command, arg, false).Run();
            Thread.Sleep(300);
        }

		private void UpdateBright()
		{
			var command = hc.Config.GetString(WebcamControlCommand);
			var arg = string.Format(hc.Config.GetString(WebcamControlBrightArgs), Bright);
			hc.ProcessRunnerFactory.Create(command, arg, false).Run();
			Thread.Sleep(300);
		}

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            switch(name)
            {
                case "Color+":
                    lock (locker)
                    {
                        Color = Color + 10;
                        UpdateColor();
                    }
                    break;

                case "Color-":
                    lock (locker)
                    {
                        Color = Color - 10;
                        UpdateColor();
                    }
                    break;

                case "Contrast+":
                    lock (locker)
                    {
                        Contrast = Contrast + 10;
                        UpdateContrast();
                    }
                    break;

                case "Contrast-":
                    lock (locker)
                    {
                        Contrast = Contrast - 10;
                        UpdateContrast();
                    }
                    break;

                case "Bright+":
                    lock (locker)
                    {
                        Bright = Bright + 10;
                        UpdateBright();
                    }
                    break;

                case "Bright-":
                    lock (locker)
                    {
                        Bright = Bright - 10;
                        UpdateBright();
                    }
                    break;

                default:
                    base.DoAction(name, actionArgs);
                    break;
            }
        }
    }
}

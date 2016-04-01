using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DashCamController;
using Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DashCam
{
    public class TestDashCamController : DashCamController.DashCamController
    {
        public event Action OnCreatePictureProcessConfig;
        public event Action OnCreateRecordProcessConfig;

        public TestDashCamController(IHostController hc)
            :base(hc)
        {
        }

        protected override ProcessConfig CreatePictureProcessConfig(int width, int height)
        {
            if (OnCreatePictureProcessConfig != null)
                OnCreatePictureProcessConfig();

            return new ProcessConfig
            {
                ExePath = "ping_foo",
                Args = "localhost"
            };
        }

        protected override ProcessConfig CreateRecordProcessConfig()
        {
            if (OnCreateRecordProcessConfig != null)
                OnCreateRecordProcessConfig();

            return new ProcessConfig
            {
                ExePath = "ping",
                Args = "localhost"
            };
        }
    }

    [TestClass]
    public class DashCamControllerTest
    {
        private void InitSettings(IConfig cfg)
        {
            cfg.Set(ConfigNames.DashCamRecorderFolder, Path.GetTempPath());
            cfg.Set(ConfigNames.DashCamRecorderFilesNumberQuota, 10);

            cfg.Set(ConfigNames.DashCamRecorderExe, "");
            cfg.Set(ConfigNames.DashCamRecorderArg, "");
            cfg.Set(ConfigNames.DashCamRecorderSplitIntervalSec, 0);
            cfg.Set(ConfigNames.DashCamPictureExe, "");
            cfg.Set(ConfigNames.DashCamPictureArg, "");
        }

        [TestMethod]
        public void TestDisabledRecordings()
        {
            //INIT
            var hc = new Mocks.MockHostController();

            InitSettings(hc.Config);
            hc.Config.Set(ConfigNames.DashCamRecorderEnabled, false);

            int createPictureProcessConfigCalls = 0;
            int createRecordProcessConfigCalls = 0;

            var ctrl = new TestDashCamController(hc);
            ctrl.OnCreatePictureProcessConfig += () => createPictureProcessConfigCalls++;
            ctrl.OnCreateRecordProcessConfig += () => createRecordProcessConfigCalls++;

            Thread.Sleep(3000);
            Assert.AreEqual(0, createPictureProcessConfigCalls);
            Assert.AreEqual(0, createRecordProcessConfigCalls);
        }

        [TestMethod]
        public void TestEnabledRecordings()
        {
            //INIT
            var hc = new Mocks.MockHostController();

            InitSettings(hc.Config);
            hc.Config.Set(ConfigNames.DashCamRecorderEnabled, false);

            int createPictureProcessConfigCalls = 0;
            int createRecordProcessConfigCalls = 0;

            var ctrl = new TestDashCamController(hc);
            ctrl.OnCreatePictureProcessConfig += () => createPictureProcessConfigCalls++;
            ctrl.OnCreateRecordProcessConfig += () => createRecordProcessConfigCalls++;
            hc.Config.Set(ConfigNames.DashCamRecorderEnabled, true);

            Thread.Sleep(10000);

            Assert.AreEqual(0, createPictureProcessConfigCalls);
            Assert.AreEqual(3, createRecordProcessConfigCalls);
        }

        [TestMethod]
        public void TestRecordingsDisabling()
        {
            //INIT
            var hc = new Mocks.MockHostController();

            InitSettings(hc.Config);
            hc.Config.Set(ConfigNames.DashCamRecorderEnabled, false);

            int createPictureProcessConfigCalls = 0;
            int createRecordProcessConfigCalls = 0;

            var ctrl = new TestDashCamController(hc);
            ctrl.OnCreatePictureProcessConfig += () => createPictureProcessConfigCalls++;
            ctrl.OnCreateRecordProcessConfig += () => createRecordProcessConfigCalls++;
            hc.Config.Set(ConfigNames.DashCamRecorderEnabled, true);

            Thread.Sleep(10000);
            hc.Config.Set(ConfigNames.DashCamRecorderEnabled, false);

            Thread.Sleep(5000);

            Assert.AreEqual(0, createPictureProcessConfigCalls);
            Assert.AreEqual(3, createRecordProcessConfigCalls);
        }

        [TestMethod]
        public void TestOrderOnePictureWithRecordings()
        {
            //INIT
            var hc = new Mocks.MockHostController();

            InitSettings(hc.Config);
            hc.Config.Set(ConfigNames.DashCamRecorderEnabled, false);

            int createPictureProcessConfigCalls = 0;
            int createRecordProcessConfigCalls = 0;

            int picturesCallbacks = 0;

            var ctrl = new TestDashCamController(hc);
            ctrl.OnCreatePictureProcessConfig += () => createPictureProcessConfigCalls++;
            ctrl.OnCreateRecordProcessConfig += () => createRecordProcessConfigCalls++;

            //Thread.Sleep(100000);

            ctrl.OrderPicture(10, 10, ms =>
                {
                    picturesCallbacks++;
                });

            hc.Config.Set(ConfigNames.DashCamRecorderEnabled, true);

            Thread.Sleep(10000);

            hc.Config.Set(ConfigNames.DashCamRecorderEnabled, false);

            Assert.AreEqual(1, createPictureProcessConfigCalls);
            Assert.AreEqual(3, createRecordProcessConfigCalls);
            Assert.AreEqual(1, picturesCallbacks);
        }
    }
}

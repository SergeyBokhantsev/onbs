using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interfaces;
using System.Linq;
using System.IO;

namespace Tests
{
    public class MockProcessRunnerFactoryForNixHelpers : IProcessRunnerFactory
    {
        public class MockProcessRunnerForNixHelpers  : IProcessRunner
        {
            public event ExitedEventHandler Exited;

            public string Name
            {
                get;
                set;
            }

            public bool HasExited
            {
                get;
                set;
            }

            public void Run()
            {
            }

            public void SendToStandardInput(string message)
            {
            }

            public string GetFromStandardOutput()
            {
                return File.ReadAllText("all.txt");
            }

            public bool WaitForExit(int timeoutMilliseconds)
            {
                return true;
            }

            public void Exit()
            {
            }
        }

        public IProcessRunner Create(string appKey)
        {
            return null;
        }

        public IProcessRunner Create(string exePath, string args, bool waitForUI)
        {
            return new MockProcessRunnerForNixHelpers();
        }
    }

    [TestClass]
    [DeploymentItem("Data\\dmesg")]
    public class NixHelpersTest
    {
        [TestMethod]
        public void FindUSBDevice()
        {
            //INIT
            var prf = new MockProcessRunnerFactoryForNixHelpers();

            //ACT
            var device = NixHelpers.DmesgFinder.FindUSBDevice("0403", "6001", prf);
            var no_device = NixHelpers.DmesgFinder.FindUSBDevice("99hh", "6001", prf);

            //ASSERT
            Assert.IsNull(no_device);

            Assert.IsNotNull(device);
            Assert.AreEqual("/dev/ttyUSB0", device.AttachedTo.First());
        }

        [TestMethod]
        public void ParseUSBDevices()
        {
            var i = NixHelpers.USBDevice.Parse(File.ReadAllText("all.txt")).ToArray();

            var t = i;
        }
    }
}

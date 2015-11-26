using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interfaces;
using System.Linq;

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
                return "[ 0.000000] console [tty0] enabled" + Environment.NewLine
                    + "[ 34.210105] usb 2-1.1: GSM modem (1-port) converter now attached to ttyUSB0" + Environment.NewLine
                    + "[ 34.210198] usb 2-1.1: GSM modem (1-port) converter now attached to ttyUSB1" + Environment.NewLine
                    + "[ 34.210285] usb 2-1.1: FTDI now attached to ttyUSB2" + Environment.NewLine
                    + "[ 41.586902] Bluetooth: RFCOMM TTY layer initialized" + Environment.NewLine
                    + "[ 197.980023] option1 ttyUSB0: option_instat_callback: error -108" + Environment.NewLine;
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
    public class NixHelpersTest
    {
        [TestMethod]
        public void EnumerateTTYUSBDevices()
        {
            //INIT
            var prf = new MockProcessRunnerFactoryForNixHelpers();

            //ACT
            var ttyUSBDevices = NixHelpers.DmesgFinder.EnumerateTTYUSBDevices(prf).ToArray();

            //ASSERT
            Assert.AreEqual(3, ttyUSBDevices.Length);
            Assert.IsTrue(ttyUSBDevices[0].Contains("ttyUSB0"));
            Assert.IsTrue(ttyUSBDevices[1].Contains("ttyUSB1"));
            Assert.IsTrue(ttyUSBDevices[2].Contains("ttyUSB2"));
        }

        [TestMethod]
        public void FindTTYUSBPort()
        {
            //INIT
            var devName1 = "GSM modem (1-port)";
            var devName2 = "FTDI";
            var prf = new MockProcessRunnerFactoryForNixHelpers();

            //ACT
            var port1 = NixHelpers.DmesgFinder.FindTTYUSBPort(devName1, prf);
            var port2 = NixHelpers.DmesgFinder.FindTTYUSBPort(devName2, prf);

            //ASSERT
            Assert.AreEqual("/dev/ttyUSB0", port1);
            Assert.AreEqual("/dev/ttyUSB2", port2);
        }
    }
}

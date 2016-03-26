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

            private readonly string filename;

            public MockProcessRunnerForNixHelpers(string filename)
            {
                this.filename = filename;
            }

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

            public bool WaitForExit(int timeoutMilliseconds)
            {
                return true;
            }

            public void Exit()
            {
            }


            public void SendToStandardInput(char c)
            {
                throw new NotImplementedException();
            }


            public bool WaitForExit(int timeoutMilliseconds, out MemoryStream output)
            {
                output = new MemoryStream(File.ReadAllBytes(filename));
                return true;
            }
        }

        private readonly string filename;

        public MockProcessRunnerFactoryForNixHelpers(string filename)
        {
            this.filename = filename;
        }

        public IProcessRunner Create(string appKey, object[] args)
        {
            return new MockProcessRunnerForNixHelpers(filename);
        }

        public IProcessRunner Create(ProcessConfig config)
        {
            return new MockProcessRunnerForNixHelpers(filename);
        }
    }

    [TestClass]
    public class NixHelpersTest
    {
        [TestMethod]
        [DeploymentItem("Data\\dmesg")]
        public void FindUSBDevice()
        {
            //INIT
            var prf = new MockProcessRunnerFactoryForNixHelpers("all.txt");

            //ACT
            var device = NixHelpers.DmesgFinder.FindUSBDevice("0403", "6001", prf);
            var no_device = NixHelpers.DmesgFinder.FindUSBDevice("99hh", "6001", prf);

            //ASSERT
            Assert.IsNull(no_device);

            Assert.IsNotNull(device);
            Assert.AreEqual("/dev/ttyUSB0", device.AttachedTo.First());
        }

        [TestMethod]
        [DeploymentItem("Data\\dmesg")]
        public void ParseUSBDevices()
        {
            var i = NixHelpers.USBDevice.Parse(File.ReadAllText("all.txt")).ToArray();

            var t = i;
        }

        [TestMethod]
        [DeploymentItem("Data\\ps")]
        public void FindProcess()
        {
            //INIT
            var prf = new MockProcessRunnerFactoryForNixHelpers("ps.txt");

            //ACT
            var kworkerPID = NixHelpers.ProcessFinder.FindProcess("kworker", prf);
            var bashPID = NixHelpers.ProcessFinder.FindProcess("bash", prf);
            var sudoPID = NixHelpers.ProcessFinder.FindProcess("sudo", prf);
            var nonExistPID = NixHelpers.ProcessFinder.FindProcess("someFakeApp", prf);

            //ASSERT
            Assert.AreEqual(1, kworkerPID);
            Assert.AreEqual(12, bashPID);
            Assert.AreEqual(123, sudoPID);
            Assert.AreEqual(-1, nonExistPID);
        }
    }
}

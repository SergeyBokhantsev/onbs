using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TestProcessRunner
    {
        [TestMethod]
        public void TestPingProcess()
        {
            //INIT
            var logger = new Mocks.Logger();
            var config = new ProcessConfig
            {
                ExePath = "ping",
                Args = "localhost"
            };

            var ping = new ProcessRunner.ProcessRunnerImpl(config, logger);

            //ACT
            ping.Run();

            MemoryStream ms;
            var waitResult = ping.WaitForExit(5000, out ms);

            var output = ms.GetString();

            //ASSERT
            Assert.IsTrue(waitResult);
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue(output.Contains("Approximate round trip times in milli-seconds:"));
        }

        [TestMethod]
        public void TestInvalidProcess()
        {
            //INIT
            var logger = new Mocks.Logger();
            var config = new ProcessConfig
            {
                ExePath = "foo_foo"
            };

            var pr = new ProcessRunner.ProcessRunnerImpl(config, logger);
            bool thrownOnRun = false;

            //ACT
            try
            {
                pr.Run();
            }
            catch (Exception)
            {
                thrownOnRun = true;
            }

            //ASSERT
            Assert.IsTrue(thrownOnRun);
        }

        [TestMethod]
        public void TestPingProcessTimeout()
        {
            //INIT
            var logger = new Mocks.Logger();
            var config = new ProcessConfig
            {
                ExePath = "ping",
                Args = "localhost -t"
            };

            var ping = new ProcessRunner.ProcessRunnerImpl(config, logger);

            //ACT
            ping.Run();

            MemoryStream ms;
            var waitResult = ping.WaitForExit(5000, out ms);

            //ASSERT
            Assert.IsFalse(waitResult);
            Assert.IsNotNull(ms);
        }

        [TestMethod]
        public void TestPingProcessTInterrupt()
        {
            //INIT
            var logger = new Mocks.Logger();
            var config = new ProcessConfig
            {
                ExePath = "ping",
                Args = "localhost -t"
            };

            var ping = new ProcessRunner.ProcessRunnerImpl(config, logger);

            new Thread(() =>
            {
                Thread.Sleep(1000);
                ping.Exit();
            }).Start();

            //ACT
            ping.Run();

            MemoryStream ms;
            var waitResult = ping.WaitForExit(8000, out ms);

            //ASSERT
            Assert.IsTrue(waitResult);
            Assert.IsNotNull(ms);
        }

        [TestMethod]
        public void TestWaitForExitProcess()
        {
            //INIT
            var logger = new Mocks.Logger();
            var config = new ProcessConfig
            {
                ExePath = "ping",
                Args = "localhost"
            };

            var ping = new ProcessRunner.ProcessRunnerImpl(config, logger);

            //ACT
            ping.Run();
            var exitedAfterOneSecond = ping.WaitForExit(1000);
            var exitedAfterFourSecond = ping.WaitForExit(4000);

            //ASSERT
            Assert.IsFalse(exitedAfterOneSecond);
            Assert.IsTrue(exitedAfterFourSecond);
        }

        [TestMethod]
        public void TestWaitForExitProcess2()
        {
            //INIT
            var logger = new Mocks.Logger();
            var config = new ProcessConfig
            {
                ExePath = "ping",
                Args = "localhost"
            };

            MemoryStream ms;
            var ping = new ProcessRunner.ProcessRunnerImpl(config, logger);

            //ACT
            var sw = new Stopwatch();
            sw.Start();
            ping.Run();
            var exited = ping.WaitForExit(10000, out ms);
            sw.Stop();

            var output = ms.GetString();

            //ASSERT
            Assert.IsTrue(exited);
            Assert.IsTrue(sw.ElapsedMilliseconds < 5000);
            Assert.IsTrue(sw.ElapsedMilliseconds > 3000);
            Assert.IsTrue(output.Contains("Approximate round trip times in milli-seconds:"));
        }
    }
}

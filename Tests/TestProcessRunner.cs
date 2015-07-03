using System;
using System.Diagnostics;
using System.Threading;
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
            var ping = new ProcessRunner.ProcessRunnerImpl("ping", "localhost", false, logger);

            //ACT
            ping.Run();
            Thread.Sleep(5000);
            ping.Exit();

            while (!ping.HasExited)
                Thread.Sleep(500);

            var output = ping.GetFromStandardOutput();

            //ASSERT
            Assert.IsFalse(string.IsNullOrEmpty(output));
            Assert.IsTrue(output.Contains("Approximate round trip times in milli-seconds:"));
        }

        [TestMethod]
        public void TestWaitForExitProcess()
        {
            //INIT
            var logger = new Mocks.Logger();
            var ping = new ProcessRunner.ProcessRunnerImpl("ping", "localhost", false, logger);

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
            var ping = new ProcessRunner.ProcessRunnerImpl("ping", "localhost", false, logger);

            //ACT
            var sw = new Stopwatch();
            sw.Start();
            ping.Run();
            var exited = ping.WaitForExit(10000);
            sw.Stop();

            var output = ping.GetFromStandardOutput();

            //ASSERT
            Assert.IsTrue(exited);
            Assert.IsTrue(sw.ElapsedMilliseconds < 5000);
            Assert.IsTrue(sw.ElapsedMilliseconds > 3000);
            Assert.IsTrue(output.Contains("Approximate round trip times in milli-seconds:"));
        }
    }
}

using System;
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
    }
}

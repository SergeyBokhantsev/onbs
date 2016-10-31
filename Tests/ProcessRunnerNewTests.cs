using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessRunner;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class ProcessRunnerNewTests
    {
        private string ReadOut(ProcessRunnerImplNew pr)
        {
            byte[] outData = null;
            if (pr.ReadStdOut(ms => outData = ms.ToArray()))
                return Encoding.UTF8.GetString(outData);
            else
                return null;
        }

        private async Task<string> ReadOutAsync(ProcessRunnerImplNew pr)
        {
            byte[] outData = null;
            if (await pr.ReadStdOutAsync(ms => Task.Run(() => outData = ms.ToArray())))
                return Encoding.UTF8.GetString(outData);
            else
                return null;
        }

        private string ReadError(ProcessRunnerImplNew pr)
        {
            byte[] outData = null;
            if (pr.ReadStdError(ms => outData = ms.ToArray()))
                return Encoding.UTF8.GetString(outData);
            else
                return null;
        }

        private async Task<string> ReadErrorAsync(ProcessRunnerImplNew pr)
        {
            byte[] outData = null;
            if (await pr.ReadStdErrorAsync(ms => Task.Run(() => outData = ms.ToArray())))
                return Encoding.UTF8.GetString(outData);
            else
                return null;
        }

        [TestMethod]
        public void ProcessRunnerNew_WaitForExit()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "google.com");

            //ACT
            pr.Run();

            var exited = pr.WaitForExit(5000);

            //ASSERT
            Assert.IsTrue(exited);

            var outText = ReadOut(pr).Trim();
            var outError = ReadError(pr);

            Assert.IsFalse(string.IsNullOrWhiteSpace(outText));

            Assert.IsTrue(outText.StartsWith("Pinging google.com"));
            Assert.IsTrue(outText.Contains("Approximate round trip times in milli-seconds:"));

            Assert.IsTrue(string.IsNullOrWhiteSpace(outError));
        }


        [TestMethod]
        public async Task ProcessRunnerNew_WaitForExit_Async()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "google.com");

            //ACT
            await pr.RunAsync();

            var exited = await pr.WaitForExitAsync(5000);

            //ASSERT
            Assert.IsTrue(exited);

            var outText = (await ReadOutAsync(pr)).Trim();
            var outError = await ReadErrorAsync(pr);

            Assert.IsFalse(string.IsNullOrWhiteSpace(outText));

            Assert.IsTrue(outText.StartsWith("Pinging google.com"));
            Assert.IsTrue(outText.Contains("Approximate round trip times in milli-seconds:"));

            Assert.IsTrue(string.IsNullOrWhiteSpace(outError));
        }

        [TestMethod]
        public void ProcessRunnerNew_ReadOut()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "google.com");

            //ACT
            pr.Run();

            var outText = ReadOut(pr).Trim();
            var outError = ReadError(pr);

            Assert.IsFalse(string.IsNullOrWhiteSpace(outText));

            Assert.IsTrue(outText.StartsWith("Pinging google.com"));
            Assert.IsTrue(outText.Contains("Approximate round trip times in milli-seconds:"));

            Assert.IsTrue(string.IsNullOrWhiteSpace(outError));
        }

        [TestMethod]
        public async Task ProcessRunnerNew_ReadOut_Async()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "google.com");

            //ACT
            await pr.RunAsync();

            var outText = (await ReadOutAsync(pr)).Trim();
            var outError = await ReadErrorAsync(pr);

            Assert.IsFalse(string.IsNullOrWhiteSpace(outText));

            Assert.IsTrue(outText.StartsWith("Pinging google.com"));
            Assert.IsTrue(outText.Contains("Approximate round trip times in milli-seconds:"));

            Assert.IsTrue(string.IsNullOrWhiteSpace(outError));
        }

        [TestMethod]
        public void ProcessRunnerNew_ReadAfterExited()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "google.com");

            //ACT
            pr.Run();

            Assert.IsFalse(pr.HasExited);

            Thread.Sleep(4500);

            Assert.IsTrue(pr.HasExited);

            var outText = ReadOut(pr).Trim();
            var outError = ReadError(pr);

            Assert.IsFalse(string.IsNullOrWhiteSpace(outText));

            Assert.IsTrue(outText.StartsWith("Pinging google.com"));
            Assert.IsTrue(outText.Contains("Approximate round trip times in milli-seconds:"));

            Assert.IsTrue(string.IsNullOrWhiteSpace(outError));
        }

        [TestMethod]
        public async Task ProcessRunnerNew_ReadAfterExited_Async()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "google.com");

            //ACT
            await pr.RunAsync();

            Assert.IsFalse(pr.HasExited);

            Thread.Sleep(4500);

            Assert.IsTrue(pr.HasExited);

            var outText = (await ReadOutAsync(pr)).Trim();
            var outError = await ReadErrorAsync(pr);

            Assert.IsFalse(string.IsNullOrWhiteSpace(outText));

            Assert.IsTrue(outText.StartsWith("Pinging google.com"));
            Assert.IsTrue(outText.Contains("Approximate round trip times in milli-seconds:"));

            Assert.IsTrue(string.IsNullOrWhiteSpace(outError));
        }

        [TestMethod]
        public void ProcessRunnerNew_StdOutEvent()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "google.com");

            var data = new List<byte>();

            //ACT
            pr.StdOut += (buf, ofs, count) => { if (count > 0) for (int i=0; i < count; ++i) data.Add(buf[i]); };

            pr.Run();

            Assert.IsFalse(pr.HasExited);

            Thread.Sleep(4500);

            Assert.IsTrue(pr.HasExited);

            var outText = ReadOut(pr);

            Assert.AreEqual(outText, Encoding.UTF8.GetString(data.ToArray()));
        }

        [TestMethod]
        public async Task ProcessRunnerNew_StdOutEvent_Async()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "google.com");

            var data = new List<byte>();

            //ACT
            pr.StdOut += (buf, ofs, count) => { if (count > 0) for (int i = 0; i < count; ++i) data.Add(buf[i]); };

            await pr.RunAsync();

            Assert.IsFalse(pr.HasExited);

            Thread.Sleep(4500);

            Assert.IsTrue(pr.HasExited);

            var outText = await ReadOutAsync(pr);

            Assert.AreEqual(outText, Encoding.UTF8.GetString(data.ToArray()));
        }

        [TestMethod]
        public void ProcessRunnerNew_Exit_And_StdOutEvent()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "-t google.com");

            var data = new List<byte>();

            bool exitedEventFired = false;
            bool exitUnexpected = false;

            //ACT
            pr.StdOut += (buf, ofs, count) => { if (count > 0) for (int i = 0; i < count; ++i) data.Add(buf[i]); };
            pr.Exited += unexpected => { exitedEventFired = true; exitUnexpected = unexpected; };

            pr.Run();

            ThreadPool.QueueUserWorkItem(s => { Thread.Sleep(5000); pr.Exit(); });

            Assert.IsTrue(pr.WaitForExit(5500));

            Thread.Sleep(1000); //Exit event fires slightly after actual process exits

            Assert.IsTrue(pr.HasExited);
            Assert.IsTrue(exitedEventFired);
            Assert.IsFalse(exitUnexpected);

            var outText = ReadOut(pr);

            Assert.AreEqual(outText, Encoding.UTF8.GetString(data.ToArray()));
        }

        [TestMethod]
        public async Task ProcessRunnerNew_Exit_And_StdOutEvent_Async()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "-t google.com");

            var data = new List<byte>();

            bool exitedEventFired = false;
            bool exitUnexpected = false;

            //ACT
            pr.StdOut += async (buf, ofs, count) => await Task.Run(() => { if (count > 0) for (int i = 0; i < count; ++i) data.Add(buf[i]); });
            pr.Exited += async unexpected => await Task.Run(() => { exitedEventFired = true; exitUnexpected = unexpected; });

            await pr.RunAsync();

            ThreadPool.QueueUserWorkItem(s => { Thread.Sleep(5000); pr.Exit(); });

            Assert.IsTrue(await pr.WaitForExitAsync(5500));

            Thread.Sleep(1000); //Exit event fires slightly after actual process exits

            Assert.IsTrue(pr.HasExited);
            Assert.IsTrue(exitedEventFired);
            Assert.IsFalse(exitUnexpected);

            var outText = await ReadOutAsync(pr);

            Assert.AreEqual(outText, Encoding.UTF8.GetString(data.ToArray()));
        }

        [TestMethod]
        public void ProcessRunnerNew_ExitedEventDeadlock()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "-t google.com");

            bool readed = false;

            //ACT
            pr.Exited += unexpected => pr.ReadStdOut(ms => { readed = true; });

            pr.Run();

            Thread.Sleep(1000);

            pr.Exit();

            Thread.Sleep(1000);

            while (!readed)
                Thread.Sleep(1000);
        }

        [TestMethod]
        public async Task ProcessRunnerNew_ExitedEventDeadlock_Async()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForTool("ping", "-t google.com");

            bool readed = false;

            //ACT
            pr.Exited += async unexpected => await Task.Run(() => pr.ReadStdOut(ms => { readed = true; }));

            await pr.RunAsync();

            Thread.Sleep(1000);

            pr.Exit();

            Thread.Sleep(1000);

            while (!readed)
                Thread.Sleep(1000);
        }

        [TestMethod]
        public void ProcessRunnerNew_Interactive()
        {
            //INIT
            var pr = ProcessRunnerImplNew.ForInteractiveApp("notepad.exe", null);

            //ACT
            pr.Run();

            Assert.IsFalse(pr.HasExited);

            Thread.Sleep(1000);

            pr.Exit();

            Assert.IsTrue(pr.WaitForExit(1000));

            Thread.Sleep(500);

            //ASSERT
            Assert.IsTrue(pr.HasExited);
        }

        [TestMethod]
        public void LoopbackLong()
        {
            //INIT
            var pr1 = ProcessRunnerImplNew.ForTool("StdInOutTester", null);
            pr1.RedirectStandardInpit = true;

            var data = new byte[1024 * 10];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = (byte)i;
            }

            //ACT
            int received = 0;

            pr1.StdOut += (byte[] buffer, int offset, int count) => received += count;
            
            pr1.Run();
            
            for (int i = 0; i < data.Length; ++i)
            {
                if (!pr1.SendToStandardInput(data[i]))
                    i--;
            }

            while (received != data.Length)
                Thread.Sleep(10);

            pr1.Exit();
            pr1.WaitForExit(1000);

            byte[] receivedData = null;
        
            pr1.ReadStdOut(ms => receivedData = ms.ToArray());

            Assert.IsTrue(data.SequenceEqual(receivedData));

            Assert.IsTrue(pr1.HasExited);
        }

        [TestMethod]
        public void CrossLoopback()
        {
            var pr1 = ProcessRunnerImplNew.ForTool("StdInOutTester", null);
            pr1.RedirectStandardInpit = true;

            var pr2 = ProcessRunnerImplNew.ForTool("StdInOutTester", null);
            pr2.RedirectStandardInpit = true;

            pr1.Run();
            pr2.Run();

            pr1.SendToStandardInput((byte)'A');
            pr2.SendToStandardInput((byte)'1');

            pr1.SendToStandardInput((byte)'B');
            pr2.SendToStandardInput((byte)'2');

            pr1.SendToStandardInput((byte)'C');
            pr2.SendToStandardInput((byte)'3');

            Thread.Sleep(1000);

            pr1.Exit();
            pr1.WaitForExit(1000);

            pr2.Exit();
            pr2.WaitForExit(1000);

            string str1 = null;
            string str2 = null;

            pr1.ReadStdOut(ms => str1 = Encoding.UTF8.GetString(ms.ToArray()));
            pr2.ReadStdOut(ms => str2 = Encoding.UTF8.GetString(ms.ToArray()));

            Assert.AreEqual("ABC", str1);
            Assert.AreEqual("123", str2);

            Assert.IsTrue(pr1.HasExited);
            Assert.IsTrue(pr2.HasExited);
        }
    }
}

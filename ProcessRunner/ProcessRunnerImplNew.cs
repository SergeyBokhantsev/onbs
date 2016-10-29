using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace ProcessRunner
{
    public class ProcessRunnerImplNew
    {
        private class Boxed<T>
        {
            private T value;

            public Boxed(T value)
            {
                this.value = value;
            }

            public static implicit operator T(Boxed<T> o)
            {
                return o.value;
            }

            public void Set(T value)
            {
                this.value = value;
            }
        }

        public static ProcessRunnerImplNew ForTool(string exeName, string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exeName,
                WorkingDirectory = Path.GetDirectoryName(exeName),
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            return new ProcessRunnerImplNew(psi, true, true) { SendCloseWindowSignalWhenExit = false };
        }

        public static ProcessRunnerImplNew ForInteractiveApp(string exeName, string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exeName,
                WorkingDirectory = Path.GetDirectoryName(exeName),
                Arguments = args,
                UseShellExecute = false
            };

            return new ProcessRunnerImplNew(psi, false, false) { SendCloseWindowSignalWhenExit = true };
        }

        public event ExitedEventHandler Exited;

        /// <summary>
        /// Handling this event with an async handler could lead to a corrupt content
        /// </summary>
        public event IncomingDataEventHandler StdOut;

        /// <summary>
        /// Handling this event with an async handler could lead to a corrupt content
        /// </summary>
        public event IncomingDataEventHandler StdError;

        private readonly ProcessStartInfo psi;

        private Process proc;

        private MemoryStream stdOut;
        private MemoryStream stdError;

        private bool runCalled;
        private bool exitCalled;

        private readonly ManualResetEvent outReadingCompleted = new ManualResetEvent(false);

        private Boxed<bool> exitedEventFired = new Boxed<bool>(false);

        public bool SendCloseWindowSignalWhenExit { get; set; }

        /// <summary>
        /// Timeout in ms after CloseMainWindow signal sent and before Kill signal
        /// </summary>
        public int TimeoutBeforeKill { get; set; }

        public bool HasExited { get { return proc.HasExited; } }

        public ProcessRunnerImplNew(ProcessStartInfo psi, bool collectOutput, bool collectError)
        {
            if (null == psi)
                throw new ArgumentNullException("psi");

            this.psi = psi;

            SendCloseWindowSignalWhenExit = true;
            TimeoutBeforeKill = 5000;

            if (collectOutput)
            {
                if (psi.RedirectStandardOutput)
                    this.StdOut += CollectStdOut;
                else
                    throw new InvalidOperationException("RedirectStandardOutput must be set to collectOutput");
            }

            if (collectError)
            {
                if (psi.RedirectStandardError)
                    this.StdError += CollectStdError;
                else
                    throw new InvalidOperationException("RedirectStandardError must be set to collectError");
            }
        }

        void CollectStdError(byte[] buffer, int offset, int count)
        {
 	        if (null == stdError)
                stdError = new MemoryStream();

            stdError.Write(buffer, offset, count);
        }

        void CollectStdOut(byte[] buffer, int offset, int count)
        {
 	        if (null == stdOut)
                stdOut = new MemoryStream();

            stdOut.Write(buffer, offset, count);
        }

        public void Run()
        {
            if (runCalled)
                throw new InvalidOperationException("Process runner already ran");

            runCalled = true;

            proc = Process.Start(psi);
            proc.EnableRaisingEvents = false;

            (new Thread(Monitor) { IsBackground = true, Name = "PR out reading" }).Start();
        }

        public async Task RunAsync()
        {
            if (runCalled)
                throw new InvalidOperationException("Process runner already ran");

            await Task.Run(() => Run());
        }

        private void Monitor()
        {
            bool ro = psi.RedirectStandardOutput;
            bool re = psi.RedirectStandardError;

            if (!ro && !re)
                outReadingCompleted.Set();

            byte[] roBuffer = new byte[512];
            byte[] reBuffer = new byte[512];

            Task<int> roTask = null;
            Task<int> reTask = null;

            int cycleReaded = -1;

            while (!(proc.HasExited && (cycleReaded == 0 || (!ro && !re))))
            {
                cycleReaded = -1;

                if (ro && roTask == null)
                    roTask = proc.StandardOutput.BaseStream.ReadAsync(roBuffer, 0, roBuffer.Length);

                if (re && reTask == null)
                    reTask = proc.StandardError.BaseStream.ReadAsync(reBuffer, 0, reBuffer.Length);

                if (ro && roTask.IsCompleted)
                {
                    cycleReaded = OnOutTaskCompleted(roTask, roBuffer, StdOut);
                    roTask = null;
                }

                if (re && reTask.IsCompleted)
                {
                    cycleReaded += OnOutTaskCompleted(reTask, reBuffer, StdError);
                    reTask = null;
                }

                if (cycleReaded == -1)
                    Thread.Sleep(100);
            }

            outReadingCompleted.Set();

            OnExited();
        }

        private int OnOutTaskCompleted(Task<int> task, byte[] buffer, IncomingDataEventHandler handler)
        {
            try
            {
                var count = task.Result;

                if (count > 0 && null != handler)
                    handler(buffer, 0, count);

                return count;
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException ?? ex;
            }
        }

        private void OnExited()
        {
            ExitedEventHandler handler = null;

            lock(exitedEventFired)
            {
                if (!exitedEventFired)
                {
                    handler = Exited;
                    exitedEventFired.Set(true);
                }
            }

            if (null != handler)
                handler(!exitCalled);
        }

        public void SendToStandardInput(string message)
        {
            throw new NotImplementedException();
        }

        public void SendToStandardInput(char c)
        {
            throw new NotImplementedException();
        }

        public bool WaitForExit(int timeoutMilliseconds)
        {
            if (proc == null)
                throw new InvalidOperationException("Process is null");

            return proc.WaitForExit(timeoutMilliseconds);
        }

        public async Task<bool> WaitForExitAsync(int timeoutMilliseconds)
        {
            if (proc == null)
                throw new InvalidOperationException("Process is null");

            return await Task.Run(() => proc.WaitForExit(timeoutMilliseconds));
        }

        public void Exit()
        {
            exitCalled = true;

            ThreadPool.QueueUserWorkItem(state =>
            {
                if (proc != null && !proc.HasExited)
                {
                    if (SendCloseWindowSignalWhenExit && TimeoutBeforeKill > 0)
                    {
                        proc.CloseMainWindow();
                        proc.WaitForExit(TimeoutBeforeKill);
                    }

                    if (!proc.HasExited)
                        proc.Kill();
                }
            });
        }

        public bool ReadStdOut(Action<MemoryStream> accessor)
        {
            outReadingCompleted.WaitOne();

            if (null != stdOut && null != accessor)
            {
                accessor(stdOut);

                return true;
            }

            return false;
        }

        public async Task<bool> ReadStdOutAsync(Func<MemoryStream, Task> accessorTask)
        {
            await Task.Run(() => outReadingCompleted.WaitOne());

            if (null != stdOut && null != accessorTask)
            {
                await accessorTask(stdOut);

                return true;
            }

            return false;
        }

        public bool ReadStdError(Action<MemoryStream> accessor)
        {
            outReadingCompleted.WaitOne();

            if (null != stdError && null != accessor)
            {
                accessor(stdError);

                return true;
            }

            return false;
        }

        public async Task<bool> ReadStdErrorAsync(Func<MemoryStream, Task> accessorTask)
        {
            await Task.Run(() => outReadingCompleted.WaitOne());

            if (null != stdError && null != accessorTask)
            {
                await accessorTask(stdError);

                return true;
            }

            return false;
        }
    }
}

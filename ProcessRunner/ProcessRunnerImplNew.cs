using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace ProcessRunnerNamespace
{
    public class ProcessRunner : IDisposable
    {
        private class LineBuffer
        {
            private const int size = 1024;

            private readonly byte[] buffer = new byte[size];

            private readonly byte[] clientBuffer = new byte[size];

            private int len;

            public int Length
            {
                get { return len; }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public bool Add(byte b)
            {
                if (len < size)
                {
                    buffer[len++] = b;
                    return true;
                }
                else
                    return false;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void Get(out byte[] data, out int count)
            {
                data = clientBuffer;
                count = len;

                if (count > 0)
                {
                    Array.Copy(buffer, 0, data, 0, count);
                    len = 0;
                }
            }
        }

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

        public static ProcessRunner ForTool(string exeName, string args)
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

            return new ProcessRunner(psi, true, true) { SendCloseWindowSignalWhenExit = false };
        }

		public static ProcessRunner ForInteractiveApp(string exeName, string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exeName,
                WorkingDirectory = Path.GetDirectoryName(exeName),
                Arguments = args,
                UseShellExecute = false
            };

            return new ProcessRunner(psi, false, false) { SendCloseWindowSignalWhenExit = true };
        }

        public static T ExecuteTool<T>(string description, Func<string, T> action, int timeout, string exe, string args = null)
        {
            ProcessRunner pr = null;

            try
            {
                pr = ProcessRunner.ForTool(exe, args);

                pr.Run();

                if (pr.WaitForExit(timeout))
                {
                    string output = null;

                    if (pr.ReadStdOut(ms => output = ms.GetString()))
                    {
                        return action(output);
                    }
                    else if (pr.ReadStdError(ms => output = ms.GetString()))
                    {
                        throw new Exception(output);
                    }
                    else
                        throw new Exception("Problem accessing out data");
                }
                else
                    throw new Exception(string.Format("Timeout ({0} ms) for process", timeout));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception in {0}: {1}", description, ex.Message), ex);
            }
            finally
            {
                TryExitEndDispose(pr);
            }
        }

        public static async Task<T> ExecuteToolAsync<T>(string description, Func<string, T> action, int timeout, string exe, string args = null)
        {
            ProcessRunner pr = null;

            try
            {
                pr = ProcessRunner.ForTool(exe, args);

                await pr.RunAsync();

                if (await pr.WaitForExitAsync(timeout))
                {
                    string output = null;

					if (await pr.ReadStdOutAsync(ms => Task.Run(() => output = ms.GetString())))
                    {
                        return action(output);
                    }
                    else if (await pr.ReadStdErrorAsync(ms => Task.Run(() => output = ms.GetString())))
                    {
                        throw new Exception(output);
                    }
                    else
                        throw new Exception("Problem accessing out data");
                }
                else
                    throw new Exception(string.Format("Timeout ({0} ms) for process", timeout));
            }
            catch (AggregateException ex)
            {
                var exception = ex.Flatten().InnerException ?? ex;

                throw new Exception(string.Format("Exception in {0}: {1}", description, exception.Message), exception);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception in {0}: {1}", description, ex.Message), ex);
            }
            finally
            {
                TryExitEndDispose(pr);
            }
        }

        public static T ExecuteTool<T>(string description, Func<MemoryStream, T> action, int timeout, string exe, string args = null)
        {
            ProcessRunner pr = null;

            try
            {
                pr = ProcessRunner.ForTool(exe, args);

                pr.Run();

                if (pr.WaitForExit(timeout))
                {
                    T result = default(T);
                    string error = null;

                    if (pr.ReadStdOut(ms => result = action(ms)))
                    {
                        return result;
                    }
                    else if (pr.ReadStdError(ms => error = ms.GetString()))
                    {
                        throw new Exception(error);
                    }
                    else
                        throw new Exception("Problem accessing out data");
                }
                else
                    throw new Exception(string.Format("Timeout ({0} ms) for process", timeout));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception in {0}: {1}", description, ex.Message), ex);
            }
            finally
            {
                TryExitEndDispose(pr);
            }
        }

        public static void TryExitEndDispose(ProcessRunner pr)
        {
            if (null != pr)
            {
                try
                {
                    if (!pr.HasExited)
                        pr.Exit();
                }
                catch { }

                pr.Dispose();
            }
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
        private readonly LineBuffer stdInBuffer = new LineBuffer();

        private bool runCalled;
        private bool exitCalled;

        private bool disposed;

        private ManualResetEvent outReadingCompleted = new ManualResetEvent(false);

        private Boxed<bool> exitedEventFired = new Boxed<bool>(false);

        public bool SendCloseWindowSignalWhenExit { get; set; }

        public bool RedirectStandardInpit 
        {
            set
            {
                if (!runCalled)
                {
                    psi.RedirectStandardInput = value;
                }
                else
                    throw new InvalidOperationException("Run is already called!");
            }
        }

        /// <summary>
        /// Timeout in ms after CloseMainWindow signal sent and before Kill signal
        /// </summary>
        public int TimeoutBeforeKill { get; set; }

        public bool HasExited { get { return proc != null && proc.HasExited; } }

        public ProcessRunner(ProcessStartInfo psi, bool collectOutput, bool collectError)
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

        ~ProcessRunner()
        {
            Dispose(false);
        }

        private void CollectStdError(byte[] buffer, int offset, int count)
        {
            if (disposed)
                return;

 	        if (null == stdError)
                stdError = new MemoryStream();

            stdError.Write(buffer, offset, count);
        }

        private void CollectStdOut(byte[] buffer, int offset, int count)
        {
            if (disposed)
                return;

 	        if (null == stdOut)
                stdOut = new MemoryStream();

            stdOut.Write(buffer, offset, count);
        }

        public void Run()
        {
            if (disposed)
                throw new ObjectDisposedException("ProcessRunner");

            if (runCalled)
                throw new InvalidOperationException("Process runner already ran");

            runCalled = true;

            proc = Process.Start(psi);
            proc.EnableRaisingEvents = false;

            (new Thread(Monitor) { IsBackground = true, Name = "PR out reading" }).Start();
        }

        public async Task RunAsync()
        {
            if (disposed)
                throw new ObjectDisposedException("ProcessRunner");

            if (runCalled)
                throw new InvalidOperationException("Process runner already ran");

            await Task.Run(() => Run());
        }

        private void TrySetOutReadingCompleted()
        {
            var mre = outReadingCompleted;

            if (null != mre)
            {
                lock (mre)
                {
                    if (!disposed)
                    {
                        mre.Set();
                    }
                }
            }
        }

        private void Monitor()
        {
            bool ro = psi.RedirectStandardOutput;
            bool re = psi.RedirectStandardError;
            bool ri = psi.RedirectStandardInput;

			if (!ro && !re)
                TrySetOutReadingCompleted();

            byte[] roBuffer = new byte[512];
            byte[] reBuffer = new byte[512];

            Task<int> roTask = null;
            Task<int> reTask = null;
            Task riTask = null;

            var cts = new CancellationTokenSource();

            int cycleReaded = -1;

            while (!(proc.HasExited && (cycleReaded == 0 || (!ro && !re))))
            {
                cycleReaded = -1;

                if (exitCalled)
                    cts.Cancel();

                if (ro && roTask == null)
                    roTask = proc.StandardOutput.BaseStream.ReadAsync(roBuffer, 0, roBuffer.Length);

                if (re && reTask == null)
                    reTask = proc.StandardError.BaseStream.ReadAsync(reBuffer, 0, reBuffer.Length);

                if (null != riTask && riTask.IsCompleted)
                    riTask = null;

                if (ri && riTask == null && !exitCalled && stdInBuffer.Length > 0)
                    riTask = WriteToStdInput(proc.StandardInput.BaseStream, cts.Token);

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

				if (cycleReaded == -1 && null == riTask)
					Thread.Sleep (300);
				else
					Thread.Sleep (1);
            }

            cts.Cancel();

            TrySetOutReadingCompleted();

            OnExited();
        }

        private async Task WriteToStdInput(Stream stream, CancellationToken ct) 
        {
            byte[] data;
            int count;
            stdInBuffer.Get(out data, out count);

            if (count > 0)
            {
                await stream.WriteAsync(data, 0, count, ct);
                await stream.FlushAsync();
            }
        }

        private int OnOutTaskCompleted(Task<int> task, byte[] buffer, IncomingDataEventHandler handler)
        {
            try
            {
                var count = task.Result;

                if (null != handler)
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

        public bool SendToStandardInput(byte b)
        {
            if (disposed)
                throw new ObjectDisposedException("ProcessRunner");

            return stdInBuffer.Add(b);
        }

        public bool WaitForExit(int timeoutMilliseconds)
        {
            if (disposed)
                throw new ObjectDisposedException("ProcessRunner");

            if (proc == null)
                throw new InvalidOperationException("Process is null");

            return proc.WaitForExit(timeoutMilliseconds);
        }

        public async Task<bool> WaitForExitAsync(int timeoutMilliseconds)
        {
            if (disposed)
                throw new ObjectDisposedException("ProcessRunner");

            if (proc == null)
                throw new InvalidOperationException("Process is null");

            return await Task.Run(() => proc.WaitForExit(timeoutMilliseconds));
        }

        public void Exit()
        {
            if (disposed)
                throw new ObjectDisposedException("ProcessRunner");

            exitCalled = true;

			try
			{
            if (proc != null && !proc.HasExited)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
						try
						{
                    if (proc != null && !proc.HasExited)
                    {
                        if (SendCloseWindowSignalWhenExit && TimeoutBeforeKill > 0)
                        {
                            proc.CloseMainWindow();
                            proc.WaitForExit(TimeoutBeforeKill);
                        }
							}
							}
						catch (Exception ex)
						{
							//throw ex;
						}

						try
						{
                        if (!proc.HasExited)
                            proc.Kill();
						}
						catch (Exception ex)
						{
							//throw ex;
						}
                    }
						
                );
            }
			}
			catch (Exception ex)
			{
				//var e = ex;

			}
        }

        public bool ReadStdOut(Action<MemoryStream> accessor)
        {
            if (disposed)
                throw new ObjectDisposedException("ProcessRunner");

            outReadingCompleted.WaitOne();

            if (null != stdOut && null != accessor)
            {
				stdOut.Seek (0, SeekOrigin.Begin);

                accessor(stdOut);

                return true;
            }

            return false;
        }

        public async Task<bool> ReadStdOutAsync(Func<MemoryStream, Task> accessorTask)
        {
            if (disposed)
                throw new ObjectDisposedException("ProcessRunner");

            await Task.Run(() => outReadingCompleted.WaitOne());

            if (null != stdOut && null != accessorTask)
            {
				stdOut.Seek (0, SeekOrigin.Begin);

                await accessorTask(stdOut);

                return true;
            }

            return false;
        }

        public bool ReadStdError(Action<MemoryStream> accessor)
        {
            if (disposed)
                throw new ObjectDisposedException("ProcessRunner");

            outReadingCompleted.WaitOne();

            if (null != stdError && null != accessor)
            {
				stdError.Seek (0, SeekOrigin.Begin);

                accessor(stdError);

                return true;
            }

            return false;
        }

        public async Task<bool> ReadStdErrorAsync(Func<MemoryStream, Task> accessorTask)
        {
            if (disposed)
                throw new ObjectDisposedException("ProcessRunner");

            await Task.Run(() => outReadingCompleted.WaitOne());

            if (null != stdError && null != accessorTask)
            {
				stdError.Seek (0, SeekOrigin.Begin);

                await accessorTask(stdError);

                return true;
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
				disposed = true;

                var mre = outReadingCompleted;

                if (null != mre)
                {
                    lock (mre)
                    {
                        mre.Set();
                        mre.Dispose();
                        outReadingCompleted = null;
                    }
                }

                if (disposing)
                {
                    if (null != stdOut)
                    {
                        stdOut.Dispose();
                        stdOut = null;
                    }

                    if (null != stdError)
                    {
                        stdError.Dispose();
                        stdError = null;
                    }
                }
            }
        }
    }
}

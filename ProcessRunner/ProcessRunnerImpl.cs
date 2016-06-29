using Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessRunner
{
    public class ProcessRunnerImpl : IProcessRunner
    {
        public event ExitedEventHandler Exited;

        private Process proc;
        private readonly ProcessConfig config;
        private readonly ILogger logger;

        private bool closing;

        public bool HasExited
        {
            get
            {
                if (proc == null)
                    return true;
                else
                    return proc.HasExited;
            }
        }

        public ProcessRunnerImpl(ProcessConfig config, ILogger logger)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (string.IsNullOrEmpty(config.ExePath))
                throw new Exception("app exe Path");

            if (logger == null)
                throw new Exception("logger");

            this.config = config;
            this.logger = logger;

            logger.LogIfDebug(this, string.Concat("Process runner created for {0}", ToString()));
        }

        public override string ToString()
        {
            return string.Concat("{", config.ExePath, " ", config.Args, "}");
        }

        public void Run()
        {
            if (proc != null)
                throw new InvalidOperationException("Process was already run");

            try
            {
                if (!config.Silent)
                    logger.Log(this, string.Format("Launching {0}", ToString()), LogLevels.Info);

                var psi = new ProcessStartInfo(config.ExePath)
                {
                    Arguments = config.Args,
                    UseShellExecute = false,
                    // ReSharper disable once AssignNullToNotNullAttribute
                    WorkingDirectory = string.IsNullOrWhiteSpace(config.ExePath) ? string.Empty : Path.GetDirectoryName(config.ExePath),
                    RedirectStandardInput = config.RedirectStandardInput,
                    RedirectStandardOutput = config.RedirectStandardOutput
                };

                proc = Process.Start(psi);

                if (proc == null)
                    throw new Exception("Created process is null unexpectedly");

                logger.LogIfDebug(this, string.Format("Launched {0}", ToString()));

                if (config.WaitForUI)
                {
                    logger.LogIfDebug(this, "Waiting for UI becomes initialized...");
                    proc.WaitForInputIdle(10000);
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                throw new Exception(string.Format("Unable to launch '{0}': {1}", ToString(), ex.Message), ex);
            }
            
            //var monitor = new Thread(Monitor);
			ThreadPool.QueueUserWorkItem(Monitor);
        }

        private void Monitor(object o)
        {
            try
            {
                if (proc == null)
                    return;

                logger.LogIfDebug(this, string.Format("Launching monitor loop for {0}", ToString()));

                while (!proc.HasExited)
                {
                    Thread.Sleep(config.AliveMonitoringInterval);
                }

                if (!config.Silent)
                    logger.Log(this, string.Format("{0} has exited", ToString()), LogLevels.Info);
            }
            catch (Exception ex)
            {
				logger.Log(this, "Exception while monitoring", LogLevels.Warning);
                logger.Log(this, ex);

				if (null != proc && !proc.HasExited)
				{
					logger.Log(this, "Application is not closed after exception, killing...", LogLevels.Warning);
					proc.Kill();
				}
            }

			OnExited ();
        }

        public void Exit()
        {
            closing = true;

            logger.LogIfDebug(this, string.Format("Begin closing {0}", ToString()));

            if (proc != null && !proc.HasExited)
            {
                try
                {
                    logger.LogIfDebug(this, "Trying to close main window...");

                    proc.CloseMainWindow();

                    proc.WaitForExit(5000);
                }
                catch (Exception ex)
                {
                    logger.Log(this, ex);
                }

                if (!proc.HasExited)
                {
                    logger.LogIfDebug(this, "Application was not closed, killing...");
                    proc.Kill();
                }
            }

            OnExited();
        }

        private void OnExited()
        {
            var handler = Exited;
            if (handler != null)
                handler(!closing);
        }

        public void SendToStandardInput(string message)
        {
            if (!proc.HasExited)
            {
                proc.StandardInput.WriteLine(message);
                proc.StandardInput.Flush();
            }
        }

		public void SendToStandardInput(char c)
		{
			if (!proc.HasExited)
			{
				proc.StandardInput.Write(c);
				proc.StandardInput.Flush();
			}
		}

        public bool WaitForExit(int timeoutMs, out MemoryStream output)
        {
            if (proc == null)
                throw new InvalidOperationException("Process was not run");

            if (!config.RedirectStandardOutput)
                throw new InvalidOperationException("Set RedirectStandardOutput=true on order to use this method");

            output = new MemoryStream();

			int bufferToReadFromLen = 0;

            var bufferToReadFrom = new byte[1024 * 16];
			var bufferToWriteTo = new byte[1024 * 16];

			var bs =  proc.StandardOutput.BaseStream;

            if (bs == null)
                throw new NullReferenceException(string.Format("{0} : Proces.StandardOutput.BaseStream is null", ToString()));

            const int sleepDelay = 100;
            int waited = 0;

            do
            {
                var readTask = bs.ReadAsync(bufferToWriteTo, 0, bufferToWriteTo.Length);
                var writeTask = output.WriteAsync(bufferToReadFrom, 0, bufferToReadFromLen);

                while (!(writeTask.IsCompleted && readTask.IsCompleted))
                {
                    Thread.Sleep(sleepDelay);
                    waited += sleepDelay;

                    if (waited > timeoutMs)
                    {
                        logger.Log(this, string.Format("Timeout occured for ProcessRunner {0}. Output stream reading will be interrupted, thus target process can be locked", ToString()), LogLevels.Warning);
                        return false;
                    }
                }

				SwitchBuffers(ref bufferToWriteTo, ref bufferToReadFrom);
				bufferToReadFromLen = readTask.Result;
            }
			while (bufferToReadFromLen > 0);
            
			output.Seek (0, SeekOrigin.Begin);

            return true;
        }

		private void SwitchBuffers(ref byte[] a, ref byte[] b)
		{
			byte[] temp = a;
			a = b;
			b = temp;
		}

        public bool WaitForExit(int timeoutMilliseconds)
        {
            if (proc == null)
                throw new InvalidOperationException("Process was not run");

            return proc.WaitForExit(timeoutMilliseconds);
        }
    }
}

using Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public string Name
        {
            get
            {
                return string.Format("{0} {1}", Path.GetFileName(config.ExePath), config.Args);
            }
        }

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

            logger.LogIfDebug(this, string.Concat("Process runner created for {0}", Name));
        }

        public void Run()
        {
            if (proc != null)
                throw new InvalidOperationException();

            try
            {
                if (!config.Silent)
                    logger.Log(this, string.Format("Launching {0}", Name), LogLevels.Info);

                var psi = new ProcessStartInfo(config.ExePath);
                psi.Arguments = config.Args;
                psi.UseShellExecute = false;
                psi.WorkingDirectory = Path.GetDirectoryName(config.ExePath);
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;

                proc = Process.Start(psi);

                logger.LogIfDebug(this, string.Format("Launched {0}", Name));

                if (config.WaitForUI)
                {
                    logger.LogIfDebug(this, "Waiting for UI becomes initialized...");
                    proc.WaitForInputIdle(10000);
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                throw new Exception(string.Format("Unable to launch '{0}': {1}", Name, ex.Message), ex);
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

                logger.LogIfDebug(this, string.Format("Launching monitor loop for {0}", Name));

                while (!proc.HasExited)
                {
                    Thread.Sleep(1000);
                }

                if (!config.Silent)
                    logger.Log(this, string.Format("{0} has exited", Name), LogLevels.Info);
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }

            OnExited();
        }

        public void Exit()
        {
            closing = true;

            logger.LogIfDebug(this, string.Format("Begin closing {0}", Name));

            if (proc != null && !proc.HasExited)
            {
                try
                {
                    logger.LogIfDebug(this, "Trying to close main window...");

                    var handle = proc.MainWindowHandle;
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

        public string GetFromStandardOutput()
        {
            StringBuilder res = new StringBuilder();
            var buffer = new char[1024];
            int readed = 0;

            do
            {
                readed = proc.StandardOutput.Read(buffer, 0, buffer.Length);
                res.Append(buffer, 0, readed);
            }
            while (readed == buffer.Length);

            return res.ToString();
        }

        public bool WaitForExit(int timeoutMilliseconds)
        {
            const int checkSpanMs = 500;
            int waitingMs = 0;

            while (waitingMs < timeoutMilliseconds)
            {
                if (proc == null || proc.HasExited)
                    return true;

                Thread.Sleep(checkSpanMs);
                waitingMs += checkSpanMs;
            }

            return false;
        }
    }
}

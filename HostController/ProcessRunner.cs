﻿using Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostController
{
    public class ProcessRunner : IProcessRunner
    {
        public event ExitedEventHandler Exited;

        private Process proc;
        private readonly string appPath;
        private readonly string arguments;
        private readonly bool useShellExecution;
        private readonly bool waitForUI;
        private readonly ILogger logger;

        private bool closing;

        public string Name
        {
            get;
            private set;
        }

        public ProcessRunner(string appPath, string arguments, bool useShellExecution, bool waitForUI, ILogger logger)
        {
            if (string.IsNullOrEmpty(appPath))
                throw new Exception("appPath");

            if (logger == null)
                throw new Exception("logger");

            this.appPath = appPath;
            this.arguments = arguments;
            this.useShellExecution = useShellExecution;
            this.waitForUI = waitForUI;
            this.logger = logger;

            Name = Path.GetFileName(appPath);

            logger.LogIfDebug(this, string.Format("Process runner created for {0} {1}", appPath, arguments));
        }

        public void Run()
        {
            if (proc != null)
                throw new InvalidOperationException();

            try
            {

                logger.Log(this, string.Format("Launching {0}", appPath), LogLevels.Info);

                var psi = new ProcessStartInfo(appPath);
                psi.Arguments = arguments;
                psi.UseShellExecute = useShellExecution;
                psi.WorkingDirectory = Path.GetDirectoryName(appPath);

                proc = Process.Start(psi);

                logger.LogIfDebug(this, string.Format("Launched {0}", appPath));

                if (waitForUI)
                {
                    logger.LogIfDebug(this, "Waiting for UI becomes initialized...");
                    proc.WaitForInputIdle(10000);
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                throw new Exception(string.Format("Unable to launch '{0}': {1}", appPath, ex.Message), ex);
            }

            new Thread(() => Monitor()).Start();
        }

        private void Monitor()
        {
            logger.LogIfDebug(this, string.Format("Launching monitor loop for {0}", appPath));

            while (!proc.HasExited)
            {
                Thread.Sleep(1000);
            }

            logger.Log(this, string.Format("{0} has exited", appPath), LogLevels.Info);

            OnExited();
        }

        public void Exit()
        {
            closing = true;

            logger.LogIfDebug(this, string.Format("Begin closing {0}", appPath));

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

    }
}

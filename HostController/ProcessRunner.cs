using Interfaces;
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

        private bool closing;

        public string Name
        {
            get;
            private set;
        }

        public ProcessRunner(string appPath, string arguments)
        {
            this.appPath = appPath;
            this.arguments = arguments;
            Name = Path.GetFileName(appPath);
        }

        public void Run()
        {
            if (proc != null)
                throw new InvalidOperationException();

            proc = Process.Start(appPath, arguments);
            proc.WaitForInputIdle(10000);

            new Thread(() => Monitor()).Start();
        }

        private void Monitor()
        {
            while (!proc.HasExited)
            {
                Thread.Sleep(1000);
            }

            OnExited();
        }

        public void Exit()
        {
            closing = true;

            if (!proc.HasExited)
            {
                try
                {
                    var handle = proc.MainWindowHandle;
                    proc.CloseMainWindow();

                    proc.WaitForExit(5000);
                }
                catch (Exception ex)
                {

                }

                if (!proc.HasExited)
                    proc.Kill();

                OnExited();
            }
        }

        private void OnExited()
        {
            var handler = Exited;
            if (handler != null)
                handler(!closing);
        }

    }
}

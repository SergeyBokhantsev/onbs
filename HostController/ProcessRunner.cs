using Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostController
{
    public class ProcessRunner : IProcessRunner
    {
        private Process proc;
        private readonly string appPath;
        private readonly string arguments;

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
        }

        public void Exit()
        {
            proc.CloseMainWindow();
        }
    }
}

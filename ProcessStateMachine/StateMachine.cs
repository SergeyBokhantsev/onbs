using ProcessRunnerNamespace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessStateMachine
{
    public abstract class StateMachine : IDisposable
    {
        private ProcessRunner process;

        private StateDescriptor root;
        private StateDescriptor current;

        private byte[] incoming = new byte[1024];
        private int incomingLen;

        public bool Active
        {
            get
            {
                return null != process;
            }
        }

        public StateMachine (StateDescriptor root)
        {
            this.root = this.current = root;
        }

        public void Start()
        {
            lock (root)
            {
                if (null != process)
                    throw new InvalidOperationException("This state machine already rinning");

                current = root;

                RunProcess();
            }
        }

        public void Stop()
        {
            var pr = process;

            if (pr != null)
                pr.Exit();
        }

        private void RunProcess()
        {            
            string exeName, args;
            GetProcessRunnerArguments(out exeName, out args);

            var psi = new ProcessStartInfo
            {
                 FileName = exeName,
                 Arguments = args,
                 UseShellExecute = false,
                 RedirectStandardError = true,
                 RedirectStandardInput = true,
                 RedirectStandardOutput = true,                  
            };

            process = new ProcessRunner(psi, false, false);

            process.Exited += unexpected =>
            {
                process = null;
                OnProcessExited();
            };

            process.StdOut += ProcessStdOut;
            process.StdError += ProcessStdError;

            process.Run();
        }

        private void ProcessStdOut(byte[] buffer, int offset, int count)
        {
            try
            {
                if (count > 0)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        byte b = buffer[i + offset];

                        if (b == (byte)'\n' || b == (byte)'\r')
                        {
                            ProcessIncomingLine();

                            incomingLen = 0;
                        }
                        else if (incomingLen == incoming.Length)
                        {
                            incomingLen = 0;
                        }
                        else
                            incoming[incomingLen++] = b;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ProcessIncomingLine()
        {
            if (incomingLen > 0)
            {
                var line = Encoding.UTF8.GetString(incoming, 0, incomingLen);

                var newState = current.GetNextState(line);

                if (null != newState)
                {
                    current = newState;
                    OnNewState(current, line);
                }
            }
        }

        private void ProcessStdError(byte[] buffer, int offset, int count)
        {

        }

        protected virtual void OnProcessExited()
        {
        }

        protected abstract void GetProcessRunnerArguments(out string exeName, out string args);

        protected abstract void OnNewState(StateDescriptor state, string line);

        public void Dispose()
        {
            Stop();
        }
    }
}

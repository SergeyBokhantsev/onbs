using ProcessRunnerNamespace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace ProcessStateMachine
{
    public abstract class StateMachine : IDisposable
    {
		protected readonly ILogger logger;

        private ProcessRunner process;

        private StateDescriptor root;
        private StateDescriptor current;

		private byte[] outBuffer = new byte[1024];
		private int outLen;

		private byte[] errBuffer = new byte[1024];
		private int errLen;

		protected bool disposed;

        public bool Active
        {
            get
            {
                return null != process;
            }
        }

        public StateMachine (StateDescriptor root, ILogger logger)
        {
			if (null == root)
				throw new ArgumentNullException ("root");

			if (null == logger)
				throw new ArgumentNullException ("logger");

            this.root = this.current = root;
			this.logger = logger;
        }

		public void Start()
        {
            lock (root)
            {
				if (disposed)
					return;

				if (null != process)
					return;

                current = root;

				OnStarting();

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
				lock(root)
				{
                	process = null;
				}

                OnProcessExited();
            };

            process.StdOut += ProcessStdOut;
            process.StdError += ProcessStdError;

            try
            {
                process.Run();
            }
            catch
            {
                ProcessRunner.TryExitEndDispose(process);
                process = null;
                OnProcessExited();
            }
        }

        private void ProcessStdOut(byte[] buffer, int offset, int count)
        {
			ProcessData (buffer, offset, count, outBuffer, ref outLen);
        }

		private void ProcessStdError(byte[] buffer, int offset, int count)
		{
			ProcessData (buffer, offset, count, errBuffer, ref errLen);
		}

		private void ProcessData(byte[] buffer, int offset, int count, byte[] thisBuffer, ref int thisLen)
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
							ProcessIncomingLine(thisBuffer, thisLen);

							thisLen = 0;
						}
						else if (thisLen == thisBuffer.Length)
						{
							thisLen = 0;
						}
						else
							thisBuffer[thisLen++] = b;
					}
				}
			}
			catch (Exception ex)
			{
				logger.Log (this, ex);
			}
		}

		private void ProcessIncomingLine(byte[] buffer, int count)
        {
			if (count > 0)
            {
				var line = Encoding.UTF8.GetString(buffer, 0, count);

                var newState = current.GetNextState(line);

                if (null != newState)
                {
                    current = newState;
                    OnNewState(current, line);
                }
				else
				{
					OnUnrecognizedLine (line);
				}
            }
        }		       

		protected virtual void OnStarting()
		{
		}

        protected virtual void OnProcessExited()
        {
        }

		protected virtual void OnUnrecognizedLine(string line)
		{
		}

        protected abstract void GetProcessRunnerArguments(out string exeName, out string args);

        protected abstract void OnNewState(StateDescriptor state, string line);

        public void Dispose()
        {
			lock (root) 
			{
				if (!disposed)
				{
					Stop ();
					disposed = true;
				}
			}
        }
    }
}

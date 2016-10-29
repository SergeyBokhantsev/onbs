using System;
using System.IO;
namespace Interfaces
{
    /// <summary>
    /// Raised when target process has exited
    /// </summary>
    /// <param name="unexpected">True if process was closed without ProcessRunner request</param>
    public delegate void ExitedEventHandler(bool unexpected);

    public delegate void IncomingDataEventHandler(byte[] buffer, int offset, int count);

    public interface IProcessRunner
    {
        event ExitedEventHandler Exited;
        bool HasExited { get; }
        void Run();
        void SendToStandardInput(string message);
		void SendToStandardInput(char c);
        //string GetFromStandardOutput();
        bool WaitForExit(int timeoutMilliseconds, out MemoryStream output);
        bool WaitForExit(int timeoutMilliseconds);
        void Exit();
    }

    public interface IProcessRunnerEx
    {
        /// <summary>
        /// Fired when target process has exited
        /// </summary>
        event ExitedEventHandler Exited;

        /// <summary>
        /// Fired if RedirectStandardOutput=true and data exist
        /// Note: called in worker thread
        /// </summary>
        event IncomingDataEventHandler StdOut;

        /// <summary>
        /// Fired if RedirectStandardError=true and data exist
        /// Note: called in worker thread
        /// </summary>
        event IncomingDataEventHandler StdError;

        bool HasExited { get; }

        /// <summary>
        /// Access Std Out data if CollectStandardOutput=true
        /// </summary>
        void ReadStdOut(Action<MemoryStream> accessor);

        /// <summary>
        /// Access Std Error data if CollectStandardError=true
        /// </summary>
        void ReadStdError(Action<MemoryStream> accessor);

        void Run();
        void SendToStandardInput(string message);
        void SendToStandardInput(char c);
        bool WaitForExit(int timeoutMilliseconds);
        void Exit();
    }

    public class ProcessConfig
    {
        public string ExePath { get; set; }
        public string Args { get; set; }
        public bool WaitForUI { get; set; }
        /// <summary>
        /// No info messages to log
        /// </summary>
        public bool Silent { get; set; }
        
		public int AliveMonitoringInterval { get; set; }
		public bool RedirectStandardOutput { get; set; }
		public bool RedirectStandardInput { get; set; }

        public ProcessConfig()
        {
            AliveMonitoringInterval = 1000;
			RedirectStandardOutput = true;
			RedirectStandardInput = true;
        }
    }

    public interface IProcessRunnerFactory
    {
        ProcessConfig CreateConfig(string appKey, object[] argumentParameters = null);
        IProcessRunner Create(ProcessConfig param);
    }
}

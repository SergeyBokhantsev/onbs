using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    /// <summary>
    /// Raised when target process has exited
    /// </summary>
    /// <param name="unexpected">True if process was closed without ProcessRunner request</param>
    public delegate void ExitedEventHandler(bool unexpected);

    public interface IProcessRunner
    {
        event ExitedEventHandler Exited;
        string Name { get; }
        bool HasExited { get; }
        void Run();
        void SendToStandardInput(string message);
        string GetFromStandardOutput();
        bool WaitForExit(int timeoutMilliseconds);
        void Exit();
    }

    public interface IProcessRunnerFactory
    {
        IProcessRunner Create(string appKey);
        IProcessRunner Create(string exePath, string args, bool waitForUI);
    }
}

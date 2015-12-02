using System;
using System.Threading;

namespace Interfaces
{
    public enum HostControllerShutdownModes { Exit, Restart, Shutdown, UnhandledException }

    public interface IHostController
    {
        ILogger Logger { get; }
        SynchronizationContext SyncContext { get ; }
        IConfig Config { get; }
        IProcessRunnerFactory ProcessRunnerFactory { get; }
        T GetController<T>() where T : class, IController;
        void Shutdown(HostControllerShutdownModes mode);
        IHostTimer CreateTimer(int span, Action<IHostTimer> action, bool isEnabled, bool firstEventImmidiatelly);
    }
}

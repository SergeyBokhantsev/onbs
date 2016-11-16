using System;
using System.Threading;
using System.Threading.Tasks;

namespace Interfaces
{
    public enum HostControllerShutdownModes { Exit, Update, Restart, Shutdown, UnhandledException }

    public interface IHostController
    {
        ILogger Logger { get; }
        ONBSSyncContext SyncContext { get; }
        IConfig Config { get; }
       // IProcessRunnerFactory ProcessRunnerFactory { get; }
        T GetController<T>() where T : class, IController;
        Task Shutdown(HostControllerShutdownModes mode);
        IHostTimer CreateTimer(int span, Action<IHostTimer> action, bool isEnabled, bool firstEventImmidiatelly, string name);
        ISpeakService SpeakService { get; }
        IRemoteStorageService RemoteStorageService { get; }
        IMetricsService MetricsService { get; }
    }

    public abstract class ONBSSyncContext : SynchronizationContext
    {
        public abstract void Post(SendOrPostCallback callback, object state, string details);
    }
}

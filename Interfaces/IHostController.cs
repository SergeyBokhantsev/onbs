using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public enum HostControllerShutdownModes { Exit, Restart, Shutdown, UnhandledException }

    public interface IHostController
    {
        ILogger Logger { get; }
        IDispatcher Dispatcher { get ; }
        IConfig Config { get; }
        IProcessRunnerFactory ProcessRunnerFactory { get; }
        T GetController<T>() where T : class, IController;
        void Shutdown(HostControllerShutdownModes mode);
    }
}

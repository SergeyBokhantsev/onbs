using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace Tests.Mocks
{
    public class MockHostController : IHostController, IProcessRunnerFactory
    {
        public MockHostController()
        {
            Logger = new Mocks.Logger();
            SyncContext = new MockONBSSyncContext();
            Config = new Mocks.Config();
        }

        public ILogger Logger
        {
            get;
            set;
        }

        public ONBSSyncContext SyncContext
        {
            get;
            set;
        }

        public IConfig Config
        {
            get;
            set;
        }

        public IProcessRunnerFactory ProcessRunnerFactory
        {
            get
            {
                return this;
            }
        }

        public T GetController<T>() where T : class, IController
        {
            throw new NotImplementedException();
        }

        public Task Shutdown(HostControllerShutdownModes mode)
        {
            throw new NotImplementedException();
        }

        public IHostTimer CreateTimer(int span, Action<IHostTimer> action, bool isEnabled, bool firstEventImmidiatelly, string name)
        {
            throw new NotImplementedException();
        }

        public IProcessRunner Create(string appKey, object[] argumentParameters = null)
        {
            throw new NotImplementedException();
        }

        public IProcessRunner Create(ProcessConfig param)
        {
            return new ProcessRunner.ProcessRunnerImpl(param, Logger);
        }


        public ISpeakService SpeakService
        {
            get { throw new NotImplementedException(); }
        }


        public IRemoteStorageService RemoteStorageService
        {
            get { throw new NotImplementedException(); }
        }
    }
}

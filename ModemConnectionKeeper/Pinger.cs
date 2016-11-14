using ProcessStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModemConnectionKeeper
{
    public class Pinger : StateMachine
    {
        public event Action<string, string> State;

        private readonly string host;

        public Pinger(string host, int interval, int timeout)
            :base(CreateRoot_Windows())
        {
            this.host = host;
        }

        private static StateDescriptor CreateRoot_Windows()
        {
            var root = new StateDescriptor();

            var pingCouldNotFineHost = new StateDescriptor("NoHost", "Ping request could not find host");
            var pingStarted = new StateDescriptor("Started", "Pinging \\S+ \\[\\S+\\] with 32 bytes of data:");
            var pingGood = new StateDescriptor("OK", "Reply from \\S+: bytes=\\S+ time=\\d+ms TTL=\\d+");
            var pingTimeout = new StateDescriptor("Timeout", "Request timed out.");
            var pingEnd = new StateDescriptor("End", "Ping statistics for \\S+:");


            root.Children.Add(pingStarted);
            root.Children.Add(pingCouldNotFineHost);

            pingStarted.Children.Add(pingGood);
            pingStarted.Children.Add(pingTimeout);

            pingGood.Children.Add(pingGood);
            pingGood.Children.Add(pingTimeout);
            pingGood.Children.Add(pingEnd);

            pingTimeout.Children.Add(pingGood);
            pingTimeout.Children.Add(pingTimeout);
            pingTimeout.Children.Add(pingEnd);

            return root;
        }

        protected override void GetProcessRunnerArguments(out string exeName, out string args)
        {
            exeName = "ping";
            args = host;
        }

        protected override void OnNewState(StateDescriptor state, string line)
        {
            State(state.Name, line);
        }

        protected override void OnProcessExited()
        {
            Thread.Sleep(10000);

            Start();
        }
    }
}

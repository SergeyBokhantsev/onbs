using ProcessStateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace ModemConnectionKeeper
{
    public class Pinger : StateMachine
    {
        public event Action<string, string> State;

        private readonly string host;
		private readonly int interval;
		private readonly int timeout;

		public Pinger(string host, int interval, int timeout, ILogger logger)
			:base(CreateRoot_Nix(), logger)
        {
            this.host = host;
			this.interval = interval;
			this.timeout = timeout;
        }

		private static StateDescriptor CreateRoot_Nix()
		{
			var root = new StateDescriptor();

			var pingCouldNotFineHost = new StateDescriptor("NoHost", "unknown host");
			var pingNoNetwork = new StateDescriptor("NoNetwork", "Network is unreachable");
			var pingStarted = new StateDescriptor("Started", "PING \\S+ \\(\\S+\\) 56\\(84\\) bytes of data.");
			var pingGood = new StateDescriptor ("OK", "64 bytes from \\S+: icmp_seq=\\d+ ttl=\\d+ time=\\S+ ms");
			var pingTimeout = new StateDescriptor("Timeout", "out");
			var pingEnd = new StateDescriptor("End", "ping statistics");


			root.Children.Add(pingStarted);
			root.Children.Add(pingCouldNotFineHost);

			pingStarted.Children.Add(pingGood);
			pingStarted.Children.Add(pingTimeout);
			pingStarted.Children.Add(pingNoNetwork);

			pingGood.Children.Add(pingGood);
			pingGood.Children.Add(pingTimeout);
			pingGood.Children.Add(pingEnd);
			pingGood.Children.Add(pingNoNetwork);

			pingTimeout.Children.Add(pingGood);
			pingTimeout.Children.Add(pingTimeout);
			pingTimeout.Children.Add(pingEnd);
			pingTimeout.Children.Add(pingNoNetwork);

			pingNoNetwork.Children.Add(pingGood);
			pingNoNetwork.Children.Add(pingTimeout);
			pingNoNetwork.Children.Add(pingNoNetwork);
			pingNoNetwork.Children.Add(pingEnd);

			return root;
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
            exeName = "sudo";
			args = string.Format ("ping -i {0} -W {1} {2}", interval, timeout, host);
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

		protected override void OnUnrecognizedLine (string line)
		{
			logger.Log (this, string.Concat ("Unrecognized line: ", line), LogLevels.Error);
		}
    }
}

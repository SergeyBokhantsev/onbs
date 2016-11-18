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
        public event Action<bool> ConnectionStatus;

        private readonly string host;
		private readonly int interval;
		private readonly int timeout;

        public ConnectionMetricsProvider Metrics { get; set; }

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

			var pingCouldNotFineHost = new StateDescriptor("NoHost", StateDescriptor.CreateSubstringPredicates("unknown host"));
			var pingNoNetwork = new StateDescriptor("NoNetwork", StateDescriptor.CreateSubstringPredicates("Network is unreachable"));
			var pingStarted = new StateDescriptor("Started", StateDescriptor.CreateRegexPredicates("PING \\S+ \\(\\S+\\) 56\\(84\\) bytes of data."));
			var pingGood = new StateDescriptor("OK", StateDescriptor.CreateSubstringPredicates("64 bytes from")) { Tag = string.Empty };
			var pingTimeout = new StateDescriptor("Timeout", StateDescriptor.CreateSubstringPredicates("out"));
			var pingEnd = new StateDescriptor("End", StateDescriptor.CreateSubstringPredicates("ping statistics"));

			root.Add(pingStarted);
			root.Add(pingCouldNotFineHost);

			pingStarted.Add(pingGood);
			pingStarted.Add(pingTimeout);
			pingStarted.Add(pingNoNetwork);

			pingGood.Add(pingGood);
			pingGood.Add(pingTimeout);
			pingGood.Add(pingEnd);
			pingGood.Add(pingNoNetwork);

			pingTimeout.Add(pingGood);
			pingTimeout.Add(pingTimeout);
			pingTimeout.Add(pingEnd);
			pingTimeout.Add(pingNoNetwork);

			pingNoNetwork.Add(pingGood);
			pingNoNetwork.Add(pingTimeout);
			pingNoNetwork.Add(pingNoNetwork);
			pingNoNetwork.Add(pingEnd);

			return root;
		}

        private static StateDescriptor CreateRoot_Windows()
        {
            var root = new StateDescriptor();

            var pingCouldNotFineHost = new StateDescriptor("NoHost", StateDescriptor.CreateSubstringPredicates("Ping request could not find host"));
            var pingStarted = new StateDescriptor("Started", StateDescriptor.CreateRegexPredicates("Pinging \\S+ \\[\\S+\\] with 32 bytes of data:"));
            var pingGood = new StateDescriptor("OK", StateDescriptor.CreateRegexPredicates("Reply from \\S+: bytes=\\S+ time=\\d+ms TTL=\\d+")) { Tag = string.Empty };
            var pingTimeout = new StateDescriptor("Timeout", StateDescriptor.CreateSubstringPredicates("Request timed out."));
            var pingEnd = new StateDescriptor("End", StateDescriptor.CreateRegexPredicates("Ping statistics for \\S+:"));


            root.Add(pingStarted);
            root.Add(pingCouldNotFineHost);

            pingStarted.Add(pingGood);
            pingStarted.Add(pingTimeout);

            pingGood.Add(pingGood);
            pingGood.Add(pingTimeout);
            pingGood.Add(pingEnd);

            pingTimeout.Add(pingGood);
            pingTimeout.Add(pingTimeout);
            pingTimeout.Add(pingEnd);

            return root;
        }

        public new void Start()
        {
            ThreadPool.QueueUserWorkItem(state =>
                {
                    base.Start();
                }, null);
        }

        protected override void GetProcessRunnerArguments(out string exeName, out string args)
        {
            exeName = "sudo";
			args = string.Format ("ping -i {0} -W {1} {2}", interval, timeout, host);
        }

        protected override void OnNewState(StateDescriptor state, string line)
        {
            OnConnectionStatus(null != state.Tag);

            if (null != Metrics)
                Metrics.PingMessage.Set(line, ColoredStates.Normal);
        }

        private void OnConnectionStatus(bool connected)
        {
            var handler = ConnectionStatus;

            if (null != handler)
            {
                handler(connected);
            }
        }

        protected override void OnProcessExited()
        {
            if (null != Metrics)
                Metrics.PingMessage.Set(string.Concat("EXITED at ", DateTime.Now.ToString()), ColoredStates.Yellow);

            Thread.Sleep(5000);

            Start();
        }

		protected override void OnUnrecognizedLine (string line)
		{
			logger.Log (this, string.Concat ("Unrecognized line: ", line), LogLevels.Error);

            if (null != Metrics)
                Metrics.PingMessage.Set(line, ColoredStates.Red);
		}
    }
}

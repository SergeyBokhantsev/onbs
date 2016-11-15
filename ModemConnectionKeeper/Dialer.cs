using System;
using ProcessStateMachine;
using Interfaces;
using System.Threading;

namespace ModemConnectionKeeper
{
	public class Dialer : StateMachine
	{
		public class DialerState : StateDescriptor
		{
			public bool? Status { get; private set; }

			public DialerState(string name, string regexPattern, bool? status = null)
				:base(name, regexPattern)
			{
				Status = status;
			}
		}

		public event Action StateChanged;

		public event Action DialerProcessExited;

		private readonly string configFilePath;

		private bool logUnrecognized;

		private const int criticalMaximum = 10;
		private int criticalStatesCount;

        public bool MaximumErrorsCountReached
        {
            get
            {
                return criticalStatesCount >= criticalMaximum;
            }
        }

        public string CurrentStateDescription { get; private set; }

        public ColoredStates State { get; private set; }

		public Dialer (string configFilePath, ILogger logger)
			:base(CreateRoot(), logger)
		{
			this.configFilePath = configFilePath;

			logUnrecognized = true;
		}

		private static StateDescriptor CreateRoot()
		{
			var root = new StateDescriptor ();

			//--> WvDial: Internet dialer version 1.61
			var d_start = new DialerState("1. Start", "--> WvDial: Internet dialer version");

			//--> Cannot open /dev/ttyUSB1: No such file or directory
			var d_cannot_open_dev = new DialerState("1.1. Cannot open /dev/ttyUSB", "--> Cannot open /dev/ttyUSB");

			//--> Initializing modem.
			var d_init_modem = new DialerState("2. Initializing Modem", "--> Initializing modem");

			//--> Modem initialized.
			var d_modem_initialized = new DialerState("3. Modem initialized", "--> Modem initialized");

			//--> Waiting for carrier.
			var d_waiting_carrier = new DialerState("4. Waiting for carrier", "--> Waiting for carrier");

			//--> No Carrier!  Trying again.
			var d_no_carrier_retry = new DialerState("4.1. No carrier", "--> No Carrier!  Trying again.");

			//--> Timed out while dialing.  Trying again.
			var d_timeout_dialing_retry = new DialerState("4.2. Dial timeout", "--> Timed out while dialing.  Trying again.", false);

			//--> Carrier detected.  Waiting for prompt.
			var d_carrier_detected = new DialerState("5. Carrier detected", "--> Carrier detected.  Waiting for prompt");

			//--> Connected, but carrier signal lost!  Retrying...
			var d_carrier_signal_lost = new DialerState("5.1. Carrier lost", "--> Connected, but carrier signal lost!");

			//--> PPP negotiation detected.
			var d_negotiation_detected = new DialerState("6. PPP Negotiation detected", "--> PPP negotiation detected");

			//--> local  IP address 10.128.210.135
			var d_local_ip = new DialerState("7. Local IP", "--> local  IP address", true);

			//--> remote IP address 192.168.168.4
			var d_remote_ip = new DialerState("8. Local IP", "--> remote IP address", true);

			//--> primary   DNS address 195.128.182.46
			var d_primary_dns = new DialerState("9. Primary DNS", "--> primary   DNS address", true);

			//--> secondary DNS address 195.128.182.45
			var d_second_dns = new DialerState("10. Secondary DNS", "--> secondary DNS address", true);

			//--> Connect time 0.6 minutes.
			//var d_connect_time_summary = new StateDescriptor("11. Connect time summary", "--> Connect time \\S+ minutes.");

			//--> Disconnecting at Mon Nov 14 12:54:10 2016
			var d_disconnecting = new DialerState("11. Disconnecting", "--> Disconnecting at");

			// Primary success sequence
			root
				.Then (d_start)
				.Then (d_init_modem)
				.Then (d_modem_initialized)
				.Then (d_waiting_carrier)
				.Then (d_carrier_detected)
				.Then (d_negotiation_detected)
				.Then (d_local_ip)
				.Then (d_remote_ip)
				.Then (d_primary_dns)
				.Then (d_second_dns);

			//No /dev/ttyUSB device case
			d_start.Then(d_cannot_open_dev).Then(d_cannot_open_dev);

			//no carier / bad carrier cases
			d_waiting_carrier.Then (d_disconnecting);
			d_carrier_detected.Then (d_disconnecting);

			//no carrier trying again case
			d_waiting_carrier.Then(d_no_carrier_retry).Then(d_waiting_carrier);

			//timed out when dialing case
			d_waiting_carrier.Then (d_timeout_dialing_retry).Then(d_waiting_carrier);

			//--> Connected, but carrier signal lost!  Retrying...
			d_carrier_detected.Then (d_carrier_signal_lost).Then (d_waiting_carrier);

			//Reobtain IPs case
			d_second_dns.Then (d_local_ip);

			//Redial case
			d_second_dns.Then (d_disconnecting).Then(d_init_modem);

			//Re-init on disconnect (success case)
			d_disconnecting.Then (d_init_modem);

			//Re-init on disconnect (no device case)
			d_disconnecting.Then (d_cannot_open_dev);

			return root;
		}

		protected override void GetProcessRunnerArguments (out string exeName, out string args)
		{
			exeName = "sudo";
			args = string.Format ("wvdial -C {0}", configFilePath);
		}

		protected override void OnStarting ()
		{
			criticalStatesCount = 0;

            CurrentStateDescription = "Starting";
            State = ColoredStates.Yellow;
		}

		protected override void OnNewState (StateDescriptor state, string line)
		{
			var dialerState = state as DialerState;

			if (null == dialerState)
				throw new Exception ("Dialer state is null");

            CurrentStateDescription = dialerState.Name;

			if (dialerState.Status.HasValue) 
			{
                if (dialerState.Status.Value)
                {
                    criticalStatesCount = 0;
                    State = ColoredStates.Normal;
                }
                else
                {
                    criticalStatesCount++;
                    State = ColoredStates.Red;
                }
			}

			OnStateChanged();

			if (MaximumErrorsCountReached) 
			{
				logger.Log (this, "Maximum critical states reached, stopping.", LogLevels.Warning);
				Stop ();
			}
		}

        private void OnStateChanged()
        {
            var handler = StateChanged;
            if (null != handler)
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    logger.Log(this, ex);
                }
            }
        }

		protected override void OnUnrecognizedLine (string line)
		{
			if (logUnrecognized)
				logger.Log (this, line, LogLevels.Info);
		}

		protected override void OnProcessExited ()
		{
			var handler = DialerProcessExited;

            if (null != handler)
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    logger.Log(this, ex);
                }
            }
		}
	}
}


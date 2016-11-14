using ModemConnectionKeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace ModemConnectionKeeperTester
{
	public class Logger : ILogger
	{
		public event LogEventHandlerDelegate LogEvent;

		public DateTime LastWarningTime { get; }

		public void Log(object caller, string message, LogLevels level)
		{
			Console.WriteLine (message);
		}

		public void Log(object caller, Exception ex)
		{
			Console.WriteLine (ex.Message);
		}

		public void Flush() 
		{
		}
	}

    class Program
    {
        static void Main(string[] args)
        {
//			using (var pinger = new Pinger ("google.com", 5, 15, new Logger ())) 
//			{
//				pinger.State += (name, line) => Console.WriteLine (name + " | " + line);
//
//				pinger.Start ();
//
//				while (pinger.Active)
//					Thread.Sleep (1000);
//
//				Thread.Sleep (-1);
//			}

			using (var dialer = new Dialer ("/home/pi/onbs/_bin/Application/Data/_wd.conf", new Logger ())) 
			{
				dialer.StateChanged += (name, line) => Console.WriteLine ("STATE: " + name + " | " + line);

				dialer.DialerProcessExited += () => { Thread.Sleep(10000); dialer.Start(); };

				dialer.Start ();

				Thread.Sleep (-1);
			}
        }
    }
}

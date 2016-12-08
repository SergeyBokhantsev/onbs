using ModemConnectionKeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using ProcessRunnerNamespace;

namespace ModemConnectionKeeperTester
{
	public class Logger : ILogger
	{
		public event LogEventHandlerDelegate LogEvent;

        public DateTime LastWarningTime { get; set; }

		public void Log(object caller, string message, LogLevels level)
		{
			Console.WriteLine (message);
		}

		public void Log(object caller, Exception ex)
		{
			Console.WriteLine ("EXCEPTION: " +  ex.Message);
		}

		public void Flush() 
		{
		}
	}

    public class Config : IConfig
    {

        public event Action<string> Changed;

        public string GetString(string name)
        {
			switch (name) 
			{
			case "Modem_vid":				
				return "12d1";

			case "Modem_storagemode_pid":
				return "1446";

			case "Modem_modemmode_pid":
				return "1506";

			default:
				throw new NotImplementedException ();
			}
        }

        public int GetInt(string name)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(string name)
        {
            throw new NotImplementedException();
        }

        public bool GetBool(string name)
        {
            throw new NotImplementedException();
        }

        public void Set<T>(string name, T value)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public Environments Environment
        {
            get 
			{ 
				throw new NotImplementedException(); 
			}
        }

        public bool IsSystemTimeValid
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public bool IsInternetConnected
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public bool IsGPSLock
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public bool IsMessagePending
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public bool IsMessageShown
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public bool IsDimLighting
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public string DataFolder
        {
            get 
            { 
				return "/home/pi/onbs/_bin/Application/Data";
            }
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

            //using (var dialer = new Dialer ("/home/pi/onbs/_bin/Application/Data/_wd.conf", new Logger ())) 
            //{
            //    dialer.StateChanged += () => Console.WriteLine ("STATE: " + dialer.CurrentStateDescription);

            //    dialer.DialerProcessExited += () => { Thread.Sleep(10000); dialer.Start(); };

            //    dialer.Start ();

            //    Thread.Sleep (-1);
            //}

            //var keeper = new ConnectionKeeper(new Config(), new Logger());

            //Thread.Sleep(-1);

			ThreadStart action = () => {

				var pr = ProcessRunner.ForTool ("sudo", "lsusb");
				pr.Run();
				pr.WaitForExit(15000);

			};

			var objs = Enumerable.Range (0, 30000).Select (i => new object ()).ToArray ();


			 Enumerable.Range (0, 30).ToList ().ForEach (i => { 
				new Thread (action).Start ();

			});

			objs = null;

			Enumerable.Range (0, 30).ToList ().ForEach (i => { 
				new Thread (action).Start ();

			});

			GC.Collect (2, GCCollectionMode.Forced, true);

			Thread.Sleep (100000);
        }
    }
}

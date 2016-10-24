using System;
using Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GtkLauncher
{
	public class ConsoleLogger : ILogger
	{
		private readonly object locker = new object();

		public event LogEventHandlerDelegate LogEvent;

		public ConsoleLogger ()
		{
			AllowedClassNames = new List<string> ();
		}

		public List<string> AllowedClassNames
		{
			get;
			private set;
		}

		#region ILogger implementation

		public DateTime LastWarningTime
		{
			get;
			private set;
		}

		public void Log(object caller, string message, LogLevels level)
		{
			if (level <= LogLevels.Warning)
				LastWarningTime = DateTime.Now;

			if (level <= this.Level)
			{
				var className = GetClassName(caller);

				if (!AllowedClassNames.Any() || AllowedClassNames.Contains(className))
				{
					lock (locker)
					{
						Console.WriteLine(string.Concat(DateTime.Now, " | ", level, " | ", className, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message));
					}
				}
			}
		}

		public void Log(object caller, Exception ex)
		{
			Log(caller, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace), LogLevels.Error);
		}

		public LogLevels Level
		{
			get
			{
				return LogLevels.Debug;
			}
		}

		#endregion

		public void Flush ()
		{
		}

		private string GetClassName(object caller)
		{
			if (caller == null)
				return "Unknown";

			return caller.GetType().Name;
		}
	}
}


using System;
using System.Threading;
using Interfaces;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace HostController
{
    public class LogMetrics : IMetricsProvider
    {
        private readonly LogLevels level;

        private readonly int maxMessagesCount;

        private readonly List<string> messages = new List<string>();

        public class Metric : IMetric
        {
            private readonly int index;
            private readonly IReadOnlyList<string> messages;

            public string Name
            {
                get { return (index+1).ToString(); }
            }

            public ColoredStates State
            {
                get { return ColoredStates.Normal; }
            }

            public Metric(int index, IReadOnlyList<string> messages)
            {
                this.index = index;
                this.messages = messages;
            }

            public override string ToString()
            {
                if (messages.Count > index)
                    return messages[index];
                else
                    return string.Empty;
            }
        }

        public event MetricsUpdatedEventHandler MetricUpdated;

        public event Action<ColoredStates> SummaryStateUpdated;

        public string Name
        {
            get;
            set;
        }

        public ColoredStates SummaryState
        {
            get { return ColoredStates.Normal; }
        }

        public IEnumerable<IMetric> Metrics
        {
            get 
            { 
                for(int i = 0; i < maxMessagesCount; ++i)
                {
                    yield return new Metric(i, messages);
                }
            }
        }

        public LogMetrics(string name, LogLevels level, int maxMessagesCount)
        {
            Name = name;
            this.level = level;
            this.maxMessagesCount = maxMessagesCount;
        }

        public void AcceptMessage(string message, LogLevels level)
        {
            if (level <= this.level)
            {
                messages.Add(message);

                if (messages.Count > maxMessagesCount)
                    messages.RemoveAt(0);

                var handler = MetricUpdated;

                if (null != handler)
                    handler(this, Metrics);
            }
        }
    }

    public class ConsoleLoggerWrapper : ILogger
    {
        public event LogEventHandlerDelegate LogEvent;

        private readonly ILogger[] loggers;

        private readonly int startTime;

        public DateTime LastWarningTime
        {
            get { return loggers.Max(l => l.LastWarningTime); }
        }

        public LogMetrics Metrics { get; set; }

        internal ConsoleLoggerWrapper(ILogger[] loggers)
        {
            if (null == loggers)
                throw new ArgumentNullException("loggers");

            this.loggers = loggers;

            this.startTime = Environment.TickCount;
        }

		private readonly object locker = new object();

        public void Log(object caller, string message, LogLevels level)
        {
			lock (locker)
			{
            if (caller != null && caller is UIModels.ModelBase)
                WriteToConsole(string.Concat(GetTimestamp(), " | ", level, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message));

            foreach (var logger in loggers)
            {
                logger.Log(caller, message, level);
            }

            OnLogEvent(caller, message, level);

            //if (null != Metrics)
               // Metrics.AcceptMessage(message, level);
			}
        }

        private string GetTimestamp()
        {
            int ticks = Environment.TickCount - startTime;
            int minutes = ticks / 60000;
            ticks -= minutes * 60000;
            int seconds = ticks / 1000;
            ticks -= seconds * 1000;
            int milliseconds = ticks;
            return string.Concat(minutes, ":", seconds, ".", milliseconds);
        }

        public void Log(object caller, Exception ex)
        {
			lock (locker) {
				WriteToConsole (string.Concat(GetTimestamp(), " | ", ex.Message, Environment.NewLine, ex.StackTrace));

				foreach (var logger in loggers) {
					logger.Log (caller, ex);
				}

				OnLogEvent (caller, ex.Message, LogLevels.Error);
			}
        }

        private void OnLogEvent(object caller, string message, LogLevels level)
        {
            var handler = LogEvent;

            if (null != handler)
                handler(caller, message, level);
        }

        [Conditional("DEBUG")]
        private void WriteToConsole(string message)
        {
            Console.WriteLine(message);
        }

        public void Flush()
        {
            foreach (var logger in loggers)
            {
                logger.Flush();
            }
        }
    }
}

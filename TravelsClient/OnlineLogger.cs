using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using TravelsClient;
using System.Threading;

namespace TravelsClient
{
    public class OnlineLogger : ILogger
    {
        private const int logMaxSize = 100 * 1024;

        public event LogEventHandlerDelegate LogEvent;

        private readonly IConfig config;
        private readonly LogLevels level;
        private readonly StringBuilder buffer = new StringBuilder();
        private readonly GeneralLoggerClient client;
        private readonly Lazy<SynchronizationContext> syncContextAccessor;

        private int logId = -1;
        private int logSize;

        private readonly int startTime;

        private int busy;

        private bool disabled;

        public DateTime LastWarningTime
        {
            get;
            private set;
        }

        public OnlineLogger(IConfig config, Lazy<SynchronizationContext> syncContextAccessor)
        {
            this.startTime = config.Uptime;

            if (null == config)
                throw new ArgumentNullException("config");

            if (null == syncContextAccessor)
                throw new ArgumentNullException("syncContextAccessor");

            this.config = config;
            this.syncContextAccessor = syncContextAccessor;
            level = LogLevels.Info;
            client = new GeneralLoggerClient(new Uri(config.GetString(ConfigNames.TravelServiceUrl)),
                                             config.GetString(ConfigNames.TravelServiceKey),
                                             config.GetString(ConfigNames.TravelServiceVehicle));
        }

        public void Log(object caller, string message, LogLevels level)
        {
            if (disabled)
                return;

            if (level <= LogLevels.Warning)
                LastWarningTime = DateTime.Now;

            if (level <= this.level)
            {
                string className = caller != null ? caller.GetType().ToString() : "NULL";

                AddLine(string.Concat(GetTimestamp(), " | ", level, " | ", className, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message, Environment.NewLine));
            }
        }

        private string GetTimestamp()
        {
            int ticks = config.Uptime - startTime;
            int minutes = ticks / 60000;
            ticks -= minutes * 60000;
            int seconds = ticks / 1000;
            ticks -= seconds * 1000;
            int milliseconds = ticks;
            return string.Concat(minutes, ":", seconds, ".", milliseconds);
        }

        private void AddLine(string p)
        {
            lock (buffer)
            {
                buffer.Append(p);
            }
        }

        public void Log(object caller, Exception ex)
        {
            if (disabled)
                return;

            Log(caller, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace), LogLevels.Error);
            Flush();
        }

        private async Task<bool> Upload()
        {
            if (Interlocked.Exchange(ref busy, 1) == 1)
            {
                return true;
            }

            try
            {
                if (config.IsInternetConnected)
                {
                    string body = null;
                    bool success = false;

                    lock (buffer)
                    {
                        if (buffer.Length > 0)
                        {
                            if (logSize + buffer.Length > logMaxSize)
                            {
                                logId = -1;
                                logSize = 0;
                            }

                            body = buffer.ToString();
                            buffer.Clear();
                        }
                    }

                    if (null != body)
                    {
                        if (logId == -1)
                        {
                            var createLogResult = await client.CreateNewLogAsync(body);

                            if (success = createLogResult.Success)
                                logId = createLogResult.Value;
                            else
                                Log(this, createLogResult.ErrorMessage, LogLevels.Warning);
                        }
                        else
                        {
                            var addResult = await client.AppendLogAsync(logId, body);

                            if (!(success = addResult.Success))
                                Log(this, addResult.ErrorMessage, LogLevels.Warning);
                        }

                        if (success)
                            logSize += body.Length;
                        else
                        {
                            lock (buffer)
                            {
                                buffer.Insert(0, body);
                            }
                        }
                    }
                }

                lock (buffer)
                {
                    return buffer.Length > 0;
                }
            }
            catch (Exception ex)
            {
                Log(this, ex);

                return true;
            }
            finally
            {
                Interlocked.Exchange(ref busy, 0);
            }
        }

        public void Flush()
        {
            var syncContext = syncContextAccessor.Value;

            if (null == syncContext)
                throw new Exception("Cannot retrieve SyncContext");

            syncContext.Post(async state => await Upload(), null);
        }

        public async Task DisableAndUpload(int timeout)
        {
            disabled = true;

            const int delay = 1000;

            int waited = 0;

            while (await Upload())
            {
                if (waited >= timeout)
                    break;

                await Task.Delay(delay);

                waited += delay;
            }
        }
    }
}

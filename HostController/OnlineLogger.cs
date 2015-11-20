﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using TravelsClient;
using System.Threading;

namespace HostController
{
    public class OnlineLogger : ILogger
    {
        private const int logMaxSize = 100 * 1024;

        private readonly IConfig config;
        private readonly LogLevels level;
        private readonly StringBuilder buffer = new StringBuilder();
        private readonly GeneralLoggerClient client;
        private readonly InterlockedGuard uploadLocker = new InterlockedGuard();

        private int logId = -1;
        private int logSize;

        public OnlineLogger(IConfig config)
        {
            this.config = config;
            level = LogLevels.Info;
            client = new GeneralLoggerClient(new Uri(config.GetString(ConfigNames.TravelServiceUrl)),
                                             config.GetString(ConfigNames.TravelServiceKey),
                                             config.GetString(ConfigNames.TravelServiceVehicle));
        }

        public void Log(object caller, string message, LogLevels level)
        {
            if (level <= this.level || caller is Elm327.Client)
            {
                string className = caller != null ? caller.GetType().ToString() : "NULL";

                Add(string.Concat(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss.fff"), " | ", level, " | ", className, " | ", Thread.CurrentThread.ManagedThreadId, " | ", message, Environment.NewLine));
            }
        }

        private void Add(string p)
        {
            lock (buffer)
            {
                buffer.Append(p);
            }
        }

        public void Log(object caller, Exception ex)
        {
            Log(caller, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace), LogLevels.Error);
            Flush();
        }

        public void Upload()
        {
            uploadLocker.ExecuteIfFree(() =>
            {
                if (!config.IsInternetConnected)
                    return;

                string body = null;

                lock (buffer)
                {
                    if (buffer.Length == 0)
                        return;

                    if (logSize + buffer.Length > logMaxSize)
                    {
                        logId = -1;
                        logSize = 0;
                    }

                    body = buffer.ToString();
                    buffer.Clear();
                }

                try
                {
                    if (logId == -1)
                    {
                        logId = client.CreateNewLog(body);
                    }
                    else
                    {
                        client.AppendLog(logId, body);
                    }

                    logSize += body.Length;
                }
                catch (Exception ex)
                {
                    buffer.Insert(0, body);
                    Log(this, ex);
                }
            });
        }

        public void Flush()
        {
            ThreadPool.QueueUserWorkItem(o => Upload());
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GPSD.Net.Messages;
using Interfaces;
using Interfaces.GPS;
using TcpServer;

namespace GPSD.Net
{
    internal class WatchInfo
    {
        public bool Valid
        {
            get
            {
                return Watch != null;
            }
        }

        public WatchMsg Watch;
        public bool Responded;
    }

    internal class GPSDClient : IDisposable
    {
        private readonly IncomingClient tcpClient;
        private readonly ILogger logger;

        private readonly LockingProperty<GPRMC> gprmc = new LockingProperty<GPRMC>();
        private readonly LockingProperty<string> nmea = new LockingProperty<string>();

        private readonly AutoResetEvent updateEvent = new AutoResetEvent(false);

        private readonly WatchInfo watchInfo = new WatchInfo();

        private const int queryBufferSize = 1024;
        private readonly byte[] queryBuffer = new byte[queryBufferSize];
        private int queryBufferLen;

		private Encoding enc = Encoding.ASCII;

	private bool disposed;

        public bool Active
        {
            get
            {
                return !disposed && tcpClient.Active;
            }
        }

        public GPSDClient(IncomingClient tcpClient, ILogger logger)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

            if (logger == null)
                throw new ArgumentNullException("logger");

            this.tcpClient = tcpClient;
            this.logger = logger;
        }

        public void Run()
        {
            if (watchInfo != null)
                throw new InvalidOperationException("Allready runned");

            updateEvent.Set();

            tcpClient.BytesReceived += BytesReceived;

            logger.LogIfDebug(this, "GPSDClient activated");

            while (Active)
            {
                if (!updateEvent.WaitOne(2000))
                    continue;

                lock (watchInfo)
                {
                    if (!watchInfo.Valid)
                        SendHello();
                    else if (!watchInfo.Responded)
                        RespondOnWatch();
                    else if (!watchInfo.Watch.enable)
                        continue;
                    else if (watchInfo.Watch.json)
                        SendJson();
                    else if (watchInfo.Watch.nmea)
                        SendNmea();
                }
            }
        }

        private void RespondOnWatch()
        {
            logger.LogIfDebug(this, "Begin responding on watch");

            var dm = Json.SimpleJsonSerializer.Serialize(new DevicesMsg(), Encoding.Default);
            var wm = Json.SimpleJsonSerializer.Serialize(watchInfo.Watch, enc);

            RetryMonitor(() =>
                {
                    WriteLn(dm);
                    WriteLn(wm);
                }, 3);

            watchInfo.Responded = true;

            logger.LogIfDebug(this, "End responding on watch");
        }

        private void BytesReceived(byte[] buffer, int count)
        {
            logger.LogIfDebug(this, string.Format("Received {0} bytes", count));

            if (count > 0)
            {
                if (queryBufferLen + count <= queryBufferSize)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        queryBuffer[queryBufferLen++] = buffer[i];

                        if (buffer[i] == 10)
                        {
                            var query = enc.GetString(queryBuffer, 0, queryBufferLen).Trim();
                            logger.LogIfDebug(this, string.Format("Query detected: {0}", query));
                            watchInfo.Watch = ParseWatchQuery(query);
                            logger.LogIfDebug(this, string.Format("Query parsed: {0}", watchInfo.Valid));
                            queryBufferLen = 0;
                            updateEvent.Set();
                        }
                    }
                }
                else
                {
                    logger.Log(this, "Query buffer overflow", LogLevels.Error);
                    queryBufferLen = 0;
                    updateEvent.Set();
                }
            }
        }

        public void SetGPRMC(GPRMC gprmc)
        {
            this.gprmc.Value = gprmc;

            if (watchInfo.Valid && watchInfo.Watch.enable && watchInfo.Watch.json)
            {
                updateEvent.Set();
                logger.LogIfDebug(this, "GPRMC accepted");
            }
        }

        public void SetNMEA(string nmea)
        {
            this.nmea.Value = nmea;

            if (watchInfo.Valid && watchInfo.Watch.enable && watchInfo.Watch.nmea)
            {
                updateEvent.Set();
                logger.LogIfDebug(this, "NMEA accepted");
            }
        }
	
		private void WriteLn(byte[] data)
		{
            lock (tcpClient)
            {
                tcpClient.Write(data, 0, data.Length);
                tcpClient.Write(new byte[] { 10 }, 0, 1);
            }
		}

        private void SendHello()
        {
            logger.LogIfDebug(this, "Begin sending hello");
            var hello = Json.SimpleJsonSerializer.Serialize(new VersionMsg(), enc);
            RetryMonitor(() => WriteLn(hello), 3);
            logger.LogIfDebug(this, "End sending hello");
        }

        private WatchMsg ParseWatchQuery(string query)
        {
            try
            {
                if (!query.Contains("?WATCH="))
                    return null;

                var jsonStr = query.Trim().Substring(query.IndexOf('=') + 1);
                var jsonData = enc.GetBytes(jsonStr);

                Json.JsonObj json = null;
                if (!Json.JsonParser.TryParse(jsonData, out json))
                    return null;

                var res = WatchMsg.Parse(json);
				return res;
            }
            catch (Exception ex)
            {
                logger.Log(this, "Exception parsing Watch Query", LogLevels.Error);
                logger.Log(this, ex);
                return null;
            }
        }

        private void SendJson()
        {
            logger.LogIfDebug(this, "Begin sending Json");

            var g = this.gprmc.Value ?? new GPRMC();

            var fake = "{\"class\":\"TPV\",\"device\":\"/dev/pts/1\"," +
                "\"time\":\"" + (g.Active ? g.Time.ToString("O") : "0") + "\",\"ept\":0.0,\"track\":" + g.TrackAngle.ToString() + "," +
                "\"lat\":" + g.Location.Lat.ToString() + ",\"lon\":" + g.Location.Lon.ToString() + ",\"speed\":"
                + g.Speed.ToString() + ",\"mode\":" + (g.Active ? "2" : "1") + "}";


            var bytes = enc.GetBytes(fake);

            RetryMonitor(() => WriteLn(bytes), 3);

            logger.LogIfDebug(this, "End sending Json");
        }

        private void SendNmea()
        {
            logger.LogIfDebug(this, "Begin sending NMEA");

			var nmea = this.nmea.Value;
            var data = enc.GetBytes(nmea);
            lock (tcpClient)
            {
                RetryMonitor(() => tcpClient.Write(data, 0, data.Length), 3);
            }

            logger.LogIfDebug(this, "End sending NMEA");
        }

        private void RetryMonitor(Action action, int tryCount)
        {
            int tryNum = 0;

            while (true)
            {
                try
                {
                    action();

                    if (tryNum > 0)
                        logger.Log(this, string.Format("ACTION SUCCEEDED at {0} try", tryNum), LogLevels.Info);

                    return;
                }
                catch (Exception ex)
                {
                    logger.Log(this, ex);

                    if (tryNum++ < tryCount)
                    {
                        logger.Log(this, "REPEATING ACTION...", LogLevels.Warning);
                    }
                    else
                    {
                        logger.Log(this, "NO MORE REPEAT", LogLevels.Error);
                        throw;
                    }
                }
                finally
                {
                    tryNum++;
                }
            }
        }

        public void Dispose()
        {
            tcpClient.Dispose();
            disposed = true;
            logger.Log(this, "GPSD Client disposed", LogLevels.Info);
        }
    }
}

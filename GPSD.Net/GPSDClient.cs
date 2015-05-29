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
        public WatchMsg Watch;
        public bool Responded;
    }

    internal class GPSDClient : IDisposable
    {
        private readonly IncomingClient tcpClient;

        private readonly LockingProperty<GPRMC> gprmc = new LockingProperty<GPRMC>();
        private readonly LockingProperty<string> nmea = new LockingProperty<string>();

        private readonly AutoResetEvent updateEvent = new AutoResetEvent(false);

        private WatchInfo watchInfo;

        private string queryBuffer = string.Empty;

		private Encoding enc = Encoding.ASCII;

        public bool Active
        {
            get
            {
                return tcpClient.Active;
            }
        }

        public GPSDClient(IncomingClient tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

            this.tcpClient = tcpClient;
        }

		public void Run()
		{
			tcpClient.BytesReceived += BytesReceived;

			SendHello();

			while (Active)
			{
				updateEvent.WaitOne();
				//Thread.Sleep(1000);

                var w = watchInfo;

                if (w == null)
                    continue;

                lock (w)
                {
                    if (!w.Responded)
                        RespondOnWatch();
                    else if (!w.Watch.enable)
                        continue;
                    else if (w.Watch.json)
                        SendJson();
                    else if (w.Watch.nmea)
                        SendNmea();
                }
			}
		}

        private void RespondOnWatch()
        {
            var dm = Json.SimpleJsonSerializer.Serialize(new DevicesMsg(), Encoding.Default);
            var wm = Json.SimpleJsonSerializer.Serialize(watchInfo.Watch, enc);

            WriteLn(dm);
            WriteLn(wm);

            watchInfo.Responded = true;
        }

        private void BytesReceived(byte[] buffer, int count)
        {
            queryBuffer += enc.GetString(buffer, 0, count);

            if (queryBuffer.EndsWith("\n"))
            {
                watchInfo = new WatchInfo() { Watch = ParseWatchQuery(queryBuffer) };

                queryBuffer = string.Empty;
                updateEvent.Set();
            }
        }

        public void SetGPRMC(GPRMC gprmc)
        {
            this.gprmc.Value = gprmc;

            var w = watchInfo;
            if (w != null)
            {
                lock (w)
                {
                    if (w.Watch.enable && w.Watch.json)
                        updateEvent.Set();
                }
            }
        }

        public void SetNMEA(string nmea)
        {
            this.nmea.Value = nmea;

            var w = watchInfo;
            if (w != null)
            {
                lock (w)
                {
                    if (w.Watch.enable && w.Watch.nmea)
                        updateEvent.Set();
                }
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
            var hello = Json.SimpleJsonSerializer.Serialize(new VersionMsg(), enc);
			WriteLn (hello);
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

                return WatchMsg.Parse(json);
            }
            catch
            {
                return null;
            }
        }

        private void SendJson()
        {
            var bytes = Json.SimpleJsonSerializer.Serialize(new TPVMsg(gprmc.Value), enc);
			WriteLn (bytes);
        }

        private void SendNmea()
        {
            var nmea = this.nmea.Value;
            var data = enc.GetBytes(nmea);
            lock (tcpClient)
            {
                tcpClient.Write(data, 0, data.Length);
            }
        }

        public void Dispose()
        {
            tcpClient.Dispose();
        }
    }
}

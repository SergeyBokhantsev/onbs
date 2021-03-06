﻿using System;
using System.Text;
using System.Threading;
using GPSD.Net.Messages;
using Interfaces;
using Interfaces.GPS;
using System.Net.Sockets;

namespace GPSD.Net
{
    internal class GPSDClient : IDisposable
    {
        private enum ClientStates { SendHello, WaitingWatch, RespondOnWatch, SendGPS }

        private readonly TcpClient tcpClient;
        private readonly NetworkStream stream;
        private readonly ILogger logger;
        private readonly IConfig config;

        public readonly LockingProperty<GPRMC> gprmc = new LockingProperty<GPRMC>();
        public readonly LockingProperty<string> nmea = new LockingProperty<string>();

        private WatchMsg watch;
        private ClientStates state = ClientStates.SendHello;

        private string queryBuffer = string.Empty;

		private Encoding enc = Encoding.ASCII;

	private bool disposed;

        public bool Active
        {
            get
            {
                return !disposed && tcpClient.Connected && stream.CanRead && stream.CanWrite;
            }
        }

		public GPSDClient(TcpClient tcpClient, NetworkStream stream, ILogger logger, IConfig config)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            
            if (logger == null)
                throw new ArgumentNullException("logger");

            if (config == null)
                throw new ArgumentNullException("config");

            this.tcpClient = tcpClient;
            this.stream = stream;
            this.logger = logger;
            this.config = config;

            nmea.Value = string.Empty;
            gprmc.Value = new GPRMC();

			logger.LogIfDebug (this, "GPSDClient created");
        }

        public void Run()
        {
			logger.LogIfDebug (this, "GPSDClient started");

            int pause = 200;

            while (Active)
            {
                switch (state)
                {
                    case ClientStates.SendHello:
                        SendHello();
                        break;

                    case ClientStates.WaitingWatch:
                        CheckIncoming();
                        break;

                    case ClientStates.RespondOnWatch:
                        RespondOnWatch();
                        break;

                    case ClientStates.SendGPS:
                        pause = 1000;
                        if (!config.GetBool(ConfigNames.GPSDPaused))
                        {
                            if (watch.json)
                                SendJson();
                            if (watch.nmea)
                                SendNmea();
                        }
                        break;
                }

                Thread.Sleep(pause);
            }
        }

        byte[] buffer = new byte[2048];
        private void CheckIncoming()
        {
            try
            {
                if (stream.DataAvailable)
                {
                    var readed = stream.Read(buffer, 0, buffer.Length);

                    if (readed > 0)
                    {
                        BytesReceived(buffer, readed);
                    }
                    else
                    {
                        Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
            }
        }

        private void RespondOnWatch()
        {
			logger.LogIfDebug (this, "Begin respond on Watch");

            var dm = Json.SimpleJsonSerializer.Serialize(new DevicesMsg(), Encoding.Default);
            var wm = Json.SimpleJsonSerializer.Serialize(watch, enc);

            WriteLn(dm);
            WriteLn(wm);

            state = ClientStates.SendGPS;

			logger.LogIfDebug (this, "End respond on Watch");
        }

        private void BytesReceived(byte[] data, int count)
        {
			if (disposed)
				return;

            logger.LogIfDebug(this, string.Concat("Received: ", count));

            queryBuffer += enc.GetString(data, 0, count);

            if (queryBuffer.EndsWith("\n"))
            {
                watch = ParseWatchQuery(queryBuffer);

                if (watch == null)
                {
                    logger.Log(this, "Error parsing incoming message, going to initial state...", LogLevels.Error);
                    logger.Log(this, queryBuffer, LogLevels.Error);
                    state = ClientStates.SendHello;
                }
                else
                {
					logger.LogIfDebug (this, "Watch message received");
                    state = ClientStates.RespondOnWatch;
                }

                queryBuffer = string.Empty;
            }
        }

        private void WriteLn(byte[] data)
        {
            stream.Write(data, 0, data.Length);
            stream.Write(new byte[] { 10 }, 0, 1);
            stream.Flush();
        }

        private void SendHello()
        {
			logger.LogIfDebug (this, "Begin hello");
            var hello = Json.SimpleJsonSerializer.Serialize(new VersionMsg(), enc);
			WriteLn (hello);
			state = ClientStates.WaitingWatch;
			logger.LogIfDebug (this, "End hello");
        }

        private WatchMsg ParseWatchQuery(string query)
        {
            try
            {
                if (!query.Contains("?WATCH="))
                    return null;

                var jsonStr = query.Trim().Substring(query.IndexOf('=') + 1);
                var jsonData = enc.GetBytes(jsonStr);

                Json.JsonObj json;
                if (!Json.JsonParser.TryParse(jsonData, out json))
                    return null;

                var res = WatchMsg.Parse(json);
				return res;
            }
            catch
            {
                return null;
            }
        }

		//double temp = 30.42;

        private void SendJson()
        {
			logger.LogIfDebug (this, "Begin seng json");

            var g = this.gprmc.Value ?? new GPRMC();

                //  var bytes = Json.SimpleJsonSerializer.Serialize(tpv, enc);

                //temp += 0.0005;

                //var fake = "{\"class\":\"TPV\",\"device\":\"/dev/pts/1\"," +
                //    "\"time\":\"" + DateTime.Now.ToString("O") + "\",\"ept\":0.005,\"track\":" + gprmc.Value.TrackAngle.ToString() + "," +
                //    "\"lat\":" + gprmc.Value.Location.Lat.ToString() + ",\"lon\":" + gprmc.Value.Location.Lon.ToString() + ",\"speed\":1.87,\"mode\":2}";
                
                var fake = "{\"class\":\"TPV\",\"device\":\"/dev/pts/1\"," +
                    "\"time\":\"" + (g.Active ? g.Time.ToString("O") : "0") + "\",\"ept\":0.0,\"track\":" + g.TrackAngle.ToString() + "," +
                    "\"lat\":" + g.Location.Lat.ToString() + ",\"lon\":" + g.Location.Lon.ToString() + ",\"speed\":"
					+ g.Speed.ToString() +",\"mode\":"+ (g.Active ? "2" : "1") +"}";


                var bytes = enc.GetBytes(fake);

                WriteLn(bytes);

			logger.LogIfDebug (this, "End send json");
        }

        private void SendNmea()
        {
			logger.LogIfDebug (this, "Begin send nmea");

            var data = enc.GetBytes(this.nmea.Value);

            if (Active)
            {
                stream.Write(data, 0, data.Length);
            }

			logger.LogIfDebug (this, "End send nmea");
        }

        public void Dispose()
        {
			logger.LogIfDebug (this, "Disposing...");
            disposed = true;
        }
    }
}






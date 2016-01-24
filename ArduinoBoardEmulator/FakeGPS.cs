using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoBoardEmulator
{
    internal class FakeGPS
    {
        private readonly string rawNmea = "$GPGGA,180843,3540.995842,N,13946.246277,E,1,12,0.78,3.0,M,0.0,M,,*49" + Environment.NewLine
+ "$GPRMC,180843,A,3540.995842,N,13946.246277,E,16.3,25.3,051115,,,A*75" + Environment.NewLine
+ "$GPGSA,A,3,07,25,16,01,18,22,26,30,08,47,24,06,1.32,0.78,1.07*08" + Environment.NewLine
+ "$GPGSV,3,1,12,07,90,0,44,25,30,240,40,16,30,120,40,01,30,0,40*75" + Environment.NewLine
+ "$GPGGA,181151,3540.856840,N,13946.165545,E,1,12,0.78,3.0,M,0.0,M,,*4A" + Environment.NewLine
+ "$GPRMC,181151,A,3540.856840,N,13946.165545,E,16.3,25.3,051115,,,A*76" + Environment.NewLine
+ "$GPGSV,3,2,12,18,59,203,42,22,59,278,40,26,15,108,40,30,49,183,41*70" + Environment.NewLine
+ "$GPGSV,3,3,12,08,35,057,49,47,32,125,46,24,20,152,47,06,16,274,42*7A" + Environment.NewLine
+ "$GPGGA,180843,3540.995842,N,13946.246277,E,1,12,0.78,3.0,M,0.0,M,,*49" + Environment.NewLine
+ "$GPRMC,180843,A,3540.995842,N,13946.246277,E,16.3,25.3,051115,,,A*75" + Environment.NewLine
+ "$GPGSA,A,3,07,25,16,01,18,22,26,30,08,47,24,06,1.32,0.78,1.07*08" + Environment.NewLine
+ "$GPGGA,181151,3540.856840,N,13946.165545,E,1,12,0.78,3.0,M,0.0,M,,*4A" + Environment.NewLine
+ "$GPRMC,181151,A,3540.856840,N,13946.165545,E,16.3,25.3,051115,,,A*76" + Environment.NewLine
+ "$GPGSV,3,1,12,07,90,0,44,25,30,240,40,16,30,120,40,01,30,0,40*75" + Environment.NewLine
+ "$GPGSV,3,2,12,18,59,203,42,22,59,278,40,26,15,108,40,30,49,183,41*70" + Environment.NewLine
+ "$GPGSV,3,3,12,08,35,057,49,47,32,125,46,24,20,152,47,06,16,274,42*7A" + Environment.NewLine
+ "$GPGGA,180843,3540.995842,N,13946.246277,E,1,12,0.78,3.0,M,0.0,M,,*49" + Environment.NewLine
+ "$GPRMC,180843,A,3540.995842,N,13946.246277,E,16.3,25.3,051115,,,A*75" + Environment.NewLine
+ "$GPGGA,181151,3540.856840,N,13946.165545,E,1,12,0.78,3.0,M,0.0,M,,*4A" + Environment.NewLine
+ "$GPRMC,181151,A,3540.856840,N,13946.165545,E,16.3,25.3,051115,,,A*76" + Environment.NewLine
+ "$GPGSA,A,3,07,25,16,01,18,22,26,30,08,47,24,06,1.32,0.78,1.07*08" + Environment.NewLine
+ "$GPGSV,3,1,12,07,90,0,44,25,30,240,40,16,30,120,40,01,30,0,40*75" + Environment.NewLine
+ "$GPGSV,3,2,12,18,59,203,42,22,59,278,40,26,15,108,40,30,49,183,41*70" + Environment.NewLine
+ "$GPGSV,3,3,12,08,35,057,49,47,32,125,46,24,20,152,47,06,16,274,42*7A" + Environment.NewLine
+ "$GPGGA,181151,3540.856840,N,13946.165545,E,1,12,0.78,3.0,M,0.0,M,,*4A" + Environment.NewLine
+ "$GPRMC,181151,A,3540.856840,N,13946.165545,E,16.3,25.3,051115,,,A*76";

        public event Action<byte[]> NMEA;

        public FakeGPS()
        {
           // (new Thread(Send) { IsBackground = true }).Start();
        }

        private void Send(object state)
        {
            int offset = 0;

            while (true)
            {
                Thread.Sleep(1000);
                var handler = NMEA;
                if (handler != null)
                {
                    var chunk = rawNmea.Substring(offset, 15);
                    handler(Encoding.ASCII.GetBytes(chunk));

                    offset += 15;

                    if (offset >= rawNmea.Length - 15)
                        offset = 0;
                }
            }
        }
    }
}

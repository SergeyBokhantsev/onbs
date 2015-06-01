using Interfaces.GPS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSController
{
    internal class NmeaParser
    {
        public event Action<GPRMC> GPRMC;

        private GPRMC gprmc;

        public void Accept(string sentence)
        {
            if (!string.IsNullOrEmpty(sentence))
            {
                if (sentence.Length < 3 || sentence[sentence.Length - 3] != '*' || !checksumOk(sentence))
                {
                    // GPS ERROR
                    return;
                }

                var items = sentence.Split(',', '*');

                if (items.Length > 0)
                {
                    switch (items[0])
                    {
                        case "GPRMC":
                            var gprmc = new GPRMC();
                            if (gprmc.Parse(items))
                            {
                                Postprocess(gprmc);
                                OnGPRMCReceived(this.gprmc);
                            }
                            break;
                    }
                }
            }
        }

        private void Postprocess(GPRMC newGprmc)
        {
            if (gprmc != null && gprmc.Active)
            {
                double heading;

                if (newGprmc.Active && newGprmc.Speed > 8)
                {
                    heading = Interfaces.GPS.Helpers.GetHeading(gprmc.Location, newGprmc.Location);
                }
                else
                {
                    heading = gprmc.TrackAngle;
                }

				newGprmc.TrackAngle = heading;
            }

			gprmc = newGprmc;
        }

        private bool checksumOk(string str)
        {
            try
            {
                byte checksum = (byte)Convert.ToInt32(str.Substring(str.Length - 2), 16);
                byte currentChecksum = 0;

                int len = str.Length;
                for (int i = 0; i < len - 3; ++i)
                    currentChecksum ^= (byte)str[i];
                return currentChecksum == checksum;
            }
            catch
            {
                return false;
            }
        }

        private void OnGPRMCReceived(GPRMC gprmc)
        {
            var handler = GPRMC;
            if (handler != null)
                handler(gprmc);
        }
    }
}

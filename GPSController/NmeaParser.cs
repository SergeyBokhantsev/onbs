using Interfaces;
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

        private LockingProperty<GPRMC> gprmc = new LockingProperty<GPRMC>();

        public GPRMC LastGPRMC
        {
            get { return gprmc.Value; }
        }

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
                                OnGPRMCReceived();
                            }
                            break;
                    }
                }
            }
        }

        private void Postprocess(GPRMC newGprmc)
        {
            if (gprmc.Value != null && gprmc.Value.Active)
            {
                double heading;

                if (newGprmc.Active && newGprmc.Speed > 5)
                {
                    heading = Interfaces.GPS.Helpers.GetHeading(gprmc.Value.Location, newGprmc.Location);
                }
                else
                {
                    heading = gprmc.Value.TrackAngle;
                }

				newGprmc.TrackAngle = heading;
            }

			gprmc.Value = newGprmc;
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

        private void OnGPRMCReceived()
        {
            var handler = GPRMC;
            if (handler != null)
                handler(gprmc.Value);
        }
    }
}

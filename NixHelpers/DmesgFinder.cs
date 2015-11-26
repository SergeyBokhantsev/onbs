using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NixHelpers
{
    public static class DmesgFinder
    {
        public static IEnumerable<string> EnumerateTTYUSBDevices(IProcessRunnerFactory prf)
        {
            try
            {
                if (prf == null)
                    throw new ArgumentNullException("ProcessRunnerFactory is null");

                var pr = prf.Create("sudo", "dmesg | grep -i tty", false);

                pr.Run();

                pr.WaitForExit(5000);

                var output = pr.GetFromStandardOutput();

                return output.Split('\n').Where(l => l.Contains("now attached to ttyUSB")).Select(l => l.Trim());
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception in EnumerateTTYUSBDevices: {0}", ex.Message), ex);
            }
        }

        public static string FindTTYUSBPort(string deviceName, IProcessRunnerFactory prf)
        {
            try
            {
                if (deviceName == null)
                    throw new ArgumentNullException("deviceName is null");

                // Looking for lines like:
                // [ 34.210198] usb 2-1.1: ElmDeviceName now attached to ttyUSB1

                foreach (var line in EnumerateTTYUSBDevices(prf).Where(l => l.Contains(deviceName)))
                {
                    const string ttyUSBString = "ttyUSB";
                    int ttyUSBIndex = line.IndexOf(ttyUSBString);

                    if (ttyUSBIndex != -1)
                    {
                        return string.Concat("/dev/", line.Substring(ttyUSBIndex, ttyUSBString.Length + 1));
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception in RPiConfigResolver.GetElm327Port resolver: {0}", ex.Message), ex);
            }
        }
    }
}

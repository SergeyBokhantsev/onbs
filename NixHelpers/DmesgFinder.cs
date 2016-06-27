using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace NixHelpers
{
    public static class DmesgFinder
    {
        public static IEnumerable<USBDevice> EnumerateUSBDevices(IProcessRunnerFactory prf)
        {
			IProcessRunner pr = null;

            try
            {
                Ensure.ArgumentIsNotNull(prf);

				pr = prf.Create("dmesg");

				pr.Run();

                MemoryStream outputStream;
				if(pr.WaitForExit(10000, out outputStream))
				{ 
                var output = outputStream.GetString();
                return USBDevice.Parse(output);
				}
				else
				{
					throw new Exception("dmsg timeout");
				}
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception in EnumerateUSBDevices: {0}", ex.Message), ex);
            }
			finally {
				if(null != pr && !pr.HasExited)
					pr.Exit();
			}
        }

        public static USBDevice FindUSBDevice(string vid, string pid, IProcessRunnerFactory prf)
        {
            return EnumerateUSBDevices(prf).FirstOrDefault(d => d.VID == vid && d.PID == pid);
        }
    }
}

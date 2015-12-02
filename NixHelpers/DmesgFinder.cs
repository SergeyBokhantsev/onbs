﻿using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NixHelpers
{
    public static class DmesgFinder
    {
        public static IEnumerable<USBDevice> EnumerateUSBDevices(IProcessRunnerFactory prf)
        {
            try
            {
                Ensure.ArgumentIsNotNull(prf);

				var pr = prf.Create("dmesg");

                pr.Run();

                pr.WaitForExit(5000);

                var output = pr.GetFromStandardOutput();

                return USBDevice.Parse(output);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception in EnumerateUSBDevices: {0}", ex.Message), ex);
            }
        }

        public static USBDevice FindUSBDevice(string vid, string pid, IProcessRunnerFactory prf)
        {
            return EnumerateUSBDevices(prf).FirstOrDefault(d => d.VID == vid && d.PID == pid);
        }
    }
}

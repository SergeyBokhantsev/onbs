using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ProcessRunnerNamespace;

namespace NixHelpers
{
    public static class DmesgFinder
    {
        public static IEnumerable<USBDevice> EnumerateUSBDevices()
        {
            return ProcessRunner.ExecuteTool("EnumerateUSBDevices", output => USBDevice.Parse(output), 8000, "dmesg");
        }

        public static USBDevice FindUSBDevice(string vid, string pid)
        {
            return EnumerateUSBDevices().FirstOrDefault(d => d.VID == vid && d.PID == pid);
        }
    }
}

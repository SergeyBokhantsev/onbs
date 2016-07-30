﻿using Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NixHelpers
{
    public class USBBusDevice
    {
        public string Bus { get; private set; }
        public string Device { get; private set; }
        public string VID { get; private set; }
        public string PID { get; private set; }
        public string Product { get; private set; }

        public static IEnumerable<USBBusDevice> Parse(Stream lsusb_Stream)
        {
            var result = new List<USBBusDevice>();

            var regex = new Regex("Bus (\\d\\d\\d) Device (\\d\\d\\d): ID (\\S\\S\\S\\S):(\\S\\S\\S\\S) (.*)");

            using (var sr = new StreamReader(lsusb_Stream))
            {
                string line = null;

                while ((line = sr.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var match = regex.Match(line);

                        if (match.Groups.Count == 6)
                        {
                            result.Add(new USBBusDevice
                            {
                                Bus = match.Groups[1].Value,
                                Device = match.Groups[2].Value,
                                VID = match.Groups[3].Value,
                                PID = match.Groups[4].Value,
                                Product = match.Groups[5].Value,
                            });
                        }
                    }
                }
            }

            return result;
        }
    }

    public static class LsUsb
    {
        public static IEnumerable<USBBusDevice> EnumerateDevices(IProcessRunnerFactory prf)
        {
            var cfg = new ProcessConfig
            {
                AliveMonitoringInterval = 1000,
                ExePath = "lsusb",
                Args = null,
                RedirectStandardOutput = true,
                RedirectStandardInput = false,
                Silent = true,
                WaitForUI = false
            };

            var pr = prf.Create(cfg);

            MemoryStream stream;
            pr.Run();
			if (pr.WaitForExit (10000, out stream)) {
				return USBBusDevice.Parse (stream);
			} else {
				pr.Exit ();
				return new USBBusDevice[0];
			}
        }
    }
}

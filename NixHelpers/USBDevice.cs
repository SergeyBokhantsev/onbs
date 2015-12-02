using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NixHelpers
{
    public class USBDevice
    {
        private readonly List<string> attachedTo = new List<string>(3);

        public string Key { get; private set; }
        public string VID { get; private set; }
        public string PID { get; private set; }
        public string Product { get; private set; }
        public string Manufacturer { get; private set; }
        public string SerialNumber { get; private set; }
        public IEnumerable<string> AttachedTo
        {
            get
            {
                return attachedTo;
            }
        }

        public static IEnumerable<USBDevice> Parse(string dmesg)
        {
            if (dmesg == null)
                yield break;

            var devicesMap = new Dictionary<string, List<string>>(6);

            var regUSB = new Regex(@"^\[.+\] (usb \d-\d.\d):(.+)");
            
            foreach (var line in dmesg.Split('\n'))
            {
                var match = regUSB.Match(line);

                if (match.Groups.Count == 3)
                {
                    if (!devicesMap.ContainsKey(match.Groups[1].Value))
                        devicesMap.Add(match.Groups[1].Value, new List<string>(10));

                    devicesMap[match.Groups[1].Value].Add(match.Groups[2].Value);
                }
            }

            var regVId = new Regex(@"idVendor=(\w{4})");
            var regPId = new Regex(@"idProduct=(\w{4})");
            var regPoduct = new Regex(@"Product: (.+)");
            var regManufacturer = new Regex(@"Manufacturer: (.+)");
            var regSerialNumber = new Regex(@"SerialNumber: (.+)");
            var regAttachedTo = new Regex(@"now attached to (.+)");
            
            foreach(var key in devicesMap.Keys)
            {
                var device = new USBDevice
                {
                    Key = key
                };

                foreach (var item in devicesMap[key])
                {
                    var match = regVId.Match(item);
                    if (match.Groups.Count == 2)
                        device.VID = match.Groups[1].Value.Trim();

                    match = regPId.Match(item);
                    if (match.Groups.Count == 2)
                        device.PID = match.Groups[1].Value.Trim();

                    match = regPoduct.Match(item);
                    if (match.Groups.Count == 2)
                        device.Product = match.Groups[1].Value.Trim();

                    match = regManufacturer.Match(item);
                    if (match.Groups.Count == 2)
                        device.Manufacturer = match.Groups[1].Value.Trim();

                    match = regSerialNumber.Match(item);
                    if (match.Groups.Count == 2)
                        device.SerialNumber = match.Groups[1].Value.Trim();

                    match = regAttachedTo.Match(item);
                    if (match.Groups.Count == 2)
                        device.attachedTo.Add(string.Concat("/dev/", match.Groups[1].Value.Trim()));
                }

                yield return device;
            }
        }
    }
}

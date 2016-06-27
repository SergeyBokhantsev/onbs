using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NixHelpers
{
    public static class CPUInfo
    {
        public static double? GetCPUTemp(IProcessRunnerFactory prf)
        {
            var pr = prf.Create("cputemp");

            pr.Run();

            MemoryStream outputStream;
			if (!pr.WaitForExit (5000, out outputStream)) {
				pr.Exit ();
				return null;
			}
            var output = outputStream.GetString();

            int rawValue = 0;
            if (int.TryParse(output, out rawValue))
                return (double)rawValue / 1000;
            else
                return null; 
        }

        public static int? GetCPUSpeed(IProcessRunnerFactory prf)
        {
            var pr = prf.Create("cpuspeed");

            pr.Run();

            MemoryStream outputStream;
            
			if (!pr.WaitForExit (5000, out outputStream)) {
				pr.Exit ();
				return null;
			}
            
			var output = outputStream.GetString();

            int rawValue = 0;
            if (int.TryParse(output, out rawValue))
                return rawValue / 1000;
            else
                return null;
        }
    }
}

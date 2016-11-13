using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ProcessRunnerNamespace;

namespace NixHelpers
{
    public static class CPUInfo
    {
        public static async Task<double?> GetCPUTemp()
        {
            try
            {
                return await ProcessRunner.ExecuteToolAsync("GetCPUTemp", o => double.Parse(o) / 1000, 3000, "cat", "/sys/class/thermal/thermal_zone0/temp");
            }
            catch
            {
                return null;
            }
        }

        public static async Task<int?> GetCPUSpeed()
        {
            try
            {
				return await ProcessRunner.ExecuteToolAsync("GetCPUTemp", (string o) => int.Parse(o) / 1000, 3000, "cat", "/sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq");			
            }
			catch (Exception ex)
            {
                return null;
            }
        }
    }
}

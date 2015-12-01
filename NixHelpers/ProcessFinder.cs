using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Interfaces;

namespace NixHelpers
{
    public static class ProcessFinder
    {
        public static IEnumerable<string> EnumerateProcesses(IProcessRunnerFactory prf)
        {
            try
            {
                Ensure.ArgumentIsNotNull(prf);

                var pr = prf.Create("ps");

                pr.Run();

                pr.WaitForExit(5000);

                var output = pr.GetFromStandardOutput();

                return output.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception in NixHelpers.ProcessFinder.EnumerateProcesses: {0}", ex.Message), ex);
            }
        }

        public static int FindProcess(string name, IProcessRunnerFactory prf)
        {
            try
            {
                var regex = new Regex(string.Format("(\\S+).+({0})", name));

                foreach (var p in EnumerateProcesses(prf))
                {
                    var match = regex.Match(p);

                    if (match.Groups.Count == 3)
                        return int.Parse(match.Groups[1].Value);
                }

                return -1;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception in NixHelpers.ProcessFinder.FindProcess: {0}", ex.Message), ex);
            }
        }
    }
}

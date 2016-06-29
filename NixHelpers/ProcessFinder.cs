using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Interfaces;

namespace NixHelpers
{
    public static class ProcessFinder
    {
        public static IEnumerable<string> EnumerateProcesses(IProcessRunnerFactory prf)
        {
			IProcessRunner pr = null;

            try
            {
                Ensure.ArgumentIsNotNull(prf);

                var cfg = prf.CreateConfig("ps");
                cfg.RedirectStandardInput = false;
                cfg.RedirectStandardOutput = true;

                pr = prf.Create(cfg);

                pr.Run();

                MemoryStream outputStream;
                
				if(!pr.WaitForExit(5000, out outputStream))
				{
					throw new Exception("Enumerate processes timeout");
				}

				var output = outputStream.GetString();

                return output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception in NixHelpers.ProcessFinder.EnumerateProcesses: {0}", ex.Message), ex);
            }
			finally {
				if(null != pr && !pr.HasExited)
					pr.Exit();
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

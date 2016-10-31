using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Interfaces;
using ProcessRunnerNamespace;

namespace NixHelpers
{
    public static class ProcessFinder
    {
        public static IEnumerable<string> EnumerateProcesses()
        {
            return ProcessRunner.ExecuteTool("EnumerateProcesses",
                o => o.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries),
                5000, "ps", "ax");
        }

        public static int FindProcess(string name)
        {
            try
            {
                var regex = new Regex(string.Format("(\\S+).+({0})", name));

                foreach (var p in EnumerateProcesses())
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

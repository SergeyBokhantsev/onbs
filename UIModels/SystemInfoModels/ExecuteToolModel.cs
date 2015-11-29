﻿using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels
{
    public abstract class ExecuteToolModel : MultilineModel
    {
        public ExecuteToolModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
        }

        protected void ExecuteTool(string toolName)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        var pr = hc.ProcessRunnerFactory.Create(toolName);
                        pr.Run();
                        pr.WaitForExit(5000);
                        var output = pr.GetFromStandardOutput();

                        hc.Logger.Log(this, output, LogLevels.Info);

                        if (!string.IsNullOrEmpty(output))
                        {
                            foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                            {
                                AddLine(line);
                            }
                        }
                        else
                        {
                            AddLine(string.Format("Unable to retrieve {0} output", toolName));
                        }
                    }
                    catch (Exception ex)
                    {
                        hc.Logger.Log(this, ex);
                        AddLine(ex.Message);
                    }
                });
        }
    }
}
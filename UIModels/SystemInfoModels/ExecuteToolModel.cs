﻿using Interfaces;
using Interfaces.UI;
using System;
using System.IO;

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
					IProcessRunner pr = null;

                    try
                    {
                        pr = hc.ProcessRunnerFactory.Create(toolName);
                        pr.Run();

                        MemoryStream outputStream;
                        
						if(!pr.WaitForExit(5000, out outputStream))
							return;
                        
						var output = outputStream.GetString();

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
					finally
					{
						if(null != pr && !pr.HasExited)
							pr.Exit();
					}
                });
        }
    }
}

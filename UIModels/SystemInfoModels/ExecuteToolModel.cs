using Interfaces;
using Interfaces.UI;
using ProcessRunnerNamespace;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UIModels
{
    public abstract class ExecuteToolModel : MultilineModel
    {
        private readonly ProcessRunner pr;

        public ExecuteToolModel(string viewName, IHostController hc, MappedPage pageDescriptor, string toolExe, string args, bool useShell)
            : base(viewName, hc, pageDescriptor)
        {
            this.Disposing += ExecuteToolModel_Disposing;

            SetProperty(ModelNames.PageTitle, toolExe);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = toolExe,
                    Arguments = args,
                    UseShellExecute = useShell,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                pr = new ProcessRunner(psi, true, true);

                pr.Exited += pr_Exited;
            }
            catch (Exception ex)
            {
                hc.Logger.Log(this, ex);
            }
        }

        protected override void Initialize()
        {
            if (null != pr)
                hc.SyncContext.Post(async state =>
                    {
                        try
                        {
                            await pr.RunAsync();
                        }
                        catch (Exception ex)
                        {
                            FontSize = 12000;

                            AddLine(ex.Message);
                            AddLine("----------");
                            if (!string.IsNullOrEmpty(ex.StackTrace))
                            {
                                foreach(var line in ex.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                                {
                                    AddLine(line);
                                }
                            }

                            throw;
                        }
                    }, null, "Running tool");

            base.Initialize();
        }

        void pr_Exited(bool unexpected)
        {
            string output = null;

            if (pr.ReadStdOut(ms => output = ms.GetString()))
            {
                if (!string.IsNullOrEmpty(output))
                {
                    foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    {
                        AddLine(line);
                    }
                }
            }
            else if (pr.ReadStdError(ms => output = ms.GetString()))
            {
                AddLine(string.Concat("ERROR: ", output));
            }
            else
                AddLine("NO OUTPUT");
        }

        void ExecuteToolModel_Disposing(object sender, EventArgs e)
        {
            if (null != pr && !pr.HasExited)
                pr.Exit();
        }
    }
}

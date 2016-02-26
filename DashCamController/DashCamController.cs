using Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DashCamController
{
    public class DashCamController : IDashCamController, IDisposable
    {
        private readonly IHostController hc;
        private readonly string recordingExe;
        private readonly string recordingArg;
        private readonly int recordingLenSec;

        private readonly AutoResetEvent monitorEvent = new AutoResetEvent(true);
        private readonly FileManager fileManager;

        private IProcessRunner cameraProcess;

        private bool disposed;

        public DashCamController(IHostController hc)
        {
            if (hc == null)
                throw new ArgumentNullException("hc");

            this.hc = hc;

            fileManager = new FileManager(hc.Config.GetString(ConfigNames.DashCamRecorderFolder),
                                            hc.Config.GetInt(ConfigNames.DashCamRecorderFilesNumberQuota));

            recordingExe = hc.Config.GetString(ConfigNames.DashCamRecorderExe);
            recordingArg = hc.Config.GetString(ConfigNames.DashCamRecorderArg);
            recordingLenSec = hc.Config.GetInt(ConfigNames.DashCamRecorderSplitIntervalSec);

            hc.Config.Changed += Config_Changed;

            var monitorThread = new Thread(MonitorLoop);
            monitorThread.Name = "DashCamMonitor";
            monitorThread.IsBackground = true;
            monitorThread.Priority = ThreadPriority.AboveNormal;
            monitorThread.Start();
        }

        void Config_Changed(string name)
        {
            if (name == ConfigNames.DashCamRecorderEnabled)
                monitorEvent.Set();
        }

        public FileInfo[] GetVideoFilesInfo()
        {
            return fileManager.GetVideoFilesInfo();
        }

        private void MonitorLoop()
        {
            while (!disposed)
            {
                monitorEvent.WaitOne();

                if (hc.Config.GetBool(ConfigNames.DashCamRecorderEnabled))
                {
                    if (cameraProcess == null || cameraProcess.HasExited)
                        DoRecord();
                }
                else
                {
                    if (cameraProcess != null && !cameraProcess.HasExited)
                        cameraProcess.Exit();
                }
            }
        }

        private void DoRecord()
        {
            if (disposed)
                return;

            try
            {
                var processConfig = new ProcessConfig
                {
                    ExePath = recordingExe,
                    Args = string.Format(recordingArg, 
                    recordingLenSec * 1000, //0
                    hc.Config.GetString("DashCamRecorderOpacity"), //1
                    hc.Config.GetString("DashCamRecorderSharpness"), //2
                    hc.Config.GetString("DashCamRecorderContrast"), //3
                    hc.Config.GetString("DashCamRecorderBrightness"), //4
                    hc.Config.GetString("DashCamRecorderSaturation"), //5
                    hc.Config.GetString("DashCamRecorderISO"), //6
                    hc.Config.GetString("DashCamRecorderEV"), //7
                    hc.Config.GetString("DashCamRecorderExposure"), //8
                    hc.Config.GetString("DashCamRecorderAWB"), //9
                    hc.Config.GetString("DashCamRecorderEffect"), //10
                    hc.Config.GetString("DashCamRecorderMetering"), //11
                    hc.Config.GetString("DashCamRecorderRotation"), //12
                    hc.Config.GetString("DashCamRecorderDRC"), //13
                    hc.Config.GetString("DashCamRecorderAnnotate"), //14
                    fileManager.GetNextFileName()), // 15
                    Silent = true,
                    WaitForUI = false,
                    AliveMonitoringInterval = 200
                };

                cameraProcess = hc.ProcessRunnerFactory.Create(processConfig);
                cameraProcess.Exited += b => monitorEvent.Set();
                cameraProcess.Run();
            }
            catch (Exception ex)
            {
                hc.Logger.Log(this, ex);
                Thread.Sleep(5000);
                monitorEvent.Set();
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                hc.Config.Changed -= Config_Changed;

				if (cameraProcess != null && !cameraProcess.HasExited)
					cameraProcess.Exit();
            }
        }

        public bool IsProtected(FileInfo fileInfo)
        {
            return fileManager.IsProtected(fileInfo);
        }

        public FileInfo ProtectDeletion(FileInfo fileInfo)
        {
            return fileManager.ProtectDeletion(fileInfo);
        }

        public FileInfo GetMP4File(FileInfo fileInfo)
        {
            var coFiles = fileManager.GetWithCoFiles(fileInfo.FullName);
            var coFile = coFiles.FirstOrDefault(f => Path.GetExtension(f).Equals(".mp4", StringComparison.InvariantCultureIgnoreCase));

            if (coFile != null)
                return new FileInfo(coFile);
            else
                return ConvertToMP4(fileInfo.FullName);
        }

        private FileInfo ConvertToMP4(string fileName)
        {
            var mp4FilePath = Path.Combine(Path.GetDirectoryName(fileName), string.Concat(Path.GetFileNameWithoutExtension(fileName), ".mp4"));

            var config = new ProcessConfig
            {
                ExePath = hc.Config.GetString(ConfigNames.DashCamConvertToMP4Exe),
                Args = string.Format(hc.Config.GetString(ConfigNames.DashCamConvertToMP4Arg), fileName, mp4FilePath),
                WaitForUI = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false
            };

            var pr = hc.ProcessRunnerFactory.Create(config);
            pr.Run();
            pr.WaitForExit(120000);
            return new FileInfo(mp4FilePath);
        }

        public void Cleanup(FileInfo fileInfo)
        {
            fileManager.Cleanup(fileInfo.FullName);
        }

        public void Copy(FileInfo fileInfo, string destinationPath, CancellationToken ct, Action<int> progressAction = null)
        {
            long overalCopied = 0;
            DateTime updateTime = DateTime.MinValue;
            byte[] buffer = new byte[4096];
            using (var stream = fileInfo.OpenRead())
            {
                using (var outStream = File.Create(destinationPath, buffer.Length, FileOptions.WriteThrough))
                {
                    int readed = 1;

                    while (readed > 0)
                    {
                        ct.ThrowIfCancellationRequested();

                        readed = stream.Read(buffer, 0, buffer.Length);
                        outStream.Write(buffer, 0, readed);
                        overalCopied += readed;

                        if (progressAction != null && DateTime.Now > updateTime)
                        {
                            var percent = (int)(((double)overalCopied / ((double)fileInfo.Length + 1)) * 100);
                            progressAction(percent);
                            updateTime = DateTime.Now.AddMilliseconds(500);
                        }
                    }
                }
            }
        }
    }
}

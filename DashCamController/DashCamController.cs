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
        private const string fileExtension = ".h264";

        private readonly IHostController hc;
        private readonly string recordingExe;
        private readonly string recordingArg;
        private readonly int recordingLenSec;
        private readonly string recordingFolder;
        private readonly int recordingFilesNumberQuota;

        private readonly AutoResetEvent monitorEvent = new AutoResetEvent(true);

        private IProcessRunner cameraProcess;

        private bool disposed;

        public DashCamController(IHostController hc)
        {
            if (hc == null)
                throw new ArgumentNullException("hc");

            this.hc = hc;

            recordingExe = hc.Config.GetString(ConfigNames.DashCamRecorderExe);
            recordingArg = hc.Config.GetString(ConfigNames.DashCamRecorderArg);
            recordingLenSec = hc.Config.GetInt(ConfigNames.DashCamRecorderSplitIntervalSec);
            recordingFolder = hc.Config.GetString(ConfigNames.DashCamRecorderFolder);
            recordingFilesNumberQuota = hc.Config.GetInt(ConfigNames.DashCamRecorderFilesNumberQuota);

            hc.Config.Changed += Config_Changed;

            var monitorThread = new Thread(MonitorLoop);
            monitorThread.Name = "DashCamMonitor";
            monitorThread.IsBackground = true;
            monitorThread.Start();
        }

        void Config_Changed(string name)
        {
            if (name == ConfigNames.DashCamRecorderEnabled)
                monitorEvent.Set();
        }

        public FileInfo[] GetVideoFilesInfo()
        {
            return Directory.GetFiles(@"D:\onbs3").Select(f => new FileInfo(f)).ToArray();

			if (!Directory.Exists (recordingFolder))
				return new FileInfo[0];

            var files = Directory.GetFiles(recordingFolder, string.Concat("*", fileExtension));
            return files.Select(f => new FileInfo(f)).OrderBy(fi =>
                {
                    int ind;
                    if (int.TryParse(Path.GetFileNameWithoutExtension(fi.Name), out ind))
                        return int.MaxValue - ind;
                    else
						return int.MinValue;
                }).ToArray();
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
                    Args = string.Format(recordingArg, recordingLenSec * 1000, GetFileName()),
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

        private object GetFileName()
        {
            if (!Directory.Exists(recordingFolder))
                Directory.CreateDirectory(recordingFolder);

			var files = Directory.GetFiles (recordingFolder, string.Concat ("*", fileExtension));

			int oldest, newest;

			var allFilesCount = FindExtremumIndexes (files, out oldest, out newest);

            if (allFilesCount >= recordingFilesNumberQuota)
            {
                ThreadPool.QueueUserWorkItem(name => File.Delete((string)name),
				 CreateFileName(oldest));
            }

			return CreateFileName(newest+1);
        }

		private string CreateFileName(int index)
		{
			return Path.Combine(recordingFolder, string.Concat(index, fileExtension));
		}

		private int FindExtremumIndexes(string[] files, out int oldest, out int newest)
		{
			oldest = int.MaxValue;
			newest = int.MinValue;
			var allFilesCount = 0;

			foreach (var file in files) 
			{
				int temp;
                var fileIndexStr = Path.GetFileNameWithoutExtension(file);
				if (int.TryParse (fileIndexStr, out temp)) 
				{
					allFilesCount++;

					if (temp < oldest)
						oldest = temp;
					if (temp > newest)
						newest = temp;
				}
			}

			if (oldest == int.MaxValue)
				oldest = -1;
			if (newest == int.MinValue)
				newest = 0;

			return allFilesCount;
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
            return false;
        }

        public void ProtectDeletion(FileInfo fileInfo)
        {
            throw new NotImplementedException();
        }
    }
}

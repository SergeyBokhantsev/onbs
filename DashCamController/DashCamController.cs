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
    public class DashCamController : IDashCamController
    {
        private const string fileExtension = ".h264";
        private const string fileNamePattern = "video_";

        private readonly IHostController hc;
        private readonly string recordingExe;
        private readonly string recordingArg;
        private readonly int recordingLenSec;
        private readonly string recordingFolder;
        private readonly int recordingFilesNumberQuota;

        private IProcessRunner cameraProcess;
        private bool recordEnabled;

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

            //var thread = new Thread(WorkingLoop);
            //thread.Priority = ThreadPriority.AboveNormal;
            //thread.IsBackground = true;
            //thread.Name = "DashCam";
            //thread.Start();
        }

        //private void WorkingLoop()
        //{
        //    while(true)
        //    {
        //        if (cameraProcess == null || cameraProcess.Exited)
        //    }
        //}

        public void StartRecording()
        {
            if (recordEnabled)
                return;

            recordEnabled = true;
            DoRecord();
        }

        public void Stop()
        {
            recordEnabled = false;

            if (cameraProcess != null && !cameraProcess.HasExited)
            {
                cameraProcess.Exit();
                cameraProcess.WaitForExit(5000);
            }
        }

        public FileInfo[] GetVideoFilesInfo()
        {
            var files = Directory.GetFiles(@"C:\Users\serg\Documents\GitHub\onbs4");//recordingFolder, string.Concat(fileNamePattern, "*", fileExtension));
            return files.Select(f => new FileInfo(f)).OrderBy(fi => fi.CreationTime).ToArray();
        }

        private void DoRecord()
        {
            if (!recordEnabled)
                return;

            try
            {
                if (cameraProcess != null && !cameraProcess.HasExited)
                {
                    cameraProcess.Exit();
                    return;
                }

                var processConfig = new ProcessConfig
                {
                    ExePath = recordingExe,
                    Args = string.Format(recordingArg, recordingLenSec * 1000, GetFileName()),
                    Silent = true,
                    WaitForUI = false,
                    AliveMonitoringInterval = 200
                };

                cameraProcess = hc.ProcessRunnerFactory.Create(processConfig);
                cameraProcess.Exited += b => DoRecord();
                cameraProcess.Run();
            }
            catch (Exception ex)
            {
                hc.Logger.Log(this, ex);
            }
        }

        private object GetFileName()
        {
            if (!Directory.Exists(recordingFolder))
                Directory.CreateDirectory(recordingFolder);

			var files = Directory.GetFiles (recordingFolder, string.Concat (fileNamePattern, "*", fileExtension));

			int oldest, newest;

			FindExtremumIndexes (files, out oldest, out newest);

            if (files.Length >= recordingFilesNumberQuota)
            {
                ThreadPool.QueueUserWorkItem(name => File.Delete((string)name),
				 CreateFileName(oldest));
            }

			return CreateFileName(newest+1);
        }

		private string CreateFileName(int index)
		{
			return Path.Combine(recordingFolder, string.Concat(fileNamePattern, index, fileExtension));
		}

		private void FindExtremumIndexes(string[] files, out int oldest, out int newest)
		{
			oldest = int.MaxValue;
			newest = int.MinValue;

			foreach (var file in files) 
			{
				int temp;
				var fileIndexStr = Path.GetFileNameWithoutExtension(file)
					.Substring (fileNamePattern.Length);
				if (int.TryParse (fileIndexStr, out temp)) 
				{
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
		}
    }
}

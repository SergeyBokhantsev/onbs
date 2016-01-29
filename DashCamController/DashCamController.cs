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
            recordEnabled = true;
            DoRecord();
        }

        public void Stop()
        {
            recordEnabled = false;
            cameraProcess.Exit();
        }

        private void DoRecord()
        {
            if (!recordEnabled)
                return;

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
                 WaitForUI = false
            };

            cameraProcess = hc.ProcessRunnerFactory.Create(processConfig);
            cameraProcess.Exited += b => DoRecord();
            cameraProcess.Run();
        }

        private object GetFileName()
        {
            if (!Directory.Exists(recordingFolder))
                Directory.CreateDirectory(recordingFolder);

            var files = Directory.GetFiles(recordingFolder, string.Concat(fileNamePattern, "*", fileExtension)).OrderByDescending(f => f).ToArray();

            if (files.Length >= recordingFilesNumberQuota)
            {
                File.Delete(files.Last());
            }

			var lastFileIndex = 0;

			if (files.Length > 0) 
			{
				var lastFileName = files.First();
				var lastFileIndexStr = Path.GetFileNameWithoutExtension (lastFileName).Substring (fileNamePattern.Length);//, lastFileName.Length - (fileNamePattern.Length + fileExtension.Length));
				lastFileIndex = int.Parse (lastFileIndexStr);
			}

            return Path.Combine(recordingFolder, string.Concat(fileNamePattern, ++lastFileIndex, fileExtension));
        }

    }
}

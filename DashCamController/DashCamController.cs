﻿using Interfaces;
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

        private readonly string pictureExe;
        private readonly string pictureArg;

        private readonly AutoResetEvent monitorEvent = new AutoResetEvent(true);
        private readonly FileManager fileManager;

        private IProcessRunner cameraProcess;

        private bool protectCurrent;

        private bool disposed;

        private readonly List<Tuple<int, int, OrderPictureCallback>> pictureOrders = new List<Tuple<int, int, OrderPictureCallback>>();

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

            pictureExe = hc.Config.GetString(ConfigNames.DashCamPictureExe);
            pictureArg = hc.Config.GetString(ConfigNames.DashCamPictureArg);

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

                if (cameraProcess == null || cameraProcess.HasExited)
                {
                    DoPicture();
                }

                if (hc.Config.GetBool(ConfigNames.DashCamRecorderEnabled))
                {
                    if (cameraProcess == null || cameraProcess.HasExited)
                    {
                        DoRecord();
                    }
                }
                else
                {
                    if (cameraProcess != null && !cameraProcess.HasExited)
                    {
                        cameraProcess.Exit();
                    }
                }
            }
        }

        public void OrderPicture(int width, int height, OrderPictureCallback callback)
        {
            lock (pictureOrders)
            {
                pictureOrders.Add(new Tuple<int, int, OrderPictureCallback>(width, height, callback));
            }

            monitorEvent.Set();
        }

        private void DoPicture()
        {
            MemoryStream ms = null;

            int width;
            int height;

            lock (pictureOrders)
            {
                if (!pictureOrders.Any())
                    return;

                var firstOrder = pictureOrders.First();

                width = firstOrder.Item1;
                height = firstOrder.Item2;
            }

            try
            {
                var processConfig = CreatePictureProcessConfig(width, height);

                cameraProcess = hc.ProcessRunnerFactory.Create(processConfig);
                cameraProcess.Run();

                bool result = cameraProcess.WaitForExit(15000, out ms);

                if (!result)
                {
                    hc.Logger.Log(this, "Timeout while taking picture", LogLevels.Warning);
                    ms = null;
                }
            }
            catch (Exception ex)
            {
                hc.Logger.Log(this, ex);
            }
            finally
            {
                cameraProcess.Exit();
            }

            OrderPictureCallback[] callbacks;
            bool moreOrders = false;

            lock (pictureOrders)
            {
                callbacks = pictureOrders.Where(p => p.Item1 == width && p.Item2 == height).Select(p => p.Item3).ToArray();
                pictureOrders.RemoveAll(p => p.Item1 == width && p.Item2 == height);
                moreOrders = pictureOrders.Any();
            }

            foreach (var c in callbacks)
            {
                hc.SyncContext.Post(o =>
                {
                    var stream = o as MemoryStream;

                    if (stream != null)
                        stream.Seek(0, SeekOrigin.Begin);

                    c(stream);

                }, ms, "Take picture callback");
            }

            if (moreOrders)
                monitorEvent.Set();
        }

        protected virtual ProcessConfig CreatePictureProcessConfig(int width, int height)
        {
            var dim = hc.Config.IsDimLighting;

            return new ProcessConfig
            {
                ExePath = pictureExe,
                Args = string.Format(pictureArg,
                width, //0
                height, //1
                hc.Config.GetString("DashCamRecorderOpacity"), //2
                hc.Config.GetString("DashCamRecorderSharpness"), //3
                hc.Config.GetString("DashCamRecorderContrast"), //4
                dim ? hc.Config.GetString("DashCamRecorderBrightness_Night") : hc.Config.GetString("DashCamRecorderBrightness"), //5
                hc.Config.GetString("DashCamRecorderSaturation"), //6
                dim ? hc.Config.GetString("DashCamRecorderISO_Night") : hc.Config.GetString("DashCamRecorderISO"), //7
                dim ? hc.Config.GetString("DashCamRecorderEV_Night") : hc.Config.GetString("DashCamRecorderEV"), //8
                hc.Config.GetString("DashCamRecorderExposure"), //9
                hc.Config.GetString("DashCamRecorderAWB"), //10
                hc.Config.GetString("DashCamRecorderEffect"), //11
                hc.Config.GetString("DashCamRecorderMetering"), //12
                hc.Config.GetString("DashCamRecorderRotation"), //13
                hc.Config.GetString("DashCamRecorderDRC"), //14
                hc.Config.GetString("DashCamRecorderAnnotate"), //15
                hc.Config.GetBool(ConfigNames.DashCamRecorderPreviewEnabled) ? string.Empty : "--nopreview" //16
                ),
                WaitForUI = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = false,
                Silent = true
            };
        }

        protected virtual ProcessConfig CreateRecordProcessConfig(string fileName)
        {
            var dim = hc.Config.IsDimLighting;

            return new ProcessConfig
            {
                ExePath = recordingExe,
                Args = string.Format(recordingArg,
                recordingLenSec * 1000, //0
                hc.Config.GetString("DashCamRecorderOpacity"), //1
                hc.Config.GetString("DashCamRecorderSharpness"), //2
                hc.Config.GetString("DashCamRecorderContrast"), //3
                dim ? hc.Config.GetString("DashCamRecorderBrightness_Night") : hc.Config.GetString("DashCamRecorderBrightness"), //4
                hc.Config.GetString("DashCamRecorderSaturation"), //5
                dim ? hc.Config.GetString("DashCamRecorderISO_Night") : hc.Config.GetString("DashCamRecorderISO"), //6
                dim ? hc.Config.GetString("DashCamRecorderEV_Night") : hc.Config.GetString("DashCamRecorderEV"), //7
                hc.Config.GetString("DashCamRecorderExposure"), //8
                hc.Config.GetString("DashCamRecorderAWB"), //9
                hc.Config.GetString("DashCamRecorderEffect"), //10
                hc.Config.GetString("DashCamRecorderMetering"), //11
                hc.Config.GetString("DashCamRecorderRotation"), //12
                hc.Config.GetString("DashCamRecorderDRC"), //13
                hc.Config.GetString("DashCamRecorderAnnotate"), //14
                fileName, //15
                hc.Config.GetInt("DashCamRecorderBitrate") * 1000000, //16
                hc.Config.GetBool("DashCamRecorderStab") ? "--vstab" : string.Empty, // 17
                hc.Config.GetBool(ConfigNames.DashCamRecorderPreviewEnabled) ? string.Empty : "--nopreview" //18
                ), 
                Silent = true,
                WaitForUI = false,
                AliveMonitoringInterval = 200,
                RedirectStandardInput = false, 
                RedirectStandardOutput = false
            };
        }

        private void DoRecord()
        {
            if (disposed)
                return;

            try
            {
                var fileName = fileManager.GetNextFileName();
                var processConfig = CreateRecordProcessConfig(fileName);
                cameraProcess = hc.ProcessRunnerFactory.Create(processConfig);
                cameraProcess.Exited += isUnexpected =>
                {
                    monitorEvent.Set();

                    if (protectCurrent)
                    {
                        protectCurrent = false;

                        try
                        {
                            ProtectDeletion(new FileInfo(fileName));
                        }
                        catch (Exception ex)
                        {
                            hc.Logger.Log(this, "Failed to protect current recorded video.", LogLevels.Error);
                            hc.Logger.Log(this, ex);
                        }
                    }
                };

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
            if (fileInfo != null)
                return fileManager.ProtectDeletion(fileInfo);
            else
            {
                if (cameraProcess != null && !cameraProcess.HasExited)
                    protectCurrent = true;

                return null;
            }
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

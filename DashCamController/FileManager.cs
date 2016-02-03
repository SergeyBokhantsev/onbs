using Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DashCamController
{
    public class FileManager
    {
        private const string fileExtension = ".h264";
        private const string fileProtectionPrefix = "_";

        private readonly string directory;
        private readonly int recordingFilesNumberQuota;
        private readonly object ioLocker = new object();

        public FileManager(string directory, int recordingFilesNumberQuota)
        {
            Ensure.ArgumentIsNotNull(directory);
            this.directory = directory;
            this.recordingFilesNumberQuota = recordingFilesNumberQuota;
        }

        public FileInfo[] GetVideoFilesInfo()
        {
            lock (ioLocker)
            {
                if (!Directory.Exists(directory))
                    return new FileInfo[0];

                var files = Directory.GetFiles(directory, string.Concat("*", fileExtension));
                return files.Select(f => new FileInfo(f)).OrderBy(fi =>
                {
                    int ind;
                    if (int.TryParse(Path.GetFileNameWithoutExtension(fi.Name), out ind))
                        return int.MaxValue - ind;
                    else
                        return int.MinValue;
                }).ToArray();
            }
        }

        public string GetNextFileName()
        {
            lock (ioLocker)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var files = Directory.GetFiles(directory, string.Concat("*", fileExtension));

                int oldest, newest;

                var allFilesCount = FindExtremumIndexes(files, out oldest, out newest);

                if (allFilesCount >= recordingFilesNumberQuota)
                {
                    ThreadPool.QueueUserWorkItem(name => Cleanup((string)name), CreateFileName(oldest));
                }

                return CreateFileName(newest + 1);
            }
        }

        public void Cleanup(string filePath)
        {
            lock (ioLocker)
            {
                foreach (var file in GetWithCoFiles(filePath))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public string[] GetWithCoFiles(string filePath)
        {
            lock(ioLocker)
            {
                var searchPattern = string.Concat(Path.GetFileNameWithoutExtension(filePath), ".*");
                return Directory.GetFiles(directory, searchPattern);
            }
        }

        private string CreateFileName(int index)
        {
            return Path.Combine(directory, string.Concat(index, fileExtension));
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
                if (int.TryParse(fileIndexStr, out temp))
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

        public bool IsProtected(FileInfo fileInfo)
        {
            return fileInfo.Name.StartsWith(fileProtectionPrefix);
        }

        public FileInfo ProtectDeletion(FileInfo fileInfo)
        {
            lock (ioLocker)
            {
                if (!IsProtected(fileInfo))
                {
                    var location = Path.GetDirectoryName(fileInfo.FullName);
                    var protectedFilePath = Path.Combine(location, string.Concat(fileProtectionPrefix, fileInfo.Name));

                    Cleanup(protectedFilePath);

                    foreach (var file in GetWithCoFiles(fileInfo.FullName))
                    {
                        File.Move(file, Path.Combine(location, string.Concat(fileProtectionPrefix, Path.GetFileName(file))));
                    }

                    return new FileInfo(protectedFilePath);
                }
                else
                    return fileInfo;
            }
        }
    }
}

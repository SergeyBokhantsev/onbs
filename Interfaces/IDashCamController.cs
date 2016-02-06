using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IDashCamController : IController
    {
        /// <summary>
        /// Returns only files with .h264 extension
        /// </summary>
        FileInfo[] GetVideoFilesInfo();

        FileInfo GetMP4File(FileInfo fileInfo);

        /// <summary>
        /// Removes all files by fileName.* pattern
        /// </summary>
        void Cleanup(FileInfo fileInfo);

        bool IsProtected(FileInfo fileInfo);
        FileInfo ProtectDeletion(FileInfo fileInfo);

        string Copy(FileInfo fileInfo, string destinationPath);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Interfaces
{
    public delegate void OrderPictureCallback(MemoryStream ms);

    public interface IDashCamController : IController
    {
        void OrderPicture(int width, int height, OrderPictureCallback callback);

        /// <summary>
        /// Returns only files with .h264 extension
        /// </summary>
        FileInfo[] GetVideoFilesInfo();

        Task<FileInfo> GetMP4File(FileInfo fileInfo);

        /// <summary>
        /// Removes all files by fileName.* pattern
        /// </summary>
        void Cleanup(FileInfo fileInfo);
        bool IsProtected(FileInfo fileInfo);
        FileInfo ProtectDeletion(FileInfo fileInfo);
        void Copy(FileInfo fileInfo, string destinationPath, CancellationToken ct, Action<int> progressAction = null);
    }
}

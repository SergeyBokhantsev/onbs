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
        FileInfo[] GetVideoFilesInfo();

        bool IsProtected(FileInfo fileInfo);
        void ProtectDeletion(FileInfo fileInfo);
    }
}

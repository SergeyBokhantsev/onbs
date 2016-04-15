using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public class RemoteFileMetadata
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Stream Stream { get; set; }
    }

    public interface IRemoteStorageService
    {
        Task UploadFile(RemoteFileMetadata file);

        Task TestUpload();
    }
}

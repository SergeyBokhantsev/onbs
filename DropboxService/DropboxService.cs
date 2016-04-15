using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;
using Interfaces;

namespace DropboxService
{
    public class DropboxService : IRemoteStorageService
    {
        private readonly DropboxClient client;

        public DropboxService(string token = null)
        {
            if (string.IsNullOrWhiteSpace(token))
                token = Environment.GetEnvironmentVariable("DropboxToken", EnvironmentVariableTarget.Machine)
					?? Environment.GetEnvironmentVariable("DropboxToken", EnvironmentVariableTarget.User)
					?? Environment.GetEnvironmentVariable("DropboxToken", EnvironmentVariableTarget.Process);

            if (string.IsNullOrWhiteSpace(token))
                token = "vT27L-8JO00AAAAAAAAEuYrQgS5qdGnyr5DfhEch28GJvIQAWCSe90HDPlSAA_6r";

            client = new DropboxClient(token);
        }

        public async Task UploadFile(RemoteFileMetadata fileData)
        {
            if (fileData == null)
                throw new ArgumentNullException("fileData");

            if (fileData.Stream == null)
                throw new ArgumentNullException("fileData.Stream");

            if (string.IsNullOrWhiteSpace(fileData.Name))
                throw new ArgumentNullException("fileData.Name");

            var result = await client.Files.UploadAsync(fileData.Name,
                Dropbox.Api.Files.WriteMode.Add.Instance,
                true,
                null,
                false,
                fileData.Stream);

            fileData.Name = result.AsFile.Name;
            fileData.Id = result.AsFile.Id;
        }

        public async Task TestUpload()
        {
            await client.Files.UploadAsync("/RPI/rpiTest.txt",
                Dropbox.Api.Files.WriteMode.Add.Instance,
                true,
                null,
                false,
                new MemoryStream(new byte[] { 80,80,80,81 }));
        }
    }
}

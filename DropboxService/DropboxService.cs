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
                token = Environment.GetEnvironmentVariable("DropboxToken", EnvironmentVariableTarget.Machine) ??
                    Environment.GetEnvironmentVariable("DropboxToken", EnvironmentVariableTarget.Machine);

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("No Dropbox token found (provide 'DropboxToken' Environment variable)");

            client = new DropboxClient(token);
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

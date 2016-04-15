using System;
using System.IO;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DropboxTest
{
    [TestClass]
    public class DropboxApiTest
    {
        private string AccessToken
        {
            get
            {
                return Environment.GetEnvironmentVariable("DropboxToken", EnvironmentVariableTarget.User);
            }
        }

        private ListFolderResult ListFolder(DropboxClient client, string path)
        {
            var task = client.Files.ListFolderAsync(new Dropbox.Api.Files.ListFolderArg(path));
            task.Wait();
            return task.Result;
        }

        private void Delete(DropboxClient client, string path)
        {
            var deleteTask = client.Files.DeleteAsync(path);
            deleteTask.Wait();
        }

        [TestMethod]
        public void CreateDropboxClient()
        {
            //INIT-ACT
            var token = AccessToken;
            Assert.IsFalse(string.IsNullOrWhiteSpace(token));

            var client = new DropboxClient(token);

            //ASSERT
            Assert.IsNotNull(client);

            client.Dispose();
        }

        [TestMethod]
        public void EnumerateInRootFolder()
        {
            //INIT
            var client = new DropboxClient(AccessToken);

            //ACT
            var result = ListFolder(client, "");

            //Assert
            Assert.IsNotNull(result);

            client.Dispose();
        }

        [TestMethod]
        public void UploadFileAutorename()
        {
            //INIT
            var client = new DropboxClient(AccessToken);
            var folderName = Guid.NewGuid().ToString();

            for (int i = 0; i < 3; ++i)
            {
                var rnd = new Random();

                var up = client.Files.UploadAsync("/" + folderName + "/1.txt",
                    Dropbox.Api.Files.WriteMode.Add.Instance,
                    true,
                    null,
                    false,
                    new MemoryStream(new byte[] { (byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255) }));

                up.Wait();
            }


            var listResult = ListFolder(client, "/" + folderName);

            //ASSERT
            Assert.AreEqual(3, listResult.Entries.Count);

            //CLEANUP
            Delete(client, "/" + folderName);
            
            client.Dispose();
        }
    }
}

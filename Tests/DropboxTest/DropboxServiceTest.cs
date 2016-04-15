using System;
using System.IO;
using Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DropboxTest
{
    [TestClass]
    public class DropboxServiceTest
    {
        [TestMethod]
        public void UploadFileTest()
        {
            //INIT
            var service = new DropboxService.DropboxService();
            var meta = new RemoteFileMetadata { Name = "/a.bin", Stream = new MemoryStream(Guid.NewGuid().ToByteArray()) };

            //ACT
            service.UploadFile(meta).Wait();

            //ASSERT
            Assert.IsFalse(string.IsNullOrWhiteSpace(meta.Id));
        }
    }
}

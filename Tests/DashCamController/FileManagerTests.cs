using DashCamController;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.DashCamController
{
    [TestClass]
    public class FileManagerTests
    {
        private string GetDirPath(string dirName)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dirName);
        }

        private string CreateNewDir()
        {
            var dir = GetDirPath(Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);
            return dir;
        }

        private void CreateFile(string filePath)
        {
            File.WriteAllText(filePath, "123");
        }

        private string CreateCoFile(string filePath)
        {
            var coFilePath = Path.Combine(Path.GetDirectoryName(filePath),
                             Path.GetFileNameWithoutExtension(filePath) + ".mp4");
            CreateFile(coFilePath);

            return coFilePath;
        }

        [TestMethod]
        public void DashFileManager_GetFiles_NoFolderExist()
        {
            //INIT
            var manager = new FileManager(GetDirPath("noDirectory"), 10);

            //ACT
            var files = manager.GetVideoFilesInfo();

            //ASSERT
            Assert.IsNotNull(files);
            Assert.AreEqual(0, files.Length);
        }

        [TestMethod]
        public void DashFileManager_GetFiles_EmptyFolder()
        {
            //INIT
            var dir = CreateNewDir();
            var manager = new FileManager(dir, 10);

            //ACT
            var files = manager.GetVideoFilesInfo();

            //ASSERT
            Assert.IsNotNull(files);
            Assert.AreEqual(0, files.Length);
        }

        [TestMethod]
        public void DashFileManager_GetNextFileName_NoFolderExist()
        {
            //INIT
            var dir = CreateNewDir();
            var manager = new FileManager(dir, 10);

            //ACT
            var fileName1 = manager.GetNextFileName();
            var fileName2 = manager.GetNextFileName();

            //ASSERT
            Assert.IsNotNull(fileName1);
            Assert.IsFalse(File.Exists(fileName1));
            Assert.AreEqual(fileName1, fileName2);
        }

        [TestMethod]
        public void DashFileManager_AddingFiles()
        {
            //INIT
            var dir = CreateNewDir();
            var manager = new FileManager(dir, 10);

            //ACT
            var fileName1 = manager.GetNextFileName();
            CreateFile(fileName1);
            var fileName2 = manager.GetNextFileName();
            CreateFile(fileName2);
            var fileName3 = manager.GetNextFileName();
            CreateFile(fileName3);

            var files = manager.GetVideoFilesInfo();

            //ASSERT
            Assert.IsNotNull(files);
            Assert.AreEqual(3, files.Length);
            Assert.IsTrue(File.Exists(fileName1));
            Assert.IsTrue(File.Exists(fileName2));
            Assert.IsTrue(File.Exists(fileName3));
        }

        [TestMethod]
        public void DashFileManager_FileCleanuping()
        {
            //INIT
            var dir = CreateNewDir();
            var manager = new FileManager(dir, 3);

            //ACT
            var fileName1 = manager.GetNextFileName();
            CreateFile(fileName1);
            var fileName2 = manager.GetNextFileName();
            CreateFile(fileName2);
            var fileName3 = manager.GetNextFileName();
            CreateFile(fileName3);
            var fileName4 = manager.GetNextFileName();
            CreateFile(fileName4);
            Thread.Sleep(1000);

            var fileName5 = manager.GetNextFileName();
            CreateFile(fileName5);
            Thread.Sleep(1000);

            var files = manager.GetVideoFilesInfo();

            //ASSERT
            Assert.IsNotNull(files);
            Assert.AreEqual(3, files.Length);
            Assert.IsFalse(File.Exists(fileName1));
            Assert.IsFalse(File.Exists(fileName2));
            Assert.IsTrue(File.Exists(fileName3));
            Assert.IsTrue(File.Exists(fileName4));
            Assert.IsTrue(File.Exists(fileName5));
        }

        [TestMethod]
        public void DashFileManager_FileCleanupingWithCoFiles()
        {
            //INIT
            var dir = CreateNewDir();
            var manager = new FileManager(dir, 3);

            //ACT
            var fileName1 = manager.GetNextFileName();
            CreateFile(fileName1);
            var coFileName1 = CreateCoFile(fileName1);

            var fileName2 = manager.GetNextFileName();
            CreateFile(fileName2);
            var coFileName2 = CreateCoFile(fileName2);

            var fileName3 = manager.GetNextFileName();
            CreateFile(fileName3);
            var coFileName3 = CreateCoFile(fileName3);

            var fileName4 = manager.GetNextFileName();
            CreateFile(fileName4);
            var coFileName4 = CreateCoFile(fileName4);

            Thread.Sleep(1000);

            var fileName5 = manager.GetNextFileName();
            CreateFile(fileName5);
            var coFileName5 = CreateCoFile(fileName5);

            Thread.Sleep(1000);

            var files = manager.GetVideoFilesInfo();

            //ASSERT
            Assert.IsNotNull(files);
            Assert.AreEqual(3, files.Length);
            Assert.IsFalse(File.Exists(fileName1));
            Assert.IsFalse(File.Exists(coFileName1));
            Assert.IsFalse(File.Exists(fileName2));
            Assert.IsFalse(File.Exists(coFileName2));
            Assert.IsTrue(File.Exists(fileName3));
            Assert.IsTrue(File.Exists(coFileName3));
            Assert.IsTrue(File.Exists(fileName4));
            Assert.IsTrue(File.Exists(coFileName4));
            Assert.IsTrue(File.Exists(fileName5));
            Assert.IsTrue(File.Exists(coFileName5));
        }

        [TestMethod]
        public void DashFileManager_ManualCleanupWithCoFiles()
        {
            //INIT
            var dir = CreateNewDir();
            var manager = new FileManager(dir, 3);

            //ACT
            var fileName1 = manager.GetNextFileName();
            CreateFile(fileName1);
            var coFileName1 = CreateCoFile(fileName1);

            var fileName2 = manager.GetNextFileName();
            CreateFile(fileName2);
            var coFileName2 = CreateCoFile(fileName2);

            manager.Cleanup(fileName1);
            manager.Cleanup(coFileName2);
           
            var files = manager.GetVideoFilesInfo();

            //ASSERT
            Assert.IsNotNull(files);
            Assert.AreEqual(0, files.Length);            
        }

        [TestMethod]
        public void DashFileManager_ProtectDeletion()
        {
            //INIT
            var dir = CreateNewDir();
            var manager = new FileManager(dir, 3);

            //ACT
            var fileName1 = manager.GetNextFileName();
            CreateFile(fileName1);

            var files = manager.GetVideoFilesInfo();
            Assert.IsFalse(manager.IsProtected(files.First()));
            
            manager.ProtectDeletion(files.First());

            files = manager.GetVideoFilesInfo();

            var isProtected = manager.IsProtected(files.First());

            //ASSERT
            Assert.IsNotNull(files);
            Assert.AreEqual(1, files.Length);
            Assert.IsTrue(isProtected);

            //CLEANUP
            manager.Cleanup(files.First().FullName);
            Assert.IsFalse(File.Exists(files.First().FullName));
        }

        [TestMethod]
        public void DashFileManager_ProtectDeletionWithCoFiles()
        {
            //INIT
            var dir = CreateNewDir();
            var manager = new FileManager(dir, 3);

            //ACT
            var fileName1 = manager.GetNextFileName();
            CreateFile(fileName1);
            var coFileName = CreateCoFile(fileName1);

            var protectedFileInfo = manager.ProtectDeletion(manager.GetVideoFilesInfo().First());

            var isProtected = manager.IsProtected(protectedFileInfo);

            var withCoFiles = manager.GetWithCoFiles(protectedFileInfo.FullName);

            //ASSERT
            Assert.IsNotNull(withCoFiles);
            Assert.AreEqual(2, withCoFiles.Length);
            Assert.IsTrue(isProtected);
            Assert.IsTrue(withCoFiles.All(f => File.Exists(f)));

            //CLEANUP
            manager.Cleanup(protectedFileInfo.FullName);
            Assert.IsTrue(withCoFiles.All(f => !File.Exists(f)));
        }
    }
}

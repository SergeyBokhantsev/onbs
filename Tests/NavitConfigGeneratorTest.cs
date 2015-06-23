using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class NavitConfigGeneratorTest
    {
        [TestMethod]
        public void NavitConfigGenerate()
        {
            //INIT
            var config = new NavitConfigGenerator.NavitConfiguration();

            config.Autozoom = true;
            config.Center = new Interfaces.GPS.GeoPoint(30.5, 50.5);
            config.KeepNorthOrient = true;
            config.Toolbar = true;

            //ACT
            config.WriteConfig("Data/NavitConfig/template.xml", "Data/NavitConfig/config.xml");

            //ASSERT
            Assert.IsTrue(File.Exists("Data/NavitConfig/config.xml"));
            
            var cfgFile = File.ReadAllText("Data/NavitConfig/config.xml");
            Assert.IsTrue(cfgFile.Contains("center=\"50.5 30.5\""));
            Assert.IsTrue(cfgFile.Contains("autozoom_active=\"1\""));
            Assert.IsTrue(cfgFile.Contains("menubar=\"0\""));
            Assert.IsTrue(cfgFile.Contains("toolbar=\"1\""));
            Assert.IsTrue(cfgFile.Contains("statusbar=\"0\""));
        }
    }
}

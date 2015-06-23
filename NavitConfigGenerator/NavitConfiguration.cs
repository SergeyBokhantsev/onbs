using Interfaces.GPS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NavitConfigGenerator
{
    public class NavitConfiguration
    {
        private class Pl
        {
            public const string Center = "{center}";
            public const string KeepNorthOrient = "{orientation}";
            public const string Autozoom = "{autozoom_active}";
            public const string Menubar = "{menubar}";
            public const string Toolbar = "{toolbar}";
            public const string Statusbar = "{statusbar}";
            public const string GPSActive = "{active}";
        }

        public GeoPoint Center
        {
            get;
            set;
        }

        public bool KeepNorthOrient
        {
            get;
            set;
        }

        public bool Autozoom
        {
            get;
            set;
        }

        public bool Menubar
        {
            get;
            set;
        }

        public bool Toolbar
        {
            get;
            set;
        }

        public bool Statusbar
        {
            get;
            set;
        }

        public bool GPSActive
        {
            get;
            set;
        }

        public void WriteConfig(string templateFileName, string outFileName)
        {
            if (!File.Exists(templateFileName))
                throw new Exception(string.Format("Navit Template file is not exists: '{0}'", templateFileName));

            var template = File.ReadAllLines(templateFileName);

            File.WriteAllLines(outFileName, ProcessLines(template));
        }

        private IEnumerable<string> ProcessLines(string[] template)
        {
            foreach (var line in template)
            {
                if (line.Contains('{'))
                {
                    var ret = line;

                    if (line.Contains(Pl.Center))
                        ret = ret.Replace(Pl.Center, string.Concat(Center.Lon.Degrees.ToString(), " ", Center.Lat.Degrees.ToString()));

                    if (line.Contains(Pl.KeepNorthOrient))
                        ret = ret.Replace(Pl.KeepNorthOrient, KeepNorthOrient ? "0" : "-1");

                    if (line.Contains(Pl.Autozoom))
                        ret = ret.Replace(Pl.Autozoom, Autozoom ? "1" : "0");

                    if (line.Contains(Pl.Menubar))
                        ret = ret.Replace(Pl.Menubar, Menubar ? "1" : "0");

                    if (line.Contains(Pl.Statusbar))
                        ret = ret.Replace(Pl.Statusbar, Statusbar ? "1" : "0");

                    if (line.Contains(Pl.Toolbar))
                        ret = ret.Replace(Pl.Toolbar, Toolbar ? "1" : "0");

                    if (line.Contains(Pl.GPSActive))
                        ret = ret.Replace(Pl.GPSActive, GPSActive ? "1" : "0");

                    yield return ret;
                }
                else
                    yield return line;
            }
        }
    }
}

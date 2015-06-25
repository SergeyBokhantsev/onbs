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
            public const string GPSActive = "{gps_active}";
            public const string Zoom = "{zoom}";
            public const string LockOnRoad = "{tracking}";

            public const string OSDCompass = "{osd_compass}";
            public const string OSDETA = "{osd_eta}";
            public const string OSDNavigationDistanceToTarget = "{osd_navigation_distance_to_target}";
            public const string OSDNavigation = "{osd_navigation}";
            public const string OSDNavigationDistanceToNext = "{osd_navigation_distance_to_next}";
            public const string OSDNavigationNextTurn = "{osd_navigation_next_turn}";
        }

        public bool OSDCompass
        {
            get;
            set;
        }

        public bool OSDETA
        {
            get;
            set;
        }

        public bool OSDNavigationDistanceToTarget
        {
            get;
            set;
        }

        public bool OSDNavigation
        {
            get;
            set;
        }

        public bool OSDNavigationDistanceToNext
        {
            get;
            set;
        }

        public bool OSDNavigationNextTurn
        {
            get;
            set;
        }

        public bool LockOnRoad
        {
            get;
            set;
        }

        public int Zoom
        {
            get;
            set;
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

                    if (line.Contains(Pl.OSDCompass))
                        ret = ret.Replace(Pl.OSDCompass, OSDCompass ? "yes" : "no");

                    if (line.Contains(Pl.OSDETA))
                        ret = ret.Replace(Pl.OSDETA, OSDETA ? "yes" : "no");

                    if (line.Contains(Pl.OSDNavigation))
                        ret = ret.Replace(Pl.OSDNavigation, OSDNavigation ? "yes" : "no");

                    if (line.Contains(Pl.OSDNavigationDistanceToNext))
                        ret = ret.Replace(Pl.OSDNavigationDistanceToNext, OSDNavigationDistanceToNext ? "yes" : "no");

                    if (line.Contains(Pl.OSDNavigationDistanceToTarget))
                        ret = ret.Replace(Pl.OSDNavigationDistanceToTarget, OSDNavigationDistanceToTarget ? "yes" : "no");

                    if (line.Contains(Pl.OSDNavigationNextTurn))
                        ret = ret.Replace(Pl.OSDNavigationNextTurn, OSDNavigationNextTurn ? "yes" : "no");

                    if (line.Contains(Pl.Zoom))
                        ret = ret.Replace(Pl.Zoom, Zoom.ToString());

                    if (line.Contains(Pl.LockOnRoad))
                        ret = ret.Replace(Pl.LockOnRoad, LockOnRoad ? "1" : "0");

					if (line.Contains (Pl.Center))
						ret = ret.Replace (Pl.Center, "4808 N 1134 E");
					    //string.Concat(Center.Lon.Degrees.ToString(), " ", Center.Lat.Degrees.ToString()));

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

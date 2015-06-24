using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ISessionConfig
    {
        bool IsSystemTimeValid { get; }
    }

    public interface IConfig : ISessionConfig
    {
        string GetString(string name);
        int GetInt(string name);
        double GetDouble(string name);
        bool GetBool(string name);
        void Set<T>(string name, T value);
        void Save();
    }

    public static class ConfigNames
    {
        public static readonly string UIHostAssemblyName = "UIHostAssemblyName";
        public static readonly string UIHostClass = "UIHostClass";

        public static readonly string GPSLocation = "GPSLocation";

        public static readonly string GPSDEnabled = "GPSDEnabled";

        public static readonly string ArduinoPortFake = "ArduinoPortFake";
        public static readonly string ArduinoPort = "ArduinoPort";
        public static readonly string ArduinoPortEnabled = "ArduinoPortEnabled";
        public static readonly string ArduinoPortSpeed = "ArduinoPortSpeed";
        public static readonly string ArduinoPortParity = "ArduinoPortParity";
        public static readonly string ArduinoPortDataBits = "ArduinoPortDataBits";
        public static readonly string ArduinoPortStopBits = "ArduinoPortStopBits";

        public static readonly string LogLevel = "LogLevel";
        public static readonly string LoggedClasses = "LoggedClasses";
        public static readonly string LogFolder = "LogFolder";

        public static readonly string SystemTimeMinDifference = "SystemTimeMinDifference";
        public static readonly string SystemTimeSetCommand = "SystemTimeSetCommand";
        public static readonly string SystemTimeSetArgs = "SystemTimeSetArgs";
        public static readonly string SystemTimeSetFormat = "SystemTimeSetFormat";
        public static readonly string SystemTimeValidByDefault = "SystemTimeValidByDefault";

        public static readonly string SystemShutdownCommand = "SystemShutdownCommand";
        public static readonly string SystemShutdownArg = "SystemShutdownArg";
        public static readonly string SystemRestartCommand = "SystemRestartCommand";
        public static readonly string SystemRestartArg = "SystemRestartArg";

        public static readonly string NavitExe = "navit_exe";
        public static readonly string NavitArgs = "navit_args";
        public static readonly string NavitConfigTemplatePath = "navit_config_template";
        public static readonly string NavitConfigPath = "navit_config_outfile";
        public static readonly string NavitKeepNorth = "navit_keep_north";
        public static readonly string NavitAutozoom = "navit_autozoom";
        public static readonly string NavitMenubar = "navit_menubar";
        public static readonly string NavitToolbar = "navit_toolbar";
        public static readonly string NavitStatusbar = "navit_statusbar";
        public static readonly string NavitGPSActive = "navit_gps_active";
    }
}

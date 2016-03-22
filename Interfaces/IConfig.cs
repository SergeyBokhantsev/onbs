using System;
namespace Interfaces
{
    public enum Environments { Win, RPi };

    public interface ISessionConfig
    {
        Environments Environment { get; }
        bool IsSystemTimeValid { get; }
        bool IsInternetConnected { get; }
        bool IsGPSLock { get; }
        bool IsMessagePending { get; }
        bool IsMessageShown { get; }
        string DataFolder { get; }
    }

    public interface IConfig : ISessionConfig
    {
        event Action<string> Changed;

        string GetString(string name);
        int GetInt(string name);
        double GetDouble(string name);
        bool GetBool(string name);
        void Set<T>(string name, T value);
        void Save();
    }

    public static class ConfigNames
    {
        public static readonly string Placeholder_Elm327Port = "ELM327PORT";
        public static readonly string Placeholder_UIFullscreen = "FULLSCREEN";
        public static readonly string Placeholder_Vehicle = "VEHICLE";
        public static readonly string Placeholder_ArduinoConfirmationTimeout = "COMM_CONFIRMATION_TIMEOUT";
        

        public static readonly string UIHostAssemblyName = "UIHostAssemblyName";
        public static readonly string UIHostClass = "UIHostClass";
        public static readonly string UIFullscreen = "UIFullscreen";

		public static readonly string XScreenForceOn = "XScreenForceOn";

        public static readonly string GPSLocation = "GPSLocation";

        public static readonly string GPSDEnabled = "GPSDEnabled";
        public static readonly string GPSDPaused = "GPSDPaused";

        public static readonly string Elm327Port = "Elm327Port";

        public static readonly string ArduinoPort = "ArduinoPort";
        public static readonly string ArduinoPortEnabled = "ArduinoPortEnabled";
        public static readonly string ArduinoPortSpeed = "ArduinoPortSpeed";
        public static readonly string ArduinoPortParity = "ArduinoPortParity";
        public static readonly string ArduinoPortDataBits = "ArduinoPortDataBits";
        public static readonly string ArduinoPortStopBits = "ArduinoPortStopBits";

        public static readonly string ArduinoConfirmationTimeout = "ArduinoConfirmationTimeout";

        public static readonly string LogLevel = "LogLevel";
        public static readonly string LoggedClasses = "LoggedClasses";
        public static readonly string LogFolder = "LogFolder";

        public static readonly string DashCamRecorderEnabled = "DashCamRecorderEnabled";
        public static readonly string DashCamRecorderPreviewEnabled = "DashCamRecorderPreviewEnabled";
        public static readonly string DashCamRecorderExe = "DashCamRecorderExe";
        public static readonly string DashCamRecorderArg = "DashCamRecorderArg";
        public static readonly string DashCamRecorderFolder = "DashCamRecorderFolder";
        public static readonly string DashCamRecorderFilesNumberQuota = "DashCamRecorderFilesNumberQuota";
        public static readonly string DashCamRecorderSplitIntervalSec = "DashCamRecorderSplitIntervalSec";

        public static readonly string DashCamPlayerExe = "DashCamPlayerExe";
        public static readonly string DashCamPlayerArg = "DashCamPlayerArg";

        public static readonly string DashCamConvertToMP4Exe = "DashCamConvertToMP4Exe";
        public static readonly string DashCamConvertToMP4Arg = "DashCamConvertToMP4Arg";

        public static readonly string SystemTimeLocalZone = "SystemTimeLocalZone";
        public static readonly string SystemTimeMinDifference = "SystemTimeMinDifference";
        public static readonly string SystemTimeSetCommand = "SystemTimeSetCommand";
        public static readonly string SystemTimeSetArgs = "SystemTimeSetArgs";
        public static readonly string SystemTimeSetFormat = "SystemTimeSetFormat";
        public static readonly string SystemTimeValidByDefault = "SystemTimeValidByDefault";

        public static readonly string SystemShutdownCommand = "SystemShutdownCommand";
        public static readonly string SystemShutdownArg = "SystemShutdownArg";
        public static readonly string SystemRestartCommand = "SystemRestartCommand";
        public static readonly string SystemRestartArg = "SystemRestartArg";
        public static readonly string SystemUpdateCommand = "SystemUpdateCommand";
        public static readonly string SystemUpdateArg = "SystemUpdateArg";

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
        public static readonly string NavitZoom = "navit_zoom";
        public static readonly string NavitLockOnRoad = "navit_lock_on_road";
        public static readonly string NavitOSDCompass = "navit_osd_compass";
        public static readonly string NavitOSDETA = "navit_osd_eta";
        public static readonly string NavitOSDNavigationDistanceToTarget = "navit_osd_navigation_distance_to_target";
        public static readonly string NavitOSDNavigation = "navit_osd_navigation";
        public static readonly string NavitOSDNavigationDistanceToNext = "navit_osd_navigation_distance_to_next";
        public static readonly string NavitOSDNavigationNextTurn = "navit_osd_navigation_next_turn";

        public static readonly string TravelServiceUrl = "TravelServiceUrl";
        public static readonly string TravelServiceVehicle = "TravelServiceVehicle";
        public static readonly string TravelServiceKey = "TravelServiceKey";
        public static readonly string TravelServiceExportRateSeconds = "TravelServiceExportRateSeconds";
        public static readonly string TravelServiceMinutesGapToOpenNewTravel = "TravelServiceMinutesGapToOpenNewTravel";
        public static readonly string TravelServiceMinPointsCountToStart = "TravelServiceMinPointsCountToStart";
        public static readonly string TravelServiceGPSFilterDistanceToSpeedRatio = "TravelServiceGPSFilterDistanceToSpeedRatio";
        public static readonly string TravelServiceGPSFilterDeadZoneMeters = "TravelServiceGPSFilterDeadZoneMeters";
        public static readonly string TravelServiceGPSFilterDeadZoneSpeed = "TravelServiceGPSFilterDeadZoneSpeed";
        
        public static readonly string InetKeeperEnabled = "InetKeeperEnabled";
        public static readonly string InetRestartIfNoConnectEnabled = "InetRestartIfNoConnectEnabled";
        public static readonly string InetRestartIfNoConnectMinutes = "InetRestartIfNoConnectMinutes";
        public static readonly string InetKeeperCheckUrl = "InetKeeperCheckUrl";
        public static readonly string InetKeeperCheckMethod = "InetKeeperCheckMethod";
        public static readonly string InetKeeperCheckFolder = "InetKeeperCheckFolder";

        public static readonly string Modem_vid = "Modem_vid";
        public static readonly string Modem_storagemode_pid = "Modem_storagemode_pid";
        public static readonly string Modem_modemmode_pid = "Modem_modemmode_pid";

        public static readonly string WeatherCityId = "WeatherCityId";

        public static readonly string OBD_DTC_DescriptionFiles = "OBD_DTC_DescriptionFiles";
    }
}

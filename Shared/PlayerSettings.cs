using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace RDRNetworkShared
{
    public class PlayerSettings
    {
        public string DisplayName { get; set; }
        public string MasterServerAddress { get; set; }
        public List<string> FavoriteServers { get; set; }
        public List<string> RecentServers { get; set; }
        public bool ScaleChatWithSafezone { get; set; }
        public string UpdateChannel { get; set; }
        public bool DisableRockstarEditor { get; set; }
       // public Keys ScreenshotKey { get; set; }
        public bool ShowFPS { get; set; }
        public bool DisableCEF { get; set; }
        public bool Timestamp { get; set; }
        public bool Militarytime { get; set; }
        //public bool AutosetBorderlessWindowed { get; set; }
        public bool UseClassicChat { get; set; }
        public bool OfflineMode { get; set; }
        public bool MediaStream { get; set; }
        public bool CEFDevtool { get; set; }
        public bool DebugMode { get; set; }

        public int ChatboxXOffset { get; set; }
        public int ChatboxYOffset { get; set; }

        public string GamePath { get; set; }

        public PlayerSettings()
        {
            MasterServerAddress = "http://resump.djoe.ovh";
            FavoriteServers = new List<string>();
            RecentServers = new List<string>();
            ScaleChatWithSafezone = true;
            UpdateChannel = "stable";
            DisableRockstarEditor = true;
            //AutosetBorderlessWindowed = false;
            //ScreenshotKey = Keys.F8;
            UseClassicChat = false;
            ShowFPS = true;
            DisableCEF = false;
            Timestamp = false;
            Militarytime = true;
            OfflineMode = false;
            MediaStream = false;
            CEFDevtool = false;
            DebugMode = false;
            GamePath = "";
        }

        public static PlayerSettings ReadSettings(string path)
        {
            var ser = new XmlSerializer(typeof(PlayerSettings));

            PlayerSettings settings = new PlayerSettings();

            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path)) settings = (PlayerSettings)ser.Deserialize(stream);
            }

            return settings;
        }

        public static void SaveSettings(string path, PlayerSettings set)
        {
            var ser = new XmlSerializer(typeof(PlayerSettings));
            if (File.Exists(path))
            {
                using (var stream = new FileStream(path, FileMode.Truncate)) ser.Serialize(stream, set);
            }
            else
            {
                using (var stream = new FileStream(path, FileMode.Create)) ser.Serialize(stream, set);
            }
        }
    }
}

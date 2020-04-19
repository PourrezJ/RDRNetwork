using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDRNetwork
{
    internal partial class Main
    {
        internal static List<string> InternetList = new List<string>();

        private static void AddServerToRecent(string server, string password = "")
        {
            if (string.IsNullOrWhiteSpace(server)) return;
            var split = server.Split(':');
            if (split.Length < 2 || string.IsNullOrWhiteSpace(split[0]) || string.IsNullOrWhiteSpace(split[1]) || !int.TryParse(split[1], out int tmpPort)) return;
            if (PlayerSettings.RecentServers.Contains(server)) return;

            PlayerSettings.RecentServers.Add(server);
            if (PlayerSettings.RecentServers.Count > 10) PlayerSettings.RecentServers.RemoveAt(0);
            RDRNetworkShared.PlayerSettings.SaveSettings(RDRNetworkFolder + "\\settings.xml", PlayerSettings);

            /*
            var item = new UIMenuItem(server) { Description = server, Text = server };
            item.Activated += (sender, selectedItem) =>
            {
                if (IsOnServer())
                {
                    Client.Disconnect("Switching servers");
                    NetEntityHandler.ClearAll();

                    if (Npcs != null)
                    {
                        Npcs.ToList().ForEach(pair => pair.Value.Clear());
                        Npcs.Clear();
                    }

                    while (IsOnServer()) Script.Yield();
                }

                var splt = server.Split(':');
                if (splt.Length < 2) return;
                int port;
                if (!int.TryParse(splt[1], out port)) return;
                ConnectToServer(splt[0], port, false, password);
                MainMenu.TemporarilyHidden = true;
                _connectTab.RefreshIndex();
            };
            _recentBrowser.Items.Add(item);*/
        }
    }
}

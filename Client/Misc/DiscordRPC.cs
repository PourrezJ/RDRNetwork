
using DiscordRPC;

namespace RDRNetwork.Misc
{
    public static class DiscordRPC
    {
        public static DiscordRpcClient InMenuDiscordClient;
        public static DiscordRpcClient OnServerDiscordClient;

        #region InMenu
        public static void InMenuDiscordInitialize(string version)
        {
            InMenuDiscordClient = new DiscordRpcClient("644509937283891211"); 

            var timer = new System.Timers.Timer(1507665886);
            timer.Elapsed += (sender, args) => { InMenuDiscordClient.Invoke(); };
            timer.Start();

            // DiscordClient.UpdateStartTime();

            InMenuDiscordClient.Initialize();

            InMenuDiscordClient.SetPresence(new RichPresence()
            {
                State = "In the menu",
                Details = "Dev Build | Ver: " + version,
                Assets = new Assets()
                {
                    LargeImageKey = "logo",
                    LargeImageText = "logo"
                }
            });
        }

        public static void InMenuDiscordUpdatePresence()
        {
            if(InMenuDiscordClient != null)
            {

            }
            InMenuDiscordClient.Invoke();
            InMenuDiscordClient.UpdateStartTime();
        }

        public static void InMenuDiscordDeinitializePresence()
        {
            InMenuDiscordClient?.Dispose();
        }

        #endregion

        public static void OnServerDiscordInitialize(string PlayerName, string ServerName)
        {
            OnServerDiscordClient = new DiscordRpcClient("644509937283891211");

            var timer = new System.Timers.Timer(1507665886);
            timer.Elapsed += (sender, args) => { InMenuDiscordClient.Invoke(); };
            timer.Start();

            // DiscordClient.UpdateStartTime();

            OnServerDiscordClient.Initialize();

            OnServerDiscordClient.SetPresence(new RichPresence()
            {
                State = "Name: " + PlayerName,
                Details = "Server: " + ServerName,
                Assets = new Assets()
                {
                    LargeImageKey = "logo",
                    LargeImageText = "logo"
                }
            });
        }

        public static void OnServerDiscordUpdatePresence()
        {
            if (OnServerDiscordClient != null)
            {
                OnServerDiscordClient.Invoke();
                OnServerDiscordClient.UpdateStartTime();
            }
        }

        public static void OnServerDiscordDeinitializePresence()
        {
            OnServerDiscordClient?.Dispose();
        }

    }
}
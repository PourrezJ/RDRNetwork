using ResuMPServer;
using System;
using System.Threading.Tasks;

namespace Freeroam
{
    public class Main : Script
    {
        public Main()
        {
            API.OnPlayerBeginConnect += API_OnPlayerBeginConnect;
            API.OnPlayerConnected += API_OnPlayerConnected;
            API.OnUpdate += API_OnUpdate;
        }

        private void API_OnPlayerBeginConnect(Client player, CancelEventArgs cancelConnection)
        {
            Console.WriteLine(player.SocialClubName + " begin connecte");
        }

        private Task API_OnPlayerConnected(Client player)
        {
            Console.WriteLine(player.SocialClubName + " is connected");
            player.Position = new RDRNetworkShared.Vector3(35.0f, 35.0f, 102.0f);
            return Task.CompletedTask;
        }

        private void API_OnUpdate()
        {
           
        }
    }
}

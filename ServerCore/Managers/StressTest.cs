using RDRNetworkShared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ResuMPServer
{
    internal static class StressTest
    {
        public static int PlayersToSim = 500;
        public static bool HasPlayers;
        public static List<Client> Players = new List<Client>();

        public static void Init()
        {
            Thread t = new Thread(Pulse);
            t.IsBackground = true;
            t.Start();
        }

        private static Random _randObj = new Random();
        public static string RandomString(int len)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < len; i++)
            {
                sb.Append((char) _randObj.Next(65, 123));
            }

            return sb.ToString();
        }

        public static void CreatePlayer()
        {
            var data = new Client(API.Shared, new NetHandle(Program.ServerInstance.NetEntityHandler.GeneratePedHandle()), null);
            data.NameInternal = RandomString(10);
            data.Fake = true;

            Players.Add(data);
            Program.ServerInstance.Clients.Add(data);

            var delta = new Delta_PlayerProperties();
            delta.Name = data.NameInternal;
            GameServer.UpdateEntityInfo(data.Id.Value, EntityType.Player, delta, data);

            Program.Output("Adding player " + data.NameInternal);
        }

        public static void UpdatePlayer(Client player)
        {
            var data = new PedData();
            
            if (player.PositionInternal == null) player.PositionInternal = Vector3.RandomXY() * 3000f * (float)_randObj.NextDouble();
            data.Position = player.PositionInternal;
            player.LastUpdate = DateTime.Now;

            data.NetHandle = player.Id.Value;
            data.PedArmor = 0;
            data.Flag = 0;
            data.PedModelHash = (int) PedHash.Michael;
            data.PlayerHealth = 100;
            data.Quaternion = new Vector3();
            data.Speed = 0;
            data.Velocity = new Vector3();
            //data.WeaponHash = (int)WeaponHash.Unarmed; désactivé besoin de revoir le cast
            data.Latency = 0.1f;
            
            Program.ServerInstance.ResendPacket(data, player, true);

            if (Environment.TickCount - player.GameVersion > 1500)
            {
                Program.ServerInstance.ResendPacket(data, player, false);
                player.GameVersion = Environment.TickCount;
            }
        }

        private static int _simId = 100;

        public static void Pulse()
        {
            while (true)
            {
                if (HasPlayers && Players.Count < PlayersToSim)
                {
                    CreatePlayer();
                }

                foreach (var client in Players)
                {
                    if (DateTime.Now.Subtract(client.LastUpdate).TotalMilliseconds > 100)
                    {
                        UpdatePlayer(client);
                    }
                }

                Thread.Yield();
            }
        }
    }
}
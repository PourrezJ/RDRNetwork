using RDRNetworkShared;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Lidgren.Network;
using RDR2;
using RDRNetwork.Utils;
using RDR2.Native;
using System.Collections.Generic;
using RDRNetwork.Sync;
using System.IO;

namespace RDRNetwork
{
    partial class Main : Script
    {


        internal static ParseableVersion CurrentVersion = ParseableVersion.FromAssembly(Assembly.GetExecutingAssembly());
        internal static PlayerSettings PlayerSettings;

        internal static int LocalDimension = 0;
        internal static int LocalTeam = -1;
        internal static DateTime LastCarEnter;

        internal static string RDRNetworkFolder = Directory.GetCurrentDirectory();
        internal static bool SaveDebugToFile = true;
        internal static string _threadsafeSubtitle;

        internal static bool VehicleLagCompensation = true;
        internal static bool OnFootLagCompensation = true;
        internal static bool ToggleNametagDraw = false;
        internal static bool TogglePosUpdate = false;

        internal static RelationshipGroup RelGroup;
        internal static RelationshipGroup FriendRelGroup;
        internal static string Weather;
        internal static TimeSpan? Time;

        private static string _customAnimation;
        private static int _animationFlag;

        internal static Dictionary<string, SyncPed> Npcs;

        internal static bool BlockControls;

        public Main()
        {
            //hook::jump(hook::get_pattern("84 C0 74 04 32 C0 EB 0E 4C 8B C7 48 8B D6", -0x1D), ReturnTrueAndForcePedMPFlag); 
            LogManager.CreateLogDirectory();
            LogManager.DebugLog("RDR:Network Starting");
            Npcs = new Dictionary<string, SyncPed>();

            try
            {
                 Streamer.Init();
                

                PlayerSettings = PlayerSettings.ReadSettings("..\\settings.xml");
                
                Time = new TimeSpan(8, 0, 0);
                World.CurrentDayTime = Time.Value;
                // RelGroup = World.AddRelationshipGroup("SYNCPED");
                // FriendRelGroup = World.AddRelationshipGroup("SYNCPED_TEAMMATES");

                // RelGroup.SetRelationshipBetweenGroups(Game.Player.Character.RelationshipGroup, Relationship.Neutral, true);
                // FriendRelGroup.SetRelationshipBetweenGroups(Game.Player.Character.RelationshipGroup, Relationship.Companion, true);


                _config = new NetPeerConfiguration("RDRNETWORK")
                {
                    Port = 8888
                };
                _config.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
                _config.ConnectionTimeout = 30f; // 30 second timeout
                
                Tick += OnTick;
                KeyDown += Main_KeyDown;
                
                Misc.DiscordRPC.OnServerDiscordDeinitializePresence();
                Misc.DiscordRPC.InMenuDiscordInitialize(CurrentVersion.ToString());         
                Misc.DiscordRPC.InMenuDiscordUpdatePresence();
                
            }
            catch(Exception ex)
            {
                LogManager.LogException(ex, "Init");
            }

            LogManager.DebugLog("RDR:Network Started");
        }

        private void Main_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.F5:
                    string ip = InputboxThread.GetUserInput("127.0.0.1", TickSpinner);
                    int port = Main.Port;
                    if (ip.Contains(":"))
                    {
                        string[] data = ip.Split(':');
                        port = Convert.ToInt32(data[1].ToString());
                    }

                    Main.ConnectToServer(ip, port);
                    LogManager.DebugLog(ip);
                    break;
                case System.Windows.Forms.Keys.F6:
                    Client.Disconnect("Quit");
                    OnLocalDisconnect();
                    break;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            Init();           
        }

        private void TickSpinner()
        {
            OnTick(null, EventArgs.Empty);
        }

        private bool _init = false;
        private void Init()
        {
            if (_init) return;
            var player = Game.Player.Character;
            if (player == null || player.Handle == 0 || Game.IsLoading) return;

            Game.FadeScreenOut(0);
           // ResetPlayer();
            _init = true;
            
            //World.RenderingCamera = MainMenuCamera;

            Game.TimeScale = 1;
            /*
            RDR2DN.Log.Message(RDR2DN.Log.Level.Debug, "Changement de model");
            Model model = new Model(PedHash.Player_Zero);
            model.Request();

            Game.Player.ChangeModel(model);
            */
            Function.Call(Hash.SHUTDOWN_LOADING_SCREEN);
            Game.FadeScreenIn(1000);
            Function.Call(Hash.SET_PLAYER_CONTROL, Game.Player.Handle, 1, 0, 0);
        }

        public static void AddMap(ServerMap map)
        {
            try
            {
                Streamer.ServerWorld = map.World;

                if (map.World.LoadedIpl != null)
                    foreach (var ipl in map.World.LoadedIpl)
                    {
                        Function.Call((Hash)0x59767C5A7A9AE6DA, ipl); // Request IPL
                    }

                if (map.World.RemovedIpl != null)
                    foreach (var ipl in map.World.RemovedIpl)
                    {
                        Function.Call((Hash)0x5A3E5CF7B4014B96, ipl); // RemoveIpl
                    }

                if (map.Objects != null)
                    foreach (var pair in map.Objects)
                    {
                        if (Streamer.ClientMap.ContainsKey(pair.Key)) continue;
                        Streamer.CreateObject(pair.Key, pair.Value);
                    }

                if (map.Vehicles != null)
                    foreach (var pair in map.Vehicles)
                    {
                        if (Streamer.ClientMap.ContainsKey(pair.Key)) continue;
                        Streamer.CreateVehicle(pair.Key, pair.Value);
                    }

                if (map.Blips != null)
                {
                    foreach (var blip in map.Blips)
                    {
                        if (Streamer.ClientMap.ContainsKey(blip.Key)) continue;
                        Streamer.CreateBlip(blip.Key, blip.Value);
                    }
                }

                if (map.Markers != null)
                {
                    foreach (var marker in map.Markers)
                    {
                        if (Streamer.ClientMap.ContainsKey(marker.Key)) continue;
                        Streamer.CreateMarker(marker.Key, marker.Value);
                    }
                }

                if (map.Pickups != null)
                {
                    foreach (var pickup in map.Pickups)
                    {
                        if (Streamer.ClientMap.ContainsKey(pickup.Key)) continue;
                        Streamer.CreatePickup(pickup.Key, pickup.Value);
                    }
                }

                if (map.TextLabels != null)
                {
                    foreach (var label in map.TextLabels)
                    {
                        if (Streamer.ClientMap.ContainsKey(label.Key)) continue;
                        Streamer.CreateTextLabel(label.Key, label.Value);
                    }
                }

                if (map.Peds != null)
                {
                    foreach (var ped in map.Peds)
                    {
                        if (Streamer.ClientMap.ContainsKey(ped.Key)) continue;
                        Streamer.CreatePed(ped.Key, ped.Value);
                    }
                }

                if (map.Particles != null)
                {
                    foreach (var ped in map.Particles)
                    {
                        if (Streamer.ClientMap.ContainsKey(ped.Key)) continue;
                        Streamer.CreateParticle(ped.Key, ped.Value);
                    }
                }

                if (map.Players != null)
                {
                    LogManager.DebugLog("STARTING PLAYER MAP");

                    foreach (var pair in map.Players)
                    {
                        if (Streamer.NetToEntity(pair.Key)?.Handle == Game.Player.Character.Handle)
                        {
                            // It's us!
                            var remPl = Streamer.NetToStreamedItem(pair.Key) as RemotePlayer;
                            remPl.Name = pair.Value.Name;
                        }
                        else
                        {
                            var ourSyncPed = Streamer.GetPlayer(pair.Key);
                            Streamer.UpdatePlayer(pair.Key, pair.Value);
                            if (ourSyncPed.Character != null)
                            {
                                //ourSyncPed.Character.RelationshipGroup = (pair.Value.Team == LocalTeam && pair.Value.Team != -1)
                                //    ? Main.FriendRelGroup
                                //    : Main.RelGroup;

                                for (int i = 0; i < 15; i++) //NEEDS A CHECK
                                {
                                   /* Function.Call(Hash.SET_PED_COMPONENT_VARIATION, ourSyncPed.Character, i,
                                        pair.Value.Props.Get((byte)i),
                                        pair.Value.Textures.Get((byte)i), 2);*/
                                }

                                lock (Streamer.HandleMap)
                                    Streamer.HandleMap.Set(pair.Key, ourSyncPed.Character.Handle);

                                ourSyncPed.Character.Alpha = pair.Value.Alpha;
                                Streamer.ReattachAllEntities(ourSyncPed, false);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Client.Disconnect("Map Parse Error");
                LogManager.LogException(ex, "MAP PARSE: FATAL ERROR WHEN PARSING MAP");
                return;
            }

            World.CurrentDayTime = new TimeSpan(map.World.Hours, map.World.Minutes, 00);

            Time = new TimeSpan(map.World.Hours, map.World.Minutes, 00);
            /*
            if (map.World.Weather >= 0 && map.World.Weather < _weather.Length)
            {
                Weather = _weather[map.World.Weather];
                Function.Call(Hash.SET_WEATHER_TYPE_NOW_PERSIST, _weather[map.World.Weather]);
            }*/

            Function.Call(Hash.PAUSE_CLOCK, true);
        }

    }
}

using Lidgren.Network;
using Newtonsoft.Json;
using RDR2;
using RDR2.Native;
using RDR2DN;
using RDRNetwork.Networking;
using RDRNetwork.Sync;
using RDRNetwork.Utils;
using RDRNetwork.Utils.Extentions;
using RDRNetworkShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using EntityType = RDRNetworkShared.EntityType;
using WeaponHash = RDRNetworkShared.WeaponHash;

namespace RDRNetwork
{
    partial class Main
    {
        private static NetPeerConfiguration _config;
        private static int Port = 4499;

        internal static NetClient Client;
        internal static float Latency;
        internal static int _bytesSent = 0;
        internal static int _bytesReceived = 0;

        internal static int _messagesSent = 0;
        internal static int _messagesReceived = 0;

        private static int _currentServerPort;
        private static string _currentServerIp;

        internal static bool JustJoinedServer;
        internal static bool HasFinishedDownloading;

        
        internal static void ConnectToServer(string ip, int port = 0, bool passProtected = false, string myPass = "")
        {
            if (IsOnServer())
            {
                Client.Disconnect("Switching servers");
                //API.Wait(1000);
            }

            if (Client == null)
            {
                var cport = GetOpenUdpPort();
                if (cport == 0)
                {
                    System.Console.WriteLine("No available UDP port was found.");
                    return;
                }
                _config.Port = cport;
                Client = new NetClient(_config);
                Client.Start();
            }

            var msg = Client.CreateMessage();

            var obj = new ConnectionRequest
            {
                SocialClubName = string.IsNullOrWhiteSpace(Game.Player.Name) ? "Unknown" : Game.Player.Name, // To be used as identifiers in server files
                DisplayName = string.IsNullOrWhiteSpace(PlayerSettings.DisplayName) ? Game.Player.Name : PlayerSettings.DisplayName.Trim(),
                ScriptVersion = "1.0.0.0",
                GameVersion = (byte)Game.Version,
            };

            if (passProtected)
            {
                if (!string.IsNullOrWhiteSpace(myPass))
                {
                    obj.Password = myPass;
                }
            }
            
            var bin = SerializeBinary(obj);

            msg.Write((byte)PacketType.ConnectionRequest);
            msg.Write(bin.Length);
            msg.Write(bin);
            try
            {
                Client.Connect(ip, port == 0 ? Port : port, msg);
            }
            catch
            {
                return;
            }
        }

        internal static bool IsOnServer()
        {
            return Client != null && Client.ConnectionStatus == NetConnectionStatus.Connected;
        }

        internal static int GetOpenUdpPort()
        {
            var startingAtPort = 6000;
            var maxNumberOfPortsToCheck = 500;
            var range = Enumerable.Range(startingAtPort, maxNumberOfPortsToCheck);
            var portsInUse =
                from p in range
                join used in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners()
            on p equals used.Port
                select p;

            return range.Except(portsInUse).FirstOrDefault();
        }

        internal static bool IsConnected()
        {
            if (Client == null)
                return false;

            var status = Client.ConnectionStatus;

            return status != NetConnectionStatus.Disconnected && status != NetConnectionStatus.None;
        }

        #region Download stuff
        private static Thread _httpDownloadThread;
        private static bool _cancelDownload;
        private static void StartFileDownload(string address)
        {
            LogManager.DebugLog("StartFileDownload: " + address);
            _cancelDownload = false;
            _httpDownloadThread?.Abort();
            _httpDownloadThread = new Thread((ThreadStart)delegate
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        var manifestJson = wc.DownloadString(address + "/manifest.json");

                        var obj = JsonConvert.DeserializeObject<FileManifest>(manifestJson);

                        wc.DownloadProgressChanged += (sender, args) =>
                        {
                            _threadsafeSubtitle = "Downloading " + args.ProgressPercentage;
                        };

                        foreach (var resource in obj.exportedFiles)
                        {
                            if (!Directory.Exists(FileTransferId._DOWNLOADFOLDER_ + resource.Key))
                                Directory.CreateDirectory(FileTransferId._DOWNLOADFOLDER_ + resource.Key);

                            for (var index = resource.Value.Count - 1; index >= 0; index--)
                            {
                                var file = resource.Value[index];
                                if (file.type == FileType.Script) continue;

                                var target = Path.Combine(FileTransferId._DOWNLOADFOLDER_, resource.Key, file.path);

                                if (File.Exists(target))
                                {
                                    var newHash = DownloadManager.HashFile(target);

                                    if (newHash == file.hash) continue;
                                }

                                wc.DownloadFileAsync(
                                    new Uri($"{address}/{resource.Key}/{file.path}"), target);

                                while (wc.IsBusy)
                                {
                                    Thread.Yield();
                                    if (!_cancelDownload) continue;
                                    wc.CancelAsync();
                                    return;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex, "HTTP FILE DOWNLOAD");
                }
            });
        }

        internal static void InvokeFinishedDownload(List<string> resources)
        {
            var confirmObj = Client.CreateMessage();
            confirmObj.Write((byte)PacketType.ConnectionConfirmed);
            confirmObj.Write(true);
            confirmObj.Write(resources.Count);

            for (int i = 0; i < resources.Count; i++)
            {
                confirmObj.Write(resources[i]);
            }

            Client.SendMessage(confirmObj, NetDeliveryMethod.ReliableOrdered, (int)ConnectionChannel.SyncEvent);

            HasFinishedDownloading = true;
            //Function.Call((Hash)0x10D373323E5B9C0D); //_REMOVE_LOADING_PROMPT
            Function.Call(Hash.DISPLAY_RADAR, true);
        }

        internal static void StartClientsideScripts(ScriptCollection scripts)
        {
            if (scripts.ClientsideScripts == null) return;
            //JavascriptHook.StartScripts(scripts);
        }
        #endregion

        private static void HandleVehiclePacket(VehicleData fullData, bool purePacket)
        {
            if (fullData.NetHandle == null) return;
            var syncPed = Streamer.GetPlayer(fullData.NetHandle.Value);

            syncPed.IsInVehicle = true;

            if (fullData.VehicleHandle != null) LogManager.DebugLog("RECEIVED LIGHT VEHICLE PACKET " + fullData.VehicleHandle);

            if (fullData.Position != null)
            {
                syncPed.Position = fullData.Position.ToVector();
            }

            if (fullData.VehicleHandle != null) syncPed.VehicleNetHandle = fullData.VehicleHandle.Value;
            if (fullData.Velocity != null) syncPed.VehicleVelocity = fullData.Velocity.ToVector();
            if (fullData.PedModelHash != null) syncPed.ModelHash = fullData.PedModelHash.Value;
            if (fullData.PedArmor != null) syncPed.PedArmor = fullData.PedArmor.Value;
            if (fullData.RPM != null) syncPed.VehicleRPM = fullData.RPM.Value;
            if (fullData.Quaternion != null) syncPed.VehicleRotation = fullData.Quaternion.ToVector();
            if (fullData.PlayerHealth != null) syncPed.PedHealth = fullData.PlayerHealth.Value;
            if (fullData.VehicleHealth != null) syncPed.VehicleHealth = fullData.VehicleHealth.Value;
            if (fullData.VehicleSeat != null) syncPed.VehicleSeat = fullData.VehicleSeat.Value;
            if (fullData.Latency != null) syncPed.Latency = fullData.Latency.Value;
            if (fullData.Steering != null) syncPed.SteeringScale = fullData.Steering.Value;
            if (fullData.Velocity != null) syncPed.Speed = fullData.Velocity.ToVector().Length();
            //if (fullData.DamageModel != null && syncPed.MainVehicle != null) syncPed.MainVehicle.SetVehicleDamageModel(fullData.DamageModel);

            if (fullData.Flag != null)
            {
                syncPed.IsVehDead = (fullData.Flag.Value & (short)VehicleDataFlags.VehicleDead) > 0;
                syncPed.IsHornPressed = (fullData.Flag.Value & (short)VehicleDataFlags.PressingHorn) > 0;
                syncPed.Siren = (fullData.Flag.Value & (short)VehicleDataFlags.SirenActive) > 0;
                syncPed.IsShooting = (fullData.Flag.Value & (short)VehicleDataFlags.Shooting) > 0;
                syncPed.IsAiming = (fullData.Flag.Value & (short)VehicleDataFlags.Aiming) > 0;
                syncPed.IsInBurnout = (fullData.Flag.Value & (short)VehicleDataFlags.BurnOut) > 0;
                syncPed.ExitingVehicle = (fullData.Flag.Value & (short)VehicleDataFlags.ExitingVehicle) != 0;
                syncPed.IsPlayerDead = (fullData.Flag.Value & (int)VehicleDataFlags.PlayerDead) != 0;
                syncPed.Braking = (fullData.Flag.Value & (short)VehicleDataFlags.Braking) != 0;
            }

            if (fullData.WeaponHash != null)
            {
                syncPed.CurrentWeapon = fullData.WeaponHash.Value;
            }

            if (fullData.AimCoords != null) syncPed.AimCoords = fullData.AimCoords.ToVector();

            if (syncPed.VehicleNetHandle != 0 && fullData.Position != null)
            {
                var car = Streamer.NetToStreamedItem(syncPed.VehicleNetHandle) as RemoteVehicle;
                if (car != null)
                {
                    car.Position = fullData.Position;
                    car.Rotation = fullData.Quaternion;
                }

            }
            else if (syncPed.VehicleNetHandle != 00 && fullData.Position == null && fullData.Flag != null && !PacketOptimization.CheckBit(fullData.Flag.Value, VehicleDataFlags.Driver))
            {
                var car = Streamer.NetToStreamedItem(syncPed.VehicleNetHandle) as RemoteVehicle;
                if (car != null)
                {
                    syncPed.Position = car.Position.ToVector();
                    syncPed.VehicleRotation = car.Rotation.ToVector();
                }
            }

            if (purePacket)
            {
                syncPed.LastUpdateReceived = Util.TickCount;
                syncPed.StartInterpolation();
            }
        }

        internal static void ProcessMessages(NetIncomingMessage msg, bool safeThreaded)
        {
            PacketType type = PacketType.WorldSharingStop;
            LogManager.DebugLog("RECEIVED MESSAGE " + msg.MessageType);

            switch (msg.MessageType)
            {
                case NetIncomingMessageType.Data:
                    type = (PacketType)msg.ReadByte();
                    ProcessDataMessage(msg, type);
                    break;

                case NetIncomingMessageType.ConnectionLatencyUpdated:
                    Latency = msg.ReadFloat();
                    break;
                case NetIncomingMessageType.StatusChanged:

                    #region StatusChanged
                    var newStatus = (NetConnectionStatus)msg.ReadByte();
                    //LogManager.DebugLog("NEW STATUS: " + newStatus);
                    switch (newStatus)
                    {
                        case NetConnectionStatus.InitiatedConnect:
                            LogManager.DebugLog("Connecting...");
                            /*World.RenderingCamera = null;*/
                            LocalTeam = -1;
                            LocalDimension = 0;
                            ResetPlayer();
                            //CEFManager.Initialize(Main.screen);
                           // StringCache?.Dispose();

                            //StringCache = new StringCache();
                            break;
                        case NetConnectionStatus.Connected:
                            foreach (var i in InternetList)
                            {
                                var spl = i.Split(':');
                                if (_currentServerIp == Dns.GetHostAddresses(spl[0])[0].ToString()) _currentServerIp = spl[0];
                            }
                            AddServerToRecent(_currentServerIp + ":" + _currentServerPort);
                            LogManager.DebugLog("Connection established!");
                            var respLen = msg.SenderConnection.RemoteHailMessage.ReadInt32();
                            var respObj = DeserializeBinary<ConnectionResponse>(msg.SenderConnection.RemoteHailMessage.ReadBytes(respLen)) as ConnectionResponse;

                            if (respObj == null)
                            {
                                LogManager.DebugLog("ERROR WHILE READING REMOTE HAIL MESSAGE");
                                return;
                            }

                            Streamer.AddLocalCharacter(respObj.CharacterHandle);

                            var confirmObj = Client.CreateMessage();
                            confirmObj.Write((byte)PacketType.ConnectionConfirmed);
                            confirmObj.Write(false);
                            Client.SendMessage(confirmObj, NetDeliveryMethod.ReliableOrdered, (int)ConnectionChannel.SyncEvent);
                            JustJoinedServer = true;
                            /*
                            MainMenu.Tabs.Remove(_welcomePage);

                            if (!MainMenu.Tabs.Contains(_serverItem)) MainMenu.Tabs.Insert(0, _serverItem);
                            if (!MainMenu.Tabs.Contains(_mainMapItem)) MainMenu.Tabs.Insert(0, _mainMapItem);

                            MainMenu.RefreshIndex();
                            */
                            if (respObj.Settings != null)
                            {
                                //OnFootLagCompensation = respObj.Settings.OnFootLagCompensation;
                                //VehicleLagCompensation = respObj.Settings.VehicleLagCompensation;

                                /*
                                if (respObj.Settings.ModWhitelist != null)
                                {
                                    if (!DownloadManager.ValidateExternalMods(respObj.Settings.ModWhitelist))
                                    {
                                        Client.Disconnect("");
                                        
                                        MainMenu.Visible = false;
                                        _mainWarning = new Warning("Failed to connect", "Unallowed mods!\nThe server has strictly disallowed the use of non-whitelisted mods.")
                                        {
                                            OnAccept = () => { _mainWarning.Visible = false; MainMenu.Visible = true; }
                                        };
                                    }

                                }*/
                            }

                            if (ParseableVersion.Parse(respObj.ServerVersion) < VersionCompatibility.LastCompatibleServerVersion)
                            {
                                Client.Disconnect("");
                                /*
                                MainMenu.Visible = false;
                                _mainWarning = new Warning("Failed to connect", "Outdated server!\nPlease inform the server administrator of the issue.")
                                {
                                    OnAccept = () => { _mainWarning.Visible = false; }
                                };
                                */
                            }

                            if (respObj.Settings.UseHttpServer)
                            {
                                StartFileDownload($"http://{_currentServerIp}:{_currentServerPort}");

                                if (Main.JustJoinedServer)
                                {
                                    World.RenderingCamera = null;
                                    //Main.MainMenu.TemporarilyHidden = false;
                                    //Main.MainMenu.Visible = false;
                                    Main.JustJoinedServer = false;
                                }
                            }
                            break;
                        case NetConnectionStatus.Disconnected:
                            var reason = msg.ReadString();

                            OnLocalDisconnect();
                            if (!string.IsNullOrEmpty(reason) && reason != "Quit" && reason != "Switching servers")
                            {
                                /*
                                MainMenu.Visible = false;
                                _mainWarning = new Warning("Disconnected", reason)
                                {
                                    OnAccept = () =>
                                    {
                                        _mainWarning.Visible = false;
                                        MainMenu.Visible = true;
                                    }
                                };*/
                            }
                            else
                            {
                                LogManager.DebugLog("Disconnected: " + reason);
                            }
                            break;
                    }
                    break;

                #endregion

                case NetIncomingMessageType.DiscoveryResponse:

                    #region DiscoveryResponse
                    msg.ReadByte();
                    var len = msg.ReadInt32();
                    var bin = msg.ReadBytes(len);
                    var data = DeserializeBinary<DiscoveryResponse>(bin) as DiscoveryResponse;
                    if (data == null) return;

                    var itemText = msg.SenderEndPoint.Address + ":" + data.Port;

                    foreach (var i in InternetList)
                    {
                        var spl = i.Split(':');
                        if (msg.SenderEndPoint.Address.ToString() == Dns.GetHostAddresses(spl[0])[0].ToString()) itemText = i;
                    }

                    var gamemode = Regex.Replace(data.Gamemode, @"(~.*?~|~|'|""|∑|\\|¦)", string.Empty);
                    var name = Regex.Replace(data.ServerName, @"(∑|¦|\\|%|$|^|')", string.Empty);

                    if (string.IsNullOrWhiteSpace(gamemode)) gamemode = "freeroam";
                    if (string.IsNullOrWhiteSpace(name)) name = "Simple GTA Network Server";

                    var map = string.Empty;
                    if (!string.IsNullOrWhiteSpace(data.Map)) map = " (" + Regex.Replace(data.Map, @"(~.*?~|~|<|>|'|""|∑|\\|¦)", string.Empty) + ")";
                    /*
                    var ourItem = new UIMenuItem(itemText) { Description = itemText, Text = name };

                    ourItem.SetRightLabel(gamemode + map + " - " + data.PlayerCount + "/" + data.MaxPlayers);

                    if (PlayerSettings.FavoriteServers.Contains(ourItem.Description)) ourItem.SetRightBadge(UIMenuItem.BadgeStyle.Star);

                    if (data.PasswordProtected) ourItem.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);

                    if (ourItem.Text != itemText && ourItem.Text != ourItem.Description)
                    {
                        var gMsg = msg;
                        ourItem.Activated += (sender, selectedItem) =>
                        {
                            if (IsOnServer())
                            {
                                Client.Disconnect("Switching servers");

                                NetEntityHandler.ClearAll();

                                if (Npcs != null)
                                {
                                    lock (Npcs)
                                    {
                                        for (var index = Npcs.ToList().Count - 1; index >= 0; index--)
                                        {
                                            Npcs.ToList()[index].Value.Clear();
                                        }
                                        Npcs.Clear();
                                    }
                                }
                                while (IsOnServer()) Script.Yield();
                            }
                            var pass = data.PasswordProtected;
                            ConnectToServer(gMsg.SenderEndPoint.Address.ToString(), data.Port, pass);
                            MainMenu.TemporarilyHidden = true;
                            _connectTab.RefreshIndex();
                        };
                    }*/

                    /*
                    if (!_serverBrowser.Items.Contains(ourItem))
                    {
                        if (_serverBrowser.Items.Any(i => i.Description.GetBetween("", ":") == Dns.GetHostAddresses(ourItem.Description.GetBetween("", ":"))[0].ToString())) _serverBrowser.Items.Remove(_serverBrowser.Items.First(i => i.Description.GetBetween("", ":") == Dns.GetHostAddresses(ourItem.Description.GetBetween("", ":"))[0].ToString()));
                        _serverBrowser.Items.Insert(_serverBrowser.Items.Count, ourItem);
                    }
                    if (ListSorting)
                    {
                        try
                        {
                            _serverBrowser.Items = _serverBrowser.Items
                                .OrderByDescending(o => Convert.ToInt32(o.RightLabel.GetBetween(" - ", "/")))
                                .ToList();

                        }
                        catch (FormatException)
                        {
                            //Ignored
                        }
                    }
                    _serverBrowser.RefreshIndex();

                    if (!_Verified.Items.Contains(ourItem) && VerifiedList.Contains(itemText))
                    {
                        _Verified.Items.Insert(_Verified.Items.Count, ourItem);
                    }

                    if (PlayerSettings.FavoriteServers.Contains(itemText))
                    {
                        if (_favBrowser.Items.Any(i => i.Description == ourItem.Description)) _favBrowser.Items.Remove(_favBrowser.Items.FirstOrDefault(i => i.Description == ourItem.Description));
                        _favBrowser.Items.Insert(_favBrowser.Items.Count, ourItem);
                    }

                    if (PlayerSettings.RecentServers.Contains(itemText))
                    {
                        if (_recentBrowser.Items.Any(i => i.Description == ourItem.Description)) _recentBrowser.Items.Remove(_recentBrowser.Items.FirstOrDefault(i => i.Description == ourItem.Description));
                        if (_recentBrowser.Items.Any(i => i.Description.GetBetween("", ":") == Dns.GetHostAddresses(ourItem.Description.GetBetween("", ":"))[0].ToString())) _recentBrowser.Items.Remove(_recentBrowser.Items.FirstOrDefault(i => i.Description.GetBetween("", ":") == Dns.GetHostAddresses(ourItem.Description.GetBetween("", ":"))[0].ToString()));
                        _recentBrowser.Items.Insert(_recentBrowser.Items.Count, ourItem);
                    }

                    if (isIPLocal(msg.SenderEndPoint.Address.ToString()) && !_lanBrowser.Items.Contains(ourItem) && _lanBrowser.Items.All(i => i.Description != ourItem.Description))
                    {
                        _lanBrowser.Items.Insert(_lanBrowser.Items.Count, ourItem);
                    }*/

                    break;

                    #endregion
            }
        }

        private static void ProcessDataMessage(NetIncomingMessage msg, PacketType type)
        {
            #region Data
            //LogManager.DebugLog("RECEIVED DATATYPE " + type);
            switch (type)
            {
                case PacketType.RedownloadManifest:
                    {
                        LogManager.DebugLog("RECEIVED DATATYPE " + type);
                        StartFileDownload(string.Format("http://{0}:{1}", _currentServerIp, _currentServerPort));
                    }
                    break;
                case PacketType.VehiclePureSync:
                    {
                        var len = msg.ReadInt32();
                        var data = msg.ReadBytes(len);
                        var packet = PacketOptimization.ReadPureVehicleSync(data);
                        HandleVehiclePacket(packet, true);
                    }
                    break;
                case PacketType.VehicleLightSync:
                    {
                        var len = msg.ReadInt32();
                        var data = msg.ReadBytes(len);
                        var packet = PacketOptimization.ReadLightVehicleSync(data);
                        //LogManager.DebugLog("RECEIVED LIGHT VEHICLE PACKET");
                        HandleVehiclePacket(packet, false);
                    }
                    break;
                case PacketType.PedPureSync:
                    {
                        var len = msg.ReadInt32();
                        var data = msg.ReadBytes(len);
                        var packet = PacketOptimization.ReadPurePedSync(data);
                        //HandlePedPacket(packet, true);
                    }
                    break;
                case PacketType.PedLightSync:
                    {
                        var len = msg.ReadInt32();
                        var data = msg.ReadBytes(len);
                        var packet = PacketOptimization.ReadLightPedSync(data);
                       // HandlePedPacket(packet, false);
                    }
                    break;
                case PacketType.BasicSync:
                    {
                        var len = msg.ReadInt32();
                        var data = msg.ReadBytes(len);

                        LogManager.DebugLog("BASICSYNC - " + data + " | " + len);
                        foreach (var value in data)
                            LogManager.DebugLog("BASICSYNC FOR - " + value);

                        PacketOptimization.ReadBasicSync(data, out int nethandle, out RDRNetworkShared.Vector3 position);

                        //HandleBasicPacket(nethandle, position.ToVector());
                    }
                    break;
                case PacketType.BulletSync:
                    {
                        //Util.Util.SafeNotify("Bullet Packet" + DateTime.Now.Millisecond);
                        var len = msg.ReadInt32();
                        var data = msg.ReadBytes(len);

                        var shooting = PacketOptimization.ReadBulletSync(data, out int nethandle, out RDRNetworkShared.Vector3 position);

                        //HandleBulletPacket(nethandle, shooting, position.ToVector());
                    }
                    break;
                case PacketType.BulletPlayerSync:
                    {
                        //Util.Util.SafeNotify("Bullet Player Packet" + DateTime.Now.Millisecond);
                        var len = msg.ReadInt32();
                        var data = msg.ReadBytes(len);

                        var shooting = PacketOptimization.ReadBulletSync(data, out int nethandle, out int nethandleTarget);
                       // HandleBulletPacket(nethandle, shooting, nethandleTarget);
                    }
                    break;
                case PacketType.UnoccupiedVehStartStopSync:
                    {
                        var veh = msg.ReadInt32();
                        var startSyncing = msg.ReadBoolean();

                        if (startSyncing)
                        {
                            //VehicleSyncManager.StartSyncing(veh);
                        }
                        else
                        {
                           // VehicleSyncManager.StopSyncing(veh);
                        }
                    }
                    break;
                case PacketType.UnoccupiedVehSync:
                    {
                        var len = msg.ReadInt32();
                        var bin = msg.ReadBytes(len);
                        var data = PacketOptimization.ReadUnoccupiedVehicleSync(bin);

                        if (data != null)
                        {
                         //   HandleUnoccupiedVehicleSync(data);
                        }
                    }
                    break;
                case PacketType.BasicUnoccupiedVehSync:
                    {
                        var len = msg.ReadInt32();
                        var bin = msg.ReadBytes(len);
                        var data = PacketOptimization.ReadBasicUnoccupiedVehicleSync(bin);

                        if (data != null)
                        {
                            //HandleUnoccupiedVehicleSync(data);
                        }
                    }
                    break;
                case PacketType.NpcVehPositionData:
                    {
                        var len = msg.ReadInt32();
                        var data = DeserializeBinary<VehicleData>(msg.ReadBytes(len)) as VehicleData;
                        if (data == null) return;
                        /*
                        lock (Npcs)
                        {
                            if (!Npcs.ContainsKey(data.Name))
                            {
                                var repr = new SyncPed(data.PedModelHash, data.Position.ToVector(),
                                    //data.Quaternion.ToQuaternion(), false);
                                    data.Quaternion.ToVector(), false);
                                Npcs.Add(data.Name, repr);
                                Npcs[data.Name].Name = "";
                                Npcs[data.Name].Host = data.Id;
                            }
                            if (Npcs[data.Name].Character != null)
                                NetEntityHandler.SetEntity(data.NetHandle, Npcs[data.Name].Character.Handle);

                            Npcs[data.Name].LastUpdateReceived = DateTime.Now;
                            Npcs[data.Name].VehiclePosition =
                                data.Position.ToVector();
                            Npcs[data.Name].ModelHash = data.PedModelHash;
                            Npcs[data.Name].VehicleHash =
                                data.VehicleModelHash;
                            Npcs[data.Name].VehicleRotation =
                                data.Quaternion.ToVector();
                            //data.Quaternion.ToQuaternion();
                            Npcs[data.Name].PedHealth = data.PlayerHealth;
                            Npcs[data.Name].VehicleHealth = data.VehicleHealth;
                            //Npcs[data.Name].VehiclePrimaryColor = data.PrimaryColor;
                            //Npcs[data.Name].VehicleSecondaryColor = data.SecondaryColor;
                            Npcs[data.Name].VehicleSeat = data.VehicleSeat;
                            Npcs[data.Name].IsInVehicle = true;

                            Npcs[data.Name].IsHornPressed = data.IsPressingHorn;
                            Npcs[data.Name].Speed = data.Speed;
                            Npcs[data.Name].Siren = data.IsSirenActive;
                        }*/
                    }
                    break;
                case PacketType.ConnectionPacket:
                    {
                        LogManager.DebugLog("RECEIVED DATATYPE " + type);
                        var len = msg.ReadInt32();
                        var data = DeserializeBinary<PedData>(msg.ReadBytes(len)) as PedData;
                        if (data == null) return;
                    }
                    break;
                case PacketType.CreateEntity:
                    {
                        var len = msg.ReadInt32();
                        //LogManager.DebugLog("Received CreateEntity");
                        if (DeserializeBinary<CreateEntity>(msg.ReadBytes(len)) is CreateEntity data && data.Properties != null)
                        {
                            switch (data.EntityType)
                            {
                                case (byte)EntityType.Vehicle:
                                    {
                                        Streamer.CreateVehicle(data.NetHandle, (VehicleProperties)data.Properties);
                                    }
                                    break;
                                case (byte)EntityType.Prop:
                                    {
                                        Streamer.CreateObject(data.NetHandle, data.Properties);
                                    }
                                    break;
                                case (byte)EntityType.Blip:
                                    {
                                        Streamer.CreateBlip(data.NetHandle, (BlipProperties)data.Properties);
                                    }
                                    break;
                                case (byte)EntityType.Marker:
                                    {
                                        Streamer.CreateMarker(data.NetHandle, (MarkerProperties)data.Properties);
                                    }
                                    break;
                                case (byte)EntityType.Pickup:
                                    {
                                        Streamer.CreatePickup(data.NetHandle, (PickupProperties)data.Properties);
                                    }
                                    break;
                                case (byte)EntityType.TextLabel:
                                    {
                                        Streamer.CreateTextLabel(data.NetHandle, (TextLabelProperties)data.Properties);
                                    }
                                    break;
                                case (byte)EntityType.Ped:
                                    {
                                        Streamer.CreatePed(data.NetHandle, data.Properties as PedProperties);
                                    }
                                    break;
                                case (byte)EntityType.Particle:
                                    {
                                        var ped = Streamer.CreateParticle(data.NetHandle, data.Properties as ParticleProperties);
                                        if (Streamer.Count(typeof(RemoteParticle)) < StreamerThread.MAX_PARTICLES) Streamer.StreamIn(ped);
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case PacketType.UpdateEntityProperties:
                    {
                        var len = msg.ReadInt32();
                        var data = DeserializeBinary<UpdateEntity>(msg.ReadBytes(len)) as UpdateEntity;
                        if (data != null && data.Properties != null)
                        {
                            switch ((EntityType)data.EntityType)
                            {
                                case EntityType.Blip:
                                    Streamer.UpdateBlip(data.NetHandle, data.Properties as Delta_BlipProperties);
                                    break;
                                /*case EntityType.Marker:
                                    Streamer.UpdateMarker(data.NetHandle, data.Properties as Delta_MarkerProperties);
                                    break;*/
                                case EntityType.Player:
                                    Streamer.UpdatePlayer(data.NetHandle, data.Properties as Delta_PlayerProperties);
                                    break;
                                case EntityType.Pickup:
                                    Streamer.UpdatePickup(data.NetHandle, data.Properties as Delta_PickupProperties);
                                    break;
                                case EntityType.Prop:
                                    Streamer.UpdateProp(data.NetHandle, data.Properties as Delta_EntityProperties);
                                    break;
                                case EntityType.Vehicle:
                                    Streamer.UpdateVehicle(data.NetHandle, data.Properties as Delta_VehicleProperties);
                                    break;
                                case EntityType.Ped:
                                    Streamer.UpdatePed(data.NetHandle, data.Properties as Delta_PedProperties);
                                    break;
                                case EntityType.TextLabel:
                                    Streamer.UpdateTextLabel(data.NetHandle, data.Properties as Delta_TextLabelProperties);
                                    break;
                                case EntityType.Particle:
                                    Streamer.UpdateParticle(data.NetHandle, data.Properties as Delta_ParticleProperties);
                                    break;
                                case EntityType.World:
                                    Streamer.UpdateWorld(data.Properties);
                                    break;
                            }
                        }
                    }
                    break;
                case PacketType.DeleteEntity:
                    {
                        var len = msg.ReadInt32();
                        var data = DeserializeBinary<DeleteEntity>(msg.ReadBytes(len)) as DeleteEntity;
                        if (data != null)
                        {
                            LogManager.DebugLog("RECEIVED DELETE ENTITY " + data.NetHandle);

                            var streamItem = Streamer.NetToStreamedItem(data.NetHandle);
                            if (streamItem != null)
                            {
                              //  VehicleSyncManager.StopSyncing(data.NetHandle);
                                Streamer.Remove(streamItem);
                                Streamer.StreamOut(streamItem);
                            }
                        }
                    }
                    break;
                case PacketType.StopResource:
                    {
                        var resourceName = msg.ReadString();
                        //JavascriptHook.StopScript(resourceName);
                    }
                    break;
                case PacketType.FileTransferRequest:
                    {
                        LogManager.DebugLog("RECEIVED DATATYPE " + type);
                        var len = msg.ReadInt32();
                        var data = DeserializeBinary<DataDownloadStart>(msg.ReadBytes(len)) as DataDownloadStart;
                        if (data != null)
                        {
                            var acceptDownload = DownloadManager.StartDownload(data.Id,
                                data.ResourceParent + Path.DirectorySeparatorChar + data.FileName,
                                (FileType)data.FileType, data.Length, data.Md5Hash, data.ResourceParent);
                            LogManager.DebugLog("FILE TYPE: " + (FileType)data.FileType);
                            LogManager.DebugLog("DOWNLOAD ACCEPTED: " + acceptDownload);
                            var newMsg = Client.CreateMessage();
                            newMsg.Write((byte)PacketType.FileAcceptDeny);
                            newMsg.Write(data.Id);
                            newMsg.Write(acceptDownload);
                            Client.SendMessage(newMsg, NetDeliveryMethod.ReliableOrdered, (int)ConnectionChannel.SyncEvent);
                        }
                        else
                        {
                            LogManager.DebugLog("DATA WAS NULL ON REQUEST");
                        }
                    }
                    break;
                case PacketType.FileTransferTick:
                    {
                        LogManager.DebugLog("RECEIVED DATATYPE " + type);
                        var channel = msg.ReadInt32();
                        var len = msg.ReadInt32();
                        var data = msg.ReadBytes(len);
                        DownloadManager.DownloadPart(channel, data);
                    }
                    break;
                case PacketType.FileTransferComplete:
                    {
                        LogManager.DebugLog("RECEIVED DATATYPE " + type);
                        var id = msg.ReadInt32();
                        DownloadManager.End(id);
                    }
                    break;
                /*case PacketType.ChatData:
                    {
                        var len = msg.ReadInt32();
                        var data = DeserializeBinary<ChatData>(msg.ReadBytes(len)) as ChatData;
                        if (data != null && !string.IsNullOrEmpty(data.Message))
                        {
                            Chat.AddMessage(data.Sender, data.Message);
                        }
                    }
                    break;*/
                case PacketType.ServerEvent:
                    {
                        var len = msg.ReadInt32();
                        var data = DeserializeBinary<SyncEvent>(msg.ReadBytes(len)) as SyncEvent;
                        if (data != null)
                        {
                            var args = Natives.DecodeArgumentListPure(data.Arguments?.ToArray() ?? new NativeArgument[0]).ToList();
                            switch ((ServerEventType)data.EventType)
                            {
                                /*case ServerEventType.PlayerSpectatorChange:
                                    {
                                        var netHandle = (int)args[0];
                                        var spectating = (bool)args[1];
                                        var lclHndl = Streamer.NetToEntity(netHandle);
                                        if (lclHndl != null && lclHndl.Handle != Game.Player.Character.Handle)
                                        {
                                            var pair = Streamer.NetToStreamedItem(netHandle) as SyncPed;
                                            if (pair != null)
                                            {
                                                pair.IsSpectating = spectating;
                                                if (spectating)
                                                    pair.Clear();
                                            }
                                        }
                                        else if (lclHndl != null && lclHndl.Handle == Game.Player.Character.Handle)
                                        {
                                            IsSpectating = spectating;
                                            if (spectating)
                                                _preSpectatorPos = Game.Player.Character.Position;
                                            if (spectating && args.Count >= 3)
                                            {
                                                var target = (int)args[2];
                                                SpectatingEntity = target;
                                            }
                                        }
                                    }
                                    break;*/
                              /*  case ServerEventType.PlayerBlipColorChange:
                                    {
                                        var netHandle = (int)args[0];
                                        var newColor = (int)args[1];
                                        var lclHndl = Streamer.NetToEntity(netHandle);
                                        if (lclHndl != null && lclHndl.Handle != Game.Player.Character.Handle)
                                        {
                                            var pair = Streamer.NetToStreamedItem(netHandle) as SyncPed;
                                            if (pair != null)
                                            {
                                                pair.BlipColor = newColor;
                                                if (pair.Character != null &&
                                                    pair.Character.CurrentBlip != null)
                                                {
                                                    pair.Character.CurrentBlip.Color = (BlipColor)newColor;
                                                }
                                            }
                                        }
                                    }
                                    break;*/
                                case ServerEventType.PlayerBlipSpriteChange:
                                    {
                                        var netHandle = (int)args[0];
                                        var newSprite = (int)args[1];
                                        var lclHndl = Streamer.NetToEntity(netHandle);
                                        if (lclHndl != null && lclHndl.Handle != Game.Player.Character.Handle)
                                        {
                                            var pair = Streamer.NetToStreamedItem(netHandle) as SyncPed;
                                            if (pair != null)
                                            {
                                                pair.BlipSprite = newSprite;
                                                if (pair.Character != null && pair.Character.CurrentBlip != null)
                                                    pair.Character.CurrentBlip.Sprite =
                                                        (BlipSprite)newSprite;
                                            }
                                        }
                                    }
                                    break;
                                /*case ServerEventType.PlayerBlipAlphaChange:
                                    {
                                        var netHandle = (int)args[0];
                                        var newAlpha = (int)args[1];
                                        var pair = Streamer.NetToStreamedItem(netHandle) as SyncPed;
                                        if (pair != null)
                                        {
                                            pair.BlipAlpha = (byte)newAlpha;
                                            if (pair.Character != null &&
                                                pair.Character.CurrentBlip != null)
                                                pair.Character.CurrentBlip.Alpha = newAlpha;
                                        }
                                    }
                                    break;*/
                                case ServerEventType.PlayerTeamChange:
                                    {
                                        var netHandle = (int)args[0];
                                        var newTeam = (int)args[1];
                                        var lclHndl = Streamer.NetToEntity(netHandle);
                                        if (lclHndl != null && lclHndl.Handle != Game.Player.Character.Handle)
                                        {
                                            var pair = Streamer.NetToStreamedItem(netHandle) as SyncPed;
                                            if (pair != null)
                                            {
                                                pair.Team = newTeam;
                                                /*
                                                if (pair.Character != null)
                                                    pair.Character.RelationshipGroup = (newTeam == LocalTeam &&
                                                                                                newTeam != -1)
                                                        ? Main.FriendRelGroup
                                                        : Main.RelGroup;*/
                                            }
                                        }
                                        else if (lclHndl != null && lclHndl.Handle == Game.Player.Character.Handle)
                                        {
                                            LocalTeam = newTeam;
                                            foreach (var opponent in Streamer.ClientMap.Values.Where(item => item is SyncPed && ((SyncPed)item).LocalHandle != -2).Cast<SyncPed>())
                                            {
                                                /*
                                                if (opponent.Character != null &&
                                                    (opponent.Team == newTeam && newTeam != -1))
                                                {
                                                    opponent.Character.RelationshipGroup =
                                                        Main.FriendRelGroup;
                                                }
                                                else if (opponent.Character != null)
                                                {
                                                    opponent.Character.RelationshipGroup =
                                                        Main.RelGroup;
                                                }*/
                                            }
                                        }
                                    }
                                    break;
                                case ServerEventType.PlayerAnimationStart:
                                    {
                                        var netHandle = (int)args[0];
                                        var animFlag = (int)args[1];
                                        var animDict = (string)args[2];
                                        var animName = (string)args[3];

                                        var lclHndl = Streamer.NetToEntity(netHandle);
                                        if (lclHndl != null && lclHndl.Handle != Game.Player.Character.Handle)
                                        {
                                            var pair = Streamer.NetToStreamedItem(netHandle) as SyncPed;
                                            if (pair != null && pair.Character != null && pair.Character.Exists())
                                            {
                                                pair.IsCustomAnimationPlaying = true;
                                                pair.CustomAnimationName = animName;
                                                pair.CustomAnimationDictionary = animDict;
                                                pair.CustomAnimationFlag = animFlag;
                                                pair.CustomAnimationStartTime = Util.TickCount;

                                                if (!string.IsNullOrEmpty(animName) &&
                                                    string.IsNullOrEmpty(animDict))
                                                {
                                                    pair.IsCustomScenarioPlaying = true;
                                                    pair.HasCustomScenarioStarted = false;
                                                }
                                            }
                                        }
                                        else if (lclHndl != null && lclHndl.Handle == Game.Player.Character.Handle)
                                        {
                                            _animationFlag = 0;
                                            _customAnimation = null;

                                            if (string.IsNullOrEmpty(animDict))
                                            {
                                                Function.Call(Hash.TASK_START_SCENARIO_AT_POSITION, Game.Player.Character, animName, 0, 0);
                                            }
                                            else
                                            {
                                                Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character,
                                                    Util.LoadDict(animDict), animName, 8f, 10f, -1, animFlag, -8f, 1, 1, 1);
                                                if ((animFlag & 1) != 0)
                                                {
                                                    _customAnimation = animDict + " " + animName;
                                                    _animationFlag = animFlag;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case ServerEventType.PlayerAnimationStop:
                                    {
                                        var netHandle = (int)args[0];
                                        var lclHndl = Streamer.NetToEntity(netHandle);
                                        if (lclHndl != null && lclHndl.Handle != Game.Player.Character.Handle)
                                        {
                                            var pair = Streamer.NetToStreamedItem(netHandle) as SyncPed;
                                            if (pair != null && pair.Character != null && pair.Character.Exists() && pair.IsCustomAnimationPlaying)
                                            {
                                                pair.Character.Task.ClearAll();
                                                pair.IsCustomAnimationPlaying = false;
                                                pair.CustomAnimationName = null;
                                                pair.CustomAnimationDictionary = null;
                                                pair.CustomAnimationFlag = 0;
                                                pair.IsCustomScenarioPlaying = false;
                                                pair.HasCustomScenarioStarted = false;

                                            }
                                        }
                                        else if (lclHndl != null && lclHndl.Handle == Game.Player.Character.Handle)
                                        {
                                            Game.Player.Character.Task.ClearAll();
                                            _animationFlag = 0;
                                            _customAnimation = null;
                                        }
                                    }
                                    break;
                                case ServerEventType.EntityDetachment:
                                    {
                                        var netHandle = (int)args[0];
                                        bool col = (bool)args[1];
                                        Streamer.DetachEntity(Streamer.NetToStreamedItem(netHandle), col);
                                    }
                                    break;
                                /*case ServerEventType.WeaponPermissionChange:
                                    {
                                        var isSingleWeaponChange = (bool)args[0];

                                        if (isSingleWeaponChange)
                                        {
                                            var hash = (int)args[1];
                                            var hasPermission = (bool)args[2];

                                            if (hasPermission) WeaponInventoryManager.Allow((WeaponHash)hash);
                                            else WeaponInventoryManager.Deny((WeaponHash)hash);
                                        }
                                        else
                                        {
                                            WeaponInventoryManager.Clear();
                                        }
                                    }
                                    break;*/
                            }
                        }
                    }
                    break;
                case PacketType.SyncEvent:
                    {
                        var len = msg.ReadInt32();
                        var data = DeserializeBinary<SyncEvent>(msg.ReadBytes(len)) as SyncEvent;
                        if (data != null)
                        {
                            var args = Natives.DecodeArgumentList(data.Arguments.ToArray()).ToList();
                            if (args.Count > 0)
                                LogManager.DebugLog("RECEIVED SYNC EVENT " + ((SyncEventType)data.EventType) + ": " + args.Aggregate((f, s) => f.ToString() + ", " + s.ToString()));
                            switch ((SyncEventType)data.EventType)
                            {
                                /*
                                case SyncEventType.LandingGearChange:
                                    {
                                        var veh = NetEntityHandler.NetToEntity((int)args[0]);
                                        var newState = (int)args[1];
                                        if (veh == null) return;
                                        Function.Call(Hash.CONTROL_LANDING_GEAR, veh, newState);
                                    }
                                    break;*/
                               /* case SyncEventType.DoorStateChange:
                                    {
                                        var veh = Streamer.NetToEntity((int)args[0]);
                                        var doorId = (int)args[1];
                                        var newFloat = (bool)args[2];
                                        if (veh == null) return;
                                        if (newFloat)
                                            new Vehicle(veh.Handle).Doors[(VehicleDoorIndex)doorId].Open(false, true);
                                        else
                                            new Vehicle(veh.Handle).Doors[(VehicleDoorIndex)doorId].Close(true);

                                        var item = Streamer.NetToStreamedItem((int)args[0]) as RemoteVehicle;
                                        if (item != null)
                                        {
                                            if (newFloat)
                                                item.Tires |= (byte)(1 << doorId);
                                            else
                                                item.Tires &= (byte)~(1 << doorId);
                                        }
                                    }
                                    break;*/
                                /*case SyncEventType.BooleanLights:
                                    {
                                        var veh = NetEntityHandler.NetToEntity((int)args[0]);
                                        var lightId = (Lights)(int)args[1];
                                        var state = (bool)args[2];
                                        if (veh == null) return;
                                        if (lightId == Lights.NormalLights)
                                            new Vehicle(veh.Handle).LightsOn = state;
                                        else if (lightId == Lights.Highbeams)
                                            Function.Call(Hash.SET_VEHICLE_FULLBEAM, veh.Handle, state);
                                    }
                                    break;*/
                                
                                /*case SyncEventType.TireBurst:
                                    {
                                        var veh = NetEntityHandler.NetToEntity((int)args[0]);
                                        var tireId = (int)args[1];
                                        var isBursted = (bool)args[2];
                                        if (veh == null) return;
                                        if (isBursted)
                                            new Vehicle(veh.Handle).Wheels[tireId].Burst();
                                        else
                                            new Vehicle(veh.Handle).Wheels[tireId].Fix();

                                        var item = NetEntityHandler.NetToStreamedItem((int)args[0]) as RemoteVehicle;
                                        if (item != null)
                                        {
                                            if (isBursted)
                                                item.Tires |= (byte)(1 << tireId);
                                            else
                                                item.Tires &= (byte)~(1 << tireId);
                                        }
                                    }
                                    break;*/
                                case SyncEventType.PickupPickedUp:
                                    {
                                        var pickupItem = Streamer.NetToStreamedItem((int)args[0]);
                                        if (pickupItem != null)
                                        {
                                            Streamer.StreamOut(pickupItem);
                                            Streamer.Remove(pickupItem);
                                        }
                                    }
                                    break;
                                case SyncEventType.StickyBombDetonation:
                                    {
                                        var playerId = (int)args[0];
                                        var syncP = Streamer.NetToStreamedItem(playerId) as SyncPed;

                                        if (syncP != null && syncP.StreamedIn && syncP.Character != null)
                                        {
                                          //  Function.Call(Hash.EXPLODE_PROJECTILES, syncP.Character, (int)WeaponHash.StickyBomb, true);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    break;
                case PacketType.PlayerDisconnect:
                    {
                        var len = msg.ReadInt32();

                        var data = DeserializeBinary<PlayerDisconnect>(msg.ReadBytes(len)) as PlayerDisconnect;
                        SyncPed target = null;
                        if (data != null && (target = Streamer.NetToStreamedItem(data.Id) as SyncPed) != null)
                        {
                            Streamer.StreamOut(target);
                            target.Clear();
                            lock (Npcs)
                            {
                                foreach (var pair in new Dictionary<string, SyncPed>(Npcs).Where(p => p.Value.Host == data.Id))
                                {
                                    Npcs.Remove(pair.Key);
                                    pair.Value.Clear();
                                }
                            }
                        }
                        if (data != null) Streamer.RemoveByNetHandle(data.Id);
                    }
                    break;
                case PacketType.ScriptEventTrigger:
                    {
                        var len = msg.ReadInt32();
                        var data =
                            DeserializeBinary<ScriptEventTrigger>(msg.ReadBytes(len)) as ScriptEventTrigger;
                        if (data != null)
                        {
                            /*
                            if (data.Arguments != null && data.Arguments.Count > 0)
                                JavascriptHook.InvokeServerEvent(data.EventName, data.Resource,
                                    DecodeArgumentListPure(data.Arguments?.ToArray()).ToArray());
                            else
                                JavascriptHook.InvokeServerEvent(data.EventName, data.Resource, new object[0]);*/
                        }
                    }
                    break;
                case PacketType.NativeCall:
                    {
                        var len = msg.ReadInt32();
                        var data = (NativeData)DeserializeBinary<NativeData>(msg.ReadBytes(len));
                        if (data == null) return;
                        LogManager.DebugLog("RECEIVED NATIVE CALL " + data.Hash);
                        Natives.DecodeNativeCall(data);
                    }
                    break;
                case PacketType.DeleteObject:
                    {
                        var len = msg.ReadInt32();
                        var data = (ObjectData)DeserializeBinary<ObjectData>(msg.ReadBytes(len));
                        if (data == null) return;
                        Util.DeleteObject(data.Position, data.Radius, data.modelHash);
                    }
                    break;
            }
            #endregion    
        }
        private static void OnLocalDisconnect()
        {
           // StopLoadingPrompt();

            if (Streamer.ServerWorld?.LoadedIpl != null)
            {
                foreach (var ipl in Streamer.ServerWorld.LoadedIpl)
                    Function.Call((Hash)0x5A3E5CF7B4014B96, ipl); // RemoveIpl
            }

            if (Streamer.ServerWorld?.RemovedIpl != null)
            {
                foreach (var ipl in Streamer.ServerWorld.RemovedIpl)
                {
                    Function.Call((Hash)0x59767C5A7A9AE6DA, ipl); // Request IPL
                }
            }

            /*
            ClearLocalEntities();

            ClearLocalBlips();

            CameraManager.Reset();
            NetEntityHandler.ClearAll();
            DEBUG_STEP = 50;
            JavascriptHook.StopAllScripts();
            JavascriptHook.TextElements.Clear();
            SyncCollector.ForceAimData = false;
            StringCache.Dispose();
            StringCache = null;*/
            _threadsafeSubtitle = null;
            _cancelDownload = true;
            _httpDownloadThread?.Abort();
            //CefController.ShowCursor = false;
            DownloadManager.Cancel();
            DownloadManager.FileIntegrity.Clear();
          /*  Chat = _backupChat;
            Chat.Clear();
            WeaponInventoryManager.Clear();
            VehicleSyncManager.StopAll();*/
            HasFinishedDownloading = false;
            /*  ScriptChatVisible = true;
              CanOpenChatbox = true;
              DisplayWastedMessage = true;
              _password = string.Empty;
              CEFManager.Draw = false;


              UIColor = Color.White;

              DEBUG_STEP = 52;

              lock (CEFManager.Browsers)
              {
                  foreach (var browser in CEFManager.Browsers)
                  {
                      browser.Close();
                      browser.Dispose();
                  }

                  CEFManager.Browsers.Clear();
              }

              CEFManager.Dispose();


              RestoreMainMenu();

              ResetWorld();

              */
            ClearStats();
            ResetPlayer();

        }

        private static void ClearStats()
        {
            _bytesReceived = 0;
            _bytesSent = 0;
            _messagesReceived = 0;
            _messagesSent = 0;
        }
    }
}

using System;
using System.Collections.Generic;
using ResuMPServer.Constant;
using ResuMPServer.Managers;
using Lidgren.Network;
using RDRNetworkShared;
using System.Threading.Tasks;
using System.Linq;

namespace ResuMPServer
{
    public class Client : Entity
    {
        internal int VehicleHandleInternal { get; set; }
        internal Vector3 PositionInternal { get; set; }
        internal Vector3 RotationInternal { get; set; }
        internal Vector3 VelocityInternal { get; set; }
        internal string NameInternal { get; set; }
        internal int HealthInternal { get; set; }
        internal int ArmorInternal { get; set; }

        internal Dictionary<int, long> LastPacketReceived = new Dictionary<int, long>();
        internal Streamer Streamer { get; set; }
        internal DateTime LastUpdate { get; set; }

        internal bool Fake { get; set; }

        internal int LastPedFlag { get; set; }
        internal int LastVehicleFlag { get; set; }
        internal NetConnection NetConnection { get; private set; }

        internal bool ConnectionConfirmed { get; set; }
        
        internal bool CEF { get; set; }
        internal bool MediaStream { get; set; }
        internal float Latency { get; set; }
        internal ParseableVersion RemoteScriptVersion { get; set; }
        internal int GameVersion { get; set; } 
        internal Vector3 LastAimPos { get; set; }
        internal int Ammo { get; set; }
        internal int ModelHash { get; set; }

        public NetHandle CurrentVehicle { get; internal set; }
        public bool IsInVehicle { get; internal set; }
        public string SocialClubName { get; internal set; }

        private Dictionary<WeaponHash, int> _weapons = new Dictionary<WeaponHash, int>();
        public Dictionary<WeaponHash, int> Weapons
        {
            get => _weapons;
            internal set => _weapons = value;
        }

        public WeaponHash CurrentWeapon { get; internal set; }

        private int _vehicleSeat;
        public int VehicleSeat
        {
            get
            {
                if (!IsInVehicle || CurrentVehicle.IsNull) return -3;
                return _vehicleSeat;
            }
            set => _vehicleSeat = value;
        }

        internal PlayerProperties Properties => Program.ServerInstance.NetEntityHandler.ToDict()[Id.Value] as PlayerProperties;

        public Client(API father, NetHandle handle, NetConnection nc) : base(father, handle)
        {
            Base = father;
            Id = handle;
            NetConnection = nc;
            Streamer = new Streamer(this);
            HealthInternal = 100;
            ArmorInternal = 0;
        }

        public static implicit operator NetHandle(Client c) => c.Id;

        public override bool Equals(object obj)
        {
            Client target;
            if ((target = obj as Client) == null) return false;
            if (NetConnection == null || target.NetConnection == null)
                return Id == target.Id;

            return NetConnection.RemoteUniqueIdentifier == target.NetConnection.RemoteUniqueIdentifier;
        }

        public static bool operator ==(Client left, Client right)
        {
            if ((object) left == null && (object) right == null) return true;
            if ((object)left == null || (object)right == null) return false;
            if (left.NetConnection == null || right.NetConnection == null) return left.Id == right.Id;

            return left.NetConnection.RemoteUniqueIdentifier == right.NetConnection.RemoteUniqueIdentifier;
        }

        public static bool operator !=(Client left, Client right)
        {
            if ((object)left == null && (object)right == null) return false;
            if ((object)left == null || (object)right == null) return true;
            if (left.NetConnection == null || right.NetConnection == null) return left.Id != right.Id;

            return left.NetConnection.RemoteUniqueIdentifier != right.NetConnection.RemoteUniqueIdentifier;
        }


        #region Properties

        public Vehicle Vehicle
        {
            get
            {
                var nh = CurrentVehicle;
                if (nh.IsNull) return null;
                return new Vehicle(API.Shared, nh);
            }
        }

        public int Team
        {
            set
            {
                Program.ServerInstance.ChangePlayerTeam(this, value);
            }
            get { return Properties.Team; }
        }

        public int Ping
        {
            get => (int)(Latency* 1000f);
        }

        public int WantedLevel
        {
            get => API.FetchNativeFromPlayer<int>(this, 0x4C9296CBCD1B971E);
            set => Program.ServerInstance.SendNativeCallToPlayer(this, 0x1454F2448DE30163, value);
        }

        public string Name
        {
            get => NameInternal;
            set => SetPlayerName(value);
        }

        public bool IsCEFenabled
        {
            get => CEF;
        }

        public bool IsMediaStreamEnabled
        {
            get => MediaStream;
        }

        public Vector3 Velocity
        {
            get => VelocityInternal ?? new Vector3();
            set => API.Shared.SendNativeToPlayer(this, 0x1C99BB7B6E96D16F, this.IsInVehicle ? this.CurrentVehicle : this.Id, value.X, value.Y, value.Z);
        }

        public string Address
        {
            get => NetConnection.RemoteEndPoint.Address.ToString();
        }

        public bool Seatbelt
        {
            get => !API.FetchNativeFromPlayer<bool>(this, 0x7EE53118C892B513, new EntityArgument(this.Id.Value), 32, true);
            set => Program.ServerInstance.SendNativeCallToPlayer(this, 0x1913FE4CBF41C463,
                new EntityArgument(this.Id.Value), 32, !value);
        }

        public int Health
        {
            get => HealthInternal;
            set {
                Program.ServerInstance.SendNativeCallToPlayer(this, 0x6B76DC1F3AE6E6A3, new LocalPlayerArgument(), value + 100);
                this.HealthInternal = value;
            }
        }

        public int Armor
        {
            get => ArmorInternal;
            set => Program.ServerInstance.SendNativeCallToPlayer(this, 0xCEA04D83135264CC, new LocalPlayerArgument(), value);
        }

        public bool OnFire
        {
            get => (LastPedFlag & (int)PedDataFlags.OnFire) != 0;
        }

        public bool IsParachuting
        {
            get => (LastPedFlag & (int)PedDataFlags.ParachuteOpen) != 0;
        }

        public bool InFreefall
        {
            get => (LastPedFlag & (int)PedDataFlags.InFreefall) != 0;
        }

        public bool IsAiming
        {
            get => (LastPedFlag & (int)PedDataFlags.Aiming) != 0;
        }

        public bool IsShooting
        {
            get => (LastPedFlag & (int)PedDataFlags.Shooting) != 0;
        }

        public bool IsReloading
        {
            get => (LastPedFlag & (int)PedDataFlags.IsReloading) != 0;
        }

        public bool IsInCover
        {
            get => (LastPedFlag & (int)PedDataFlags.IsInCover) != 0;
        }

        public bool IsOnLadder
        {
            get => (LastPedFlag & (int)PedDataFlags.IsOnLadder) != 0;
        }

        public Vector3 AimingPoint
        {
            get => LastAimPos;
        }

        public bool Dead
        {
            get {
                if (IsInVehicle)
                    return (LastVehicleFlag & (int)VehicleDataFlags.PlayerDead) != 0;
                else return (LastPedFlag & (int)PedDataFlags.PlayerDead) != 0;
            }
        }

        public string Nametag
        {
            get => Properties.NametagText;
            set => SetPlayerNametag(value);
        }

        public bool NametagVisible
        {
            get => GetPlayerNametagVisible();
            set => SetPlayerNametagVisible(value);
        }

        public Color NametagColor
        {
            get => GetPlayerNametagColor();
            set => SetPlayerNametagColor((byte)value.red, (byte)value.green, (byte)value.blue); 
        }

        public bool Spectating
        {
            get => PacketOptimization.CheckBit(Properties.Flag, EntityFlag.PlayerSpectating);
        }

        #endregion

        #region Methods

        public void SendChatMessage(string message)
        {
            API.Shared.SendChatMessageToPlayer(this, message);
        }

        public void SendChatMessage(string sender, string message)
        {
            API.Shared.SendChatMessageToPlayer(this, sender, message);
        }
        #endregion

        #region Entity Inheritance

        public string Version
        {
            get { return this.RemoteScriptVersion.ToString(); }
        }

        #region Methods
        public void TriggerEvent(string eventName, params object[] args)
        {
            API.Shared.TriggerClientEvent(this, eventName, args);
        }

        public void PlayPlayerAnimation(int flag, string animDict, string animName)
        {
            Program.ServerInstance.PlayCustomPlayerAnimation(this, flag, animDict, animName);
        }

        public void PlayPlayerScenario(string scenarioName)
        {
            Program.ServerInstance.PlayCustomPlayerAnimation(this, 0, null, scenarioName);
        }

        public void StopPlayerAnimation()
        {
            Program.ServerInstance.PlayCustomPlayerAnimationStop(this);
        }

        public void RemoveAllPlayerWeapons()
        {
            Weapons.Clear();
            Properties.WeaponTints.Clear();
            Properties.WeaponComponents.Clear();

            /* désactivé car il faut modifié le cast
            Properties.WeaponTints.Add((int)WeaponHash.Unarmed, 0);
            Properties.WeaponComponents.Add((int)WeaponHash.Unarmed, new List<int>());
            */
            Program.ServerInstance.SendServerEventToPlayer(this, ServerEventType.WeaponPermissionChange, false);
            API.Shared.SendNativeToPlayer(this, 0xF25DF915FA38C5F3, new LocalPlayerArgument(), true);

            var delta = new Delta_PlayerProperties();
            delta.WeaponTints = Properties.WeaponTints;
            delta.WeaponComponents = Properties.WeaponComponents;
            GameServer.UpdateEntityInfo(Id.Value, EntityType.Player, delta, this);
        }


        public void SetPlayerNametag(string text)
        {
            Properties.NametagText = text;

            var delta = new Delta_PlayerProperties();
            delta.NametagText = text;
            GameServer.UpdateEntityInfo(Id.Value, EntityType.Player, delta);
        }

        public void ResetPlayerNametag()
        {
            Properties.NametagText = " ";

            var delta = new Delta_PlayerProperties();
            delta.NametagText = " ";
            GameServer.UpdateEntityInfo(Id.Value, EntityType.Player, delta);
        }

        private void SetPlayerNametagVisible(bool visible)
        {
            if (visible)
                Properties.NametagSettings = PacketOptimization.ResetBit(Properties.NametagSettings, 1);
            else
                Properties.NametagSettings = PacketOptimization.SetBit(Properties.NametagSettings, 1);

            var delta = new Delta_PlayerProperties();
            delta.NametagSettings = Properties.NametagSettings;
            GameServer.UpdateEntityInfo(Id.Value, EntityType.Player, delta);
        }

        private bool GetPlayerNametagVisible()
        {
            return !PacketOptimization.CheckBit(Properties.NametagSettings, 1);
        }

        private void SetPlayerNametagColor(byte r, byte g, byte b)
        {
            Properties.NametagSettings = PacketOptimization.SetBit(Properties.NametagSettings, 2);

            var col = Extensions.FromArgb(0, r, g, b) << 8;
            Properties.NametagSettings |= col;

            var delta = new Delta_PlayerProperties();
            delta.NametagSettings = Properties.NametagSettings;
            GameServer.UpdateEntityInfo(Id.Value, EntityType.Player, delta);
        }

        private Color GetPlayerNametagColor()
        {
            var output = new Color();

            byte a, r, g, b;

            var col = Properties.NametagSettings >> 8;
            Extensions.ToArgb(col, out a, out r, out g, out b);

            output.alpha = a;
            output.red = r;
            output.green = g;
            output.blue = b;

            return output;
        }

        public void ResetPlayerNametagColor()
        {
            Properties.NametagSettings = PacketOptimization.ResetBit(Properties.NametagSettings, 2);

            Properties.NametagSettings &= 255;

            var delta = new Delta_PlayerProperties();
            delta.NametagSettings = Properties.NametagSettings;
            GameServer.UpdateEntityInfo(Id.Value, EntityType.Player, delta);
        }

        public void SetPlayerToSpectator()
        {
            Program.ServerInstance.SetPlayerOnSpectate(this, true);
            Properties.Flag |= (byte)EntityFlag.PlayerSpectating;

            var delta = new Delta_PlayerProperties();
            delta.Flag = Properties.Flag;
            GameServer.UpdateEntityInfo(Id.Value, EntityType.Player, delta, this);
        }

        public void SetPlayerToSpectatePlayer(Client target)
        {
            Program.ServerInstance.SetPlayerOnSpectatePlayer(this, target);
            Properties.Flag |= (byte)EntityFlag.PlayerSpectating;

            var delta = new Delta_PlayerProperties();
            delta.Flag = Properties.Flag;
            GameServer.UpdateEntityInfo(Id.Value, EntityType.Player, delta, this);
        }

        public void UnspectatePlayer()
        {
            Program.ServerInstance.SetPlayerOnSpectate(this, false);
            Properties.Flag = (byte)PacketOptimization.ResetBit(this.Properties.Flag, EntityFlag.PlayerSpectating);

            var delta = new Delta_PlayerProperties();
            delta.Flag = Properties.Flag;
            GameServer.UpdateEntityInfo(Id.Value, EntityType.Player, delta, this);
        }

        public bool SetPlayerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return false;
            if (API.Shared.GetAllPlayers().Any(p => p.NameInternal.ToLower() == newName.ToLower())) return false;

            NameInternal = newName;

            Program.ServerInstance.NetEntityHandler.NetToProp<PlayerProperties>(Id.Value).Name =
                newName;

            var delta = new Delta_PlayerProperties();
            delta.Name = newName;
            GameServer.UpdateEntityInfo(Id.Value, EntityType.Player, delta);

            return true;
        }
        #endregion
        #endregion

    }
}
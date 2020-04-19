using RDRNetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResuMPServer
{
    public class CancelEventArgs
    {
        public bool Cancel { get; set; }
        public string Reason { get; set; }

        public CancelEventArgs() { }
        public CancelEventArgs(bool cancel)
        {
            Cancel = cancel;
        }
    }

    public abstract class Script
    {
        public API API = new API();
    }

    public partial class API
    {
        #region Fields
        public long TickCount => DateTime.Now.Ticks / 10000;
        public static API Shared => Program.ServerInstance.PublicAPI;

        #endregion

        #region Meta
        internal ScriptingEngine ResourceParent { get; set; }
        internal List<NetHandle> ResourceEntities = new List<NetHandle>();
        internal List<ColShape> ResourceColShapes = new List<ColShape>();
        #endregion

        #region Delegates
        public delegate Task EmptyEventAsync();
        public delegate Task CommandEvent(Client sender, string command, CancelEventArgs cancel);
        public delegate Task ChatEvent(Client sender, string message, CancelEventArgs cancel);
        public delegate Task PlayerEvent(Client player);
        public delegate Task PlayerKilledEvent(Client player, NetHandle entityKiller, int weapon);
        public delegate Task ServerEventTrigger(Client sender, string eventName, params object[] arguments);
        public delegate Task PickupEvent(Client pickupee, NetHandle pickupHandle);
        public delegate Task EntityEvent(NetHandle entity);
        public delegate Task MapChangeEvent(string mapName, XmlGroup map);
        public delegate Task VehicleChangeEvent(Client player, NetHandle vehicle);
        public delegate Task GlobalColShapeEvent(ColShape colshape, NetHandle entity);
        public delegate Task EntityDataChangedEvent(NetHandle entity, string key, object oldValue);
        public delegate Task ResourceEvent(string resourceName);
        public delegate Task DataReceivedEvent(string data);
        public delegate Task PlayerIntEvent(Client player, int oldValue);
        public delegate Task PlayerWeaponEvent(Client player, WeaponHash oldValue);
        public delegate Task PlayerAmmoEvent(Client player, WeaponHash weapon, int oldValue);
        public delegate Task EntityHealthEvent(NetHandle entity, float oldValue);
        public delegate Task EntityBooleanEvent(NetHandle entity, bool oldValue);
        public delegate Task EntityIntEvent(NetHandle entity, int index);
        public delegate Task TrailerEvent(NetHandle tower, NetHandle trailer);

        public delegate void EmptyEvent();
        public delegate void PlayerConnectingEvent(Client player, CancelEventArgs cancelConnection);
        public delegate void PlayerDisconnectedEvent(Client player, string reason);

        #endregion

        #region Events
        public event EmptyEventAsync OnResourceStart;
        public event EmptyEventAsync OnResourceStop;
        public event EmptyEvent OnUpdate;
        public event ChatEvent OnChatMessage;
        public event CommandEvent OnChatCommand;
        public event PlayerConnectingEvent OnPlayerBeginConnect;
        public event PlayerEvent OnPlayerConnected;
        public event PlayerEvent OnPlayerFinishedDownload;
        public event PlayerDisconnectedEvent OnPlayerDisconnected;
        public event PlayerKilledEvent OnPlayerDeath;
        public event PlayerEvent OnPlayerRespawn;
        public event ServerEventTrigger OnClientEventTrigger;
        public event PickupEvent OnPlayerPickup;
        public event EntityEvent OnPickupRespawn;
        public event MapChangeEvent OnMapChange;
        public event VehicleChangeEvent OnPlayerEnterVehicle;
        public event VehicleChangeEvent OnPlayerExitVehicle;
        public event EntityEvent OnVehicleDeath;
        public event GlobalColShapeEvent OnEntityEnterColShape;
        public event GlobalColShapeEvent OnEntityExitColShape;
        public event EntityDataChangedEvent OnEntityDataChange;
        public event ResourceEvent OnServerResourceStart;
        public event ResourceEvent OnServerResourceStop;
        public event EntityHealthEvent OnVehicleHealthChange;
        public event PlayerIntEvent OnPlayerHealthChange;
        public event PlayerIntEvent OnPlayerArmorChange;
        public event PlayerWeaponEvent OnPlayerWeaponSwitch;
        public event PlayerAmmoEvent OnPlayerWeaponAmmoChange;
        public event EntityBooleanEvent OnVehicleSirenToggle;
        public event EntityIntEvent OnVehicleDoorBreak;
        public event EntityIntEvent OnVehicleWindowSmash;
        public event EntityIntEvent OnVehicleTyreBurst;
        public event TrailerEvent OnVehicleTrailerChange;
        public event PlayerIntEvent OnPlayerModelChange;
        public event PlayerEvent OnPlayerDetonateStickies;

        internal async Task invokePlayerDetonateStickies(Client player)
        {
            if (OnPlayerDetonateStickies != null)
                await OnPlayerDetonateStickies?.Invoke(player);
        }

        internal async Task invokePlayerModelChange(Client player, int oldModel)
        {
            if (OnPlayerModelChange != null)
                await OnPlayerModelChange?.Invoke(player, oldModel);
        }

        internal async Task invokeVehicleTrailerChange(NetHandle veh1, NetHandle veh2)
        {
            if (OnVehicleTrailerChange != null)
                await OnVehicleTrailerChange?.Invoke(veh1, veh2);
        }

        internal async Task invokeVehicleDoorBreak(NetHandle vehicle, int index)
        {
            if (OnVehicleDoorBreak != null)
                await OnVehicleDoorBreak?.Invoke(vehicle, index);
        }

        internal async Task invokeVehicleWindowBreak(NetHandle vehicle, int index)
        {
            if (OnVehicleWindowSmash != null)
                await OnVehicleWindowSmash?.Invoke(vehicle, index);
        }

        internal async Task invokeVehicleTyreBurst(NetHandle vehicle, int index)
        {
            if (OnVehicleTyreBurst != null)
                await OnVehicleTyreBurst?.Invoke(vehicle, index);
        }

        internal async Task invokeVehicleSirenToggle(NetHandle entity, bool oldValue)
        {
            if (OnVehicleSirenToggle != null)
                await OnVehicleSirenToggle?.Invoke(entity, oldValue);
        }

        internal async Task invokeVehicleHealthChange(NetHandle entity, float oldValue)
        {
            if (OnVehicleHealthChange != null)
                await OnVehicleHealthChange?.Invoke(entity, oldValue);
        }

        internal async Task invokePlayerWeaponSwitch(Client entity, int oldValue)
        {
            if (OnPlayerWeaponSwitch != null)
                await OnPlayerWeaponSwitch?.Invoke(entity, (WeaponHash)oldValue);
        }

        internal async Task invokePlayerWeaponAmmoChange(Client entity, int weapon, int oldValue)
        {
            if (OnPlayerWeaponAmmoChange != null)
                await OnPlayerWeaponAmmoChange?.Invoke(entity, (WeaponHash)weapon, oldValue);
        }

        internal async Task invokePlayerArmorChange(Client entity, int oldValue)
        {
            if (OnPlayerArmorChange != null)
                await OnPlayerArmorChange?.Invoke(entity, oldValue);
        }

        internal async Task invokePlayerHealthChange(Client entity, int oldValue)
        {
            if (OnPlayerHealthChange != null)
                await OnPlayerHealthChange?.Invoke(entity, oldValue);
        }

        internal async Task invokeOnEntityDataChange(NetHandle entity, string key, object oldValue)
        {
            if (OnEntityDataChange != null)
                await OnEntityDataChange?.Invoke(entity, key, oldValue);
        }

        internal async Task invokeColShapeEnter(ColShape shape, NetHandle vehicle)
        {
            if (OnEntityEnterColShape != null)
                await OnEntityEnterColShape?.Invoke(shape, vehicle);
        }

        internal async Task invokeColShapeExit(ColShape shape, NetHandle vehicle)
        {
            if (OnEntityExitColShape != null)
                await OnEntityExitColShape?.Invoke(shape, vehicle);
        }

        internal async Task invokeVehicleDeath(NetHandle vehicle)
        {
            if (OnVehicleDeath != null)
                await OnVehicleDeath?.Invoke(vehicle);
        }

        internal async Task invokeMapChange(string mapName, XmlGroup map)
        {
            if (OnMapChange != null)
                await OnMapChange?.Invoke(mapName, map);
        }

        internal async Task invokePlayerEnterVeh(Client player, NetHandle veh)
        {
            if (OnPlayerEnterVehicle != null)
                await OnPlayerEnterVehicle?.Invoke(player, veh);
        }

        internal async Task invokePlayerExitVeh(Client player, NetHandle veh)
        {
            if (OnPlayerExitVehicle != null)
                await OnPlayerExitVehicle?.Invoke(player, veh);
        }

        internal async Task invokeClientEvent(Client sender, string eventName, params object[] arsg)
        {
            if (OnClientEventTrigger != null)
                await OnClientEventTrigger?.Invoke(sender, eventName, arsg);
        }

        internal async Task invokeFinishedDownload(Client sender)
        {
            if (OnPlayerFinishedDownload != null)
                await OnPlayerFinishedDownload?.Invoke(sender);
        }

        internal async Task invokePickupRespawn(NetHandle pickup)
        {
            if (OnPickupRespawn != null)
                await OnPickupRespawn?.Invoke(pickup);
        }

        internal async Task InvokeResourceStart()
        {
            if (OnResourceStart != null)
                await OnResourceStart?.Invoke();
        }

        internal void invokeUpdate()
        {
            OnUpdate?.Invoke();
        }

        internal async Task invokeServerResourceStart(string resource)
        {
            if (OnServerResourceStart != null)
                await OnServerResourceStart?.Invoke(resource);
        }

        internal async Task invokeServerResourceStop(string resource)
        {
            if (OnPlayerDetonateStickies != null)
                await OnServerResourceStop?.Invoke(resource);
        }

        internal void invokeResourceStop()
        {
            OnResourceStop?.Invoke();

            lock (ResourceEntities)
            {
                for (int i = ResourceEntities.Count - 1; i >= 0; i--)
                {
                    DeleteEntityInternal(ResourceEntities[i]);
                }
                ResourceEntities.Clear();
            }

            lock (ResourceColShapes)
            {
                for (int i = ResourceColShapes.Count - 1; i >= 0; i--)
                {
                    Program.ServerInstance.ColShapeManager.Remove(ResourceColShapes[i]);
                }
                ResourceColShapes.Clear();
            }
        }

        internal async Task<bool> invokeChatMessage(Client sender, string msg)
        {
            var args = new CancelEventArgs(false);
            if (OnChatMessage != null)
                await OnChatMessage?.Invoke(sender, msg, args);
            return !args.Cancel;
        }

        internal async Task invokePlayerPickup(Client pickupee, NetHandle pickup)
        {
            if (OnPlayerPickup != null)
                await OnPlayerPickup?.Invoke(pickupee, pickup);
        }

        internal async Task invokeChatCommand(Client sender, string msg, CancelEventArgs ce)
        {
            if (OnChatCommand != null)
                await OnChatCommand?.Invoke(sender, msg, ce);
        }

        internal void invokePlayerBeginConnect(Client player, CancelEventArgs e)
        {
            if (OnPlayerBeginConnect != null)
                OnPlayerBeginConnect?.Invoke(player, e);
        }

        internal async Task invokePlayerConnected(Client player)
        {
            if (OnPlayerConnected != null)
                await OnPlayerConnected?.Invoke(player);
        }

        internal void invokePlayerDisconnected(Client player, string reason)
        {
            OnPlayerDisconnected?.Invoke(player, reason);
        }

        internal async Task invokePlayerDeath(Client player, NetHandle netHandle, int weapon)
        {
            if (OnPlayerDeath != null)
                await OnPlayerDeath?.Invoke(player, netHandle, weapon);
        }

        internal async Task invokePlayerRespawn(Client player)
        {
            if (OnPlayerRespawn != null)
                await OnPlayerRespawn?.Invoke(player);
        }

        #endregion

        #region Methods
        public void TriggerClientEventForAll(string eventName, params object[] args)
        {
            var packet = new ScriptEventTrigger();
            packet.EventName = eventName;
            if (ResourceParent == null)
                packet.Resource = "*";
            else
                packet.Resource = ResourceParent.ResourceParent.DirectoryName;

            packet.Arguments = GameServer.ParseNativeArguments(args);

            Program.ServerInstance.SendToAll(packet, PacketType.ScriptEventTrigger, true, ConnectionChannel.ClientEvent);
        }

        public void TriggerClientEvent(Client player, string eventName, params object[] args)
        {
            var packet = new ScriptEventTrigger();
            packet.EventName = eventName;
            packet.Resource = ResourceParent == null ? "*" : ResourceParent.ResourceParent.DirectoryName;
            packet.Arguments = GameServer.ParseNativeArguments(args);

            Program.ServerInstance.SendToClient(player, packet, PacketType.ScriptEventTrigger, true, ConnectionChannel.ClientEvent);
        }

        public Client GetPlayerFromHandle(NetHandle handle)
        {
            return Program.ServerInstance.Clients.FirstOrDefault(c => c.Id == handle);
        }
        #endregion
    }
}

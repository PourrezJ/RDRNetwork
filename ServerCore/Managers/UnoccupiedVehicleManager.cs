﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lidgren.Network;
using RDRNetworkShared;

namespace ResuMPServer.Managers
{
    internal class UnoccupiedVehicleManager
    {
        private const int UPDATE_RATE = 250;
        private const float SYNC_RANGE = 130;
        private const float SYNC_RANGE_SQUARED = SYNC_RANGE * SYNC_RANGE;
        private const float DROPOFF = 30;
        private const float DROPOFF_SQUARED = DROPOFF * DROPOFF;

        private long _lastUpdate;

        private Dictionary<int, Client> Syncer = new Dictionary<int, Client>();

        public void Pulse()
        {
            if (Program.GetTicks() - _lastUpdate <= UPDATE_RATE) return;
            _lastUpdate = Program.GetTicks();
            Task.Run((Action)Update);
        }

        public Client GetSyncer(int handle)
        {
            return Syncer.Get(handle);
        }

        public void UnsyncAllFrom(Client player)
        {
            for (var i = Syncer.Count - 1; i >= 0; i--)
            {
                var el = Syncer.ElementAt(i);

                if (el.Value == player)
                {
                    Syncer.Remove(el.Key);
                }
            }
        }


        private static bool IsVehicleUnoccupied(NetHandle vehicle)
        {
            var players = Program.ServerInstance.PublicAPI.GetAllPlayers();
            var vehicles = Program.ServerInstance.NetEntityHandler.ToCopy().Select(pair => pair.Value).Where(p => p is VehicleProperties).Cast<VehicleProperties>();
            var prop = Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(vehicle.Value);

            return players.TrueForAll(c => c.CurrentVehicle != vehicle) && vehicles.All(v => v.Trailer != vehicle.Value) && prop.AttachedTo == null;
        }


        //private void Update()
        //{
        //    for (var index = Program.ServerInstance.PublicAPI.getAllVehicles().Count - 1; index >= 0; index--)
        //    {
        //        UpdateVehicle(Program.ServerInstance.PublicAPI.getAllVehicles()[index].Value, Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(Program.ServerInstance.PublicAPI.getAllVehicles()[index].Value));
        //    }
        //}

        public void Update()
        {
            foreach (var vehicle in Program.ServerInstance.PublicAPI.GetAllVehicles())
            {
                UpdateVehicle(vehicle.Value, Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(vehicle.Value));
            }
        }


        //private void UpdateVehicle(int handle, EntityProperties prop)
        //{
        //    if (handle == 0 || prop == null) return;

        //    if (!IsVehicleUnoccupied(new NetHandle(handle))) //OCCUPIED
        //    {
        //        if (Syncer.ContainsKey(handle))
        //        {
        //            StopSync(Syncer[handle], handle);
        //        }
        //        return;
        //    }

        //    if (prop.Position == null) return;

        //    var players = Program.ServerInstance.PublicAPI.getAllPlayers().Where(c => (c.Properties.Dimension == prop.Dimension || prop.Dimension == 0) && c.Position != null).OrderBy(c => c.Position.DistanceToSquared2D(prop.Position)).Take(1).ToArray();
        //    if (players[0] == null) return;

        //    if (players[0].Position.DistanceToSquared(prop.Position) < SYNC_RANGE_SQUARED && (players[0].Properties.Dimension == prop.Dimension || prop.Dimension == 0))
        //    {
        //        if (Syncer.ContainsKey(handle))
        //        {
        //            if (Syncer[handle] != players[0])
        //            {
        //                StopSync(Syncer[handle], handle);
        //                StartSync(players[0], handle);
        //            }
        //        }
        //        else
        //        {
        //            StartSync(players[0], handle);
        //        }
        //    }
        //    else
        //    {
        //        if (Syncer.ContainsKey(handle))
        //        {
        //            StopSync(players[0], handle);
        //        }
        //    }
        //}

        public void UpdateVehicle(int handle, VehicleProperties prop)
        {
            if (handle == 0 || prop == null) return;
            if (!IsVehicleUnoccupied(new NetHandle(handle)))
            {
                if (Syncer.ContainsKey(handle))
                {
                    Sync(Syncer[handle], handle, false);
                    return;
                }
            }

            if (Syncer.ContainsKey(handle)) // This vehicle already has a syncer
            {
                if (Syncer[handle].PositionInternal.DistanceToSquared(prop.Position) > SYNC_RANGE_SQUARED || (Syncer[handle].Properties.Dimension != prop.Dimension && prop.Dimension != 0))
                {
                    Sync(Syncer[handle], handle, false);

                    FindSyncer(handle, prop);
                }
            }
            else // This car has no syncer
            {
                FindSyncer(handle, prop);
            }
        }

        public void FindSyncer(int handle, VehicleProperties prop)
        {
            if (prop.Position == null) return;

            var players =
                Program.ServerInstance.PublicAPI.GetAllPlayers()
                    .Where(c => (c.Properties.Dimension == prop.Dimension || prop.Dimension == 0) && c.PositionInternal != null)
                    .OrderBy(c => c.PositionInternal.DistanceToSquared(prop.Position));

            Client targetPlayer;

            if ((targetPlayer = players.FirstOrDefault()) != null && targetPlayer.PositionInternal.DistanceToSquared(prop.Position) < SYNC_RANGE_SQUARED - DROPOFF_SQUARED)
            {
                Sync(targetPlayer, handle, true);
            }
        }


        private void Sync(Client player, int vehicle, bool status)
        {
            var packet = Program.ServerInstance.Server.CreateMessage();
            packet.Write((byte)PacketType.UnoccupiedVehStartStopSync);
            packet.Write(vehicle);
            packet.Write(status);

            Program.ServerInstance.Server.SendMessage(packet, player.NetConnection, NetDeliveryMethod.ReliableOrdered, (int)ConnectionChannel.SyncEvent);
            //Console.WriteLine("[DEBUG MESSAGE] [+] Setting veh sync + " + status + " for: " + player.Name + " | Vehicle: " + vehicle);

            if(status)
            {
                Syncer.Set(vehicle, player);
            }
            else
            {
                Syncer.Remove(vehicle);
            }
        }
    }
}
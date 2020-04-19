using RDRNetworkShared;
using System;

namespace ResuMPServer
{
    public partial class API
    {
        public void SendNativeToPlayer(Client player, ulong longHash, params object[] args)
        {
            Program.ServerInstance.SendNativeCallToPlayer(player, longHash, args);
        }

        public void SendNativeToPlayer(Client player, Hash native, params object[] args)
        {
            SendNativeToPlayer(player, (ulong)native, args);
        }

        public void SendNativeToAllPlayers(ulong longHash, params object[] args)
        {
            Program.ServerInstance.SendNativeCallToAllPlayers(longHash, args);
        }

        public void SendNativeToAllPlayers(Hash native, params object[] args)
        {
            SendNativeToAllPlayers((ulong)native, args);
        }

        public void SendNativeToPlayersInRange(Vector3 pos, float range, ulong hash, params object[] args)
        {
            foreach (var client in GetAllPlayers())
            {
                if (pos.DistanceToSquared(client.PositionInternal) < range * range)
                {
                    SendNativeToPlayer(client, hash, args);
                }
            }
        }

        public void SendNativeToPlayersInRange(Vector3 pos, float range, Hash native, params object[] args)
        {
            SendNativeToPlayersInRange(pos, range, (ulong)native, args);
        }

        public void SendNativeToPlayersInRangeInDimension(Vector3 pos, float range, int dimension, ulong hash, params object[] args)
        {
            if (dimension == 0)
            {
                SendNativeToPlayersInRange(pos, range, hash, args);
                return;
            }

            foreach (var client in GetAllPlayers())
            {
                if (client.Properties.Dimension == dimension && pos.DistanceToSquared(client.PositionInternal) < range * range)
                {
                    SendNativeToPlayer(client, hash, args);
                }
            }
        }

        public void SendNativeToPlayersInRangeInDimension(Vector3 pos, float range, int dimension, Hash native, params object[] args)
        {
            SendNativeToPlayersInRangeInDimension(pos, range, dimension, native, args);
        }

        public void SendNativeToPlayersInDimension(int dimension, ulong hash, params object[] args)
        {
            if (dimension == 0)
            {
                SendNativeToAllPlayers(hash, args);
                return;
            }

            foreach (var client in GetAllPlayers())
            {
                if (client.Properties.Dimension == dimension)
                {
                    SendNativeToPlayer(client, hash, args);
                }
            }
        }

        public void SendNativeToPlayersInDimension(int dimension, Hash native, params object[] args)
        {
            SendNativeToPlayersInDimension(dimension, (ulong)native, args);
        }

        public static T FetchNativeFromPlayer<T>(Client player, ulong longHash, params object[] args)
        {
            var returnType = GameServer.ParseReturnType(typeof(T));
            if (returnType == null)
            {
                throw new ArgumentException("Type \"" + typeof(T) + "\" is not a valid return type.");
            }

            return (T)Program.ServerInstance.ReturnNativeCallFromPlayer(player, longHash,
                returnType, args);
        }

        internal static T SafeFetchNativeFromPlayer<T>(Client player, ulong longHash, params object[] args)
        {
            var returnType = GameServer.ParseReturnType(typeof(T));

            if (returnType == null)
            {
                throw new ArgumentException("Type \"" + typeof(T) + "\" is not a valid return type.");
            }

            return (T)Program.ServerInstance.ReturnNativeCallFromPlayer(player, longHash,
                returnType, args);
        }

        public static T FetchNativeFromPlayer<T>(Client player, Hash native, params object[] args)
        {
            return FetchNativeFromPlayer<T>(player, (ulong)native, args);
        }

        internal T SafeFetchNativeFromPlayer<T>(Client player, Hash native, params object[] args)
        {
            return SafeFetchNativeFromPlayer<T>(player, (ulong)native, args);
        }

        public void SetVehicleLocked(NetHandle vehicle, bool locked)
        {
            if (DoesEntityExist(vehicle))
            {
                if (locked)
                {
                    Program.ServerInstance.NetEntityHandler.ToDict()[vehicle.Value].Flag =
                        (byte)PacketOptimization.SetBit(Program.ServerInstance.NetEntityHandler.ToDict()[vehicle.Value].Flag,
                            EntityFlag.VehicleLocked);
                }
                else
                {
                    Program.ServerInstance.NetEntityHandler.ToDict()[vehicle.Value].Flag =
                        (byte)PacketOptimization.ResetBit(Program.ServerInstance.NetEntityHandler.ToDict()[vehicle.Value].Flag,
                            EntityFlag.VehicleLocked);
                }
                Program.ServerInstance.SendNativeCallToAllPlayers(0xB664292EAECF7FA6, new EntityArgument(vehicle.Value), locked ? 10 : 1);

                var delta = new Delta_VehicleProperties();
                delta.Flag = Program.ServerInstance.NetEntityHandler.ToDict()[vehicle.Value].Flag;
                GameServer.UpdateEntityInfo(vehicle.Value, EntityType.Vehicle, delta);
            }
        }

        public bool GetVehicleLocked(NetHandle vehicle)
        {
            if (DoesEntityExist(vehicle))
            {
                return PacketOptimization.CheckBit(Program.ServerInstance.NetEntityHandler.ToDict()[vehicle.Value].Flag,
                            EntityFlag.VehicleLocked);
            }
            return false;
        }

    }
}

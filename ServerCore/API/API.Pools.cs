using RDRNetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResuMPServer
{
    public partial class API
    {
        public List<Client> GetAllPlayers()
        {
            return new List<Client>(Program.ServerInstance.Clients);
        }

        public List<NetHandle> GetAllVehicles()
        {
            return
                new List<NetHandle>(
                    Program.ServerInstance.NetEntityHandler.ToCopy()
                        .Where(pair => pair.Value.EntityType == (byte)EntityType.Vehicle)
                        .Select(pair => new NetHandle(pair.Key)));
        }

        public List<NetHandle> GetAllObjects()
        {
            return
                new List<NetHandle>(
                    Program.ServerInstance.NetEntityHandler.ToCopy()
                        .Where(pair => pair.Value.EntityType == (byte)EntityType.Prop)
                        .Select(pair => new NetHandle(pair.Key)));
        }

        public List<NetHandle> GetAllMarkers()
        {
            return
                new List<NetHandle>(
                    Program.ServerInstance.NetEntityHandler.ToCopy()
                        .Where(pair => pair.Value.EntityType == (byte)EntityType.Marker)
                        .Select(pair => new NetHandle(pair.Key)));
        }

        public List<NetHandle> GetAllBlips()
        {
            return
                new List<NetHandle>(
                    Program.ServerInstance.NetEntityHandler.ToCopy()
                        .Where(pair => pair.Value.EntityType == (byte)EntityType.Blip)
                        .Select(pair => new NetHandle(pair.Key)));
        }

        public List<NetHandle> GetAllPickups()
        {
            return
                new List<NetHandle>(
                    Program.ServerInstance.NetEntityHandler.ToCopy()
                        .Where(pair => pair.Value.EntityType == (byte)EntityType.Pickup)
                        .Select(pair => new NetHandle(pair.Key)));
        }

        public List<NetHandle> GetAllLabels()
        {
            return
                new List<NetHandle>(
                    Program.ServerInstance.NetEntityHandler.ToCopy()
                        .Where(pair => pair.Value.EntityType == (byte)EntityType.TextLabel)
                        .Select(pair => new NetHandle(pair.Key)));
        }

        public List<NetHandle> GetAllPeds()
        {
            return
                new List<NetHandle>(
                    Program.ServerInstance.NetEntityHandler.ToCopy()
                        .Where(pair => pair.Value.EntityType == (byte)EntityType.Ped)
                        .Select(pair => new NetHandle(pair.Key)));
        }
    }
}

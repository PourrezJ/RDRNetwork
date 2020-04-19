using RDRNetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResuMPServer
{
    public partial class API
    {
        public bool SetEntitySyncedData(NetHandle entity, string key, object value)
        {
            if (DoesEntityExist(entity))
            {
                return Program.ServerInstance.SetEntityProperty(entity.Value, key, value);
            }
            return false;
        }

        public dynamic GetEntitySyncedData(NetHandle entity, string key)
        {
            if (DoesEntityExist(entity))
            {
                return Program.ServerInstance.GetEntityProperty(entity.Value, key);
            }
            return null;
        }

        public string[] GetAllEntitySyncedData(NetHandle entity)
        {
            if (DoesEntityExist(entity))
            {
                return Program.ServerInstance.GetEntityAllProperties(entity.Value);
            }
            return new string[0];
        }

        public void ResetEntitySyncedData(NetHandle entity, string key)
        {
            if (DoesEntityExist(entity))
            {
                Program.ServerInstance.ResetEntityProperty(entity.Value, key);
            }
        }

        public bool HasEntitySyncedData(NetHandle entity, string key)
        {
            if (DoesEntityExist(entity))
            {
                return Program.ServerInstance.HasEntityProperty(entity.Value, key);
            }
            return false;
        }

        public bool SetWorldSyncedData(string key, object value)
        {
            return Program.ServerInstance.SetEntityProperty(1, key, value, true);
        }

        public dynamic GetWorldSyncedData(string key)
        {
            return Program.ServerInstance.GetEntityProperty(1, key);
        }

        public string[] GetAllWorldSyncedData()
        {
            return Program.ServerInstance.GetEntityAllProperties(1);
        }

        public void ResetWorldSyncedData(string key)
        {
            Program.ServerInstance.ResetEntityProperty(1, key, true);
        }

        public bool HasWorldSyncedData(string key)
        {
            return Program.ServerInstance.HasEntityProperty(1, key);
        }

        public bool SetEntityData(NetHandle entity, string key, object value)
        {
            lock (Program.ServerInstance.EntityProperties)
            {
                if (Program.ServerInstance.EntityProperties.ContainsKey(entity))
                {
                    Program.ServerInstance.EntityProperties[entity].Set(key, value);
                }
                else
                {
                    Program.ServerInstance.EntityProperties.Add(entity, new Dictionary<string, object>());
                    Program.ServerInstance.EntityProperties[entity].Set(key, value);
                }
            }
            return false;
        }

        public dynamic GetEntityData(NetHandle entity, string key)
        {
            lock (Program.ServerInstance.EntityProperties)
            {
                if (Program.ServerInstance.EntityProperties.ContainsKey(entity))
                {
                    return Program.ServerInstance.EntityProperties[entity].Get(key);
                }
            }
            return null;
        }

        public void ResetEntityData(NetHandle entity, string key)
        {
            lock (Program.ServerInstance.EntityProperties)
            {
                if (Program.ServerInstance.EntityProperties.ContainsKey(entity) && Program.ServerInstance.EntityProperties[entity].ContainsKey(key))
                {
                    Program.ServerInstance.EntityProperties[entity].Remove(key);
                }
            }
        }

        public bool HasEntityData(NetHandle entity, string key)
        {
            lock (Program.ServerInstance.EntityProperties)
            {
                if (Program.ServerInstance.EntityProperties.ContainsKey(entity))
                {
                    return Program.ServerInstance.EntityProperties[entity].ContainsKey(key);
                }
            }

            return false;
        }

        public string[] GetAllEntityData(NetHandle entity)
        {
            if (DoesEntityExist(entity) && Program.ServerInstance.EntityProperties.ContainsKey(entity))
            {
                return Program.ServerInstance.EntityProperties[entity].Select(pair => pair.Key).ToArray();
            }

            return new string[0];
        }

        public bool SetWorldData(string key, object value)
        {
            lock (Program.ServerInstance.WorldProperties)
            {
                Program.ServerInstance.WorldProperties.Set(key, value);
            }

            return true;
        }

        public dynamic GetWorldData(string key)
        {
            lock (Program.ServerInstance.WorldProperties)
            {
                return Program.ServerInstance.WorldProperties.Get(key);
            }
        }

        public string[] GetAllWorldData()
        {
            lock (Program.ServerInstance.WorldProperties)
            {
                return Program.ServerInstance.WorldProperties.Select(pair => pair.Key).ToArray();
            }
        }

        public void ResetWorldData(string key)
        {
            lock (Program.ServerInstance.WorldProperties)
            {
                Program.ServerInstance.WorldProperties.Remove(key);
            }
        }

        public bool HasWorldData(string key)
        {
            lock (Program.ServerInstance.WorldProperties)
            {
                return Program.ServerInstance.WorldProperties.ContainsKey(key);
            }
        }
    }
}

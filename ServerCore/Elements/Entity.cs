using RDRNetworkShared;
using System.Collections.Generic;
using System.Linq;

namespace ResuMPServer
{
    public abstract class Entity
    {
        internal Entity(API father, NetHandle handle)
        {
            Base = father;
            this.Id = handle;
        }

        public NetHandle Id { get; protected set; }

        public int Value
        {
            get { return Id.Value; }
        }

        protected API Base { get; set; }

        public static implicit operator NetHandle(Entity c)
        {
            return c.Id;
        }

        public override int GetHashCode()
        {
            return Id.Value;
        }

        public override bool Equals(object obj)
        {
            return (obj as NetHandle?)?.Value == Id.Value;
        }

        public static bool operator ==(Entity left, Entity right)
        {
            if ((object) left == null && (object) right == null) return true;
            if ((object) left == null || (object) right == null) return false;

            return left.Id == right.Id;
        }

        public static bool operator !=(Entity left, Entity right)
        {
            if ((object)left == null && (object)right == null) return false;
            if ((object)left == null || (object)right == null) return true;

            return left.Id != right.Id;
        }

        #region Properties
        /*
        public bool FreezePosition
        {
            set
            {
                Base.SetEntityPositionFrozen(this, value);
            }
        }*/

        public virtual Vector3 Position
        {
            set
            {
                Base.SetEntityPosition(this, value);
            }
            get
            {
                return Base.GetEntityPosition(this);
            }
        }

        public virtual Vector3 Rotation
        {
            set
            {
                Base.SetEntityRotation(this, value);
            }
            get
            {
                return Base.GetEntityRotation(this);
            }
        }

        public bool IsNull
        {
            get { return Id.IsNull; }
        }

        public bool Exists
        {
            get { return DoesEntityExist(); }
        }

        public EntityType Type
        {
            get
            {
                if (!DoesEntityExist()) return (EntityType)0;
                return (EntityType)Program.ServerInstance.NetEntityHandler.ToDict()[Value].EntityType;
            }
        }

        public virtual int Transparency
        {
            set
            {
                Base.SetEntityTransparency(this, value);
            }
            get { return Base.GetEntityTransparency(this); }
        }

        public int Dimension
        {
            set
            {
                Base.SetEntityDimension(this, value);
            }
            get { return Base.GetEntityDimension(this); }
        }

        public bool Invincible
        {
            set
            {
                Base.SetEntityInvincible(this, value);
            }
            get { return Base.GetEntityInvincible(this); }
        }

        public bool Collisionless
        {
            set
            {
                Base.SetEntityCollisionless(this, value);
            }
            get { return Base.GetEntityCollisionless(this); }
        }

        public int Model
        {
            get => Program.ServerInstance.NetEntityHandler.ToDict()[Value].ModelHash;
        }
        
        #endregion

        #region Methods

        public void Delete()
        {
            Base.DeleteEntity(this);
        }

        public void MovePosition(Vector3 target, int duration)
        {
            Base.MoveEntityPosition(this, target, duration);
        }

        public void MoveRotation(Vector3 target, int duration)
        {
            Base.MoveEntityRotation(this, target, duration);
        }

        public void AttachTo(NetHandle entity, string bone, Vector3 offset, Vector3 rotation)
        {
            Base.AttachEntityToEntity(this, entity, bone, offset, rotation);
        }

        public void Detach()
        {
            Base.DetachEntity(this);
        }

        public void Detach(bool resetCollision)
        {
            Base.DetachEntity(this, resetCollision);
        }
        /*
        public void CreateParticleEffect(string ptfxLib, string ptfxName, Vector3 offset, Vector3 rotation, float scale, int bone = -1)
        {
            Base.CreateParticleEffectOnEntity(ptfxLib, ptfxName, this, offset, rotation, scale, bone, Dimension);
        }
        */
        #region Data
        public bool SetData(string key, object value)
        {
            lock (Program.ServerInstance.EntityProperties)
            {
                if (Program.ServerInstance.EntityProperties.ContainsKey(this))
                {
                    Program.ServerInstance.EntityProperties[this].Set(key, value);
                }
                else
                {
                    Program.ServerInstance.EntityProperties.Add(this, new Dictionary<string, object>());
                    Program.ServerInstance.EntityProperties[this].Set(key, value);
                }
            }
            return false;
        }

        public dynamic GetData(string key)
        {
            lock (Program.ServerInstance.EntityProperties)
            {
                if (Program.ServerInstance.EntityProperties.ContainsKey(this))
                {
                    return Program.ServerInstance.EntityProperties[this].Get(key);
                }
            }
            return null;
        }

        public void ResetData(string key)
        {
            lock (Program.ServerInstance.EntityProperties)
            {
                if (Program.ServerInstance.EntityProperties.ContainsKey(this) && Program.ServerInstance.EntityProperties[this].ContainsKey(key))
                {
                    Program.ServerInstance.EntityProperties[this].Remove(key);
                }
            }
        }

        public bool HasData(string key)
        {
            lock (Program.ServerInstance.EntityProperties)
            {
                if (Program.ServerInstance.EntityProperties.ContainsKey(this))
                {
                    return Program.ServerInstance.EntityProperties[this].ContainsKey(key);
                }
            }

            return false;
        }

        public string[] GetAllEntityData(NetHandle entity)
        {
            if (DoesEntityExist() && Program.ServerInstance.EntityProperties.ContainsKey(entity))
            {
                return Program.ServerInstance.EntityProperties[entity].Select(pair => pair.Key).ToArray();
            }

            return new string[0];
        }

        public bool SetSyncedData(string key, object value)
        {
            if (DoesEntityExist())
            {
                return Program.ServerInstance.SetEntityProperty(this.Value, key, value);
            }
            return false;
        }

        public dynamic GetSyncedData(string key)
        {
            if (DoesEntityExist())
            {
                return Program.ServerInstance.GetEntityProperty(this.Value, key);
            }
            return null;
        }

        public string[] GetAllSyncedData(NetHandle entity)
        {
            if (DoesEntityExist())
            {
                return Program.ServerInstance.GetEntityAllProperties(this.Value);
            }
            return new string[0];
        }

        public void ResetSyncedData(NetHandle entity, string key)
        {
            if (DoesEntityExist())
            {
                Program.ServerInstance.ResetEntityProperty(this.Value, key);
            }
        }

        public bool HasEntitySyncedData(string key)
        {
            if (DoesEntityExist())
            {
                return Program.ServerInstance.HasEntityProperty(this.Value, key);
            }
            return false;
        }
        #endregion

        public bool DoesEntityExist()
        {
            return Program.ServerInstance.NetEntityHandler.ToDict().ContainsKey(this.Value);
        }


        
        #endregion
    }
}
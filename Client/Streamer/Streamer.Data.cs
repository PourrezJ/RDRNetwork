using RDR2.Math;
using RDRNetwork.Utils.Extentions;
using RDRNetworkShared;

namespace RDRNetwork
{
    internal static partial class Streamer
    {
        private static int _localHandleCounter = 0;

        internal static RemoteProp CreateObject(int model, RDR2.Math.Vector3 position, RDR2.Math.Vector3 rotation, bool dynamic, int netHash)
        {
            RemoteProp rem = new RemoteProp()
            {
                RemoteHandle = netHash,
                ModelHash = model,
                EntityType = 2,
                Position = position.ToLVector(),
                Rotation = rotation.ToLVector(),
                StreamedIn = false,
                LocalOnly = false,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHash, rem);
            }
            return rem;
        }

        internal static RemoteProp CreateObject(int netHandle, EntityProperties prop)
        {
            RemoteProp rem = new RemoteProp()
            {
                RemoteHandle = netHandle,

                Position = prop.Position,
                Rotation = prop.Rotation,
                Dimension = prop.Dimension,
                ModelHash = prop.ModelHash,
                EntityType = 2,
                Alpha = prop.Alpha,
                IsInvincible = prop.IsInvincible,
                SyncedProperties = prop.SyncedProperties,
                AttachedTo = prop.AttachedTo,
                Attachables = prop.Attachables,
                Flag = prop.Flag,
                PositionMovement = prop.PositionMovement,
                RotationMovement = prop.RotationMovement,

                StreamedIn = false,
                LocalOnly = false,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static RemoteVehicle CreateVehicle(int model, RDRNetworkShared.Vector3 position, RDRNetworkShared.Vector3 rotation, int netHash)
        {
            short vehComp = ~0;
            switch (model)
            {
                /*
                case unchecked((int)VehicleHash.Taxi):
                    vehComp = 1 << 5;
                    break;
                case (int)VehicleHash.Police:
                    vehComp = 1 << 2;
                    break;
                case (int)VehicleHash.Skylift:
                    vehComp = -1537;
                    break;*/
            }

            RemoteVehicle rem = new RemoteVehicle()
            {
                RemoteHandle = netHash,
                ModelHash = model,
                Position = position,
                Rotation = rotation,
                StreamedIn = false,
                LocalOnly = false,
                IsDead = false,
                Health = 1000,
                Alpha = 255,
                Livery = 0,
                NumberPlate = "NETWORK",
                EntityType = (byte)EntityType.Vehicle,
                PrimaryColor = 0,
                SecondaryColor = 0,
                Dimension = 0,
                VehicleComponents = vehComp,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHash, rem);
            }
            return rem;
        }

        internal static RemoteVehicle CreateVehicle(int netHandle, VehicleProperties prop)
        {
            RemoteVehicle rem = new RemoteVehicle()
            {
                RemoteHandle = netHandle,

                PrimaryColor = prop.PrimaryColor,
                SecondaryColor = prop.SecondaryColor,
                Health = prop.Health,
                IsDead = prop.IsDead,
                Mods = prop.Mods,
                Siren = prop.Siren,
                Doors = prop.Doors,
                Trailer = prop.Trailer,
                TraileredBy = prop.TraileredBy,
                Tires = prop.Tires,
                Livery = prop.Livery,
                NumberPlate = prop.NumberPlate,
                Position = prop.Position,
                Rotation = prop.Rotation,
                ModelHash = prop.ModelHash,
                EntityType = prop.EntityType,
                Dimension = prop.Dimension,
                Alpha = prop.Alpha,
                SyncedProperties = prop.SyncedProperties,
                AttachedTo = prop.AttachedTo,
                Attachables = prop.Attachables,
                IsInvincible = prop.IsInvincible,
                Flag = prop.Flag,
                VehicleComponents = prop.VehicleComponents,
                PositionMovement = prop.PositionMovement,
                RotationMovement = prop.RotationMovement,
                DamageModel = prop.DamageModel,

                StreamedIn = false,
                LocalOnly = false,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static RemotePed CreatePed(int netHandle, PedProperties prop)
        {
            RemotePed rem = new RemotePed()
            {
                RemoteHandle = netHandle,

                Position = prop.Position,
                Rotation = prop.Rotation,
                ModelHash = prop.ModelHash,
                EntityType = prop.EntityType,
                Dimension = prop.Dimension,
                Alpha = prop.Alpha,
                SyncedProperties = prop.SyncedProperties,
                AttachedTo = prop.AttachedTo,
                Attachables = prop.Attachables,
                Flag = prop.Flag,
                IsInvincible = prop.IsInvincible,
                PositionMovement = prop.PositionMovement,
                RotationMovement = prop.RotationMovement,

                LoopingAnimation = prop.LoopingAnimation,

                StreamedIn = false,
                LocalOnly = false,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static RemoteBlip CreateBlip(RDRNetworkShared.Vector3 pos, int netHandle)
        {
            RemoteBlip rem = new RemoteBlip()
            {
                RemoteHandle = netHandle,
                Position = pos,
                StreamedIn = false,
                LocalOnly = false,
                Alpha = 255,
                Dimension = 0,
                Sprite = 0,
                Scale = 1f,
                AttachedNetEntity = 0,
                EntityType = (byte)EntityType.Blip,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static RemoteBlip CreateBlip(int netHandle, BlipProperties prop)
        {
            RemoteBlip rem = new RemoteBlip()
            {
                RemoteHandle = netHandle,
                SyncedProperties = prop.SyncedProperties,
                Sprite = prop.Sprite,
                Scale = prop.Scale,
                Color = prop.Color,
                Dimension = prop.Dimension,
                IsShortRange = prop.IsShortRange,
                AttachedNetEntity = prop.AttachedNetEntity,
                Position = prop.Position,
                Rotation = prop.Rotation,
                ModelHash = prop.ModelHash,
                EntityType = (byte)EntityType.Blip,
                Alpha = prop.Alpha,
                IsInvincible = prop.IsInvincible,
                RangedBlip = prop.RangedBlip,
                AttachedTo = prop.AttachedTo,
                Attachables = prop.Attachables,
                PositionMovement = prop.PositionMovement,
                RotationMovement = prop.RotationMovement,
                Flag = prop.Flag,
                Name = prop.Name,
                RouteVisible = prop.RouteVisible,
                RouteColor = prop.RouteColor,

                StreamedIn = false,
                LocalOnly = false,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static RemoteBlip CreateBlip(IStreamedItem entity, int netHandle)
        {
            RemoteBlip rem = new RemoteBlip()
            {
                RemoteHandle = netHandle,
                AttachedNetEntity = entity.RemoteHandle,
                EntityType = (byte)EntityType.Blip,
                StreamedIn = false,
                LocalOnly = false,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static RemoteMarker CreateMarker(int netHandle, MarkerProperties prop)
        {
            RemoteMarker rem = new RemoteMarker()
            {
                RemoteHandle = netHandle,

                Direction = prop.Direction,
                MarkerType = prop.MarkerType,
                Red = prop.Red,
                Green = prop.Green,
                Blue = prop.Blue,
                Scale = prop.Scale,
                Position = prop.Position,
                Rotation = prop.Rotation,
                Dimension = prop.Dimension,
                BobUpAndDown = prop.BobUpAndDown,
                ModelHash = prop.ModelHash,
                EntityType = (byte)EntityType.Marker,
                Alpha = prop.Alpha,
                IsInvincible = prop.IsInvincible,
                SyncedProperties = prop.SyncedProperties,
                AttachedTo = prop.AttachedTo,
                Attachables = prop.Attachables,
                Flag = prop.Flag,

                PositionMovement = prop.PositionMovement,
                RotationMovement = prop.RotationMovement,

                StreamedIn = false,
                LocalOnly = false,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static RemoteTextLabel CreateTextLabel(int netHandle, TextLabelProperties prop)
        {
            RemoteTextLabel rem = new RemoteTextLabel()
            {
                RemoteHandle = netHandle,

                Red = prop.Red,
                Green = prop.Green,
                Blue = prop.Blue,
                Alpha = prop.Alpha,
                Size = prop.Size,
                Position = prop.Position,
                Dimension = prop.Dimension,
                EntityType = (byte)EntityType.TextLabel,
                Text = prop.Text,
                Range = prop.Range,
                IsInvincible = prop.IsInvincible,
                EntitySeethrough = prop.EntitySeethrough,
                SyncedProperties = prop.SyncedProperties,
                AttachedTo = prop.AttachedTo,
                Attachables = prop.Attachables,

                PositionMovement = prop.PositionMovement,
                RotationMovement = prop.RotationMovement,

                StreamedIn = false,
                LocalOnly = false,
                Flag = prop.Flag,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static RemoteParticle CreateParticle(int netHandle, ParticleProperties prop)
        {
            RemoteParticle rem = new RemoteParticle()
            {
                RemoteHandle = netHandle,

                Position = prop.Position,
                Rotation = prop.Rotation,
                ModelHash = prop.ModelHash,
                EntityType = prop.EntityType,
                Dimension = prop.Dimension,
                Alpha = prop.Alpha,
                SyncedProperties = prop.SyncedProperties,
                AttachedTo = prop.AttachedTo,
                Attachables = prop.Attachables,
                IsInvincible = prop.IsInvincible,
                Flag = prop.Flag,
                PositionMovement = prop.PositionMovement,
                RotationMovement = prop.RotationMovement,
                Library = prop.Library,
                Name = prop.Name,
                EntityAttached = prop.EntityAttached,
                BoneAttached = prop.BoneAttached,
                Scale = prop.Scale,

                StreamedIn = false,
                LocalOnly = false,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static RemotePickup CreatePickup(RDR2.Math.Vector3 pos, RDR2.Math.Vector3 rot, int pickupHash, int amount, int netHandle)
        {
            RemotePickup rem = new RemotePickup()
            {
                RemoteHandle = netHandle,
                Position = pos.ToLVector(),
                Rotation = rot.ToLVector(),
                ModelHash = pickupHash,
                Amount = amount,
                StreamedIn = false,
                LocalOnly = false,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static RemotePickup CreatePickup(int netHandle, PickupProperties prop)
        {
            RemotePickup rem = new RemotePickup()
            {
                RemoteHandle = netHandle,

                Amount = prop.Amount,
                PickedUp = prop.PickedUp,
                Position = prop.Position,
                Rotation = prop.Rotation,
                ModelHash = prop.ModelHash,
                EntityType = prop.EntityType,
                Alpha = prop.Alpha,
                Dimension = prop.Dimension,
                SyncedProperties = prop.SyncedProperties,
                AttachedTo = prop.AttachedTo,
                Attachables = prop.Attachables,
                IsInvincible = prop.IsInvincible,
                CustomModel = prop.CustomModel,

                PositionMovement = prop.PositionMovement,
                RotationMovement = prop.RotationMovement,

                Flag = prop.Flag,

                StreamedIn = false,
                LocalOnly = false,
            };

            lock (ClientMap)
            {
                ClientMap.Add(netHandle, rem);
            }
            return rem;
        }

        internal static int CreateLocalVehicle(int model, RDRNetworkShared.Vector3 pos, float heading)
        {
            var veh = CreateVehicle(model, pos, new RDRNetworkShared.Vector3(0, 0, heading), --_localHandleCounter);
            veh.LocalOnly = true;

            if (Count(typeof(RemoteVehicle)) < StreamerThread.MAX_VEHICLES)
                StreamIn(veh);

            return veh.RemoteHandle;
        }

        internal static int CreateLocalBlip(RDRNetworkShared.Vector3 pos)
        {
            var b = CreateBlip(pos, --_localHandleCounter);
            b.LocalOnly = true;

            if (Count(typeof(RemoteBlip)) < StreamerThread.MAX_BLIPS)
                StreamIn(b);

            return b.RemoteHandle;
        }

        internal static int CreateLocalObject(int model, RDR2.Math.Vector3 pos, RDR2.Math.Vector3 rot)
        {
            var p = CreateObject(model, pos, rot, false, --_localHandleCounter);
            p.LocalOnly = true;

            if (Count(typeof(RemoteProp)) < StreamerThread.MAX_OBJECTS)
                StreamIn(p);

            return p.RemoteHandle;
        }

        internal static int CreateLocalPickup(int model, RDR2.Math.Vector3 pos, RDR2.Math.Vector3 rot, int amount)
        {
            var p = CreatePickup(pos, rot, model, amount, --_localHandleCounter);
            p.LocalOnly = true;

            if (Count(typeof(RemotePickup)) < StreamerThread.MAX_PICKUPS)
                StreamIn(p);

            return p.RemoteHandle;
        }

        internal static int CreateLocalPed(int model, RDRNetworkShared.Vector3 pos, float heading)
        {
            var pp = new PedProperties();
            pp.EntityType = (byte)EntityType.Ped;
            pp.Position = pos;
            pp.Alpha = 255;
            pp.ModelHash = model;
            pp.Rotation = new RDRNetworkShared.Vector3(0, 0, heading);
            pp.Dimension = 0;

            var handle = --_localHandleCounter;

            var p = CreatePed(handle, pp);
            p.LocalOnly = true;
            p.RemoteHandle = handle;

            if (Count(typeof(RemotePed)) < StreamerThread.MAX_PEDS)
                StreamIn(p);

            return p.RemoteHandle;
        }

        internal static int CreateLocalLabel(string text, RDR2.Math.Vector3 pos, float range, float size, bool entitySeethrough, int dimension = 0)
        {
            var newId = --_localHandleCounter;
            RemoteTextLabel label = new RemoteTextLabel()
            {
                Position = pos.ToLVector(),
                Size = size,
                Alpha = 255,
                Red = 255,
                Green = 255,
                Blue = 255,
                Dimension = dimension,
                EntityType = (byte)EntityType.TextLabel,
                LocalOnly = true,
                RemoteHandle = newId,
                Text = text,
                Range = range,
                EntitySeethrough = entitySeethrough,
            };

            lock (ClientMap)
            {
                ClientMap.Add(newId, label);
            }

            if (Count(typeof(RemoteTextLabel)) < StreamerThread.MAX_LABELS)
                StreamIn(label);

            return newId;
        }
    }
}

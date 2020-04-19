using ResuMPServer.Constant;
using RDRNetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResuMPServer
{
    public partial class API
    {
        public void SetEntityPosition(NetHandle netHandle, Vector3 newPosition)
        {
            // why send this info to all players? Client have a streamer for this '--
            //SendNativeToAllPlayers(0x239A3351AC1DA385, new EntityArgument(netHandle.Value), newPosition.X, newPosition.Y, newPosition.Z, 0, 0, 0);

            if (DoesEntityExist(netHandle))
            {
                Program.ServerInstance.NetEntityHandler.ToDict()[netHandle.Value].Position = newPosition;

                var delta = new Delta_EntityProperties();

                delta.Position = newPosition;
                GameServer.UpdateEntityInfo(netHandle.Value, EntityType.Prop, delta);

                SetEntityData(netHandle, "__LAST_POSITION_SET", TickCount);
            }
        }

        public void MoveEntityPosition(NetHandle netHandle, Vector3 target, int duration)
        {
            if (DoesEntityExist(netHandle))
            {
                Program.ServerInstance.CreatePositionInterpolation(netHandle.Value, target, duration);
            }
        }

        public void MoveEntityRotation(NetHandle netHandle, Vector3 target, int duration)
        {
            if (DoesEntityExist(netHandle))
            {
                Program.ServerInstance.CreateRotationInterpolation(netHandle.Value, target, duration);
            }
        }

        public void AttachEntityToEntity(NetHandle entity, NetHandle entityTarget, string bone, Vector3 positionOffset, Vector3 rotationOffset)
        {
            if (DoesEntityExist(entity) && DoesEntityExist(entityTarget) && entity != entityTarget)
            {
                if (Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].AttachedTo != null)
                {
                    DetachEntity(entity, true);
                }

                Attachment info = new Attachment();

                info.NetHandle = entityTarget.Value;
                info.Bone = bone;
                info.PositionOffset = positionOffset;
                info.RotationOffset = rotationOffset;

                if (
                    Program.ServerInstance.NetEntityHandler.NetToProp<EntityProperties>(entityTarget.Value).Attachables ==
                    null)
                {
                    Program.ServerInstance.NetEntityHandler.NetToProp<EntityProperties>(entityTarget.Value).Attachables
                        = new List<int>();
                }

                Program.ServerInstance.NetEntityHandler.NetToProp<EntityProperties>(entity.Value).AttachedTo = info;
                Program.ServerInstance.NetEntityHandler.NetToProp<EntityProperties>(entityTarget.Value).Attachables.Add(entity.Value);

                var ent1 = new Delta_EntityProperties();
                ent1.AttachedTo = info;
                GameServer.UpdateEntityInfo(entity.Value, EntityType.Prop, ent1);

                var ent2 = new Delta_EntityProperties();
                ent2.Attachables = Program.ServerInstance.NetEntityHandler.NetToProp<EntityProperties>(entityTarget.Value).Attachables;
                GameServer.UpdateEntityInfo(entityTarget.Value, EntityType.Prop, ent2);
            }
        }

        public bool IsEntityAttachedToAnything(NetHandle entity)
        {
            if (DoesEntityExist(entity))
            {
                return Program.ServerInstance.NetEntityHandler.NetToProp<EntityProperties>(entity.Value).AttachedTo != null;
            }
            return false;
        }

        public bool IsEntityAttachedToEntity(NetHandle entity, NetHandle attachedTo)
        {
            if (DoesEntityExist(entity))
            {
                if (!IsEntityAttachedToAnything(entity)) return false;
                return Program.ServerInstance.NetEntityHandler.NetToProp<EntityProperties>(entity.Value).AttachedTo.NetHandle == attachedTo.Value;
            }

            return false;
        }

        public void DetachEntity(NetHandle entity, bool resetCollision = true)
        {
            if (DoesEntityExist(entity))
            {
                Program.ServerInstance.DetachEntity(entity.Value, resetCollision);
                var prop = Program.ServerInstance.NetEntityHandler.NetToProp<EntityProperties>(entity.Value);
                if (prop != null && prop.AttachedTo != null)
                {
                    var target =
                        Program.ServerInstance.NetEntityHandler.NetToProp<EntityProperties>(prop.AttachedTo.NetHandle);

                    if (target != null && target.Attachables != null)
                    {
                        target.Attachables.Remove(entity.Value);
                    }
                }
                prop.AttachedTo = null;
            }
        }

        public void SetPlayerSeatbelt(Client player, bool seatbelt)
        {
            SendNativeToPlayer(player, 0x1913FE4CBF41C463,
                new EntityArgument(player.Id.Value), 32, !seatbelt);
        }

        public bool GetPlayerSeatbelt(Client player)
        {
            return !FetchNativeFromPlayer<bool>(player, 0x7EE53118C892B513, new EntityArgument(player.Id.Value), 32, true);
        }

        public void FreezePlayer(Client player, bool freeze)
        {
            if (player.IsInVehicle)
            {
                SendNativeToPlayer(player, 0x428CA6DBD1094446, new EntityArgument(player.CurrentVehicle.Value), freeze);
            }

            SendNativeToPlayer(player, 0x428CA6DBD1094446, new EntityArgument(player.Id.Value), freeze);
        }

        public void SetEntityRotation(NetHandle netHandle, Vector3 newRotation)
        {
            SendNativeToAllPlayers(0x8524A8B0171D5E07, new EntityArgument(netHandle.Value), newRotation.X, newRotation.Y, newRotation.Z, 2, 1);

            if (DoesEntityExist(netHandle))
            {
                Program.ServerInstance.NetEntityHandler.ToDict()[netHandle.Value].Rotation = newRotation;

                var delta = new Delta_EntityProperties();
                delta.Rotation = newRotation;
                GameServer.UpdateEntityInfo(netHandle.Value, EntityType.Prop, delta);
            }
        }

        public Vector3 GetEntityPosition(NetHandle entity)
        {
            EntityProperties props = null;
            if (!Program.ServerInstance.NetEntityHandler.ToDict().TryGetValue(entity.Value, out props))
                return new Vector3();
            return props.Position ?? new Vector3();
        }

        public Vector3 GetEntityRotation(NetHandle entity)
        {
            EntityProperties props = null;
            if (!Program.ServerInstance.NetEntityHandler.ToDict().TryGetValue(entity.Value, out props))
                return new Vector3();
            return props.Rotation ?? new Vector3(0, 0, 0);
        }

        public Vector3 GetEntityVelocity(NetHandle entity)
        {
            EntityProperties props = null;
            if (!Program.ServerInstance.NetEntityHandler.ToDict().TryGetValue(entity.Value, out props))
                return new Vector3();
            return props.Velocity ?? new Vector3();
        }


        public void SetPlayerIntoVehicle(Client player, NetHandle vehicle, int seat)
        {
            //var start = Environment.TickCount;
            //while (!doesEntityExistForPlayer(player, vehicle) && Environment.TickCount - start < 1000) { }
            SendNativeToPlayer(player, 0xF75B0D629E1C063D, new LocalPlayerArgument(), new EntityArgument(vehicle.Value), seat);

            player.IsInVehicle = true;
            player.CurrentVehicle = vehicle;

            SetEntityData(player, "__LAST_POSITION_SET", TickCount);
        }

        public void WarpPlayerOutOfVehicle(Client player)
        {
            if (player.IsInVehicle)
            {
                SendNativeToPlayer(player, 0xD3DBCE61A490BE02, new LocalPlayerArgument(), new EntityArgument(player.Vehicle.Value), 16);
            }
        }

        public int GetPlayerVehicleSeat(Client player)
        {
            if (!player.IsInVehicle || player.CurrentVehicle.IsNull) return -3;
            return player.VehicleSeat;
        }

        public bool DoesEntityExist(NetHandle entity)
        {
            return Program.ServerInstance.NetEntityHandler.ToDict().ContainsKey(entity.Value);
        }

        public bool DoesEntityExistForPlayer(Client player, NetHandle entity)
        {
            return
                (bool)
                    Program.ServerInstance.ReturnNativeCallFromPlayer(player,0x7239B21A38F536BA, new BooleanArgument(),
                        new EntityArgument(entity.Value));
        }

        private void DeleteEntityInternal(NetHandle netHandle)
        {
            Program.ServerInstance.NetEntityHandler.DeleteEntity(netHandle.Value);
            if (Program.ServerInstance.EntityProperties.ContainsKey(netHandle))
                Program.ServerInstance.EntityProperties.Remove(netHandle);
        }

        public void DeleteEntity(NetHandle netHandle)
        {
            if (!netHandle.IsNull &&
                Program.ServerInstance.NetEntityHandler.NetToProp<EntityProperties>(netHandle.Value)?.EntityType ==
                (byte)EntityType.Player)
                return;

            DeleteEntityInternal(netHandle);
            lock (ResourceEntities) ResourceEntities.Remove(netHandle);
        }

        public void SetEntityTransparency(NetHandle entity, int newAlpha)
        {
            if (Program.ServerInstance.NetEntityHandler.ToDict().ContainsKey(entity.Value))
            {
                Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Alpha = (byte)newAlpha;
                if (newAlpha < 255)
                    Program.ServerInstance.SendNativeCallToAllPlayers(0x44A0870B7E92D7C0, new EntityArgument(entity.Value), newAlpha, false);
                else
                    Program.ServerInstance.SendNativeCallToAllPlayers(0x9B1E824FFBB7027A, new EntityArgument(entity.Value));

                var delta = new Delta_EntityProperties();

                delta.Alpha = (byte)newAlpha;
                GameServer.UpdateEntityInfo(entity.Value, EntityType.Prop, delta);

            }
        }

        public byte GetEntityTransparency(NetHandle entity)
        {
            if (Program.ServerInstance.NetEntityHandler.ToDict().ContainsKey(entity.Value))
            {
                return Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Alpha;
            }

            return 0;
        }

        public void SetEntityDimension(NetHandle entity, int dimension)
        {
            if (Program.ServerInstance.NetEntityHandler.ToDict().ContainsKey(entity.Value))
            {
                Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Dimension = dimension;

                var delta = new Delta_EntityProperties();
                delta.Dimension = dimension;
                GameServer.UpdateEntityInfo(entity.Value, EntityType.Prop, delta);

                KeyValuePair<int, EntityProperties> pair;
                if (
                    (pair =
                        Program.ServerInstance.NetEntityHandler.ToCopy()
                            .FirstOrDefault(
                                p => p.Value is BlipProperties && ((BlipProperties)p.Value).AttachedNetEntity ==
                        entity.Value)).Key != 0)
                {
                    Program.ServerInstance.NetEntityHandler.ToDict()[pair.Key].Dimension = dimension;

                    var deltaBlip = new Delta_EntityProperties();
                    deltaBlip.Dimension = dimension;
                    GameServer.UpdateEntityInfo(pair.Key, EntityType.Prop, deltaBlip);
                }

                if (
                   (pair =
                       Program.ServerInstance.NetEntityHandler.ToCopy()
                           .FirstOrDefault(
                               p => p.Value is ParticleProperties && ((ParticleProperties)p.Value).EntityAttached ==
                       entity.Value)).Key != 0)
                {
                    Program.ServerInstance.NetEntityHandler.ToDict()[pair.Key].Dimension = dimension;

                    var deltaBlip = new Delta_EntityProperties();
                    deltaBlip.Dimension = dimension;
                    GameServer.UpdateEntityInfo(pair.Key, EntityType.Prop, deltaBlip);
                }

                if (Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Attachables != null)
                    foreach (var attached in Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Attachables)
                    {
                        SetEntityDimension(new NetHandle(attached), dimension);
                    }
            }
        }

        public int GetEntityDimension(NetHandle entity)
        {
            if (Program.ServerInstance.NetEntityHandler.ToDict().ContainsKey(entity.Value))
            {
                return Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Dimension;
            }

            return 0;
        }

        public void SetEntityInvincible(NetHandle entity, bool invincible)
        {
            if (Program.ServerInstance.NetEntityHandler.ToDict().ContainsKey(entity.Value))
            {
                Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].IsInvincible = invincible;

                var delta = new Delta_EntityProperties();
                delta.IsInvincible = invincible;
                GameServer.UpdateEntityInfo(entity.Value, EntityType.Prop, delta);

                SendNativeToAllPlayers(0x3882114BDE571AD4, entity, invincible);
            }
        }

        public bool GetEntityInvincible(NetHandle entity)
        {
            if (Program.ServerInstance.NetEntityHandler.ToDict().ContainsKey(entity.Value))
            {
                return Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].IsInvincible;
            }

            return false;
        }

        public void SetEntityCollisionless(NetHandle entity, bool collisionless)
        {
            if (DoesEntityExist(entity))
            {
                Program.ServerInstance.SendNativeCallToAllPlayers(0x1A9205C1B9EE827F, entity, !collisionless, true);

                if (!collisionless)
                {
                    Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Flag = (byte)
                        PacketOptimization.ResetBit(Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Flag,
                            EntityFlag.Collisionless);
                }
                else
                {
                    Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Flag = (byte)
                        PacketOptimization.SetBit(Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Flag,
                            EntityFlag.Collisionless);

                }

                var delta = new Delta_EntityProperties();
                delta.Flag = Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Flag;
                GameServer.UpdateEntityInfo(entity.Value, EntityType.Prop, delta);
            }
        }

        public bool GetEntityCollisionless(NetHandle entity)
        {
            if (DoesEntityExist(entity))
            {
                return PacketOptimization.CheckBit(Program.ServerInstance.NetEntityHandler.ToDict()[entity.Value].Flag, EntityFlag.Collisionless);
            }

            return false;
        }

        public void PlayPedAnimation(NetHandle ped, bool looped, string animDict, string animName)
        {
            PedProperties prop;
            if ((prop = Program.ServerInstance.NetEntityHandler.NetToProp<PedProperties>(ped.Value)) != null)
            {
                if (looped)
                {
                    prop.LoopingAnimation = animDict + " " + animName;

                    var delta = new Delta_PedProperties();
                    delta.LoopingAnimation = prop.LoopingAnimation;
                    GameServer.UpdateEntityInfo(ped.Value, EntityType.Ped, delta);
                }

                SendNativeToAllPlayers(0xEA47FE3719165B94, ped, animDict, animName, -8f, -10f, -1, looped ? 1 : 0, 8f, false, false, false);
            }
        }

        public void PlayPedScenario(NetHandle ped, string scenario)
        {
            PedProperties prop;
            if ((prop = Program.ServerInstance.NetEntityHandler.NetToProp<PedProperties>(ped.Value)) != null)
            {
                prop.LoopingAnimation = scenario;

                var delta = new Delta_PedProperties();
                delta.LoopingAnimation = prop.LoopingAnimation;
                GameServer.UpdateEntityInfo(ped.Value, EntityType.Ped, delta);

                SendNativeToAllPlayers(0x142A02425FF02BD9, ped, scenario, 0, false);
            }
        }

        public void StopPedAnimation(NetHandle ped)
        {
            PedProperties prop;
            if ((prop = Program.ServerInstance.NetEntityHandler.NetToProp<PedProperties>(ped.Value)) != null)
            {
                prop.LoopingAnimation = "";

                var delta = new Delta_PedProperties();
                delta.LoopingAnimation = prop.LoopingAnimation;
                GameServer.UpdateEntityInfo(ped.Value, EntityType.Ped, delta);

                SendNativeToAllPlayers(0xE1EF3C1216AFF2CD, ped);
            }
        }
    }
}

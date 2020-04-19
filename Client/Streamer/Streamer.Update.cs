using RDR2;
using RDR2.Native;
using RDRNetwork.Models;
using RDRNetwork.Sync;
using RDRNetwork.Utils;
using RDRNetwork.Utils.Extentions;
using RDRNetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RDRNetwork
{
    internal static partial class Streamer
    {
        internal static void UpdatePlayer(int netHandle, PlayerProperties prop)
        {
            RemotePlayer rem = NetToStreamedItem(netHandle) as RemotePlayer;
            if (rem == null) return;

            rem.Props = prop.Props;
            rem.Textures = prop.Textures;
            rem.Team = prop.Team;
            rem.BlipSprite = prop.BlipSprite;
            rem.BlipColor = prop.BlipColor;
            rem.BlipAlpha = prop.BlipAlpha;
            rem.Accessories = prop.Accessories;
            rem.Name = prop.Name;
            rem.ModelHash = prop.ModelHash;
            rem.EntityType = prop.EntityType;
            rem.Alpha = prop.Alpha;
            rem.Dimension = prop.Dimension;
            rem.RemoteHandle = netHandle;
            rem.IsInvincible = prop.IsInvincible;
            rem.SyncedProperties = prop.SyncedProperties;
            rem.AttachedTo = prop.AttachedTo;
            rem.Attachables = prop.Attachables;
            rem.Flag = prop.Flag;
            rem.PositionMovement = prop.PositionMovement;
            rem.RotationMovement = prop.RotationMovement;
            rem.WeaponTints = prop.WeaponTints;
            rem.WeaponComponents = prop.WeaponComponents;
            rem.NametagText = prop.NametagText;
            rem.NametagSettings = prop.NametagSettings;

            if (!(rem is SyncPed)) return;
            if (prop.Position != null)
                ((SyncPed)rem).Position = prop.Position.ToVector();
            if (prop.Rotation != null)
                ((SyncPed)rem).Rotation = prop.Rotation.ToVector();

            //((SyncPed)rem).DirtyWeapons = true;
        }

        internal static void UpdatePlayer(int netHandle, Delta_PlayerProperties prop)
        {
            LogManager.DebugLog("UPDATING PLAYER " + netHandle + " PROP NULL? " + (prop == null));

            if (IsLocalPlayer(NetToStreamedItem(netHandle)))
            {
                UpdateRemotePlayer(netHandle, prop);
                return;
            }

            if (prop == null) return;
            var veh = GetPlayer(netHandle);
            if (prop.Props != null) veh.Props = prop.Props;
            if (prop.Textures != null) veh.Textures = prop.Textures;
            if (prop.BlipSprite != null) veh.BlipSprite = prop.BlipSprite.Value;
            if (prop.Team != null) veh.Team = prop.Team.Value;
            if (prop.BlipColor != null) veh.BlipColor = prop.BlipColor.Value;
            if (prop.BlipAlpha != null) veh.BlipAlpha = prop.BlipAlpha.Value;
            if (prop.Accessories != null) veh.Accessories = prop.Accessories;
            if (prop.IsInvincible != null) veh.IsInvincible = prop.IsInvincible.Value;
            if (prop.WeaponTints != null)
            {
                veh.WeaponTints = prop.WeaponTints;
               // veh.DirtyWeapons = true;
            }
            if (prop.WeaponComponents != null)
            {
                veh.WeaponComponents = prop.WeaponComponents;
              //  veh.DirtyWeapons = true;
            }
            if (prop.Name != null)
            {
                veh.Name = prop.Name;
                LogManager.DebugLog("New name: " + prop.Name);
            }
            if (prop.Position != null) veh.Position = prop.Position.ToVector();
            if (prop.Rotation != null) veh.Rotation = prop.Rotation.ToVector();
            if (prop.ModelHash != null) veh.ModelHash = prop.ModelHash.Value;
            if (prop.EntityType != null) veh.EntityType = prop.EntityType.Value;
            if (prop.Alpha != null) veh.Alpha = prop.Alpha.Value;
            if (prop.Flag != null) veh.Flag = prop.Flag.Value;

            if (prop.NametagText != null) veh.NametagText = prop.NametagText;
            if (prop.NametagSettings != null) veh.NametagSettings = prop.NametagSettings.Value;

            if (prop.Dimension != null)
            {
                veh.Dimension = prop.Dimension.Value;
                //if (veh.Dimension != Main.LocalDimension && veh.StreamedIn && veh.Dimension != 0) StreamOut(veh);
            }

            if (prop.Attachables != null) veh.Attachables = prop.Attachables;
            if (prop.AttachedTo != null)
            {
                veh.AttachedTo = prop.AttachedTo;
                var attachedTo = NetToStreamedItem(prop.AttachedTo.NetHandle);
                if (attachedTo != null)
                {
                    AttachEntityToEntity(veh as IStreamedItem, attachedTo, prop.AttachedTo);
                }
            }
            if (prop.SyncedProperties != null)
            {
                if (veh.SyncedProperties == null) veh.SyncedProperties = new Dictionary<string, NativeArgument>();
                foreach (var pair in prop.SyncedProperties)
                {
                    if (pair.Value is LocalGamePlayerArgument)
                        veh.SyncedProperties.Remove(pair.Key);
                    else
                        veh.SyncedProperties.Set(pair.Key, pair.Value);
                }
            }

            if (prop.PositionMovement != null) veh.PositionMovement = prop.PositionMovement;
            if (prop.RotationMovement != null) veh.RotationMovement = prop.RotationMovement;
        }

        internal static void UpdateRemotePlayer(int netHandle, Delta_PlayerProperties prop)
        {
            RemotePlayer veh = NetToStreamedItem(netHandle) as RemotePlayer;
            if (prop == null || veh == null) return;
            if (prop.Props != null) veh.Props = prop.Props;
            if (prop.Textures != null) veh.Textures = prop.Textures;
            if (prop.BlipSprite != null) veh.BlipSprite = prop.BlipSprite.Value;
            if (prop.Team != null) veh.Team = prop.Team.Value;
            if (prop.BlipColor != null) veh.BlipColor = prop.BlipColor.Value;
            if (prop.BlipAlpha != null) veh.BlipAlpha = prop.BlipAlpha.Value;
            if (prop.Accessories != null) veh.Accessories = prop.Accessories;
            if (prop.Name != null)
            {
                veh.Name = prop.Name;
                LogManager.DebugLog("New name: " + prop.Name);
            }
            if (prop.Position != null) veh.Position = prop.Position;
            if (prop.Rotation != null) veh.Rotation = prop.Rotation;
            if (prop.WeaponTints != null) veh.WeaponTints = prop.WeaponTints;
            if (prop.WeaponComponents != null) veh.WeaponComponents = prop.WeaponComponents;
            if (prop.ModelHash != null) veh.ModelHash = prop.ModelHash.Value;
            if (prop.EntityType != null) veh.EntityType = prop.EntityType.Value;
            if (prop.Alpha != null) veh.Alpha = prop.Alpha.Value;
            if (prop.Flag != null) veh.Flag = prop.Flag.Value;
            if (prop.IsInvincible != null) veh.IsInvincible = prop.IsInvincible.Value;
            if (prop.NametagText != null) veh.NametagText = prop.NametagText;
            if (prop.NametagSettings != null) veh.NametagSettings = prop.NametagSettings.Value;

            if (prop.Dimension != null)
            {
                veh.Dimension = prop.Dimension.Value;
                //if (veh.Dimension != Main.LocalDimension && veh.StreamedIn && veh.Dimension != 0) StreamOut(veh);
            }

            if (prop.Attachables != null) veh.Attachables = prop.Attachables;
            if (prop.AttachedTo != null)
            {
                veh.AttachedTo = prop.AttachedTo;
                var attachedTo = NetToStreamedItem(prop.AttachedTo.NetHandle);
                if (attachedTo != null)
                {
                    AttachEntityToEntity(veh as IStreamedItem, attachedTo, prop.AttachedTo);
                }
            }
            if (prop.SyncedProperties != null)
            {
                if (veh.SyncedProperties == null) veh.SyncedProperties = new Dictionary<string, NativeArgument>();
                foreach (var pair in prop.SyncedProperties)
                {
                    if (pair.Value is LocalGamePlayerArgument)
                        veh.SyncedProperties.Remove(pair.Key);
                    else
                        veh.SyncedProperties.Set(pair.Key, pair.Value);
                }
            }

            if (prop.PositionMovement != null) veh.PositionMovement = prop.PositionMovement;
            if (prop.RotationMovement != null) veh.RotationMovement = prop.RotationMovement;
        }

        internal static void UpdateWorld(Delta_EntityProperties prop)
        {
            if (prop == null || ServerWorld == null) return;

            if (prop.Position != null) ServerWorld.Position = prop.Position;
            if (prop.Rotation != null) ServerWorld.Rotation = prop.Rotation;
            if (prop.ModelHash != null) ServerWorld.ModelHash = prop.ModelHash.Value;
            if (prop.EntityType != null) ServerWorld.EntityType = prop.EntityType.Value;
            if (prop.Alpha != null) ServerWorld.Alpha = prop.Alpha.Value;
            if (prop.Flag != null) ServerWorld.Flag = prop.Flag.Value;

            if (prop.Dimension != null)
            {
                ServerWorld.Dimension = prop.Dimension.Value;
            }

            if (prop.Attachables != null) ServerWorld.Attachables = prop.Attachables;
            if (prop.AttachedTo != null)
            {
                ServerWorld.AttachedTo = prop.AttachedTo;

            }
            if (prop.SyncedProperties != null)
            {
                if (ServerWorld.SyncedProperties == null) ServerWorld.SyncedProperties = new Dictionary<string, NativeArgument>();
                foreach (var pair in prop.SyncedProperties)
                {
                    if (pair.Value is LocalGamePlayerArgument)
                        ServerWorld.SyncedProperties.Remove(pair.Key);
                    else
                    {
                        NativeArgument oldValue = ServerWorld.SyncedProperties.Get(pair.Key);

                        ServerWorld.SyncedProperties.Set(pair.Key, pair.Value);

                       // JavascriptHook.InvokeDataChangeEvent(new LocalHandle(0), pair.Key, Main.DecodeArgumentListPure(oldValue).FirstOrDefault());
                    }
                }
            }
        }

        internal static void UpdateVehicle(int netHandle, Delta_VehicleProperties prop)
        {
            RemoteVehicle veh = null;
            if (prop == null || (veh = (Streamer.NetToStreamedItem(netHandle) as RemoteVehicle)) == null) return;

            if (prop.PrimaryColor != null) veh.PrimaryColor = prop.PrimaryColor.Value;
            if (prop.SecondaryColor != null) veh.SecondaryColor = prop.SecondaryColor.Value;
            if (prop.Health != null) veh.Health = prop.Health.Value;
            if (prop.IsDead != null) veh.IsDead = prop.IsDead.Value;
            if (prop.Mods != null)
            {
                var oldMods = veh.Mods;
                veh.Mods = prop.Mods;
                if (veh.StreamedIn)
                {
                    var car = new Vehicle(NetToEntity(veh)?.Handle ?? 0);

                    if (car.Handle != 0)
                        foreach (var pair in prop.Mods.Where(pair => !oldMods.ContainsKey(pair.Key) || oldMods[pair.Key] != pair.Value))
                        {
                            //if (pair.Key <= 60)
                            //{
                            //    if (prop.Mods.ContainsKey(pair.Key))
                            //    {
                            //        if (pair.Key >= 17 && pair.Key <= 22)
                            //            car.Mods[(VehicleToggleModType)pair.Key].IsInstalled = pair.Value != 0;
                            //        else
                            //            car.SetMod(pair.Key, pair.Value, false);
                            //    }
                            //    else
                            //    {
                            //        Function.Call(Hash.REMOVE_VEHICLE_MOD, car, pair.Key);
                            //    }
                            //}
                            //else
                            //{
                            //    car.SetNonStandardVehicleMod(pair.Key, pair.Value);
                            //}
                        }
                }
            }
            if (prop.Siren != null) veh.Siren = prop.Siren.Value;
            if (prop.Doors != null) veh.Doors = prop.Doors.Value;
            if (prop.Trailer != null) veh.Trailer = prop.Trailer.Value;
            if (prop.TraileredBy != null) veh.TraileredBy = prop.TraileredBy.Value;
            if (prop.Tires != null) veh.Tires = prop.Tires.Value;
            if (prop.Livery != null) veh.Livery = prop.Livery.Value;
            if (prop.NumberPlate != null)
            {
                veh.NumberPlate = prop.NumberPlate;

                if (veh.StreamedIn && Regex.IsMatch(prop.NumberPlate, "^[a-zA-Z0-9]{0,9}$"))
                {
                    //new Vehicle(veh.LocalHandle).Mods.LicensePlate = prop.NumberPlate;
                }
            }
            if (prop.Position != null) veh.Position = prop.Position;
            if (prop.Rotation != null) veh.Rotation = prop.Rotation;
            if (prop.ModelHash != null) veh.ModelHash = prop.ModelHash.Value;
            if (prop.EntityType != null) veh.EntityType = prop.EntityType.Value;
            if (prop.Alpha != null) veh.Alpha = prop.Alpha.Value;
            if (prop.Flag != null) veh.Flag = prop.Flag.Value;
            if (prop.VehicleComponents != null) veh.VehicleComponents = prop.VehicleComponents.Value;
            if (prop.IsInvincible != null) veh.IsInvincible = prop.IsInvincible.Value;
            if (prop.DamageModel != null) veh.DamageModel = prop.DamageModel;

            if (prop.Dimension != null)
            {
                veh.Dimension = prop.Dimension.Value;
                //if (veh.Dimension != Main.LocalDimension && veh.StreamedIn && veh.Dimension != 0) StreamOut(veh);
            }

            if (prop.Attachables != null) veh.Attachables = prop.Attachables;
            if (prop.AttachedTo != null)
            {
                veh.AttachedTo = prop.AttachedTo;
                
                var attachedTo = NetToStreamedItem(prop.AttachedTo.NetHandle);
                if (attachedTo != null)
                {
                    AttachEntityToEntity(veh as IStreamedItem, attachedTo, prop.AttachedTo);
                }
            }
            if (prop.SyncedProperties != null)
            {
                if (veh.SyncedProperties == null) veh.SyncedProperties = new Dictionary<string, NativeArgument>();
                foreach (var pair in prop.SyncedProperties)
                {
                    if (pair.Value is LocalGamePlayerArgument)
                        veh.SyncedProperties.Remove(pair.Key);
                    else
                        veh.SyncedProperties.Set(pair.Key, pair.Value);
                }
            }

            if (prop.PositionMovement != null) veh.PositionMovement = prop.PositionMovement;
            if (prop.RotationMovement != null) veh.RotationMovement = prop.RotationMovement;
        }

        internal static void UpdateBlip(int netHandle, Delta_BlipProperties prop)
        {
            IStreamedItem item = null;
            if (prop == null || (item = NetToStreamedItem(netHandle)) == null) return;
            var blip = item as RemoteBlip;
            if (prop.Sprite != null) blip.Sprite = prop.Sprite.Value;
            if (prop.Scale != null) blip.Scale = prop.Scale.Value;
            if (prop.Color != null) blip.Color = prop.Color.Value;
            if (prop.IsShortRange != null) blip.IsShortRange = prop.IsShortRange.Value;
            if (prop.AttachedNetEntity != null) blip.AttachedNetEntity = prop.AttachedNetEntity.Value;
            if (prop.Position != null) blip.Position = prop.Position;
            if (prop.Rotation != null) blip.Rotation = prop.Rotation;
            if (prop.ModelHash != null) blip.ModelHash = prop.ModelHash.Value;
            if (prop.EntityType != null) blip.EntityType = prop.EntityType.Value;
            if (prop.Alpha != null) blip.Alpha = prop.Alpha.Value;
            if (prop.Flag != null) blip.Flag = prop.Flag.Value;
            if (prop.RangedBlip != null) blip.RangedBlip = prop.RangedBlip.Value;
            if (prop.IsInvincible != null) blip.IsInvincible = prop.IsInvincible.Value;
            if (prop.Name != null) blip.Name = prop.Name;
            if (prop.RouteVisible != null) blip.RouteVisible = prop.RouteVisible.Value;
            if (prop.RouteColor != null) blip.RouteColor = prop.RouteColor.Value;

            if (prop.Dimension != null)
            {
                blip.Dimension = prop.Dimension.Value;
                //if (blip.Dimension != Main.LocalDimension && item.StreamedIn && blip.Dimension != 0) StreamOut(item);
            }

            if (prop.Attachables != null) blip.Attachables = prop.Attachables;
            if (prop.AttachedTo != null)
            {
                blip.AttachedTo = prop.AttachedTo;
                var attachedTo = NetToStreamedItem(prop.AttachedTo.NetHandle);
                if (attachedTo != null)
                {
                    AttachEntityToEntity(blip as IStreamedItem, attachedTo, prop.AttachedTo);
                }
            }
            if (prop.SyncedProperties != null)
            {
                if (blip.SyncedProperties == null) blip.SyncedProperties = new Dictionary<string, NativeArgument>();
                foreach (var pair in prop.SyncedProperties)
                {
                    if (pair.Value is LocalGamePlayerArgument)
                        blip.SyncedProperties.Remove(pair.Key);
                    else
                        blip.SyncedProperties.Set(pair.Key, pair.Value);
                }
            }

            if (prop.PositionMovement != null) blip.PositionMovement = prop.PositionMovement;
            if (prop.RotationMovement != null) blip.RotationMovement = prop.RotationMovement;
        }

        internal static void UpdateParticle(int netHandle, Delta_ParticleProperties prop)
        {
            RemoteParticle veh = null;
            if (prop == null || (veh = (NetToStreamedItem(netHandle) as RemoteParticle)) == null) return;

            if (prop.Position != null) veh.Position = prop.Position;
            if (prop.Rotation != null) veh.Rotation = prop.Rotation;
            if (prop.ModelHash != null) veh.ModelHash = prop.ModelHash.Value;
            if (prop.EntityType != null) veh.EntityType = prop.EntityType.Value;
            if (prop.Alpha != null) veh.Alpha = prop.Alpha.Value;
            if (prop.Flag != null) veh.Flag = prop.Flag.Value;
            if (prop.IsInvincible != null) veh.IsInvincible = prop.IsInvincible.Value;
            if (prop.Name != null) veh.Name = prop.Name;
            if (prop.Library != null) veh.Library = prop.Library;
            if (prop.BoneAttached != null) veh.BoneAttached = prop.BoneAttached.Value;
            if (prop.Scale != null) veh.Scale = prop.Scale.Value;
            if (prop.EntityAttached != null) veh.EntityAttached = prop.EntityAttached.Value;

            if (prop.Dimension != null)
            {
                veh.Dimension = prop.Dimension.Value;
                //if (veh.Dimension != Main.LocalDimension && veh.StreamedIn && veh.Dimension != 0) StreamOut(veh);
            }

            if (prop.Attachables != null) veh.Attachables = prop.Attachables;
            if (prop.AttachedTo != null)
            {
                veh.AttachedTo = prop.AttachedTo;
                var attachedTo = NetToStreamedItem(prop.AttachedTo.NetHandle);
                if (attachedTo != null)
                {
                    AttachEntityToEntity(veh as IStreamedItem, attachedTo, prop.AttachedTo);
                }
            }
            if (prop.SyncedProperties != null)
            {
                if (veh.SyncedProperties == null) veh.SyncedProperties = new Dictionary<string, NativeArgument>();
                foreach (var pair in prop.SyncedProperties)
                {
                    if (pair.Value is LocalGamePlayerArgument)
                        veh.SyncedProperties.Remove(pair.Key);
                    else
                        veh.SyncedProperties.Set(pair.Key, pair.Value);
                }
            }

            if (prop.PositionMovement != null) veh.PositionMovement = prop.PositionMovement;
            if (prop.RotationMovement != null) veh.RotationMovement = prop.RotationMovement;
        }

        internal static void UpdatePickup(int netHandle, Delta_PickupProperties prop)
        {
            IStreamedItem item = null;
            if (prop == null || (item = NetToStreamedItem(netHandle)) == null) return;
            var veh = item as RemotePickup;
            if (prop.Amount != null) veh.Amount = prop.Amount.Value;
            if (prop.PickedUp != null) veh.PickedUp = prop.PickedUp.Value;
            if (prop.Position != null) veh.Position = prop.Position;
            if (prop.Rotation != null) veh.Rotation = prop.Rotation;
            if (prop.ModelHash != null) veh.ModelHash = prop.ModelHash.Value;
            if (prop.EntityType != null) veh.EntityType = prop.EntityType.Value;
            if (prop.Alpha != null) veh.Alpha = prop.Alpha.Value;
            if (prop.Flag != null) veh.Flag = prop.Flag.Value;
            if (prop.IsInvincible != null) veh.IsInvincible = prop.IsInvincible.Value;
            if (prop.CustomModel != null) veh.CustomModel = prop.CustomModel.Value;

            if (prop.Dimension != null)
            {
                veh.Dimension = prop.Dimension.Value;
                //if (veh.Dimension != Main.LocalDimension && item.StreamedIn && veh.Dimension != 0) StreamOut(item);
            }

            if (prop.Attachables != null) veh.Attachables = prop.Attachables;
            if (prop.AttachedTo != null)
            {
                veh.AttachedTo = prop.AttachedTo;
                var attachedTo = NetToStreamedItem(prop.AttachedTo.NetHandle);
                if (attachedTo != null)
                {
                    AttachEntityToEntity(veh as IStreamedItem, attachedTo, prop.AttachedTo);
                }
            }
            if (prop.SyncedProperties != null)
            {
                if (veh.SyncedProperties == null) veh.SyncedProperties = new Dictionary<string, NativeArgument>();
                foreach (var pair in prop.SyncedProperties)
                {
                    if (pair.Value is LocalGamePlayerArgument)
                        veh.SyncedProperties.Remove(pair.Key);
                    else
                        veh.SyncedProperties.Set(pair.Key, pair.Value);
                }
            }

            if (prop.PositionMovement != null) veh.PositionMovement = prop.PositionMovement;
            if (prop.RotationMovement != null) veh.RotationMovement = prop.RotationMovement;
        }

        internal static void UpdatePed(int netHandle, Delta_PedProperties prop)
        {
            RemotePed veh = null;
            if (prop == null || (veh = (NetToStreamedItem(netHandle) as RemotePed)) == null) return;

            if (prop.Position != null) veh.Position = prop.Position;
            if (prop.Rotation != null) veh.Rotation = prop.Rotation;
            if (prop.ModelHash != null) veh.ModelHash = prop.ModelHash.Value;
            if (prop.EntityType != null) veh.EntityType = prop.EntityType.Value;
            if (prop.Alpha != null) veh.Alpha = prop.Alpha.Value;
            if (prop.Flag != null) veh.Flag = prop.Flag.Value;
            if (prop.LoopingAnimation != null) veh.LoopingAnimation = prop.LoopingAnimation;
            if (prop.IsInvincible != null) veh.IsInvincible = prop.IsInvincible.Value;

            if (prop.Dimension != null)
            {
                veh.Dimension = prop.Dimension.Value;
                //if (veh.Dimension != Main.LocalDimension && veh.StreamedIn && veh.Dimension != 0) StreamOut(veh);
            }

            if (prop.Attachables != null) veh.Attachables = prop.Attachables;
            if (prop.AttachedTo != null)
            {
                veh.AttachedTo = prop.AttachedTo;
                var attachedTo = NetToStreamedItem(prop.AttachedTo.NetHandle);
                if (attachedTo != null)
                {
                    AttachEntityToEntity(veh as IStreamedItem, attachedTo, prop.AttachedTo);
                }
            }
            if (prop.SyncedProperties != null)
            {
                if (veh.SyncedProperties == null) veh.SyncedProperties = new Dictionary<string, NativeArgument>();
                foreach (var pair in prop.SyncedProperties)
                {
                    if (pair.Value is LocalGamePlayerArgument)
                        veh.SyncedProperties.Remove(pair.Key);
                    else
                        veh.SyncedProperties.Set(pair.Key, pair.Value);
                }
            }

            if (prop.PositionMovement != null) veh.PositionMovement = prop.PositionMovement;
            if (prop.RotationMovement != null) veh.RotationMovement = prop.RotationMovement;
        }

        internal static void UpdateProp(int netHandle, Delta_EntityProperties prop)
        {
            IStreamedItem item;
            if (prop == null || (item = NetToStreamedItem(netHandle)) == null) return;
            var veh = item as EntityProperties;
            if (veh == null) return;
            if (prop.Position != null) veh.Position = prop.Position;
            if (prop.Rotation != null) veh.Rotation = prop.Rotation;
            if (prop.ModelHash != null) veh.ModelHash = prop.ModelHash.Value;
            if (prop.EntityType != null) veh.EntityType = prop.EntityType.Value;
            if (prop.Alpha != null) veh.Alpha = prop.Alpha.Value;
            if (prop.Flag != null) veh.Flag = prop.Flag.Value;
            if (prop.IsInvincible != null) veh.IsInvincible = prop.IsInvincible.Value;

            if (prop.Dimension != null)
            {
                veh.Dimension = prop.Dimension.Value;

                var localPl = item as RemotePlayer;
                if (localPl != null && localPl.LocalHandle == -2) Main.LocalDimension = prop.Dimension.Value;
                //else if (veh.Dimension != Main.LocalDimension && item.StreamedIn && veh.Dimension != 0) StreamOut(item);
            }

            if (prop.Attachables != null) veh.Attachables = prop.Attachables;
            if (prop.AttachedTo != null)
            {
                veh.AttachedTo = prop.AttachedTo;
                var attachedTo = NetToStreamedItem(prop.AttachedTo.NetHandle);
                if (attachedTo != null)
                {
                    LogManager.DebugLog("ATTACHING THIS ENTITY (" + (veh.EntityType) + " id: " + netHandle + ") TO " + attachedTo.GetType());
                    AttachEntityToEntity(veh as IStreamedItem, attachedTo, prop.AttachedTo);
                }
            }

            if (prop.SyncedProperties != null)
            {
                if (veh.SyncedProperties == null) veh.SyncedProperties = new Dictionary<string, NativeArgument>();
                foreach (var pair in prop.SyncedProperties)
                {
                    if (pair.Value is LocalGamePlayerArgument)
                    {
                        veh.SyncedProperties.Remove(pair.Key);
                    }
                    else
                    {
                        NativeArgument oldValue = veh.SyncedProperties.Get(pair.Key);

                        veh.SyncedProperties.Set(pair.Key, pair.Value);

                        var ent = new LocalHandle(NetToEntity(veh as IStreamedItem)?.Handle ?? 0);
                        //if (!ent.IsNull) JavascriptHook.InvokeDataChangeEvent(ent, pair.Key, Main.DecodeArgumentListPure(oldValue).FirstOrDefault());
                    }
                }
            }

            if (prop.PositionMovement != null) veh.PositionMovement = prop.PositionMovement;
            if (prop.RotationMovement != null) veh.RotationMovement = prop.RotationMovement;
        }

        internal static void UpdateTextLabel(int netHandle, Delta_TextLabelProperties prop)
        {
            RemoteTextLabel veh = null;
            if (prop == null || (veh = (NetToStreamedItem(netHandle) as RemoteTextLabel)) == null) return;

            if (prop.Position != null) veh.Position = prop.Position;
            if (prop.Rotation != null) veh.Rotation = prop.Rotation;
            if (prop.ModelHash != null) veh.ModelHash = prop.ModelHash.Value;
            if (prop.EntityType != null) veh.EntityType = prop.EntityType.Value;
            if (prop.Alpha != null) veh.Alpha = prop.Alpha.Value;
            if (prop.Flag != null) veh.Flag = prop.Flag.Value;
            if (prop.Text != null) veh.Text = prop.Text;
            if (prop.Size != null) veh.Size = prop.Size.Value;
            if (prop.EntitySeethrough != null) veh.EntitySeethrough = prop.EntitySeethrough.Value;
            if (prop.Range != null) veh.Range = prop.Range.Value;
            if (prop.Red != null) veh.Red = prop.Red.Value;
            if (prop.Green != null) veh.Green = prop.Green.Value;
            if (prop.Blue != null) veh.Blue = prop.Blue.Value;
            if (prop.IsInvincible != null) veh.IsInvincible = prop.IsInvincible.Value;

            if (prop.Dimension != null)
            {
                veh.Dimension = prop.Dimension.Value;
                //if (veh.Dimension != Main.LocalDimension && veh.StreamedIn && veh.Dimension != 0) StreamOut(veh);
            }

            if (prop.Attachables != null) veh.Attachables = prop.Attachables;
            if (prop.AttachedTo != null)
            {
                veh.AttachedTo = prop.AttachedTo;
                var attachedTo = NetToStreamedItem(prop.AttachedTo.NetHandle);
                if (attachedTo != null)
                {
                    AttachEntityToEntity(veh as IStreamedItem, attachedTo, prop.AttachedTo);
                }
            }
            if (prop.SyncedProperties != null)
            {
                if (veh.SyncedProperties == null) veh.SyncedProperties = new Dictionary<string, NativeArgument>();
                foreach (var pair in prop.SyncedProperties)
                {
                    if (pair.Value is LocalGamePlayerArgument)
                        veh.SyncedProperties.Remove(pair.Key);
                    else
                        veh.SyncedProperties.Set(pair.Key, pair.Value);
                }
            }

            if (prop.PositionMovement != null) veh.PositionMovement = prop.PositionMovement;
            if (prop.RotationMovement != null) veh.RotationMovement = prop.RotationMovement;
        }
    }
}

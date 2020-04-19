using Lidgren.Network;
using RDR2;
using RDR2.Native;
using RDRNetwork.Misc;
using RDRNetwork.Utils;
using RDRNetwork.Utils.Extentions;
using RDRNetworkShared;
using System;
using static RDRNetwork.Utils.Extentions.PedExtensions;

namespace RDRNetwork.Sync
{
    public partial class SyncCollector
    {
        private static bool _lastShooting;
        private static bool _lastBullet;
        private static DateTime _lastShot;
        private static bool _sent = true;

        private static void PedData(Ped player)
        {
            bool aiming = player.IsSubtaskActive(ESubtask.AIMED_SHOOTING_ON_FOOT) || player.IsSubtaskActive(ESubtask.AIMING_THROWABLE); // Game.IsControlPressed(GTA.Control.Aim);
            bool shooting = Function.Call<bool>(Hash.IS_PED_SHOOTING, player.Handle);

            RDR2.Math.Vector3 aimCoord = new RDR2.Math.Vector3();
            if (aiming || shooting)
            {
                //aimCoord = Main.RaycastEverything(new RDR2.Math.Vector2(0, 0));
                aimCoord = World.CrosshairRaycast(100, IntersectOptions.Everything, Game.Player.Character).HitPosition;
            }

            Weapon currentWeapon = player.Weapons.Current;
            
            var obj = new PedData
            {
                AimCoords = aimCoord.ToLVector(),
                Position = player.Position.ToLVector(),
                Quaternion = player.Rotation.ToLVector(),
                //PedArmor = (byte)player.Armor,
                PedArmor = 0,
                PedModelHash = player.Model.Hash,
                WeaponHash = (int)currentWeapon.Hash,
                WeaponAmmo = currentWeapon.Ammo,
                PlayerHealth = (byte)Util.Clamp(0, player.Health, 255),
                Velocity = player.Velocity.ToLVector(),
                Flag = 0
            };
            

            if (player.IsRagdoll)
                obj.Flag |= (int)PedDataFlags.Ragdoll;
            if (player.IsInMeleeCombat)
                obj.Flag |= (int)PedDataFlags.InMeleeCombat;
            if (aiming || shooting)
                obj.Flag |= (int)PedDataFlags.Aiming;
            if ((player.IsInMeleeCombat && Game.IsControlJustPressed(0, Control.Attack)))
                obj.Flag |= (int)PedDataFlags.Shooting;
            if (Function.Call<bool>(Hash.IS_PED_JUMPING, player.Handle))
                obj.Flag |= (int)PedDataFlags.Jumping;
            if (player.IsInCover())
                obj.Flag |= (int)PedDataFlags.IsInCover;
            if (!Function.Call<bool>(Hash.IS_PED_IN_COVER, player))
                obj.Flag |= (int)PedDataFlags.IsInLowerCover;
            if (player.IsInCoverFacingLeft)
                obj.Flag |= (int)PedDataFlags.IsInCoverFacingLeft;
            if (player.IsReloading)
                obj.Flag |= (int)PedDataFlags.IsReloading;
            if (ForceAimData)
                obj.Flag |= (int)PedDataFlags.HasAimData;
            
            if (player.IsSubtaskActive(ESubtask.USING_LADDER))
                obj.Flag |= (int)PedDataFlags.IsOnLadder;
            if (Function.Call<bool>(Hash.IS_PED_CLIMBING, player) && !player.IsSubtaskActive(ESubtask.USING_LADDER))
                obj.Flag |= (int)PedDataFlags.IsVaulting;
            if (Function.Call<bool>(Hash.IS_ENTITY_ON_FIRE, player))
                obj.Flag |= (int)PedDataFlags.OnFire;
            if (player.IsDead)
                obj.Flag |= (int)PedDataFlags.PlayerDead;
            
            if (player.IsSubtaskActive(168))
            {
                obj.Flag |= (int)PedDataFlags.ClosingVehicleDoor;
            }
            
            if (player.IsSubtaskActive(161) || player.IsSubtaskActive(162) || player.IsSubtaskActive(163) ||
                player.IsSubtaskActive(164))
            {
                obj.Flag |= (int)PedDataFlags.EnteringVehicle;

                obj.VehicleTryingToEnter =
                    Streamer.EntityToNet(Function.Call<int>(Hash.GET_VEHICLE_PED_IS_ENTERING,
                        player));

                obj.SeatTryingToEnter = (sbyte)
                    Function.Call<int>(Hash.GET_SEAT_PED_IS_TRYING_TO_ENTER,
                        player);
            }

            obj.Speed = GetPedWalkingSpeed(player);

            lock (Lock)
            {
                LastSyncPacket = obj;
            }

            bool sendShootingPacket;
            
            if (obj.WeaponHash != null && !WeaponDataProvider.IsWeaponAutomatic(unchecked((RDRNetworkShared.WeaponHash)obj.WeaponHash.Value)))
            {
                sendShootingPacket = (shooting && !player.IsSubtaskActive(ESubtask.AIMING_PREVENTED_BY_OBSTACLE) && !player.IsSubtaskActive(ESubtask.MELEE_COMBAT));
            }
            else
            {
                if (!_lastShooting && !player.IsSubtaskActive(ESubtask.MELEE_COMBAT))
                {
                    sendShootingPacket = (shooting && !player.IsSubtaskActive(ESubtask.AIMING_PREVENTED_BY_OBSTACLE) &&
                                          !player.IsSubtaskActive(ESubtask.MELEE_COMBAT)) ||
                                         ((player.IsInMeleeCombat || player.IsSubtaskActive(ESubtask.MELEE_COMBAT)) &&
                                          Game.IsEnabledControlPressed(0,Control.Attack));
                }
                else
                {
                    sendShootingPacket = (!player.IsSubtaskActive(ESubtask.AIMING_PREVENTED_BY_OBSTACLE) &&
                                          !player.IsSubtaskActive(ESubtask.MELEE_COMBAT) &&
                                          !player.IsReloading &&
                                          player.Weapons.Current.AmmoInClip > 0 &&
                                          Game.IsEnabledControlPressed(0,Control.Attack)) ||
                                         ((player.IsInMeleeCombat || player.IsSubtaskActive(ESubtask.MELEE_COMBAT)) &&
                                          Game.IsEnabledControlPressed(0,Control.Attack));
                }

                if (!sendShootingPacket && _lastShooting && !_lastBullet)
                {
                    _lastBullet = true;
                    _lastShooting = false;
                    return;
                }
            }

            _lastBullet = false;

            if (player.IsRagdoll) sendShootingPacket = false;

            if (!player.IsSubtaskActive(ESubtask.MELEE_COMBAT) && player.Weapons.Current.Ammo == 0) sendShootingPacket = false;

            if (sendShootingPacket && !_lastShooting && DateTime.Now.Subtract(_lastShot).TotalMilliseconds > 50)
            {
                //Util.Util.SafeNotify("Sending BPacket " + DateTime.Now.Millisecond);
                _sent = false;
                _lastShooting = true;
                _lastShot = DateTime.Now;

                var msg = Main.Client.CreateMessage();
                byte[] bin;

                var syncPlayer = Util.GetPedWeHaveDamaged();

                if (syncPlayer != null)
                {
                    bin = PacketOptimization.WriteBulletSync(0, true, syncPlayer.RemoteHandle);
                    msg.Write((byte)PacketType.BulletPlayerSync);
                }
                else
                {
                    bin = PacketOptimization.WriteBulletSync(0, true, aimCoord.ToLVector());
                    msg.Write((byte)PacketType.BulletSync);
                }

                msg.Write(bin.Length);
                msg.Write(bin);
                Main.Client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, (int)ConnectionChannel.BulletSync);
            }
            else if (!sendShootingPacket && !_sent && DateTime.Now.Subtract(_lastShot).TotalMilliseconds > 50)
            {
                _sent = true;
                _lastShooting = false;
                _lastShot = DateTime.Now;

                var msg = Main.Client.CreateMessage();

                byte[] bin = PacketOptimization.WriteBulletSync(0, false, aimCoord.ToLVector());
                msg.Write((byte)PacketType.BulletSync);

                msg.Write(bin.Length);
                msg.Write(bin);
                Main.Client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, (int)ConnectionChannel.BulletSync);
            }
        }

        private static byte GetPedWalkingSpeed(Ped ped)
        {
            byte output = 0;
            /*
            if (SyncPed.GetAnimalAnimationDictionary(ped.Model.Hash) != null)
            {
                // Player has an animal skin
                var hash = (PedHash)ped.Model.Hash;

                switch (hash)
                {
                    case PedHash.ChickenHawk:
                    case PedHash.Cormorant:
                    case PedHash.Crow:
                    case PedHash.Seagull:
                    case PedHash.Pigeon:
                        if (ped.Velocity.Length() > 0.1) output = 1;
                        if (ped.IsInAir || ped.Velocity.Length() > 0.5) output = 3;
                        break;
                    case PedHash.Dolphin:
                    case PedHash.Fish:
                    case PedHash.Humpback:
                    case PedHash.KillerWhale:
                    case PedHash.Stingray:
                    case PedHash.HammerShark:
                    case PedHash.TigerShark:
                        if (ped.Velocity.Length() > 0.1) output = 1;
                        if (ped.Velocity.Length() > 0.5) output = 2;
                        break;
                }
            }
            if (Function.Call<bool>(Hash.IS_PED_WALKING, ped)) output = 1;
            if (Function.Call<bool>(Hash.IS_PED_RUNNING, ped)) output = 2;
            if (Function.Call<bool>(Hash.IS_PED_SPRINTING, ped) || ped.IsPlayer && Game.IsControlPressed(Control.Sprint)) output = 3;
            */
            //if (Function.Call<bool>(Hash.IS_PED_STRAFING, ped)) ;

            /*if (ped.IsSubtaskActive(ESubtask.AIMING_GUN))
            {
                if (ped.Velocity.LengthSquared() > 0.1f*0.1f)
                    output = 1;
            }
            */

            return output;
        }
    }
}

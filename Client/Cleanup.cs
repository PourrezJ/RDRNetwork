using RDR2;
using RDR2.Native;
using RDRNetwork.API;
using System;

namespace RDRNetwork
{
    internal static class CleanupGame
    {
        private static DateTime LastDateTime = DateTime.Now;

        internal static void OnTick()
        {
            Function.Call(Hash.SET_RANDOM_TRAINS, false);
            
            Function.Call(Hash.DELETE_ALL_TRAINS);

            Function.Call(Hash.SET_AMBIENT_PED_RANGE_MULTIPLIER_THIS_FRAME, 0f);
            Function.Call(Hash.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);           
            Function.Call(Hash.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
            Function.Call(Hash.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
            Function.Call(Hash.SET_SCENARIO_PED_DENSITY_MULTIPLIER_THIS_FRAME, 0f);


            Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player.Handle, 0f);

            Function.Call(Hash.SET_PLAYER_WANTED_LEVEL, Game.Player.Handle, 0, false);
            Function.Call(Hash.SET_MAX_WANTED_LEVEL, 0);
            Function.Call(Hash.CLEAR_PLAYER_WANTED_LEVEL, 0);

            for (int a = 0; a < 12; a++)
                Function.Call(Hash.ENABLE_DISPATCH_SERVICE, a, false);

            Function.Call(Hash.ADD_SCENARIO_BLOCKING_AREA, -10000.0f, -10000.0f, -1000.0f, 10000.0f, 10000.0f, 1000.0f, 0, 1, 1, 1);
            Function.Call(Hash.SET_CREATE_RANDOM_COPS, false);
            Function.Call(Hash.SET_RANDOM_BOATS, false);
            Function.Call(Hash.SUPPRESS_SHOCKING_EVENTS_NEXT_FRAME);
           
            
            if (DateTime.Now.Subtract(LastDateTime).TotalMilliseconds >= 500)
            {
                var playerChar = Game.Player.Character;

                LastDateTime = DateTime.Now;
                /*
                foreach (var entity in World.GetAllPeds())
                {
                    if (!entity.Exists())
                        continue;

                    if (!Streamer.ContainsLocalHandle(entity.Handle) && entity != playerChar)
                    {
                        entity.MarkAsNoLongerNeeded();
                       // entity.Kill();
                        entity.Delete();
                    }
                }*/
                /*
                foreach (var entity in World.GetAllVehicles())
                {
                    var veh = Main.NetEntityHandler.NetToStreamedItem(entity.Handle, useGameHandle: true) as RemoteVehicle;
                    if (veh == null)
                    {
                        entity.MarkAsNoLongerNeeded();
                        entity.Delete();
                    }
                }*/
            }
        }
    }

    internal class Controls : Script
    {
        private static void OnTick(object sender, EventArgs e)
        {
            /*
            CallCollection Function = new CallCollection();
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.FrontendSocialClub, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.FrontendSocialClubSecondary, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.EnterCheatCode, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.SpecialAbility, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.SpecialAbilityPc, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.SpecialAbilitySecondary, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.CharacterWheel, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Phone, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Duck, true);

            if (Main.IsConnected())
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.FrontendPause, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.FrontendPauseAlternate, true);
            }

            var playerChar = Game.Player.Character;
            if (playerChar.IsJumping)
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.MeleeAttack, true);

            }

            if (playerChar.IsRagdoll)
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Attack, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Attack2, true);

            }

            if (Game.IsControlPressed(Control.Aim) && !playerChar.IsInVehicle() && playerChar.Weapons.Current.Hash != WeaponHash.Unarmed)
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Jump, true);
            }

            if (playerChar.IsInVehicle())
            {
                if (playerChar.CurrentVehicle.IsInAir && playerChar.CurrentVehicle.Model.Hash == 941494461)
                {
                    Function.Call(Hash.DISABLE_ALL_CONTROL_ACTIONS, 0);
                }
            }

            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.Enter, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.VehExit, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Control.HorseExit, true);

            Function.Execute();*/
        }
    }
}
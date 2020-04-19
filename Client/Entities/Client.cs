using RDR2.Native;
using RDRNetwork.API;

namespace RDRNetwork.Entities
{
    internal class Client
    {
        /*
   private static void ClearLocalEntities()
   {
       lock (EntityCleanup)
       {
           for (var index = EntityCleanup.Count - 1; index >= 0; index--)
           {
               var prop = new Prop(EntityCleanup[index]);
               if (prop.Exists()) prop.Delete();
           }
           EntityCleanup.Clear();
       }
   }

   private static void ClearLocalBlips()
   {
       lock (BlipCleanup)
       {
           for (var index = BlipCleanup.Count - 1; index >= 0; index--)
           {
               var b = new Blip(BlipCleanup[index]);
               if (b.Exists()) b.Remove();
           }
           BlipCleanup.Clear();
       }
   }*/

        private static void ResetPlayer()
        {
            var playerChar = Game.Player.Character;

            //playerChar.Position = _vinewoodSign;
            //playerChar.IsPositionFrozen = false;

            //CustomAnimation = null;
            //AnimationFlag = 0;

            var model = new Model(PedHash.Player_Zero);
            model.Request();

            var player = Game.Player;
            player.ChangeModel(model);
            playerChar = Game.Player.Character;

            playerChar.MaxHealth = 200;
            playerChar.DeadEyeCore = 500;
            playerChar.HealthCore = 500;
            playerChar.StaminaCore = 500;

            playerChar.FreezePosition = false;
            player.IsInvincible = false;
            playerChar.SetNoCollision(playerChar, false);
            playerChar.Alpha = 255;
            playerChar.IsInvincible = false;
            playerChar.Weapons.RemoveAll();

            // Function.Call(Hash.SET_RUN_SPRINT_MULTIPLIER_FOR_PLAYER, player, 1f);
            Function.Call(Hash.SET_SWIM_MULTIPLIER_FOR_PLAYER, player.Handle, 1f);
            // Function.Call(Hash.SET_FAKE_WANTED_LEVEL, 0);
            Function.Call(Hash.DETACH_ENTITY, playerChar.Handle, true, true);
        }
        /*
        private static void ResetWorld()
        {
            World.RenderingCamera = MainMenuCamera;
            MainMenu.Visible = true;
            MainMenu.TemporarilyHidden = false;
            IsSpectating = false;
            Weather = null;
            Time = null;
            LocalTeam = -1;
            LocalDimension = 0;
        }

        private static void ClearStats()
        {
            BytesReceived = 0;
            BytesSent = 0;
            MessagesReceived = 0;
            MessagesSent = 0;
        }*/
    }
}

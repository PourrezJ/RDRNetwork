using RDRNetworkShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResuMPServer
{
    public partial class API
    {
        public void SetBlipColor(NetHandle blip, int color)
        {
            if (DoesEntityExist(blip))
            {
                ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Color = color;
                var delta = new Delta_BlipProperties();
                delta.Color = color;
                GameServer.UpdateEntityInfo(blip.Value, EntityType.Blip, delta);
            }

            Program.ServerInstance.SendNativeCallToAllPlayers(0x03D7FB09E75D6B7E, blip, color);
        }

        public int GetBlipColor(NetHandle blip)
        {
            if (DoesEntityExist(blip))
            {
                return ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Color;
            }
            return 0;
        }

        public void SetBlipName(NetHandle blip, string name)
        {
            if (DoesEntityExist(blip))
            {
                if (name == null) name = "";

                ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Name = name;
                var delta = new Delta_BlipProperties();
                delta.Name = name;
                GameServer.UpdateEntityInfo(blip.Value, EntityType.Blip, delta);
            }
        }

        public string GetBlipName(NetHandle blip)
        {
            if (DoesEntityExist(blip))
            {
                return ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Name;
            }
            return null;
        }

        public void SetBlipTransparency(NetHandle blip, int alpha)
        {
            if (DoesEntityExist(blip))
            {
                ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Alpha = (byte)alpha;
                var delta = new Delta_BlipProperties();
                delta.Alpha = (byte)alpha;
                GameServer.UpdateEntityInfo(blip.Value, EntityType.Blip, delta);
            }

            Program.ServerInstance.SendNativeCallToAllPlayers(0x45FF974EEE1C8734, blip, alpha);
        }

        public int GetBlipTransparency(NetHandle blip)
        {
            if (DoesEntityExist(blip))
            {
                return ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Alpha;
            }
            return 0;
        }

        public void SetBlipShortRange(NetHandle blip, bool range)
        {
            if (DoesEntityExist(blip))
            {
                ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).IsShortRange = range;

                var delta = new Delta_BlipProperties();
                delta.IsShortRange = range;
                GameServer.UpdateEntityInfo(blip.Value, EntityType.Blip, delta);
            }
            Program.ServerInstance.SendNativeCallToAllPlayers(0xBE8BE4FE60E27B72, blip, range);
        }

        public bool GetBlipShortRange(NetHandle blip)
        {
            if (DoesEntityExist(blip))
            {
                return ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).IsShortRange;
            }
            return false;
        }

        public void SetBlipPosition(NetHandle blip, Vector3 newPos)
        {
            if (DoesEntityExist(blip))
            {
                ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Position = newPos;

                var delta = new Delta_BlipProperties();
                delta.Position = newPos;
                GameServer.UpdateEntityInfo(blip.Value, EntityType.Blip, delta);
            }
            Program.ServerInstance.SendNativeCallToAllPlayers(0xAE2AF67E9D9AF65D, blip, newPos.X, newPos.Y, newPos.Z);
        }

        public Vector3 GetBlipPosition(NetHandle blip)
        {
            if (DoesEntityExist(blip))
            {
                return ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Position;
            }

            return null;
        }

        public void SetBlipSprite(NetHandle blip, int sprite)
        {
            if (DoesEntityExist(blip))
            {
                ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Sprite = sprite;

                var delta = new Delta_BlipProperties();
                delta.Sprite = sprite;
                GameServer.UpdateEntityInfo(blip.Value, EntityType.Blip, delta);
            }
            Program.ServerInstance.SendNativeCallToAllPlayers(0xDF735600A4696DAF, blip, sprite);
        }

        public int GetBlipSprite(NetHandle blip)
        {
            if (DoesEntityExist(blip))
            {
                return ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Sprite;
            }

            return 0;
        }

        public void SetBlipScale(NetHandle blip, float scale)
        {
            if (DoesEntityExist(blip))
            {
                ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Scale = scale;

                var delta = new Delta_BlipProperties();
                delta.Scale = scale;
                GameServer.UpdateEntityInfo(blip.Value, EntityType.Blip, delta);
            }
            Program.ServerInstance.SendNativeCallToAllPlayers(0xD38744167B2FA257, blip, scale);
        }

        public float GetBlipScale(NetHandle blip)
        {
            if (DoesEntityExist(blip))
            {
                return ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).Scale;
            }

            return 0;
        }

        public void SetBlipRouteVisible(NetHandle blip, bool visible)
        {
            if (DoesEntityExist(blip))
            {
                ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).RouteVisible = visible;

                var delta = new Delta_BlipProperties();
                delta.RouteVisible = visible;
                GameServer.UpdateEntityInfo(blip.Value, EntityType.Blip, delta);
            }
            Program.ServerInstance.SendNativeCallToAllPlayers(0x4F7D8A9BFB0B43E9, blip, visible);
        }

        public bool GetBlipRouteVisible(NetHandle blip)
        {
            if (DoesEntityExist(blip))
            {
                return ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).RouteVisible;
            }

            return false;
        }

        public void SetBlipRouteColor(NetHandle blip, int color)
        {
            if (DoesEntityExist(blip))
            {
                ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).RouteColor = color;

                var delta = new Delta_BlipProperties();
                delta.RouteColor = color;
                GameServer.UpdateEntityInfo(blip.Value, EntityType.Blip, delta);
            }
            Program.ServerInstance.SendNativeCallToAllPlayers(0x837155CD2F63DA09, blip, color);
        }

        public int GetBlipRouteColor(NetHandle blip)
        {
            if (DoesEntityExist(blip))
            {
                return ((BlipProperties)Program.ServerInstance.NetEntityHandler.ToDict()[blip.Value]).RouteColor;
            }

            return 0;
        }
    }
}

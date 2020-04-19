using ResuMPServer.Constant;
using RDRNetworkShared;

namespace ResuMPServer
{
    public class Marker : Entity
    {
        internal Marker(API father, NetHandle handle) : base(father, handle)
        {
        }

        #region Properties

        public int MarkerType
        {
            get => GetMarkerType();
            set => SetMarkerType(value);
        }

        public Vector3 Scale
        {
            get => GetMarkerScale();
            set => SetMarkerScale(value);
        }

        public Vector3 Direction
        {
            get => GetMarkerDirection();
            set => SetMarkerDirection(value);
        }

        public Color Color
        {
            get => GetMarkerColor();
            set => SetMarkerColor(value.alpha, value.red, value.green, value.blue);
        }

        #endregion

        #region Methods
        public void SetMarkerType(int type)
        {
            if (DoesEntityExist())
            {
                ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).MarkerType = type;

                var delta = new Delta_MarkerProperties();
                delta.MarkerType = type;
                GameServer.UpdateEntityInfo(Value, EntityType.Marker, delta);
            }
        }

        public int GetMarkerType()
        {
            if (DoesEntityExist())
            {
                return ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).MarkerType;
            }

            return 0;
        }

        public void SetMarkerScale(Vector3 scale)
        {
            if (DoesEntityExist())
            {
                ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Scale = scale;

                var delta = new Delta_MarkerProperties();
                delta.Scale = scale;
                GameServer.UpdateEntityInfo(Value, EntityType.Marker, delta);
            }
        }

        public Vector3 GetMarkerScale()
        {
            if (DoesEntityExist())
            {
                return ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Scale;
            }

            return null;
        }

        public void SetMarkerDirection(Vector3 dir)
        {
            if (DoesEntityExist())
            {
                ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Direction = dir;

                var delta = new Delta_MarkerProperties();
                delta.Direction = dir;
                GameServer.UpdateEntityInfo(Value, EntityType.Marker, delta);
            }
        }


        public Vector3 GetMarkerDirection()
        {
            if (DoesEntityExist())
            {
                return ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Direction;
            }

            return null;
        }

        public void SetMarkerColor(int alpha, int red, int green, int blue)
        {
            if (DoesEntityExist())
            {
                ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Alpha = (byte)alpha;
                ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Red = (byte)red;
                ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Green = (byte)green;
                ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Blue = (byte)blue;

                var delta = new Delta_MarkerProperties();
                delta.Alpha = (byte)alpha;
                delta.Red = (byte)red;
                delta.Green = (byte)green;
                delta.Blue = (byte)blue;
                GameServer.UpdateEntityInfo(Value, EntityType.Marker, delta);
            }
        }

        public Color GetMarkerColor()
        {
            Color output = new Color();

            if (DoesEntityExist())
            {
                output.alpha = ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Alpha;
                output.red = ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Red;
                output.green = ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Green;
                output.blue = ((MarkerProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Blue;
            }

            return output;
        }
        #endregion
    }
}
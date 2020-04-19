using RDR2.Math;
using System;

namespace RDRNetwork.Utils.Extentions
{
    internal static class Vector3Extentions
    {
        internal static RDR2.Math.Quaternion ToQuaternion(this RDRNetworkShared.Quaternion q)
        {
            return new RDR2.Math.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        internal static RDR2.Math.Vector3 ToVector(this RDRNetworkShared.Vector3 v)
        {
            if ((object)v == null) 
                return new Vector3();
            return new RDR2.Math.Vector3(v.X, v.Y, v.Z);
        }

        internal static RDRNetworkShared.Vector3 ToLVector(this RDR2.Math.Vector3 vec)
        {
            return new RDRNetworkShared.Vector3()
            {
                X = vec.X,
                Y = vec.Y,
                Z = vec.Z,
            };
        }

        internal static RDRNetworkShared.Quaternion ToLQuaternion(this RDR2.Math.Quaternion vec)
        {
            return new RDRNetworkShared.Quaternion()
            {
                X = vec.X,
                Y = vec.Y,
                Z = vec.Z,
                W = vec.W,
            };
        }

        internal static float LengthSquared(this RDRNetworkShared.Vector3 left)
        {
            return left.X * left.X + left.Y * left.Y + left.Z + left.Z;
        }

        internal static float Length(this RDRNetworkShared.Vector3 left)
        {
            return (float)Math.Sqrt(left.LengthSquared());
        }

        internal static RDRNetworkShared.Vector3 Sub(this RDRNetworkShared.Vector3 left, RDRNetworkShared.Vector3 right)
        {
            if ((object)left == null && (object)right == null) return new RDRNetworkShared.Vector3();
            if ((object)left == null) return right;
            if ((object)right == null) return left;
            return new RDRNetworkShared.Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        internal static RDRNetworkShared.Vector3 Add(this RDRNetworkShared.Vector3 left, RDRNetworkShared.Vector3 right)
        {
            if ((object)left == null && (object)right == null) return new RDRNetworkShared.Vector3();
            if ((object)left == null) return right;
            if ((object)right == null) return left;
            return new RDRNetworkShared.Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        internal static Quaternion ToQuaternion(this Vector3 vect)
        {
            vect = new Vector3()
            {
                X = vect.X.Denormalize() * -1,
                Y = vect.Y.Denormalize() - 180f,
                Z = vect.Z.Denormalize() - 180f,
            };

            vect = vect.ToRadians();

            float rollOver2 = vect.Z * 0.5f;
            float sinRollOver2 = (float)Math.Sin((double)rollOver2);
            float cosRollOver2 = (float)Math.Cos((double)rollOver2);
            float pitchOver2 = vect.Y * 0.5f;
            float sinPitchOver2 = (float)Math.Sin((double)pitchOver2);
            float cosPitchOver2 = (float)Math.Cos((double)pitchOver2);
            float yawOver2 = vect.X * 0.5f; // pitch
            float sinYawOver2 = (float)Math.Sin((double)yawOver2);
            float cosYawOver2 = (float)Math.Cos((double)yawOver2);
            Quaternion result = new Quaternion();
            result.X = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
            result.Y = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;
            result.Z = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
            result.W = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
            return result;
        }

        internal static float ToRadians(this float val)
        {
            return (float)(Math.PI / 180) * val;
        }

        internal static Vector3 ToRadians(this Vector3 i)
        {
            return new Vector3()
            {
                X = ToRadians(i.X),
                Y = ToRadians(i.Y),
                Z = ToRadians(i.Z),
            };
        }

        internal static Vector3 Denormalize(this Vector3 v)
        {
            return new Vector3(v.X.Denormalize(), v.Y.Denormalize(), v.Z.Denormalize());
        }

        public static Vector3 RotationToDirection(this Vector3 rotation)
        {
            var z = DegToRad(rotation.Z);
            var x = DegToRad(rotation.X);
            var num = Math.Abs(Math.Cos(x));
            return new Vector3
            {
                X = (float)(-Math.Sin(z) * num),
                Y = (float)(Math.Cos(z) * num),
                Z = (float)Math.Sin(x)
            };
        }

        public static Vector3 DirectionToRotation(this Vector3 direction)
        {
            direction.Normalize();

            var x = Math.Atan2(direction.Z, direction.Y);
            var y = 0;
            var z = -Math.Atan2(direction.X, direction.Y);

            return new Vector3
            {
                X = (float)RadToDeg(x),
                Y = (float)RadToDeg(y),
                Z = (float)RadToDeg(z)
            };
        }

        public static double DegToRad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        public static double RadToDeg(double deg)
        {
            return deg * 180.0 / Math.PI;
        }
    }
}
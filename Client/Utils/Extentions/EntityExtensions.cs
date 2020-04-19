using RDR2;
using RDR2.Math;

namespace RDRNetwork.Utils.Extentions
{
    internal static class EntityExtensions
    {
        public static bool IsInRangeOfEx(this Entity ent, Vector3 pos, float range)
        {
            return ent.Position.DistanceToSquared(pos) < (range * range);
        }
    }
}
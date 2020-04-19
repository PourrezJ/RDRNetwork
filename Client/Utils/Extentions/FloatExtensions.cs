namespace RDRNetwork.Utils.Extentions
{
    internal static class FloatExtensions
    {
        public static float Denormalize(this float h)
        {
            return h < 0f ? h + 360f : h;
        }
    }
}

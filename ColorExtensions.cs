using System.Drawing;
using System.Numerics;

namespace RayTracing
{
    public static class ColorExtensions
    {
        public static Color FromVec3(this Color color, Vector3 vec)
        {
            return Color.FromArgb((byte)(vec.X * 255), (byte)(vec.Y * 255), (byte)(vec.Z * 255));
        }
    }
}

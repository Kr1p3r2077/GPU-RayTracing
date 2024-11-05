using System.Numerics;

namespace RayTracing
{
    public class MakarMath
    {
        public static float AngleBetween(Vector3 a, Vector3 b)
        {
            a = Normalize(a);
            b = Normalize(b);

            float sideA = a.Length();
            float sideB = b.Length();
            float sideC = Vector3.Distance(a, b);

            float angle = (sideC * sideC - sideA * sideA - sideB * sideB) / (-2 * sideA * sideB);
            angle = (float)Math.Acos(angle);

            return angle;
        }

        public static Vector3 Normalize(Vector3 a)
        {
            Vector3 v = new Vector3();
            var l = a.Length();
            v.X = a.X / l;
            v.Y = a.Y / l;
            v.Z = a.Z / l;

            return v;
        }
    }
}
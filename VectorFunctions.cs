using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    public static class VectorFunctions
    {
        public static Vector3 RotateVector(Vector3 baseVector, Vector3 rotateVector)
        {

            float len = baseVector.Length();

            Vector2 YZProj = new Vector2(baseVector.Y, baseVector.Z);
            YZProj = RotateVector2(YZProj, rotateVector.X);
            baseVector.Y = YZProj.X;
            baseVector.Z = YZProj.Y;

            //baseVector = Vector3.Normalize(baseVector) * len;

            Vector2 XZProj = new Vector2(baseVector.X, baseVector.Z);
            XZProj = RotateVector2(XZProj, rotateVector.Y);
            baseVector.X = XZProj.X;
            baseVector.Z = XZProj.Y;

            //baseVector = Vector3.Normalize(baseVector) * len;

            Vector2 XYProj = new Vector2(baseVector.X, baseVector.Y);
            XYProj = RotateVector2(XYProj, rotateVector.Z);
            baseVector.X = XYProj.X;
            baseVector.Y = XYProj.Y;

            //baseVector = Vector3.Normalize(baseVector) * len;

            return baseVector;
        }

        public static float AngleBetween(Vector3 a, Vector3 b)
        {
            return (float)Math.Acos((double)(a.X * b.X + a.Y * b.Y + a.Z * b.Z));
        }

        private static Vector2 RotateVector2(Vector2 vector2, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            Vector2 result;
            result.X = vector2.X * cos - vector2.Y * sin;
            result.Y = vector2.X * sin + vector2.Y * cos;

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Shapes
{
    internal class Sphere : Shape
    {
        Vector3 position;
        float radius;
        public Sphere(Vector3 pos, float r = 1)
        {
            position = pos;
            radius = r;
        }

        public Sphere(Vector3 pos, Material mat, float r = 1)
        {
            position = pos;
            radius = r;
            material = mat;
        }

        public override bool Intersect(Vector3 origin, Vector3 dir, out float fraction, out Vector3 normal)
        {
            fraction = 0;
            normal = Vector3.Zero;

            Vector3 L = origin - position;
            float a = Vector3.Dot(dir, dir);
            float b = 2.0f * Vector3.Dot(L, dir);
            float c = Vector3.Dot(L, L) - radius * radius;
            float D = b * b - 4 * a * c;

            if (D < 0) return false;

            float x1 = (-b - (float)Math.Sqrt(D)) / (2.0f * a);
            float x2 = (-b + (float)Math.Sqrt(D)) / (2.0f * a);

            if (x1 > 0)
            {
                fraction = x1;
            }
            else if (x2 > 0)
            {
                fraction = x2;
            }
            else
            {
                return false;
            }

            normal = Vector3.Normalize(dir * fraction + L);

            return true;
        }
    }
}

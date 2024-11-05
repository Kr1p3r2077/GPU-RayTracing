using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Shapes
{
    public abstract class Shape
    {
        public Material material;
        public abstract bool Intersect(Vector3 origin, Vector3 dir, out float fraction, out Vector3 normal);
    }
}

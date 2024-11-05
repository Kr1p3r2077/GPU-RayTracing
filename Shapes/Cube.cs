using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace RayTracing.Shapes
{
    public class Cube : Shape
    {
        Vector3 position;
        Matrix3 rotation;
        Vector3 halfSize;

        public Cube(System.Numerics.Vector3 position, System.Numerics.Vector3 rot ,System.Numerics.Vector3 size, Material material)
        {
            //rotation = Matrix3.CreateRotationX(rot.X) * Matrix3.CreateRotationY(rot.Y) * Matrix3.CreateRotationZ(rot.Z);
            Matrix3.CreateFromQuaternion(Quaternion.FromEulerAngles(new Vector3(rot.X, rot.Y, rot.Z)), out rotation);

            this.position = new Vector3(position.X,position.Y,position.Z);
            this.halfSize = new Vector3(size.X, size.Y, size.Z);
            this.material = material;
        }

        public Cube(System.Numerics.Vector3 position, System.Numerics.Vector3 size, Material material)
        {
            rotation = Matrix3.Identity;

            this.position = new Vector3(position.X, position.Y, position.Z);
            this.halfSize = new Vector3(size.X, size.Y, size.Z);
            this.material = material;
        }

        public override bool Intersect(System.Numerics.Vector3 origin, System.Numerics.Vector3 dir, out float fraction, out System.Numerics.Vector3 normal)
        {
            fraction = 0f;
            normal = System.Numerics.Vector3.Zero;

            Vector3 rd = rotation * new Vector3(dir.X, dir.Y, dir.Z);
            Vector3 ro = rotation * new Vector3(origin.X - position.X, origin.Y - position.Y, origin.Z - position.Z);

            Vector3 m = Vector3.One / rd;

            Vector3 s = new Vector3((rd.X < 0.0f) ? 1.0f : -1.0f,
                          (rd.Y < 0.0f) ? 1.0f : -1.0f,
                          (rd.Z < 0.0f) ? 1.0f : -1.0f);
            Vector3 t1 = m * (-ro + s * halfSize);
            Vector3 t2 = m * (-ro - s * halfSize);

            float tN = Math.Max(Math.Max(t1.X, t1.Y), t1.Z);
            float tF = Math.Min(Math.Min(t2.X, t2.Y), t2.Z);

            if (tN > tF || tF < 0.0) return false;

            Matrix3 txi = Matrix3.Transpose(rotation);

            Vector3 _n;
            if (t1.X > t1.Y && t1.X > t1.Z)
                _n = txi.Row0 * s.X;
            else if (t1.Y > t1.Z)
                _n = txi.Row1 * s.Y;
            else
                _n = txi.Row2 * s.Z;

            normal = new System.Numerics.Vector3(_n.X, _n.Y, _n.Z);
            fraction = tN;

            return true;

        }
    }
}
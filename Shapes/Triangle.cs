using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Shapes
{
    public class Triangle : Shape
    {
        public Vector3 A, B, C;
        Vector2 t0, t1, t2;
        public string texture;

        public Triangle(Vector3 a, Vector3 b, Vector3 c, Material mat, bool invertNormals)
        {
            A = a;
            B = b;
            C = c;
            material = mat;

            E1 = B - A;
            E2 = C - A;
            normal = Vector3.Cross(E1, E2);
            normal *= invertNormals ? -1 : 1;
        }

        public void SetTextureCoordinates(Assimp.Vector3D t0, Assimp.Vector3D t1, Assimp.Vector3D t2)
        {
            this.t0 = new Vector2(t0.X, t0.Y);
            this.t1 = new Vector2(t1.X, t1.Y);
            this.t2 = new Vector2(t2.X, t2.Y);

            TE1 = this.t1 - this.t0;
            TE2 = this.t2 - this.t0;
        }

        float maxX = 0, maxY=0, minX = 0, minY = 0;
        public Vector2 GetUV(float u, float v)
        {
            var uv = t0 + (TE1 * u + TE2 * v);

            //Vector3 expectedCoordinate = A + E1 * u + E2 * v;
            /*
            maxX = Math.Max(maxX, fragWorldPos.X);
            maxY = Math.Max(maxY, fragWorldPos.Y);
            minX = Math.Min(minX, fragWorldPos.X);
            minY = Math.Min(minY, fragWorldPos.Y);
            Console.Write(maxX + " ");
            Console.Write(maxY + " ");
            Console.Write(minX + " ");
            Console.Write(minY + "\n");
            */

            //Console.WriteLine(fragWorldPos);

            //var uv = new Vector2((fragWorldPos.Y + 5f) / 10f, (fragWorldPos.Z) / 10f); //WORLD POS

            //Console.WriteLine(uv);
            /*
            if (float.IsNaN(uv.X) || float.IsNaN(uv.Y))
            {

            }
            */
            return uv;
        }

        public Vector3 normal;
        public Vector3 E1, E2;
        Vector2 TE1, TE2;

        public float intersectionU, intersectionV;

        public override bool Intersect(Vector3 origin, Vector3 dir, out float fraction, out Vector3 normal)
        {
            fraction = 0;

            dir = Vector3.Normalize(dir);

            normal = this.normal;

            float det = -Vector3.Dot(dir, this.normal);
            float invdet = 1.0f / det;
            Vector3 AO = origin - A;
            Vector3 DAO = Vector3.Cross(AO, dir);
            var u = Vector3.Dot(E2, DAO) * invdet;
            var v = -Vector3.Dot(E1, DAO) * invdet;
            var t = Vector3.Dot(AO, this.normal) * invdet;

            intersectionU = u;
            intersectionV = v;

            if (det >= 0.000001f && t >= 0.0f && u >= 0.0f && v >= 0.0f && (u + v) <= 1.0f)
            {
                fraction = t;
                return true;
            }
            else
            {
                return false;
            }
        }


        bool intersect1(Vector3 ro, Vector3 rd, out float fraction, out Vector3 normal)
        {
            fraction = 0;

            Vector3 v1v0 = B - A;
            Vector3 v2v0 = C - A;
            Vector3 rov0 = ro - A;
            normal = Vector3.Cross(v1v0, v2v0);
            Vector3 q = Vector3.Cross(rov0, rd);
            float d = 1.0f / Vector3.Dot(rd, normal);
            float u = d * Vector3.Dot(-q, v2v0);
            float v = d * Vector3.Dot(q, v1v0);
            float t = d * Vector3.Dot(-normal, rov0);
            return u < 0.0f || v < 0.0f || (u + v) > 1.0f;

            fraction = new Vector3(t, u, v).Length();
            //return (det >= 0.000001f && t >= 0.0f && u >= 0.0f && v >= 0.0f && (u + v) <= 1.0f);
        }
    }
}

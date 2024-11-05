using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    public class Material
    {
        public Vector3 emmitance;
        public Vector3 reflectance;
        public float roughness;
        public float opacity;

        public Material() { }

        public Material(Vector3 emmitance, Vector3 color, float roughness, float opacity)
        {
            roughness = lerp(0.0001f, 1f, roughness);
            this.emmitance = emmitance;
            this.reflectance = color;
            this.roughness = 1 - roughness;
            this.opacity = 1 - opacity;
        }

        float lerp(float v0, float v1, float t)
        {
            return v0 + t * (v1 - v0);
        }

        public Material(Color color, float roughness, float opacity)
        {
            roughness = lerp(0.0001f, 0.9f, roughness);
            opacity = lerp(0.05f, 1f, opacity);
            this.emmitance = Vector3.Zero;
            this.reflectance = new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
            this.roughness = 1 - roughness;
            this.opacity = 1 - opacity;
        }

        public Material(Vector3 color, float roughness, float opacity)
        {
            roughness = lerp(0.0001f, 0.9f, roughness);
            opacity = lerp(0.05f, 1f, opacity);
            this.emmitance = Vector3.Zero;
            this.reflectance = color;
            this.roughness = 1 - roughness;
            this.opacity = 1 - opacity;
        }


        public static Material Lamp(Color color, float power = 1)
        {
            return new Material(new Vector3(color.R, color.G, color.B) * power / 255f, Vector3.Zero, 1, 1);
        }
        public static Material Lamp()
        {
            return new Material(Vector3.One, Vector3.Zero, 1, 1);
        }
        public static Material Lamp(float power)
        {
            return new Material(Vector3.One * power, Vector3.Zero, 1, 1);
        }
        public static Material Glossy(Color color, float reflect = 1)
        {
            return new Material(new Vector3(color.R,color.G,color.B) / 255f * reflect, 0, 1);
        }
        public static Material Glass(Color color)
        {
            return new Material(new Vector3(color.R, color.G, color.B) / 255f, 0, 0.0f);
        }

        public static Material Diffuse(Color color, float r = 0.6f)
        {
            return new Material(new Vector3(color.R, color.G, color.B) / 255f, r, 1f);
        }

        public static Material Diffuse()
        {
            return new Material(Vector3.One, 0.6f, 1f);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    public static class TextureLoader
    {
        public static Dictionary<string, Color[,]> textures = new Dictionary<string, Color[,]>();
        public static void LoadTexture(string filename)
        {
            if (textures.ContainsKey(filename))
                return;

            var bmp = new Bitmap(filename);
            Color[,] tex = new Color[bmp.Width, bmp.Height];
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    tex[i, j] = bmp.GetPixel(i, j);
                }
            }
            textures.Add(filename, tex);

        }

        public static System.Numerics.Vector3 GetTexture(string filename, System.Numerics.Vector2 uv)
        {
            var clr = textures[filename][
                (int)((textures[filename].GetLength(0) - 1) * uv.X),
                (int)((textures[filename].GetLength(1) - 1) * (1f - uv.Y))
                ];

            return new System.Numerics.Vector3((float)clr.R / 255f, (float)clr.G / 255f, (float)clr.B / 255f);
        }
    }
}

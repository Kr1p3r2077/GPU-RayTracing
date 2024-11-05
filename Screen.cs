using RayTracing.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using ThreadState = System.Threading.ThreadState;
using System.Reflection;
using Assimp;

namespace RayTracing
{
    public static class Screen
    {
        public static int w, h;

        public static int samples = 32;
        public static int bounces = 6;

        //X - forward
        //Y - right
        //Z - up

        public static float rate => ((float)w / (float)h);

        static Vector3 cameraPos = new Vector3(-5, 0, -1); //1 0 5 //-5 0 4
        //static Vector3 cameraPos = new Vector3(-6.5f, 8, 0.82f); //1 0 5 //-5 0 4
        static Vector3 cameraRot = new Vector3(0, 0, 0);
        //static Vector3 cameraRot = new Vector3(0.76f, -4, -57);

        //static Color backgroundColor = Color.FromArgb(14, 15, 34);
        static Vector3 bgColor = new Vector3(14f/255f, 15f/255f, 36f/255f);
        //static Material backgroundMaterial = Material.Lamp(new Color().FromVec3(bgColor), backgroundIntensity);

        static float backgroundIntensity = 0.9f;
        static float inderectLightIntensity = 0.12f;
        static Vector3 glColor = new Vector3(1, 1, 1);


        static Bitmap screen;
        static float fov = 0.9f; // 0.34

        static Stopwatch sw;
        static int rendered = 0;

        static int threadCount = 32;
        static public Thread[] threads = new Thread[threadCount];
        static public Color[,,] screens;

        public static void RenderRange(int w1, int w2, int num)
        {
            Console.WriteLine(num);
            for (int i = w1; i < w2; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    rendered++;
                    Vector3 totalColor = Vector3.Zero;
                    double x = (((double)i / (double)w) * 2) - 1;
                    double y = (((double)j / (double)h) * 2) - 1;
                    y *= -1;
                    x *= rate;

                    x *= fov;
                    y *= fov;

                    x *= 0.75f;
                    y *= 0.75f;
                    var rayDirection = new Vector3(1, (float)x, (float)y); //forward //right //up

                    rayDirection = VectorFunctions.RotateVector(rayDirection, cameraRot / 180f * (float)Math.PI);
                    
                    rayDirection = Vector3.Normalize(rayDirection);

                    for (int b = 0; b < samples; b++)
                    {
                        Vector3 sampleColor = TracePath(cameraPos, rayDirection);
                        totalColor += sampleColor;
                    }

                    totalColor /= samples;
                    totalColor = Vector3.Clamp(totalColor, Vector3.Zero, Vector3.One * 255);

                    screens[num, i - w1, j] = Color.FromArgb(255, (int)totalColor.X, (int)totalColor.Y, (int)totalColor.Z);
                }
            }
        }

        public static void RenderThreaded(int _w, int _h, string filename = "render.png")
        {
            sw = new Stopwatch();
            sw.Start();

            w = _w;
            h = _h;
            int ind = 0;
            int step = _w / threadCount;
            screens = new Color[threadCount, step, _h];
            List<(int from, int to, int index)> ranges = new List<(int from, int to, int index)>();
            for(int i = 0; i <= _w - step; i += step)
            {
                Console.WriteLine(i.ToString() + " " + (i + step) + " " + ind);
                ranges.Add((i, i + step, ind));
                threads[ind] = new Thread(() => RenderRange(i, i + step, ind));
                threads[ind].Start();
                if (ind == threadCount - 1) break;
                Thread.Sleep(150);
                ind++;
            }

            Console.Clear();
            /*
            foreach(var th in threads)
            {
                th.Start();
            }
            */

            float wh = w * h;

            //IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;

            //askbarProgress.Reset(hwnd);
            Console.CursorVisible = false;

            while (threads.Any(th => th.ThreadState == ThreadState.Running))
            {
                Thread.Sleep(500);
                Console.SetCursorPosition(0, 0);
                var percent = ((float)rendered / wh) * 100;

                //TaskbarProgress.SetValue(hwnd, (double)percent, 100);

                Console.WriteLine(percent.ToString("0.00") + "%");
                Console.WriteLine($"Threads working: {threads.Count(el => el.ThreadState == ThreadState.Running)} / {threadCount}          ");
            }

            //TaskbarProgress.Reset(hwnd);
            Console.CursorVisible = true;

            Console.Clear();

            MergeScreens(step);

            try
            {
                screen.Save(filename);
            }
            catch
            {
                screen.Save("2cs.png");
            }
            //Console.WriteLine("Denoising...");
            //var gd = new GdPictureImaging();
            //int imageId = gd.CreateGdPictureImageFromFile(filename);
            //gd.FxBitonalDespeckle(imageId, false);
            //gd.SaveAsPNG(imageId, filename);
            //gd.ReleaseGdPictureImage(imageId);

            Console.WriteLine(sw.Elapsed);

            Console.WriteLine($"saved as {filename}");
            //Console.WriteLine(min);
            //Console.WriteLine(max);
        }

        static void MergeScreens(int step)
        {
            Console.WriteLine("Merging...");
            screen = new Bitmap(w, h);
            for(int i = 0; i < threads.Length; i++)
            {
                for (int x = 0; x < step; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        if (i * step + x >= w) continue;
                        screen.SetPixel(i * step + x, y, screens[i, x, y]);
                    }
                }
            }
            try
            {
                Console.Clear();
            }
            catch { }
        }
        
        static void Denoise()
        {
            Console.Clear();
            Console.WriteLine("Denoising");
            for (int i = screen.Width - 2; i >=0; i--)
            {
                for (int j = 0; j < screen.Height - 1; j++)
                {
                    var c = screen.GetPixel(i, j);
                    if(c.R + c.G + c.B < 42)
                    {
                        screen.SetPixel(i, j, screen.GetPixel(i + 1, j));
                    }
                }
            }
        }

        static float max = 0, min = 0;
        static bool CastRay(Vector3 origin, Vector3 dir, out float fraction, out Vector3 normal, out Material material)
        {
            float far = 1000000;
            float minDistance = far;
            normal = Vector3.Zero;
            material = new Material();

            foreach (var shape in Scene.shapes)
            {
                float F;
                Vector3 N;

                if (shape.Intersect(origin, dir, out F, out N) && F < minDistance)
                {
                    minDistance = F;
                    normal = Vector3.Normalize(N);
                    material = shape.material;
                }
            }

            /*
            if (!intersected)
            {
                normal = -dir;
                material = Material.Lamp(new Color().FromVec3(bgColor), backgroundIntensity);
            }
            */

            fraction = minDistance;


            return minDistance != far;
        }

        static float N_IN = 0.98f;
        static float N_OUT = 1f;
        static Vector3 TracePath(Vector3 origin, Vector3 dir)
        {
            Vector3 L = Vector3.Zero;
            Vector3 F = Vector3.One;

            bool anyHit = false;
            for (int i = 0; i < bounces; i++)
            {
                float fraction;
                Vector3 normal;
                Material material;

                dir = Vector3.Normalize(dir);
                bool hit = CastRay(origin, dir, out fraction, out normal, out material);

                if (hit)
                {
                    anyHit = true;
                    Vector3 newOrigin = origin + fraction * dir;

                    /*
                    Vector3 newDir = Vector3.Reflect(dir, 
                        VectorFunctions.RotateVector(normal,
                        new Vector3(
                            (float)random.NextDouble() * 2f - 1f,
                            (float)random.NextDouble() * 2f - 1f,
                            (float)random.NextDouble() * 2f - 1f
                        ) * 1.57f));
                    */

                    var randVector = new Vector3(
                            (float)random.NextDouble() * 2f - 1f,
                            (float)random.NextDouble() * 2f - 1f,
                            (float)random.NextDouble() * 2f - 1f
                        ) * 1.35f;

                    Vector3 newDir = VectorFunctions.RotateVector(normal,
                        randVector);
                
                    /*
                    
                    Vector3 hemisphereDistrDirection = NormalOrientedHemispherePoint(normal);

                    Vector3 randomVector = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
                    randomVector = Vector3.Normalize(2.0f * randomVector - Vector3.One);

                    Vector3 tangent = Vector3.Cross(randomVector, normal);
                    Vector3 bitangent = Vector3.Cross(normal, tangent);

                    OpenTK.Mathematics.Matrix3 transform = new OpenTK.Mathematics.Matrix3(tangent.X, bitangent.X, normal.X, tangent.Y, bitangent.Y, normal.Y, tangent.Z, bitangent.Z, normal.Z);

                    var _newDir = transform * new OpenTK.Mathematics.Vector3(hemisphereDistrDirection.X, hemisphereDistrDirection.Y, hemisphereDistrDirection.Z);
                    Vector3 newDir = new Vector3(_newDir.X, _newDir.Y, _newDir.Z);

                    */

                    bool refracted = IsRefracted(dir, normal, material.opacity, N_IN, N_OUT);
                    if (refracted)
                    {
                        Vector3 idealRefraction = IdealRefract(dir, normal, N_IN, N_OUT);
                        newDir = Vector3.Normalize(Vector3.Lerp(-newDir, idealRefraction, material.roughness));
                        newOrigin += normal * (Vector3.Dot(newDir, normal) < 0.0 ? -0.001f : 0.001f);
                    }
                    else
                    {
                        Vector3 idealReflection = Vector3.Reflect(dir, normal);
                        newDir = Vector3.Normalize(Vector3.Lerp(newDir, idealReflection, material.roughness));
                        newOrigin += normal * 0.001f;
                    }

                    dir = newDir;
                    origin = newOrigin;

                    Vector3 additionalEmmitance = new Vector3(0, 0, 0);
                   // L += glColor * 0.15f; // global lighting

                    L += F * (material.emmitance + additionalEmmitance);
                    F *= material.reflectance; // reflectance = color
                }
                else
                {
                    L += bgColor * inderectLightIntensity;
                    F = Vector3.Zero;
                }
            }

            if (!anyHit)
            {
                L = bgColor * backgroundIntensity;
            }
            else
            {
                L += glColor * 0.05f;
            }

            return L * 255f;
        }

        static Random random = new Random();

        static Vector3 RandomHemispherePoint()
        {
            double x = random.NextDouble();
            double y = random.NextDouble();

            float cosTheta = (float)Math.Sqrt(1.0 - x);
            float sinTheta = (float)Math.Sqrt(x);
            float phi = 2.0f * (float)Math.PI * (float)y;

            return new Vector3((float)Math.Cos(phi) * sinTheta, (float)Math.Sin(phi) * sinTheta, cosTheta);
        }

        static Vector3 NormalOrientedHemispherePoint(Vector3 normal)
        {
            Vector3 v = RandomHemispherePoint();

            if (Vector3.Dot(v, normal) < 0.0f)
            {
                return -v;
            }
            else
            {
                return v;
            }
        }

        static float FresnelSchlick(float nIn, float nOut, Vector3 direction, Vector3 normal)
        {
            float R0 = ((nOut - nIn) * (nOut - nIn)) / ((nOut + nIn) * (nOut + nIn));
            float fresnel = (float)(R0 + (1.0 - R0) * Math.Pow((1.0 - Math.Abs(Vector3.Dot(direction, normal))), 5.0));
            return fresnel;
        }

        static Vector3 Refract(Vector3 I, Vector3 normal, float ratio)
        {
            Vector3 R;
            var k = 1.0 - ratio * ratio * (1.0 - Vector3.Dot(normal, I) * Vector3.Dot(normal, I));

            if (k < 0.0)
                R = Vector3.Zero;
            else
                R = ratio * I - (float)(ratio * Vector3.Dot(normal, I) + Math.Sqrt(k)) * normal;

            return R;
        }   

        static Vector3 IdealRefract(Vector3 direction, Vector3 normal, float nIn, float nOut)
        {
            bool fromOutside = Vector3.Dot(normal, direction) < 0.0f;

            float ratio = fromOutside ? nOut / nIn : nIn / nOut;

            Vector3 refraction, reflection;
            refraction = fromOutside ? Refract(direction, normal, ratio) : -Refract(-direction, normal, ratio);
            reflection = Vector3.Reflect(direction, normal);

            return refraction == Vector3.Zero ? reflection : refraction;
        }

        /*
        static Vector3 MatrixMultiply(Vector3 va, Vector3 vb, Vector3 vc, Vector3 d)
        {
            float a, b, c; //transform
            
            a = va.X * d.X + vb.X * d.X + vc.X * d.X;
            b = va.Y * d.Y + vb.Y * d.Y + vc.Y * d.Y;
            c = va.Z * d.Z + vb.Z * d.Z + vc.Z * d.Z;

            
            //a = va.X * d.X + va.Y * d.Y + va.Z * d.Z;
            //b = vb.X * d.X + vb.Y * d.Y + vb.Z * d.Z;
            //c = vc.X * d.X + vc.Y * d.Y + vc.Z * d.Z;
            

            return Vector3.Normalize(new Vector3(a, b, c));
        }
        */

        static bool IsRefracted(Vector3 direction, Vector3 normal, float opacity, float nIn, float nOut)
        {
            float rand = (float)random.NextDouble();
            return opacity > rand;
            float fresnel = FresnelSchlick(nIn, nOut, direction, normal);
            return opacity > rand && fresnel < rand;
        }

        public static float ConvertToRadians(float angle)
        {
            return (float)(Math.PI / 180) * angle;
        }
    }
}
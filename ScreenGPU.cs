using OpenCLTemplate;
using RayTracing.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    public static class ScreenGPU
    {
        static Random random = new Random();
        public static float rate => w / h;
        public static float w = 1920;
        public static float h = 1080;
        public static float fov = 1f;//1f
        public static int samples = 64;
        public static int bounces = 4;

        public static Vector3 cameraPos = new Vector3(-4, 0, -1); //1 0 5 //-5 0 4
        //static Vector3 cameraPos = new Vector3(-6.5f, 8, 0.82f); //1 0 5 //-5 0 4
        public static Vector3 cameraRot = new Vector3(0, 0, 0);
        //static Vector3 cameraRot = new Vector3(0.76f, -4, -57);

        static Vector3 bgColor = new Vector3(14f / 255f, 15f / 255f, 36f / 255f);
        static float backgroundIntensity = 0.9f;
        static float inderectLightIntensity = 0.12f;
        static Vector3 glColor = new Vector3(1, 1, 1);
        static float nIn = 0.98f;
        static float nOut = 1f;

        public static void Render(string filename)
        {
            Bitmap bmp = new Bitmap((int)w, (int)h);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var clr = Pixel(i, j);
                    var color = Color.FromArgb(255, (byte)clr.X, (byte)clr.Y, (byte)clr.Z);
                    bmp.SetPixel(i, j, color);

                }
            }
            bmp.Save(filename);
        }

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

                float far = 1000000;
                float minDistance = far;
                normal = Vector3.Zero;
                material = new Material();

                foreach (var shape in Scene.shapes)
                {
                    float FR;
                    Vector3 N;

                    if (shape.Intersect(origin, dir, out FR, out N) && FR < minDistance)
                    {
                        minDistance = FR;
                        normal = Vector3.Normalize(N);
                        material = shape.material;
                    }
                }

                fraction = minDistance;
                bool hit = minDistance != far;

                if (hit)
                {
                    anyHit = true;
                    Vector3 newOrigin = origin + fraction * dir;


                    Vector3 newDir = VectorFunctions.RotateVector(normal,
                        new Vector3(
                            (float)random.NextDouble() * 2f - 1f,
                            (float)random.NextDouble() * 2f - 1f,
                            (float)random.NextDouble() * 2f - 1f
                        ) * 1.35f);

                    bool refracted = material.opacity > random.NextDouble();
                    if (refracted)
                    {
                        Vector3 idealRefraction = IdealRefract(dir, normal);
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

                    L += F * (material.emmitance + additionalEmmitance);
                    F *= material.reflectance;
                }
                else
                {
                    L += bgColor * inderectLightIntensity;
                    F = Vector3.Zero;
                }
            }

            if (anyHit)
            {
                L += glColor * 0.05f;
            }
            else
            {
                L = bgColor * backgroundIntensity;
            }

            return L * 255f;
        }

        static Vector3 Pixel(int i, int j)
        {
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

            return totalColor;
        }

        static Vector3 IdealRefract(Vector3 direction, Vector3 normal)
        {
            bool fromOutside = Vector3.Dot(normal, direction) < 0.0f;

            float ratio = fromOutside ? nOut / nIn : nIn / nOut;

            Vector3 refraction, reflection;
            refraction = fromOutside ? Refract(direction, normal, ratio) : -Refract(-direction, normal, ratio);
            reflection = Vector3.Reflect(direction, normal);

            return refraction == Vector3.Zero ? reflection : refraction;
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

        public static float[] accumulatedScreen = new float[(int)(w * h * 3)];
        public static float[] res = new float[(int)(w * h * 3)];

        public static int repeatTime = 0;
        public static void RenderGPURepeated(string filename, int repeats = 1, Action<float[]> OnStageRendered = null)
        {
            CompileGPUProgram();
            for (int i = 0; i < repeats; i++)
            {
                repeatTime = i;
                RenderGPU(filename);
                res = accumulatedScreen.Select(el => el /= (float)(i + 1)).ToArray();
                /*
                for (int j = 0; j < accumulatedScreen.Length; j++)
                {
                    res[j] = accumulatedScreen[j] / (float)i;
                }

                OnStageRendered?.Invoke(res);
                */
                GC.Collect();
            }

            for (int i = 0; i < accumulatedScreen.Length; i++)
            {
                accumulatedScreen[i] /= (float)repeats;
            }
            SaveToFile(accumulatedScreen, filename);

            accumulatedScreen = new float[(int)(w * h * 3)];
        }
        public static void RenderGPU(string filename, bool writeFile = false)
        {
            var renderFunction = new CLCalc.Program.Kernel("Render");

            float[] renderData1 = GetRenderData();

            CLCalc.Program.Variable renderData1Var = new CLCalc.Program.Variable(renderData1);
            CLCalc.Program.Variable ScreenVar = new CLCalc.Program.Variable(new float[(int)(w * h * 3)]);
            CLCalc.Program.Variable TimeVar = new CLCalc.Program.Variable(new float[] { (float)random.Next() + (float)random.NextDouble() });

            CLCalc.Program.Variable[] arguments = new CLCalc.Program.Variable[] { renderData1Var, ScreenVar, TimeVar };

            int[] workers = new int[2] { (int)w, (int)h };

            renderFunction.Execute(arguments, workers);

            float[] screenResult = new float[(int)(w * h * 3)];
            ScreenVar.ReadFromDeviceTo(screenResult);

            GC.Collect();

            AppendScreen(screenResult);
            if (writeFile)
            {
                SaveToFile(screenResult, filename);
            }
            Console.WriteLine("a");
        }

        private static void CompileGPUProgram()
        {
            CLCalc.Program.Compile(File.ReadAllText("GpuFunctions/RayTracing.c")
                .Replace("__global Triangle triangles[10000];", $"__global Triangle triangles[{Scene.shapes.Count}];"));
        }

        private static void AppendScreen(float[] screenResult)
        {
            for(int i = 0; i < accumulatedScreen.Length; i++)
            {
                accumulatedScreen[i] += screenResult[i];
            }
        }

        private static float[] GetRenderData()
        {
            var trs = Scene.shapes.Where(el => el.GetType() == typeof(Triangle)).ToArray();
            var trCount = trs.Length;

            var renderData1 = new float[]
            {
                rate,//0
                w,//1
                h,//2
                fov,//3
                backgroundIntensity,//4
                inderectLightIntensity,//5
                nIn,//6
                nOut,//7
                samples,//8
                bounces,//9
                trCount,//10
                cameraPos.X, cameraPos.Y, cameraPos.Z, // 11 12 13
                cameraRot.X, cameraRot.Y, cameraRot.Z, // 14 15 16
                bgColor.X, bgColor.Y, bgColor.Z, // 17 18 19
                glColor.X, glColor.Y, glColor.Z, // 20 21 22
            }.ToList();

            foreach (var shape in trs)
            {
                var tr = (Triangle)shape;
                renderData1.Add(tr.A.X); //23
                renderData1.Add(tr.A.Y);//24
                renderData1.Add(tr.A.Z);//25
                renderData1.Add(tr.B.X);//26
                renderData1.Add(tr.B.Y);//27
                renderData1.Add(tr.B.Z);//28
                renderData1.Add(tr.C.X);//29
                renderData1.Add(tr.C.Y);//30
                renderData1.Add(tr.C.Z);//31
                renderData1.Add(tr.E1.X);//32
                renderData1.Add(tr.E1.Y);//33
                renderData1.Add(tr.E1.Z);//34
                renderData1.Add(tr.E2.X);//35
                renderData1.Add(tr.E2.Y);//36
                renderData1.Add(tr.E2.Z);//37
                renderData1.Add(tr.normal.X);//38
                renderData1.Add(tr.normal.Y);//39
                renderData1.Add(tr.normal.Z);//40

                renderData1.Add(shape.material.emmitance.X);//41
                renderData1.Add(shape.material.emmitance.Y);//42
                renderData1.Add(shape.material.emmitance.Z);//43
                renderData1.Add(shape.material.reflectance.X);//44
                renderData1.Add(shape.material.reflectance.Y);//45
                renderData1.Add(shape.material.reflectance.Z);//46
                renderData1.Add(shape.material.roughness);//47
                renderData1.Add(shape.material.opacity);//48
            }

            return renderData1.ToArray();
        }

        private static void SaveToFile(float[] screenResult, string filename)
        {
            Bitmap bmp = new Bitmap((int)w, (int)h);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    int location = (j * (int)w + i) * 3;

                    int r = (int)(screenResult[location + 0]);
                    int g = (int)(screenResult[location + 1]);
                    int b = (int)(screenResult[location + 2]);

                    r = Math.Clamp(r, 0, 255);
                    g = Math.Clamp(g, 0, 255);
                    b = Math.Clamp(b, 0, 255);
                    Color clr = Color.FromArgb(255, r, g, b);

                    if(r != 255)
                    {

                    }
                    bmp.SetPixel(i, j, clr);
                }
            }
            bmp.Save(filename);
        }
    }

}

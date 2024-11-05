using RayTracing.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RayTracing
{
    public static class SceneReader
    {
        public static void ClearScene()
        {
            Scene.shapes.Clear();
            GC.Collect();
        }
        public static void ReadSceneFromFile(string filename)
        {
            var lines = File.ReadAllLines(filename);

            for(int i = 0; i < lines.Length; i++)
            {
                List<string> words = lines[i].Split().ToList();

                int c = 1;
                string[] line = new string[1] { " " };
                while (line[0] != "}")
                {
                    line = lines[i + c].Split();
                    words.AddRange(line);
                    c++;
                }

                words = words.Where(word => word != "").ToList();

                if (words[0] == "cube")
                {
                    Vector3 position = Vector3.Zero;
                    Vector3 size = Vector3.One;
                    Color color = Color.White;
                    Material mat = Material.Diffuse();
                    float roughness = 0.8f;
                    float power = 1f;
                    for (int j = 0; j < words.Count; j++)
                    {
                        if (words[j] == "pos:")
                        {
                            position.X = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            position.Y = float.Parse(words[j + 2], CultureInfo.InvariantCulture);
                            position.Z = float.Parse(words[j + 3], CultureInfo.InvariantCulture);
                            j += 3;
                        }
                        if (words[j] == "roughness:")
                        {
                            roughness = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            j++;
                        }
                        if (words[j] == "power:")
                        {
                            power = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            j++;
                        }
                        if (words[j] == "size:")
                        {
                            size.X = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            size.Y = float.Parse(words[j + 2], CultureInfo.InvariantCulture);
                            size.Z = float.Parse(words[j + 3], CultureInfo.InvariantCulture);
                            j += 3;
                        }
                        if (words[j] == "color:")
                        {
                            color = Color.FromArgb(255, int.Parse(words[j + 1]), int.Parse(words[j + 2]), int.Parse(words[j + 3]));
                            j += 3;
                        }
                        if (words[j] == "material:")
                        {
                            if (words[j + 1] == "Diffuse")
                            {
                                mat = Material.Diffuse(color, roughness);
                            }
                            if (words[j + 1] == "Lamp")
                            {
                                mat = Material.Lamp(color, power);
                            }
                            if (words[j + 1] == "Glossy")
                            {
                                mat = Material.Glossy(color);
                            }
                            j++;
                        }
                    }

                    Scene.shapes.Add(new Cube(position, size, mat));
                }

                if (words[0] == "sphere")
                {
                    Vector3 position = Vector3.Zero;
                    float radius = 1f;
                    Color color = Color.White;
                    Material mat = Material.Diffuse();
                    float roughness = 0.8f;
                    float power = 1f;
                    for (int j = 0; j < words.Count; j++)
                    {
                        if (words[j] == "pos:")
                        {
                            position.X = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            position.Y = float.Parse(words[j + 2], CultureInfo.InvariantCulture);
                            position.Z = float.Parse(words[j + 3], CultureInfo.InvariantCulture);
                            j += 3;
                        }
                        if (words[j] == "roughness:")
                        {
                            roughness = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            j++;
                        }
                        if (words[j] == "power:")
                        {
                            power = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            j++;
                        }
                        if (words[j] == "radius:")
                        {
                            radius = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            j++;
                        }
                        if (words[j] == "color:")
                        {
                            color = Color.FromArgb(255, int.Parse(words[j + 1]), int.Parse(words[j + 2]), int.Parse(words[j + 3]));
                            j += 3;
                        }
                        if (words[j] == "material:")
                        {
                            if (words[j + 1] == "Diffuse")
                            {
                                mat = Material.Diffuse(color, roughness);
                            }
                            if (words[j + 1] == "Lamp")
                            {
                                mat = Material.Lamp(color, power);
                            }
                            if (words[j + 1] == "Glossy")
                            {
                                mat = Material.Glossy(color);
                            }
                            if (words[j + 1] == "Glass")
                            {
                                mat = Material.Glass(color);
                            }
                            j++;
                        }
                    }

                    Scene.shapes.Add(new Sphere(position, mat, radius));
                }

                if (words[0] == "model")
                {
                    Vector3 position = Vector3.Zero;
                    Vector3 size = Vector3.One;
                    Color color = Color.White;
                    Material mat = Material.Diffuse();
                    string path = "";
                    string texture = "";
                    float roughness = 0.6f;
                    bool invertNormals = false;
                    float power = 1f;
                    for (int j = 0; j < words.Count; j++)
                    {
                        if (words[j] == "texture:")
                        {
                            texture = words[j + 1];
                            TextureLoader.LoadTexture(texture);
                            j++;
                        }
                        if (words[j] == "path:")
                        {
                            path = words[j + 1];
                            j++;
                        }
                        if (words[j] == "pos:")
                        {
                            position.X = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            position.Y = float.Parse(words[j + 2], CultureInfo.InvariantCulture);
                            position.Z = float.Parse(words[j + 3], CultureInfo.InvariantCulture);
                            j += 3;
                        }
                        if (words[j] == "size:")
                        {
                            size.X = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            size.Y = float.Parse(words[j + 2], CultureInfo.InvariantCulture);
                            size.Z = float.Parse(words[j + 3], CultureInfo.InvariantCulture);
                            j += 3;
                        }
                        if (words[j] == "color:")
                        {
                            color = Color.FromArgb(255, int.Parse(words[j + 1]), int.Parse(words[j + 2]), int.Parse(words[j + 3]));
                            j += 3;
                        }
                        if (words[j] == "power:")
                        {
                            power = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            j++;
                        }
                        if (words[j] == "roughness:")
                        {
                            roughness = float.Parse(words[j + 1], CultureInfo.InvariantCulture);
                            j++;
                        }
                        if (words[j] == "invertNormals:")
                        {
                            if (words[j + 1] == "true")
                                invertNormals = true;
                            j++;
                        }
                        if (words[j] == "material:")
                        {
                            if (words[j + 1] == "Diffuse")
                            {
                                mat = Material.Diffuse(color, roughness);
                            }
                            if (words[j + 1] == "Lamp")
                            {
                                mat = Material.Lamp(color, power);
                            }
                            if (words[j + 1] == "Glossy")
                            {
                                mat = Material.Glossy(color);
                            }
                            j++;
                        }
                    }

                    if (texture != "")
                        TextureLoader.LoadTexture(texture);

                    ModelImporter.ImportModel(path, position, size, mat, texture, invertNormals);
                }

                i += c - 1;
            }
        }
    }
}

using Assimp;
using Matrix4 = OpenTK.Mathematics.Matrix4;
using Quaternion = OpenTK.Mathematics.Quaternion;
using RayTracing.Shapes;
using System.Numerics;
using OpenTK.Mathematics;

namespace RayTracing
{
    public static class ModelImporter
    {

        public static Triangle[] ImportModel(string filename, System.Numerics.Vector3 position, System.Numerics.Vector3 size, Material mat, string texture = "", bool invertNormals = false)
        {
            var importer = new AssimpContext();
            if (!importer.IsImportFormatSupported(Path.GetExtension(filename)))
            {
                throw new ArgumentException("Model format " + Path.GetExtension(filename) + " is not supported!  Cannot load {1}", "filename");
            }

            Console.WriteLine("Importing " + filename);
            if (!File.Exists(filename)) { throw new FileNotFoundException(filename); }

            var model = importer.ImportFile(filename, PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.CalculateTangentSpace);

            var indices = model.Meshes[0].GetIndices();

            Triangle[] triangles = new Triangle[indices.Length / 3];

            for (int i=0;i < indices.Length / 3; i++)
            {
                var v1 = model.Meshes[0].Vertices[indices[i * 3 + 0]];
                var v2 = model.Meshes[0].Vertices[indices[i * 3 + 1]];
                var v3 = model.Meshes[0].Vertices[indices[i * 3 + 2]];
                v1 = new Vector3D(v1.X * size.X, v1.Y * size.Y, v1.Z * size.Z);
                v2 = new Vector3D(v2.X * size.X, v2.Y * size.Y, v2.Z * size.Z);
                v3 = new Vector3D(v3.X * size.X, v3.Y * size.Y, v3.Z * size.Z);

                triangles[i] = new Triangle(
                    new System.Numerics.Vector3(v1.X, v1.Y, v1.Z) + position,
                    new System.Numerics.Vector3(v2.X, v2.Y, v2.Z) + position,
                    new System.Numerics.Vector3(v3.X, v3.Y, v3.Z) + position,
                    mat,
                    invertNormals
                    );


                var tcoord0 = model.Meshes[0].TextureCoordinateChannels[0][indices[i * 3 + 0]];
                var tcoord1 = model.Meshes[0].TextureCoordinateChannels[0][indices[i * 3 + 1]];
                var tcoord2 = model.Meshes[0].TextureCoordinateChannels[0][indices[i * 3 + 2]];

                triangles[i].SetTextureCoordinates(tcoord0, tcoord1, tcoord2);
                triangles[i].texture = texture;
                Scene.shapes.Add(triangles[i]);
            }
            
            return triangles;
        }
    }
}

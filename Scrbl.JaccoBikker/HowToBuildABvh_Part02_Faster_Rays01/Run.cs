using Scrbl.JaccoBikker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using static Scrbl.JaccoBikker.Bvh.HowToBuildABvh_Part01_Basics_Step02;

namespace Scrbl.JaccoBikker.Bvh
{
    internal class HowToBuildABvh_Part02_Faster_Rays01 : HowToBuildABvh_Part01_Basics_Step03_Struct_BvhNode
    {

        public new void Run(RayTraceSettings settings)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"// ------------------------ //");
            Console.WriteLine($"");

            Console.WriteLine("Rendering HowToBuildABvh_Part02_Faster_Rays01...");
            Console.WriteLine("Setup...");

            var scene = CreateScene(settings, RandomGenerator);

            var bvh = new BvhGenerator().Build(scene);

            stopwatch.Stop();

            Console.WriteLine($"Setup duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine("BvhNode size: " + Marshal.SizeOf(typeof(BvhNode)));

            Console.WriteLine("Rendering...");

            stopwatch.Restart();

            RenderScene(scene, bvh, settings);

            stopwatch.Stop();

            Console.WriteLine("Rendering finished...");

            Console.WriteLine($"Render duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine($"");
            Console.WriteLine($"// ------------------------ //");
        }


        new public Scene CreateScene(RayTraceSettings settings, IRandomGenerator randomGenerator)
        {
            var scene = new Scene();

            LoadSceneFromTriFile(@"C:\Steven\Atelier\Scrbl\Scrbl.JaccoBikker\.assets\unity.tri", scene);

            return scene;
        }

        new public void RenderScene(Scene scene, Bvh bvh, RayTraceSettings settings)
        {
            var saveImage = !string.IsNullOrEmpty(settings.ImageSavePath);

            var buffer = saveImage ? new Vector3d[settings.ImageWidth * settings.ImageHeight] : null;

            var index = 0;

            Vector3f p0 = new(-2.5f, 0.8f, -0.5f), p1 = new(-0.5f, 0.8f, -0.5f), p2 = new(-2.5f, -1.2f, -0.5f);
            var ray = new Ray();

            var nearest = float.PositiveInfinity;
            var furthest = float.NegativeInfinity;

            for (int y = 0; y < settings.ImageHeight; y++)
            {
                for (int x = 0; x < settings.ImageWidth; x++)
                {
                    Vector3f pixelPos = p0 + (p1 - p0) * ((float)x / (float)settings.ImageWidth) + (p2 - p0) * ((float)y / settings.ImageHeight);

                    ray.O = settings.CameraPosition;
                    ray.D = Vector3f.Normalize(pixelPos - ray.O);
                    ray.T = float.PositiveInfinity;

                    var pixel = new Color(0, 0, 0);


                    if (bvh.Intersection(ray, scene, out float t))
                    {
                        nearest = Math.Min(nearest, t);
                        furthest = Math.Max(furthest, t);

                        var c = Utility.Remap(t, 1.2f, 3.4f, 1f, 0f, true);

                        pixel = new Color(c, c, c);
                    }

                    buffer?[index++] = pixel;
                }
            }

            Console.WriteLine($"Nearest intersection: {nearest}");
            Console.WriteLine($"Furthest intersection: {furthest}");

            if (buffer != null)
                Png.Save(settings.ImageSavePath, settings.ImageWidth, settings.ImageHeight, buffer);
        }

        private void LoadSceneFromTriFile(string path, Scene scene)
        {
            char[] SplitOptions = new[] { ' ', '\t', '\n', '\r' };

            using (var reader = new StreamReader(path))
            {
                string line = reader.ReadLine();

                string[] tokens = line.Split(SplitOptions, StringSplitOptions.RemoveEmptyEntries);

                var triangleCount = uint.Parse(tokens[0], CultureInfo.InvariantCulture);

                scene.TriangleCount = triangleCount;
                scene.Triangles = new Triangle[triangleCount];
                scene.TriangleIndices = new uint[triangleCount];

                for (uint t = 0; t < triangleCount; t++)
                {
                    line = reader.ReadLine();

                    // Split by spaces, removing any extra empty entries from multiple spaces or trailing \n
                    tokens = line.Split(SplitOptions, StringSplitOptions.RemoveEmptyEntries);

                    // Use InvariantCulture to ensure dots ('.') are correctly parsed as decimals regardless of system language
                    float a = float.Parse(tokens[0], CultureInfo.InvariantCulture);
                    float b = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                    float c = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                    float d = float.Parse(tokens[3], CultureInfo.InvariantCulture);
                    float e = float.Parse(tokens[4], CultureInfo.InvariantCulture);
                    float f = float.Parse(tokens[5], CultureInfo.InvariantCulture);
                    float g = float.Parse(tokens[6], CultureInfo.InvariantCulture);
                    float h = float.Parse(tokens[7], CultureInfo.InvariantCulture);
                    float i = float.Parse(tokens[8], CultureInfo.InvariantCulture);

                    scene.Triangles[t] = new Triangle
                    {
                        vertex0 = new Vector3f(a, b, c),
                        vertex1 = new Vector3f(d, e, f),
                        vertex2 = new Vector3f(g, h, i)
                    };

                    scene.TriangleIndices[t] = t;
                }
            }
        }
    }
}

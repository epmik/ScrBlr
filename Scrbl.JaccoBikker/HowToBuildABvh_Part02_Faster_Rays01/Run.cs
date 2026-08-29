using Scrbl.JaccoBikker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
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
            var time = float.PositiveInfinity;

            for (int y = 0; y < settings.ImageHeight; y++)
            {
                for (int x = 0; x < settings.ImageWidth; x++)
                {
                    Vector3f pixelPos = p0 + (p1 - p0) * ((float)x / (float)settings.ImageWidth) + (p2 - p0) * ((float)y / settings.ImageHeight);

                    ray.Origin = settings.CameraPosition;
                    ray.Direction = Vector3f.Normalize(pixelPos - ray.Origin);
                    ray.Time = float.PositiveInfinity;

                    var pixel = new Color(0, 0, 0);

                    time = float.PositiveInfinity;

                    if (bvh.Intersection(ray, scene, ref time))
                    {
                        nearest = Math.Min(nearest, time);
                        furthest = Math.Max(furthest, time);

                        var c = Utility.Remap(time, 1.2f, 3.4f, 1f, 0f, true);

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

                    tokens = line.Split(SplitOptions, StringSplitOptions.RemoveEmptyEntries);

                    scene.Triangles[t].A.X = float.Parse(tokens[0], CultureInfo.InvariantCulture);
                    scene.Triangles[t].A.Y = float.Parse(tokens[1], CultureInfo.InvariantCulture);
                    scene.Triangles[t].A.Z = float.Parse(tokens[2], CultureInfo.InvariantCulture);
                    scene.Triangles[t].B.X = float.Parse(tokens[3], CultureInfo.InvariantCulture);
                    scene.Triangles[t].B.Y = float.Parse(tokens[4], CultureInfo.InvariantCulture);
                    scene.Triangles[t].B.Z = float.Parse(tokens[5], CultureInfo.InvariantCulture);
                    scene.Triangles[t].C.X = float.Parse(tokens[6], CultureInfo.InvariantCulture);
                    scene.Triangles[t].C.Y = float.Parse(tokens[7], CultureInfo.InvariantCulture);
                    scene.Triangles[t].C.Z = float.Parse(tokens[8], CultureInfo.InvariantCulture);

                    scene.TriangleIndices[t] = t;
                }
            }
        }
    }
}

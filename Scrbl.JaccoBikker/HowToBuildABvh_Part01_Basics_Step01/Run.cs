using Scrbl.JaccoBikker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace Scrbl.JaccoBikker.Bvh
{
    internal class HowToBuildABvh_Part01_Basics_Step01
    {
        public class RayTraceSettings
        {
            public uint TriangleCount { get; init; } = 64;
            public int ImageWidth { get; init; } = 640;
            public int ImageHeight { get; init; } = 640;
            public Vector3f CameraPosition { get; init; } = new Vector3f(0, 0, -18);
            public string ImageSavePath { get; set; }
        }

        protected IRandomGenerator RandomGenerator { get; set; } = new RandomGenerator();

        public void Run(RayTraceSettings settings)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"// ------------------------ //");
            Console.WriteLine($"");

            Console.WriteLine("Rendering How-to-build-a-bvh-part-01-basics-step-01...");
            Console.WriteLine("Setup...");

            var scene = CreateScene(settings, RandomGenerator);

            stopwatch.Stop();

            Console.WriteLine($"Setup duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine("Rendering...");

            stopwatch.Restart();

            RenderScene(scene, settings);

            stopwatch.Stop();

            Console.WriteLine("Rendering finished...");

            Console.WriteLine($"Render duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine($"");
            Console.WriteLine($"// ------------------------ //");
        }

        public void RenderScene(Scene scene,  RayTraceSettings settings)
        {
            var buffer = new Vector3d[settings.ImageWidth * settings.ImageHeight];

            var index = 0;

            Vector3f p0 = new(-1, 1, -15), p1 = new(1, 1, -15), p2 = new(-1, -1, -15);
            var ray = new Ray();

            for (int y = 0; y < settings.ImageHeight; y++)
            {
                for (int x = 0; x < settings.ImageWidth; x++)
                {
                    Vector3f pixelPos = p0 + (p1 - p0) * ((float)x / (float)settings.ImageWidth) + (p2 - p0) * ((float)y / settings.ImageHeight);

                    ray.O = settings.CameraPosition;
                    ray.D = Vector3f.Normalize(pixelPos - ray.O);
                    ray.T = float.PositiveInfinity;

                    var pixel = new Color(0, 0, 0);

                    for (int i = 0; i < scene.TriangleCount; i++)
                    {
                        if(Intersection.Compute(ray, scene.Triangles[i], out var timeResult))
                        {
                            pixel = new Color(1, 1, 1);

                            break;
                        }
                    }

                    buffer[index++] = pixel;
                }
            }

            Png.Save(settings.ImageSavePath, settings.ImageWidth, settings.ImageHeight, buffer);
        }

        public Scene CreateScene(RayTraceSettings settings, IRandomGenerator randomGenerator)
        {
            var scene = new Scene 
            { 
                TriangleCount = settings.TriangleCount, 
                Triangles = new Triangle[settings.TriangleCount],
                TriangleIndices = new uint[settings.TriangleCount],

            };

            for (uint i = 0; i < scene.TriangleCount; i++)
            {
                scene.TriangleIndices[i] = i;
            }

            for (int i = 0; i < scene.TriangleCount; i++)
            {
                Vector3f r0 = randomGenerator.Vector3f();
                Vector3f r1 = randomGenerator.Vector3f();
                Vector3f r2 = randomGenerator.Vector3f();

                scene.Triangles[i] = new Triangle();

                scene.Triangles[i].vertex0 = r0 * 9f - new Vector3f(5f);
                scene.Triangles[i].vertex1 = scene.Triangles[i].vertex0 + r1;
                scene.Triangles[i].vertex2 = scene.Triangles[i].vertex0 + r2;
            }

            return scene;
        }
    }
}

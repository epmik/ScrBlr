using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Scrbl.JaccoBikker.Bvh
{
    internal class HowToBuildABvh_Part01_Basics
    {
        public class RayTraceSettings
        {
            public int TriangleCount { get; init; } = 64;
            public int ImageWidth { get; init; } = 640;
            public int ImageHeight { get; init; } = 640;
            public Vector3d CameraPosition { get; init; } = new Vector3d(0, 0, -18);
        }

        public void Main(string path)
        {
            var settings = new RayTraceSettings 
            { 
            };

            var scene = CreateScene(settings);

            RayTraceScene(scene, settings);
        }

        public void RayTraceScene(Scene scene, RayTraceSettings settings)
        {
            var buffer = new Vector3d[settings.ImageWidth * settings.ImageHeight];

            var index = 0;

            Vector3d p0 = new(-1, 1, -15), p1 = new(1, 1, -15), p2 = new(-1, -1, -15);
            var ray = new Ray();

            for (int y = 0; y < settings.ImageHeight; y++)
            {
                for (int x = 0; x < settings.ImageWidth; x++)
                {
                    Vector3d pixelPos = p0 + (p1 - p0) * ((double)x / (double)settings.ImageWidth) + (p2 - p0) * ((double)y / settings.ImageHeight);
                    ray.O = settings.CameraPosition;
                    ray.D = Vector3d.Normalize(pixelPos - ray.O);
                    ray.T = double.PositiveInfinity;

                    var pixel_color = new Color(0, 0, 0);

                    for (int i = 0; i < scene.TriangleCount; i++)
                    {
                        Intersection.Compute(ray, scene.Triangles[i]);
                    }
                        
                }
            }
        }

        public Scene CreateScene(RayTraceSettings settings)
        {
            var scene = new Scene 
            { 
                TriangleCount = settings.TriangleCount, 
                Triangles = new Triangle[settings.TriangleCount] 
            };

            for (int i = 0; i < N; i++)
            {
                float3 r0(RandomFloat(), RandomFloat(), RandomFloat() );
                float3 r1(RandomFloat(), RandomFloat(), RandomFloat() );
                float3 r2(RandomFloat(), RandomFloat(), RandomFloat() );

                tri[i].vertex0 = r0 * 9 - float3(5);
                tri[i].vertex1 = tri[i].vertex0 + r1;
                tri[i].vertex2 = tri[i].vertex0 + r2;
            }

            return scene;
        }
    }
}

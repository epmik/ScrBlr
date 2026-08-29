using Scrbl.JaccoBikker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using static Scrbl.JaccoBikker.Bvh.HowToBuildABvh_Part01_Basics_Step02;

namespace Scrbl.JaccoBikker.Bvh
{
    internal class HowToBuildABvh_Part01_Basics_Step03_Struct_BvhNode : HowToBuildABvh_Part01_Basics_Step01
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct BvhNode
        {
            public Vector3f Min, Max;
            public uint NodeOrPrimitiveIndex;
            public uint PrimitiveCount;
            public bool IsLeaf => PrimitiveCount > 0;
            public bool IsNode => PrimitiveCount == 0;
        };

        public class BvhGenerator
        {

            private Scene _scene;

            private Bvh _bhv;
            private uint _usedNodeCount = 1;

            public Bvh Build(Scene scene)
            {
                _scene = scene;

                var count = (uint)_scene.TriangleCount;

                _bhv = new Bvh
                {
                    Nodes = new BvhNode[count * 2 - 1],
                };

                _bhv.Nodes[0].PrimitiveCount = count;

                UpdateNodeBounds(ref _bhv.Nodes[0]);
                
                SubdivideRecursive(ref _bhv.Nodes[0]);

                return _bhv;
            }

            void UpdateNodeBounds(ref BvhNode node)
            {
                node.Min = new Vector3f(float.PositiveInfinity);
                node.Max = new Vector3f(float.NegativeInfinity);

                for (uint first = node.NodeOrPrimitiveIndex, i = 0; i < node.PrimitiveCount; i++)
                {
                    uint leafTriIdx = _scene.TriangleIndices[first + i];
                    var leafTri = _scene.Triangles[leafTriIdx];

                    node.Min = Vector3f.Min(node.Min, leafTri.A);
                    node.Min = Vector3f.Min(node.Min, leafTri.B);
                    node.Min = Vector3f.Min(node.Min, leafTri.C);

                    node.Max = Vector3f.Max(node.Max, leafTri.A);
                    node.Max = Vector3f.Max(node.Max, leafTri.B);
                    node.Max = Vector3f.Max(node.Max, leafTri.C);
                }
            }

            void SubdivideRecursive(ref BvhNode node)
            {
                // terminate recursion
                if (node.PrimitiveCount <= 2) 
                    return;

                // determine split axis and position
                var extent = node.Max - node.Min;
                int axis = 0;
                if (extent.Y > extent.X) axis = 1;
                if (extent.Z > extent[axis]) axis = 2;

                var splitPos = node.Min[axis] + extent[axis] * 0.5;

                // in-place partition
                var i = (int)node.NodeOrPrimitiveIndex;
                var j = (int)(i + node.PrimitiveCount - 1);

                while (i <= j)
                {
                    if (Center(_scene.Triangles[_scene.TriangleIndices[i]])[axis] < splitPos)
                    {
                        i++;
                    }
                    else
                    {
                        // https://stackoverflow.com/questions/804706/swap-two-variables-without-using-a-temporary-variable
                        (_scene.TriangleIndices[i], _scene.TriangleIndices[j]) = (_scene.TriangleIndices[j], _scene.TriangleIndices[i]);
                        j--;
                    }
                }

                // abort split if one of the sides is empty
                var leftCount = (uint)i - node.NodeOrPrimitiveIndex;

                if (leftCount == 0 || leftCount == node.PrimitiveCount) 
                    return;
                
                var leftChildIdx = _usedNodeCount++;
                var rightChildIdx = _usedNodeCount++;

                _bhv.Nodes[leftChildIdx].NodeOrPrimitiveIndex = node.NodeOrPrimitiveIndex;
                _bhv.Nodes[leftChildIdx].PrimitiveCount = leftCount;

                _bhv.Nodes[rightChildIdx].NodeOrPrimitiveIndex = (uint)i;
                _bhv.Nodes[rightChildIdx].PrimitiveCount = node.PrimitiveCount - leftCount;

                node.NodeOrPrimitiveIndex = leftChildIdx;
                node.PrimitiveCount = 0;

                UpdateNodeBounds(ref _bhv.Nodes[leftChildIdx]);
                UpdateNodeBounds(ref _bhv.Nodes[rightChildIdx]);

                // recurse
                SubdivideRecursive(ref _bhv.Nodes[leftChildIdx]);
                SubdivideRecursive(ref _bhv.Nodes[rightChildIdx]);
            }

            private static Vector3f Center(in Triangle triangle)
            {
                return triangle.A + triangle.B + triangle.C * 0.33333333f;
            }
        }

        public class Bvh
        {
            public BvhNode[] Nodes { get; set; }

            public bool Intersection(in Ray ray, Scene scene)
            {
                var nearest = float.PositiveInfinity;
                var hit = false;

                IntersectionRecursive(ray, scene, 0, ref nearest, ref hit);

                return hit;
            }

            public bool Intersection(in Ray ray, Scene scene, ref float nearest)
            {
                var hit = false;

                IntersectionRecursive(in ray, scene, 0, ref nearest, ref hit);

                return hit;
            }

            private void IntersectionRecursive(in Ray ray, Scene scene, uint nodeIdx, ref float nearest, ref bool hit)
            {
                ref var node = ref Nodes[nodeIdx];

                if (!Scrbl.JaccoBikker.Intersection.IntersectAABB(in ray, ref node.Min, ref node.Max))
                    return;

                if (node.IsLeaf)
                {
                    var intersectionResult = new Intersection.TimeResult();

                    for (uint i = 0; i < node.PrimitiveCount; i++)
                    {
                        // Avoid deeply nested property lookups in tight loops
                        var triangleIndex = scene.TriangleIndices[node.NodeOrPrimitiveIndex + i];
                        ref var triangle = ref scene.Triangles[triangleIndex];

                        intersectionResult.Time = float.PositiveInfinity;

                        if (Scrbl.JaccoBikker.Intersection.Compute(in ray, ref triangle, ref intersectionResult))
                        {
                            if(intersectionResult.Time < nearest)
                            {
                                nearest = (float)intersectionResult.Time;
                                hit = true; 
                            }
                        }
                    }
                }
                else
                {
                    IntersectionRecursive(in ray, scene, node.NodeOrPrimitiveIndex, ref nearest, ref hit);

                    IntersectionRecursive(in ray, scene, node.NodeOrPrimitiveIndex + 1, ref nearest, ref hit);
                }
            }
        }

        public new void Run(RayTraceSettings settings)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"// ------------------------ //");
            Console.WriteLine($"");

            Console.WriteLine("Rendering HowToBuildABvh_Part01_Basics_Step03_Struct_BvhNode...");
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

        public void RenderScene(Scene scene, Bvh bvh, RayTraceSettings settings)
        {
            var saveImage = !string.IsNullOrEmpty(settings.ImageSavePath);

            var buffer = saveImage ? new Vector3d[settings.ImageWidth * settings.ImageHeight] : null;

            var index = 0;

            Vector3f p0 = new(-1, 1, -15), p1 = new(1, 1, -15), p2 = new(-1, -1, -15);
            var ray = new Ray();

            var nearest = float.PositiveInfinity;
            var furthest  = float.NegativeInfinity;
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

                        pixel = new Color(1, 1, 1);
                    }

                    buffer?[index++] = pixel;
                }
            }

            Console.WriteLine($"Nearest intersection: {nearest}");
            Console.WriteLine($"Furthest intersection: {furthest}");

            if (buffer != null)
                Png.Save(settings.ImageSavePath, settings.ImageWidth, settings.ImageHeight, buffer);
        }
    }
}

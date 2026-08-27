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
        public struct BvhNode
        {
            public Vector3d Min, Max;
            public uint NodeOrPrimitiveIndex;
            public bool IsLeaf => PrimitiveCount > 0;
            public bool IsNode => PrimitiveCount == 0;

            //public uint RightNodeIndex => NodeOrPrimitiveIndex + 1;

            public uint PrimitiveCount;
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
                node.Min = new Vector3d(double.PositiveInfinity);
                node.Max = new Vector3d(double.NegativeInfinity);

                for (uint first = node.NodeOrPrimitiveIndex, i = 0; i < node.PrimitiveCount; i++)
                {
                    uint leafTriIdx = _scene.TriangleIndices[first + i];
                    var leafTri = _scene.Triangles[leafTriIdx];

                    node.Min = Vector3d.Min(node.Min, leafTri.vertex0);
                    node.Min = Vector3d.Min(node.Min, leafTri.vertex1);
                    node.Min = Vector3d.Min(node.Min, leafTri.vertex2);

                    node.Max = Vector3d.Max(node.Max, leafTri.vertex0);
                    node.Max = Vector3d.Max(node.Max, leafTri.vertex1);
                    node.Max = Vector3d.Max(node.Max, leafTri.vertex2);
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

            private static Vector3d Center(Triangle triangle)
            {
                return triangle.vertex0 + triangle.vertex1 + triangle.vertex2 * 0.33333333;
            }
        }

        public class Bvh
        {
            public BvhNode[] Nodes { get; set; }

            public bool Intersection(Ray ray, Scene scene)
            {
                return IntersectionRecursive(ray, scene, 0);
            }

            private bool IntersectionRecursive(Ray ray, Scene scene, uint nodeIdx)
            {
                var node = Nodes[nodeIdx];

                if (!Scrbl.JaccoBikker.Intersection.IntersectAABB(ray, node.Min, node.Max)) 
                        return false;

                if (node.IsLeaf)
                {
                    for (uint i = 0; i < node.PrimitiveCount; i++)
                    {
                        if(Scrbl.JaccoBikker.Intersection.Compute(ray, scene.Triangles[scene.TriangleIndices[node.NodeOrPrimitiveIndex + i]], out var timeResult))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if(IntersectionRecursive(ray, scene, node.NodeOrPrimitiveIndex))
                        return true;

                    if(IntersectionRecursive(ray, scene, node.NodeOrPrimitiveIndex + 1))
                        return true;
                }

                return false;
            }
        }

        public new void Run(RayTraceSettings settings)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"// ------------------------ //");
            Console.WriteLine($"");

            Console.WriteLine("Rendering How-to-build-a-bvh-part-01-basics-step-03...");
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

                    var pixel = new Color(0, 0, 0);

                    if(bvh.Intersection(ray, scene))
                    {
                        pixel = new Color(1, 1, 1);
                    }

                    buffer[index++] = pixel;
                }
            }

            Png.Save(settings.ImageSavePath, settings.ImageWidth, settings.ImageHeight, buffer);
        }
    }
}

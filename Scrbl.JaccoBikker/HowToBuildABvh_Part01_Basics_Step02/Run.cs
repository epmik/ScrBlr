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
    internal class HowToBuildABvh_Part01_Basics_Step02 : HowToBuildABvh_Part01_Basics_Step01
    {
        [StructLayout(LayoutKind.Sequential)]
        public class BvhNode
        {
            public Vector3f Min, Max;
            public uint LeftNodeIndex;
            public bool IsLeaf => PrimitiveCount > 0;

            public uint RightNodeIndex => LeftNodeIndex + 1;

            public uint PrimitiveIndex, PrimitiveCount;
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

                uint rootNodeIdx = 0;
           
                // assign all triangles to root node
                BvhNode root = new BvhNode
                {
                    LeftNodeIndex = 0,
                    PrimitiveIndex = 0,
                    PrimitiveCount = count
                };

                _bhv.Nodes[0] = root;

                UpdateNodeBounds(root);
                
                // subdivide recursively
                SubdivideRecursive(root);

                return _bhv;
            }

            void UpdateNodeBounds(BvhNode node)
            {
                node.Min = new Vector3f(float.PositiveInfinity);
                node.Max = new Vector3f(float.NegativeInfinity);

                for (uint first = node.PrimitiveIndex, i = 0; i < node.PrimitiveCount; i++)
                {
                    uint leafTriIdx = _scene.TriangleIndices[first + i];
                    var leafTri = _scene.Triangles[leafTriIdx];

                    node.Min = Vector3f.Min(node.Min, leafTri.vertex0);
                    node.Min = Vector3f.Min(node.Min, leafTri.vertex1);
                    node.Min = Vector3f.Min(node.Min, leafTri.vertex2);

                    node.Max = Vector3f.Max(node.Max, leafTri.vertex0);
                    node.Max = Vector3f.Max(node.Max, leafTri.vertex1);
                    node.Max = Vector3f.Max(node.Max, leafTri.vertex2);
                }
            }

            void SubdivideRecursive(BvhNode node)
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
                var i = (int)node.PrimitiveIndex;
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
                var leftCount = (uint)i - node.PrimitiveIndex;

                if (leftCount == 0 || leftCount == node.PrimitiveCount) 
                    return;
                
                // create child nodes
                var leftChildIdx = _usedNodeCount++;
                var rightChildIdx = _usedNodeCount++;

                _bhv.Nodes[leftChildIdx] = new BvhNode
                {
                    PrimitiveIndex = node.PrimitiveIndex,
                    PrimitiveCount = leftCount
                };

                _bhv.Nodes[rightChildIdx] = new BvhNode
                {
                    PrimitiveIndex = (uint)i,
                    PrimitiveCount = node.PrimitiveCount - leftCount
                };

                node.LeftNodeIndex = leftChildIdx;
                node.PrimitiveCount = 0;

                UpdateNodeBounds(_bhv.Nodes[leftChildIdx]);
                UpdateNodeBounds(_bhv.Nodes[rightChildIdx]);

                // recurse
                SubdivideRecursive(_bhv.Nodes[leftChildIdx]);
                SubdivideRecursive(_bhv.Nodes[rightChildIdx]);
            }

            private static Vector3f Center(Triangle triangle)
            {
                return triangle.vertex0 + triangle.vertex1 + triangle.vertex2 * 0.33333333f;
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
                        if(Scrbl.JaccoBikker.Intersection.Compute(ray, scene.Triangles[scene.TriangleIndices[node.PrimitiveIndex + i]], out var timeResult))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if(IntersectionRecursive(ray, scene, node.LeftNodeIndex))
                        return true;

                    if(IntersectionRecursive(ray, scene, node.RightNodeIndex))
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

            Console.WriteLine("Rendering How-to-build-a-bvh-part-01-basics-step-02...");
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

            for (int y = 0; y < settings.ImageHeight; y++)
            {
                for (int x = 0; x < settings.ImageWidth; x++)
                {
                    Vector3f pixelPos = p0 + (p1 - p0) * ((float)x / (float)settings.ImageWidth) + (p2 - p0) * ((float)y / settings.ImageHeight);

                    ray.O = settings.CameraPosition;
                    ray.D = Vector3f.Normalize(pixelPos - ray.O);
                    ray.T = float.PositiveInfinity;

                    var pixel = new Color(0, 0, 0);

                    //for (int i = 0; i < scene.TriangleCount; i++)
                    //{
                    //    if (Intersection.Compute(ray, scene.Triangles[i], out var timeResult))
                    //    {
                    //        pixel = new Color(1, 1, 1);

                    //        break;
                    //    }
                    //}

                    if(bvh.Intersection(ray, scene))
                    {
                        pixel = new Color(1, 1, 1);
                    }

                    buffer?[index++] = pixel;
                }
            }

            if(buffer != null)
                Png.Save(settings.ImageSavePath, settings.ImageWidth, settings.ImageHeight, buffer);
        }
    }
}

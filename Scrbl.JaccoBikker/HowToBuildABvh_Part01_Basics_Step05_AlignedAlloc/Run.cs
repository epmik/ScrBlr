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
    internal class HowToBuildABvh_Part01_Basics_Step05_AlignedAlloc : HowToBuildABvh_Part01_Basics_Step01
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct BvhNode
        {
            public Vector3f Min;    // 12 bytes
            public Vector3f Max;    // 12 bytes
            public uint NodeOrPrimitiveIndex;   // 4 bytes
            public uint PrimitiveCount;// 4 bytes
            public readonly bool IsLeaf => PrimitiveCount > 0;
            public readonly bool IsNode => PrimitiveCount == 0;
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

                _bhv = new Bvh(count * 2 - 1);

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

                    node.Min = Vector3f.Min(node.Min, new Vector3f(leafTri.vertex0));
                    node.Min = Vector3f.Min(node.Min, new Vector3f(leafTri.vertex1));
                    node.Min = Vector3f.Min(node.Min, new Vector3f(leafTri.vertex2));

                    node.Max = Vector3f.Max(node.Max, new Vector3f(leafTri.vertex0));
                    node.Max = Vector3f.Max(node.Max, new Vector3f(leafTri.vertex1));
                    node.Max = Vector3f.Max(node.Max, new Vector3f(leafTri.vertex2));
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
                
                var leftChildIdx = (int)(_usedNodeCount++);
                var rightChildIdx = (int)(_usedNodeCount++);

                _bhv.Nodes[leftChildIdx].NodeOrPrimitiveIndex = node.NodeOrPrimitiveIndex;
                _bhv.Nodes[leftChildIdx].PrimitiveCount = leftCount;

                _bhv.Nodes[rightChildIdx].NodeOrPrimitiveIndex = (uint)i;
                _bhv.Nodes[rightChildIdx].PrimitiveCount = node.PrimitiveCount - leftCount;

                node.NodeOrPrimitiveIndex = (uint)leftChildIdx;
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

        public unsafe class Bvh : IDisposable
        {
            private uint _nodeCount;

            //public Span<BvhNode> NodeSpan;

            private void* _nodesMemmoryHandle = default;

            /// <summary>
            /// Why 32-Byte Alignment is usually bestAVX / AVX2 Vector Alignment: 
            /// Modern CPUs use AVX registers, which are exactly 256 bits (32 bytes) wide. 
            /// Aligning your 32-byte struct to a 32-byte boundary ensures that any 256-bit SIMD instruction 
            /// can load your entire bounding box data structure (Min and Max together) in a single CPU cycle.
            /// 
            /// Zero Padding Waste: Since your struct is exactly 32 bytes, allocating with 32-byte alignment creates 
            /// a perfectly packed contiguous block of memory with absolutely zero memory overhead.
            /// 
            /// Why 64-Byte Alignment is usedCPU Cache Line Optimization: 
            /// Modern CPU cache lines are 64 bytes wide. If you use 64-byte alignment, every even index 
            /// node (Nodes[0], Nodes[2], Nodes[4]) will sit exactly at the beginning of a fresh cache line. 
            /// Preventing Split Cache Line Accesses: Because 32 divides perfectly into 64, aligning to 64 bytes 
            /// implicitly aligns your data to 32 bytes anyway. No individual BvhNode will ever accidentally straddle 
            /// across two different CPU cache lines, preventing a performance penalty known as a "cache line split.
            /// 
            /// Change it to 32 if you want to optimize for pure memory efficiency while retaining full support for 256-bit 
            /// AVX acceleration.Keep it at 64 if your BVH traversal works on pairs of nodes (like wide BVH trees) or if 
            /// you want to aggressively optimize for CPU cache line boundaries.
            /// </summary>
            private const nuint MemmoryAlignment = 32;

            public unsafe Span<BvhNode> Nodes
            {
                get
                {
                    if (_nodesMemmoryHandle == null)
                        return Span<BvhNode>.Empty;

                    return new Span<BvhNode>(_nodesMemmoryHandle, (int)_nodeCount);
                }
            }

            public Bvh(uint nodeCount)
            {
                AllocateMemory(nodeCount);
            }

            private void AllocateMemory(uint nodeCount)
            {
                if (_nodesMemmoryHandle != null)
                {
                    ReleaseMemory(); // Prevent memory leaks if AllocateMemory is called twice
                }

                _nodeCount = nodeCount;
                _nodesMemmoryHandle = NativeMemory.AlignedAlloc((nuint)(nodeCount * sizeof(BvhNode)), MemmoryAlignment);
            }

            private void ReleaseMemory()
            {
                try
                {
                    if (_nodesMemmoryHandle != default)
                    {
                        NativeMemory.AlignedFree(_nodesMemmoryHandle);
                    }
                }
                finally
                {
                    _nodesMemmoryHandle = default;
                }
            }

            public bool Intersection(Ray ray, Scene scene)
            {
                return IntersectionRecursive(ray, scene, 0);
            }

            private bool IntersectionRecursive(Ray ray, Scene scene, uint nodeIdx)
            {
                var node = Nodes[(int)nodeIdx];

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

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects)
                ReleaseMemory();
            }

            ~Bvh()
            {
                // use try/finally block to correctly handle exceptions and ensure that unmanaged resources are released
                // since a desctructors run on their own thread
                // see https://stackoverflow.com/a/4899622
                try
                {
                    Dispose(disposing: false);
                }
                finally
                {
                    _nodesMemmoryHandle = default;
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        public new void Run(RayTraceSettings settings)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"// ------------------------ //");
            Console.WriteLine($"");

            Console.WriteLine("Rendering HowToBuildABvh_Part01_Basics_Step05_AlignedAlloc...");
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

                    buffer?[index++] = pixel;
                }
            }

            if(buffer != null)
                Png.Save(settings.ImageSavePath, settings.ImageWidth, settings.ImageHeight, buffer);
        }
    }
}
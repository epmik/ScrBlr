using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Scrbl.JaccoBikker.Test
{
    public class BvhUnitTest
    {
        private class RayTraceSettings
        {
            public uint TriangleCount { get; init; } = 64;
            public int ImageWidth { get; init; } = 640;
            public int ImageHeight { get; init; } = 640;
            public Vector3f CameraPosition { get; init; } = new Vector3f(0, 0, -18);
        }

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
                            if (intersectionResult.Time < nearest)
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



        [Fact]
        public void CreateSceneTest()
        {
            var setting = new RayTraceSettings();

            var randomGenerator = new RandomGenerator();

            var scene = CreateScene(setting, randomGenerator);
        }

        [Theory]
        [InlineData(64, 640, 640)]
        [InlineData(1024, 1024, 1024)]
        //[InlineData(4096, 1024, 1024)]
        public void CreateAndRenderSceneTest(uint triangleCount, int imageWidth, int imageHeight)
        {
            var setting = new RayTraceSettings
            {
                TriangleCount = triangleCount,
                ImageWidth = imageWidth,
                ImageHeight = imageHeight
            };

            var randomGenerator = new RandomGenerator();

            var scene = CreateScene(setting, randomGenerator);

            RenderScene(scene, setting);
        }

        [Theory]
        [InlineData(64, 640, 640)]
        [InlineData(1024, 1024, 1024)]
        [InlineData(4096, 1024, 1024)]
        public void CreateBvhAndRenderSceneTest(uint triangleCount, int imageWidth, int imageHeight)
        {
            var setting = new RayTraceSettings
            {
                TriangleCount = triangleCount,
                ImageWidth = imageWidth,
                ImageHeight = imageHeight
            };

            var randomGenerator = new RandomGenerator();

            var scene = CreateScene(setting, randomGenerator);

            var bvh = new BvhGenerator().Build(scene);

            RenderScene(scene, bvh, setting);
        }

        [Theory]
        [InlineData(640, 640)]
        [InlineData(1024, 1024)]
        public void CreateBvhFromTriFileRenderSceneTest(int imageWidth, int imageHeight)
        {
            var setting = new RayTraceSettings
            {
                ImageWidth = imageWidth,
                ImageHeight = imageHeight,
                CameraPosition = new Vector3f(-1.5f, -0.2f, -2.5f),
            };

            var randomGenerator = new RandomGenerator();

            var scene = LoadSceneFromTriFile(@"C:\Steven\Atelier\Scrbl\Scrbl.JaccoBikker\.assets\unity.tri");

            var bvh = new BvhGenerator().Build(scene);

            RenderScene(scene, bvh, setting);
        }

        [Theory]
        [InlineData(640, 640, @"C:\Steven\Atelier\Scrbl\Scrbl.JaccoBikker\.output\unity.tri-640x640.png")]
        [InlineData(1024, 1024, @"C:\Steven\Atelier\Scrbl\Scrbl.JaccoBikker\.output\unity.tri-1024x1024.png")]
        public void CreateBvhFromTriFileRenderSceneAndSaveTest(int imageWidth, int imageHeight, string path)
        {
            var setting = new RayTraceSettings
            {
                ImageWidth = imageWidth,
                ImageHeight = imageHeight,
                CameraPosition = new Vector3f(-1.5f, -0.2f, -2.5f),
            };

            var randomGenerator = new RandomGenerator();

            var scene = LoadSceneFromTriFile(@"C:\Steven\Atelier\Scrbl\Scrbl.JaccoBikker\.assets\unity.tri");

            var bvh = new BvhGenerator().Build(scene);

            RenderScene(scene, bvh, setting, path);
        }

        private Scene LoadSceneFromTriFile(string path)
        {
            var scene = new Scene();

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

            return scene;
        }

        private Scene CreateScene(RayTraceSettings settings, IRandomGenerator randomGenerator)
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

                scene.Triangles[i].A = r0 * 9f - new Vector3f(5f);
                scene.Triangles[i].B = scene.Triangles[i].A + r1;
                scene.Triangles[i].C = scene.Triangles[i].A + r2;
            }

            return scene;
        }


        unsafe void RenderScene(Scene scene, Bvh bvh, RayTraceSettings settings, string path)
        {
            void* bufferHandle = default;

            try
            {
                bufferHandle = NativeMemory.AlignedAlloc((nuint)(settings.ImageWidth * settings.ImageHeight * sizeof(Vector3f)), 64);

                var vector3fBuffer = new Span<Vector3f>(bufferHandle, settings.ImageWidth * settings.ImageHeight);

                var byteBuffer = new Span<byte>(bufferHandle, settings.ImageWidth * settings.ImageHeight * sizeof(Vector3f));

                //var index = 0;

                Vector3f p0 = new(-2.5f, 0.8f, -0.5f), p1 = new(-0.5f, 0.8f, -0.5f), p2 = new(-2.5f, -1.2f, -0.5f);
                var ray = new Ray();

                var nearest = float.PositiveInfinity;
                var furthest = float.NegativeInfinity;
                var time = float.PositiveInfinity;
                Color3f pixel;
                float c;
                var index = 0;

                for (int y = 0; y < settings.ImageHeight; y++)
                {
                    for (int x = 0; x < settings.ImageWidth; x++)
                    {
                        Vector3f pixelPos = p0 + (p1 - p0) * ((float)x / (float)settings.ImageWidth) + (p2 - p0) * ((float)y / settings.ImageHeight);

                        ray.Origin = settings.CameraPosition;
                        ray.Direction = Vector3f.Normalize(pixelPos - ray.Origin);
                        ray.Time = float.PositiveInfinity;

                        time = float.PositiveInfinity;

                        pixel.X = 0;
                        pixel.Y = 0;
                        pixel.Z = 0;

                        if (bvh.Intersection(ray, scene, ref time))
                        {
                            nearest = Math.Min(nearest, time);
                            furthest = Math.Max(furthest, time);

                            c = Utility.Remap(time, 1.2f, 3.4f, 1f, 0f, true);

                            pixel.X = c;
                            pixel.Y = c;
                            pixel.Z = c;
                        }

                        vector3fBuffer[index++] = pixel;
                    }
                }

                using (var image = SixLabors.ImageSharp.Image.LoadPixelData<Rgb24>(byteBuffer, settings.ImageWidth, settings.ImageHeight))
                {
                    image.SaveAsPng(path);
                }
            }
            finally
            {
                NativeMemory.AlignedFree(bufferHandle);
            }
        }

        void RenderScene(Scene scene, Bvh bvh, RayTraceSettings settings)
        {
            Vector3f p0 = new(-2.5f, 0.8f, -0.5f), p1 = new(-0.5f, 0.8f, -0.5f), p2 = new(-2.5f, -1.2f, -0.5f);
            var ray = new Ray();

            var nearest = float.PositiveInfinity;
            var furthest = float.NegativeInfinity;
            var time = float.PositiveInfinity;
            Color3f pixel;
            float c;

            for (int y = 0; y < settings.ImageHeight; y++)
            {
                for (int x = 0; x < settings.ImageWidth; x++)
                {
                    Vector3f pixelPos = p0 + (p1 - p0) * ((float)x / (float)settings.ImageWidth) + (p2 - p0) * ((float)y / settings.ImageHeight);

                    ray.Origin = settings.CameraPosition;
                    ray.Direction = Vector3f.Normalize(pixelPos - ray.Origin);
                    ray.Time = float.PositiveInfinity;

                    time = float.PositiveInfinity;

                    if (bvh.Intersection(ray, scene, ref time))
                    {
                        nearest = Math.Min(nearest, time);
                        furthest = Math.Max(furthest, time);

                        c = Utility.Remap(time, 1.2f, 3.4f, 1f, 0f, true);

                        pixel.X = c;
                        pixel.Y = c;
                        pixel.Z = c;
                    }
                }
            }
        }

        private void RenderScene(Scene scene, RayTraceSettings settings)
        {
            //var buffer = new Vector3d[settings.ImageWidth * settings.ImageHeight];

            //var index = 0;

            Vector3f p0 = new(-1, 1, -15), p1 = new(1, 1, -15), p2 = new(-1, -1, -15);

            var ray = new Ray();

            var timeResult = new Intersection.TimeResult();

            Color3f pixel;

            for (int y = 0; y < settings.ImageHeight; y++)
            {
                for (int x = 0; x < settings.ImageWidth; x++)
                {
                    Vector3f pixelPos = p0 + (p1 - p0) * ((float)x / (float)settings.ImageWidth) + (p2 - p0) * ((float)y / settings.ImageHeight);

                    ray.Origin = settings.CameraPosition;
                    ray.Direction = Vector3f.Normalize(pixelPos - ray.Origin);
                    ray.Time = float.PositiveInfinity;

                    for (int i = 0; i < scene.TriangleCount; i++)
                    {
                        if (Intersection.Compute(ray, scene.Triangles[i], ref timeResult))
                        {
                            pixel.X = 1;
                            pixel.Y = 1;
                            pixel.Z = 1;

                            break;
                        }
                    }

                    //buffer[index++] = pixel;
                }
            }

            //Png.Save(settings.ImageSavePath, settings.ImageWidth, settings.ImageHeight, buffer);
        }
    }
}

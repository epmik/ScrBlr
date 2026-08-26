using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scrblr.Rtx
{
    class Chapter300BoundingVolumeHierarchies : Chapter200MotionBlur
    {

        //protected class MinMax
        //{
        //    public Vector3d Min { get; set; } = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        //    public Vector3d Max { get; set; } = new Vector3d(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

        //    public void Clamp(Vector3d point)
        //    {
        //        Min = new Vector3d(Math.Min(Min.X, point.X), Math.Min(Min.Y, point.Y), Math.Min(Min.Z, point.Z));
        //        Max = new Vector3d(Math.Max(Max.X, point.X), Math.Max(Max.Y, point.Y), Math.Max(Max.Z, point.Z));
        //    }

        //    public void Grow(Vector3d point)
        //    {
        //        Min = new Vector3d(Min.X - point.X, Min.Y - point.Y, Min.Z - point.Z);
        //        Max = new Vector3d(Max.X + point.X, Max.Y + point.Y, Max.Z + point.Z);
        //    }
        //}

        public enum SortingMethod
        {
            RaySign,    // Faster: Uses the sign bit of the ray direction to pick the near child
            DualSlab    // Precise: Computes actual intersection distances to sort children
        }

        //protected class BvhNode : Hittable
        //{
        //    Aabb aabb;
        //    Hittable left;
        //    Hittable right;

        //    public BvhNode() 
        //    { 
        //    }

        //    public override bool Hit(Ray ray, Scene scene, double min, double max, out HitRecord rec)
        //    {
        //        rec = new HitRecord();

        //        if (!aabb.Hit(ray, min, max))
        //            return false;

        //        bool hit_left = left.Hit(ray, scene, min, max, out rec);
        //        bool hit_right = right.Hit(ray, scene, min, hit_left ? rec.T : max, out rec);

        //        return hit_left || hit_right;
        //    }
        //}

        protected struct BvhNode
        {
            public Aabb Bounds;
            public uint LeftFirst, SphereCount;
            public bool IsLeaf => SphereCount > 0;
        }

        protected class Bvh
        {
            public BvhNode[] Nodes;
            public uint[] SphereIndices;
            private Sphere[] _spheres;
            private uint _nodesUsed = 1;

            public void Generate(Sphere[] spheres)
            {
                _spheres = spheres;
                
                SphereIndices = new uint[spheres.Length];
                
                for (int i = 0; i < spheres.Length; i++) 
                    SphereIndices[i] = (uint)i;
                
                Nodes = new BvhNode[spheres.Length * 2];
                Nodes[0].LeftFirst = 0;
                Nodes[0].SphereCount = (uint)spheres.Length; // <-- the missing piece

                Refit(0, (uint)spheres.Length);

                Subdivide(0);
            }

            private void Subdivide(uint nodeIdx)
            {
                ref BvhNode node = ref Nodes[nodeIdx];
                if (node.SphereCount <= 2) return;

                // Split along longest axis
                Vector3d size = node.Bounds.Max - node.Bounds.Min;
                int axis = size.X > size.Y && size.X > size.Z ? 0 : (size.Y > size.Z ? 1 : 2);
                double split = node.Bounds.Min[axis] + size[axis] * 0.5;

                // Partition spheres
                int i = (int)node.LeftFirst, j = i + (int)node.SphereCount - 1;
                while (i <= j)
                {
                    if (GetCenter(SphereIndices[i], axis) < split) i++;
                    else { uint t = SphereIndices[i]; SphereIndices[i] = SphereIndices[j]; SphereIndices[j] = t; j--; }
                }
                uint leftCount = (uint)i - node.LeftFirst;
                if (leftCount == 0 || leftCount == node.SphereCount) return;

                // Create children
                uint leftChild = _nodesUsed++, rightChild = _nodesUsed++;
                Nodes[leftChild].LeftFirst = node.LeftFirst;
                Nodes[leftChild].SphereCount = leftCount;
                Nodes[rightChild].LeftFirst = (uint)i;
                Nodes[rightChild].SphereCount = node.SphereCount - leftCount;
                node.LeftFirst = leftChild; 
                node.SphereCount = 0;

                Refit(leftChild, leftCount); 
                Refit(rightChild, node.SphereCount);

                Subdivide(leftChild); Subdivide(rightChild);
            }

            private void Refit(uint nodeIdx, uint count)
            {
                ref BvhNode node = ref Nodes[nodeIdx];
                
                node.Bounds = new Aabb();

                for (int i = 0; i < count; i++)
                {
                    node.Bounds.Grow(_spheres[SphereIndices[node.LeftFirst + i]]);
                }
            }

            private double GetCenter(uint idx, int axis) => axis == 0 ? _spheres[idx].Center.X : (axis == 1 ? _spheres[idx].Center.Y : _spheres[idx].Center.Z);
        }

        public override async Task Main(string path)
        {
            Scene scene;

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"// ------------------------ //");
            Console.WriteLine($"");

            Console.WriteLine("Rendering 300 Bounding Volume Hierarchies...");
            Console.WriteLine("Setup...");

            _randomGenerator = new RandomGeneratorThreadSafe(1024);

            CreateScene(new SceneSettings { ImageWidth = 1600, ShutterDuration = 0.25, AddSmallDynamicSpheres = true, AddSmallStaticSpheres = true, AddLargeSpheres = true }, out scene);

            var bhv = new Bvh();

            bhv.Generate(scene.Objects);

            stopwatch.Stop();

            Console.WriteLine($"Setup duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine("Rendering...");

            var tracker = new ProgressTracker();

            Task progressTask = StartProgressReportingLoop(tracker);

            stopwatch.Restart();

            var renderTask = RenderAsync(scene, path, tracker);

            await renderTask;

            stopwatch.Stop();

            Console.WriteLine("Rendering finished...");

            Console.WriteLine($"Render duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine($"");
            Console.WriteLine($"// ------------------------ //");
        }
    }
}

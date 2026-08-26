using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scrblr.Rtx
{
    class Chapter300BoundingVolumeHierarchies : Chapter200MotionBlur
    {

        protected class MinMax
        {
            public Vector3d Min { get; set; } = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            public Vector3d Max { get; set; } = new Vector3d(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

            public void Clamp(Vector3d point)
            {
                Min = new Vector3d(Math.Min(Min.X, point.X), Math.Min(Min.Y, point.Y), Math.Min(Min.Z, point.Z));
                Max = new Vector3d(Math.Max(Max.X, point.X), Math.Max(Max.Y, point.Y), Math.Max(Max.Z, point.Z));
            }

            public void Grow(Vector3d point)
            {
                Min = new Vector3d(Min.X - point.X, Min.Y - point.Y, Min.Z - point.Z);
                Max = new Vector3d(Max.X + point.X, Max.Y + point.Y, Max.Z + point.Z);
            }
        }

        protected class Aabb
        {
            public Vector3d Min { get; set; } = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            public Vector3d Max { get; set; } = new Vector3d(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

            public Aabb(Vector3d min, Vector3d max)
            {
                Min = new Vector3d(Math.Min(Min.X, min.X), Math.Min(Min.Y, min.Y), Math.Min(Min.Z, min.Z));
                Max = new Vector3d(Math.Max(Max.X, max.X), Math.Max(Max.Y, max.Y), Math.Max(Max.Z, max.Z));
            }

            public void Clamp(Vector3d point)
            {
                Min = new Vector3d(Math.Min(Min.X, point.X), Math.Min(Min.Y, point.Y), Math.Min(Min.Z, point.Z));
                Max = new Vector3d(Math.Max(Max.X, point.X), Math.Max(Max.Y, point.Y), Math.Max(Max.Z, point.Z));
            }

            public void Grow(Vector3d point)
            {
                Min = new Vector3d(Min.X - point.X, Min.Y - point.Y, Min.Z - point.Z);
                Max = new Vector3d(Max.X + point.X, Max.Y + point.Y, Max.Z + point.Z);
            }

            public bool Hit(Ray ray, double min, double max)
            {
                for (int axis = 0; axis < 3; axis++)
                {
                    double invD = 1.0 / ray.Direction[axis];
                    double t0 = (Min[axis] - ray.Origin[axis]) * invD;
                    double t1 = (Max[axis] - ray.Origin[axis]) * invD;
                    
                    if (invD < 0.0)
                    {
                        (t0, t1) = (t1, t0);
                    }
                    
                    min = Math.Max(t0, min);
                    max = Math.Min(t1, max);
                    
                    if (max <= min)
                    {
                        return false;
                    }
                }
                return true;
            }

            public static Aabb FromSphere(Sphere sphere)
            {
                return new Aabb(
                    new Vector3d(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius),
                    new Vector3d(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius));
            }
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

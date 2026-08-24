using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scrblr.Rtx
{
    class Chapter200MotionBlur : Chapter141WithProgressThreaded
    {

        public override async Task Main(string path)
        {
            HittableList world;
            Camera cam;

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"// ------------------------ //");
            Console.WriteLine($"");

            Console.WriteLine("Rendering 200 Motion Blur...");
            Console.WriteLine("Setup...");

            _randomGenerator = new RandomGeneratorThreadSafe(1024);

            CreateScene(new SceneSettings { ShutterTime = 0.25, AddSmallDynamicSpheres = true, AddSmallStaticSpheres = false, AddLargeSpheres = false }, out world, out cam);

            stopwatch.Stop();

            Console.WriteLine($"Setup duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine("Rendering...");

            var tracker = new ProgressTracker();

            Task progressTask = StartProgressReportingLoop(tracker);

            stopwatch.Restart();

            var renderTask = RenderAsync(cam, world, path, tracker);

            await renderTask;

            stopwatch.Stop();

            Console.WriteLine("Rendering finished...");

            Console.WriteLine($"Render duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine($"");
            Console.WriteLine($"// ------------------------ //");
        }
    }
}

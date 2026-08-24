using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scrblr.Rtx
{
    class Chapter141WithProgressThreaded : Chapter141WithProgress
    {
        protected async static Task RenderAsync(Camera camera, HittableList world, string path, ProgressTracker tracker)
        {
            await Task.Run(() =>
            {
                camera.Initialize();

                tracker.TotalSteps = camera.image_width * camera.image_height;

                var buffer = new Vector3d[camera.image_width * camera.image_height];

                Parallel.For(0, camera.image_height, j =>
                {
                    for (int i = 0; i < camera.image_width; i++)
                    {
                        var pixel_color = new Color(0, 0, 0);

                        for (int sample = 0; sample < camera.samples_per_pixel; sample++)
                        {
                            var r = camera.get_ray(i, j);
                            pixel_color += camera.RayColor(r, camera.max_depth, world);
                        }

                        pixel_color *= camera.pixel_samples_scale;

                        if (!camera.OutputLinearColorSpace)
                        {
                            pixel_color = new Color(
                                linear_to_gamma(pixel_color.X),
                                linear_to_gamma(pixel_color.Y),
                                linear_to_gamma(pixel_color.Z));
                        }

                        buffer[j * camera.image_width + i] = pixel_color;

                        tracker.Increment();
                    }
                });

                Png.Save(path, camera.image_width, camera.image_height, buffer);

            });
        }

        public virtual async Task Main(string path)
        {
            HittableList world;
            Camera cam;

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"// ------------------------ //");
            Console.WriteLine($"");

            Console.WriteLine("Rendering 141 A Final Render - With Progress and Threaded...");
            Console.WriteLine("Setup...");

            _randomGenerator = new RandomGeneratorThreadSafe(1024);

            CreateScene(new SceneSettings(), out world, out cam);

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

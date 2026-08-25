using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scrblr.Rtx
{
    class Chapter141WithProgressThreaded : Chapter141WithProgress
    {
        protected async static Task RenderAsync(Scene scene, string path, ProgressTracker tracker)
        {
            await Task.Run(() =>
            {
                scene.Camera.Initialize();

                tracker.TotalSteps = scene.Camera.image_width * scene.Camera.image_height;

                var buffer = new Vector3d[scene.Camera.image_width * scene.Camera.image_height];

                Parallel.For(0, scene.Camera.image_height, j =>
                {
                    for (int i = 0; i < scene.Camera.image_width; i++)
                    {
                        var pixel_color = new Color(0, 0, 0);

                        for (int sample = 0; sample < scene.Camera.samples_per_pixel; sample++)
                        {
                            var r = scene.Camera.get_ray(scene, i, j);
                            pixel_color += scene.Camera.RayColor(r, scene.Camera.max_depth, scene);
                        }

                        pixel_color *= scene.Camera.pixel_samples_scale;

                        if (!scene.Camera.OutputLinearColorSpace)
                        {
                            pixel_color = new Color(
                                linear_to_gamma(pixel_color.X),
                                linear_to_gamma(pixel_color.Y),
                                linear_to_gamma(pixel_color.Z));
                        }

                        buffer[j * scene.Camera.image_width + i] = pixel_color;

                        tracker.Increment();
                    }
                });

                Png.Save(path, scene.Camera.image_width, scene.Camera.image_height, buffer);

            });
        }

        public virtual async Task Main(string path)
        {
            Scene scene;
            Camera cam;

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"// ------------------------ //");
            Console.WriteLine($"");

            Console.WriteLine("Rendering 141 A Final Render - With Progress and Threaded...");
            Console.WriteLine("Setup...");

            _randomGenerator = new RandomGeneratorThreadSafe(1024);

            CreateScene(new SceneSettings(), out scene);

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

using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scrblr.Rtx
{
    class Chapter141Threaded : Chapter141WithProgress
    {

        private async static Task RenderAsync(Camera camera, HittableList world, string path, ProgressTracker tracker)
        {

            camera.Initialize();

            tracker.TotalSteps = camera.image_width * camera.image_height;

            var buffer = new Vector3d[camera.image_width * camera.image_height];

            await Task.Run(() =>
            {

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

        public async new Task Main(string path)
        {
            HittableList world;
            Camera cam;

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine("Rendering 141 A Final Render...");
            Console.WriteLine("Setup...");

            CreateScene(out world, out cam);

            stopwatch.Stop();

            Console.WriteLine($"Setup duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine("Rendering...");

            var tracker = new ProgressTracker();

            stopwatch.Restart();

            var renderTask = RenderAsync(cam, world, path + "-" + cam.samples_per_pixel + "-samples.png", tracker);

            var progressTask = StartProgressReportingLoop(tracker);

            await renderTask;

            stopwatch.Stop();

            Console.WriteLine("Rendering finished...");

            Console.WriteLine($"Render duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine($"Press a key to continue");

            Console.Read();
        }
    }
}

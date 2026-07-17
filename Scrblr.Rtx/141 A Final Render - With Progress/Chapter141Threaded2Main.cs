using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scrblr.Rtx
{
    class Chapter141Threaded2 : Chapter141WithProgress
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

        public async Task Main(string path)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine("Rendering 141 A Final Render...");
            Console.WriteLine("Setup...");

            var random = new Random(42);

            //Utility.RandomSeed(42);  // Seed the random number generator for reproducibility

            //HittableList world;
            //Camera cam;

            //CreateScene(out world, out cam);

            HittableList world = new HittableList();

            var ground_material = new Lambertian(new Color(0.5, 0.5, 0.5));
            world.Add(new Sphere(new Point3(0, -1000, 0), 1000, ground_material));

            for (int a = -11; a < 11; a++)
            {
                for (int b = -11; b < 11; b++)
                {
                    var choose_mat = random.NextDouble();
                    var center = new Point3(a +0.9 * random.NextDouble(), 0.2, b + 0.9 * random.NextDouble());

                    if ((center - new Point3(4, 0.2, 0)).Length() > 0.9)
                    {
                        if (choose_mat < 0.8)
                        {
                            // diffuse
                            var albedo = Color.random() * Color.random();
                            world.Add(new Sphere(center, 0.2, new Lambertian(albedo)));
                        }
                        else if (choose_mat < 0.95)
                        {
                            // metal
                            var albedo = Color.random(0.5, 1);
                            var fuzz = random.NextDouble() * 0.5;
                            world.Add(new Sphere(center, 0.2, new Metal(albedo, fuzz)));
                        }
                        else
                        {
                            // glass
                            world.Add(new Sphere(center, 0.2, new Dielectric(1.5)));
                        }
                    }
                }
            }

            var material1 = new Dielectric(1.5);
            world.Add(new Sphere(new Vector3d(0, 1, 0), 1.0, material1));

            var material2 = new Lambertian(new Color(0.4, 0.2, 0.1));
            world.Add(new Sphere(new Vector3d(-4, 1, 0), 1.0, material2));

            var material3 = new Metal(new Color(0.7, 0.6, 0.5), 0.0);
            world.Add(new Sphere(new Vector3d(4, 1, 0), 1.0, material3));


            var cam = new Camera();

            cam.aspect_ratio = 16.0 / 9.0;
            cam.image_width = 400;
            cam.max_depth = 50;

            cam.vfov = 20;
            cam.lookfrom = new Point3(13, 2, 3);
            cam.lookat = new Point3(0, 0, 0);
            cam.vup = new Vector3d(0, 1, 0);

            cam.defocus_angle = 0.6;
            cam.focus_dist = 10.0;

            stopwatch.Stop();

            Console.WriteLine($"Setup duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")} ms");

            Console.WriteLine("Rendering...");

            var tracker = new ProgressTracker();

            using var cts = new CancellationTokenSource();

            stopwatch.Restart();

            cam.samples_per_pixel = 10;

            var renderTask = RenderAsync(cam, world, path + "-" + cam.samples_per_pixel + "-samples.png", tracker);

            Task progressTask = StartProgressReportingLoop(tracker);

            await renderTask;

            cts.Cancel();

            try
            {
                await progressTask;
            }
            catch (OperationCanceledException) 
            { 
                /* Expected cancellation */ 
            }

            stopwatch.Stop();

            Console.WriteLine("Rendering finished...");

            Console.WriteLine($"Render duration: {stopwatch.Elapsed.TotalMilliseconds} ms");

            Console.WriteLine($"Press a key to continue");

            Console.Read();
        }
    }
}

using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scrblr.Rtx
{
    class Chapter141WithProgress : Chapter141
    {
        public interface IProgressTracker
        {
            int TotalSteps { get; set; }
            int CurrentStep { get; }
            double PercentageCompleted { get; }
            void Increment();
        }

        public class ProgressTracker : IProgressTracker
        {
            private int _currentStep;

            public int TotalSteps { get; set; }
            public int CurrentStep => _currentStep;
            public double PercentageCompleted => TotalSteps == 0 ? 0.0 : (double)_currentStep / TotalSteps * 100.0;

            public ProgressTracker()
                : this(1)
            {
            }

            public ProgressTracker(int totalSteps)
            {
                TotalSteps = totalSteps;
                _currentStep = 0;
            }

            public void Increment()
            {
                Interlocked.Increment(ref _currentStep);
            }
        }


        private static async Task StartProgressReportingLoop(IProgressTracker tracker)
        {
            if (tracker == null) return;

            int lastReportedStep = -1;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Keep looping until Main tells us to stop via the token
                while (tracker.CurrentStep < tracker.TotalSteps)
                {
                    int currentStep = tracker.CurrentStep;

                    if (currentStep > lastReportedStep)
                    {
                        double totalSeconds = stopwatch.Elapsed.TotalSeconds;
                        double pixelsPerSecond = totalSeconds > 0 ? currentStep / totalSeconds : 0;

                        Console.Write($"\rProgress: {tracker.PercentageCompleted:F2}% ({currentStep}/{tracker.TotalSteps} px) | Speed: {pixelsPerSecond:F0} px/s    ");

                        lastReportedStep = currentStep;
                    }

                    // Pass the token to the delay so it wakes up immediately upon cancellation
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
            catch (OperationCanceledException)
            {
                int finalStep = tracker.CurrentStep;
                if (finalStep > lastReportedStep)
                {
                    double totalSeconds = stopwatch.Elapsed.TotalSeconds;
                    double pixelsPerSecond = totalSeconds > 0 ? finalStep / totalSeconds : 0;

                    Console.Write($"\rFinal Progress: {tracker.PercentageCompleted:F2}% ({finalStep}/{tracker.TotalSteps} px) | Speed: {pixelsPerSecond:F0} px/s    ");
                }
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine();
            }
        }

        private static void Render(Camera camera, HittableList world, string path, ProgressTracker tracker)
        {
            camera.Initialize();

            tracker.TotalSteps = camera.image_width * camera.image_height;

            var buffer = new Vector3d[camera.image_width * camera.image_height];

            var index = 0;

            for (int j = 0; j < camera.image_height; j++)
            {
                for (int i = 0; i < camera.image_width; i++)
                {
                    //color pixel_color(0,0,0);
                    //for (int sample = 0; sample < samples_per_pixel; sample++)
                    //{
                    //    ray r = get_ray(i, j);
                    //    pixel_color += ray_color(r, world);
                    //}
                    //write_color(std::cout, pixel_samples_scale * pixel_color);

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

                    buffer[index++] = pixel_color;

                    tracker.Increment();
                }
            }

            Png.Save(path, camera.image_width, camera.image_height, buffer);

        }

        public override void Main(string path)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine("Rendering 141 A Final Render...");
            Console.WriteLine("Setup...");

            HittableList world = new HittableList();

            var ground_material = new Lambertian(new Color(0.5, 0.5, 0.5));
            world.Add(new Sphere(new Point3(0, -1000, 0), 1000, ground_material));

            for (int a = -11; a < 11; a++)
            {
                for (int b = -11; b < 11; b++)
                {
                    var choose_mat = Utility.RandomDouble();
                    var center = new Point3(a + 0.9 * Utility.RandomDouble(), 0.2, b + 0.9 * Utility.RandomDouble());

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
                            var fuzz = Utility.RandomDouble(0, 0.5);
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
            cam.image_width = 600;
            cam.samples_per_pixel = 10;
            cam.max_depth = 50;

            cam.vfov = 20;
            cam.lookfrom = new Point3(13, 2, 3);
            cam.lookat = new Point3(0, 0, 0);
            cam.vup = new Vector3d(0, 1, 0);

            cam.defocus_angle = 0.6;
            cam.focus_dist = 10.0;

            //cam.Render(world, path);

            //cam.samples_per_pixel = 100;

            //cam.Render(world, path + "-100-samples.png");

            cam.samples_per_pixel = 20;

            stopwatch.Stop();

            Console.WriteLine($"Setup duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")} ms");

            Console.WriteLine("Rendering...");

            var tracker = new ProgressTracker();

            Task progressTask = StartProgressReportingLoop(tracker);

            stopwatch.Restart();

            Render(cam, world, path + "-" + cam.samples_per_pixel + "-samples.png", tracker);

            stopwatch.Stop();

            Console.WriteLine("Rendering finished...");

            Console.WriteLine($"Render duration: {stopwatch.Elapsed.TotalMilliseconds} ms");

            Console.WriteLine($"Press a key to continue");

            Console.Read();
        }
    }
}

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


        protected static async Task StartProgressReportingLoop(IProgressTracker tracker)
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

            Task progressTask = StartProgressReportingLoop(tracker);

            stopwatch.Restart();

            Render(cam, world, path + "-" + cam.samples_per_pixel + "-samples.png", tracker);

            stopwatch.Stop();

            Console.WriteLine("Rendering finished...");

            Console.WriteLine($"Render duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine($"Press a key to continue");

            Console.Read();
        }
    }
}

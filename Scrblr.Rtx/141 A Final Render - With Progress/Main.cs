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

        private static void Render(Scene scene, string path, ProgressTracker tracker)
        {
            scene.Camera.Initialize();

            tracker.TotalSteps = scene.Camera.image_width * scene.Camera.image_height;

            var buffer = new Vector3d[scene.Camera.image_width * scene.Camera.image_height];

            var index = 0;

            for (int j = 0; j < scene.Camera.image_height; j++)
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

                    buffer[index++] = pixel_color;

                    tracker.Increment();
                }
            }

            Png.Save(path, scene.Camera.image_width, scene.Camera.image_height, buffer);
        }

        public override void Main(string path)
        {
            Scene world;

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"// ------------------------ //");
            Console.WriteLine($"");

            Console.WriteLine("Rendering 141 A Final Render - With Progress...");
            Console.WriteLine("Setup...");
            
            _randomGenerator = new RandomGenerator(1024);

            CreateScene(new SceneSettings(), out world);

            stopwatch.Stop();

            Console.WriteLine($"Setup duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine("Rendering...");

            var tracker = new ProgressTracker();

            Task progressTask = StartProgressReportingLoop(tracker);

            stopwatch.Restart();

            Render(world, path, tracker);

            stopwatch.Stop();

            Console.WriteLine("Rendering finished...");

            Console.WriteLine($"Render duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine($"");
            Console.WriteLine($"// ------------------------ //");
        }
    }
}

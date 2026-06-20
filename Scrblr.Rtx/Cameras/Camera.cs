using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorProfiles;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections;
using System.Drawing;
using System.Reflection;

namespace Scrblr.Rtx
{
    // C++: class hit_record
    // Since hit_record is passed as a mutable reference parameter to track hit details, 
    // keeping it as a class or a mutable ref struct works. A simple class mimics C++ behavior cleanly.
    public class Camera
    {
        public double aspectRatio = 1.0;
        public int imageWidth = 100;
        public int samples_per_pixel = 10;   // Count of random samples for each pixel
        public int max_depth = 10;   // Maximum number of ray bounces into sceneint max_depth = 10;   // Maximum number of ray bounces into scene

        public double vfov = 90;  // Vertical view angle (field of view)

        public Vector3d lookfrom = new Vector3d(0, 0, 0);   // Point camera is looking from
        public Vector3d lookat = new Vector3d(0, 0, -1);  // Point camera is looking at
        public Vector3d vup = new Vector3d(0, 1, 0);     // Camera-relative "up" direction

        int imageHeight = 100;      // Rendered image height
        double pixel_samples_scale; // Color scale factor for a sum of pixel samples
        Vector3d center = new Vector3d(0, 0, 0);
        Vector3d pixel00Loc;    // Location of pixel 0, 0
        Vector3d pixelDeltaU;  // Offset to pixel to the right
        Vector3d pixelDeltaV;  // Offset to pixel below

        Vector3d u, v, w;              // Camera frame basis vectors

        public bool UseLambertianReflection = true;

        // Linear space handles light with physically accurate math (matching how light behaves in the real world), while Gamma space
        // compresses brightness values to match how the human eye perceives light and how traditional monitors display it.
        // Physics
        // Linear Space: Accurately models how light blends, falls off, and bounces in the real world.
        // Gamma (sRGB) Space: Physically inaccurate; light addition or blending results in unnaturally bright or washed-out images.
        // Human Perception
        // Linear Space: Treats all mathematical values equally, which conflicts with our eyes (we are much more sensitive to dark tones than bright ones).
        // Gamma (sRGB) Space: Compresses digital brightness values (using a power curve, usually ~2.2) to match human vision perfectly.
        // Use Case
        // Linear Space: Ideal for 3D rendering, shading calculations, and compositing (where math must equal real-world light).
        // Gamma (sRGB) Space: Ideal for storing, displaying, and transmitting images on screens (saves file size and prevents banding).
        public bool OutputLinearColorSpace = false;

        public void Render(HittableList world, string path)
        {
            Initialize();

            //var ppmWriter = new PpmWriter(path + ".ppm.ppm", imageWidth, imageHeight);

            //ppmWriter.OutputLinearColorSpace = OutputLinearColorSpace;

            var buffer = new byte[imageWidth * imageHeight * 3];

            var index = 0;

            Vector3d pixelColor;

            for (int j = 0; j < imageHeight; j++)
            {
                for (int i = 0; i < imageWidth; i++)
                {
                    pixelColor = new Vector3d();

                    for (int sample = 0; sample < samples_per_pixel; sample++)
                    {
                        var r = GetRay(i, j);

                        pixelColor += RayColor(r, max_depth, world);
                    }

                    pixelColor *= pixel_samples_scale;

                    //ppmWriter.Write(pixelColor);

                    if (!OutputLinearColorSpace)
                    {
                        pixelColor.X = linear_to_gamma(pixelColor.X);
                        pixelColor.Y = linear_to_gamma(pixelColor.Y);
                        pixelColor.Z = linear_to_gamma(pixelColor.Z);
                    }

                    buffer[index++] = (byte)(255 * Math.Clamp(pixelColor.X, 0.0, 1.0));
                    buffer[index++] = (byte)(255 * Math.Clamp(pixelColor.Y, 0.0, 1.0));
                    buffer[index++] = (byte)(255 * Math.Clamp(pixelColor.Z, 0.0, 1.0));
                }
            }

            SaveAsPng(path, imageWidth, imageHeight, buffer);
        }

        private static double linear_to_gamma(double linear_component)
        {
            if (linear_component > 0)
                return Math.Sqrt(linear_component);

            return 0;
        }

        Ray GetRay(int i, int j) 
        {
            // Construct a camera ray originating from the origin and directed at randomly sampled
            // point around the pixel location i, j.

            var offset = SampleSquare();
            var pixel_sample = pixel00Loc
                          + ((i + offset.X) * pixelDeltaU)
                          + ((j + offset.Y) * pixelDeltaV);

            var ray_origin = center;
            var ray_direction = pixel_sample - ray_origin;

            return new Ray(ray_origin, ray_direction);
        }

        Vector3d SampleSquare()
        {
            // Returns the vector to a random point in the [-.5,-.5]-[+.5,+.5] unit square.
            return new Vector3d(Utility.RandomDouble() - 0.5, Utility.RandomDouble() - 0.5, 0.0);
        }

        private void Initialize()
        {
            imageHeight = (int)(imageWidth / aspectRatio);
            imageHeight = (imageHeight < 1) ? 1 : imageHeight;

            pixel_samples_scale = 1.0 / samples_per_pixel;

            center = lookfrom;

            // Camera Settings
            double focal_length = (lookfrom - lookat).Length();
            var theta = Utility.ToRadians(vfov);
            var h = Math.Tan(theta / 2);
            var viewport_height = 2 * h * focal_length; 
            double viewportWidth = viewport_height * ((double)imageWidth / (double)imageHeight);

            // Calculate the u,v,w unit basis vectors for the camera coordinate frame.
            w = Vector3d.UnitVector(lookfrom - lookat);
            u = Vector3d.UnitVector(Vector3d.Cross(vup, w));
            v = Vector3d.Cross(w, u);

            // Calculate the vectors across the horizontal and down the vertical viewport edges.
            Vector3d viewport_u = viewportWidth * u;    // Vector across viewport horizontal edge
            Vector3d viewport_v = viewport_height * -v;  // Vector down viewport vertical edge

            // Calculate the horizontal and vertical delta vectors from pixel to pixel.
            pixelDeltaU = viewport_u / imageWidth;
            pixelDeltaV = viewport_v / imageHeight;

            // Calculate the location of the upper left pixel.
            Vector3d viewport_upper_left = center - (focal_length * w) - viewport_u / 2 - viewport_v / 2;
            pixel00Loc = viewport_upper_left + 0.5 * (pixelDeltaU + pixelDeltaV);
        }

        private Vector3d RayColor(Ray r, int depth, HittableList world)
        {
            // If we've exceeded the ray bounce limit, no more light is gathered.
            if (depth <= 0)
                return new Vector3d(0, 0, 0);

            HitRecord rec = new HitRecord();

            if (world.Hit(r, new Interval(0.001, Utility.Infinity), rec))
            {
                Ray scattered;
                Vector3d attenuation;
                if (rec.mat.Scatter(r, rec, out attenuation, out scattered))
                    return attenuation * RayColor(scattered, depth - 1, world);
                
                return new Vector3d(0, 0, 0);
            }

            Vector3d unit_direction = Vector3d.UnitVector(r.Direction);
            var a = 0.5 * (unit_direction.Y + 1.0);

            return (1.0 - a) * new Vector3d(1.0, 1.0, 1.0) + a * new Vector3d(0.5, 0.7, 1.0);
        }

        private static void SaveAsPng(string path, int width, int height, byte[] rgb)
        {
            using(var image = SixLabors.ImageSharp.Image.LoadPixelData<Rgb24>(rgb, width, height))
            {
                image.SaveAsPng(path);
            }

            //var ppmWriter = new PpmWriter(path + ".ppm", width, height);

            //for(var i = 0; i < rgb.Length; i += 3)
            //{
            //    ppmWriter.Write(rgb[i], rgb[i + 1], rgb[i + 2]);
            //}

        }
    }
}

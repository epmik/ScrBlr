using System.Drawing;

namespace Scrblr.Rtx
{
    // C++: class hit_record
    // Since hit_record is passed as a mutable reference parameter to track hit details, 
    // keeping it as a class or a mutable ref struct works. A simple class mimics C++ behavior cleanly.
    public class CameraMultiSample
    {
        public double aspectRatio = 1.0;
        public int imageWidth = 100;
        public int samples_per_pixel = 10;   // Count of random samples for each pixel

        int imageHeight = 100;      // Rendered image height
        double pixel_samples_scale; // Color scale factor for a sum of pixel samples
        Vector3d cameraCenter = new Vector3d(0, 0, 0);
        Vector3d pixel00Loc;    // Location of pixel 0, 0
        Vector3d pixelDeltaU;  // Offset to pixel to the right
        Vector3d pixelDeltaV;  // Offset to pixel below

        public void Render(HittableList world, string path)
        {
            Initialize();

            var ppmWriter = new PpmWriter(path, imageWidth, imageHeight);

            for (int j = 0; j < imageHeight; j++)
            {
                for (int i = 0; i < imageWidth; i++)
                {
                    Vector3d pixelColor = new Vector3d(0,0,0);

                    for (int sample = 0; sample < samples_per_pixel; sample++)
                    {
                        var r = GetRay(i, j);

                        pixelColor += RayColor(r, world);
                    }

                    ppmWriter.Write(pixel_samples_scale * pixelColor);
                }
            }

        }

        Ray GetRay(int i, int j) 
        {
            // Construct a camera ray originating from the origin and directed at randomly sampled
            // point around the pixel location i, j.

            var offset = SampleSquare();
            var pixel_sample = pixel00Loc
                          + ((i + offset.X) * pixelDeltaU)
                          + ((j + offset.Y) * pixelDeltaV);

            var ray_origin = cameraCenter;
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

            cameraCenter = new Vector3d(0, 0, 0);

            // Camera Settings
            double focalLength = 1.0;
            double viewportHeight = 2.0;
            double viewportWidth = viewportHeight * ((double)imageWidth / (double)imageHeight);

            // Calculate the vectors across the horizontal and down the vertical viewport edges.
            Vector3d viewportU = new Vector3d(viewportWidth, 0, 0);
            Vector3d viewportV = new Vector3d(0, -viewportHeight, 0);

            // Calculate the horizontal and vertical delta vectors from pixel to pixel.
            pixelDeltaU = viewportU / imageWidth;
            pixelDeltaV = viewportV / imageHeight;

            // Calculate the location of the upper left pixel.
            Vector3d viewportUpperLeft = cameraCenter - new Vector3d(0, 0, focalLength) - viewportU / 2 - viewportV / 2;
            pixel00Loc = viewportUpperLeft + 0.5 * (pixelDeltaU + pixelDeltaV);
        }

        private Vector3d RayColor(Ray r, HittableList world)
        {
            HitRecord rec = new HitRecord();

            if (world.Hit(r, new Interval(0, Utility.Infinity), rec))
            {
                return 0.5 * (rec.Normal + new Vector3d(1, 1, 1));
            }

            Vector3d unit_direction = Vector3d.UnitVector(r.Direction);
            var a = 0.5 * (unit_direction.Y + 1.0);

            return (1.0 - a) * new Vector3d(1.0, 1.0, 1.0) + a * new Vector3d(0.5, 0.7, 1.0);
        }
    }
}

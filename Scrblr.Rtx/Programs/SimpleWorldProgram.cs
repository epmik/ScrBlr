namespace Scrblr.Rtx
{
    using System;

    class SimpleWorldProgram
    {
        static Vector3d RayColorWithWorld(Ray r, HittableList world)
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

        public void Main()
        {
            // Image Settings
            double aspectRatio = 16.0 / 9.0;
            int imageWidth = 400;

            // Calculate the image height, and ensure that it's at least 1.
            int imageHeight = (int)(imageWidth / aspectRatio);
            imageHeight = (imageHeight < 1) ? 1 : imageHeight;

            // World
            HittableList world = new HittableList();

            world.Add(new SphereNoMaterial(new Vector3d(0, 0, -1), 0.5));
            world.Add(new SphereNoMaterial(new Vector3d(0, -100.5, -1), 100));

            // Camera Settings
            double focalLength = 1.0;
            double viewportHeight = 2.0;
            double viewportWidth = viewportHeight * ((double)imageWidth / imageHeight);
            Vector3d cameraCenter = new Vector3d(0, 0, 0);

            // Calculate the vectors across the horizontal and down the vertical viewport edges.
            Vector3d viewportU = new Vector3d(viewportWidth, 0, 0);
            Vector3d viewportV = new Vector3d(0, -viewportHeight, 0);

            // Calculate the horizontal and vertical delta vectors from pixel to pixel.
            Vector3d pixelDeltaU = viewportU / imageWidth;
            Vector3d pixelDeltaV = viewportV / imageHeight;

            // Calculate the location of the upper left pixel.
            Vector3d viewportUpperLeft = cameraCenter - new Vector3d(0, 0, focalLength) - viewportU / 2 - viewportV / 2;
            Vector3d pixel00Loc = viewportUpperLeft + 0.5 * (pixelDeltaU + pixelDeltaV);

            var ppmWriter = new PpmWriter(".output/RayColorWithWorld.ppm", imageWidth, imageHeight);

            for (int j = 0; j < imageHeight; j++)
            {
                for (int i = 0; i < imageWidth; i++)
                {
                    Vector3d pixelCenter = pixel00Loc + (i * pixelDeltaU) + (j * pixelDeltaV);
                    Vector3d rayDirection = pixelCenter - cameraCenter;
                    Ray r = new Ray(cameraCenter, rayDirection);

                    Vector3d pixelColor = RayColorWithWorld(r, world);

                    ppmWriter.Write(pixelColor);
                }
            }
        }
    }
}

namespace Scrblr.Rtx
{
    class Chapter060
    {
        struct Ray
        {
            private Vector3d _orig;
            private Vector3d _dir;

            // Parameterless constructor (Defaults to 0,0,0 vectors)
            public Ray()
            {
                _orig = new Vector3d();
                _dir = new Vector3d();
            }

            // Main constructor
            public Ray(Vector3d origin, Vector3d direction)
            {
                _orig = origin;
                _dir = direction;
            }

            // Getters matching your original origin() and direction()
            public readonly Vector3d Origin => _orig;
            public readonly Vector3d Direction => _dir;

            // Linear interpolation along the ray: orig + t * dir
            public readonly Vector3d At(double t)
            {
                return _orig + (t * _dir);
            }
        }

        static double HitSphere(Vector3d center, double radius, Ray r)
        {
            Vector3d oc = center - r.Origin;

            double a = Vector3d.Dot(r.Direction, r.Direction);
            double b = -2.0 * Vector3d.Dot(r.Direction, oc);
            double c = Vector3d.Dot(oc, oc) - (radius * radius);

            double discriminant = (b * b) - (4 * a * c);

            if (discriminant < 0)
            {
                return -1.0;
            }
            else
            {
                return (-b - Math.Sqrt(discriminant)) / (2.0 * a);
            }
        }

        static Vector3d RayColor(Ray r)
        {
            var t = HitSphere(new Vector3d(0, 0, -1), 0.5, r);

            if (t > 0.0)
            {
                var N = Vector3d.UnitVector(r.At(t) - new Vector3d(0, 0, -1));
                return 0.5 * new Color(N.X + 1, N.Y + 1, N.Z + 1);
            }

            Vector3d unit_direction = Vector3d.UnitVector(r.Direction);
            var a = 0.5 * (unit_direction.Y + 1.0);
            return (1.0 - a) * new Vector3d(1.0, 1.0, 1.0) + a * new Vector3d(0.5, 0.7, 1.0);
        }


        public void Main(string path)
        {
            // Image Settings
            double aspectRatio = 16.0 / 9.0;
            int imageWidth = 400;

            // Calculate the image height, and ensure that it's at least 1.
            int imageHeight = (int)(imageWidth / aspectRatio);
            imageHeight = (imageHeight < 1) ? 1 : imageHeight;

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

            var buffer = new Vector3d[imageWidth * imageHeight];

            var index = 0;

            for (int j = 0; j < imageHeight; j++)
            {
                for (int i = 0; i < imageWidth; i++)
                {
                    var pixel_center = pixel00Loc + (i * pixelDeltaU) + (j * pixelDeltaV);
                    var ray_direction = pixel_center - cameraCenter;
                    var r = new Ray(cameraCenter, ray_direction);

                    buffer[index++] = RayColor(r);
                }
            }

            Png.Save(path, imageWidth, imageHeight, buffer);
        }
    }
}

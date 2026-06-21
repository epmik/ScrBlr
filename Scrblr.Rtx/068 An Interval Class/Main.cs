namespace Scrblr.Rtx
{
    class Chapter068
    {
        public struct Interval
        {
            public double Min { get; set; }
            public double Max { get; set; }

            // C++: interval() : min(+infinity), max(-infinity) {}
            // Explicit parameterless constructor (Supported in modern C#)
            public Interval()
            {
                Min = double.PositiveInfinity;
                Max = double.NegativeInfinity;
            }

            // C++: interval(double min, double max) : min(min), max(max) {}
            public Interval(double min, double max)
            {
                Min = min;
                Max = max;
            }

            // C++: double size() const
            public readonly double Size() => Max - Min;

            // C++: bool contains(double x) const
            public readonly bool Contains(double x) => Min <= x && x <= Max;

            // C++: bool surrounds(double x) const
            public readonly bool Surrounds(double x) => Min < x && x < Max;

            public readonly double Clamp(double x) => Math.Clamp(x, Min, Max);

            // C++: static const interval empty, universe;
            // In C#, static read-only fields provide the closest thread-safe equivalent to C++ static const objects
            public static readonly Interval Empty = new Interval(double.PositiveInfinity, double.NegativeInfinity);
            public static readonly Interval Universe = new Interval(double.NegativeInfinity, double.PositiveInfinity);
        }

        class HitRecord
        {
            public Vector3d P { get; set; }
            public Vector3d Normal { get; set; }

            public double T { get; set; }

            bool front_face;

            public HitRecord()
            {
                
            }

            public HitRecord(HitRecord record)
            {
                P = record.P;
                Normal = record.Normal;
                T = record.T;
                front_face = record.front_face;
            }

            public void set_face_normal(Ray r, Vector3d outward_normal) 
            {
                // Sets the hit record normal vector.
                // NOTE: the parameter `outward_normal` is assumed to have unit length.

                front_face = Vector3d.Dot(r.Direction, outward_normal) < 0;
                Normal = front_face? outward_normal : -outward_normal;
            }
        }

        abstract class Hittable
        {
            public abstract bool Hit(Ray r, Interval ray_t, out HitRecord rec);
        }

        class HittableList : Hittable
        {
            public List<Hittable> Objects { get; set; } = new List<Hittable>();

            // Constructors
            public HittableList() { }

            public HittableList(Hittable obj)
            {
                Add(obj);
            }

            // Methods
            public void Clear()
            {
                Objects.Clear();
            }

            public void Add(Hittable obj)
            {
                Objects.Add(obj);
            }

            public override bool Hit(Ray r, Interval ray_t, out HitRecord rec)
            {
                rec = new HitRecord();
                HitRecord tempRec;
                bool hitAnything = false;
                double closestSoFar = ray_t.Max;

                foreach (Hittable obj in Objects)
                {
                    if (obj.Hit(r, new Interval(ray_t.Min, closestSoFar), out tempRec))
                    {
                        hitAnything = true;
                        closestSoFar = tempRec.T;

                        rec = new HitRecord(tempRec);
                    }
                }

                return hitAnything;
            }
        }

        class Sphere : Hittable
        {
            Point3 _center;
            double _radius;

            public Sphere(Point3 center, double radius)
            {
                _center = center;
                _radius = radius;
            }

            public override bool Hit(Ray r, Interval ray_t, out HitRecord rec)
            {
                rec = new HitRecord();
                Vector3d oc = _center - r.Origin;

                var a = r.Direction.LengthSquared();
                var h = Vector3d.Dot(r.Direction, oc);
                var c = oc.LengthSquared() - _radius * _radius;

                var discriminant = h * h - a * c;

                if (discriminant < 0)
                    return false;

                var sqrtd = Math.Sqrt(discriminant);

                // Find the nearest root that lies in the acceptable range.
                var root = (h - sqrtd) / a;
                if (!ray_t.Surrounds(root))
                {
                    root = (h + sqrtd) / a;
                    if (!ray_t.Surrounds(root))
                        return false;
                }

                rec.T = root;
                rec.P = r.At(rec.T); 
                Vector3d outward_normal = (rec.P - _center) / _radius;
                rec.set_face_normal(r, outward_normal);

                return true;
            }
        };

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

        static Vector3d RayColor(Ray r, Hittable world)
        {
            HitRecord rec;

            if (world.Hit(r, new Interval(0, double.PositiveInfinity), out rec))
            {
                return 0.5 * (rec.Normal + new Color(1, 1, 1));
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

            // World

            HittableList world = new HittableList();

            world.Add(new Sphere(new Vector3d(0, 0, -1), 0.5));
            world.Add(new Sphere(new Vector3d(0, -100.5, -1), 100));

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

                    buffer[index++] = RayColor(r, world);
                }
            }

            Png.Save(path, imageWidth, imageHeight, buffer);
        }
    }
}

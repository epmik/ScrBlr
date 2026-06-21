namespace Scrblr.Rtx
{
    class Chapter070
    {
        class Camera
        {
            public double aspect_ratio = 1.0;
            public int image_width = 100;

            int image_height = 100;

            Vector3d center = new Vector3d(0, 0, 0);
            Vector3d pixel00_loc;    // Location of pixel 0, 0
            Vector3d pixel_delta_u;  // Offset to pixel to the right
            Vector3d pixel_delta_v;  // Offset to pixel below

            public void Render(HittableList world, string path)
            {
                Initialize();

                var buffer = new Vector3d[image_width * image_height];

                var index = 0;

                for (int j = 0; j < image_height; j++)
                {
                    for (int i = 0; i < image_width; i++)
                    {
                        var pixel_center = pixel00_loc + (i * pixel_delta_u) + (j * pixel_delta_v);
                        var ray_direction = pixel_center - center;
                        var r = new Ray(center, ray_direction);

                        buffer[index++] = RayColor(r, world);
                    }
                }

                Png.Save(path, image_width, image_height, buffer);

            }

            private void Initialize()
            {
                image_height = (int)(image_width / aspect_ratio);
                image_height = (image_height < 1) ? 1 : image_height;

                center = new  Point3(0, 0, 0);

                // Determine viewport dimensions.
                var focal_length = 1.0;
                var viewport_height = 2.0;
                var viewport_width = viewport_height * ((double)(image_width) / image_height);

                // Calculate the vectors across the horizontal and down the vertical viewport edges.
                var viewport_u = new Vector3d(viewport_width, 0, 0);
                var viewport_v = new Vector3d(0, -viewport_height, 0);

                // Calculate the horizontal and vertical delta vectors from pixel to pixel.
                pixel_delta_u = viewport_u / image_width;
                pixel_delta_v = viewport_v / image_height;

                // Calculate the location of the upper left pixel.
                var viewport_upper_left = center - new Vector3d(0, 0, focal_length) - viewport_u / 2 - viewport_v / 2;

                pixel00_loc = viewport_upper_left + 0.5 * (pixel_delta_u + pixel_delta_v);
            }

            private Vector3d RayColor(Ray r, HittableList world)
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
        }

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


        public void Main(string path)
        {
            HittableList world = new HittableList();

            world.Add(new Sphere(new Vector3d(0, 0, -1), 0.5));
            world.Add(new Sphere(new Vector3d(0, -100.5, -1), 100));

            var cam = new Camera();

            cam.aspect_ratio = 16.0 / 9.0;
            cam.image_width = 400;

            cam.Render(world, path);
        }
    }
}

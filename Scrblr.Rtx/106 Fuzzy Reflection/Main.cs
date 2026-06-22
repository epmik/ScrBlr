using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scrblr.Rtx
{
    class Chapter106
    {
        abstract class Material
        {
            public virtual bool Scatter(
                Ray rIn,
                HitRecord rec,
                out Vector3d attenuation,
                out Ray scattered)
            {
                // Set default output values before returning false
                attenuation = new Vector3d(0, 0, 0);
                scattered = new Ray();

                return false;
            }
        }
        
        class Lambertian : Material
        {
            Color _albedo;

            public Lambertian(Color albedo)
            {
                _albedo = albedo;
            }

            override public bool Scatter(Ray r_in, HitRecord rec, out Color attenuation, out Ray scattered)
            {
                var scatter_direction = rec.Normal + Vector3d.random_unit_vector();

                // Catch degenerate scatter direction
                if (Vector3d.NearZero(scatter_direction))
                    scatter_direction = rec.Normal;

                scattered = new Ray(rec.P, scatter_direction);
                attenuation = _albedo;

                return true;
            }
        }

        class Metal : Material
        {
            Color _albedo;
            double _fuzz;

            public Metal(Color albedo, double fuzz)
            {
                _albedo = albedo;
                _fuzz = fuzz;
            }

            override public bool Scatter(Ray r_in, HitRecord rec, out Color attenuation, out Ray scattered)
            {
                Vector3d reflected = Vector3d.Reflect(r_in.Direction, rec.Normal);
                
                reflected = Vector3d.UnitVector(reflected) + (_fuzz * Vector3d.random_unit_vector());
                
                scattered = new Ray(rec.P, reflected);
                
                attenuation = _albedo;
                
                return (Vector3d.Dot(scattered.Direction, rec.Normal) > 0);
            }
        }

        class Camera
        {
            public double aspect_ratio = 1.0;
            public int image_width = 100;
            public int samples_per_pixel = 10;   // Count of random samples for each pixel
            public int max_depth = 10;   // Maximum number of ray bounces into scene

            int image_height = 100;
            double pixel_samples_scale;  // Color scale factor for a sum of pixel samples

            Vector3d center = new Vector3d(0, 0, 0);
            Vector3d pixel00_loc;    // Location of pixel 0, 0
            Vector3d pixel_delta_u;  // Offset to pixel to the right
            Vector3d pixel_delta_v;  // Offset to pixel below

            public bool OutputLinearColorSpace = false;  // Output linear color space (no gamma correction) if true

            static readonly Interval intensity = new Interval(0.000, 0.999);

            public void Render(HittableList world, string path)
            {
                Initialize();

                var buffer = new Vector3d[image_width * image_height];

                var index = 0;

                for (int j = 0; j < image_height; j++)
                {
                    for (int i = 0; i < image_width; i++)
                    {
                        //color pixel_color(0,0,0);
                        //for (int sample = 0; sample < samples_per_pixel; sample++)
                        //{
                        //    ray r = get_ray(i, j);
                        //    pixel_color += ray_color(r, world);
                        //}
                        //write_color(std::cout, pixel_samples_scale * pixel_color);

                        var pixel_color = new Color(0, 0, 0);

                        for (int sample = 0; sample < samples_per_pixel; sample++)
                        {
                            var r = get_ray(i, j);
                            pixel_color += RayColor(r, max_depth, world);
                        }

                        pixel_color *= pixel_samples_scale;

                        if(!OutputLinearColorSpace)
                        {
                            pixel_color = new Color(
                                linear_to_gamma(pixel_color.X),
                                linear_to_gamma(pixel_color.Y),
                                linear_to_gamma(pixel_color.Z));
                        }

                        buffer[index++] = pixel_color;
                    }
                }

                Png.Save(path, image_width, image_height, buffer);

            }

            Ray get_ray(int i, int j) 
            {
                // Construct a camera ray originating from the origin and directed at randomly sampled
                // point around the pixel location i, j.

                var offset = sample_square();
                var pixel_sample = pixel00_loc
                                    + ((i + offset.X) * pixel_delta_u)
                                    + ((j + offset.Y) * pixel_delta_v);

                var ray_origin = center;
                var ray_direction = pixel_sample - ray_origin;

                return new Ray(ray_origin, ray_direction);
            }

            Vector3d sample_square() 
            {
                // Returns the vector to a random point in the [-.5,-.5]-[+.5,+.5] unit square.
                return new Vector3d(Utility.RandomDouble() - 0.5, Utility.RandomDouble() - 0.5, 0);
            }

            private void Initialize()
            {
                image_height = (int)(image_width / aspect_ratio);
                image_height = (image_height < 1) ? 1 : image_height;

                pixel_samples_scale = 1.0 / samples_per_pixel;

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

            private Vector3d RayColor(Ray r, int depth, HittableList world)
            {
                // If we've exceeded the ray bounce limit, no more light is gathered.
                if (depth <= 0)
                    return new Color(0, 0, 0);

                HitRecord rec;

                if (world.Hit(r, new Interval(0.001, double.PositiveInfinity), out rec))
                {
                    Ray scattered;
                    Color attenuation;

                    if (rec.mat.Scatter(r, rec, out attenuation, out scattered))
                        return attenuation * RayColor(scattered, depth - 1, world);

                    return new Color(0, 0, 0);
                }

                Vector3d unit_direction = Vector3d.UnitVector(r.Direction);
                var a = 0.5 * (unit_direction.Y + 1.0);
                return (1.0 - a) * new Vector3d(1.0, 1.0, 1.0) + a * new Vector3d(0.5, 0.7, 1.0);
            }

            double linear_to_gamma(double linear_component)
            {
                if (linear_component > 0)
                    return Math.Sqrt(linear_component);

                return 0;
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

            public Material mat;

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
                mat = record.mat;
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
            Material _mat;

            public Sphere(Point3 center, double radius, Material mat)
            {
                _center = center;
                _radius = radius;
                _mat = mat;
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
                rec.mat = _mat;

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

            var material_ground = new Lambertian(new Color(0.8, 0.8, 0.0));
            var material_center = new Lambertian(new Color(0.1, 0.2, 0.5));
            var material_left = new Metal(new Color(0.8, 0.8, 0.8), 0.3);
            var material_right = new Metal(new Color(0.8, 0.6, 0.2), 1.0);

            world.Add(new Sphere(new Vector3d(0, -100.5, -1), 100, material_ground));
            world.Add(new Sphere(new Vector3d(0, 0, -1.2), 0.5, material_center));
            world.Add(new Sphere(new Vector3d(-1, 0, -1), 0.5, material_left));
            world.Add(new Sphere(new Vector3d(1, 0, -1), 0.5, material_right));


            var cam = new Camera();

            cam.aspect_ratio = 16.0 / 9.0;
            cam.image_width = 400;
            cam.samples_per_pixel = 100;
            cam.max_depth = 50;

            cam.Render(world, path);
        }
    }
}

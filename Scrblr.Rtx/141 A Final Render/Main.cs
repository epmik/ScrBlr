using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scrblr.Rtx
{
    class Chapter141
    {
        protected IRandomGenerator _randomGenerator;

        protected class SceneSettings
        {
            public bool AddLargeSpheres { get; set; } = true;
            public bool AddSmallStaticSpheres { get; set; } = true;
            public bool AddSmallDynamicSpheres { get; set; } = false;

            public int ImageWidth { get; set; } = 400;

            public int SamplesPerPixel { get; set; } = 16;
        }

        protected class Camera
        {
            protected IRandomGenerator _randomGenerator;
            public double aspect_ratio = 1.0;
            public int image_width = 100;
            public int samples_per_pixel = 10;   // Count of random samples for each pixel
            public int max_depth = 10;   // Maximum number of ray bounces into scene

            public double vfov = 90;  // Vertical view angle (field of view)
            public Point3 lookfrom = new Point3(0, 0, 0);   // Point camera is looking from
            public Point3 lookat = new Point3(0, 0, -1);  // Point camera is looking at
            public Vector3d vup = new Vector3d(0, 1, 0);     // Camera-relative "up" direction

            public double defocus_angle = 0;  // Variation angle of rays through each pixel
            public double focus_dist = 10;    // Distance from camera lookfrom point to plane of perfect focus

            public int image_height = 100;
            public double pixel_samples_scale;  // Color scale factor for a sum of pixel samples

            public double ShutterTime = 0.0;

            Vector3d center = new Vector3d(0, 0, 0);
            Vector3d pixel00_loc;    // Location of pixel 0, 0
            Vector3d pixel_delta_u;  // Offset to pixel to the right
            Vector3d pixel_delta_v;  // Offset to pixel below
            Vector3d u, v, w;              // Camera frame basis vectors
            Vector3d defocus_disk_u;       // Defocus disk horizontal radius
            Vector3d defocus_disk_v;       // Defocus disk vertical radius


            public bool OutputLinearColorSpace = false;  // Output linear color space (no gamma correction) if true

            static readonly Interval intensity = new Interval(0.000, 0.999);

            public Camera(IRandomGenerator randomGenerator)
            {
                _randomGenerator = randomGenerator;
            }

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

                        if (!OutputLinearColorSpace)
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

            public Ray get_ray(int i, int j)
            {
                // Construct a camera ray originating from the defocus disk and directed at a randomly
                // sampled point around the pixel location i, j.

                var offset = sample_square();
                var pixel_sample = pixel00_loc
                                    + ((i + offset.X) * pixel_delta_u)
                                    + ((j + offset.Y) * pixel_delta_v);

                var ray_origin = (defocus_angle <= 0) ? center : defocus_disk_sample();
                var ray_direction = pixel_sample - ray_origin;

                var t = ShutterTime > 0.0 ? _randomGenerator.Double() * ShutterTime : 0.0;

                return new Ray(ray_origin, ray_direction, t);
            }

            Vector3d sample_square()
            {
                // Returns the vector to a random point in the [-.5,-.5]-[+.5,+.5] unit square.
                return new Vector3d(_randomGenerator.Double() - 0.5, _randomGenerator.Double() - 0.5, 0);
            }

            Vector3d defocus_disk_sample()
            {
                // Returns a random point in the camera defocus disk.
                var p = Vector3d.random_in_unit_disk();
                return center + (p[0] * defocus_disk_u) + (p[1] * defocus_disk_v);
            }

            public void Initialize()
            {
                image_height = (int)(image_width / aspect_ratio);
                image_height = (image_height < 1) ? 1 : image_height;

                pixel_samples_scale = 1.0 / samples_per_pixel;

                center = lookfrom;

                // Determine viewport dimensions.
                var focal_length = (lookfrom - lookat).Length();
                var theta = Utility.ToRadians(vfov);
                var h = Math.Tan(theta / 2);
                var viewport_height = 2 * h * focus_dist;
                var viewport_width = viewport_height * ((double)(image_width) / image_height);

                // Calculate the u,v,w unit basis vectors for the camera coordinate frame.
                w = Vector3d.UnitVector(lookfrom - lookat);
                u = Vector3d.UnitVector(Vector3d.Cross(vup, w));
                v = Vector3d.Cross(w, u);

                // Calculate the vectors across the horizontal and down the vertical viewport edges.
                Vector3d viewport_u = viewport_width * u;    // Vector across viewport horizontal edge
                Vector3d viewport_v = viewport_height * -v;  // Vector down viewport vertical edge

                // Calculate the horizontal and vertical delta vectors from pixel to pixel.
                pixel_delta_u = viewport_u / image_width;
                pixel_delta_v = viewport_v / image_height;

                // Calculate the location of the upper left pixel.
                var viewport_upper_left = center - (focus_dist * w) - viewport_u / 2 - viewport_v / 2;
                pixel00_loc = viewport_upper_left + 0.5 * (pixel_delta_u + pixel_delta_v);

                // Calculate the camera defocus disk basis vectors.
                var defocus_radius = focus_dist * Math.Tan(Utility.ToRadians(defocus_angle / 2));
                defocus_disk_u = u * defocus_radius;
                defocus_disk_v = v * defocus_radius;
            }

            public Vector3d RayColor(Ray r, int depth, HittableList world)
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
        }

        protected static double linear_to_gamma(double linear_component)
        {
            if (linear_component > 0)
                return Math.Sqrt(linear_component);

            return 0;
        }

        protected abstract class Material
        {
            protected IRandomGenerator _randomGenerator;

            protected Material(IRandomGenerator randomGenerator)
            {
                _randomGenerator = randomGenerator;
            }

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

        protected class Lambertian : Material
        {
            Color _albedo;

            public Lambertian(Color albedo, IRandomGenerator randomGenerator)
                : base(randomGenerator)
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

        protected class Dielectric : Material
        {
            // Refractive index in vacuum or air, or the ratio of the material's refractive index over
            // the refractive index of the enclosing media
            double _refraction_index;

            public Dielectric(double refraction_index, IRandomGenerator randomGenerator)
                : base(randomGenerator)
            {
                _refraction_index = refraction_index;
            }

            override public bool Scatter(Ray r_in, HitRecord rec, out Color attenuation, out Ray scattered)
            {
                attenuation = new Color(1.0, 1.0, 1.0);
                double ri = rec.front_face ? (1.0 / _refraction_index) : _refraction_index;

                var unit_direction = Vector3d.UnitVector(r_in.Direction);

                double cos_theta = Math.Min(Vector3d.Dot(-unit_direction, rec.Normal), 1.0);
                double sin_theta = Math.Sqrt(1.0 - cos_theta * cos_theta);

                bool cannot_refract = ri * sin_theta > 1.0;
                Vector3d direction;

                if (cannot_refract || Reflectance(cos_theta, ri) > _randomGenerator.Double())
                    direction = Vector3d.Reflect(unit_direction, rec.Normal);
                else
                    direction = Vector3d.Refract(unit_direction, rec.Normal, ri);

                scattered = new Ray(rec.P, direction);

                return true;
            }

            static double Reflectance(double cosine, double refraction_index)
            {
                // Use Schlick's approximation for reflectance.
                var r0 = (1 - refraction_index) / (1 + refraction_index);
                r0 = r0 * r0;
                return r0 + (1 - r0) * Math.Pow((1 - cosine), 5);
            }
        }

        protected class Metal : Material
        {
            Color _albedo;
            double _fuzz;

            public Metal(Color albedo, double fuzz, IRandomGenerator randomGenerator)
                : base(randomGenerator)
            {
                _albedo = albedo;
                _fuzz = fuzz;
            }

            override public bool Scatter(Ray r_in, HitRecord rec, out Color attenuation, out Ray scattered)
            {
                Vector3d reflected = Vector3d.Reflect(r_in.Direction, rec.Normal);

                reflected = Vector3d.UnitVector(reflected) + (_fuzz * _randomGenerator.UnitVector3d());

                scattered = new Ray(rec.P, reflected);

                attenuation = _albedo;

                return (Vector3d.Dot(scattered.Direction, rec.Normal) > 0);
            }
        }

        protected struct Interval
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

        protected class HitRecord
        {
            public Vector3d P { get; set; }
            public Vector3d Normal { get; set; }

            public Material mat;

            public double T { get; set; }

            public bool front_face;

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
                Normal = front_face ? outward_normal : -outward_normal;
            }
        }

        protected abstract class Hittable
        {
            public abstract bool Hit(Ray r, Interval ray_t, out HitRecord rec);
        }

        protected class HittableList : Hittable
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

        protected class Sphere : Hittable
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

        protected struct Ray
        {
            private Vector3d _orig;
            private Vector3d _dir;

            private double _time;

            // Parameterless constructor (Defaults to 0,0,0 vectors)
            public Ray()
            {
                _orig = new Vector3d();
                _dir = new Vector3d();
            }

            // Main constructor
            public Ray(Vector3d origin, Vector3d direction, double time = 0.0)
            {
                _orig = origin;
                _dir = direction;
                _time = time;
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

        protected void CreateScene(SceneSettings sceneSettings, out HittableList world, out Camera cam)
        {
            world = new HittableList();


            var ground_material = new Lambertian(new Color(0.5, 0.5, 0.5), _randomGenerator);
            world.Add(new Sphere(new Point3(0, -1000, 0), 1000, ground_material));

            if (sceneSettings.AddSmallStaticSpheres)
            {

                for (int a = -11; a < 11; a++)
                {
                    for (int b = -11; b < 11; b++)
                    {
                        var choose_mat = _randomGenerator.Double();
                        var center = new Point3(a + 0.9 * _randomGenerator.Double(), 0.2, b + 0.9 * _randomGenerator.Double());

                        if ((center - new Point3(4, 0.2, 0)).Length() > 0.9)
                        {
                            if (choose_mat < 0.8)
                            {
                                // diffuse
                                var albedo = _randomGenerator.Color() * _randomGenerator.Color();
                                world.Add(new Sphere(center, 0.2, new Lambertian(albedo, _randomGenerator)));
                            }
                            else if (choose_mat < 0.95)
                            {
                                // metal
                                var albedo = _randomGenerator.Vector3d(0.5, 1);
                                var fuzz = _randomGenerator.Double() * 0.5;
                                world.Add(new Sphere(center, 0.2, new Metal(albedo, fuzz, _randomGenerator)));
                            }
                            else
                            {
                                // glass
                                world.Add(new Sphere(center, 0.2, new Dielectric(1.5, _randomGenerator)));
                            }
                        }
                    }
                }
            }

            if (sceneSettings.AddLargeSpheres)
            { 
                var material1 = new Dielectric(1.5, _randomGenerator);
                world.Add(new Sphere(new Vector3d(0, 1, 0), 1.0, material1));

                var material2 = new Lambertian(new Color(0.4, 0.2, 0.1), _randomGenerator);
                world.Add(new Sphere(new Vector3d(-4, 1, 0), 1.0, material2));

                var material3 = new Metal(new Color(0.7, 0.6, 0.5), 0.0, _randomGenerator);
                world.Add(new Sphere(new Vector3d(4, 1, 0), 1.0, material3));
            }

            cam = new Camera(_randomGenerator);

            cam.aspect_ratio = 16.0 / 9.0;
            cam.max_depth = 50;
            cam.vfov = 20;
            cam.lookfrom = new Point3(13, 2, 3);
            cam.lookat = new Point3(0, 0, 0);
            cam.vup = new Vector3d(0, 1, 0);
            cam.defocus_angle = 0.6;
            cam.focus_dist = 10.0;

            cam.samples_per_pixel = sceneSettings.SamplesPerPixel;
            cam.image_width = sceneSettings.ImageWidth;
        }


        public virtual void Main(string path)
        {
            HittableList world;
            Camera cam;


            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"// ------------------------ //");
            Console.WriteLine($"");

            Console.WriteLine("Rendering 141 A Final Render...");
            Console.WriteLine("Setup...");

            _randomGenerator = new RandomGenerator(1024);

            CreateScene(new SceneSettings { ImageWidth = 200, SamplesPerPixel = 8 }, out world, out cam);

            stopwatch.Stop();

            Console.WriteLine($"Setup duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine("Rendering...");

            stopwatch.Restart();

            cam.Render(world, path);

            stopwatch.Stop();

            Console.WriteLine("Rendering finished...");

            Console.WriteLine($"Render duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            Console.WriteLine($"");
            Console.WriteLine($"// ------------------------ //");
        }
    }
}

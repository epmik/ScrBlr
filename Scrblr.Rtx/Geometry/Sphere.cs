namespace Scrblr.Rtx
{
    using System;

    // C++: class sphere : public hittable
    public class Sphere : Hittable
    {
        private readonly Vector3d _center;
        private readonly double _radius;
        Material _mat;

        public Sphere(Vector3d center, double radius, Material mat)
        {
            _mat = mat;
            _center = center;
            // std::fmax(0, radius) translates directly to Math.Max(0, radius)
            _radius = Math.Max(0.0, radius);
        }

        public override bool Hit(Ray r, Interval ray_t, HitRecord rec)
        {
            Vector3d oc = _center - r.Origin;

            // Notice the optimization from your updated C++ source:
            // a = length_squared(), and using 'h' (half of b) to simplify the quadratic discriminant formula
            double a = r.Direction.LengthSquared();
            double h = Vector3d.Dot(r.Direction, oc);
            double c = oc.LengthSquared() - (_radius * _radius);

            double discriminant = (h * h) - (a * c);
            if (discriminant < 0)
            {
                return false;
            }

            double sqrtd = Math.Sqrt(discriminant);

            // Find the nearest root that lies in the acceptable range.
            double root = (h - sqrtd) / a;
            if (!ray_t.Surrounds(root))
            {
                root = (h + sqrtd) / a;
                if (!ray_t.Surrounds(root))
                {
                    return false;
                }
            }

            // Populate the passed reference object with the hit details
            rec.T = root;
            rec.P = r.At(rec.T);

            Vector3d outward_normal = (rec.P - _center) / _radius;
            rec.CalculateFaceNormal(r, outward_normal);
            rec.mat = _mat;

            return true;
        }
    }
}

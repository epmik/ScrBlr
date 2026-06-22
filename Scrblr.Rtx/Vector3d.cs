global using Color = Scrblr.Rtx.Vector3d;
global using Point3 = Scrblr.Rtx.Vector3d;

namespace Scrblr.Rtx
{
    using System;

    public struct Vector3d
    {
        public static Vector3d Zero = new Vector3d(0, 0, 0);
        public static Vector3d One = new Vector3d(1, 1, 1);

        // Constructors
        public Vector3d() : this(0, 0, 0) { }

        public Vector3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // Getters matching your original x(), y(), z()
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        // Indexer mimicking the operator[]
        public double this[int i]
        {
            readonly get
            {
                return i switch
                {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    _ => throw new IndexOutOfRangeException("Vector index out of bounds!")
                };
            }
            set
            {
                switch (i)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default: throw new IndexOutOfRangeException("Vector index out of bounds!");
                }
            }
        }

        // Unary minus operator
        public static Vector3d operator -(Vector3d v) => new Vector3d(-v.X, -v.Y, -v.Z);

        // Binary math operators
        public static Vector3d operator +(Vector3d u, Vector3d v) => new Vector3d(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        public static Vector3d operator -(Vector3d u, Vector3d v) => new Vector3d(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        public static Vector3d operator *(Vector3d u, Vector3d v) => new Vector3d(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
        public static Vector3d operator *(double t, Vector3d v) => new Vector3d(t * v.X, t * v.Y, t * v.Z);
        public static Vector3d operator *(Vector3d v, double t) => t * v;
        public static Vector3d operator /(Vector3d v, double t) => (1.0 / t) * v;

        // Methods
        public readonly double LengthSquared() => (X * X) + (Y * Y) + (Z * Z);
        public readonly double Length() => Math.Sqrt(LengthSquared());

        // Utility Static Functions
        public static double Dot(Vector3d u, Vector3d v) => (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z);

        public static Vector3d Cross(Vector3d u, Vector3d v)
        {
            return new Vector3d(
                (u.Y * v.Z) - (u.Z * v.Y),
                (u.Z * v.X) - (u.X * v.Z),
                (u.X * v.Y) - (u.Y * v.X)
            );
        }

        public static Vector3d UnitVector(Vector3d v) => v / v.Length();

        public override readonly string ToString() => $"{X} {Y} {Z}";

        public static Vector3d random()
        {
            return new Vector3d(Utility.RandomDouble(), Utility.RandomDouble(), Utility.RandomDouble());
        }

        public static Vector3d random(double min, double max)
        {
            return new Vector3d(Utility.RandomDouble(min, max), Utility.RandomDouble(min, max), Utility.RandomDouble(min, max));
        }

        public static Vector3d random_unit_vector() 
        {
            while (true) 
            {
                var p = random(-1, 1);

                var lensq = p.LengthSquared();
                
                if (1e-160 < lensq && lensq <= 1)
                    return p / Math.Sqrt(lensq);
            }
        }

        public static Vector3d random_on_hemisphere(Vector3d normal) 
        {
            var on_unit_sphere = random_unit_vector();

            if (Vector3d.Dot(on_unit_sphere, normal) > 0.0) // In the same hemisphere as the normal
                return on_unit_sphere;
            else
                return -on_unit_sphere;
        }

        // Helper to catch near-zero vectors and avoid floating-point errors
        public static bool NearZero(Vector3d v)
        {
            double s = 1e-8;
            return (Math.Abs(v.X) < s) && (Math.Abs(v.Y) < s) && (Math.Abs(v.Z) < s);
        }

        public static Vector3d Reflect(Vector3d v, Vector3d n) 
        {
            return v - 2 * Vector3d.Dot(v, n) * n;
        }

        public static Vector3d Refract(Vector3d uv, Vector3d n, double etai_over_etat)
        {
            var cos_theta = Math.Min(Vector3d.Dot(-uv, n), 1.0);
            var r_out_perp = etai_over_etat * (uv + cos_theta * n);
            var r_out_parallel = -Math.Sqrt(Math.Abs(1.0 - r_out_perp.LengthSquared())) * n;
            return r_out_perp + r_out_parallel;
        }

        public static Vector3d random_in_unit_disk() 
        {
            while (true) 
            {
                var p = new Vector3d(Utility.RandomDouble(-1, 1), Utility.RandomDouble(-1, 1), 0);
                if (p.LengthSquared() < 1)
                    return p;
            }
        }
    }
}

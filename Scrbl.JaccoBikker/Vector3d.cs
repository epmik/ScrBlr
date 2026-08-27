global using Color = Scrbl.JaccoBikker.Vector3d;
global using Point3 = Scrbl.JaccoBikker.Vector3d;

namespace Scrbl.JaccoBikker
{
    using System;

    public readonly struct Vector3d : IEquatable<Vector3d>
    {
        // Fix: Static properties prevent modification and use default/cached values
        public static Vector3d Zero => default;
        public static Vector3d One => new(1.0, 1.0, 1.0);

        // Fix: Init-only properties make the struct safely immutable
        public double X { get; init; }
        public double Y { get; init; }
        public double Z { get; init; }

        // Constructors
        public Vector3d(double v) => (X, Y, Z) = (v, v, v);

        public Vector3d(double x, double y, double z) => (X, Y, Z) = (x, y, z);

        // Indexer (Read-only now)
        public double this[int i] => i switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new ArgumentOutOfRangeException(nameof(i), "Vector index out of bounds!")
        };

        // Unary Operators
        public static Vector3d operator -(Vector3d v) => new(-v.X, -v.Y, -v.Z);

        // Binary Operators
        public static Vector3d operator +(Vector3d u, Vector3d v) => new(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        public static Vector3d operator -(Vector3d u, Vector3d v) => new(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        public static Vector3d operator *(Vector3d u, Vector3d v) => new(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
        public static Vector3d operator *(double t, Vector3d v) => new(t * v.X, t * v.Y, t * v.Z);
        public static Vector3d operator *(Vector3d v, double t) => t * v;
        public static Vector3d operator /(Vector3d v, double t) => (1.0 / t) * v;

        // Instance Methods
        public double LengthSquared() => (X * X) + (Y * Y) + (Z * Z);
        public double Length() => Math.Sqrt(LengthSquared());

        // Utility Static Functions
        public static double Dot(Vector3d u, Vector3d v) => (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z);

        public static Vector3d Cross(Vector3d u, Vector3d v) => new(
            (u.Y * v.Z) - (u.Z * v.Y),
            (u.Z * v.X) - (u.X * v.Z),
            (u.X * v.Y) - (u.Y * v.X)
        );

        public static Vector3d UnitVector(Vector3d v) => v / v.Length();

        // Fix: Use standard string formatting or interpolation
        public override string ToString() => $"{X} {Y} {Z}";

        // Fix: Added high-performance Equality overrides
        public bool Equals(Vector3d other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        public override bool Equals(object? obj) => obj is Vector3d other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public static bool operator ==(Vector3d left, Vector3d right) => left.Equals(right);
        public static bool operator !=(Vector3d left, Vector3d right) => !left.Equals(right);

        // Math Functions
        public static bool NearZero(Vector3d v)
        {
            const double s = 1e-8;
            return (Math.Abs(v.X) < s) && (Math.Abs(v.Y) < s) && (Math.Abs(v.Z) < s);
        }

        public static Vector3d Reflect(Vector3d v, Vector3d n) => v - 2 * Dot(v, n) * n;

        public static Vector3d Refract(Vector3d uv, Vector3d n, double etaiOverEtat)
        {
            var cosTheta = Math.Min(Dot(-uv, n), 1.0);
            var rOutPerp = etaiOverEtat * (uv + cosTheta * n);
            var rOutParallel = -Math.Sqrt(Math.Abs(1.0 - rOutPerp.LengthSquared())) * n;
            return rOutPerp + rOutParallel;
        }

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

        public static Vector3d random_in_unit_disk()
        {
            while (true)
            {
                var p = new Vector3d(Utility.RandomDouble(-1, 1), Utility.RandomDouble(-1, 1), 0);
                if (p.LengthSquared() < 1)
                    return p;
            }
        }

        internal static Vector3d Min(Vector3d a, Vector3d b)
        {
            return new Vector3d(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
        }

        internal static Vector3d Max(Vector3d a, Vector3d b)
        {
            return new Vector3d(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        }

        //// 2. Use 'in' for vector arguments to pass by reference without copying
        //public static Vector3d operator +(in Vector3d u, in Vector3d v) => new(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        //public static Vector3d operator -(in Vector3d u, in Vector3d v) => new(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        //public static Vector3d operator *(in Vector3d u, in Vector3d v) => new(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
        //public static Vector3d operator *(double t, in Vector3d v) => new(t * v.X, t * v.Y, t * v.Z);

        //// 3. Update utility methods to use 'in' references
        //public static double Dot(in Vector3d u, in Vector3d v) => (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z);

        //public static Vector3d Cross(in Vector3d u, in Vector3d v) => new(
        //    (u.Y * v.Z) - (u.Z * v.Y),
        //    (u.Z * v.X) - (u.X * v.Z),
        //    (u.X * v.Y) - (u.Y * v.X)
        //);
    }
}

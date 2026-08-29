global using Color3d = Scrbl.JaccoBikker.Vector3d;
global using Point3d = Scrbl.JaccoBikker.Vector3d;

namespace Scrbl.JaccoBikker
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Vector3d /* : IEquatable<Vector3d> */
    {
        //public static Vector3d Zero => default;
        //public static Vector3d One => new(1.0f, 1.0f, 1.0);

        public double X;
        public double Y;
        public double Z;

        // Constructors
        public Vector3d(double v) => (X, Y, Z) = (v, v, v);

        public Vector3d(in Vector3d v) => (X, Y, Z) = (v.X, v.Y, v.Z);

        public Vector3d(in Vector3f v) => (X, Y, Z) = (v.X, v.Y, v.Z);

        public Vector3d(double x, double y, double z) => (X, Y, Z) = (x, y, z);

        public Vector3d(float v) => (X, Y, Z) = (v, v, v);

        public Vector3d(float x, float y, float z) => (X, Y, Z) = (x, y, z);

        public double this[int i] => i switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new ArgumentOutOfRangeException(nameof(i), "Vector index out of bounds!")
        };

        // Unary Operators
        public static Vector3d operator -(in Vector3d v) => new(-v.X, -v.Y, -v.Z);

        // Binary Operators
        public static Vector3d operator +(in Vector3d u, in Vector3d v) => new(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        public static Vector3d operator -(in Vector3d u, in Vector3d v) => new(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        public static Vector3d operator *(in Vector3d u, in Vector3d v) => new(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
        public static Vector3d operator *(float t, in Vector3d v) => new(t * v.X, t * v.Y, t * v.Z);
        public static Vector3d operator *(in Vector3d v, float t) => t * v;
        public static Vector3d operator /(in Vector3d v, float t) => (1.0f / t) * v;
        public static Vector3d operator *(double t, in Vector3d v) => new(t * v.X, t * v.Y, t * v.Z);
        public static Vector3d operator *(in Vector3d v, double t) => t * v;
        public static Vector3d operator /(in Vector3d v, double t) => (1.0f / t) * v;

        // Instance Methods
        public double LengthSquared() => (X * X) + (Y * Y) + (Z * Z);
        public double Length() => Math.Sqrt(LengthSquared());

        // Utility Static Functions
        public static double Dot(in Vector3d u, in Vector3d v) => (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z);

        public static Vector3d Cross(in Vector3d u, in Vector3d v) => new(
            (u.Y * v.Z) - (u.Z * v.Y),
            (u.Z * v.X) - (u.X * v.Z),
            (u.X * v.Y) - (u.Y * v.X)
        );

        public static Vector3d Normalize(in Vector3d v) => v / v.Length();

        // Fix: Use standard string formatting or interpolation
        public override string ToString() => $"{X} {Y} {Z}";

        //public bool Equals(in Vector3d other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        //public override bool Equals(object? obj) => obj is Vector3d other && Equals(other);
        //public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public static bool operator ==(in Vector3d left, in Vector3d right) => left.Equals(right);
        public static bool operator !=(in Vector3d left, in Vector3d right) => !left.Equals(right);

        // Math Functions
        public static bool NearZero(in Vector3d v)
        {
            const float s = 1e-8f;
            return (Math.Abs(v.X) < s) && (Math.Abs(v.Y) < s) && (Math.Abs(v.Z) < s);
        }

        public static Vector3d Reflect(in Vector3d v, in Vector3d n) => v - 2 * Dot(v, n) * n;

        public static Vector3d Refract(in Vector3d uv, in Vector3d n, float etaiOverEtat)
        {
            var cosTheta = Math.Min(Dot(-uv, n), 1.0f);
            var rOutPerp = etaiOverEtat * (uv + cosTheta * n);
            var rOutParallel = -Math.Sqrt(Math.Abs(1.0f - rOutPerp.LengthSquared())) * n;
            return rOutPerp + rOutParallel;
        }

        internal static Vector3d Min(in Vector3d a, in Vector3d b)
        {
            return new Vector3d(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
        }

        internal static Vector3d Max(in Vector3d a, in Vector3d b)
        {
            return new Vector3d(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        }
    }
}

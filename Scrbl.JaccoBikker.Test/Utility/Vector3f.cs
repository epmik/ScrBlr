global using Color3f = Scrbl.JaccoBikker.Vector3f;
global using Point3f = Scrbl.JaccoBikker.Vector3f;

namespace Scrbl.JaccoBikker
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector3f /* : IEquatable<Vector3f> */
    {
        //public static Vector3f Zero => default;
        //public static Vector3f One => new(1.0f, 1.0f, 1.0);

        public float X;
        public float Y;
        public float Z;

        // Constructors
        public Vector3f(in double v) => (X, Y, Z) = ((float)v, (float)v, (float)v);

        public Vector3f(in Vector3f v) => (X, Y, Z) = (v.X, v.Y, v.Z);

        //public Vector3f(in Vector3d v) => (X, Y, Z) = ((float)v.X, (float)v.Y, (float)v.Z);

        public Vector3f(in double x, in double y, in double z) => (X, Y, Z) = ((float)x, (float)y, (float)z);

        public Vector3f(in float v) => (X, Y, Z) = (v, v, v);

        public Vector3f(in float x, in float y, in float z) => (X, Y, Z) = (x, y, z);

        public float this[in int i] => i switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new ArgumentOutOfRangeException(nameof(i), "Vector index out of bounds!")
        };

        public static implicit operator Vector3f(float value)
        {
            return new Vector3f(value);
        }

        public static implicit operator Vector3f(double value)
        {
            return new Vector3f(value);
        }

        public static Vector3f operator -(in Vector3f v) => new(-v.X, -v.Y, -v.Z);
        public static Vector3f operator +(in Vector3f u, in Vector3f v) => new(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        public static Vector3f operator -(in Vector3f u, in Vector3f v) => new(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        public static Vector3f operator *(in Vector3f u, in Vector3f v) => new(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
        public static Vector3f operator *(float t, in Vector3f v) => new(t * v.X, t * v.Y, t * v.Z);
        public static Vector3f operator *(in Vector3f v, float t) => t * v;
        public static Vector3f operator /(in Vector3f v, float t) => (1.0f / t) * v;

        // Instance Methods
        public float LengthSquared() => (X * X) + (Y * Y) + (Z * Z);
        public float Length() => MathF.Sqrt(LengthSquared());

        // Utility Static Functions
        public static float Dot(in Vector3f u, in Vector3f v) => (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z);

        public static Vector3f Cross(in Vector3f u, in Vector3f v) => new(
            (u.Y * v.Z) - (u.Z * v.Y),
            (u.Z * v.X) - (u.X * v.Z),
            (u.X * v.Y) - (u.Y * v.X)
        );

        public static Vector3f Normalize(in Vector3f v) => v / v.Length();

        // Fix: Use standard string formatting or interpolation
        public override string ToString() => $"{X} {Y} {Z}";

        //public bool Equals(in Vector3f other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        //public override bool Equals(object? obj) => obj is Vector3f other && Equals(other);
        //public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public static bool operator == (in Vector3f left, in Vector3f right) => left.Equals(right);
        public static bool operator != (in Vector3f left, in Vector3f right) => !left.Equals(right);

        // Math Functions
        public static bool NearZero(in Vector3f v)
        {
            const float s = 1e-8f;
            return (MathF.Abs(v.X) < s) && (MathF.Abs(v.Y) < s) && (MathF.Abs(v.Z) < s);
        }

        public static Vector3f Reflect(in Vector3f v, in Vector3f n) => v - 2 * Dot(v, n) * n;

        public static Vector3f Refract(in Vector3f uv, in Vector3f n, float etaiOverEtat)
        {
            var cosTheta = MathF.Min(Dot(-uv, n), 1.0f);
            var rOutPerp = etaiOverEtat * (uv + cosTheta * n);
            var rOutParallel = -MathF.Sqrt(MathF.Abs(1.0f - rOutPerp.LengthSquared())) * n;
            return rOutPerp + rOutParallel;
        }

        internal static Vector3f Min(in Vector3f a, in Vector3f b)
        {
            return new Vector3f(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y), MathF.Min(a.Z, b.Z));
        }

        internal static Vector3f Max(in Vector3f a, in Vector3f b)
        {
            return new Vector3f(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y), MathF.Max(a.Z, b.Z));
        }
    }
}

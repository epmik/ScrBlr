global using Color3f = Scrbl.JaccoBikker.Vector3f;
global using Point3f = Scrbl.JaccoBikker.Vector3f;

namespace Scrbl.JaccoBikker
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly struct Vector3f : IEquatable<Vector3f>
    {
        // Fix: Static properties prevent modification and use default/cached values
        public static Vector3f Zero => default;
        public static Vector3f One => new(1.0f, 1.0f, 1.0);

        // Fix: Init-only properties make the struct safely immutable
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public float x { get { return X; } }
        public float y { get { return Y; } }
        public float z { get { return Z; } }

        // Constructors
        public Vector3f(double v) => (X, Y, Z) = ((float)v, (float)v, (float)v);

        public Vector3f(Vector3d v) => (X, Y, Z) = ((float)v.X, (float)v.Y, (float)v.Z);

        public Vector3f(double x, double y, double z) => (X, Y, Z) = ((float)x, (float)y, (float)z);

        public Vector3f(float v) => (X, Y, Z) = (v, v, v);

        public Vector3f(float x, float y, float z) => (X, Y, Z) = (x, y, z);

        // Indexer (Read-only now)
        public float this[int i] => i switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new ArgumentOutOfRangeException(nameof(i), "Vector index out of bounds!")
        };

        // Unary Operators
        public static Vector3f operator -(Vector3f v) => new(-v.X, -v.Y, -v.Z);

        // Binary Operators
        public static Vector3f operator +(Vector3f u, Vector3f v) => new(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        public static Vector3f operator -(Vector3f u, Vector3f v) => new(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        public static Vector3f operator *(Vector3f u, Vector3f v) => new(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
        public static Vector3f operator *(float t, Vector3f v) => new(t * v.X, t * v.Y, t * v.Z);
        public static Vector3f operator *(Vector3f v, float t) => t * v;
        public static Vector3f operator /(Vector3f v, float t) => (1.0f / t) * v;

        // Instance Methods
        public float LengthSquared() => (X * X) + (Y * Y) + (Z * Z);
        public float Length() => MathF.Sqrt(LengthSquared());

        // Utility Static Functions
        public static float Dot(Vector3f u, Vector3f v) => (u.X * v.X) + (u.Y * v.Y) + (u.Z * v.Z);

        public static Vector3f Cross(Vector3f u, Vector3f v) => new(
            (u.Y * v.Z) - (u.Z * v.Y),
            (u.Z * v.X) - (u.X * v.Z),
            (u.X * v.Y) - (u.Y * v.X)
        );

        public static Vector3f Normalize(Vector3f v) => v / v.Length();

        // Fix: Use standard string formatting or interpolation
        public override string ToString() => $"{X} {Y} {Z}";

        // Fix: Added high-performance Equality overrides
        public bool Equals(Vector3f other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        public override bool Equals(object? obj) => obj is Vector3f other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public static bool operator ==(Vector3f left, Vector3f right) => left.Equals(right);
        public static bool operator !=(Vector3f left, Vector3f right) => !left.Equals(right);

        // Math Functions
        public static bool NearZero(Vector3f v)
        {
            const float s = 1e-8f;
            return (MathF.Abs(v.X) < s) && (MathF.Abs(v.Y) < s) && (MathF.Abs(v.Z) < s);
        }

        public static Vector3f Reflect(Vector3f v, Vector3f n) => v - 2 * Dot(v, n) * n;

        public static Vector3f Refract(Vector3f uv, Vector3f n, float etaiOverEtat)
        {
            var cosTheta = MathF.Min(Dot(-uv, n), 1.0f);
            var rOutPerp = etaiOverEtat * (uv + cosTheta * n);
            var rOutParallel = -MathF.Sqrt(MathF.Abs(1.0f - rOutPerp.LengthSquared())) * n;
            return rOutPerp + rOutParallel;
        }

        internal static Vector3f Min(Vector3f a, Vector3f b)
        {
            return new Vector3f(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y), MathF.Min(a.Z, b.Z));
        }

        internal static Vector3f Max(Vector3f a, Vector3f b)
        {
            return new Vector3f(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y), MathF.Max(a.Z, b.Z));
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

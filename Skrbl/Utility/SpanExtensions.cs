using OpenTK.Mathematics;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Skrbl
{
    public static class SpanExtensions
    {
        public static Span<float> AsSpan(this ref System.Numerics.Vector2 source)
        {
            return MemoryMarshal.CreateSpan(ref source.X, 2);
        }

        public static Span<float> AsSpan(this ref System.Numerics.Vector3 source)
        {
            return MemoryMarshal.CreateSpan(ref source.X, 3);
        }

        public static Span<float> AsSpan(this ref System.Numerics.Vector4 source)
        {
            return MemoryMarshal.CreateSpan(ref source.X, 4);
        }
        public static Span<float> AsSpan(this ref Color4 source)
        {
            return MemoryMarshal.CreateSpan(ref source.R, 4);
        }

        public static System.Numerics.Vector4 ToVector4(this Color4 source)
        {
            return new System.Numerics.Vector4(source.AsSpan());
        }
    }
}
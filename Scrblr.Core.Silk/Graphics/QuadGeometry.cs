using System.Drawing;
using System.Drawing.Imaging;
using StbImageSharp;
using System.IO;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System.Numerics;

namespace Scrblr.Core
{
    // A simple class meant to help create shaders.
    public class QuadGeometry
    {
        public float Width;
        public float Height;

        public Vector3 _translate = Vector3.Zero;

        public Vector3 _scale = Vector3.One;

        public Vector3 _rotate = Vector3.Zero;

        private Vector3 _xyz0 = Vector3.Zero;
        private Vector3 _xyz1 = Vector3.Zero;
        private Vector3 _xyz2 = Vector3.Zero;
        private Vector3 _xyz3 = Vector3.Zero;

        private Vector4 _rgba0 = Vector4.One;
        private Vector4 _rgba1 = Vector4.One;
        private Vector4 _rgba2 = Vector4.One;
        private Vector4 _rgba3 = Vector4.One;

        public QuadGeometry(float size)
        {
            Size(size);
        }

        public QuadGeometry(float width, float height)
        {
            Size(width, height);
        }

        public QuadGeometry Size(float size)
        {
            return Size(size, size);
        }

        public QuadGeometry Size(float width, float height)
        {
            var halfWidth = width * 0.5f;
            var halfHeight = height * 0.5f;

            _xyz0.X = -halfWidth;
            _xyz0.Y = halfHeight;

            _xyz1.X = halfWidth;
            _xyz1.Y = halfHeight;

            _xyz2.X = halfWidth;
            _xyz2.Y = -halfHeight;

            _xyz3.X = -halfWidth;
            _xyz3.Y = -halfHeight;

            return this;
        }

        public QuadGeometry Translate(float x, float y)
        {
            return Translate(x, y, 0f);
        }

        public QuadGeometry Translate(float x, float y, float z)
        {
            _translate.X = x;
            _translate.Y = y;
            _translate.Z = z;

            return this;
        }

        public QuadGeometry Color(float r, float g, float b)
        {
            return Color(r, g, b, 1f);
        }

        public QuadGeometry Color(float r, float g, float b, float a)
        {
            _rgba0.X = r;
            _rgba0.Y = g;
            _rgba0.Z = b;
            _rgba0.W = a;

            _rgba1.X = r;
            _rgba1.Y = g;
            _rgba1.Z = b;
            _rgba1.W = a;

            _rgba2.X = r;
            _rgba2.Y = g;
            _rgba2.Z = b;
            _rgba2.W = a;

            _rgba3.X = r;
            _rgba3.Y = g;
            _rgba3.Z = b;
            _rgba3.W = a;

            return this;
        }

        public void Write()
        {
            Write(new[] { _xyz0.X, _xyz0.Y, _xyz0.Z }, BufferTargetARB.ArrayBuffer);
            Write(new[] { _rgba0.X, _rgba0.Y, _rgba0.Z, _rgba0.W }, BufferTargetARB.ArrayBuffer);
        }

        public unsafe void Write(float[] data, BufferTargetARB target)
        {
            fixed (float* dataPtr = &data[0])
            {
                Context.GL.BufferSubData(target, 0, (nuint)(data.Length * sizeof(float)), dataPtr);

                GLUtility.CheckError();
            }
        }

        //public unsafe void Write<TType>(TType[] data, BufferTargetARB target) where TType : unmanaged
        //{
        //    fixed (TType* dataPtr = &data[0])
        //    {
        //        var elementSizeInBytes = sizeof(TType);

        //        Context.GL.BufferSubData(target, 0, (nuint)(data.Length * elementSizeInBytes), dataPtr);

        //        GLUtility.CheckError();
        //    }
        //}
    }
}
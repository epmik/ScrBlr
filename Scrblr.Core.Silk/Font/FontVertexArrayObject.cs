using Silk.NET.OpenGL;
using System;

namespace Scrblr.Core
{
    public class FontVertexArrayObject : IDisposable
    {
        private readonly uint _handle;
        private readonly int _stride;

        public FontVertexArrayObject(int stride)
        {
            if (stride <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stride));
            }

            _stride = stride;

            Context.GL.GenVertexArrays(1, out _handle);
            GLUtility.CheckError();
        }

        public void Dispose()
        {
            Context.GL.DeleteVertexArray(_handle);
            GLUtility.CheckError();
        }

        public void Bind()
        {
            Context.GL.BindVertexArray(_handle);
            GLUtility.CheckError();
        }

        public unsafe void VertexAttribPointer(int location, int size, VertexAttribPointerType type, bool normalized, int offset)
        {
            Context.GL.EnableVertexAttribArray((uint)location);
            Context.GL.VertexAttribPointer((uint)location, size, type, normalized, (uint)_stride, (void*)offset);
            GLUtility.CheckError();
        }
    }
}

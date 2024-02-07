using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace Scrblr.Core
{
    public class FontBufferObject<T> : IDisposable where T : unmanaged
    {
        private readonly uint _handle;
        private readonly BufferTargetARB _bufferType;
        private readonly int _size;

        public unsafe FontBufferObject(int size, BufferTargetARB bufferType, bool isDynamic)
        {
            _bufferType = bufferType;
            _size = size;

            _handle = Context.GL.GenBuffer();
            GLUtility.CheckError();

            Bind();

            var elementSizeInBytes = Marshal.SizeOf<T>();
            Context.GL.BufferData(bufferType, (nuint)(size * elementSizeInBytes), null, isDynamic ? BufferUsageARB.StreamDraw : BufferUsageARB.StaticDraw);
            GLUtility.CheckError();
        }

        public void Bind()
        {
            Context.GL.BindBuffer(_bufferType, _handle);
            GLUtility.CheckError();
        }

        public void Dispose()
        {
            Context.GL.DeleteBuffer(_handle);
            GLUtility.CheckError();
        }

        public unsafe void SetData(T[] data, int startIndex, int elementCount)
        {
            Bind();

            fixed (T* dataPtr = &data[startIndex])
            {
                var elementSizeInBytes = sizeof(T);

                Context.GL.BufferSubData(_bufferType, 0, (nuint)(elementCount * elementSizeInBytes), dataPtr);
                GLUtility.CheckError();
            }
        }
    }
}

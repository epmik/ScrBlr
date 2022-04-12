using OpenTK.Graphics.OpenGL4;
using System;

namespace Scrblr.Core
{

    public enum VertexBufferType20220316
    {
        StaticDraw = BufferUsageHint.StaticDraw,
        DynamicDraw = BufferUsageHint.DynamicDraw,
    }

    [Flags]
    public enum VertexDataType20220316
    {
        None = 0,
        Position = 1,
        Color = 2,
        // 4
        // 8
        // 16
        // 32
    }

    public class VertexBuffer20220316<T> : IDisposable where T : struct
    {
        public int BufferHandle;

        public int ArrayHandle;
        public int TypeSize { get; private set; }
        public int BufferSize { get; private set; }
        public int UsedCount { get; private set; }
        public int BufferStride { get; private set; }
        public VertexDataType20220316 VertexDataType { get; private set; }
        public VertexBufferType20220316 VertexBufferType { get; private set; }

        public VertexBuffer20220316(
            int count,
            VertexDataType20220316 vertexDataType,
            VertexBufferType20220316 vertexBufferType)
        {
            VertexDataType = vertexDataType;
            VertexBufferType = vertexBufferType;
            TypeSize = TypeSize<T>.Size;
            BufferStride = Stride(vertexDataType);
            BufferSize = count * BufferStride;

            BufferHandle = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferHandle);

            GL.BufferData(BufferTarget.ArrayBuffer, BufferSize, IntPtr.Zero, (BufferUsageHint)vertexBufferType);

            ArrayHandle = GL.GenVertexArray();

            GL.BindVertexArray(ArrayHandle);

            var offset = 0;

            if (vertexDataType.HasFlag(VertexDataType20220316.Position))
            {
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, BufferStride, offset);
                offset += 3 * TypeSize;
            }

            if (vertexDataType.HasFlag(VertexDataType20220316.Color))
            {
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, BufferStride, offset);
                offset += 4 * TypeSize;
            }
        }

        private int Stride(VertexDataType20220316 vertexDataType)
        {
            var stride = 0;

            if (vertexDataType.HasFlag(VertexDataType20220316.Position))
            {
                stride += TypeSize * 3;
            }

            if (vertexDataType.HasFlag(VertexDataType20220316.Color))
            {
                stride += TypeSize * 4;
            }

            return stride;
        }

        public void Write(ref T[] data)
        {
            var size = data.Length * TypeSize;

            if (UsedCount + size > BufferSize)
            {
                throw new Exception("VertexBuffer<T>.Write(ref T[] data) failed. The VertexBuffer isn't large enough to hold this data.");
            }

            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedCount, size, data);

            UsedCount += size;
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferHandle);

            GL.BindVertexArray(ArrayHandle);
        }

        public void UnBind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void EnableArray(VertexDataType20220316 vertexDataType)
        {
            if (vertexDataType.HasFlag(VertexDataType20220316.Position))
            {
                GL.EnableVertexAttribArray(0);
            }

            if (vertexDataType.HasFlag(VertexDataType20220316.Color))
            {
                GL.EnableVertexAttribArray(1);
            }
        }

        public void DisableArray(VertexDataType20220316 vertexDataType)
        {
            if (vertexDataType.HasFlag(VertexDataType20220316.Position))
            {
                GL.DisableVertexAttribArray(0);
            }

            if (vertexDataType.HasFlag(VertexDataType20220316.Color))
            {
                GL.DisableVertexAttribArray(1);
            }
        }

        public void Dispose()
        {
            UnBind();
            GL.DeleteBuffer(BufferHandle);
        }
    }
}
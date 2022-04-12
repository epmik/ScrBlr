using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public enum VertexBufferUsage
    {
        StaticDraw = BufferUsageHint.StaticDraw,
        DynamicDraw = BufferUsageHint.DynamicDraw,
    }

    public class VertexBuffer : IDisposable
    {
        #region Fields and Properties

        public int Handle;
        
        public int TotalBytes { get; private set; }

        public int UsedBytes { get; private set; }

        public VertexBufferUsage VertexBufferUsage { get; private set; }

        public VertexBufferLayout Layout { get; private set; }

        #endregion Fields and Properties

        #region Constructors

        /// <summary>
        /// <paramref name="elementCount"/> is not the buffer size in bytes, it is the number of vertices the buffer can hold. 
        /// So the byte size of the buffer is the size of an element (defined by <paramref name="parts"/>) times <paramref name="elementCount"/>
        /// </summary>
        /// <param name="elementCount"></param>
        /// <param name="parts"></param>
        /// <param name="vertexBufferType"></param>
        public VertexBuffer(
            int elementCount,
            IEnumerable<VertexBufferLayout.Part> parts,
            VertexBufferUsage vertexBufferType)
            : this(elementCount, new VertexBufferLayout(parts), vertexBufferType) 
        {

        }

        /// <summary>
        /// <paramref name="elementCount"/> is not the buffer size in bytes, it is the number of vertices the buffer can hold. 
        /// So the byte size of the buffer is the size of an element (defined by <paramref name="parts"/>) times <paramref name="elementCount"/>
        /// </summary>
        /// <param name="elementCount"></param>
        /// <param name="layout"></param>
        /// <param name="vertexBufferUsage"></param>
        public VertexBuffer(
            int elementCount,
            VertexBufferLayout layout,
            VertexBufferUsage vertexBufferUsage)
        {
            Layout = layout;
            VertexBufferUsage = vertexBufferUsage;
            TotalBytes = elementCount * Layout.Stride;

            Handle = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);

            GL.BufferData(BufferTarget.ArrayBuffer, TotalBytes, IntPtr.Zero, (BufferUsageHint)VertexBufferUsage);

            Layout.Handle = GL.GenVertexArray();

            GL.BindVertexArray(Layout.Handle);

            var offset = 0;

            foreach (var part in Layout.Parts)
            {
                GL.VertexAttribPointer(part.ShaderLocation, part.Count, (VertexAttribPointerType)part.Type, false, Layout.Stride, offset);
                offset += part.Stride;
            }

            EnableElements();
        }

        #endregion Constructors

        public int UsedElements()
        {
            return (int)Math.Ceiling((float)UsedBytes / (float)Layout.Stride);
        }

        public int TotelElements()
        {
            return TotalBytes / Layout.Stride;
        }

        public bool CanWriteElements(int count)
        {
            return UsedElements() + count < TotelElements();
        }

        public void Clear()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);

            //GL.BufferData(BufferTarget.ArrayBuffer, BufferSize, IntPtr.Zero, (BufferUsageHint)VertexBufferUsage);

            //GL.ClearBufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedCount, size, data);

            GL.InvalidateBufferData(Handle);

            UsedBytes = 0;
        }

        public void Write<T>(ref T[] data) where T : struct
        {
            var size = data.Length * TypeSize<T>.Size;

            if(UsedBytes + size > TotalBytes)
            {
                throw new Exception("VertexBuffer<T>.Write(ref T[] data) failed. The VertexBuffer isn't large enough to hold this data.");
            }

            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedBytes, size, data);

            UsedBytes += size;
        }

        public void Write<T>(T[] data) where T : struct
        {
            Write(ref data);
        }

        public void Write(Color4 data)
        {
            Write(new[] { data.R, data.G, data.B, data.A });
        }

        public void Write(Vector3 data)
        {
            Write(new[] { data.X, data.Y, data.Z });
        }

        public void Write(Vector4 data)
        {
            Write(new[] { data.X, data.Y, data.Z, data.W });
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);

            GL.BindVertexArray(Layout.Handle);
        }

        public void UnBind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void EnableElements()
        {
            foreach(var e in Layout.Parts)
            {
                EnableElement(e.Identifier);
            }
        }

        public void DisableElements()
        {
            foreach (var e in Layout.Parts)
            {
                DisableElement(e.Identifier);
            }
        }

        public void EnableElement(VertexBufferLayout.PartIdentifier identifier)
        {
            GL.BindVertexArray(Layout.Handle);

            var e = Layout.Parts.SingleOrDefault(o => o.Identifier == identifier);

            if(e == null)
            {
                throw new InvalidOperationException($"EnableElement(VertexBufferLayout.ElementIdentifier) failed. The element {identifier} could not be found.");
            }
            
            GL.EnableVertexAttribArray(e.ShaderLocation);
        }

        public void DisableElement(VertexBufferLayout.PartIdentifier identifier)
        {
            GL.BindVertexArray(Layout.Handle);

            var e = Layout.Parts.SingleOrDefault(o => o.Identifier == identifier);

            if (e == null)
            {
                throw new InvalidOperationException($"DisableElement(VertexBufferLayout.ElementIdentifier) failed. The element {identifier} could not be found.");
            }

            GL.DisableVertexAttribArray(e.ShaderLocation);
        }

        public void Dispose()
        {
            UnBind();
            
            GL.DeleteBuffer(Handle);
            
            GC.SuppressFinalize(this);
        }
    }
}
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Scrblr.Core
{
    public class VertexBuffer : IDisposable
    {
        #region Fields and Properties

        public int Handle;
        
        public int TotalBytes { get; private set; }

        public int UsedBytes { get; private set; }

        public VertexBufferUsage VertexBufferUsage { get; private set; }

        public VertexMapping Mapping { get; private set; }

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
            VertexMapping.Map[] parts,
            VertexBufferUsage vertexBufferType)
            : this(elementCount, new VertexMapping(parts), vertexBufferType) 
        {

        }

        /// <summary>
        /// <paramref name="elementCount"/> is not the buffer size in bytes, it is the number of vertices the buffer can hold. 
        /// So the byte size of the buffer is the size of an element (defined by <paramref name="parts"/>) times <paramref name="elementCount"/>
        /// </summary>
        /// <param name="elementCount"></param>
        /// <param name="mapping"></param>
        /// <param name="vertexBufferUsage"></param>
        public VertexBuffer(
            int elementCount,
            VertexMapping mapping,
            VertexBufferUsage vertexBufferUsage)
        {
            Mapping = mapping;
            VertexBufferUsage = vertexBufferUsage;
            TotalBytes = elementCount * Mapping.Stride;

            Handle = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);

            GL.BufferData(BufferTarget.ArrayBuffer, TotalBytes, IntPtr.Zero, (BufferUsageHint)VertexBufferUsage);

            Mapping.Handle = GL.GenVertexArray();

            GL.BindVertexArray(Mapping.Handle);

            var offset = 0;
            var location = 0;

            foreach (var part in Mapping.Maps)
            {
                GL.VertexAttribPointer(location++, part.Count, (VertexAttribPointerType)part.ElementType, false, Mapping.Stride, offset);

                offset += part.Stride;
            }
        }

        #endregion Constructors

        //public VertexBufferWriter Writer()
        //{
        //    return new VertexBufferWriter(this);
        //}

        public VertexFlag VertexFlags
        {
            get { return Mapping.VertexFlags; }
        }

        public string StandardShaderDictionaryKey()
        {
            return VertexFlags.StandardShaderDictionaryKey();
        }

        public int UsedElements()
        {
            return (int)Math.Ceiling((float)UsedBytes / (float)Mapping.Stride);
        }

        public int TotelElements()
        {
            return TotalBytes / Mapping.Stride;
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

        public void WriteRaw(ref float[] data)
        {
            var size = data.Length * sizeof(float);

            //var count = (float)size / (float)Mapping.Stride;
            //var decimalPart = count % 1;

            //if(decimalPart != 0)
            //{
            //    throw new Exception("Write<T>(ref T[] data) failed. The provided data must be a multiple of the Stride");
            //}

            if (UsedBytes + size > TotalBytes)
            {
                throw new Exception("VertexBuffer<T>.Write(ref T[] data) failed. The VertexBuffer isn't large enough to hold this data.");
            }

            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedBytes, size, data);

            UsedBytes += size;
        }

        public void WriteRaw(params float[] data)
        {
            WriteRaw(ref data);
        }

        //public void Write(ref Color4 data)
        //{
        //    Write(data.R, data.G, data.B, data.A);
        //}

        //public void Write(ref Vector2 data)
        //{
        //    Write(data.X, data.Y);
        //}

        //public void Write(ref Vector3 data)
        //{
        //    Write(data.X, data.Y, data.Z);
        //}

        //public void Write(ref Vector4 data)
        //{
        //    Write(data.X, data.Y, data.Z, data.W );
        //}

        private int _currentWriteMapIndex;

        public void WriteFixed(VertexFlag vertexFlag, ref float[] data)
        {
            if(!Mapping.VertexFlags.HasFlag(vertexFlag))
            {
                return;
            }

            if(Mapping.Maps[_currentWriteMapIndex].VertexFlag == vertexFlag)
            {
                WriteRaw(ref data);

                IncrementCurrentWriteMapIndex();

                return;
            }

            WriteDefaultValuesUntil(vertexFlag);

            WriteFixed(vertexFlag, ref data);
        }

        public void WriteFixed(VertexFlag vertexFlag, params float[] data)
        {
            WriteFixed(vertexFlag, ref data);
        }

        public void WriteDefaultValuesUntil(VertexFlag vertexFlag)
        {
            if(Mapping.Maps[_currentWriteMapIndex].VertexFlag == vertexFlag)
            {
                return;
            }

            WriteRaw(DefaultWriteValues(Mapping.Maps[_currentWriteMapIndex]));

            IncrementCurrentWriteMapIndex();

            WriteDefaultValuesUntil(vertexFlag);
        }

        private static readonly float[] DefaultColorValue3 = new float[] { 1f, 1f, 1f };
        private static readonly float[] DefaultColorValue4 = new float[] { 1f, 1f, 1f, 1f };
        private static readonly float[] DefaultVectorValue2 = new float[] { 0f, 0f };
        private static readonly float[] DefaultVectorValue3 = new float[] { 0f, 0f, 0f };
        private static readonly float[] DefaultVectorValue4 = new float[] { 0f, 0f, 0f, 1f };

        private float[] DefaultWriteValues(VertexMapping.Map map)
        {
            switch (map.VertexFlag)
            {
                case VertexFlag.Color0:
                case VertexFlag.Color1:
                case VertexFlag.Color2:
                case VertexFlag.Color3:
                    return map.Count == 3 ? DefaultColorValue3 : DefaultColorValue4;
                case VertexFlag.Position0:
                case VertexFlag.Position1:
                case VertexFlag.Position2:
                case VertexFlag.Position3:
                case VertexFlag.Normal0:
                case VertexFlag.Normal1:
                case VertexFlag.Uv0:
                case VertexFlag.Uv1:
                case VertexFlag.Uv2:
                case VertexFlag.Uv3:
                case VertexFlag.Uv4:
                case VertexFlag.Uv5:
                case VertexFlag.Uv6:
                case VertexFlag.Uv7:
                case VertexFlag.Attr0:
                case VertexFlag.Attr1:
                case VertexFlag.Attr2:
                case VertexFlag.Attr3:
                case VertexFlag.Attr4:
                case VertexFlag.Attr5:
                case VertexFlag.Attr6:
                case VertexFlag.Attr7:
                    return map.Count == 3
                        ? DefaultVectorValue3
                        : map.Count == 4
                            ? DefaultVectorValue4
                            : DefaultVectorValue2;
                default:
                    throw new InvalidOperationException($"");
            }
        }

        private void IncrementCurrentWriteMapIndex()
        {
            _currentWriteMapIndex++;

            if (_currentWriteMapIndex == Mapping.Maps.Length)
            {
                _currentWriteMapIndex = 0;
            }
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);

            GL.BindVertexArray(Mapping.Handle);
        }

        public void UnBind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void EnableElements(Shader shader)
        {
            //GL.BindVertexArray(Mapping.Handle);

            //foreach (var e in Mapping.Maps)
            //{
            //    if (!shader.TryAttributeLocation(e.ShaderInputName, out int location))
            //    {
            //        continue;
            //    }

            //    if (e.Enabled && VertexFlags(true).HasFlag(e.VertexFlag))
            //    {
            //        GL.EnableVertexAttribArray(location);
            //    }
            //}
        }

        public void EnableElements(VertexFlag vertexFlags)
        {
            GL.BindVertexArray(Mapping.Handle);

            DisableElements();

            var location = 0;

            foreach (var map in Mapping.Maps)
            {
                if(!vertexFlags.HasFlag(map.VertexFlag))
                {
                    //GL.DisableVertexAttribArray(location++);

                    continue;
                }

                GL.EnableVertexAttribArray(location++);
            }
        }

        //public void ToggleElements(Shader shader)
        //{
        //    ToggleElements(shader, VertexFlags);
        //}

        //public VertexFlag EnabledVertexFlags { get; private set; }

        //public void ToggleElements(Shader shader, VertexFlag vertexFlags)
        //{
        //    if(EnabledVertexFlags == vertexFlags)
        //    {
        //        return;
        //    }

        //    GL.BindVertexArray(Mapping.Handle);

        //    EnabledVertexFlags = VertexFlag.None;
        //}

        public void DisableElements()
        {
            for (var location = 0; location < Mapping.Maps.Length; location++)
            {
                GL.DisableVertexAttribArray(location);
            }
        }

        //public void EnableElement(VertexFlag vertexFlag)
        //{
        //    GL.BindVertexArray(Layout.Handle);

        //    var e = Layout.Parts.SingleOrDefault(o => o.VertexFlag == vertexFlag);

        //    if (e == null)
        //    {
        //        throw new InvalidOperationException($"EnableElement(VertexBufferLayout.ElementIdentifier) failed. The element {vertexFlag} could not be found.");
        //    }

        //    GL.EnableVertexAttribArray(e.ShaderLocation);
        //}

        //public void DisableElement(VertexFlag identifier)
        //{
        //    GL.BindVertexArray(Layout.Handle);

        //    var e = Layout.Parts.SingleOrDefault(o => o.Identifier == identifier);

        //    if (e == null)
        //    {
        //        throw new InvalidOperationException($"DisableElement(VertexBufferLayout.ElementIdentifier) failed. The element {identifier} could not be found.");
        //    }

        //    GL.DisableVertexAttribArray(e.ShaderLocation);
        //}

        public void Dispose()
        {
            UnBind();
            
            GL.DeleteBuffer(Handle);
            
            GC.SuppressFinalize(this);
        }
    }
}
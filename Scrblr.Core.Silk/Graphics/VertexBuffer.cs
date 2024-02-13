using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Scrblr.Core
{

    public class VertexBuffer : VertexBuffer<float>
    {
        public VertexBuffer(
            uint count,
            VertexFlag flags,
            VertexBufferUsage usage = VertexBufferUsage.Dynamic)
            : base(count, flags, usage)
        {

        }
    }

    public class VertexBuffer<TElementType> : IDisposable where TElementType : unmanaged
    {
        #region Fields and Properties

        public uint Handle;

        private uint _elementSize;

        private uint _vertexArrayHandle;

        public uint TotalBytes { get; private set; }

        public uint UsedBytes { get; private set; }

        public VertexBufferUsage VertexBufferUsage { get; private set; }

        public static VertexFlag[] AllVertexFlags = null;

        private uint? _stride = null;

        public uint Stride
        {
            get
            {
                if(_stride == null)
                {
                    _stride = CalculateStride(VertexFlags);
                }

                return _stride.Value;
            }
        }

        public VertexFlag VertexFlags
        {
            get;
            private set;
        }

        #endregion Fields and Properties

        #region Constructors

        static VertexBuffer()
        {
            AllVertexFlags = Enum.GetValues<VertexFlag>();
        }

        /// <summary>
        /// <paramref name="elementCount"/> is not the buffer size in bytes, it is the number of vertices the buffer can hold. 
        /// So the byte size of the buffer is the size of an element (defined by <paramref name="parts"/>) times <paramref name="elementCount"/>
        /// </summary>
        /// <param name="elementCount"></param>
        /// <param name="parts"></param>
        /// <param name="vertexBufferType"></param>
        public unsafe VertexBuffer(
            uint count,
            VertexFlag flags,
            VertexBufferUsage usage = VertexBufferUsage.Dynamic)
        {

            VertexFlags = flags;
            VertexBufferUsage = usage;
            _elementSize = (uint)Marshal.SizeOf<TElementType>();
            TotalBytes = count * Stride;

            Handle = Context.GL.GenBuffer();

            Context.GL.BindBuffer(GLEnum.ArrayBuffer, Handle);

            Context.GL.BufferData(GLEnum.ArrayBuffer, TotalBytes, IntPtr.Zero, (GLEnum)VertexBufferUsage);

            _vertexArrayHandle = Context.GL.GenVertexArray();

            Context.GL.BindVertexArray(_vertexArrayHandle);

            uint offset = 0;
            uint location = 0;

            var vertexFlagArray = AllVertexFlags.Where(o => VertexFlags.HasFlag(o)).ToArray();

            foreach (var vertexFlag in vertexFlagArray)
            {
                Context.GL.VertexAttribPointer(location++, (int)vertexFlag.ElementCount(), GLEnum.Float, false, Stride, (void*)offset);

                offset += CalculateStride(vertexFlag);
            }
        }

        #endregion Constructors

        private uint CalculateStride(VertexFlag flags)
        {
            uint stride = 0;

            foreach (var v in AllVertexFlags)
            {
                if(flags.HasFlag(v))
                {
                    stride += _elementSize * v.ElementCount();
                }
            }

            return stride;
        }

        //public VertexBufferWriter Writer()
        //{
        //    return new VertexBufferWriter(this);
        //}

        //public int UsedElements()
        //{
        //    return (int)Math.Ceiling((float)UsedBytes / (float)Mapping.Stride);
        //}

        //public int TotelElements()
        //{
        //    return TotalBytes / Mapping.Stride;
        //}

        //public bool CanWriteElements(int count)
        //{
        //    return UsedElements() + count < TotelElements();
        //}

        public void Invalidate()
        {
            Context.GL.BindBuffer(GLEnum.ArrayBuffer, Handle);

            //GL.BufferData(BufferTarget.ArrayBuffer, BufferSize, IntPtr.Zero, (BufferUsageHint)VertexBufferUsage);

            //GL.ClearBufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedCount, size, data);

            Context.GL.InvalidateBufferData(Handle);

            UsedBytes = 0;
        }

        public unsafe void Write(ref TElementType[] data)
        {
            var size = (uint)(data.Length * sizeof(TElementType));

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

            fixed (TElementType* ptr = &data[0])
            {
                Context.GL.BufferSubData(GLEnum.ArrayBuffer, (nint)UsedBytes, (nuint)(data.Length * _elementSize), ptr);
            }

            UsedBytes += size;
        }

        public unsafe void Write(VertexFlag vertexFlag, ref TElementType[] data)
        {

        }

        //public void WriteRaw(ref float[] data)
        //{
        //    var size = data.Length * sizeof(float);

        //    //var count = (float)size / (float)Mapping.Stride;
        //    //var decimalPart = count % 1;

        //    //if(decimalPart != 0)
        //    //{
        //    //    throw new Exception("Write<T>(ref T[] data) failed. The provided data must be a multiple of the Stride");
        //    //}

        //    if (UsedBytes + size > TotalBytes)
        //    {
        //        throw new Exception("VertexBuffer<T>.Write(ref T[] data) failed. The VertexBuffer isn't large enough to hold this data.");
        //    }

        //    GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)UsedBytes, size, data);

        //    UsedBytes += size;
        //}

        //public void WriteRaw(params float[] data)
        //{
        //    WriteRaw(ref data);
        //}

        //private int _currentWriteMapIndex;

        //public void WriteFixed(VertexFlag vertexFlag, ref float[] data)
        //{
        //    if (!Mapping.VertexFlags.HasFlag(vertexFlag))
        //    {
        //        return;
        //    }

        //    if (Mapping.Maps[_currentWriteMapIndex].VertexFlag == vertexFlag)
        //    {
        //        WriteRaw(ref data);

        //        IncrementCurrentWriteMapIndex();

        //        return;
        //    }

        //    WriteDefaultValuesUntil(vertexFlag);

        //    WriteFixed(vertexFlag, ref data);
        //}

        //public void WriteFixed(VertexFlag vertexFlag, params float[] data)
        //{
        //    WriteFixed(vertexFlag, ref data);
        //}

        //public void WriteDefaultValuesUntil(VertexFlag vertexFlag)
        //{
        //    if (Mapping.Maps[_currentWriteMapIndex].VertexFlag == vertexFlag)
        //    {
        //        return;
        //    }

        //    WriteRaw(DefaultWriteValues(Mapping.Maps[_currentWriteMapIndex]));

        //    IncrementCurrentWriteMapIndex();

        //    WriteDefaultValuesUntil(vertexFlag);
        //}

        //private static readonly float[] DefaultColorValue3 = new float[] { 1f, 1f, 1f };
        //private static readonly float[] DefaultColorValue4 = new float[] { 1f, 1f, 1f, 1f };
        //private static readonly float[] DefaultVectorValue2 = new float[] { 0f, 0f };
        //private static readonly float[] DefaultVectorValue3 = new float[] { 0f, 0f, 0f };
        //private static readonly float[] DefaultVectorValue4 = new float[] { 0f, 0f, 0f, 1f };

        //private float[] DefaultWriteValues(VertexMapping.Map map)
        //{
        //    switch (map.VertexFlag)
        //    {
        //        case VertexFlag.Color0:
        //        case VertexFlag.Color1:
        //        case VertexFlag.Color2:
        //        case VertexFlag.Color3:
        //            return map.Count == 3 ? DefaultColorValue3 : DefaultColorValue4;
        //        case VertexFlag.Position0:
        //        case VertexFlag.Position1:
        //        case VertexFlag.Position2:
        //        case VertexFlag.Position3:
        //        case VertexFlag.Normal0:
        //        case VertexFlag.Normal1:
        //        case VertexFlag.Uv0:
        //        case VertexFlag.Uv1:
        //        case VertexFlag.Uv2:
        //        case VertexFlag.Uv3:
        //        case VertexFlag.Uv4:
        //        case VertexFlag.Uv5:
        //        case VertexFlag.Uv6:
        //        case VertexFlag.Uv7:
        //        case VertexFlag.Attr0:
        //        case VertexFlag.Attr1:
        //        case VertexFlag.Attr2:
        //        case VertexFlag.Attr3:
        //        case VertexFlag.Attr4:
        //        case VertexFlag.Attr5:
        //        case VertexFlag.Attr6:
        //        case VertexFlag.Attr7:
        //            return map.Count == 3
        //                ? DefaultVectorValue3
        //                : map.Count == 4
        //                    ? DefaultVectorValue4
        //                    : DefaultVectorValue2;
        //        default:
        //            throw new InvalidOperationException($"");
        //    }
        //}

        //private void IncrementCurrentWriteMapIndex()
        //{
        //    _currentWriteMapIndex++;

        //    if (_currentWriteMapIndex == Mapping.Maps.Length)
        //    {
        //        _currentWriteMapIndex = 0;
        //    }
        //}

        public void Bind()
        {
            Context.GL.BindBuffer(GLEnum.ArrayBuffer, Handle);

            Context.GL.BindVertexArray(_vertexArrayHandle);
        }

        public void UnBind()
        {
            Context.GL.BindBuffer(GLEnum.ArrayBuffer, 0);
        }


        public void EnableElements()
        {
            Bind();

            uint location = 0;

            foreach (var flag in AllVertexFlags)
            {
                if (VertexFlags.HasFlag(flag))
                {
                    Context.GL.EnableVertexAttribArray(location++);
                }
                else
                {
                    Context.GL.DisableVertexAttribArray(location++);
                }
            }
        }

        public void DisableElements()
        {
            uint location = 0;

            foreach (var flag in AllVertexFlags)
            {
                Context.GL.DisableVertexAttribArray(location++);
            }
        }

        public void Dispose()
        {
            UnBind();

            Context.GL.DeleteBuffer(Handle);

            GC.SuppressFinalize(this);
        }

        //public override string ToString()
        //{
        //    return VertexFlags.StandardShaderDictionaryKey();
        //}
    }
}
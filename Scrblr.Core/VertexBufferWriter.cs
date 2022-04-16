using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public class VertexBufferWriter : IDisposable
    {
        #region Fields and Properties

        private VertexBuffer _vertexBuffer;
        
        #endregion Fields and Properties

        #region Constructors

        /// <summary>
        /// <paramref name="elementCount"/> is not the buffer size in bytes, it is the number of vertices the buffer can hold. 
        /// So the byte size of the buffer is the size of an element (defined by <paramref name="parts"/>) times <paramref name="elementCount"/>
        /// </summary>
        /// <param name="elementCount"></param>
        /// <param name="parts"></param>
        /// <param name="vertexBufferType"></param>
        public VertexBufferWriter(VertexBuffer vertexBuffer) 
        {
            _vertexBuffer = vertexBuffer;
        }

        #endregion Constructors

        public VertexBufferWriter Write(VertexFlag vertexFlag, params float[] data)
        {

        }

        public VertexBufferWriter Write(VertexFlag vertexFlag, ref float[] data)
        {

        }

        private const int InvalidLockPosition = -1;

        private int _vertexLockedPosition = InvalidLockPosition;

        public void LockVertex()
        {
            _vertexLockedPosition = _vertexBuffer.UsedBytes;
        }

        public void NextVertex()
        {
            Flush();

            _vertexLockedPosition = _vertexBuffer.UsedBytes;
        }

        public void Flush()
        {
            if(_vertexLockedPosition != InvalidLockPosition)
            {
                _vertexBuffer.Write();

                _vertexLockedPosition = InvalidLockPosition;
            }
        }

        public void Dispose()
        {
            Flush();
        }
    }
}
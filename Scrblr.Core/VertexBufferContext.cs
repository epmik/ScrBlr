using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scrblr.Core
{
    public class VertexBufferContext : IDisposable
    {
        public VertexBufferUsage VertexBufferUsage { get; private set; }

        public VertexBufferLayout Layout { get; private set; }

        private VertexBuffer _vertexBuffer;

        #region Constructors

        public VertexBufferContext(
            IEnumerable<VertexBufferLayout.Part> parts,
            VertexBufferUsage vertexBufferType = VertexBufferUsage.DynamicDraw)
            : this(1024 * 1024, parts, vertexBufferType)
        {

        }

        public VertexBufferContext(
            int count,
            IEnumerable<VertexBufferLayout.Part> parts,
            VertexBufferUsage vertexBufferType = VertexBufferUsage.DynamicDraw)
            : this(count, new VertexBufferLayout(parts), vertexBufferType)
        {

        }

        public VertexBufferContext(
            int count,
            VertexBufferLayout layout,
            VertexBufferUsage vertexBufferUsage = VertexBufferUsage.DynamicDraw)
        {
            Layout = layout;
            VertexBufferUsage = vertexBufferUsage;

            _vertexBuffer = new VertexBuffer(count, Layout, VertexBufferUsage);
        }

        #endregion Constructors

        public void FetchVertexBuffer()
        {

        }

        #region Dispose

        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _vertexBuffer = null;

            GC.SuppressFinalize(this);
        }

        #endregion Dispose
    }
}

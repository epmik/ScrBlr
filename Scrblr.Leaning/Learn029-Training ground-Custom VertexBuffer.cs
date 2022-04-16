using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn029-Training ground-Custom VertexBuffer")]
    public class Learn029 : AbstractSketch
    {
        private VertexBuffer _vertexBuffer;

        private float[] _vertices =
        {
            // Position             // colors             
             0.5f, -0.5f, 0.0f,     1.0f, 0.0f, 0.0f, 1.0f, // bottom right
             0.5f,  0.5f, 0.0f,     0.0f, 0.0f, 1.0f, 1.0f, // top right
            -0.5f, -0.5f, 0.0f,     0.0f, 1.0f, 0.0f, 1.0f, // bottom left
            -0.5f,  0.5f, 0.0f,     0.0f, 0.0f, 1.0f, 1.0f, // top left
        };

        private VertexFlag _vertexFlags = VertexFlag.Position0 | VertexFlag.Color0;

        public Learn029()
            : base(600, 600)
        {
        }

        public void Load()
        {
            _vertexBuffer = Graphics.CreateVertexBuffer();
        }

        public void Render()
        {
            Graphics.ActiveVertexBuffer(_vertexBuffer);
            Graphics.Quad();
        }
    }
}

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn027-Playgrounds-UltraCompact")]
    public class Learn027 : AbstractSketch
    {
        public Learn027()
            : base(600, 600)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
        }

        //private float[] _vertices =
        //{
        //    // Position             // colors               // texture
        //     0.5f, -0.5f, 0.0f,     1.0f, 0.0f, 0.0f, 1.0f,       1.0f, 0.0f, // bottom right
        //     0.5f,  0.5f, 0.0f,     0.0f, 0.0f, 1.0f, 1.0f,       1.0f, 1.0f, // top right
        //    -0.5f, -0.5f, 0.0f,     0.0f, 1.0f, 0.0f, 1.0f,       0.0f, 0.0f, // bottom left
        //    -0.5f,  0.5f, 0.0f,     0.0f, 0.0f, 1.0f, 1.0f,       0.0f, 1.0f, // top left
        //};

        private Texture _texture;

        //private VertexFlag _vertexFlags = VertexFlag.Position0 | VertexFlag.Color0 | VertexFlag.Uv0;

        public void Load()
        {
            //Graphics.CreateVertexBuffer(_vertexFlags);

            //Graphics.WriteVertexBuffer(_vertexFlags, _vertices);

            _texture = new Texture("resources/textures/orange-transparent-1024x1024.png");

            Graphics.ClearColor(1f, 1f, 1f, 1.0f);

            Graphics.Enable(EnableFlag.DepthTest);
        }

        public void UnLoad()
        {
            _texture.Dispose();
        }

        public void Render()
        {
            //Graphics.EnableVertexBuffer(_vertexFlags);

            //Graphics.Scale(0.5f);
            //Graphics.Rotate(45f, Axis.Y);
            //Graphics.Translate(0f, 0f, -20f);

            //Graphics.Render(PrimitiveType.TriangleStrip, 0, 4);

            Graphics.Quad();
        }

        public void Update()
        {
        }

        public void Resize(ResizeEventArgs a)
        {
        }

        public void MouseWheel(MouseWheelEventArgs a)
        {
        }
    }
}

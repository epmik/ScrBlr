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
    [Sketch(Name = "Learn027-Training ground")]
    public class Learn027 : AbstractSketch
    {
        public Learn027()
            : base(600, 600)
        {
        }

        public void Render()
        {
            Graphics.Quad();




            //var shader = Graphics.StandardShader(VertexFlag.Position0 | VertexFlag.Color0);

            //shader.Use();

            //var vertexBuffer = Graphics.StandardVertexBuffer();

            //vertexBuffer.Bind();

            //vertexBuffer.EnableElements(VertexFlag.Position0 | VertexFlag.Color0);

            //shader.Uniform("uModelMatrix", Matrix4.CreateTranslation(0, 0, -4f));
            //shader.Uniform("uViewMatrix", Graphics.ActiveCamera().ViewMatrix());
            //shader.Uniform("uProjectionMatrix", Graphics.ActiveCamera().ProjectionMatrix());

            //GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }
    }
}

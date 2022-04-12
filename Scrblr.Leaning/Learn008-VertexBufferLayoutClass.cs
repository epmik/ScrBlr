using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn008-VertexBuffer-VertexBufferLayout-Class")]
    public class Learn008 : AbstractSketch20220317
    {
        public Learn008()
            : base(12, 12)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;
        }

        private VertexBuffer _vertexBuffer;
        private Shader _shader;

        private float[] _positionColor1 =
        {
             // positions        // colors
             1f,  0f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,    // bottom right
             0f,  0f, 0.0f,  0.0f, 1.0f, 0.0f,  1.0f,    // bottom left
             1f,  1f, 0.0f,  0.0f, 0.0f, 1.0f,  1.0f,    // top right
             0f,  0f, 0.0f,  0.0f, 1.0f, 0.0f,  1.0f,    // bottom left
             0f,  1f, 0.0f,  0.0f, 0.0f, 1.0f,  1.0f,    // top left
             1f,  1f, 0.0f,  0.0f, 0.0f, 1.0f,  1.0f,    // top right
        };

        private float[] _positionColor2 =
        {
             // positions        // colors
             0f, -1f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,    // bottom right
            -1f, -1f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,    // bottom left
             0f,  0f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,    // top right
            -1f, -1f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,    // bottom left
            -1f,  0f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,    // top left
             0f,  0f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,    // top right
        };

        public void Load()
        {
            ClearColor(1f, 1f, 1f, 1f);
            ClearColor(1f, 1f, 1f);
            ClearColor(255, 255, 255, 255);
            ClearColor(255, 255, 255);

            _vertexBuffer = new VertexBuffer(
                12,
                new[] {
                    new VertexBufferLayout.Part { Identifier = VertexBufferLayout.PartIdentifier.Position1, Type = VertexBufferLayout.ElementType.Single, Count = 3 },
                    new VertexBufferLayout.Part { Identifier = VertexBufferLayout.PartIdentifier.Color1, Type = VertexBufferLayout.ElementType.Single, Count = 4 },
                },
                VertexBufferUsage.StaticDraw);

            _vertexBuffer.Bind();

            _vertexBuffer.Write(ref _positionColor1);
            _vertexBuffer.Write(ref _positionColor2);

            _vertexBuffer.EnableElements();





            const string vertexShaderSource = @"
#version 330 core

in vec3 iPosition;  
in vec4 iColor;

out vec4 ioColor; // output a color to the fragment shader

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

void main(void)
{
    gl_Position = vec4(iPosition, 1.0) * uModel * uView * uProjection;

	ioColor = iColor;
}";

            const string fragmentShaderSource = @"
#version 330 core

in vec4 ioColor;

out vec4 oColor;

void main()
{
    oColor = ioColor;
}";

            _shader = new Shader(vertexShaderSource, fragmentShaderSource);

            QueryGraphicsCardCapabilities();
        }

        public void UnLoad()
        {
            _vertexBuffer.Dispose();
        }

        public void Render()
        {
            Clear(ClearFlag.Color);

            _shader.Use();

            _shader.Uniform("uView", ViewMatrix);
            _shader.Uniform("uProjection", ProjectionMatrix);

            _vertexBuffer.Bind();
            _vertexBuffer.EnableElements();

            for (var y = -5.0f; y < 5.0f; y += 2.5f)
            {
                for (var x = -5.0f; x < 5.0f; x += 2.5f)
                {
                    var model = Matrix4.CreateTranslation(x, y, 0.0f);

                    _shader.Uniform("uModel", model);

                    GL.DrawArrays(PrimitiveType.Triangles, 0, 12);
                }
            }

        }

        public void Update()
        {
            
        }
    }
}

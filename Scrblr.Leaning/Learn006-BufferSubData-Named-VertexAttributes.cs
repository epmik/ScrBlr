using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn006-BufferSubData-Named-VertexAttributes")]
    public class Learn006 : AbstractSketch20220317
    {
        public Learn006()
            : base(4, 4)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;
        }

        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private Shader20220413 _shader;

        private readonly float[] _positionColor1 =
        {
             // positions        // colors
             1f,  0f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,    // bottom right
             0f,  0f, 0.0f,  0.0f, 1.0f, 0.0f,  1.0f,    // bottom left
             1f,  1f, 0.0f,  0.0f, 0.0f, 1.0f,  1.0f,    // top right
             0f,  0f, 0.0f,  0.0f, 1.0f, 0.0f,  1.0f,    // bottom left
             0f,  1f, 0.0f,  0.0f, 0.0f, 1.0f,  1.0f,    // top left
             1f,  1f, 0.0f,  0.0f, 0.0f, 1.0f,  1.0f,    // top right
        };

        private readonly float[] _positionColor2 =
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

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            GL.BufferData(BufferTarget.ArrayBuffer, (_positionColor1.Length * sizeof(float)) + (_positionColor2.Length * sizeof(float)), IntPtr.Zero, BufferUsageHint.StaticDraw);

            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, _positionColor1.Length * sizeof(float), _positionColor1);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(_positionColor1.Length * sizeof(float)), _positionColor2.Length * sizeof(float), _positionColor2);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

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

            _shader = new Shader20220413(vertexShaderSource, fragmentShaderSource);

            // instead of using hardcoded values
            // GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            // we retrieve a location index by name
            // var iPositionLocation = _shader.AttributeLocation("iPosition");
            // and call
            // GL.VertexAttribPointer(iPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            // in that case, we can ommit 'layout(location = 0)' from the vertex shader 
            // layout(location = 0) in vec3 iPosition;
            var iPositionLocation = _shader.AttributeLocation("iPosition");
            var iColorLocation = _shader.AttributeLocation("iColor");

            GL.VertexAttribPointer(iPositionLocation, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);

            GL.VertexAttribPointer(iColorLocation, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));

            QueryGraphicsCardCapabilities();
        }

        public void UnLoad()
        {
            
        }

        public void Render()
        {
            Clear(ClearFlag.ColorBuffer);

            _shader.Use();

            GL.BindVertexArray(_vertexArrayObject);

            var model = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);

            _shader.Uniform("uModel", model);
            _shader.Uniform("uView", ViewMatrix);
            _shader.Uniform("uProjection", ProjectionMatrix);

            var iPositionLocation = _shader.AttributeLocation("iPosition");
            var iColorLocation = _shader.AttributeLocation("iColor");

            GL.EnableVertexAttribArray(iPositionLocation);
            GL.EnableVertexAttribArray(iColorLocation);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 12);
        }

        public void Update()
        {
            
        }
    }
}

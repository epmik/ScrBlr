using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn007-VertexBuffer-Class")]
    public class Learn007 : AbstractSketch20220317
    {
        public Learn007()
            : base(4, 4)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;
        }

        private VertexBuffer20220316<float> _vertexBuffer;
        //private int _vertexBufferObject;
        //private int _vertexArrayObject;
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




            _vertexBuffer = new VertexBuffer20220316<float>(12, VertexDataType20220316.Position | VertexDataType20220316.Color, VertexBufferType20220316.StaticDraw);

            _vertexBuffer.Bind();

            _vertexBuffer.Write(ref _positionColor1);
            _vertexBuffer.Write(ref _positionColor2);

            _vertexBuffer.EnableArray(VertexDataType20220316.Position);
            _vertexBuffer.EnableArray(VertexDataType20220316.Color);




            //_vertexBufferObject = GL.GenBuffer();

            //GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            //GL.BufferData(BufferTarget.ArrayBuffer, (_positionColor1.Length * sizeof(float)) + (_positionColor2.Length * sizeof(float)), IntPtr.Zero, BufferUsageHint.StaticDraw);

            //GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, _positionColor1.Length * sizeof(float), _positionColor1);
            //GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(_positionColor1.Length * sizeof(float)), _positionColor2.Length * sizeof(float), _positionColor2);

            //_vertexArrayObject = GL.GenVertexArray();
            //GL.BindVertexArray(_vertexArrayObject);



            const string vertexShaderSource = @"
#version 330 core

layout(location = 0) in vec3 iPosition;  
layout(location = 1) in vec4 iColor;

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

            //var iPositionLocation = _shader.AttributeLocation("iPosition");
            //var iColorLocation = _shader.AttributeLocation("iColor");

            //GL.VertexAttribPointer(iPositionLocation, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);

            //GL.VertexAttribPointer(iColorLocation, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));

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

            //GL.BindVertexArray(_vertexArrayObject);

            var model = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);

            _shader.Uniform("uModel", model);
            _shader.Uniform("uView", ViewMatrix);
            _shader.Uniform("uProjection", ProjectionMatrix);

            //var iPositionLocation = _shader.AttributeLocation("iPosition");
            //var iColorLocation = _shader.AttributeLocation("iColor");

            //GL.EnableVertexAttribArray(iPositionLocation);
            //GL.EnableVertexAttribArray(iColorLocation);

            _vertexBuffer.Bind();
            _vertexBuffer.EnableArray(VertexDataType20220316.Position);
            _vertexBuffer.EnableArray(VertexDataType20220316.Color);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 12);
        }

        public void Update()
        {
            
        }
    }
}

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn005-BufferSubData-EnableDisable-VertexAttribute")]
    public class Learn005 : AbstractSketch20220317
    {
        public Learn005()
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
             1f,  0f, 0.0f,  1.0f, 0.0f, 0.0f,    // bottom right
             0f,  0f, 0.0f,  0.0f, 1.0f, 0.0f,    // bottom left
             1f,  1f, 0.0f,  0.0f, 0.0f, 1.0f,    // top right
             0f,  0f, 0.0f,  0.0f, 1.0f, 0.0f,    // bottom left
             0f,  1f, 0.0f,  0.0f, 0.0f, 1.0f,    // top left
             1f,  1f, 0.0f,  0.0f, 0.0f, 1.0f,    // top right
        };

        private readonly float[] _positionColor2 =
        {
             // positions        // colors
             0f, -1f, 0.0f,  1.0f, 0.0f, 0.0f,    // bottom right
            -1f, -1f, 0.0f,  1.0f, 0.0f, 0.0f,    // bottom left
             0f,  0f, 0.0f,  1.0f, 0.0f, 0.0f,    // top right
            -1f, -1f, 0.0f,  1.0f, 0.0f, 0.0f,    // bottom left
            -1f,  0f, 0.0f,  1.0f, 0.0f, 0.0f,    // top left
             0f,  0f, 0.0f,  1.0f, 0.0f, 0.0f,    // top right
        };

        public void Load()
        {
            ClearColor(1f, 1f, 1f, 1f);
            ClearColor(1f, 1f, 1f);
            ClearColor(255, 255, 255, 255);
            ClearColor(255, 255, 255);

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            GL.BufferData(BufferTarget.ArrayBuffer, 2 * _positionColor1.Length * sizeof(float), IntPtr.Zero, BufferUsageHint.StaticDraw);

            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, _positionColor1.Length * sizeof(float), _positionColor1);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(_positionColor1.Length * sizeof(float)), _positionColor2.Length * sizeof(float), _positionColor2);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            QueryGraphicsCardCapabilities();

            const string vertexShaderSource = @"
#version 330 core

// the position variable has attribute position 0
layout(location = 0) in vec3 iPosition;  

// This is where the color values we assigned in the main program goes to
layout(location = 1) in vec3 iColor;

out vec3 ioColor; // output a color to the fragment shader

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

void main(void)
{
    gl_Position = vec4(iPosition, 1.0) * uModel * uView * uProjection;

	// We use the ioColor variable to pass on the color information to the frag shader
	ioColor = iColor;
}";

            const string fragmentShaderSource = @"
#version 330 core

out vec4 oColor;

in vec3 ioColor;

void main()
{
    oColor = vec4(ioColor, 1.0);
}";

            _shader = new Shader20220413(vertexShaderSource, fragmentShaderSource);
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

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.EnableVertexAttribArray(0);
            // Disable variable 1 in the shader // color 
            GL.DisableVertexAttribArray(1);

            GL.DrawArrays(PrimitiveType.Triangles, 6, 6);
        }

        public void Update()
        {
            
        }
    }
}

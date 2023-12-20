using Scrblr.Core;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Xml.Linq;
using Silk.NET.GLFW;
using System.Reflection.Metadata;
using System.Numerics;

namespace Scrblr.Leaning
{
    //[Sketch(Name = "Learn004-BufferSubData-DrawArrays-Position-UniformColor")]
    public class Learn004 : AbstractSketch
    {
        private static uint Vbo;
        //private static uint Ebo;
        private static uint Vao;
        private static uint Shader;

        private static Random _random = new Random();

        private Vector4 _color = new Vector4(
            (float)(_random.NextDouble()),
            (float)(_random.NextDouble()),
            (float)(_random.NextDouble()),
            1.0f);

        private bool _colorUpR = true;
        private bool _colorUpG = true;
        private bool _colorUpB = true;

        private double _colorFactor = 0.5;

        //Vertex shaders are run on each vertex.
        private static readonly string VertexShaderSource = @"
        #version 330 core
        
        layout (location = 0) in vec3 vPos;

        void main()
        {
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";

        //Fragment shaders are run on each fragment/pixel of the geometry.
        private static readonly string FragmentShaderSource = @"
        #version 330 core

        uniform vec4 uColor; 

        out vec4 oColor;

        void main()
        {
            // oColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
            oColor = uColor;
        }
        ";

        private static uint VertexElementCount = 7;

        //Vertex data, uploaded to the VBO.
        private static readonly float[] Vertices2 =
        {
            //X    Y      Z
            0.75f, 0.75f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, // 0
            0.75f, 0.25f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, // 1
            0.25f, 0.75f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, // 3
            0.75f, 0.25f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, // 1
            0.25f, 0.25f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, // 2
            0.25f, 0.75f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, // 3
        };

        private static readonly float[] Vertices1 =
        {
            //X    Y      Z
            0.75f, -0.75f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, // 0
            0.75f, -0.25f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, // 1
            0.25f, -0.75f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, // 3
            0.75f, -0.25f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, // 1
            0.25f, -0.25f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, // 2
            0.25f, -0.75f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, // 3
        };

        ////Index data, uploaded to the EBO.
        //private static readonly uint[] Indices =
        //{
        //    0, 1, 3,
        //    1, 2, 3
        //};

        private unsafe void Load()
        {
            Gl.ClearColor(Color.CornflowerBlue);

            //Creating a vertex array.
            Vao = Gl.GenVertexArray();
            Gl.BindVertexArray(Vao);

            var size = Vertices1.Length * sizeof(float) + Vertices2.Length * sizeof(float);
            var offset = 0;

            //Initializing a vertex buffer that holds the vertex data.
            Vbo = Gl.GenBuffer(); //Creating the buffer.
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo); //Binding the buffer.
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)size, IntPtr.Zero, BufferUsageARB.StaticDraw); //Setting buffer data.
            fixed (void* v = &Vertices1[0])
            {
                size = Vertices1.Length * sizeof(float);
                Gl.BufferSubData(BufferTargetARB.ArrayBuffer, (nint)offset, (nuint)size, v); //Setting buffer data.
                offset += size;
            }
            fixed (void* v = &Vertices2[0])
            {
                size = Vertices2.Length * sizeof(float);
                Gl.BufferSubData(BufferTargetARB.ArrayBuffer, (nint)offset, (nuint)size, v); //Setting buffer data.
                offset += size;
            }

            ////Initializing a element buffer that holds the index data.
            //Ebo = Gl.GenBuffer(); //Creating the buffer.
            //Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, Ebo); //Binding the buffer.
            //fixed (void* i = &Indices[0])
            //{
            //    size = Indices.Length * sizeof(uint);
            //    Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw); //Setting buffer data.
            //}

            //Creating a vertex shader.
            uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
            Gl.ShaderSource(vertexShader, VertexShaderSource);
            Gl.CompileShader(vertexShader);

            //Checking the shader for compilation errors.
            string infoLog = Gl.GetShaderInfoLog(vertexShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine($"Error compiling vertex shader {infoLog}");
            }

            //Creating a fragment shader.
            uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(fragmentShader, FragmentShaderSource);
            Gl.CompileShader(fragmentShader);

            //Checking the shader for compilation errors.
            infoLog = Gl.GetShaderInfoLog(fragmentShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine($"Error compiling fragment shader {infoLog}");
            }

            //Combining the shaders under one shader program.
            Shader = Gl.CreateProgram();
            Gl.AttachShader(Shader, vertexShader);
            Gl.AttachShader(Shader, fragmentShader);
            Gl.LinkProgram(Shader);

            //Checking the linking for errors.
            Gl.GetProgram(Shader, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(Shader)}");
            }

            //Delete the no longer useful individual shaders;
            Gl.DetachShader(Shader, vertexShader);
            Gl.DetachShader(Shader, fragmentShader);
            Gl.DeleteShader(vertexShader);
            Gl.DeleteShader(fragmentShader);
        }

        private unsafe void Render()
        {
            //Clear the color channel.
            Gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            //Bind the geometry and shader.
            Gl.BindVertexArray(Vao);
            Gl.UseProgram(Shader);

            //Tell opengl how to give the data to the shaders.
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VertexElementCount * sizeof(float), null);
            Gl.EnableVertexAttribArray(0);

            //Gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VertexElementCount * sizeof(float), (void*)3);
            //Gl.EnableVertexAttribArray(1);

            Gl.Uniform4(Gl.GetUniformLocation(Shader, "uColor"), _color.X, _color.Y, _color.Z, _color.W);

            //Draw the geometry.
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 12);
        }

        private void Update()
        {
            var factor = ElapsedTime * _colorFactor;

            _color.X = (float)(_colorUpR ? _color.X + factor : _color.X - factor);
            _color.Y = (float)(_colorUpG ? _color.Y + factor : _color.Y - factor);
            _color.Z = (float)(_colorUpB ? _color.Z + factor : _color.Z - factor);

            if (_color.X > 1.0f)
            {
                _colorUpR = false;
                _color.X = 1.0f;
            }
            if (_color.X < 0.0f)
            {
                _colorUpR = true;
                _color.X = 0.0f;
            }

            if (_color.Y > 1.0f)
            {
                _colorUpG = false;
                _color.Y = 1.0f;
            }
            if (_color.Y < 0.0f)
            {
                _colorUpG = true;
                _color.Y = 0.0f;
            }

            if (_color.Z > 1.0f)
            {
                _colorUpB = false;
                _color.Z = 1.0f;
            }
            if (_color.Z < 0.0f)
            {
                _colorUpB = true;
                _color.Z = 0.0f;
            }

            //Diagnostics.Log(_color.ToString());
        }

        private void UnLoad()
        {
            //Remember to delete the buffers.
            Gl.DeleteBuffer(Vbo);
            //Gl.DeleteBuffer(Ebo);
            Gl.DeleteVertexArray(Vao);
            Gl.DeleteProgram(Shader);
        }

        private void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                Dispose();
            }
        }

    }
}

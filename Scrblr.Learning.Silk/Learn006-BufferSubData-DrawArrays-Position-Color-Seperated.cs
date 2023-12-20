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
    //[Sketch(Name = "Learn006-BufferSubData-DrawArrays-Position-Color-Seperated")]
    public class Learn006 : AbstractSketch
    {
        private static uint Vbo;
        private static uint Vao;
        private static uint Shader;

        private static Random _random = new Random();

        private Vector4 _color = new Vector4(
            (float)(_random.NextDouble()),
            (float)(_random.NextDouble()),
            (float)(_random.NextDouble()),
            1.0f);

        public Vector3 ModelPosition = new Vector3(0f, 0f, 0);

        private float ModelScale { get; set; } = 1f;
        private Quaternion ModelRotation { get; set; } = Quaternion.Identity;

        public float ProjectionFov { get; set; } = 45.00f;
        public float ProjectionNear { get; set; } = 0.10f;
        public float ProjectionFar { get; set; } = 100.00f;

        public Vector3 ViewPosition = new Vector3(0f, 0f, -1f);

        private Matrix4x4 ViewMatrix = Matrix4x4.Identity;

        private Matrix4x4 ProjectionMatrix;

        //Vertex shaders are run on each vertex.
        private static readonly string VertexShaderSource = @"
        #version 330 core

        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;
        
        layout (location = 0) in vec3 vPos;
        layout (location = 1) in vec4 iColor;

        out vec4 pColor;

        void main()
        {
            gl_Position = vec4(vPos, 1.0) * uModel * uView * uProjection;
            pColor = iColor;
        }
        ";

        //Fragment shaders are run on each fragment/pixel of the geometry.
        private static readonly string FragmentShaderSource = @"
        #version 330 core

        in vec4 pColor; 

        out vec4 oColor;

        void main()
        {
            oColor = pColor;
        }
        ";

        //Vertex data, uploaded to the VBO.
        private static readonly float[] _vertices =
        {
            //X    Y      Z
            0.95f, 0.95f, 0.0f,
            0.95f, -0.95f, 0.0f,
            0.05f, 0.95f, 0.0f,
            0.95f, -0.95f, 0.0f,
            0.05f, -0.95f, 0.0f,
            0.05f, 0.95f, 0.0f,

            -0.95f, 0.95f, 0.0f,
            -0.95f, -0.95f, 0.0f,
            -0.05f, 0.95f, 0.0f,
            -0.95f, -0.95f, 0.0f,
            -0.05f, -0.95f, 0.0f,
            -0.05f, 0.95f, 0.0f,
        };

        //Vertex data, uploaded to the VBO.
        private static readonly float[] _colors =
        {
            //X    Y      Z
            1.0f, 0.0f, 0.0f, 1.0f, // 0
            1.0f, 0.0f, 0.0f, 1.0f, // 1
            1.0f, 0.0f, 0.0f, 1.0f, // 3
            1.0f, 0.0f, 0.0f, 1.0f, // 1
            1.0f, 0.0f, 0.0f, 1.0f, // 2
            1.0f, 0.0f, 0.0f, 1.0f, // 3
            0.0f, 1.0f, 0.0f, 1.0f, // 0
            0.0f, 1.0f, 0.0f, 1.0f, // 1
            0.0f, 1.0f, 0.0f, 1.0f, // 3
            0.0f, 1.0f, 0.0f, 1.0f, // 1
            0.0f, 1.0f, 0.0f, 1.0f, // 2
            0.0f, 1.0f, 0.0f, 1.0f, // 3
        };

        private static Color[] _pickColors = new Color[]
        {
            ColorTranslator.FromHtml("#900c3f"),
            ColorTranslator.FromHtml("#c70739"),
            ColorTranslator.FromHtml("#ff5733"),
            ColorTranslator.FromHtml("#ff8d19"),
            ColorTranslator.FromHtml("#ffc301"),
        };


        ////Index data, uploaded to the EBO.
        //private static readonly uint[] Indices =
        //{
        //    0, 1, 3,
        //    1, 2, 3
        //};

        private unsafe void Load()
        {
            for (var i = 0; i < _colors.Length;)
            {
                var c = _pickColors[_random.Next(_pickColors.Length)];

                _colors[i++] = (float)c.R / 255f;
                _colors[i++] = (float)c.G / 255f;
                _colors[i++] = (float)c.B / 255f;
                _colors[i++] = (float)c.A / 255f;
            }

            //Getting the opengl api for drawing to the screen.
            Gl = GL.GetApi(window);

            Gl.ClearColor(Color.CornflowerBlue);

            //Creating a vertex array.
            Vao = Gl.GenVertexArray();
            Gl.BindVertexArray(Vao);

            var size = _vertices.Length * sizeof(float) + _colors.Length * sizeof(float);
            var offset = 0;

            //Initializing a vertex buffer that holds the vertex data.
            Vbo = Gl.GenBuffer(); //Creating the buffer.
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo); //Binding the buffer.
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)size, IntPtr.Zero, BufferUsageARB.StaticDraw); //Setting buffer data.
            fixed (void* v = &_vertices[0])
            {
                size = _vertices.Length * sizeof(float);
                Gl.BufferSubData(BufferTargetARB.ArrayBuffer, (nint)offset, (nuint)size, v); //Setting buffer data.
                offset += size;
            }
            fixed (void* v = &_colors[0])
            {
                size = _colors.Length * sizeof(float);
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

            ViewPosition = CalculateInitialViewPosition();

            ProjectionMatrix = CreateProjectionMatrix();
        }

        private unsafe void Render()
        {
            //Clear the color channel.
            Gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            //Bind the geometry and shader.
            Gl.BindVertexArray(Vao);
            Gl.UseProgram(Shader);

            var viewMatrix =  Matrix4x4.Identity * Matrix4x4.CreateFromQuaternion(ModelRotation) * Matrix4x4.CreateScale(ModelScale) * Matrix4x4.CreateTranslation(ModelPosition);

            Gl.UniformMatrix4(Gl.GetUniformLocation(Shader, "uModel"), 1, false, (float*)&viewMatrix);

            fixed (float* v = &ViewMatrix.M11)
            {
                Gl.UniformMatrix4(Gl.GetUniformLocation(Shader, "uView"), 1, false, v);
            }

            fixed (float* v = &ProjectionMatrix.M11)
            {
                Gl.UniformMatrix4(Gl.GetUniformLocation(Shader, "uProjection"), 1, false, v);
            }

            //Tell opengl how to give the data to the shaders.
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
            Gl.EnableVertexAttribArray(0);

            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)(_vertices.Length * sizeof(float)));
            Gl.EnableVertexAttribArray(1);

            //Gl.Uniform4(Gl.GetUniformLocation(Shader, "uColor"), _color.X, _color.Y, _color.Z, _color.W);

            //Draw the geometry.
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 12);
        }

        private void Update()
        {
        }

        private void UnLoad()
        {
            //Remember to delete the buffers.
            Gl.DeleteBuffer(Vbo);
            //Gl.DeleteBuffer(Ebo);
            Gl.DeleteVertexArray(Vao);
            Gl.DeleteProgram(Shader);
        }

        private void Resize(uint width, uint height)
        {
            //var size = new int[2];
            //var maxsize = new int[2];

            //Gl.GetInteger(GLEnum.Viewport, size);
            //Gl.GetInteger(GLEnum.MaxViewportDims, maxsize);

            // internal window is already resized
            Gl.Viewport(0, 0, width, height);

            ProjectionMatrix = CreateProjectionMatrix();
        }

        private Vector3 CalculateInitialViewPosition()
        {
            // calculate the z-axis position so that the provided sketch size fits into the initial view
            var a = (180f - ProjectionFov) * 0.5f;
            var d = -(float)Math.Tan(Utility.DegreesToRadians(a)) * (2f * 0.5f);
            //var d = -(float)Math.Tan(Utility.DegreesToRadians(a)) * (SketchWidth * 0.5f);

            return new Vector3(0.0f, 0.0f, d);
        }

        private Matrix4x4 CreateProjectionMatrix()
        {
            return Matrix4x4.Identity;

            // For the matrix, we use a few parameters.
            //   Field of view. This determines how much the viewport can see at once. 45 is considered the most "realistic" setting, but most video games nowadays use 90
            //   Aspect ratio. This should be set to Width / Height.
            //   Near-clipping. Any vertices closer to the camera than this value will be clipped.
            //   Far-clipping. Any vertices farther away from the camera than this value will be clipped.

            var a = (180f - ProjectionFov) * 0.5f;
            var d = Math.Tan(Utility.DegreesToRadians(a)) * ((float)window.Size.X * 0.5f);

            return Matrix4x4.CreatePerspectiveFieldOfView((float)Utility.DegreesToRadians(ProjectionFov), window.Size.X / (float)window.Size.Y, ProjectionNear, ProjectionFar);

            return Matrix4x4.CreateOrthographic(window.Size.X, window.Size.Y, ProjectionNear, ProjectionFar);
        }
    }
}

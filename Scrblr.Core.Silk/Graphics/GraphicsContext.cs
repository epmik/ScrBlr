using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using System.Numerics;
using FontStashSharp;

namespace Scrblr.Core
{
    public class GraphicsContext : IDisposable
    {
        public static GL GL;

        /// <summary>
        /// default == ProjectionMode.Perspective
        /// </summary>
        public ProjectionMode ProjectionMode { get; set; } = ProjectionMode.Perspective;

        public IWindow? Window { get; private set; }

        private static FontRenderer? _fontRenderer;
        private static FontSystem? _fontSystem;

        public float Width { get; set; } = 1;
        public float Height { get; set; } = 1;
        public float Near { get; set; } = 0.1f;
        public float Far { get; set; } = 100f;

        /// <summary>
        /// field of view in degrees
        /// <para>
        /// this can be the vertical (default) or horizontal field of view
        /// </para>
        /// <para>
        /// if the window is higher than it's width, then the Fov is considered horizontal
        /// </para>
        /// </summary>
        public float Fov = 45f;

        public double ElapsedTime { get; private set; }

        protected Matrix4x4 ProjectionMatrix;

        protected Matrix4x4 ViewMatrix;

        protected Matrix4x4 ModelMatrix;


        private uint __vertexBufferObject;

        private uint __vertexArrayObject;

        private Shader __shader;

        private Texture __texture1;

        private Texture __texture2;

        #region Shaders

        #region Vertex Shaders

        string _vertexShader_xyz_uv_rgba = @"
#version 330 core

layout(location = 0) in vec3 i_xyz;
layout(location = 1) in vec2 i_uv;
layout(location = 2) in vec4 i_rgba;

out vec2 p_uv;
out vec4 p_rgba;

uniform mat4 u_model;
uniform mat4 u_view;
uniform mat4 u_projection;

void main(void)
{
    p_uv = i_uv;
    p_rgba = i_rgba;

    gl_Position = vec4(i_xyz, 1.0) * u_model * u_view * u_projection;
}
";

        #endregion Vertex Shaders

        #region Fragment Shaders

        string _fragmentShader_xyz_uv_rgba = @"
#version 330

in vec2 p_uv;
in vec4 p_rgba;

out vec4 o_rgba;

// uniform sampler2D texture0;
// uniform sampler2D texture1;
// uniform float textureMix;

void main()
{
    //o_rgba = mix(texture(texture0, texCoord), texture(texture1, texCoord), textureMix) * p_rgba;
    o_rgba = p_rgba;
}
";

        #endregion Fragment Shaders

        #endregion Shaders


        public GraphicsContext()
            : this(1f)
        {
        }

        public GraphicsContext(float size)
            : this(size, size)
        {
        }

        public GraphicsContext(float width, float height)
        {
            Width = width;
            Height = height;
        }

        ~GraphicsContext() => Dispose(false);

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _fontRenderer?.Dispose();

            _fontRenderer = null;

            GL.DeleteBuffer(__vertexBufferObject);

            //GL.DeleteBuffer(_elementBufferObject);

            GL.DeleteVertexArray(__vertexArrayObject);

            __shader.Delete();
        }

        private void CreateFontContext()
        {
            _fontRenderer = new FontRenderer();

            //var settings = new FontSystemSettings
            //{
            //    FontResolutionFactor = 2,
            //    KernelWidth = 2,
            //    KernelHeight = 2
            //};

            //_fontSystem = new FontStashSharp.FontSystem(settings);
            _fontSystem = new FontSystem();
            //_fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/droidsans.ttf"));
            //_fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/Roboto-Black.ttf"));
            _fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/Ubuntu-Regular.ttf"));
            _fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/droidsansjapanese.ttf"));
            _fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/symbola-emoji.ttf"));
        }

        protected unsafe void Setup(IWindow window)
        {
            Window = window;

            if (Context.GL == null)
            {
                Context.GL = GL = GL.GetApi(Window);
            }

            // background color
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // enable clockwise winding
            GL.FrontFace(FrontFaceDirection.CW);

            // cull the backface
            GL.CullFace(GLEnum.Back);

            // enable culling
            GL.Enable(EnableCap.CullFace);

            __vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(__vertexArrayObject);

            GLUtility.CheckError();

            __vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(GLEnum.ArrayBuffer, __vertexBufferObject);
            //fixed (void* v = &_vertices[0])
            //    GL.BufferData(GLEnum.ArrayBuffer, (nuint)(_vertices.Length * sizeof(float)), v, GLEnum.StaticDraw);

            GLUtility.CheckError();

            __shader = new Shader(GL, _vertexShader_xyz_uv_rgba, _fragmentShader_xyz_uv_rgba);

            GLUtility.CheckError();

            __shader.Use();

            GLUtility.CheckError();

            var location = __shader.GetAttribLocation("i_xyz");

            if (location != uint.MaxValue)
            {
                GLUtility.CheckError();

                GL.EnableVertexAttribArray(location);
                GLUtility.CheckError();

                GL.VertexAttribPointer(location, 3, GLEnum.Float, false, 9 * sizeof(float), (void*)0);
                GLUtility.CheckError();
            }

            location = __shader.GetAttribLocation("i_uv");
            
            if(location != uint.MaxValue)
            {
                GLUtility.CheckError();

                GL.EnableVertexAttribArray(location);
                GLUtility.CheckError();

                GL.VertexAttribPointer(location, 2, GLEnum.Float, false, 9 * sizeof(float), (void*)(3 * sizeof(float)));
                GLUtility.CheckError();
            }

            location = __shader.GetAttribLocation("i_rgba");
            
            if (location != uint.MaxValue)
            {
                GLUtility.CheckError();

                GL.EnableVertexAttribArray(location);
                GLUtility.CheckError();

                GL.VertexAttribPointer(location, 4, GLEnum.Float, false, 9 * sizeof(float), (void*)(5 * sizeof(float)));
                GLUtility.CheckError();
            }

            //_texture = Texture.LoadFromFile(GL, ".resources/container.png");
            //_texture.Use(TextureUnit.Texture0);

            //_texture2 = Texture.LoadFromFile(GL, ".resources/awesomeface.png");
            //_texture2.Use(TextureUnit.Texture1);

            //_shader.SetInt("texture0", 0);
            //_shader.SetInt("texture1", 1);

            CreateFontContext();

            ModelMatrix = Matrix4x4.Identity;

            ViewMatrix = Matrix4x4.Identity;

            UpdateProjectionMatrix();

            //var fovRadians = (float)Utility.DegreesToRadians(Fov);

            //float extent = Width * 0.5f;
            //var z = extent / (float)Math.Sin(fovRadians * 0.5f);

            //_viewPosition = new Vector3(0, 0, -(z - Near));
        }

        protected void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        protected void Flush()
        {
            GL.BindVertexArray(__vertexArrayObject);

            __shader.Use();

            __shader.SetMatrix4("u_model", ModelMatrix);
            __shader.SetMatrix4("u_view", ViewMatrix);
            __shader.SetMatrix4("u_projection", ProjectionMatrix);

            //__shader.SetFloat("textureMix", _textureMix);

            // And that's it for now! In the next tutorial, we'll see how to setup a full coordinates system.

            //GL.DrawElements(GLEnum.Triangles, (uint)(6), GLEnum.UnsignedInt, (void*)0);

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        }

        protected void Resize(Vector2D<int> size)
        {
            GL.Viewport(0, 0, (uint)size.X, (uint)size.Y);

            UpdateProjectionMatrix();
        }

        protected void WriteText(string text, int x, int y)
        {
            var font = _fontSystem.GetFont(18);

            //var size = font.MeasureString(text, scale);
            //var origin = new Vector2(size.X / 2.0f, size.Y / 2.0f);

            _fontRenderer.Begin();

            //font.DrawText(_fontRenderer, text, new Vector2(400, 400), FSColor.Yellow, _rads, origin, scale);

            font.DrawText(_fontRenderer, text, new Vector2(20, 20), FSColor.White);

            _fontRenderer.End();
        }

        protected void UpdateProjectionMatrix()
        {
            var fovRadians = (float)Utility.DegreesToRadians(Fov);

            var aspectRatioHorizontal = Window.Size.X / (float)Window.Size.Y;
            var aspectRatioVertical = Window.Size.Y / (float)Window.Size.X;

            switch (ProjectionMode)
            {
                case ProjectionMode.Orthographic:
                    if (aspectRatioHorizontal >= aspectRatioVertical)
                    {
                        ProjectionMatrix = Matrix4x4.CreateOrthographic(Width * aspectRatioHorizontal, Height, Near, Far);
                    }
                    else
                    {
                        ProjectionMatrix = Matrix4x4.CreateOrthographic(Width, Height * aspectRatioVertical, Near, Far);
                    }
                    break;
                case ProjectionMode.Perspective:
                    if (aspectRatioHorizontal >= aspectRatioVertical)
                    {
                        var top = (float)Math.Tan(fovRadians * 0.5f) * Near;
                        var bottom = -top;
                        var right = top * aspectRatioHorizontal;
                        var left = -right;
                        ProjectionMatrix = Matrix4x4.CreatePerspective(right - left, top - bottom, Near, Far);
                    }
                    else
                    {
                        var right = (float)Math.Tan(fovRadians * 0.5f) * Near;
                        var left = -right;
                        var top = right * aspectRatioVertical;
                        var bottom = -top;
                        ProjectionMatrix = Matrix4x4.CreatePerspective(right - left, top - bottom, Near, Far);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
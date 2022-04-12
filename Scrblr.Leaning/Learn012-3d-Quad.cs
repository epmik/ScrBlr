using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn012-3d-Quad")]
    public class Learn012 : AbstractSketch20220406
    {
        const string vertexShaderSource = @"
#version 330 core
layout (location = 0) in vec3 iPosition;
layout (location = 1) in vec3 iColor;
layout (location = 2) in vec2 iTexCoord;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

out vec3 ioColor;
out vec2 ioTexCoord;

void main()
{
	gl_Position = vec4(iPosition, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;
	ioColor = iColor;
	ioTexCoord = vec2(iTexCoord.x, iTexCoord.y);
}
";

        const string fragmentShaderSource = @"
#version 330 core

out vec4 oFragColor;

in vec3 ioColor;
in vec2 ioTexCoord;

uniform sampler2D uTexture1;
uniform sampler2D uTexture2;

void main()
{
	// linearly interpolate between both textures (80% container, 20% awesomeface)
	oFragColor = mix(texture(uTexture1, ioTexCoord), texture(uTexture2, ioTexCoord), 0.2);
}
";

        float[] _vertices = {
            // positions          // colors           // texture coords
             0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f  // top left 
        };

        uint[] _indices = {
            0, 1, 3, // first triangle
            1, 2, 3  // second triangle
        };

        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _indexBufferObject;
        private int _texture1;
        private int _texture2;
        private Shader _shader;

        public Learn012()
            : base(600, 600)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;

            Samples = 8;
            SketchOrigin = SketchOrigin.Center;
        }


        public void Load()
        {
            ClearColor(1f, 1f, 1f, 1f);
            ClearColor(1f, 1f, 1f);
            ClearColor(255, 255, 255, 255);
            ClearColor(255, 255, 255);

            _vertexBufferObject = GL.GenBuffer();
            _vertexArrayObject = GL.GenVertexArray();
            _indexBufferObject = GL.GenBuffer();

            GL.BindVertexArray(_vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);


            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            _shader = new Shader(vertexShaderSource, fragmentShaderSource);

            _texture1 = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _texture1);
            // set the texture wrapping parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);   // set texture wrapping to GL_REPEAT (default wrapping method)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            // set texture filtering parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap("resources/textures/orange-white-2048x2048.jpg"))
            {
                var bits = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(
                    TextureTarget.Texture2D, 
                    0, 
                    PixelInternalFormat.Rgba, 
                    bits.Width, 
                    bits.Height, 
                    0,
                    PixelFormat.Bgra, 
                    PixelType.UnsignedByte, 
                    bits.Scan0);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                bitmap.UnlockBits(bits);
            }

            _texture2 = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _texture2);
            // set the texture wrapping parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);   // set texture wrapping to GL_REPEAT (default wrapping method)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            // set texture filtering parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap("resources/textures/smiley-transparent-1024x1024.png"))
            {
                var bits = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    bits.Width,
                    bits.Height,
                    0,
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    bits.Scan0);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                bitmap.UnlockBits(bits);
            }
        }

        public void UnLoad()
        {
            GL.DeleteTextures(1, ref _texture1);
        }

        public void Render()
        {
            Clear(ClearFlag.Color);

            GL.Enable(EnableCap.Texture2D);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture1);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _texture2);

            _shader.Use();

            GL.BindVertexArray(_vertexArrayObject);

            var model =  Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);

            _shader.Uniform("uModelMatrix", model);
            _shader.Uniform("uViewMatrix", ViewMatrix);
            _shader.Uniform("uProjectionMatrix", ProjectionMatrix);
            _shader.Uniform("uTexture1", _texture1);
            _shader.Uniform("uTexture2", _texture2);

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        public void Update()
        {

        }
    }
}

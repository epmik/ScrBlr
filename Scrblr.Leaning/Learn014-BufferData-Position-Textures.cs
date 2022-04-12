using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn014-BufferData-Position-Textures")]
    public class Learn014 : AbstractSketch20220406
    {
        public Learn014()
            : base(600, 600)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;
        }

        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _texture0;
        private int _texture1;
        private Shader _shader;

        const string vertexShaderSource = @"
#version 330 core

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

layout(location = 0) in vec3 iPosition;  
layout(location = 1) in vec3 iColor;
layout (location = 2) in vec2 iTexCoord;

out vec3 ioColor; // output a color to the fragment shader
out vec2 ioTexCoord;

void main(void)
{
    gl_Position = vec4(iPosition, 1.0) * model * view * projection;
	ioColor = iColor;
	ioTexCoord = iTexCoord;
}";

        const string fragmentShaderSource = @"
#version 330 core

uniform sampler2D uTexture0;
uniform sampler2D uTexture1;

in vec3 ioColor;
in vec2 ioTexCoord;

out vec4 oColor;

void main()
{
    //oColor = vec4(ioColor, 1.0);
    //oColor = texture(uTexture0, ioTexCoord);
    oColor = mix(texture(uTexture0, ioTexCoord), texture(uTexture1, ioTexCoord), 0.2);
}";

        private readonly float[] _positionColor =
        {
             // positions        // colors
             200f, -200f, 0.0f,  1.0f, 0.0f, 0.0f,   1.0f, -1.0f,  // bottom right
            -200f, -200f, 0.0f,  0.0f, 1.0f, 0.0f,   0.0f, -1.0f, // bottom left
             200f,  200f, 0.0f,  0.0f, 0.0f, 1.0f,   1.0f, 0.0f, // top right
            -200f, -200f, 0.0f,  0.0f, 1.0f, 0.0f,   0.0f, -1.0f, // bottom left
            -200f,  200f, 0.0f,  0.0f, 0.0f, 1.0f,   0.0f, 0.0f,  // top left 
             200f,  200f, 0.0f,  0.0f, 0.0f, 1.0f,   1.0f, 0.0f, // top right
        };

        public void Load()
        {
            ClearColor(255, 255, 255);

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            GL.BufferData(BufferTarget.ArrayBuffer, _positionColor.Length * sizeof(float), _positionColor, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();

            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            _shader = new Shader(vertexShaderSource, fragmentShaderSource);


            _texture0 = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, _texture0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);   // set texture wrapping to GL_REPEAT (default wrapping method)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap("resources/textures/orange-white-2048x2048.jpg"))
            //using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap("resources/textures/orange-transparent-2048x2048.png"))
            //using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap("resources/textures/smiley-transparent-1024x1024.png"))
            {
                bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

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


            _texture1 = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, _texture1);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);   // set texture wrapping to GL_REPEAT (default wrapping method)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap("resources/textures/smiley-1024x1024.jpg"))
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap("resources/textures/smiley-transparent-1024x1024.png"))
            {
                bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

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
            GL.DeleteTexture(_texture0);
            GL.DeleteTexture(_texture1);
        }

        public void Render()
        {
            Clear(ClearFlag.Color);

            _shader.Use();

            // Bind the VAO
            GL.BindVertexArray(_vertexArrayObject);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _texture1);

            var model = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);

            _shader.Uniform("model", model);
            _shader.Uniform("view", ViewMatrix);
            _shader.Uniform("projection", ProjectionMatrix);
            _shader.Uniform("uTexture0", 0);
            _shader.Uniform("uTexture1", 1);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public void Update()
        {
            
        }
    }
}

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn018-VertexBufferClass-TextureClass")]
    public class Learn018 : AbstractSketch20220410
    {
        public Learn018()
            : base(600, 600)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;
        }

        //private int _vertexBufferObject;
        //private int _vertexArrayObject;
        //private int _texture0;
        //private int _texture1;
        private Shader20220413 _shader;
        private VertexBuffer20220413 _vertexBuffer;
        private Texture20220413 _texture0;
        private Texture20220413 _texture1;
        private float _angle;

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
    oColor = mix(texture(uTexture0, ioTexCoord), texture(uTexture1, ioTexCoord), 0.5) * vec4(ioColor, 1.0); 
}";

        private float[] _positionColor =
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

            _vertexBuffer = new VertexBuffer20220413(
             12,
             new[] {
                    new VertexBufferLayout20220413.Part { Identifier = VertexBufferLayout20220413.PartIdentifier.Position0, Type = VertexBufferLayout20220413.ElementType.Single, Count = 3 },
                    new VertexBufferLayout20220413.Part { Identifier = VertexBufferLayout20220413.PartIdentifier.Color0, Type = VertexBufferLayout20220413.ElementType.Single, Count = 3 },
                    new VertexBufferLayout20220413.Part { Identifier = VertexBufferLayout20220413.PartIdentifier.Uv0, Type = VertexBufferLayout20220413.ElementType.Single, Count = 2 },
             },
             VertexBufferUsage.StaticDraw);

            _vertexBuffer.Bind();

            _vertexBuffer.Write(ref _positionColor);


            _shader = new Shader20220413(vertexShaderSource, fragmentShaderSource);


            _texture0 = new Texture20220413("resources/textures/orange-transparent-1024x1024.png");
            //_texture0 = new Texture("resources/textures/orange-transparent-2048x2048.png");
            //_texture0 = new Texture("resources/textures/orange-white-2048x2048.jpg");
            //_texture0 = new Texture("resources/textures/orange-transparent-2048x2048.png");

            //_texture1 = new Texture("resources/textures/smiley-1024x1024.jpg");
            _texture1 = new Texture20220413("resources/textures/smiley-transparent-1024x1024.png");

            //Camera.Position.Y = 200;
        }

        public void UnLoad()
        {
            _texture0.Dispose();
            _texture1.Dispose();
        }

        public void Render()
        {
            Clear(ClearFlag.ColorBuffer);

            _shader.Use();

            _vertexBuffer.Bind();

            _vertexBuffer.EnableElements();

            GL.ActiveTexture(TextureUnit.Texture0);
            _texture0.Bind();

            GL.ActiveTexture(TextureUnit.Texture1);
            _texture1.Bind();

            var model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_angle));
            model *= Matrix4.CreateTranslation(100.0f, 0.0f, 0.0f);

            _shader.Uniform("model", model);
            _shader.Uniform("view", ViewMatrix);
            _shader.Uniform("projection", ProjectionMatrix);
            _shader.Uniform("uTexture0", 0);
            _shader.Uniform("uTexture1", 1);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public void Update()
        {
            _angle += (float)(45f * ElapsedTime);
        }
    }
}

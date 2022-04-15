using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn024-Mixed-Colored-Textered-Rendering")]
    public class Learn024 : AbstractSketch
    {
        public Learn024()
            : base(8, 8)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;

            Samples = 8;
        }

        const string vertexShaderSource = @"
#version 330 core

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

layout(location = 0) in vec3 iPosition;  
layout(location = 1) in vec3 iColor;
layout(location = 2) in vec2 iTexCoord;

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

uniform sampler2D uTexture1;

in vec3 ioColor;
in vec2 ioTexCoord;

out vec4 oColor;

void main()
{
    //oColor = vec4(ioColor, 1.0);
    oColor = texture(uTexture1, ioTexCoord);
}";

        private Texture _texture0;

        private Texture _texture1;

        private Shader _shader;

        private readonly float[] _vertices =
        {
             // positions        // colors
             2f, -2f, -2.0f,  1.0f, 0.0f, 0.0f,   1.0f, -1.0f,  // bottom right
            -2f, -2f, -2.0f,  0.0f, 1.0f, 0.0f,   0.0f, -1.0f, // bottom left
             2f,  2f, -2.0f,  0.0f, 0.0f, 1.0f,   1.0f, 0.0f, // top right
            -2f, -2f, -2.0f,  0.0f, 1.0f, 0.0f,   0.0f, -1.0f, // bottom left
            -2f,  2f, -2.0f,  0.0f, 0.0f, 1.0f,   0.0f, 0.0f,  // top left 
             2f,  2f, -2.0f,  0.0f, 0.0f, 1.0f,   1.0f, 0.0f, // top right
        };

        public void Load()
        {
            Graphics.ClearColor(255, 255, 255);

            //Graphics.CurrentShader = new Shader(vertexShaderSource, fragmentShaderSource);

            QueryGraphicsCardCapabilities();

            _shader = new Shader(vertexShaderSource, fragmentShaderSource);

            _texture0 = new Texture("resources/textures/orange-transparent-1024x1024.png");
            //_texture0.UnitAndBind(TextureUnit.Texture0);

            _texture1 = new Texture("resources/textures/smiley-transparent-1024x1024.png");
            //_texture1.UnitAndBind(TextureUnit.Texture1);

            Graphics.VertexBuffer.Bind();

            Graphics.VertexBuffer.Write(_vertices);

        }

        public void UnLoad()
        {

        }

        public void Render()
        {
            Graphics.ClearBuffers();

            //Graphics.Enable(EnableFlag.DepthTest);

            var grey = 0;
            var greystep = 20;

            var random = new Random(0);

            Graphics.PushMatrix();
            {
                var y = 3f;
                var z = (float)(-2 - 8 * random.NextDouble());

                _shader.Use();

                _texture0.Bind();

                Graphics.VertexBuffer.Bind();
                Graphics.VertexBuffer.EnableElements(_shader);

                var model = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);

                _shader.Uniform("model", model);
                _shader.Uniform("view", Camera.ViewMatrix());
                _shader.Uniform("projection", Camera.ProjectionMatrix());
                _shader.Uniform("uTexture1", 0);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                //{
                //    // default must be in center
                //    Graphics.Quad();
                //}

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(-3f, y, z);

                //    // default
                //    // pre-translated
                //    Graphics.Quad();

                //    Graphics.PopMatrix();
                //}

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Rotate(45f, Axis.Z);

                //    Graphics.Scale(0.5f, 0.5f);

                //    Graphics.Translate(-1.5f, y, z);

                //    // default
                //    // pre-rotated, -scaled and -translated
                //    Graphics.Quad()
                //        .Color(255, 0, 0);

                //    Graphics.PopMatrix();
                //}

                //{
                //    Graphics.PushMatrix();

                //    // rotation / translate / scale order is important
                //    // is identical too previous quad
                //    Graphics.Quad()
                //        .Rotate(45f, Axis.Z)
                //        .Scale(0.5f, 0.5f)
                //        .Translate(0f, y, z)
                //        .Color(200, 0, 0);

                //    Graphics.PopMatrix();
                //}

                //{
                //    Graphics.PushMatrix();

                //    // rotation / translate / scale order is important
                //    // see difference with previous quad
                //    Graphics.Quad()
                //        .Rotate(45f, Axis.Z)
                //        .Translate(1.5f, y, z)
                //        .Scale(0.5f, 0.5f)
                //        .Color(150, 0, 0);

                //    Graphics.PopMatrix();
                //}

                //{
                //    Graphics.PushMatrix();

                //    // different width and height
                //    Graphics.Translate(3f, y, (float)(-2 - 8 * random.NextDouble()));

                //    Graphics.Quad()
                //        .Width(0.5f)
                //        .Height(1.5f)
                //        .Color(grey);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}





                //y = 1.5f;

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(-3f, y, (float)(-2 - 8 * random.NextDouble()));

                //    Graphics.Quad()
                //        .Color(grey);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(-3f, y, (float)(-2 - 8 * random.NextDouble()));

                //    // different position
                //    Graphics.Quad()
                //        .Position(0.5f, -0.50f)
                //        .Color(0, 0, 255);

                //    Graphics.PopMatrix();
                //}

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(-3f, y, (float)(-2 - 8 * random.NextDouble()));

                //    // texture
                //    Graphics.Quad()
                //        .Texture(_texture0);

                //    Graphics.PopMatrix();
                //}



                //{
                //    Graphics.PushMatrix();

                //    Graphics.Scale(0.5f, 0.5f);

                //    // default
                //    Graphics.Quad()
                //        .Translate(-3f, y, z)
                //        .Color(grey);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    // default
                //    Graphics.Quad()
                //        .Scale(0.5f, 0.5f)
                //        .Translate(-1.5f, y, z)
                //        .Color(grey);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    // default
                //    Graphics.Quad()
                //        .Scale(0.5f, 0.5f)
                //        .Translate(0, y, z)
                //        .Rotate(45, Axis.Z)
                //        .Color(grey);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(-3f, y, (float)(-2 - 8 * random.NextDouble()));

                //    // default
                //    Graphics.Quad();

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(-1.5f, y, (float)(-2 - 8 * random.NextDouble()));

                //    // color
                //    Graphics.Quad()
                //        .Color(grey);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(0, y, (float)(-2 - 8 * random.NextDouble()));

                //    // position + color
                //    Graphics.Quad()
                //        .Position(0, 0)
                //        .Color(grey);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    // position + texture
                //    Graphics.Translate(1.5f, y, (float)(-2 - 8 * random.NextDouble()));

                //    Graphics.Quad()
                //        .Position(0, 0)
                //        .Texture(_texture0);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(3f, y, (float)(-2 - 8 * random.NextDouble()));

                //    // position + texture + color
                //    Graphics.Quad()
                //        .Position(0, 0)
                //        .Color(grey)
                //        .Texture(_texture0);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}





                //y = 0;

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(-3f, y, (float)(-2 - 8 * random.NextDouble()));

                //    // position + texture + texture + color
                //    Graphics.Quad()
                //        .Position(0, 0)
                //        .Color(grey)
                //        .Texture(_texture0)
                //        .Texture(_texture1);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(-1.5f, y, (float)(-2 - 8 * random.NextDouble()));

                //    // position + normal
                //    Graphics.Quad()
                //        .Position(0, 0)
                //        .Normal(0, 0, 1);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(0, y, (float)(-2 - 8 * random.NextDouble()));

                //    // position + normal + color
                //    Graphics.Quad()
                //        .Position(0, 0)
                //        .Normal(0, 0, 1)
                //        .Color(grey);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    // position + normal + texture + color
                //    Graphics.Translate(1.5f, y, (float)(-2 - 8 * random.NextDouble()));

                //    Graphics.Quad()
                //        .Position(0, 0)
                //        .Normal(0, 0, 1)
                //        .Color(grey)
                //        .Texture(_texture0)
                //        .Texture(_texture1);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(3f, y, (float)(-2 - 8 * random.NextDouble()));

                //    // position + normal + texture + texture + color
                //    Graphics.Quad()
                //        .Position(0, 0)
                //        .Normal(0, 0, 1)
                //        .Color(grey)
                //        .Texture(_texture0)
                //        .Texture(_texture1);

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}






                //y = -1.5f;

                //{
                //    Graphics.PushMatrix();

                //    Graphics.Translate(-3f, y, (float)(-2 - 8 * random.NextDouble()));

                //    {
                //        // position + normal + texture + texture + color
                //        var geometry = Graphics.CreateGeometry(GeometryType.TriangleStrip);

                //        geometry.Vertex(0.5f, -0.5f).Normal(0, 0, 1).Color(grey).Uv0(0, 0).Uv1(0, 0);
                //        geometry.Vertex(-0.5f, -0.5f).Normal(0, 0, 1).Color(0, 0, 1f).Uv0(0, 0).Uv1(0, 0);
                //        geometry.Vertex(0.5f, 0.5f).Normal(0, 0, 1).Color(255, 0, 255).Uv0(0, 0).Uv1(0, 0);
                //        geometry.Vertex(-0.5f, 0.5f).Normal(0, 0, 1).Color(255, 255, 0).Uv0(0, 0).Uv1(0, 0);
                //    }

                //    Graphics.PopMatrix();

                //    grey += greystep;
                //}

            }
            Graphics.PopMatrix();
        }

        public void Update()
        {
        }
    }
}

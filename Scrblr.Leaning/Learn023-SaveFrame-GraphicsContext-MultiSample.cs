using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn023-SaveFrame-GraphicsContext-MultiSample")]
    public class Learn023 : AbstractSketch2d20220413
    {
        public Learn023()
            : base(600, 600)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;

            Samples = 8;
        }

        const string vertexShaderSource = @"
#version 330 core

in vec3 iPosition;  
in vec4 iColor;

out vec4 ioColor;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

void main(void)
{
    gl_Position = vec4(iPosition, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;

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

        public void Load()
        {
            Graphics.ClearColor(255, 255, 255);

            Graphics.CurrentShader = new Shader20220413(vertexShaderSource, fragmentShaderSource);

            QueryGraphicsCardCapabilities();
        }

        public void UnLoad()
        {

        }

        public void Render()
        {
            Graphics.ClearBuffers();

            Graphics.PushMatrix();
            {
                Rotate(12);

                Translate(0, 0);

                Graphics.Fill(Color4.IndianRed);

                Graphics.Rectangle(0, 0, 100, 100);

                {
                    Graphics.PushMatrix();

                    Translate(100, 0, -2);

                    Graphics.Fill(30);

                    Graphics.Quad(
                        50, 50,
                        50, -50,
                        -50, 50,
                        -50, -50
                        );

                    Graphics.PopMatrix();
                }

                {
                    Graphics.PushMatrix();

                    Translate(50, 0, -4);

                    Graphics.Fill(60);

                    Graphics.Quad(
                        -50, -50,
                        50, -50,
                        -50, 50,
                        50, 50);

                    Graphics.PopMatrix();
                }

                {
                    Graphics.PushMatrix();

                    Translate(0, 0, -6);

                    Graphics.Fill(90);

                    Graphics.Quad(-50, 50, 50, 50, -50, -50, 50, -50);

                    Graphics.PopMatrix();
                }

                {
                    Graphics.PushMatrix();

                    Translate(0, -50, -8);

                    Graphics.Fill(120);

                    Graphics.Quad(
                        50, -50,
                        -50, -50,
                        50, 50,
                        -50, 50);

                    Graphics.PopMatrix();
                }
            }
            Graphics.PopMatrix();
        }

        public void Update()
        {

        }
    }
}

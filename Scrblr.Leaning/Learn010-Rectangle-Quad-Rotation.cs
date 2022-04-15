using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn010-Rectangle-Quad-Rotation")]
    public class Learn010 : AbstractSketch20220406
    {
        public Learn010()
            : base(600, 600)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;

            SketchOrigin = SketchOrigin.Center;
        }

        public void Load()
        {
            ClearColor(255, 255, 255);

            const string vertexShaderSource = @"
#version 330 core

in vec3 iPosition;  
in vec4 iColor;

out vec4 ioColor; // output a color to the fragment shader

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

            Shader = new Shader20220413(vertexShaderSource, fragmentShaderSource);

            QueryGraphicsCardCapabilities();

            Camera.Position.Y = 300;
        }

        public void UnLoad()
        {
            
        }

        public void Render()
        {
            Clear(ClearFlag.ColorBuffer);

            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);

            PushMatrix();
            {
                Rotate(45);

                Translate(0, 0);

                Fill(Color4.Orange);

                Rectangle(0, 0, 100, 100);

                {
                    PushMatrix();

                    Translate(100, 0);

                    Fill(Color4.Blue);

                    Quad(
                        50, 50, 
                        50, -50,
                        -50, 50,
                        -50, -50
                        );

                    PopMatrix();
                }

                {
                    PushMatrix();

                    Translate(-100, 0);

                    Fill(Color4.Green);

                    Quad(
                        -50, -50,
                        50, -50,
                        -50, 50,
                        50, 50);

                    PopMatrix();
                }

                {
                    PushMatrix();

                    Translate(0, 100);

                    Fill(Color4.Blue);

                    Quad(-50, 50, 50, 50, -50, -50, 50, -50);

                    PopMatrix();
                }

                {
                    PushMatrix();

                    Translate(0, -100);

                    Fill(Color4.Blue);

                    Quad(50, -50, -50, -50, 50, 50, -50, 50);

                    PopMatrix();
                }
            }
            PopMatrix();

            Translate(155, 200);

            Fill(Color4.Yellow);

            Rectangle(0, 0, 100, 100);
        }

        public void Update()
        {
        }
    }
}

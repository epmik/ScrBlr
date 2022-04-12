using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn009-Coordinates")]
    public class Learn009 : AbstractSketch20220406
    {
        public Learn009()
            : base(600, 600)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;

            SketchOrigin = SketchOrigin.TopLeft;
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

            Shader = new Shader(vertexShaderSource, fragmentShaderSource);

            QueryGraphicsCardCapabilities();
        }

        public void UnLoad()
        {
            
        }

        public void Render()
        {
            Clear(ClearFlag.Color);

            PushMatrix();
            {
                Rotate(45);

                Translate(0, 0);

                Fill(Color4.Orange);

                Rectangle(0, 0, 100, 100);

                PushMatrix();
                {
                    Rotate(25);

                    Translate(50, 50);

                    Fill(Color4.Blue);

                    Rectangle(0, 0, 100, 100);
                }
                PopMatrix();
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

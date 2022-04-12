using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn021-FrameBufferClass-MultiSamples")]
    public class Learn021 : AbstractSketch2d
    {
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

        private FrameBuffer _frameBuffer;

        public Learn021()
            : base(600, 600)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;

            Samples = 8;
            DepthBits = 32;
            StencilBits = 24;
        }

        public void Load()
        {
            ClearColor(255, 255, 255);

            Graphics.CurrentShader = new Shader(vertexShaderSource, fragmentShaderSource);

            QueryGraphicsCardCapabilities();

            _frameBuffer = new FrameBuffer(
                WindowWidth,
                WindowHeight,
                depthBits: DepthBits, 
                stencilBits: StencilBits,
                samples: Samples);

            Debug.WriteLine($"_frameBuffer.Status(): {_frameBuffer.Status()}");
        }

        public void UnLoad()
        {

        }

        public void Render()
        {
            Clear(ClearFlag.Color);

            Graphics.PushMatrix();
            {
                Rotate(12);

                Translate(0, 0);

                Graphics.Fill(Color4.IndianRed);

                Graphics.Rectangle(0, 0, 100, 100);

                {
                    Graphics.PushMatrix();

                    Translate(100, 0);

                    Graphics.Fill(Color4.Red);

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

                    Translate(-100, 0);

                    Graphics.Fill(Color4.Yellow);

                    Graphics.Quad(
                        -50, -50,
                        50, -50,
                        -50, 50,
                        50, 50);

                    Graphics.PopMatrix();
                }

                {
                    Graphics.PushMatrix();

                    Translate(0, 100);

                    Graphics.Fill(Color4.Orange);

                    Graphics.Quad(-50, 50, 50, 50, -50, -50, 50, -50);

                    Graphics.PopMatrix();
                }

                {
                    Graphics.PushMatrix();

                    Translate(0, -100);

                    Graphics.Fill(Color4.Blue);

                    Graphics.LineLoop(
                        50, -50, 
                        -50, -50, 
                        50, 50, 
                        -50, 50);

                    Graphics.PopMatrix();
                }

                {
                    Graphics.PushMatrix();

                    Translate(0, -150);

                    {
                        var shape = Graphics.CreateGeometry(GeometryType.Lines);

                        shape.Vertex(50, -50).Color(Color4.Blue).Weight(20);

                        shape.Vertex(-50, -50).Color(255, 255, 255).Weight(10);

                        shape.Vertex(50, 50).Color(255, 0, 255).Weight(5);

                        shape.Vertex(-50, 50).Color(255, 255, 0).Weight(50);
                    }

                    Graphics.PopMatrix();
                }
            }
            Graphics.PopMatrix();

            Translate(155, 200);

            Graphics.Fill(Color4.LightGoldenrodYellow);

            Graphics.Rectangle(0, 0, 100, 100);
        }

        public void Update()
        {
        }
    }
}

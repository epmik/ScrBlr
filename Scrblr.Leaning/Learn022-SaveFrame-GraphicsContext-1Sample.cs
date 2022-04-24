using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn022-SaveFrame-GraphicsContext-1Sample")]
    public class Learn022 : AbstractSketch
    {
//        const string vertexShaderSource = @"
//#version 330 core

//in vec3 iPosition;  
//in vec4 iColor;

//out vec4 ioColor;

//uniform mat4 uModelMatrix;
//uniform mat4 uViewMatrix;
//uniform mat4 uProjectionMatrix;

//void main(void)
//{
//    gl_Position = vec4(iPosition, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;

//	ioColor = iColor;
//}";

//        const string fragmentShaderSource = @"
//#version 330 core

//in vec4 ioColor;

//out vec4 oColor;

//void main()
//{
//    oColor = ioColor;
//}";

        public Learn022()
            : base(600, 600)
        {
            Samples = 1;
        }

        public void Load()
        {
            Graphics.ClearColor(255, 255, 255);

            //Graphics.CurrentShader = new Shader20220413(vertexShaderSource, fragmentShaderSource);

            QueryGraphicsCardCapabilities();
        }

        public void Render()
        {
            Graphics.ClearBuffers(ClearFlag.ColorBuffer);

            Graphics.PushMatrix();
            {
                Graphics.Rotate(12);

                Graphics.Translate(0, 0, -2);

                Graphics.Rectangle().Size(100).Color(Color4.IndianRed);

                {
                    Graphics.PushMatrix();

                    Graphics.Translate(100, 0, -2);

                    Graphics.Quad().Points(
                        -50,  50,
                        -50, -50,
                         50,  50,
                         50, -50).Color(Color4.Red);

                    Graphics.PopMatrix();
                }

                {
                    Graphics.PushMatrix();

                    Graphics.Translate(-100, 0, -2);

                    Graphics.Quad().Points(
                        -50, 50,
                        -50, -50,
                         50, 50,
                         50, -50).Color(Color4.Yellow);

                    Graphics.PopMatrix();
                }

                {
                    Graphics.PushMatrix();

                    Graphics.Translate(0, 100);

                    Graphics.Quad().Points(
                        -50, 50,
                        -50, -50,
                         50, 50,
                         50, -50).Color(Color4.Orange);

                    Graphics.PopMatrix();
                }

                ////{
                ////    Graphics.PushMatrix();

                ////    Translate(0, -100, -2);

                ////    Graphics.Fill(Color4.Blue);

                ////    Graphics.LineLoop(
                ////        50, -50, 
                ////        -50, -50, 
                ////        50, 50, 
                ////        -50, 50);

                ////    Graphics.PopMatrix();
                ////}

                ////{
                ////    Graphics.PushMatrix();

                ////    Translate(0, -150, -2);

                ////    {
                ////        var shape = Graphics.CreateGeometry(GeometryType20220413.Lines);

                ////        shape.Vertex(50, -50).Color(Color4.Blue).Weight(20);

                ////        shape.Vertex(-50, -50).Color(255, 255, 255).Weight(10);

                ////        shape.Vertex(50, 50).Color(255, 0, 255).Weight(5);

                ////        shape.Vertex(-50, 50).Color(255, 255, 0).Weight(50);
                ////    }

                ////    Graphics.PopMatrix();
                ////}
            }
            Graphics.PopMatrix();

            Graphics.Translate(155, 200, -2);

            Graphics.Rectangle().Size(100).Color(Color4.LightGoldenrodYellow);
        }
    }
}

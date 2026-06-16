using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scrblr.Sketches
{
    [Sketch(Name = "Sketch005-gradient-in-device-coordinates")]
    public class Sketch005 : AbstractSketch
    {

        const string vertexShaderSource = @"
#version 330 core

layout(location = 0) in vec3 iPosition;
layout(location = 1) in vec4 iColor;

out vec4 ioColor;

void main(void)
{
    ioColor = iColor;

    gl_Position = vec4(iPosition, 1.0);
}
";

        const string fragmentShaderSource = @"
#version 330

in vec4 ioColor;

out vec4 oColor;

void main()
{
    oColor = ioColor; 
}
";

        private Shader _shader;

        private VertexBuffer _vertexBuffer;

        public Sketch005()
            : base(1024, 1024)
        {

        }

        public void Load()
        {
            var y = -1f;
            var offset = 2f / (float)Graphics.Height;

            var vertices = new float[(Graphics.Height + 1) * 2 * 7];

            var index = 0;

            // blue - orange
            var fromColor = new Color4(204, 102, 0, 255);
            var toColor = new Color4(0, 102, 153, 255);

            // orange - fuchia
            //fromColor = new Color4(255, 9, 158, 255);
            //toColor = new Color4(255, 140, 57, 255);

            // orange - red 
            fromColor = new Color4(255, 54, 0, 255);
            toColor = new Color4(255, 144, 0, 255);

            // pink - red
            //fromColor = new Color4(255, 54, 0, 255);
            //toColor = new Color4(255, 9, 158, 255);

            while (y <= 1f)
            {
                var color = Utility.LerpColor(fromColor, toColor, Utility.ReBase(y, -1f, 1f, 0f, 1f));

                vertices[index++] = -1f;
                vertices[index++] = y;
                vertices[index++] = 0f;

                vertices[index++] = color.R;
                vertices[index++] = color.G;
                vertices[index++] = color.B;
                vertices[index++] = color.A;

                vertices[index++] = 1f;
                vertices[index++] = y;
                vertices[index++] = 0;

                vertices[index++] = color.R;
                vertices[index++] = color.G;
                vertices[index++] = color.B;
                vertices[index++] = color.A;

                y += offset;
            }


            _vertexBuffer = new VertexBuffer(
                (Graphics.Height + 1) * 2,
                new[] {
                    new VertexMapping.Map { VertexFlag = VertexFlag.Position0, ElementType = VertexMapping.ElementType.Single, Count = 3 }, // location 0 - name iPosition0
                    new VertexMapping.Map { VertexFlag = VertexFlag.Color0, ElementType = VertexMapping.ElementType.Single, Count = 4 },    // location 1 - name iColor0
                },
                VertexBufferUsage.DynamicDraw);

            _vertexBuffer.Bind();

            _vertexBuffer.WriteRaw(vertices);


            _shader = new Shader(vertexShaderSource, fragmentShaderSource);

            _shader.Use();
        }

        public void UnLoad()
        {
            _vertexBuffer.Dispose();

            _shader.Dispose();
        }

        public void Update()
        {
        }

        public void Render()
        {
            Graphics.Render(_vertexBuffer, _shader, GeometryType.Lines);
        }

        public void KeyUp(KeyboardKeyEventArgs a)
        {
            if (a.Key == Keys.Tab)
            {
                
            }
            else if (a.Key == Keys.S)
            {
                Random.ReSeed();
            }
        }

        public void MouseMove(MouseMoveEventArgs a)
        {
        }

        public void CameraMouseDown(MouseButtonEventArgs a)
        {
        }

        public void CameraMouseUp(MouseButtonEventArgs a)
        {
        }
    }
}

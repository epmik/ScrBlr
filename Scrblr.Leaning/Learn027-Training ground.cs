using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn027-Training ground")]
    public class Learn027 : AbstractSketch
    {
        int _rows = 5, _columns = 5;
        float _rowHeight, _columnWidth, _rowMargin, _columnMargin, _columnsStartX, _rowStartY;
        float _rotationDegreesPerSecond = 90, _degrees;
        private Texture _gridNoTransparency, _gridWithTransparency, _smileyWithTransparency;

        public Learn027()
            : base(8, 8)
        {
            _columnMargin = FrustumWidth / (float)((_columns * 2) + (_columns + 1));

            _rowMargin = FrustumHeight / (float)((_rows * 2) + (_rows + 1));

            if(_columnMargin < _rowMargin)
            {
                _rowMargin = _columnMargin;
            }
            else
            {
                _columnMargin = _rowMargin;
            }

            _columnWidth = _columnMargin + _columnMargin;
            _rowHeight = _rowMargin + _rowMargin;

            _columnsStartX = (FrustumWidth / -2) + _columnWidth;
            _rowStartY = (FrustumHeight / 2) - _rowHeight;
        }

        public void Load()
        {
            _gridNoTransparency = new Texture("resources/textures/orange-white-1024x1024.jpg");

            _gridWithTransparency = new Texture("resources/textures/orange-transparent-1024x1024.png");

            _smileyWithTransparency = new Texture("resources/textures/smiley-transparent-1024x1024.png");
        }

        public void Update()
        {
            _degrees += (float)(_rotationDegreesPerSecond * ElapsedTime);

            if(_degrees >= 360f)
            {
                _degrees -= 360f;
            }
        }

        public void Render()
        {
            Graphics.ClearColor(128);

            var columnsX = _columnsStartX;
            var rowY = _rowStartY;

            //// default, black at center of screen
            Graphics.Rectangle();

            //GL.Enable(EnableCap.Texture2D);
            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


            // column 1 row 1
            // translated and red (int)
            Graphics.PushMatrix();
            Graphics.Translate(columnsX, rowY);
            Graphics.Rectangle().Color(255, 0, 0);
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;


            // column 2 row 1
            // grey (single parameter) and rotated
            Graphics.PushMatrix();
            Graphics.Rotate(45, Axis.Y);
            Graphics.Translate(columnsX, rowY);
            Graphics.Rectangle().Color(220);
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;


            // column 3 row 1
            // blue (float) and rotating
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(columnsX, rowY);
            Graphics.Rectangle().Color(0, 0, 1f);
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;

            // column 4 row 1
            // green (float + transparency) and rotating
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Translate(columnsX, rowY);
            Graphics.Rectangle().Color(0, 1f, 0, 0.5f);
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;


            // column 5 row 1
            // green, width and height, 
            Graphics.PushMatrix();
            Graphics.Translate(columnsX, rowY);
            Graphics.Rectangle().Width(1.25f).Height(0.5f).Color(0, 1f, 0);
            Graphics.PopMatrix();



            columnsX = _columnsStartX;
            rowY -= _rowHeight + _rowMargin;



            // column 1 row 2
            Graphics.PushMatrix();
            Graphics.Translate(columnsX, rowY);
            Graphics.Rectangle().Texture(_gridNoTransparency);
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // column 2 row 2
            // texture with transparency
            Graphics.PushMatrix();
            Graphics.Translate(columnsX, rowY);
            Graphics.Rectangle().Texture(_gridWithTransparency);
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // column 3 row 2
            // red texture no transparency
            Graphics.PushMatrix();
            Graphics.Translate(columnsX, rowY);
            Graphics.Rectangle().Color(0, 225, 225).Texture(_gridNoTransparency);
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // column 4 row 2
            // scaled, red, texture with transparency
            Graphics.PushMatrix();
            Graphics.Scale(0.75f);
            Graphics.Translate(columnsX, rowY);
            Graphics.Rectangle().Color(0, 225, 225).Texture(_gridWithTransparency);
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // column 5 row 2
            // scaled, 2 textures with transparency
            Graphics.PushMatrix();
            Graphics.Scale(0.75f, 1.25f);
            Graphics.Translate(columnsX, rowY);
            Graphics.Rectangle().Texture(_gridNoTransparency).Texture(_smileyWithTransparency);
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            columnsX = _columnsStartX;
            rowY -= _rowHeight + _rowMargin;



            // column 1 row 3
            // rotated, color, 2 textures with transparency
            Graphics.PushMatrix();
            Graphics.Rotate(45, Axis.Z);
            Graphics.Translate(columnsX, rowY);
            Graphics.Rectangle().Color(0, 225, 225).Texture(_gridNoTransparency).Texture(_smileyWithTransparency);
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // column 2 row 3
            // default quad
            Graphics.PushMatrix();
            Graphics.Translate(columnsX, rowY);
            Graphics
                .Quad();
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // skip default rectangle in the middle
            columnsX += _columnWidth + _columnMargin;



            // column 4 row 3
            // red, points float[] array containing 2 values per point
            Graphics.PushMatrix();
            Graphics.Translate(columnsX, rowY);
            Graphics
                .Quad()
                .Color(1f, 0f, 0f)
                .Points(new float[] 
                { 
                    0.7f, 0.3f, 
                    0.8f, -0.2f, 
                    -0.3f, 0.9f, 
                    -0.2f, -0.7f 
                });
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // column 5 row 3
            // grey, points float[] array containing 3 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Translate(columnsX, rowY);
            Graphics
                .Quad()
                .Color(0.85f)
                .Points(new float[] 
                { 
                    0.7f, 0.3f, 0.0f, 
                    0.8f, -0.2f, 0.0f, 
                    -0.3f, 0.9f, 0.0f, 
                    -0.2f, -0.7f, 0.0f
                });
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            columnsX = _columnsStartX;
            rowY -= _rowHeight + _rowMargin;



            // column 1 row 4
            // grey transparent, points float[][] array containing 2 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(columnsX, rowY);
            Graphics
                .Quad()
                .Color(0.85f, 0.5f)
                .Points(new float[,] 
                { 
                    { 0.7f, 0.3f }, 
                    { 0.8f, -0.2f }, 
                    { -0.3f, 0.9f }, 
                    { -0.2f, -0.7f } 
                });
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // column 2 row 4
            // points float[][] array containing 3 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(25, Axis.Z);
            Graphics.Translate(columnsX, rowY);
            Graphics
                .Quad()
                .Color(100, 100, 0)
                .Points(new float[,]
                {
                    { 0.7f, 0.3f, 0.0f },
                    { 0.8f, -0.2f, 0.0f },
                    { -0.3f, 0.9f, 0.0f },
                    { -0.2f, -0.7f, 0.0f }
                });
            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // column 3 row 4
            // points float[][] array containing 3 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(columnsX, rowY);
            Graphics
                .Quad()
                .Points(new float[,]
                {
                    { 0.7f, 0.3f, 0.0f },
                    { 0.8f, -0.2f, 0.0f },
                    { -0.3f, 0.9f, 0.0f },
                    { -0.2f, -0.7f, 0.0f }
                })
                .Texture(_gridNoTransparency);

            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // column 4 row 4
            // points float[][] array containing 3 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Translate(columnsX, rowY);
            Graphics
                .Quad()
                .Points(new float[,]
                {
                    { 0.7f, 0.3f, 0.0f },
                    { 0.8f, -0.2f, 0.0f },
                    { -0.3f, 0.9f, 0.0f },
                    { -0.2f, -0.7f, 0.0f }
                })
                .Texture(_gridWithTransparency);

            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // column 5 row 4
            // points float[][] array containing 3 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Translate(columnsX, rowY);
            Graphics
                .Quad()
                .Points(new float[,]
                {
                    { 0.7f, 0.3f, 0.0f },
                    { 0.8f, -0.2f, 0.0f },
                    { -0.3f, 0.9f, 0.0f },
                    { -0.2f, -0.7f, 0.0f }
                })
                .Uvs(new float[,] 
                { 
                    { 0.1f, 0.2f }, 
                    { 0.0f, 0.9f }, 
                    { 0.85f, 0.0f }, 
                    { 0.9f, 1.0f } 
                })
                .Texture(_gridWithTransparency);

            Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;



            // column 4 row 3
            // rotated, color, 2 textures with transparency
            //Graphics.PushMatrix();
            //Graphics.Translate(columnsX, rowY);
            //Graphics
            //    .Quad()
            //    .Points(new float[][] { new float[] { 0.7f, 0.3f }, new float[] { 0.8f, -0.2f }, new float[] { -0.3f, 0.9f }, new float[] { -0.2f, -0.7f } })
            //    .Color(0, 225, 225)
            //    .Uvs(new float[][] { new float[] { 0.1f, 0.9f }, new float[] { 0.0f, 0.2f }, new float[] { 0.85f, 1.0f }, new float[] { 0.9f, 0.0f } })
            //    .Texture(_gridNoTransparency)
            //    .Texture(_smileyWithTransparency);
            //Graphics.PopMatrix();

            columnsX += _columnWidth + _columnMargin;







            //// column 1 row 3
            //// blue, 2 textures with transparency
            //Graphics.PushMatrix();
            //Graphics.Translate(columnsX, rowY);
            //Graphics.Rectangle().Color(0, 0, 1f).Texture(_gridWithTransparency).Texture(_smileyWithTransparency);
            //Graphics.PopMatrix();

            //columnsX += _columnWidth + _columnMargin;



            //Graphics.Disable(EnableFlag.Rendering);

            if (!Graphics.IsEnabled(EnableFlag.Rendering))
            {
                Graphics.ClearBuffers();

                var vertexFlags = VertexFlag.Position0;// | VertexFlag.Color0;// | VertexFlag.Uv0 | VertexFlag.Uv1;

                var shader = Graphics.StandardShader(vertexFlags);

                shader.Use();

                var vertexBuffer = Graphics.StandardVertexBuffer();

                vertexBuffer.Bind();

                vertexBuffer.EnableElements(vertexFlags);

                //_gridNoTransparency.UnitAndBind(TextureUnit.Texture0);
                //_smileyWithTransparency.UnitAndBind(TextureUnit.Texture1);

                shader.Uniform("uModelMatrix", Matrix4.CreateTranslation(0, 0, 0));
                shader.Uniform("uViewMatrix", Graphics.ActiveCamera().ViewMatrix());
                shader.Uniform("uProjectionMatrix", Graphics.ActiveCamera().ProjectionMatrix());
                //shader.Uniform("uTexture0", 0);
                //shader.Uniform("uTexture1", 1);

                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            }
        }
    }
}

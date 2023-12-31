﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn027-Training ground-Rectangle-Quad-3d")]
    public class Learn027 : AbstractSketch
    {
        int _rows = 5, _columns = 5;
        float _rowHeight, _columnWidth, _rowMargin, _columnMargin, _columnsStartX, _rowStartY;
        float _rotationDegreesPerSecond = 90, _degrees;
        private Texture _gridNoTransparency, _gridWithTransparency, _smileyWithTransparency;
        private MethodInfo[] _renderMethodInfoArray;

        public Learn027()
            : base(8, 8)
        {
            _columnMargin = FrustumWidth / (float)((_columns * 2) + (_columns + 1));

            _rowMargin = FrustumHeight / (float)((_rows * 2) + (_rows + 1));

            if (_columnMargin < _rowMargin)
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

            _renderMethodInfoArray = GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(o => o.Name.StartsWith("Render") && o.Name.Length > 6).OrderBy(o => o.Name).ToArray();
        }

        public void Load()
        {
            _gridNoTransparency = new Texture("resources/textures/orange-white-1024x1024.jpg");

            _gridWithTransparency = new Texture("resources/textures/orange-transparent-1024x1024.png");

            _smileyWithTransparency = new Texture("resources/textures/smiley-transparent-1024x1024.png");

            Graphics.ActiveCamera().ProjectionMode = ProjectionMode.Orthographic;
        }

        public void UnLoad()
        {
            _gridNoTransparency.Dispose();
            _gridWithTransparency.Dispose();
            _smileyWithTransparency.Dispose();
        }
        public void Update()
        {
            _degrees += (float)(_rotationDegreesPerSecond * ElapsedTime);

            if (_degrees >= 360f)
            {
                _degrees -= 360f;
            }
        }

        public void Render()
        {
            Graphics.ClearColor(128);

            Graphics.State.Disable(EnableFlag.FrontFaceCulling);
            Graphics.State.Disable(EnableFlag.BackFaceCulling);

            var i = 0;
            var rowY = _rowStartY;

            for (var r = 0; r < _rows && i < _renderMethodInfoArray.Length; r++)
            {
                var columnX = _columnsStartX;

                for (var c = 0; c < _columns && i < _renderMethodInfoArray.Length; c++)
                {
                    _renderMethodInfoArray[i++].Invoke(this, new object[] { columnX, rowY });

                    columnX += _columnWidth + _columnMargin;
                }

                rowY -= _rowHeight + _rowMargin;
            }
        }

        private void Render001(float x, float y)
        {
            //// default
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle();
            Graphics.PopMatrix();
        }

        private void Render002(float x, float y)
        {
            // translated and red (int)
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle().Color(255, 0, 0);
            Graphics.PopMatrix();
        }

        private void Render003(float x, float y)
        {
            // grey (single parameter) and rotated
            Graphics.PushMatrix();
            Graphics.Rotate(45, Axis.Y);
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle().Color(220);
            Graphics.PopMatrix();
        }

        private void Render004(float x, float y)
        {
            // blue (float) and rotating
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle().Color(0, 0, 1f);
            Graphics.PopMatrix();
        }

        private void Render005(float x, float y)
        {
            // green (float + transparency) and rotating
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle().Color(0, 1f, 0, 0.5f);
            Graphics.PopMatrix();
        }

        private void Render006(float x, float y)
        {
            // green, width and height, 
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle().Width(1.25f).Height(0.5f).Color(0, 1f, 0);
            Graphics.PopMatrix();
        }

        private void Render007(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle().Texture(_gridNoTransparency);
            Graphics.PopMatrix();
        }

        private void Render008(float x, float y)
        {
            // texture with transparency
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle().Texture(_gridWithTransparency);
            Graphics.PopMatrix();
        }

        private void Render009(float x, float y)
        {
            // red texture no transparency
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle().Color(0, 225, 225).Texture(_gridNoTransparency);
            Graphics.PopMatrix();
        }

        private void Render010(float x, float y)
        {
            // scaled, red, texture with transparency
            Graphics.PushMatrix();
            Graphics.Scale(0.75f);
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle().Color(0, 225, 225).Texture(_gridWithTransparency);
            Graphics.PopMatrix();
        }

        private void Render011(float x, float y)
        {
            // scaled, 2 textures with transparency
            Graphics.PushMatrix();
            Graphics.Scale(0.75f, 1.25f);
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle().Texture(_gridNoTransparency).Texture(_smileyWithTransparency);
            Graphics.PopMatrix();
        }

        private void Render012(float x, float y)
        {
            // rotated, color, 2 textures with transparency
            Graphics.PushMatrix();
            Graphics.Rotate(45, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Rectangle().Color(0, 225, 225).Texture(_gridNoTransparency).Texture(_smileyWithTransparency);
            Graphics.PopMatrix();
        }

        private void Render013(float x, float y)
        {
            // default quad
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics
                .Quad();
            Graphics.PopMatrix();
        }

        private void Render014(float x, float y)
        {
            // red, points float[] array containing 2 values per point
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics
                .Quad()
                .Color(1f, 0f, 0f)
                .Points(new float[]
                {
                    // CCW
                    // top left
                    -0.7f, 0.3f,
                    // bottom left
                    -0.8f, -0.2f,
                    // top right
                    0.3f, 0.9f,
                    // bottom right
                    0.2f, -0.7f,
                });
            Graphics.PopMatrix();
        }

        private void Render015(float x, float y)
        {
            // grey, points float[] array containing 3 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Quad()
                .Color(0.85f)
                .Points(new float[]
                {
                    // CCW
                    // top left
                    -0.7f, 0.3f, 0.0f,
                    // bottom left
                    -0.8f, -0.2f, 0.0f,
                    // top right
                    0.3f, 0.9f, 0.0f,
                    // bottom right
                    0.2f, -0.7f, 0.0f
                });
            Graphics.PopMatrix();
        }

        private void Render016(float x, float y)
        {
            // grey transparent, points float[][] array containing 2 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Quad()
                .Color(0.85f, 0.5f)
                .Points(new float[,]
                {
                    // CCW
                    // top left
                    { -0.7f, 0.3f },
                    // bottom left
                    { -0.8f, -0.2f },
                    // top right
                    { 0.3f, 0.9f },
                    // bottom right
                    { 0.2f, -0.7f }
                });
            Graphics.PopMatrix();
        }

        private void Render017(float x, float y)
        {
            // points float[][] array containing 3 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(25, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Quad()
                .Color(100, 100, 0)
                .Points(new float[,]
                {
                    // CCW
                    // top left
                    { -0.7f, 0.3f, 0.0f },
                    // bottom left
                    { -0.8f, -0.2f, 0.0f },
                    // top right
                    { 0.3f, 0.9f, 0.0f },
                    // bottom right
                    { 0.2f, -0.7f, 0.0f }
                });
            Graphics.PopMatrix();
        }

        private void Render018(float x, float y)
        {
            // points float[][] array containing 3 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Quad()
                .Points(new float[,]
                {
                    // CCW
                    // top left
                    { -0.7f, 0.3f, 0.0f },
                    // bottom left
                    { -0.8f, -0.2f, 0.0f },
                    // top right
                    { 0.3f, 0.9f, 0.0f },
                    // bottom right
                    { 0.2f, -0.7f, 0.0f }
                })
                .Texture(_gridNoTransparency);

            Graphics.PopMatrix();
        }

        private void Render019(float x, float y)
        {
            // points float[][] array containing 3 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Quad()
                .Points(new float[,]
                {
                    // CCW
                    // top left
                    { -0.7f, 0.3f, 0.0f },
                    // bottom left
                    { -0.8f, -0.2f, 0.0f },
                    // top right
                    { 0.3f, 0.9f, 0.0f },
                    // bottom right
                    { 0.2f, -0.7f, 0.0f }
                })
                .Texture(_gridWithTransparency);

            Graphics.PopMatrix();
        }

        private void Render020(float x, float y)
        {
            // points float[][] array containing 3 values per point
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Quad()
                .Points(new float[,]
                {
                    // CCW
                    // top left
                    { -0.7f, 0.3f, 0.0f },
                    // bottom left
                    { -0.8f, -0.2f, 0.0f },
                    // top right
                    { 0.3f, 0.9f, 0.0f },
                    // bottom right
                    { 0f, -0.7f, 0.0f }
                })
                .Uvs(new float[,]
                {
                    // top left
                    { 0.1f, 0.9f },
                    // bottom left
                    { 0, 0.1f },
                    // top right
                    { 0.85f, 0f },
                    // bottom right
                    { 0.75f, 0.25f },
                })
                .Texture(_gridWithTransparency);

            Graphics.PopMatrix();
        }
    }
}

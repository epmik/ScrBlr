using OpenTK.Graphics.OpenGL4;
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
    [Sketch(Name = "Learn028-Training ground-Circle-Ellipse")]
    public class Learn028 : AbstractSketch
    {
        int _rows = 5, _columns = 5;
        float _rowHeight, _columnWidth, _rowMargin, _columnMargin, _columnsStartX, _rowStartY;
        float _rotationDegreesPerSecond = 90, _degrees;
        private Texture _gridNoTransparency, _gridWithTransparency, _smileyWithTransparency;
        private MethodInfo[] _renderMethodInfoArray;

        public Learn028()
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

            if(_degrees >= 360f)
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

        //private void Render000(float x, float y)
        //{
        //    Graphics.PushMatrix();
        //    Graphics.Translate(x, y, -2f);
        //    Graphics.Rectangle();
        //    Graphics.PopMatrix();
        //}

        private void RenderCircle001(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Circle();
            Graphics.PopMatrix();
        }

        private void RenderCircle002(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Circle(1f).Segments(3).Color(225, 0, 128);
            Graphics.PopMatrix();
        }

        private void RenderCircle003(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Circle().Radius(0.25f).Color(128, 32, 128);
            Graphics.PopMatrix();
        }

        private void RenderCircle004(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Circle().Segments(7).Color(0, 64, 225);
            Graphics.PopMatrix();
        }

        private void RenderCircle005(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Circle().AutoSegments(true).Color(64, 125, 255);
            Graphics.PopMatrix();
        }

        private void RenderCircle006(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Circle().AutoSegments(false).Color(255, 64, 128);
            Graphics.PopMatrix();
        }

        private void RenderCircle007(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Circle().Segments(6).Texture(_gridNoTransparency);
            Graphics.PopMatrix();
        }

        private void RenderCircle008(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Circle().AutoSegments(false).Segments(7).Color(32, 96, 224).Texture(_gridNoTransparency);
            Graphics.PopMatrix();
        }

        private void RenderCircle009(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Circle().AutoSegments(false).Segments(5).Texture(_gridWithTransparency);
            Graphics.PopMatrix();
        }

        private void RenderCircle010(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Circle().AutoSegments(false).Segments(5).Color(32, 96, 224).Texture(_smileyWithTransparency);
            Graphics.PopMatrix();
        }

        private void RenderCircle011(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Circle().AutoSegments(false).Segments(5).Color(154, 96, 0).Texture(_gridNoTransparency).Texture(_smileyWithTransparency);
            Graphics.PopMatrix();
        }






        private void RenderEllipse001(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Ellipse();
            Graphics.PopMatrix();
        }

        private void RenderEllipse002(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Ellipse(1.50f, 1f).Color(225, 0, 128);
            Graphics.PopMatrix();
        }

        private void RenderEllipse003(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Ellipse().Width(0.25f).Height(1.25f).Color(128, 32, 128);
            Graphics.PopMatrix();
        }

        private void RenderEllipse004(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Ellipse().Segments(8).Color(0, 64, 225);
            Graphics.PopMatrix();
        }

        private void RenderEllipse005(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Ellipse().Width(1.25f).Height(0.5f).AutoSegments(true).Color(64, 125, 255);
            Graphics.PopMatrix();
        }

        private void RenderEllipse006(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Ellipse().Width(1.15f).Height(1.55f).AutoSegments(false).Texture(_gridNoTransparency);
            Graphics.PopMatrix();
        }

        private void RenderEllipse007(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Ellipse().AutoSegments(false).Segments(5).Texture(_gridNoTransparency);
            Graphics.PopMatrix();
        }

        private void RenderEllipse008(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Ellipse().AutoSegments(false).Segments(7).Color(32, 96, 224).Texture(_gridNoTransparency);
            Graphics.PopMatrix();
        }

        private void RenderEllipse009(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Ellipse().AutoSegments(false).Segments(5).Texture(_gridWithTransparency);
            Graphics.PopMatrix();
        }

        private void RenderEllipse010(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Circle().AutoSegments(false).Segments(5).Color(32, 96, 224).Texture(_smileyWithTransparency);
            Graphics.PopMatrix();
        }

        private void RenderEllipse011(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Ellipse().AutoSegments(false).Segments(5).Color(154, 96, 0).Texture(_gridNoTransparency).Texture(_smileyWithTransparency);
            Graphics.PopMatrix();
        }
    }
}

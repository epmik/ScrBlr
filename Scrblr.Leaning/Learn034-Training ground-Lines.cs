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
    [Sketch(Name = "Learn034-Training ground-Lines")]
    public class Learn034 : AbstractSketch
    {
        int _rows = 5, _columns = 5;
        float _rowHeight, _columnWidth, _rowMargin, _columnMargin, _columnsStartX, _rowStartY;
        float _rotationDegreesPerSecond = 90, _degrees;
        private Texture _gridNoTransparency, _gridWithTransparency, _smileyWithTransparency;
        private MethodInfo[] _renderMethodInfoArray;

        public Learn034()
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

            ProjectionMode = ProjectionMode.Orthographic;
        }

        public void Load()
        {
            _gridNoTransparency = new Texture("resources/textures/orange-white-1024x1024.jpg");

            _gridWithTransparency = new Texture("resources/textures/orange-transparent-1024x1024.png");

            _smileyWithTransparency = new Texture("resources/textures/smiley-transparent-1024x1024.png");

            //var firstPersonCamera = new FirstPersonCamera
            //{
            //    Fov = 90f,
            //    AspectRatio = FrustumWidth / FrustumHeight,
            //    Near = 1f,
            //    Far = 1000f,
            //    //Position = new Vector3(0, 0, 2),
            //};

            //AttachCamera(firstPersonCamera, true, true);
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

            Graphics.State.Enable(EnableFlag.DepthTest);
            Graphics.State.Disable(EnableFlag.BackFaceCulling);
            Graphics.State.Disable(EnableFlag.FrontFaceCulling);

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

        private void RenderLines000(float x, float y)
        {
            Graphics.PushMatrix();
            //Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Line()
                ;
            Graphics.PopMatrix();
        }

        private void RenderLines001(float x, float y)
        {
            Graphics.PushMatrix();
            //Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Line()
                .To(0f, 1f)
                ;
            Graphics.PopMatrix();
        }

        private void RenderLines002(float x, float y)
        {
            Graphics.PushMatrix();
            //Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Line()
                .From(-0.5f, 0.5f)
                .To(0.5f, -0.5f)
                ;
            Graphics.PopMatrix();
        }

        private void RenderLines003(float x, float y)
        {
            Graphics.PushMatrix();
            //Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Line()
                .Color(255, 0, 0)
                .Width(0.2f)
                .From(0.5f, 0.5f)
                .To(-0.5f, -0.5f)
                ;
            Graphics.PopMatrix();
        }

        private void RenderLines004(float x, float y)
        {
            Graphics.PushMatrix();
            //Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Line()
                .Color(255, 0, 0)
                .Width(0.05f)
                .From(0.0f, 0.0f)
                .To(0.5f,  0.5f)
                .From(0.0f, 0.0f)
                .To(0.5f, -0.5f)
                .From(0.0f, 0.0f)
                .To(-0.5f, -0.5f)
                .From(0.0f, 0.0f)
                .To(-0.5f, 0.5f)
                ;
            Graphics.PopMatrix();
        }

        private void RenderLines005(float x, float y)
        {
            Graphics.PushMatrix();
            //Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Line()
                .Color(255, 0, 0)
                .Width(0.05f)
                .From(0.0f, 0.0f)
                .To(0.5f, 0.5f)
                .Color(0, 255, 0)
                .From(0.0f, 0.0f)
                .To(0.5f, -0.5f)
                .Color(255, 255, 0)
                .From(0.0f, 0.0f)
                .To(-0.5f, -0.5f)
                .Color(255, 0, 255)
                .From(0.0f, 0.0f)
                .To(-0.5f, 0.5f)
                ;
            Graphics.PopMatrix();
        }

        private void RenderLines006(float x, float y)
        {
            Graphics.PushMatrix();
            //Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Line()
                .Color(0, 0, 255)
                .Width(0.01f)
                .From(0.0f, 0.0f)
                .To( 0.5f,  0.5f)
                .To( 0.5f, -0.5f)
                .To(-0.5f, -0.5f)
                .To(-0.5f,  0.5f)
                ;
            Graphics.PopMatrix();
        }

        private void RenderLines007(float x, float y)
        {
            Graphics.PushMatrix();
            //Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Line()
                .Color(0, 0, 255)
                .Width(0.01f)
                .From(0.0f, 0.0f)
                .Width(0.05f)
                .To(0.5f, 0.5f)
                ;
            Graphics.PopMatrix();
        }

        private void RenderLines008(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Line()
                .Color(0, 0, 255)
                .Width(0.01f)
                .From(0.0f, 0.0f)
                .Width(0.05f)
                .To(0.5f, 0.5f)
                .Width(0.75f)
                .To(0.5f, -0.5f)
                .Width(0.01f)
                .To(-0.5f, -0.5f)
                .Width(0.25f)
                .To(-0.5f, 0.5f)
                ;
            Graphics.PopMatrix();
        }

        private void RenderLines009(float x, float y)
        {
            Graphics.PushMatrix();
            //Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics
                .Line()
                .Color(255)
                .From(0.5f, 0.5f)
                .Width(0.75f)
                .Color(128)
                .To(0.5f, -0.5f)
                .Width(0.1f)
                .Color(0)
                .To(-0.5f, -0.5f)
                ;
            Graphics.PopMatrix();
        }
    }
}

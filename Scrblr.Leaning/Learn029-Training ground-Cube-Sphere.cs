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
    [Sketch(Name = "Learn029-Training ground-Cube-Sphere")]
    public class Learn029 : AbstractSketch
    {
        int _rows = 5, _columns = 5;
        float _rowHeight, _columnWidth, _rowMargin, _columnMargin, _columnsStartX, _rowStartY;
        float _rotationDegreesPerSecond = 90, _degrees;
        private Texture _gridNoTransparency, _gridWithTransparency, _smileyWithTransparency;
        private MethodInfo[] _renderMethodInfoArray;

        public Learn029()
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

        private void RenderCube001(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Cube();
            Graphics.PopMatrix();
        }

        private void RenderCube002(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Cube(1f, 0.5f, 0.25f).Color(225, 0, 128);
            Graphics.PopMatrix();
        }

        private void RenderCube003(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Cube().Width(0.50f).Color(128, 32, 128);
            Graphics.PopMatrix();
        }

        private void RenderCube004(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Cube().Height(0.5f).Color(0, 64, 225);
            Graphics.PopMatrix();
        }

        private void RenderCube005(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Translate(x, y, -2f);
            Graphics.Cube().Depth(0.5f).Color(64, 125, 255);
            Graphics.PopMatrix();
        }

        private void RenderCube006(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Cube()
                .Texture(_gridNoTransparency);
            Graphics.PopMatrix();
        }

        private void RenderCube007(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Cube()
                .Texture(_gridWithTransparency);
            Graphics.PopMatrix();
        }

        private void RenderCube008(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Cube()
                .Color(64, 125, 255)
                .Texture(_gridNoTransparency);
            Graphics.PopMatrix();
        }

        private void RenderCube009(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(x, y, -2f);
            Graphics.Cube()
                .Color(64, 125, 255)
                .Texture(_gridWithTransparency);
            Graphics.PopMatrix();
        }







        private void RenderSphere001(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Sphere();
            Graphics.PopMatrix();
        }

        private void RenderSphere002(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Sphere().Color(12, 67, 5);
            Graphics.PopMatrix();
        }

        private void RenderSphere003(float x, float y)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y, -2f);
            Graphics.Sphere().Color(75, 31, 4).Subdivisions(3);
            Graphics.PopMatrix();
        }



        //private void RenderSphere002(float x, float y)
        //{
        //    Graphics.PushMatrix();
        //    Graphics.Translate(x, y, -2f);
        //    Graphics.Sphere(1.50f, 1f).Color(225, 0, 128);
        //    Graphics.PopMatrix();
        //}

        //private void RenderSphere003(float x, float y)
        //{
        //    Graphics.PushMatrix();
        //    Graphics.Translate(x, y, -2f);
        //    Graphics.Sphere().Width(0.25f).Height(1.25f).Color(128, 32, 128);
        //    Graphics.PopMatrix();
        //}

        //private void RenderSphere004(float x, float y)
        //{
        //    Graphics.PushMatrix();
        //    Graphics.Translate(x, y, -2f);
        //    Graphics.Sphere().Segments(8).Color(0, 64, 225);
        //    Graphics.PopMatrix();
        //}

        //private void RenderSphere005(float x, float y)
        //{
        //    Graphics.PushMatrix();
        //    Graphics.Translate(x, y, -2f);
        //    Graphics.Sphere().Width(1.25f).Height(0.5f).AutoSegments(true).Color(64, 125, 255);
        //    Graphics.PopMatrix();
        //}

        //private void RenderSphere006(float x, float y)
        //{
        //    Graphics.PushMatrix();
        //    Graphics.Translate(x, y, -2f);
        //    Graphics.Sphere().Width(1.15f).Height(1.55f).AutoSegments(false).Texture(_gridNoTransparency);
        //    Graphics.PopMatrix();
        //}

        //private void RenderSphere007(float x, float y)
        //{
        //    Graphics.PushMatrix();
        //    Graphics.Translate(x, y, -2f);
        //    Graphics.Sphere().AutoSegments(false).Segments(5).Texture(_gridNoTransparency);
        //    Graphics.PopMatrix();
        //}

        //private void RenderSphere008(float x, float y)
        //{
        //    Graphics.PushMatrix();
        //    Graphics.Rotate(_degrees, Axis.X);
        //    Graphics.Rotate(_degrees, Axis.Z);
        //    Graphics.Translate(x, y, -2f);
        //    Graphics.Sphere().AutoSegments(false).Segments(7).Color(32, 96, 224).Texture(_gridNoTransparency);
        //    Graphics.PopMatrix();
        //}

        //private void RenderSphere009(float x, float y)
        //{
        //    Graphics.PushMatrix();
        //    Graphics.Rotate(_degrees, Axis.X);
        //    Graphics.Rotate(_degrees, Axis.Z);
        //    Graphics.Translate(x, y, -2f);
        //    Graphics.Sphere().AutoSegments(false).Segments(5).Texture(_gridWithTransparency);
        //    Graphics.PopMatrix();
        //}

        //private void RenderSphere010(float x, float y)
        //{
        //    Graphics.PushMatrix();
        //    Graphics.Rotate(_degrees, Axis.X);
        //    Graphics.Rotate(_degrees, Axis.Z);
        //    Graphics.Translate(x, y, -2f);
        //    Graphics.Cube().AutoSegments(false).Segments(5).Color(32, 96, 224).Texture(_smileyWithTransparency);
        //    Graphics.PopMatrix();
        //}

        //private void RenderSphere011(float x, float y)
        //{
        //    Graphics.PushMatrix();
        //    Graphics.Rotate(_degrees, Axis.Y);
        //    Graphics.Rotate(_degrees, Axis.Z);
        //    Graphics.Translate(x, y, -2f);
        //    Graphics.Sphere().AutoSegments(false).Segments(5).Color(154, 96, 0).Texture(_gridNoTransparency).Texture(_smileyWithTransparency);
        //    Graphics.PopMatrix();
        //}
    }
}

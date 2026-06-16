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
    [Sketch(Name = "Sketch002-a-million-rectangles")]
    public class Sketch002 : AbstractSketch
    {
        private MethodInfo[] _renderMethodInfoArray;
        private MethodInfo[] _updateMethodInfoArray;
        private int _currentRenderMethodIndex = 0;

        public Sketch002()
            : base(1024, 1024)
        {

            _renderMethodInfoArray = GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(o => o.Name.StartsWith("Render") && o.Name.Length > 6).OrderBy(o => o.Name).ToArray();

            var updateMethodInfoArray = GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(o => o.Name.StartsWith("Update") && o.Name.Length > 6).OrderBy(o => o.Name).ToArray();

            _updateMethodInfoArray = new MethodInfo[_renderMethodInfoArray.Length];

            for (var i = 0; i < _renderMethodInfoArray.Length; i++)
            {
                var ext = _renderMethodInfoArray[i].Name.Substring(6);

                _updateMethodInfoArray[i] = GetType().GetMethod("Update" + ext, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public void Load()
        {
        }

        public void UnLoad()
        {
            
        }

        public void Update()
        {
            _updateMethodInfoArray[_currentRenderMethodIndex]?.Invoke(this, null);
        }

        public void Render()
        {
            _renderMethodInfoArray[_currentRenderMethodIndex]?.Invoke(this, null);
        }

        public void Update002()
        {
            
        }

        public void Render000()
        {
            Random.ReSeed(Random.Seed);

            Graphics.ClearColor(255);

            //Graphics.Background(Color4.White);

            //Graphics.Background(r, g, b, a);

            //Graphics.Background(r, g, b, a, r, g, b, a, angle);

            var count = 100;

            var paddingFactor = 0.2f;
            var padding = FrustumWidth / count * paddingFactor;
            var size = (FrustumWidth - (padding * (count + 1))) / count;
            var offsetX = size + padding;
            var offsetY = size + padding;
            var startX = padding + size / 2;
            var y = padding + size / 2;

            Graphics.PushMatrix();
            Graphics.Translate(-FrustumWidth / 2f, -FrustumHeight / 2f);

            //DrawRectangle001(0, 0, size);

            var c = 0;

            for (var countY = 0; countY < count; countY++)
            {
                var x = startX;

                for (var countX = 0; countX < count; countX++)
                {
                    DrawRectangle000(x, y, size);

                    //if(FrameCount == 1)
                    //{
                    //    Console.Write($"{c}:{x}/{y} ");
                    //}

                    x += offsetX;

                    c++;
                }

                y += offsetY;
            }

            Graphics.PopMatrix();
        }

        private void DrawRectangle000(float x, float y, float size)
        {
            Graphics.PushMatrix();
            //Graphics.Rotate(Random.Value(0f, 90f));
            Graphics.Translate(x, y);
            Graphics.Rectangle().Size(Random.Value(size * 0.1f, size * 0.55f), size);
            Graphics.PopMatrix();
        }

        public void Render001()
        {
            Random.ReSeed(Random.Seed);

            Graphics.ClearColor(255);

            var count = 75;

            var paddingFactor = 0.2f;
            var padding = FrustumWidth / count * paddingFactor;
            var size = (FrustumWidth - (padding * (count + 1))) / count;
            var offsetX = size + padding;
            var offsetY = size + padding;
            var startX = padding + size / 2;
            var y = padding + size / 2;

            Graphics.PushMatrix();
            Graphics.Translate(-FrustumWidth / 2f, -FrustumHeight / 2f);

            //DrawRectangle001(0, 0, size);

            for (var countY = 0; countY < count; countY++)
            {
                var x = startX;

                for (var countX = 0; countX < count; countX++)
                {
                    DrawRectangle001(x, y, size);

                    x += offsetX;
                }

                y += offsetY;
            }

            Graphics.PopMatrix();
        }

        private void DrawRectangle001(float x, float y, float size)
        {
            Graphics.PushMatrix();
            Graphics.Rotate(Random.Value(0f, 90f));
            Graphics.Translate(x, y);
            Graphics.Rectangle().Size(Random.Value(size * 0.1f, size * 0.55f), size);
            Graphics.PopMatrix();
        }

        public void Render002()
        {
            Random.ReSeed(Random.Seed);

            Graphics.ClearColor(255);

            var count = 50;

            var paddingFactor = 0.2f;
            var padding = FrustumWidth / count * paddingFactor;
            var size = (FrustumWidth - (padding * (count + 1))) / count;
            var offsetX = size + padding;
            var offsetY = size + padding;
            var startX = padding + size / 2;
            var y = padding + size / 2;

            Graphics.PushMatrix();
            Graphics.Translate(-FrustumWidth / 2f, -FrustumHeight / 2f);

            //DrawRectangle001(0, 0, size);

            for (var countY = 0; countY < count; countY++)
            {
                var x = startX;

                for (var countX = 0; countX < count; countX++)
                {
                    DrawRectangle002(x, y, size);

                    x += offsetX;
                }

                y += offsetY;
            }

            Graphics.PopMatrix();
        }

        private void DrawRectangle002(float x, float y, float size)
        {
            Graphics.PushMatrix();
            Graphics.Translate(x, y);
            Graphics.Rotate(Random.Value(0f, 25f));
            Graphics.Rectangle().Size(Random.Value(size * 0.1f, size * 0.55f), size);
            Graphics.PopMatrix();
        }

        public void KeyUp(KeyboardKeyEventArgs a)
        {
            if (a.Key == Keys.Tab)
            {
                _currentRenderMethodIndex++;

                if (_currentRenderMethodIndex == _renderMethodInfoArray.Length)
                {
                    _currentRenderMethodIndex = 0;
                }
            }
            else if (a.Key == Keys.S)
            {
                Random.ReSeed();
            }
        }

        public void MouseMove(MouseMoveEventArgs a)
        {
            //var screen = Graphics.ModelToScreenSpace(new Vector3(0, 0, -2), Matrix4.Identity, Graphics.ActiveCamera().ViewMatrix(), Graphics.ActiveCamera().ProjectionMatrix(), Width, Height);

            //Diagnostics.Log((int)screen.X + " | " + (int)screen.Y);
        }

        public void CameraMouseDown(MouseButtonEventArgs a)
        {
        }

        public void CameraMouseUp(MouseButtonEventArgs a)
        {
        }
    }
}

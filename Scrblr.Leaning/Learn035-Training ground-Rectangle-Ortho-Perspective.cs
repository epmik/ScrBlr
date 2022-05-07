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
    [Sketch(Name = "Learn035-Training ground-Rectangle-Ortho-Perspective")]
    public class Learn035 : AbstractSketch
    {
        float _rotationDegreesPerSecond = 90, _degrees;
        Texture _gridNoTransparency, _gridWithTransparency, _smileyWithTransparency;
        FirstPersonCamera _firstPersonCamera;
        bool _lPressed = false;
        Vector3 _rectanglePosition = new Vector3(0.5f, 0, -2);
        float _rectangleWidth = 2f;
        float _rectangleHeight = 1f;
        Vector3 _rectangleTopLeft;
        Vector3 _rectangleTopRight;
        Vector3 _rectangleBottomRight;
        Vector3 _rectangleBottomLeft;

        public Learn035()
            : base(2, 2)
        {
        }

        public void Load()
        {
            _gridNoTransparency = new Texture("resources/textures/orange-white-1024x1024.jpg");

            _gridWithTransparency = new Texture("resources/textures/orange-transparent-1024x1024.png");

            _smileyWithTransparency = new Texture("resources/textures/smiley-transparent-1024x1024.png");

            _firstPersonCamera = new FirstPersonCamera
            {
                Fov = 90f,
                AspectRatio = FrustumWidth / FrustumHeight,
                Near = 1f,
                Far = 1000f,
            };

            AttachCamera(_firstPersonCamera, true, true);

            _rectangleTopLeft = new Vector3(_rectangleWidth * -0.5f, _rectangleHeight * 0.5f, 0f);
            _rectangleTopRight = new Vector3(_rectangleWidth * 0.5f, _rectangleHeight * 0.5f, 0f);
            _rectangleBottomRight = new Vector3(_rectangleWidth * 0.5f, _rectangleHeight * -0.5f, 0f);
            _rectangleBottomLeft = new Vector3(_rectangleWidth * -0.5f, _rectangleHeight * -0.5f, 0f);
        }

        public void UnLoad()
        {
            _gridNoTransparency.Dispose();
            _gridWithTransparency.Dispose();
            _smileyWithTransparency.Dispose();
        }

        public void Update()
        {
            
        }

        public void KeyUp(KeyboardKeyEventArgs a)
        {
            if(a.Key == Keys.P)
            {
                Graphics.ActiveCamera().ProjectionMode = Graphics.ActiveCamera().ProjectionMode.Next();
            }
            if (a.Key == Keys.R)
            {
                _degrees += 30f;

                if (_degrees >= 360f)
                {
                    _degrees -= 360f;
                }
            }
            if (a.Key == Keys.L)
            {
                _lPressed = true;
            }
        }

        public void Render()
        {
            Graphics.ClearColor(128);

            Graphics.State.Disable(EnableFlag.BackFaceCulling);
            Graphics.State.Disable(EnableFlag.FrontFaceCulling);

            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Translate(ref _rectanglePosition);

            Graphics.Rectangle().Width(_rectangleWidth).Height(_rectangleHeight);

            if (_lPressed)
            {
                _lPressed = false;

                var screen = ModelToScreenSpace(_rectanglePosition);
                var screenTopLeft = ModelToScreenSpace(_rectangleTopLeft);
                var screenTopRight = ModelToScreenSpace(_rectangleTopRight);
                var screenBottomRight = ModelToScreenSpace(_rectangleBottomRight);
                var screenBottomLeft = ModelToScreenSpace(_rectangleBottomLeft);

                Diagnostics.Log((int)screen.X + " | " + (int)screen.Y);
                Diagnostics.Log((int)screenTopLeft.X + " | " + (int)screenTopLeft.Y);
                Diagnostics.Log((int)Math.Max(screenTopRight.X - screenTopLeft.X, screenBottomRight.X - screenBottomLeft.X) + " | " + (int)Math.Max(screenBottomRight.Y - screenTopRight.Y, screenBottomLeft.Y - screenTopLeft.Y));
                //Diagnostics.Log((int)screenBottomRight.X + " | " + (int)screenBottomRight.Y);
                //Diagnostics.Log((int)screenBottomLeft.X + " | " + (int)screenBottomLeft.Y);
                Diagnostics.Log("");
            }

            Graphics.PopMatrix();
        }

    }
}

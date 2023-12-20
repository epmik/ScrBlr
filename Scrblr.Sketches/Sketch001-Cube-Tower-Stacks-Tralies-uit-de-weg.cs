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

namespace Scrblr.Sketches
{
    [Sketch(Name = "Sketch001-Cube-Tower-Stacks-Tralies-uit-de-weg")]
    public class Sketch001 : AbstractSketch
    {
        float _rotationDegreesPerSecond = 90, _degrees;
        FirstPersonCamera _firstPersonCamera;
        int? _seed = 0;
        int _stackCount = 160;
        float _minHeight = 0.01f;
        float _maxHeight = 0.05f;
        float _minWidth = 0.5f;
        float _maxWidth = 2.25f;
        float _minDepth = 0.5f;
        float _maxDepth = 2.25f;
        float _maxXrotation = 10f;
        float _maxYrotation = 90f;
        float _maxZrotation = 0f;

        Color4[] _colors = new[] 
        {
            new Color4(255, 0, 0, 255),
            new Color4(255, 255, 255, 255),
            new Color4(0, 0, 0, 255),
        };


        public Sketch001()
            : base(2, 2)
        {
            if(_seed != null)
            {
                RandomSeed(_seed.Value);
            }
        }

        public void Load()
        {
            _firstPersonCamera = new FirstPersonCamera
            {
                Fov = 45f,
                AspectRatio = FrustumWidth / FrustumHeight,
                Near = 1.5f,
                Far = 1000f,
                AllowRotation = false,
            };

            AttachCamera(_firstPersonCamera, true, true);

            _firstPersonCamera.MouseDownAction += CameraMouseDown;
            _firstPersonCamera.MouseUpAction += CameraMouseUp;
        }

        public void UnLoad()
        {
            
        }

        public void Update()
        {
            RandomSeed(RandomSeed());

            _degrees += (float)(_rotationDegreesPerSecond * ElapsedTime);

            if(_degrees >= 360f)
            {
                _degrees -= 360f;
            }
        }

        public void Render()
        {
            Graphics.ClearColor(255);

            Graphics.State.Enable(EnableFlag.DepthTest);
            Graphics.State.Enable(EnableFlag.BackFaceCulling);

            var y = 0f;

            for(var i = 0; i < _stackCount; i++)
            {
                y += RenderCube(y);
            }
        }

        private float RenderCube(float y)
        {
            var h = Random(_minHeight, _maxHeight);
            var w = Random(_minWidth, _maxWidth);
            var d = Random(_minDepth, _maxDepth);
            var margin = 0f;// Random(0.01f, 0.1f);
            var color = _colors[Random(_colors.Length)];

            Graphics.PushMatrix();
            Graphics.Rotate(Random(0f, _maxXrotation), Axis.X);
            Graphics.Rotate(Random(0f, _maxYrotation), Axis.Y);
            Graphics.Rotate(Random(0f, _maxZrotation), Axis.Z);
            Graphics.Translate(0, y, -2);
            Graphics.Cube().Color(color).Height(h).Width(w).Depth(d);
            Graphics.PopMatrix();

            return h + margin;
        }


        public void KeyUp(KeyboardKeyEventArgs a)
        {
            if (a.Key == Keys.P)
            {
                Graphics.ActiveCamera().ProjectionMode = Graphics.ActiveCamera().ProjectionMode.Next();
            }
        }

        public void MouseMove(MouseMoveEventArgs a)
        {
            var screen = Graphics.ModelToScreenSpace(new Vector3(0, 0, -2), Matrix4.Identity, Graphics.ActiveCamera().ViewMatrix(), Graphics.ActiveCamera().ProjectionMatrix(), Width, Height);

            Diagnostics.Log((int)screen.X + " | " + (int)screen.Y);
        }

        public void CameraMouseDown(MouseButtonEventArgs a)
        {
            if ((int)a.Button == (int)MouseButton.Left)
            {
                _firstPersonCamera.AllowRotation = true;
            }
        }

        public void CameraMouseUp(MouseButtonEventArgs a)
        {
            if ((int)a.Button == (int)MouseButton.Left)
            {
                _firstPersonCamera.AllowRotation = false;
            }
        }
    }
}

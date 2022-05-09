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
    [Sketch(Name = "Learn036-Training ground-Cube-Always_Facing_Camera")]
    public class Learn036 : AbstractSketch
    {
        float _rotationDegreesPerSecond = 90f, _degrees = 45f;
        Texture _gridNoTransparency, _gridWithTransparency, _smileyWithTransparency;
        FirstPersonCamera _firstPersonCamera;
        bool _rotateFirst = true;
        bool _renderCube = true;
        Vector3 _cubePosition = new Vector3(0, 0, -2f);

        public Learn036()
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
                Fov = 45f,
                AspectRatio = FrustumWidth / FrustumHeight,
                Near = 1f,
                Far = 1000f,
                Position = new Vector3(0, 0, 2f),
            };

            AttachCamera(_firstPersonCamera, true, true);

            HideAndLockCursor();
        }

        public void UnLoad()
        {
            _gridNoTransparency.Dispose();
            _gridWithTransparency.Dispose();
            _smileyWithTransparency.Dispose();
        }

        public void Update()
        {
            //_degrees += (float)(_rotationDegreesPerSecond * ElapsedTime);

            //if(_degrees >= 360f)
            //{
            //    _degrees -= 360f;
            //}
        }

        public void KeyUp(KeyboardKeyEventArgs a)
        {
            if(a.Key == Keys.P)
            {
                Graphics.ActiveCamera().ProjectionMode = Graphics.ActiveCamera().ProjectionMode.Next();
            }
            if (a.Key == Keys.R)
            {
                _rotateFirst = !_rotateFirst;
            }
            if (a.Key == Keys.T)
            {
                _renderCube = !_renderCube;
            }
        }

        public void MouseMove(MouseMoveEventArgs a)
        {
        }

        public void Render()
        {
            var camera = Graphics.ActiveCamera();

            Graphics.ClearColor(128);

            Graphics.State.Enable(EnableFlag.BackFaceCulling);

            Graphics.PushMatrix();
            if(_rotateFirst)
            {
                var distance = (_cubePosition - camera.Position).Length;

                var ratio = camera.AspectRatio;

                camera.ProjectionMode = ProjectionMode.Orthographic;
                camera.Width = camera.DepthRatio * distance * 2f;
                camera.Height = camera.Width * ratio;

                //Matrix4.CreateOrthographicOffCenter(-size_x, size_x, -size_y, size_y, camera.Near, camera.Far);

                Graphics.Rotate(_degrees, Axis.Y);
                Graphics.Translate(ref _cubePosition);
                Graphics.RotateTo(camera.Position);
            }
            else
            {
                Graphics.Translate(0, 0, -2);
                Graphics.Rotate(_degrees, Axis.Y);
                Graphics.RotateTo(camera.Position);
            }
            if(_renderCube)
            {
                Graphics.Cube();
                    //.Texture(_gridWithTransparency);
            }
            else
            {
                Graphics.Circle().Color(255, 0, 0, 128);
                
                Graphics.Circle().Color(255, 0, 255, 128);
            }
            Graphics.PopMatrix();

            Graphics.Cube().Size(0.1f);
        }
    }
}

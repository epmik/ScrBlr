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
    [Sketch(Name = "Learn033-Training ground-Sphere-Ortho-Perspective")]
    public class Learn033 : AbstractSketch
    {
        float _rotationDegreesPerSecond = 90, _degrees;
        Texture _gridNoTransparency, _gridWithTransparency, _smileyWithTransparency;
        FirstPersonCamera _firstPersonCamera;

        public Learn033()
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

        public void KeyUp(KeyboardKeyEventArgs a)
        {
            if(a.Key == Keys.P)
            {
                Graphics.ActiveCamera().ProjectionMode = Graphics.ActiveCamera().ProjectionMode.Next();
            }
        }

        public void Render()
        {
            Graphics.ClearColor(128);

            Graphics.State.Disable(EnableFlag.BackFaceCulling);
            Graphics.State.Disable(EnableFlag.FrontFaceCulling);

            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(0, 0, -2);
            Graphics.Sphere()
                .Color(255, 255, 255);
                //.Texture(_gridWithTransparency);
            Graphics.PopMatrix();
        }
    }
}

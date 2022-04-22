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
    [Sketch(Name = "Learn032-Training ground-Cube-Visualizing-DepthBuffer")]
    public class Learn032 : AbstractSketch
    {
        float _rotationDegreesPerSecond = 90, _degrees;
        Texture _gridNoTransparency, _gridWithTransparency, _smileyWithTransparency;
        FirstPersonCamera _firstPersonCamera;
        Shader _vizualizeDepthBufferShader;

        internal const string _fragmentShaderSource = @"
#version 330 core

out vec4 oColor0;

void main()
{
    oColor0 = vec4(vec3(gl_FragCoord.z), 1.0);
}";

        public Learn032()
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

            HideAndLockCursor();

            AttachCamera(_firstPersonCamera, true, true);

            _vizualizeDepthBufferShader = new Shader(Graphics.QueryStandardVertexShaderSource(VertexFlag.Position0 | VertexFlag.Color0), _fragmentShaderSource);

            Graphics.AttachShader(_vizualizeDepthBufferShader);
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
            if (a.Key == Keys.P)
            {
                Graphics.ActiveCamera().ProjectionMode = Graphics.ActiveCamera().ProjectionMode.Next();
            }
            if (a.Key == Keys.O)
            {
                Graphics.State.Toggle(EnableFlag.DepthTest);
            }
        }

        public void Render()
        {
            Graphics.ClearColor(128);



            Graphics.State.Enable(EnableFlag.BackFaceCulling);
            Graphics.State.Disable(EnableFlag.FrontFaceCulling);

            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(-0.75f, 0, -2);
            Graphics.Cube().Color(0, 0, 128);
            Graphics.PopMatrix();




            Graphics.State.Enable(EnableFlag.FrontFaceCulling);
            Graphics.State.Disable(EnableFlag.BackFaceCulling);

            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(0, 0, -4);
            Graphics.Cube().Color(0, 128, 0);
            Graphics.PopMatrix();




            Graphics.State.Enable(EnableFlag.BackFaceCulling);
            Graphics.State.Disable(EnableFlag.FrontFaceCulling);

            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(0.75f, 0, -6);
            Graphics.Cube().Color(128, 0, 0);
            Graphics.PopMatrix();
        }
    }
}

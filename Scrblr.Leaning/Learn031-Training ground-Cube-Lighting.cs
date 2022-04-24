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
    [Sketch(Name = "Learn031-Training ground-Cube-Lighting")]
    public class Learn031 : AbstractSketch
    {
        float _rotationDegreesPerSecond = 45, _degrees;
        Texture _gridNoTransparency, _gridWithTransparency, _smileyWithTransparency;
        FirstPersonCamera _firstPersonCamera;

        public Learn031()
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
            if (a.Key == Keys.O)
            {
                Graphics.State.Toggle(EnableFlag.DepthTest);
            }
        }

        public void Render()
        {
            Graphics.ClearColor(0);

            var shader = Graphics.StandardShader(VertexFlag.Position0 | VertexFlag.Normal0 | VertexFlag.Color0);

            shader.Uniform("uLightCount", 1);
            shader.Uniform("uLightPosition", new Vector4(0, 0, -1, 0));  // w == 0 == directional light, w == 1 ==  positional light
            shader.Uniform("uLightDiffuseColor", new Vector3(1f, 1f, 1f));
            shader.Uniform("uLightAmbientStrength", 0.05f);
            shader.Uniform("uLightAmbientColor", new Vector3(1f, 1f, 1f));

            Graphics.State.Enable(EnableFlag.BackFaceCulling);
            //Graphics.State.Disable(EnableFlag.FrontFaceCulling);

            Graphics.PushMatrix();
            //Graphics.Rotate(_degrees, Axis.X);
            Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(-0.75f, 0, -2);
            Graphics.Cube().Color(222, 71, 0);
            Graphics.PopMatrix();




            //shader.Uniform("uLightPosition", new Vector4(1, -1, 0, 0));  // 0 == directional light, 1 ==  positional light
            //shader.Uniform("uLightDiffuseColor", new Vector3(1f, 0.25f, 0.5f));
            //shader.Uniform("uLightAmbientStrength", 0.25f);
            //shader.Uniform("uLightAmbientColor", new Vector3(0f, 0f, 1f));

            Graphics.PushMatrix();
            Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            //Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(0, 0, -4);
            Graphics.Cube().Color(0, 0, 224);
            Graphics.PopMatrix();




            //shader.Uniform("uLightPosition", new Vector4(1, -1, 0, 0));  // 0 == directional light, 1 ==  positional light
            //shader.Uniform("uLightDiffuseColor", new Vector3(1f, 0.25f, 0.5f));
            //shader.Uniform("uLightAmbientStrength", 0.25f);
            //shader.Uniform("uLightAmbientColor", new Vector3(0f, 0f, 1f));

            Graphics.PushMatrix();
            //Graphics.Rotate(_degrees, Axis.X);
            //Graphics.Rotate(_degrees, Axis.Y);
            Graphics.Rotate(_degrees, Axis.Z);
            Graphics.Translate(0.75f, 0, -6);
            Graphics.Cube().Color(42, 148, 1);
            Graphics.PopMatrix();
        }
    }
}

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn028-Training ground-Custom Shader")]
    public class Learn028 : AbstractSketch
    {

        const string vertexShaderSource = @"
#version 330 core

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

layout(location = 0) in vec3 iPosition0;
layout(location = 1) in vec4 iColor0;
layout(location = 2) in vec2 iUv0;

out vec2 ioUv0;
out vec4 ioColor0;

void main(void)
{
    ioUv0 = iUv0;
    ioColor0 = iColor0;

    gl_Position = vec4(iPosition0, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;
}
";

        const string fragmentShaderSource = @"
#version 330

uniform sampler2D uTexture0;
uniform sampler2D uTexture1;

in vec4 ioColor0;
in vec2 ioUv0;

out vec4 oColor0;

void main()
{
    oColor0 = mix(texture(uTexture0, ioUv0), texture(uTexture1, ioUv0), 0.5) * ioColor0; 
}
";
        private Shader _shader;

        public Learn028()
            : base(600, 600)
        {
        }

        public void Load()
        {
            _shader = Graphics.CreateShader(vertexShaderSource, fragmentShaderSource);
        }

        public void Render()
        {
            Graphics.ActiveShader(_shader);
            Graphics.Quad();
        }
    }
}

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Scrblr.Leaning
{
    [Sketch(Name = "Learn027-Training ground")]
    public class Learn027 : AbstractSketch
    {
        public Learn027()
            : base(600, 600)
        {
        }

        private Texture _texture;

        public void Load()
        {
            _texture = new Texture("resources/textures/orange-transparent-1024x1024.png");

            Graphics.ClearColor(1f, 1f, 1f, 1.0f);

            Graphics.Enable(EnableFlag.DepthTest);
        }

        public void UnLoad()
        {
            _texture.Dispose();
        }

        public void Render()
        {
            Graphics.Quad();
        }
    }
}

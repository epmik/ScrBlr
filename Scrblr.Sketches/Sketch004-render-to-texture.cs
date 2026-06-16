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
    [Sketch(Name = "Sketch004-render-to-texture")]
    public class Sketch004 : AbstractSketch
    {
        private GraphicsContext _renderContext;

        public Sketch004()
            : base(1024, 1024)
        {

        }

        public void Load()
        {

            _renderContext = new GraphicsContext(
                WindowWidth,
                WindowHeight,
                Graphics.ColorBits,
                DepthBits,
                StencilBits,
                1);
        }

        public void UnLoad()
        {
            _renderContext.Dispose();

            _renderContext = null;
        }

        public void Update()
        {
        }

        public void Render()
        {
            Random.ReSeed(Random.Seed);

            Graphics.ClearColor(255);

            //////////////

            Graphics.PushMatrix();

            Graphics.Translate(-200, 0, 0);

            Graphics.Hairline()
                .From(0, 0, 0)
                .To(50, 0, 0)
                .From(50, 50, 0)
                .To(0, 0, 0);

            Graphics.PopMatrix();

            //////////////

            Graphics.PushMatrix();

            Graphics.Translate(-100, 0, 0);

            Graphics.Hairline()
                .From(0, 0, 0)
                .To(50, 0, 0)
                .To(50, 50, 0)
                .To(0, 0, 0);

            Graphics.PopMatrix();

            //////////////

            Graphics.PushMatrix();

            Graphics.Translate(0, 0, 0);

            Graphics.Hairline()
                .From(0, 0, 0)
                .To(50, 0, 0)
                .To(50, 50, 0)
                .Close();

            Graphics.PopMatrix();

            //////////////

            Graphics.PushMatrix();

            Graphics.Translate(100, 0, 0);

            Graphics.Hairline()
                .From(0, 0, 0)
                .To(50, 0, 0)
                .From(50, 50, 0)
                .To(0, 0, 0)
                .To(0, 50, 0)
                .To(50, 100, 0);

            Graphics.PopMatrix();

            //////////////

            Graphics.PushMatrix();

            Graphics.Translate(200, 0, 0);

            Graphics.Hairline()
                .From(0, 0, 0)
                .To(50, 0, 0)
                .From(50, 50, 0)
                .To(0, 0, 0)
                .To(0, 50, 0)
                .To(50, 100, 0)
                .Close();

            Graphics.PopMatrix();

            //////////////
        }

        public void KeyUp(KeyboardKeyEventArgs a)
        {
            if (a.Key == Keys.Tab)
            {
                
            }
            else if (a.Key == Keys.S)
            {
                Random.ReSeed();
            }
        }

        public void MouseMove(MouseMoveEventArgs a)
        {
        }

        public void CameraMouseDown(MouseButtonEventArgs a)
        {
        }

        public void CameraMouseUp(MouseButtonEventArgs a)
        {
        }
    }
}

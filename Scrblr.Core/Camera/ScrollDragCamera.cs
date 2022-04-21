using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public class ScrollDragCamera : AbstractCamera
    {
        private float _scrollFactor = 1f;

        public ScrollDragCamera()
        {

        }

        public override Matrix4 ProjectionMatrix()
        {
            switch (ProjectionMode)
            {
                case ProjectionMode.Perspective:
                    return Matrix4.CreatePerspectiveOffCenter(Left, Right, Bottom, Top, Near, Far);
                default:
                    return Matrix4.CreateOrthographicOffCenter(Left * _scrollFactor, Right * _scrollFactor, Bottom * _scrollFactor, Top * _scrollFactor, Near, Far);
            }
        }

        public override void KeyUp(KeyboardKeyEventArgs a)
        {
            if(a.Key == Keys.P)
            {
                ProjectionMode = ProjectionMode.Next();
            }
        }

        public override void MouseWheel(MouseWheelEventArgs a)
        {
            Scroll(a.OffsetY, ElapsedTime);
        }

        private void Scroll(float direction, double time)
        {
            var multiplier = direction < 0 ? 1.06f : 0.92f;

            _scrollFactor *= multiplier;

            if (_scrollFactor < 0.1f)
            {
                _scrollFactor = 0.1f;
            }
        }
    }
}
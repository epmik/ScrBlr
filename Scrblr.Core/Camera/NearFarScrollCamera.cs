using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public class NearFarScrollCamera : AbstractCamera
    {
        private float _scrollFactor = 1f;

        public NearFarScrollCamera()
        {

        }

        public override Matrix4 ProjectionMatrix()
        {
            return Matrix4.CreateOrthographicOffCenter(Left * _scrollFactor, Right * _scrollFactor, Bottom * _scrollFactor, Top * _scrollFactor, Near, Far);
            //return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), AspectRatio, Near, Far);
        }

        public void Scroll(float direction, double time)
        {
            var multiplier = direction < 0 ? 1.06f : 0.92f;

            _scrollFactor *= multiplier;

            if (_scrollFactor < 0.1f)
            {
                _scrollFactor = 0.1f;
            }

            //Console.WriteLine($"_scrollFactor: {_scrollFactor}");
        }
    }
}
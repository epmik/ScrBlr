using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public class NearFarScrollCamera20220410 : AbstractCamera20220410
    {
        public NearFarScrollCamera20220410()
        {

        }

        public void Scroll(float direction, double time)
        {
            var length = Far - Near;

            var speed = (float)(length * 5 * time);

            var position = (Position.Z - Near) / length;

            //var factor = (float)Math.Pow(position, 4);
            var factor = position != 0 ? -(float)Math.Log(position) : 0;

            var movement = speed * factor * -direction;

            Position.Z += movement;

            if (Position.Z < Near)
            {
                Position.Z = Near;
            }
            else if (Position.Z > Far)
            {
                Position.Z = Far;
            }

            Console.WriteLine($"Position.Z: {Position.Z}");
        }
    }
}
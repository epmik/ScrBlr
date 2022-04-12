using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public abstract class AbstractCamera20220410 : ICamera20220410
    {
        public Vector3 Position = new Vector3(0, 0, 0);
        public Vector3 Forward = new Vector3(0, 0, -1);
        public Vector3 Up = new Vector3(0, 1, 0);
        public Vector3 Right = new Vector3(1, 0, 0);
        public float Fov { get; set; }
        public float Near { get; set; }
        public float Far { get; set; }

        public Matrix4 ViewMatrix()
        {
            return Matrix4.LookAt(
                Position,
                Position + Forward,
                Up);
        }
    }
}
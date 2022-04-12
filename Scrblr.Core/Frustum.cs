using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public class Frustum
    {
        public float Fov { get; set; }
        public float Near { get; set; }
        public float Far { get; set; }

        public float FrustumLeft;
        public float FrustumRight;
        public float FrustumTop;
        public float FrustumBottom;
    }
}
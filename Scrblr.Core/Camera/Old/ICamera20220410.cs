using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public interface ICamera20220410
    {
        float Fov { get; set; }
        float Near { get; set; }
        float Far { get; set; }
    }
}
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public enum CameraMode
    {
        Fps = 0,
        NearFarScroll,
        Free,
        Orbit
    }
}
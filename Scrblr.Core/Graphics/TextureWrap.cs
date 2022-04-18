using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public enum TextureWrap
    {
        Repeat = TextureWrapMode.Repeat,
        Clamp = TextureWrapMode.ClampToEdge,
    }
}
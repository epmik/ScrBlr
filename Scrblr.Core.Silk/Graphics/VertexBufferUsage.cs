using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public enum VertexBufferUsage
    {
        Static = GLEnum.StaticDraw,
        Dynamic = GLEnum.DynamicDraw,
    }
}
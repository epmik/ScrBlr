using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public enum VertexBufferUsage
    {
        StaticDraw = BufferUsageHint.StaticDraw,
        DynamicDraw = BufferUsageHint.DynamicDraw,
    }
}
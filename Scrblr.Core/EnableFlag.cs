using OpenTK.Graphics.OpenGL4;
using System;

namespace Scrblr.Core
{
    public enum EnableFlag
    {
        DepthTest = EnableCap.DepthTest,
        StencilTest = EnableCap.StencilTest,
        Texture2d = EnableCap.Texture2D,

        Rendering = 1,
        ClearBuffer,
    }
}
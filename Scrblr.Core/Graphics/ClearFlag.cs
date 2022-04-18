using OpenTK.Graphics.OpenGL4;
using System;

namespace Scrblr.Core
{
    [Flags]
    public enum ClearFlag
    {
        None = ClearBufferMask.None,
        ColorBuffer = ClearBufferMask.ColorBufferBit,
        DepthBuffer = ClearBufferMask.DepthBufferBit,
        StencilBuffer = ClearBufferMask.StencilBufferBit,
    }
}
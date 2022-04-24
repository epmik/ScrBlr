using OpenTK.Graphics.OpenGL4;
using System;

namespace Scrblr.Core
{
    [Flags]
    public enum BindFlag
    {
        Default = 0,
        ReadWrite = 0,
        Write,
        Read,
    }
}
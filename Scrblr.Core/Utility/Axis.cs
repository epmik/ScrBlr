using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public enum Axis
    {
        None,
        X,
        Y,
        Z,
    }

    public static class AxisExentions
    {
        public static Vector3 ToVector(this Axis axis)
        {
            return axis == Axis.X
                    ? Vector3.UnitX
                    : axis == Axis.Y
                        ? Vector3.UnitY
                        : Vector3.UnitZ;
        }
    }
}
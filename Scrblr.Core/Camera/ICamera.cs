using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scrblr.Core
{
    public interface ICamera : IEventComponent
    {
        ProjectionMode ProjectionMode { get; set; }
        float Fov { get; }
        float Near { get; }
        float Far { get; }
        float Left { get; }
        float Right { get; }
        float Top { get; }
        float Bottom { get; }
        float AspectRatio { get; }

        Matrix4 ProjectionMatrix();

        Matrix4 ViewMatrix();
    }
}
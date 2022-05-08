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
        float Width { get; set; }
        float Height { get; set; }
        float AspectRatio { get; }
        float DepthRatio { get; }
        Vector3 Position { get; }
        Vector3 DirectionVector { get; }
        Vector3 ForwardVector { get; }
        Vector3 UpVector { get; }
        Vector3 RightVector { get; }

        Matrix4 ProjectionMatrix();

        Matrix4 ViewMatrix();
    }
}
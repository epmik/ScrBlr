using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Numerics;
using Color = System.Drawing.Color;

namespace Scrbl.Tutorials;

public unsafe class BufferAttribute
{
    public uint ElementCount { get; private set; }
    public uint ElementSize { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public BufferAttribute(string name, void* data, uint elementCount, uint elementSize)
    {
        Name = name;
        ElementCount = elementCount;
        ElementSize = elementSize;
    }
}
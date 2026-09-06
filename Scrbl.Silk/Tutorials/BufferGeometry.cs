using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Numerics;
using Color = System.Drawing.Color;

namespace Scrbl.Tutorials;

public unsafe class BufferGeometry
{
    public List<BufferAttribute> Attributes { get; private set; } = new List<BufferAttribute>();

    public void AddAttribute(void*data, BufferAttribute attribute)
    {
        Attributes.Add(attribute);
    }

    public void Enable()
    {
        //// Set up our vertex attributes! These tell the vertex array (VAO) how to process the vertex data we defined
        //// earlier. Each vertex array contains attributes. 

        //// Our stride constant. The stride must be in bytes, so we take the first attribute (a vec3), multiply it
        //// by the size in bytes of a float, and then take our second attribute (a vec2), and do the same.
        //const uint stride = ((3 * sizeof(float))) + (2 * sizeof(float) + (3 * sizeof(float)));

        //// Enable the "aPosition" attribute in our vertex array, providing its size and stride too.
        //const uint positionLoc = 0;
        //_gl.EnableVertexAttribArray(positionLoc);
        //_gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride, (void*)0);

        //// Now we need to enable our texture coordinates! We've defined that as location 1 so that's what we'll use
        //// here. The code is very similar to above, but you must make sure you set its offset to the **size in bytes**
        //// of the attribute before.
        //const uint textureLoc = 1;
        //_gl.EnableVertexAttribArray(textureLoc);
        //_gl.VertexAttribPointer(textureLoc, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

        //// Now we need to enable our texture coordinates! We've defined that as location 1 so that's what we'll use
        //// here. The code is very similar to above, but you must make sure you set its offset to the **size in bytes**
        //// of the attribute before.
        //const uint colorLoc = 2;
        //_gl.EnableVertexAttribArray(colorLoc);
        //_gl.VertexAttribPointer(colorLoc, 3, VertexAttribPointerType.Float, false, stride, (void*)(5 * sizeof(float)));
    }

    public void Disable()
    {
        //_gl.BindVertexArray(0);
        //_gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        //_gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }
}
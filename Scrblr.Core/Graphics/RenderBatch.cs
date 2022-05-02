using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public class RenderBatch
    {
        public Shader Shader;

        public VertexBuffer VertexBuffer;

        public GraphicsState State;

        public Matrix4 ModelMatrix;

        public Matrix4 ProjectionMatrix;

        public Matrix4 ViewMatrix;

        public Vector3 ViewPosition;

        public GeometryType GeometryType;

        public int ElementIndex;

        public int ElementCount;

        public Texture Texture0;

        public Texture Texture1;

        public Texture Texture2;

        public Texture Texture3;

        public VertexFlag VertexFlag;
    }
}
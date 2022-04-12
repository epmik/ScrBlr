using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public class RenderChunk
    {
        /// <summary>
        /// default == false
        /// </summary>
        public bool Enabled;

        public Shader Shader;

        public VertexBuffer VertexBuffer;

        public Matrix4 ModelMatrix;

        public Matrix4 ProjectionMatrix;

        public Matrix4 ViewMatrix;

        public GeometryType GeometryType;

        public int ElementIndex;

        public int ElementCount;
    }
    public class RenderChunk20220410
    {
        /// <summary>
        /// default == false
        /// </summary>
        public bool Enabled;

        public Shader Shader;

        public VertexBuffer VertexBuffer;

        public Matrix4 ModelMatrix;

        public GeometryType GeometryType;

        public int ElementIndex;

        public int ElementCount;
    }
}
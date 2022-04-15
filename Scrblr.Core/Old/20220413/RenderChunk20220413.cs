using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public class RenderChunk20220413
    {
        /// <summary>
        /// default == false
        /// </summary>
        public bool Enabled;

        public Shader20220413 Shader;

        public VertexBuffer20220413 VertexBuffer;

        public Matrix4 ModelMatrix;

        public Matrix4 ProjectionMatrix;

        public Matrix4 ViewMatrix;

        public GeometryType20220413 GeometryType;

        public int ElementIndex;

        public int ElementCount;
    }
}
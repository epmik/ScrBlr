using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public class VertexBufferLayout
    {
        public enum ElementType
        {
            None,
            Int32 = VertexAttribPointerType.Int,
            Single = VertexAttribPointerType.Float,
        }

        public enum PartIdentifier
        {
            None,
            Position1,
            Position2,
            Position3,
            Position4,
            Position5,
            Position6,
            Position7,
            Position8,
            Color1,
            Color2,
            Color3,
            Color4,
            Color5,
            Color6,
            Color7,
            Color8,
            Uv1,
            Uv2,
            Uv3,
            Uv4,
            Uv5,
            Uv6,
            Uv7,
            Uv8,
        }

        public class Part
        {
            /// <summary>
            /// default == ElementIdentifier.None
            /// </summary>
            public PartIdentifier Identifier = PartIdentifier.None;

            /// <summary>
            /// default == ElementType.None
            /// </summary>
            public ElementType Type = ElementType.None;

            /// <summary>
            /// default == 0
            /// </summary>
            public int Count;

            /// <summary>
            /// default == true
            /// </summary>
            public bool Enabled = true;

            public int ShaderLocation;

            /// <summary>
            /// stride of this element in bytes
            /// </summary>
            public int Stride
            {
                get { return Count * Size(Type); }
            }

            public static int Size(ElementType elementType)
            {
                switch(elementType)
                {
                    case ElementType.Int32:
                        return sizeof(Int32);
                    case ElementType.Single:
                        return sizeof(Single);
                    default:
                        throw new NotImplementedException("VertexBufferLayout.Element.Size(Element) failed: found unknown ElementType");
                }
            }
        }


        public Part[] Parts;

        public int Handle;

        public VertexBufferLayout(IEnumerable<Part> parts)
        {
            Parts = parts.ToArray();

            ValidateAndSetShaderLocations();
        }

        private void ValidateAndSetShaderLocations()
        {
            if (Parts.Any(o => o.Identifier == PartIdentifier.None))
            {
                throw new InvalidOperationException("VertexBufferLayout() constructor failed. Element.Identifier cannot be ElementIdentifier.None.");
            }

            if (Parts.Any(o => o.Type == ElementType.None))
            {
                throw new InvalidOperationException("VertexBufferLayout() constructor failed. Element.Type cannot be ElementType.None.");
            }

            if (Parts.Any(o => o.Count == 0))
            {
                throw new InvalidOperationException("VertexBufferLayout() constructor failed. Element.Count cannot be 0.");
            }

            if (Parts.All(o => o.ShaderLocation == 0))
            {
                for(var i = 0; i < Parts.Length; i++)
                {
                    Parts[i].ShaderLocation = i;
                }
            }
            else if (Parts.Any(o => o.ShaderLocation == 0))
            {
                throw new InvalidOperationException("VertexBufferLayout() constructor failed. Element.ShaderLocation cannot be 0 unless they're all 0.");
            }
        }

        /// <summary>
        /// stride of all the elements that make up this vertex buffer lay-out
        /// </summary>
        public int Stride
        {
            get { return Parts.Sum(o => o.Stride); }
        }

        /// <summary>
        /// stride of all the elements that make up this vertex buffer lay-out
        /// </summary>
        public int ElementCount
        {
            get { return Parts.Sum(o => o.Count); }
        }

    }
}
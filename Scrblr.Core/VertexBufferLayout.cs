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
        #region Fields and Properties

        public enum ElementType
        {
            None,
            Int32 = VertexAttribPointerType.Int,
            Single = VertexAttribPointerType.Float,
        }

        public class Part
        {
            /// <summary>
            /// default == ElementIdentifier.None
            /// </summary>
            public VertexFlag VertexFlag = VertexFlag.None;

            /// <summary>
            /// default == ElementType.None
            /// </summary>
            public ElementType ElementType = ElementType.None;

            /// <summary>
            /// default == 0
            /// </summary>
            public int Count;

            /// <summary>
            /// default == true
            /// </summary>
            // todo is this needed, not set anywhere, use VertexFlag and VertexFlags properties?
            public bool Enabled = true;

            //public int ShaderLocation;

            public string ShaderInputName;

            /// <summary>
            /// stride of this element in bytes
            /// </summary>
            public int Stride
            {
                get { return Count * Size(ElementType); }
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

        #endregion Fields and Properties

        #region Constructors

        public VertexBufferLayout(IEnumerable<Part> parts)
        {
            Parts = parts.ToArray();

            ValidateAndSetShaderInputNames();
        }

        #endregion Constructors

        private void ValidateAndSetShaderInputNames()
        {
            if (Parts.Any(o => o.VertexFlag == VertexFlag.None))
            {
                throw new InvalidOperationException("VertexBufferLayout() constructor failed. Element.Identifier cannot be ElementIdentifier.None.");
            }

            if (Parts.Any(o => o.ElementType == ElementType.None))
            {
                throw new InvalidOperationException("VertexBufferLayout() constructor failed. Element.Type cannot be ElementType.None.");
            }

            if (Parts.Any(o => o.Count == 0))
            {
                throw new InvalidOperationException("VertexBufferLayout() constructor failed. Element.Count cannot be 0.");
            }

            foreach(var part in Parts)
            {
                if(!string.IsNullOrEmpty(part.ShaderInputName))
                {
                    continue;
                }

                part.ShaderInputName = part.VertexFlag.ShaderInputName();
            }

            if (Parts.Any(o => string.IsNullOrEmpty(o.ShaderInputName)))
            {
                throw new InvalidOperationException("VertexBufferLayout() constructor failed. Element.ShaderVariableName cannot be null or empty.");
            }

            //if (Parts.All(o => o.ShaderLocation == 0))
            //{
            //    for(var i = 0; i < Parts.Length; i++)
            //    {
            //        Parts[i].ShaderLocation = i;
            //    }
            //}
            //else if (Parts.Any(o => o.ShaderLocation == 0))
            //{
            //    throw new InvalidOperationException("VertexBufferLayout() constructor failed. Element.ShaderLocation cannot be 0 unless they're all 0.");
            //}
        }

        /// <summary>
        /// stride of all the elements that make up this vertex buffer lay-out
        /// </summary>
        public int Stride
        {
            get { return Parts.Sum(o => o.Stride); }
        }

        ///// <summary>
        ///// stride of all the elements that make up this vertex buffer lay-out
        ///// </summary>
        //public int ElementCount
        //{
        //    get { return Parts.Sum(o => o.Count); }
        //}

    }
}
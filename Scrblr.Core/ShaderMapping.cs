using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public class ShaderMapping
    {
        // TODO implement/use this
        // vertex buffer/shader binding can be complicated: use int location id's or named variables and/or both?
        // for the standard (internal shaders) it could be feasible to use fixed locations id's.
        #region Fields and Properties

        public class Map
        {
            /// <summary>
            /// default == ElementIdentifier.None
            /// </summary>
            public VertexFlag VertexFlag = VertexFlag.None;

            public int ShaderLocation;

            public string ShaderInputName;
        }

        public VertexFlag VertexFlags { get; private set; }

        public Map[] Maps;

        #endregion Fields and Properties

        #region Constructors

        public ShaderMapping(IEnumerable<Map> parts)
        {
            Maps = parts.ToArray();

            VertexFlags = Maps.Select(o => o.VertexFlag).Combine();

            ValidateAndSetShaderInputNames();
        }

        #endregion Constructors

        private void ValidateAndSetShaderInputNames()
        {
            if (Maps.Any(o => o.VertexFlag == VertexFlag.None))
            {
                throw new InvalidOperationException("VertexMapping() constructor failed. Map.VertexFlag cannot be VertexFlag.None.");
            }

            foreach(var map in Maps)
            {
                if(!string.IsNullOrEmpty(map.ShaderInputName))
                {
                    continue;
                }

                map.ShaderInputName = map.VertexFlag.ShaderInputName();
            }

            if (Maps.Any(o => string.IsNullOrEmpty(o.ShaderInputName)))
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
    }
}
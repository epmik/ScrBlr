using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public enum VertexFlag
    {
        // for 64 bit enums
        // see https://stackoverflow.com/questions/1060760/what-to-do-when-bit-mask-flags-enum-gets-too-large
        None = 0,


        [ShaderBind(InputName = "iPosition0", Uid = "po0")]
        Position0 = 1 << 0,
        [ShaderBind(InputName = "iPosition1", Uid = "po1")]
        Position1 = 1 << 1,
        [ShaderBind(InputName = "iPosition2", Uid = "po2")]
        Position2 = 1 << 2,
        [ShaderBind(InputName = "iPosition3", Uid = "po3")]
        Position3 = 1 << 3,


        [ShaderBind(InputName = "iNormal0", Uid = "no0")]
        Normal0 = 1 << 4,
        [ShaderBind(InputName = "iNormal1", Uid = "no1")]
        Normal1 = 1 << 5,


        [ShaderBind(InputName = "iColor0", Uid = "co0")]
        Color0 = 1 << 6,
        [ShaderBind(InputName = "iColor1", Uid = "co1")]
        Color1 = 1 << 7,
        [ShaderBind(InputName = "iColor2", Uid = "co2")]
        Color2 = 1 << 8,
        [ShaderBind(InputName = "iColor3", Uid = "co3")]
        Color3 = 1 << 9,


        [ShaderBind(InputName = "iUv0", Uid = "uv0")]
        Uv0 = 1 << 10,
        [ShaderBind(InputName = "iUv1", Uid = "uv1")]
        Uv1 = 1 << 11,
        [ShaderBind(InputName = "iUv2", Uid = "uv2")]
        Uv2 = 1 << 12,
        [ShaderBind(InputName = "iUv3", Uid = "uv3")]
        Uv3 = 1 << 13,
        [ShaderBind(InputName = "iUv4", Uid = "uv4")]
        Uv4 = 1 << 14,
        [ShaderBind(InputName = "iUv5", Uid = "uv5")]
        Uv5 = 1 << 15,
        [ShaderBind(InputName = "iUv6", Uid = "uv6")]
        Uv6 = 1 << 16,
        [ShaderBind(InputName = "iUv7", Uid = "uv7")]
        Uv7 = 1 << 17,


        [ShaderBind(InputName = "iAttr1", Uid = "at0")]
        Attr0 = 1 << 18,
        [ShaderBind(InputName = "iAttr1", Uid = "at1")]
        Attr1 = 1 << 19,
        [ShaderBind(InputName = "iAttr2", Uid = "at2")]
        Attr2 = 1 << 20,
        [ShaderBind(InputName = "iAttr3", Uid = "at3")]
        Attr3 = 1 << 21,
        [ShaderBind(InputName = "iAttr4", Uid = "at4")]
        Attr4 = 1 << 22,
        [ShaderBind(InputName = "iAttr5", Uid = "at5")]
        Attr5 = 1 << 23,
        [ShaderBind(InputName = "iAttr6", Uid = "at6")]
        Attr6 = 1 << 24,
        [ShaderBind(InputName = "iAttr7", Uid = "at7")]
        Attr7 = 1 << 25,
    }

    public static class VertexFlagExtensions
    {
        public static VertexFlag Combine(this IEnumerable<VertexFlag> vertexFlags)
        {
            var v = VertexFlag.None;

            foreach (var flag in vertexFlags)
            {
                v = v.AddFlag(flag);
            }

            return v;
        }

        public static string ShaderInputName(this VertexFlag vertexFlag)
        {
            var shaderBindAttribute = vertexFlag.GetAttribute<ShaderBindAttribute>();

            return shaderBindAttribute.InputName;
        }

        public static string StandardShaderDictionaryKey(this VertexFlag vertexFlag)
        {
            string key = string.Empty;

            foreach(var v in (VertexFlag[])Enum.GetValues(typeof(VertexFlag)))
            {
                if(!vertexFlag.HasFlag(v))
                {
                    continue;
                }

                var shaderBindAttribute = v.GetAttribute<ShaderBindAttribute>();

                if(shaderBindAttribute == null)
                {
                    continue;
                }

                key += shaderBindAttribute.Uid + "-";

            }

            return key.Substring(0, key.Length - 1);
        }
    }
}
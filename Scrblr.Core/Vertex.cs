using OpenTK.Mathematics;
using System;
using System.Linq;

namespace Scrblr.Core
{
    public class Vertex
    {
        public float[] Data;

        public VertexFlag VertexFlags
        {
            get { return Mapping.VertexFlags; }
        }

        public readonly VertexMapping Mapping;

        public Vertex(VertexFlag vertexFlag)
            : this(VertexMapping.QueryDefaultMapping(vertexFlag))
        {
        }

        public Vertex(VertexMapping mapping)
        {
            Mapping = mapping;
            Data = new float[Mapping.Stride];
        }

        public float[] Get(VertexFlag vertexFlag)
        {
            if(!VertexFlags.HasFlag(vertexFlag))
            {
                throw new InvalidOperationException($"GeometryVertex.Get(VertexFlag) failed. GeometryVertex contains no data for VertexFlag {vertexFlag}");
            }

            return Data.Skip(IndexFor(vertexFlag)).Take(SizeFor(vertexFlag)).ToArray();
        }

        public Vertex Set(params float[] data)
        {
            if (Mapping.Stride != data.Length)
            {
                throw new ArgumentException($"GeometryVertex.Set(params float[]) failed. data.Length is not equal to the Stride provided in the Mapping: {Mapping.Stride}");
            }
            return this;
        }

        public Vertex Set(VertexFlag vertexFlag, params float[] data)
        {
            var index = IndexFor(vertexFlag);
            var size = SizeFor(vertexFlag);

            if(size != data.Length)
            {
                throw new ArgumentException($"GeometryVertex.Set(VertexFlag, params float[]) failed. data.Length is not equal to the Size provided in the Mapping: {vertexFlag} {size}");
            }

            for(var i = 0; i < size; i++)
            {
                Data[index + i] = data[i];
            }
            return this;
        }

        private int IndexFor(VertexFlag vertexFlag)
        {
            var index = 0;

            foreach(var map in Mapping.Maps)
            {
                if(map.VertexFlag == vertexFlag)
                {
                    return index;
                }

                index += map.Stride;
            }

            return index;
        }

        private int SizeFor(VertexFlag vertexFlag)
        {
            foreach (var map in Mapping.Maps)
            {
                if (map.VertexFlag == vertexFlag)
                {
                    return map.Stride;
                }
            }

            return 0;
        }

        //public Vector3 _position;
        //public Vector3 _vertexNormal;
        //public Vector3 _faceNormal;
        ///// <summary>
        ///// default == Color4.White
        ///// </summary>
        //public Color4 _color0 = Color4.White;
        ///// <summary>
        ///// default == Color4.White
        ///// </summary>
        //public Color4 _color1 = Color4.White;
        //public Vector2 _uv0;
        //public Vector2 _uv1;
        //public Vector2 _uv2;
        //public Vector2 _uv3;

        //public Vertex()
        //{
        //}

        //public Vertex(float x, float y, float z = 0f)
        //    : this()
        //{
        //    Position(x, y, z);
        //}

        public Vertex Position(float x, float y, float z = 0f)
        {
            Set(VertexFlag.Position0, x, y, z);

            return this;
        }

        //public Vertex VertexNormal(float x, float y, float z)
        //{
        //    _vertexNormal.X = x;
        //    _vertexNormal.Y = y;
        //    _vertexNormal.Z = z;

        //    return this;
        //}

        //public Vertex FaceNormal(float x, float y, float z)
        //{
        //    _faceNormal.X = x;
        //    _faceNormal.Y = y;
        //    _faceNormal.Z = z;

        //    return this;
        //}

        //public Vertex Color0(float r, float g, float b, float a = -1f)
        //{
        //    _color0.R = r;
        //    _color0.G = g;
        //    _color0.B = g;
        //    _color0.A = a;

        //    return this;
        //}

        //public Vertex Color0(int r, int g, int b, int a = 255)
        //{
        //    return Color0(r * Utility.ByteToUnitSingleFactor, g * Utility.ByteToUnitSingleFactor, b * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
        //}

        //public Vertex Color0(float grey, float a = -1f)
        //{
        //    return Color0(grey, grey, grey, a);
        //}

        //public Vertex Color0(int grey, int a = 255)
        //{
        //    return Color0(grey * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
        //}

        //public Vertex Color1(float r, float g, float b, float a = -1f)
        //{
        //    _color1.R = r;
        //    _color1.G = g;
        //    _color1.B = g;
        //    _color1.A = a;

        //    return this;
        //}

        //public Vertex Color1(int r, int g, int b, int a = 255)
        //{
        //    return Color1(r * Utility.ByteToUnitSingleFactor, g * Utility.ByteToUnitSingleFactor, b * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
        //}

        //public Vertex Color1(float grey, float a = -1f)
        //{
        //    return Color1(grey, grey, grey, a);
        //}

        //public Vertex Color1(int grey, int a = 255)
        //{
        //    return Color1(grey * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
        //}

        //public Vertex Uv0(float u, float v)
        //{
        //    _uv0.X = u;
        //    _uv0.Y = v;

        //    return this;
        //}

        //public Vertex Uv1(float u, float v)
        //{
        //    _uv1.X = u;
        //    _uv1.Y = v;

        //    return this;
        //}

        //public Vertex Uv2(float u, float v)
        //{
        //    _uv2.X = u;
        //    _uv2.Y = v;

        //    return this;
        //}

        //public Vertex Uv3(float u, float v)
        //{
        //    _uv3.X = u;
        //    _uv3.Y = v;

        //    return this;
        //}
    }
}
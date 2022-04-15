using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public abstract class AbstractGeometry : IDisposable
    {
        #region Fields and Properties

        private struct RotationAxis
        {
            public float Radians;
            public Vector3 Axis;

            public RotationAxis(float radians, Vector3 axis)
            {
                Radians = radians;
                Axis = axis;
            }

            public RotationAxis(float radians, Axis axis)
            {
                Radians = radians;
                Axis = axis.ToVector();
            }
        }

        public class GeometryVertex
        {
            public Vector3 _position;
            public Vector3 _normal;
            public Color4 _color;
            public Vector2 _uv0;
            public Vector2 _uv1;
            public Vector2 _uv2;
            public Vector2 _uv3;

            public GeometryVertex()
            {
            }

            public GeometryVertex(float x, float y, float z = 0f)
                : this()
            {
                Position(x, y, z);
            }

            public GeometryVertex Position(float x, float y, float z = 0f)
            {
                _position.X = x;
                _position.Y = y;
                _position.Z = z;

                return this;
            }

            public GeometryVertex Normal(float x, float y, float z)
            {
                _normal.X = x;
                _normal.Y = y;
                _normal.Z = z;

                return this;
            }

            public GeometryVertex Color(float r, float g, float b, float a = -1f)
            {
                _color.R = r;
                _color.G = g;
                _color.B = g;
                _color.A = a;

                return this;
            }

            public GeometryVertex Color(int r, int g, int b, int a = 255)
            {
                return Color(r * Utility.ByteToUnitSingleFactor, g * Utility.ByteToUnitSingleFactor, b * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
            }

            public GeometryVertex Color(float grey, float a = -1f)
            {
                return Color(grey, grey, grey, a);
            }

            public GeometryVertex Color(int grey, int a = 255)
            {
                return Color(grey * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
            }

            public GeometryVertex Uv0(float u, float v)
            {
                _uv0.X = u;
                _uv0.Y = v;

                return this;
            }

            public GeometryVertex Uv1(float u, float v)
            {
                _uv1.X = u;
                _uv1.Y = v;

                return this;
            }

            public GeometryVertex Uv2(float u, float v)
            {
                _uv2.X = u;
                _uv2.Y = v;

                return this;
            }

            public GeometryVertex Uv3(float u, float v)
            {
                _uv3.X = u;
                _uv3.Y = v;

                return this;
            }
        }

        public GeometryType GeometryType { get; private set; }

        private GeometryVertex[] _vertices;

        public static readonly int DefaultVertexCount = 1024 * 32;

        public int VertexCount { get; private set; }

        public Vector3 _position;
        public Vector3 _scale;
        public Vector3 _normal;
        public Color4 _color;

        private static int DefaultRotationAxisArrayLenght = 8;

        private RotationAxis[] _rotationAxisArray = new RotationAxis[DefaultRotationAxisArrayLenght];

        private int _rotationAxisArrayCount;

        public Texture _texture0;
        public Texture _texture1;
        public Texture _texture2;
        public Texture _texture3;

        public Shader _shader;
        public Matrix4 _modelMatrix;

        /// <summary>
        /// default == VertexFlag.Position0 | VertexFlag.Color0
        /// </summary>
        public VertexFlag VertexFlag = VertexFlag.Position0 | VertexFlag.Color0;

        #endregion Fields and Properties

        #region Constructors

        public AbstractGeometry(GeometryType geometryType, Shader shader, Matrix4 modelMatrix)
            : this(geometryType, DefaultVertexCount, shader, modelMatrix)
        {
        }

        public AbstractGeometry(GeometryType geometryType, int vertexCount, Shader shader, Matrix4 modelMatrix)
        {
            GeometryType = geometryType;
            _vertices = new GeometryVertex[vertexCount];
            _shader = shader;
            _modelMatrix = modelMatrix;
        }

        #endregion Constructors

        public Matrix4 ModelMatrix()
        {
            var m = _modelMatrix;

            if(_scale.X != 0f || _scale.X != 0f || _scale.X != 0f)
            {
                m = m * Matrix4.CreateScale(_scale);
            }

            if (_rotationAxisArrayCount > 0)
            {
                for(var i = 0; i < _rotationAxisArrayCount; i++)
                {
                    var rotationAxis = _rotationAxisArray[i];

                    m = m * Matrix4.CreateFromAxisAngle(rotationAxis.Axis, rotationAxis.Radians);
                }
            }

            if (_position.X != 0f || _position.X != 0f || _position.X != 0f)
            {
                m = m * Matrix4.CreateTranslation(_position);
            }

            return m;
        }

        private void GrowRotationAxisArray()
        {
            var rotationAxisArray = new RotationAxis[_rotationAxisArray.Length + DefaultRotationAxisArrayLenght];

            _rotationAxisArray.CopyTo(rotationAxisArray, 0);

            _rotationAxisArray = rotationAxisArray;
        }

        private void AddToRotationAxisArray(float radians, Axis axis)
        {
            if(_rotationAxisArrayCount == _rotationAxisArray.Length)
            {
                GrowRotationAxisArray();
            }

            _rotationAxisArray[_rotationAxisArrayCount++] = new RotationAxis();
        }

        public AbstractGeometry Position(float x, float y, float z = 0f)
        {
            _position.X = x;
            _position.Y = y;
            _position.Z = z;

            return this;
        }

        public AbstractGeometry Normal(float x, float y, float z)
        {
            _normal.X = x;
            _normal.Y = y;
            _normal.Z = z;

            return this;
        }

        public AbstractGeometry Scale(float x, float y, float z = 1f)
        {
            _scale.X = x;
            _scale.Y = y;
            _scale.Z = z;

            return this;
        }

        public AbstractGeometry Rotate(float degrees, Axis axis)
        {
            AddToRotationAxisArray(MathHelper.DegreesToRadians(degrees), axis);

            return this;
        }

        public TGeometry Color<TGeometry>(float r, float g, float b, float a = -1f) where TGeometry : AbstractGeometry
        {
            _color.R = r;
            _color.G = b;
            _color.B = b;
            _color.A = a;

            return (TGeometry)this;
        }

        public TGeometry Color<TGeometry>(int r, int g, int b, int a = 255) where TGeometry : AbstractGeometry
        {
            return Color<TGeometry>(r * Utility.ByteToUnitSingleFactor, g * Utility.ByteToUnitSingleFactor, b * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
        }

        public TGeometry Color<TGeometry>(float grey, float a = -1f) where TGeometry : AbstractGeometry
        {
            return Color<TGeometry>(grey, grey, grey, a);
        }

        public TGeometry Color<TGeometry>(int grey, int a = 255) where TGeometry : AbstractGeometry
        {
            return Color<TGeometry>(grey * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
        }

        public AbstractGeometry Texture(Texture texture)
        {
            if (_texture0 == null)
            {
                _texture0 = texture;
            }
            else if (_texture1 == null)
            {
                _texture1 = texture;
            }
            else if (_texture2 == null)
            {
                _texture2 = texture;
            }
            else if (_texture3 == null)
            {
                _texture3 = texture;
            }
            else
            {
                throw new NotImplementedException("Shapes.AbstractShape.Texture(Texture texture) failed. Too many textures were attached.");
            }

            return this;
        }

        public AbstractGeometry Shader(Shader shader)
        {
            _shader = shader;

            return this;
        }

        public AbstractGeometry Transform(ref Matrix4 transform)
        {
            _modelMatrix = transform;

            return this;
        }

        public GeometryVertex[] Vertices()
        {
            return _vertices;
        }

        public GeometryVertex Vertex()
        {
            if (VertexCount >= _vertices.Length)
            {
                throw new InvalidOperationException($"Vertex(float x, float y) failed. _vertices.Length had been reached: {VertexCount}");
            }

            var v = new GeometryVertex();

            _vertices[VertexCount++] = v;

            return v;
        }

        public GeometryVertex Vertex(float x, float y, float z = 0f)
        {
            return Vertex().Position(x, y, z);
        }

        //protected void AllocateVertices()
        //{
        //    foreach(var v in _vertices)
        //    {
        //        Vertex();
        //    }
        //}

        public void Dispose()
        {
            _vertices = null;
        }
    }
}
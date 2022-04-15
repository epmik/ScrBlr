using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    //public interface IGeometry : IDisposable
    //{
    //    Matrix4 ModelMatrix();

    //    IGeometry Position(float x, float y, float z = 0f);

    //    IGeometry Normal(float x, float y, float z);

    //    IGeometry Scale(float x, float y, float z = 1f);

    //    IGeometry Rotate(float degrees, Axis axis);

    //    IGeometry Color(float r, float g, float b, float a = -1f);

    //    IGeometry Color(int r, int g, int b, int a = 255);

    //    IGeometry Color(float grey, float a = -1f);

    //    IGeometry Color(int grey, int a = 255);

    //    IGeometry Texture(Texture texture);

    //    IGeometry Shader(Shader shader);

    //    IGeometry Transform(ref Matrix4 transform);

    //    Geometry.GeometryVertex[] Vertices();

    //    Geometry.GeometryVertex Vertex();

    //    Geometry.GeometryVertex Vertex(float x, float y, float z = 0f);
    //}

    public class Geometry : IDisposable
    {
        #region Fields and Properties

        private enum TransformType
        {
            Translation,
            Rotation,
            Scale,
        }

        private struct Transform
        {
            public float Radians;
            public Vector3 Vector;
            public TransformType TransformType;
        }

        #region GeometryVertex

        public class GeometryVertex
        {
            public Vector3 _position;
            public Vector3 _normal;
            /// <summary>
            /// default == Color4.White
            /// </summary>
            public Color4 _color = Color4.White;
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

        #endregion GeometryVertex

        public GeometryType GeometryType { get; private set; }

        private GeometryVertex[] _vertices;

        public static readonly int DefaultVertexCount = 1024 * 32;

        public int VertexCount { get; private set; }

        public Vector3 _position;
        
        public Vector3 _scale;
        
        public Vector3 _normal;

        /// <summary>
        /// default == Color4.White
        /// </summary>
        public Color4 _color = Color4.White;

        private static int DefaultTransformStackSize = 8;

        private Transform[] _transformStack = new Transform[DefaultTransformStackSize];

        private int _transformArrayCount;

        public Texture _texture0;
        public Texture _texture1;
        public Texture _texture2;
        public Texture _texture3;

        public Shader _shader;
        public Matrix4 _modelMatrix;

        /// <summary>
        /// default == VertexFlag.Position0 | VertexFlag.Color0
        /// </summary>
        public VertexFlag VertexFlags = VertexFlag.Position0 | VertexFlag.Color0;

        #endregion Fields and Properties

        #region Constructors

        public Geometry(GeometryType geometryType, Shader shader, Matrix4 modelMatrix)
            : this(geometryType, DefaultVertexCount, shader, modelMatrix)
        {
        }

        public Geometry(GeometryType geometryType, int vertexCount, Shader shader, Matrix4 modelMatrix)
        {
            GeometryType = geometryType;
            _vertices = new GeometryVertex[vertexCount];
            _shader = shader;
            _modelMatrix = modelMatrix;
        }

        #endregion Constructors

        public Matrix4 ModelMatrix()
        {
            return ApplyTransformStack(_modelMatrix);
        }

        public Matrix4 ApplyTransformStack(Matrix4 m)
        {
            for (var i = 0; i < _transformArrayCount; i++)
            {
                var transform = _transformStack[i];

                switch (transform.TransformType)
                {
                    case TransformType.Translation:
                        m = m * Matrix4.CreateTranslation(transform.Vector);
                        break;
                    case TransformType.Scale:
                        m = m * Matrix4.CreateScale(transform.Vector);
                        break;
                    case TransformType.Rotation:
                        m = m * Matrix4.CreateFromAxisAngle(transform.Vector, transform.Radians);
                        break;
                    default:
                        throw new NotImplementedException($"Geometry.ModelMatrix() failed. Found unknown TransformType.Translation: {TransformType.Translation}");
                }
            }

            return m;
        }

        private void GrowTransformStack()
        {
            var transformStack = new Transform[_transformStack.Length + DefaultTransformStackSize];

            _transformStack.CopyTo(transformStack, 0);

            _transformStack = transformStack;
        }

        private Geometry AddTransform(TransformType transformType, Vector3 vector, float radians = 0f)
        {
            if (_transformArrayCount == _transformStack.Length)
            {
                GrowTransformStack();
            }

            _transformStack[_transformArrayCount++] = new Transform
            {
                TransformType = transformType,
                Vector = vector,
                Radians = radians
            };

            return this;
        }

        public virtual Geometry Position(float x, float y, float z = 0f)
        {
            _position.X = x;
            _position.Y = y;
            _position.Z = z;

            return this;
        }

        public Geometry Normal(float x, float y, float z)
        {
            VertexFlags = VertexFlags.AddFlag(VertexFlag.Normal0);

            _normal.X = x;
            _normal.Y = y;
            _normal.Z = z;

            return this;
        }

        public Geometry Translate(float x, float y, float z = 0f)
        {
            return AddTransform(TransformType.Translation, new Vector3(x, y, z));
        }

        public Geometry Scale(float s)
        {
            return Scale(s, s, s);
        }

        public Geometry Scale(float x, float y, float z = 1f)
        {
            return AddTransform(TransformType.Scale, new Vector3(x, y, z));
        }

        public Geometry Rotate(float degrees, Axis axis)
        {
            return Rotate(degrees, axis.ToVector());
        }

        public Geometry Rotate(float degrees, Vector3 axis)
        {
            return AddTransform(TransformType.Rotation, axis, MathHelper.DegreesToRadians(degrees));
        }

        public Geometry Color(float r, float g, float b, float a = 1f)
        {
            _color.R = r;
            _color.G = g;
            _color.B = b;
            _color.A = a;

            return this;
        }

        public Geometry Color(int r, int g, int b, int a = 255)
        {
            return Color(r * Utility.ByteToUnitSingleFactor, g * Utility.ByteToUnitSingleFactor, b * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
        }

        public Geometry Color(float grey, float a = 1f)
        {
            return Color(grey, grey, grey, a);
        }

        public Geometry Color(int grey, int a = 255)
        {
            return Color(grey * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
        }

        public Geometry Texture(Texture texture)
        {
            if (_texture0 == null)
            {
                VertexFlags = VertexFlags.AddFlag(VertexFlag.Uv0);

                _texture0 = texture;
            }
            else if (_texture1 == null)
            {
                VertexFlags = VertexFlags.AddFlag(VertexFlag.Uv1);

                _texture1 = texture;
            }
            else if (_texture2 == null)
            {
                VertexFlags = VertexFlags.AddFlag(VertexFlag.Uv2);

                _texture2 = texture;
            }
            else if (_texture3 == null)
            {
                VertexFlags = VertexFlags.AddFlag(VertexFlag.Uv3);

                _texture3 = texture;
            }
            else
            {
                throw new NotImplementedException("Shapes.AbstractShape.Texture(Texture texture) failed. Too many textures were attached.");
            }

            return this;
        }

        public Geometry Shader(Shader shader)
        {
            _shader = shader;

            return this;
        }

        public Shader Shader()
        {
            return _shader;
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

        protected void AllocateVertices()
        {
            foreach (var v in _vertices)
            {
                Vertex();
            }
        }

        public class QuadGeometry : Geometry
        {
            /// <summary>
            /// default == 1f
            /// </summary>
            public float _width = 1f;

            /// <summary>
            /// default == 1f
            /// </summary>
            public float _height = 1f;

            private GeometryVertex[] _defaultVertices = new GeometryVertex[]
            {
                // left top
                new GeometryVertex().Position(0.5f, 0.5f).Color(1f, 1f, 1f).Normal(0, 0, 1f).Uv0(0f, 0f).Uv1(0f, 0f).Uv2(0f, 0f).Uv3(0f, 0f),
                // left bottom
                new GeometryVertex().Position(0.5f, -0.5f).Color(1f, 1f, 1f).Normal(0, 0, 1f).Uv0(0f, 1f).Uv1(0f, 1f).Uv2(0f, 1f).Uv3(0f, 1f),
                // right top
                new GeometryVertex().Position(-0.5f, 0.5f).Color(1f, 1f, 1f).Normal(0, 0, 1f).Uv0(1f, 0f).Uv1(1f, 0f).Uv2(1f, 0f).Uv3(1f, 0f),
                // right bottom
                new GeometryVertex().Position(-0.5f, -0.5f).Color(1f, 1f, 1f).Normal(0, 0, 1f).Uv0(1f, 1f).Uv1(1f, 1f).Uv2(1f, 1f).Uv3(1f, 1f),
            };

            public QuadGeometry(Shader shader, Matrix4 modelMatrix)
                : base(GeometryType.TriangleStrip, 4, shader, modelMatrix)
            {
                AllocateVertices();

                for(var i = 0; i < _vertices.Length; i++)
                {
                    _vertices[i]._position = _defaultVertices[i]._position;
                    _vertices[i]._normal = _defaultVertices[i]._normal;
                    _vertices[i]._color = _defaultVertices[i]._color;
                    _vertices[i]._uv0 = _defaultVertices[i]._uv0;
                    _vertices[i]._uv1 = _defaultVertices[i]._uv1;
                    _vertices[i]._uv2 = _defaultVertices[i]._uv2;
                    _vertices[i]._uv3 = _defaultVertices[i]._uv3;
                }
            }

            public QuadGeometry Width(float w)
            {
                _width = w;

                return UpdateVertexPositions();
            }

            public QuadGeometry Height(float h)
            {
                _height = h;

                return UpdateVertexPositions();
            }

            public override Geometry Position(float x, float y, float z = 0f)
            {
                base.Position(x, y, z);

                return UpdateVertexPositions();
            }

            private QuadGeometry UpdateVertexPositions()
            {
                var hw = _width * 0.5f;
                var hh = _height * 0.5f;

                // ccw triangle order
                _vertices[0].Position(_position.X + hw, _position.Y + hh);
                _vertices[1].Position(_position.X + hw, _position.Y - hh);
                _vertices[2].Position(_position.X - hw, _position.Y + hh);
                _vertices[3].Position(_position.X - hw, _position.Y - hh);

                return this;
            }
        }

        public void Dispose()
        {
            _vertices = null;
        }
    }
}
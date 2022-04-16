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

    public partial class Geometry : IDisposable
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

        public GeometryType GeometryType { get; private set; }

        private Vertex[] _vertices;

        public static readonly int DefaultVertexCount = 1024 * 32;

        public int VertexSize { get { return _vertices.Length; } }

        public int _vertexCount;

        public Vector3 _position;
        
        public Vector3 _scale;
        
        public Vector3 _normal;

        /// <summary>
        /// default == black
        /// </summary>
        public float[] _color = new float[] { 0f, 0f, 0f, 1f };

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
            _vertices = new Vertex[vertexCount];
            _shader = shader;
            _modelMatrix = modelMatrix;
        }

        #endregion Constructors

        public Matrix4 ModelMatrix()
        {
            return ApplyTransformStack(_modelMatrix);
        }

        private Matrix4 ApplyTransformStack(Matrix4 m)
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

        private Geometry AddTransform(TransformType transformType, Vector3 vector, float radians = 0f)
        {
            if (_transformArrayCount == _transformStack.Length)
            {
                Array.Resize(ref _transformStack, _transformStack.Length + DefaultTransformStackSize);
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
            _color[0] = r;
            _color[1] = g;
            _color[2] = b;
            _color[3] = a;

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

        //public Geometry Shader(Shader shader)
        //{
        //    _shader = shader;

        //    return this;
        //}

        //public Shader Shader()
        //{
        //    return _shader;
        //}

        public Vertex[] Vertices()
        {
            return _vertices;
        }

        public Vertex Vertex()
        {
            if (_vertexCount >= _vertices.Length)
            {
                throw new InvalidOperationException($"Vertex(float x, float y) failed. _vertices.Length had been reached: {_vertexCount}");
            }

            var v = new Vertex(VertexFlags);

            _vertices[_vertexCount++] = v;

            return v;
        }

        public Vertex Vertex(float x, float y, float z = 0f)
        {
            return Vertex().Set(VertexFlag.Position0, x, y, z);
        }

        private static readonly float[] DefaultNormal = new float[] { 0, 0, 1f };
        private static readonly float[] DefaultColor = new float[] { 1, 1, 1f, 1f };

        public virtual void WriteTo(VertexBuffer vertexBuffer)
        {

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

            public QuadGeometry(Shader shader, Matrix4 modelMatrix)
                : base(GeometryType.TriangleStrip, 4, shader, modelMatrix)
            {
                
            }

            public QuadGeometry Width(float w)
            {
                _width = w;

                return this;
            }

            public QuadGeometry Height(float h)
            {
                _height = h;

                return this;
            }

            //public override Geometry Position(float x, float y, float z = 0f)
            //{
            //    base.Position(x, y, z);

            //    return UpdateVertexPositions();
            //}

            private float[][] Points()
            {
                var hw = _width * 0.5f;
                var hh = _height * 0.5f;

                // ccw triangle order
                return new float[][]
                {
                    // top left
                    new float[] { _position.X + hw, _position.Y + hh, 0f },
                    // bottom left
                    new float[] { _position.X + hw, _position.Y - hh, 0f },
                    // top right
                    new float[] { _position.X - hw, _position.Y + hh, 0f },
                    // bottom right
                    new float[] { _position.X - hw, _position.Y - hh, 0f },
                };
            }

            private float[][] Uvs()
            {
                return new float[][]
                {
                    // top left
                    new float[] { 0f, 0f },
                    // bottom left
                    new float[] { 0, 1f },
                    // top right
                    new float[] { 1f, 0f },
                    // bottom right
                    new float[] { 1f, 1f },
                };
            }

            public override void WriteTo(VertexBuffer vertexBuffer)
            {
                var points = Points();
                var uvs = Uvs();

                for (var i = 0; i < 4; i++)
                {
                    vertexBuffer.WriteFixed(VertexFlag.Position0, points[i]);
                    
                    vertexBuffer.WriteFixed(VertexFlag.Normal0, DefaultNormal);

                    vertexBuffer.WriteFixed(VertexFlag.Color0, ref _color);

                    vertexBuffer.WriteFixed(VertexFlag.Uv0, uvs[i]);

                    vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);
                }
            }
        }

        public void Dispose()
        {
            _vertices = null;
        }
    }
}
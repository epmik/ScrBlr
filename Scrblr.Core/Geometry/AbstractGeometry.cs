using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public abstract class AbstractGeometry : IGeometry
    {
        protected enum TransformType
        {
            Translation,
            Rotation,
            Scale,
        }

        protected struct Transform
        {
            public float Radians;
            public Vector3 Vector;
            public TransformType TransformType;
        }

        public Guid Guid = Guid.NewGuid();

        protected int _vertexCount;

        // false
        protected bool _vertexCountIsResolutionDependant;

        /// <summary>
        /// VertexCount is dependent upon 
        /// </summary>
        /// <param name="modelMatrix"></param>
        /// <param name="viewMatrix"></param>
        /// <param name="projectionMatrix"></param>
        /// <returns></returns>
        public int VertexCount(Matrix4 modelMatrix, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            return VertexCount(ref modelMatrix, ref viewMatrix, ref projectionMatrix);
        }

        /// <summary>
        /// VertexCount is dependent upon 
        /// </summary>
        /// <param name="modelMatrix"></param>
        /// <param name="viewMatrix"></param>
        /// <param name="projectionMatrix"></param>
        /// <returns></returns>
        public virtual int VertexCount(ref Matrix4 modelMatrix, ref Matrix4 viewMatrix, ref Matrix4 projectionMatrix)
        {
            if (!_vertexCountIsResolutionDependant)
            {
                return _vertexCount;
            }
            // TODO implement
            return _vertexCount;
        }

        /// <summary>
        /// VertexCount is dependent upon 
        /// </summary>
        /// <param name="modelMatrix"></param>
        /// <param name="viewMatrix"></param>
        /// <param name="projectionMatrix"></param>
        /// <returns></returns>
        public virtual int VertexCount(Matrix4 modelViewProjectionMatrix)
        {
            return VertexCount(ref modelViewProjectionMatrix);
        }

        /// <summary>
        /// VertexCount is dependent upon 
        /// </summary>
        /// <param name="modelMatrix"></param>
        /// <param name="viewMatrix"></param>
        /// <param name="projectionMatrix"></param>
        /// <returns></returns>
        public virtual int VertexCount(ref Matrix4 modelViewProjectionMatrix)
        {
            if (!_vertexCountIsResolutionDependant)
            {
                return _vertexCount;
            }
            // TODO implement
            return _vertexCount;
        }

        public GeometryType GeometryType { get; private set; }

        /// <summary>
        /// default == VertexFlag.Position0 | VertexFlag.Color0
        /// </summary>
        public VertexFlag VertexFlags { get; protected set; } = VertexFlag.Position0;

        // along the positive z-axis
        protected static readonly float[] DefaultNormal = new float[] { 0, 0, 1f };

        // black
        protected static readonly float[] DefaultColor = new float[] { 0, 0, 0, 1f };

        private static int DefaultTransformStackSize = 8;

        private Transform[] _transformStack = new Transform[DefaultTransformStackSize];

        private int _transformArrayCount;

        //public Shader _shader;
        public Matrix4 _modelMatrix;

        public Texture _texture0;
        public Texture _texture1;
        public Texture _texture2;
        public Texture _texture3;

        public virtual RenderBatch[] ToRenderBatch(GraphicsContext graphicsContext, GraphicsState graphicsState, Shader shader, VertexBuffer vertexBuffer, ICamera camera)
        {
            var modelMatrix = ModelMatrix();
            var viewMatrix = camera.ViewMatrix();
            var projectionMatrix = camera.ProjectionMatrix();

            var vertexCount = VertexCount(ref modelMatrix, ref viewMatrix, ref projectionMatrix);

            WriteToVertexBuffer(vertexBuffer);

            return new[] {
                new RenderBatch
                {
                    State = graphicsState,
                    Shader = shader,
                    VertexBuffer = vertexBuffer,
                    ViewMatrix = viewMatrix,
                    ViewPosition = camera.Position,
                    ProjectionMatrix = projectionMatrix,
                    ModelMatrix = modelMatrix,
                    GeometryType = GeometryType,
                    ElementCount = vertexCount,
                    ElementIndex = vertexBuffer.UsedElements() - vertexCount,
                    Texture0 = _texture0,
                    Texture1 = _texture1,
                    Texture2 = _texture2,
                    VertexFlags = VertexFlags,
                }
            };
        }

        public abstract void WriteToVertexBuffer(VertexBuffer vertexBuffer);

        public AbstractGeometry(GeometryType geometryType, Matrix4 modelMatrix)
            : this(geometryType, 0, modelMatrix)
        {
        }

        public AbstractGeometry(GeometryType geometryType, int vertexCount, Matrix4 modelMatrix)
        {
            _vertexCount = vertexCount;
            GeometryType = geometryType;
            _modelMatrix = modelMatrix;
        }

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

        protected TGeometry AddTransform<TGeometry>(TransformType transformType, Vector3 vector, float radians = 0f) where TGeometry : AbstractGeometry<TGeometry>
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

            return (TGeometry)this;
        }

        public virtual void Dispose()
        {

        }
    }

    public abstract class AbstractGeometry<TGeometry> : AbstractGeometry where TGeometry : AbstractGeometry<TGeometry>
    {
        #region Fields and Properties

        public Vector3 _position;

        public Vector3 _scale;

        public float[] _normal = new float[] { 0, 0, 0 };

        /// <summary>
        /// default == black
        /// </summary>
        public float[] _color = new float[] { 0, 0, 0, 1f };


        #endregion Fields and Properties

        #region Constructors

        public AbstractGeometry(GeometryType geometryType, Matrix4 modelMatrix)
            : this(geometryType, 0, modelMatrix)
        {
        }

        public AbstractGeometry(GeometryType geometryType, int vertexCount, Matrix4 modelMatrix)
            : base(geometryType, vertexCount, modelMatrix)
        {
        }

        #endregion Constructors

        public virtual TGeometry Position(float x, float y, float z = 0f)
        {
            _position.X = x;
            _position.Y = y;
            _position.Z = z;

            return (TGeometry)this;
        }

        public virtual TGeometry Normal(float x, float y, float z)
        {
            VertexFlags = VertexFlags.AddFlag(VertexFlag.Normal0);

            _normal[0] = x;
            _normal[1] = y;
            _normal[2] = z;

            return (TGeometry)this;
        }

        public virtual TGeometry Translate(float x, float y, float z = 0f) 
        {
            return AddTransform<TGeometry>(TransformType.Translation, new Vector3(x, y, z));
        }

        public virtual TGeometry Scale(float s) 
        {
            return Scale(s, s, s);
        }

        public virtual TGeometry Scale(float x, float y, float z = 1f) 
        {
            return AddTransform<TGeometry>(TransformType.Scale, new Vector3(x, y, z));
        }

        public virtual TGeometry Rotate(float degrees, Axis axis) 
        {
            return Rotate(degrees, axis.ToVector());
        }

        public virtual TGeometry Rotate(float degrees, Vector3 axis) 
        {
            return AddTransform<TGeometry>(TransformType.Rotation, axis, MathHelper.DegreesToRadians(degrees));
        }

        public virtual TGeometry Color(float r, float g, float b, float a = 1f) 
        {
            VertexFlags = VertexFlags.AddFlag(VertexFlag.Color0);

            _color[0] = r;
            _color[1] = g;
            _color[2] = b;
            _color[3] = a;

            return (TGeometry)this;
        }

        public TGeometry Color(Color4 color)
        {
            return Color(color.R, color.G, color.B, color.A);
        }

        public TGeometry Color(int r, int g, int b, int a = 255) 
        {
            return Color(r * Utility.ByteToUnitSingleFactor, g * Utility.ByteToUnitSingleFactor, b * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
        }

        public TGeometry Color(float grey, float a = 1f) 
        {
            return Color(grey, grey, grey, a);
        }

        public TGeometry Color(int grey, int a = 255) 
        {
            return Color(grey * Utility.ByteToUnitSingleFactor, a * Utility.ByteToUnitSingleFactor);
        }

        public virtual TGeometry Texture(Texture texture) 
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

            return (TGeometry)this;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scrblr.Core
{
    public class GraphicsContext : GraphicsSettings, IDisposable
    {
        #region Shader Sources

        internal const string Po0Co0Vss = @"
#version 330 core

in vec3 iPosition0;  
in vec4 iColor0;

out vec4 ioColor0;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;

	ioColor0 = iColor0;
}";

        internal const string Po0Co0Fss = @"
#version 330 core

in vec4 ioColor0;
in vec2 ioUv0;

out vec4 oColor0;

void main()
{
    oColor0 = ioColor0;
}";


        internal const string Po0Uv0Vss = @"
#version 330 core

in vec3 iPosition0;  
in vec2 iUv0;

out vec2 ioUv0;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;

	ioUv0 = iUv0;
}";

        internal const string Po0Uv0Fss = @"
#version 330 core

uniform sampler2D uTexture0;

in vec2 ioUv0;

out vec4 oColor0;

void main()
{
    oColor0 = texture(uTexture0, ioUv0); 
}";


        internal const string Po0Co0Uv0Vss = @"
#version 330 core

in vec3 iPosition0;  
in vec4 iColor0;
in vec2 iUv0;

out vec4 ioColor0;
out vec2 ioUv0;

uniform mat4 uModelMatrix;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

void main(void)
{
    gl_Position = vec4(iPosition0, 1.0) * uModelMatrix * uViewMatrix * uProjectionMatrix;

	ioColor0 = iColor0;
	ioUv0 = iUv0;
}";

        internal const string Po0Co0Uv0Fss = @"
#version 330 core

uniform sampler2D uTexture0;

in vec4 ioColor0;
in vec2 ioUv0;

out vec4 oColor0;

void main()
{
    oColor0 = texture(uTexture0, ioUv0) * ioColor0; 
}";

        #endregion Shader Sources

        #region Fields and Properties


        private int _vertexBufferElementCount = 32768;

        public VertexBuffer VertexBuffer { get; set; }

        /// <summary>
        /// default == 256
        /// </summary>
        private int _maxRenderChunks = 256;

        private int _renderChunkCount;

        private RenderChunk[] _renderChunks;


        /// <summary>
        /// default == 4096
        /// </summary>
        private int _maxGeometryCount = 4096;

        private int _geometryCount;

        private Geometry[] _geometry;


        /// <summary>
        /// default == 4096
        /// </summary>
        private int _maxTesselatedGeometryCount = 4096;

        private int _tesselatedGeometryCount;

        private Geometry[] _tesselatedGeometry;




        private ICamera _activeCamera { get; set; }

        public ICamera ActiveCamera { get { return _activeCamera; } set { _activeCamera = value; } }

        private static int GraphicsContextCount { get; set; }

        public static GraphicsContext Default { get; set; }

        /// <summary>
        /// default == Color4.White
        /// </summary>
        protected Color4 _clearColor = Color4.White;

        /// <summary>
        /// the OpenGl Handle to the intenal framebuffer if any was created, or 0 if this is the default framebuffer
        /// default == 0 or the default framebuffer
        /// </summary>
        public int Handle
        {
            get
            {
                return _frameBuffer == null ? 0 : _frameBuffer.Handle;
            }
        }

        public bool IsDefault
        {
            get { return _frameBuffer == null; }
        }

        private FrameBuffer _frameBuffer;

        private Shader _activeShader { get; set; }

        public void ActiveShader(Shader shader)
        {
            _activeShader = shader;
        }

        public Shader ActiveShader()
        {
            return _activeShader;
        }

        private Dictionary<string, Shader> _standardShaderDictionary;

        #endregion Fields and Properties

        #region Constructors

        public GraphicsContext(
            int width,
            int height,
            int colorBits = GraphicsSettings.DefaultColorBits,
            int depthBits = GraphicsSettings.DefaultDepthBits,
            int stencilBits = GraphicsSettings.DefaultStencilBits,
            int samples = GraphicsSettings.DefaultSamples)
            : base(width, height, colorBits, depthBits, stencilBits, samples)
        {
            ModelMatrix = Matrix4.Identity;

            GraphicsContextCount++;

            if (GraphicsContextCount == 1)
            {
                // default OpenGl context
                GraphicsContext.Default = this;
            }
            else
            {
                // custom OpenGl context, create a framebuffer to render to
                _frameBuffer = new FrameBuffer(Width, Height, ColorBits, DepthBits, StencilBits, Samples);
            }
        }

        public GraphicsContext(GraphicsSettings graphicsSettings)
            : this(
                  graphicsSettings.Width,
                  graphicsSettings.Height,
                  graphicsSettings.ColorBits,
                  graphicsSettings.DepthBits,
                  graphicsSettings.StencilBits,
                  graphicsSettings.Samples)
        {
        }

        #endregion Constructors

        #region Matrix Stack Stuff

        private const int _modelMatrixStackSize = 128;
        private int _modelMatrixStackIndex = 0;

        private Matrix4[] _modelMatrixStack = new Matrix4[_modelMatrixStackSize];

        public Matrix4 ModelMatrix
        {
            get
            {
                return _modelMatrixStack[_modelMatrixStackIndex];
            }
            set
            {
                _modelMatrixStack[_modelMatrixStackIndex] = value;
            }
        }

        public void ClearMatrixStack()
        {
            _modelMatrixStackIndex = 0;
            _modelMatrixStack[_modelMatrixStackIndex] = Matrix4.Identity;
        }

        public void PushMatrix()
        {
            if (_modelMatrixStackIndex + 1 >= _modelMatrixStackSize)
            {
                throw new InvalidOperationException($"PushMatrix() failed. _currentModelMatrixStackIndex has been reached: {_modelMatrixStackIndex}");
            }

            _modelMatrixStack[_modelMatrixStackIndex + 1] = _modelMatrixStack[_modelMatrixStackIndex];
            _modelMatrixStackIndex++;
        }

        public void PopMatrix()
        {
            if (_modelMatrixStackIndex == 0)
            {
                return;
                // throw new InvalidOperationException($"PopMatrix() failed. _currentModelMatrixStackIndex is {_currentModelMatrixStackIndex}. There are more PopMatrix() calls then PushMatrix() calls.");
            }

            _modelMatrixStackIndex--;
        }

        public void Translate(float x, float y)
        {
            // assume 2d translation along the x-axis and y-axis
            Translate(x, y, 0f);
        }

        public void Translate(float x, float y, float z)
        {
            // todo test this out
            //Matrix4.Mult(_modelMatrixStack[_currentModelMatrixStackIndex], Matrix4.CreateTranslation(x, y, z), out _modelMatrixStack[_currentModelMatrixStackIndex]);

            ModelMatrix = ModelMatrix * Matrix4.CreateTranslation(x, y, z);
        }

        public void Scale(float x, float y)
        {
            // assume 2d scaling along the x-axis and y-axis
            Scale(x, y, 1f);
        }

        public void Scale(float x, float y, float z)
        {
            // todo test this out
            //Matrix4.Mult(_modelMatrixStack[_currentModelMatrixStackIndex], Matrix4.CreateScale(x, y, z), out _modelMatrixStack[_currentModelMatrixStackIndex]);

            ModelMatrix = ModelMatrix * Matrix4.CreateScale(x, y, z);
        }

        public void Rotate(float degrees, Vector3 axis)
        {
            // todo test this out
            //Matrix4.Mult(_modelMatrixStack[_currentModelMatrixStackIndex], Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(degrees)), out _modelMatrixStack[_currentModelMatrixStackIndex]);

            ModelMatrix = ModelMatrix * Matrix4.CreateFromAxisAngle(axis, MathHelper.DegreesToRadians(degrees));
        }

        /// <summary>
        /// default == PrimitiveType.Triangles
        /// </summary>
        public PrimitiveType ActivePrimitiveType { get; set; } = PrimitiveType.Triangles;

        public void Render()
        {
            Render(ActivePrimitiveType, 0, VertexBuffer.TotelElements());
        }

        public void Render(int index, int count)
        {
            Render(ActivePrimitiveType, index, count);
        }

        public void Render(PrimitiveType primitiveType, int index, int count)
        {
            VertexBuffer.Bind();

            var shader = ActiveOrStandardShader();

            shader.Use();

            GL.DrawArrays(primitiveType, index, count);
        }

        private Shader ActiveOrStandardShader()
        {
            var shader = ActiveShader();

            if (shader == null)
            {
                shader = QueryStandardShader(VertexBuffer.EnabledVertexFlags);
            }

            return shader;
        }

        /// <summary>
        /// default == VertexFlag.Position0 | VertexFlag.Color0
        /// </summary>
        //private VertexFlag ActiveVertexBufferFlags = VertexFlag.Position0 | VertexFlag.Color0;

        public void EnableVertexBuffer(VertexFlag vertexFlags)
        {
            EnableVertexBuffer(vertexFlags, ActiveShader());
        }

        public void EnableVertexBuffer(VertexFlag vertexFlags, Shader shader)
        {
            VertexBuffer.ToggleElements(shader, vertexFlags);
        }

        public void Rotate(float degrees)
        {
            // assume 2d rotation along the z-axis
            Rotate(degrees, Vector3.UnitZ);
        }

        public void Rotate(float degrees, Axis axis)
        {
            Rotate(degrees, axis.ToVector());
        }

        #endregion Matrix Stack Stuff

        #region Load

        public virtual void Load()
        {
            InitializeStandardShaderDictionary();

            _renderChunks = new RenderChunk[_maxRenderChunks];

            for (var i = 0; i < _maxRenderChunks; i++)
            {
                _renderChunks[i] = new RenderChunk();
            }

            _geometry = new Geometry[_maxGeometryCount];

            _tesselatedGeometry = new Geometry[_maxTesselatedGeometryCount];

            VertexBuffer = new VertexBuffer(
                _vertexBufferElementCount,
                new[] {
                    new VertexBufferLayout.Part { VertexFlag = VertexFlag.Position0, ElementType = VertexBufferLayout.ElementType.Single, Count = 3 },
                    //new VertexBufferLayout.Part { VertexFlag = VertexFlag.Normal0, ElementType = VertexBufferLayout.ElementType.Single, Count = 3 },
                    new VertexBufferLayout.Part { VertexFlag = VertexFlag.Color0, ElementType = VertexBufferLayout.ElementType.Single, Count = 4 },
                    new VertexBufferLayout.Part { VertexFlag = VertexFlag.Uv0, ElementType = VertexBufferLayout.ElementType.Single, Count = 2 },
                    new VertexBufferLayout.Part { VertexFlag = VertexFlag.Uv1, ElementType = VertexBufferLayout.ElementType.Single, Count = 2 },
                },
                VertexBufferUsage.DynamicDraw);

            // disable depth testing by default for 2d
            Disable(EnableFlag.DepthTest);
        }

        #endregion Load

        #region Reset

        public void Reset()
        {
            VertexBuffer.Clear();

            ResetRenderChunks();

            ResetGeometry();

            ResetTesselatedGeometry();
        }

        private void ResetRenderChunks()
        {
            _renderChunkCount = 0;
        }

        #endregion Reset

        #region Flush

        public void Flush()
        {
            if (_geometryCount < 1)
            {
                return;
            }

            TesselateGeometry();

            GeometryToRenderChunks();

            if (_renderChunkCount < 1)
            {
                return;
            }

            FlushRenderChunk();

            Reset();
        }

        private void FlushRenderChunk()
        {
            for (var c = 0; c < _renderChunkCount; c++)
            {
                var renderChunk = _renderChunks[c];

                renderChunk.Shader.Use();

                renderChunk.Shader.Uniform("uViewMatrix", renderChunk.ViewMatrix);
                renderChunk.Shader.Uniform("uProjectionMatrix", renderChunk.ProjectionMatrix);
                renderChunk.Shader.Uniform("uModelMatrix", renderChunk.ModelMatrix);

                if (renderChunk.VertexFlag.HasFlag(VertexFlag.Uv0))
                {
                    renderChunk.Texture0.UnitAndBind(TextureUnit.Texture0);
                    renderChunk.Shader.Uniform("uTexture0", 0);
                }

                renderChunk.VertexBuffer.Bind();
                renderChunk.VertexBuffer.ToggleElements(renderChunk.Shader);

                GL.DrawArrays((PrimitiveType)renderChunk.GeometryType, renderChunk.ElementIndex, renderChunk.ElementCount);
            }
        }

        #endregion Flush

        #region Standard Shaders Functions

        private void InitializeStandardShaderDictionary()
        {
            if (_standardShaderDictionary == null)
            {
                _standardShaderDictionary = new Dictionary<string, Shader>();
            }

            _standardShaderDictionary.Clear();

            InitializeStandardShaderDictionary("Po0Co0", GraphicsContext.Po0Co0Vss, GraphicsContext.Po0Co0Fss);
            InitializeStandardShaderDictionary("Po0Uv0", GraphicsContext.Po0Uv0Vss, GraphicsContext.Po0Uv0Fss);
            InitializeStandardShaderDictionary("Po0Co0Uv0", GraphicsContext.Po0Co0Uv0Vss, GraphicsContext.Po0Co0Uv0Fss);
            //InitializeStandardShaderDictionary("Po0Co0Uv0Uv1", GraphicsContext.Po0Co0Uv0Uv1Vss, GraphicsContext.Po0Co0Uv0Uv1Fss);

            //InitializeStandardShaderDictionary("Po0Uv0");
            //InitializeStandardShaderDictionary("Po0Co0Uv0");
            //InitializeStandardShaderDictionary("Po0Co0Uv0Uv1");

            //InitializeStandardShaderDictionary(VertexFlag.Position0 | VertexFlag.Color0);
            //InitializeStandardShaderDictionary(VertexFlag.Position0 | VertexFlag.Uv0);
            //InitializeStandardShaderDictionary(VertexFlag.Position0 | VertexFlag.Color0 | VertexFlag.Uv0);
            //InitializeStandardShaderDictionary(VertexFlag.Position0 | VertexFlag.Color0 | VertexFlag.Uv0 | VertexFlag.Uv1);
        }

        private void InitializeStandardShaderDictionary(string key, string vertexShaderSource, string fragmentShaderSource)
        {
            var shader = new Shader(vertexShaderSource, fragmentShaderSource);

            _standardShaderDictionary.Add(key.ToLowerInvariant(), shader);
        }

        //private void InitializeStandardShaderDictionary(VertexFlag vertexFlag)
        //{
        //    InitializeStandardShaderDictionary(vertexFlag.ShaderUid());
        //}

        //private void InitializeStandardShaderDictionary(string key)
        //{
        //    var vertexShaderSource = QueryGraphicsContextShaderSource(key, "vss");
        //    var fragmentShaderSource = QueryGraphicsContextShaderSource(key, "fss");

        //    if (vertexShaderSource != null && fragmentShaderSource != null)
        //    {
        //        var shader = new Shader(vertexShaderSource, fragmentShaderSource);

        //        _standardShaderDictionary.Add(key.ToLowerInvariant(), shader);

        //        return;
        //    }

        //    if (vertexShaderSource == null && fragmentShaderSource == null)
        //    {
        //        return;
        //    }

        //    if (vertexShaderSource != null)
        //    {
        //        throw new NotImplementedException($"InitializeStandardShaderDictionary(string key) failed. could not find vertex shader source for key: {key}. The GraphicsContext class should contain a const property with the name {key}vss (case incensitive)");
        //    }

        //    if (fragmentShaderSource != null)
        //    {
        //        throw new NotImplementedException($"InitializeStandardShaderDictionary(string key) failed. could not find flagment shader source for key: {key}. The GraphicsContext class should contain a const property with the name {key}fss (case incensitive)");
        //    }
        //}

        //private string QueryGraphicsContextShaderSource(string key, string postfix)
        //{
        //    var fieldInfo = (typeof(GraphicsContext)).GetField(key + "Vss", BindingFlags.IgnoreCase);

        //    if(fieldInfo == null)
        //    {
        //        return null;
        //    }

        //    return (string)fieldInfo.GetValue(null);
        //}

        //private Shader QueryStandardShaderFor(Geometry geometry)
        //{
        //    if(geometry.Shader() != null)
        //    {
        //        return geometry.Shader();
        //    }

        //    return QueryStandardShaderFor(_vertexBuffer);
        //}

        protected Shader QueryStandardShader(VertexBuffer vertexBuffer)
        {
            return QueryStandardShader(vertexBuffer.VertexFlags(true), vertexBuffer);
        }

        protected Shader QueryStandardShader(VertexFlag vertexFlag, VertexBuffer vertexBuffer)
        {
            var key = vertexFlag.StandardShaderDictionaryKey();

            if (_standardShaderDictionary.ContainsKey(key.ToLowerInvariant()))
            {
                return _standardShaderDictionary[key];
            }

            throw new InvalidOperationException($"QueryShaderFor(VertexBuffer vertexBuffer) failed. Could not find a shader source for key: {key}.");
        }

        protected Shader QueryStandardShader(VertexFlag vertexFlag)
        {
            var key = vertexFlag.StandardShaderDictionaryKey();

            if (_standardShaderDictionary.ContainsKey(key.ToLowerInvariant()))
            {
                return _standardShaderDictionary[key];
            }

            throw new InvalidOperationException($"QueryShaderFor(VertexBuffer vertexBuffer) failed. Could not find a shader source for key: {key}.");
        }

        #endregion Standard Shaders Functions

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

            GL.Viewport(0, 0, Width, Height);
        }

        public void Enable(EnableFlag enableFlag)
        {
            GL.Enable((EnableCap)enableFlag);
        }

        public void Disable(EnableFlag enableFlag)
        {
            GL.Disable((EnableCap)enableFlag);
        }

        public void ClearBuffers()
        {
            var clearFlag = ClearFlag.None;

            if (ColorBits != 0)
            {
                clearFlag = clearFlag.AddFlag(ClearFlag.ColorBuffer);
            }

            if (DepthBits != 0)
            {
                clearFlag = clearFlag.AddFlag(ClearFlag.DepthBuffer);
            }

            if (StencilBits != 0)
            {
                clearFlag = clearFlag.AddFlag(ClearFlag.StencilBuffer);
            }

            ClearBuffers(clearFlag);
        }

        public void ClearBuffers(ClearFlag clearFlag)
        {
            GL.Clear((ClearBufferMask)clearFlag);
        }

        public void ClearColor(float r, float g, float b, float a = 1f)
        {
            _clearColor.R = r;
            _clearColor.G = g;
            _clearColor.B = b;
            _clearColor.A = a;

            GL.ClearColor(_clearColor);
        }

        public void ClearColor(int r, int g, int b, int a = 255)
        {
            ClearColor(Utility.ToUnitSingle(r), Utility.ToUnitSingle(g), Utility.ToUnitSingle(b), Utility.ToUnitSingle(a));
        }


        protected void InsertRenderChunk(Geometry geometry)
        {
            if (!VertexBuffer.CanWriteElements(geometry.VertexCount) || _renderChunkCount + 1 >= _maxRenderChunks)
            {
                Flush();
            }

            var shader = geometry.Shader();

            if (shader == null)
            {
                shader = QueryStandardShader(geometry.VertexFlags, VertexBuffer);
            }

            _renderChunks[_renderChunkCount].Shader = shader;
            _renderChunks[_renderChunkCount].VertexBuffer = VertexBuffer;
            _renderChunks[_renderChunkCount].ViewMatrix = ActiveCamera.ViewMatrix();
            _renderChunks[_renderChunkCount].ProjectionMatrix = ActiveCamera.ProjectionMatrix();
            _renderChunks[_renderChunkCount].ModelMatrix = geometry.ModelMatrix();
            _renderChunks[_renderChunkCount].GeometryType = geometry.GeometryType;
            _renderChunks[_renderChunkCount].ElementIndex = VertexBuffer.UsedElements();
            _renderChunks[_renderChunkCount].ElementCount = geometry.VertexCount;
            _renderChunks[_renderChunkCount].Texture0 = geometry._texture0;
            _renderChunks[_renderChunkCount].Texture1 = geometry._texture1;
            _renderChunks[_renderChunkCount].Texture2 = geometry._texture2;
            _renderChunks[_renderChunkCount].Texture3 = geometry._texture3;

            _renderChunks[_renderChunkCount].VertexFlag = geometry.VertexFlags;

            _renderChunkCount++;
        }

        //protected void InsertRenderChunk(GeometryType geometryType, int elementCount)
        //{
        //    if (!_vertexBuffer.CanWriteElements(elementCount) || _renderChunkCount + 1 >= _maxRenderChunks)
        //    {
        //        Flush();
        //    }

        //    _renderChunks[_renderChunkCount].Shader = QueryShaderFor(geometry);
        //    _renderChunks[_renderChunkCount].VertexBuffer = _vertexBuffer;
        //    _renderChunks[_renderChunkCount].ViewMatrix = CurrentCamera.ViewMatrix();
        //    _renderChunks[_renderChunkCount].ProjectionMatrix = CurrentCamera.ProjectionMatrix();
        //    _renderChunks[_renderChunkCount].ModelMatrix = CurrentModelMatrix;
        //    _renderChunks[_renderChunkCount].GeometryType = geometryType;
        //    _renderChunks[_renderChunkCount].ElementIndex = _vertexBuffer.UsedElements();
        //    _renderChunks[_renderChunkCount].ElementCount = elementCount;

        //    _renderChunkCount++;
        //}

        private void TesselateGeometry()
        {
            if (_geometryCount == 0)
            {
                return;
            }

            // todo
        }

        private void GeometryToRenderChunks()
        {
            GeometryToRenderChunks(_geometry, _geometryCount);

            GeometryToRenderChunks(_tesselatedGeometry, _tesselatedGeometryCount);
        }

        private void GeometryToRenderChunks(Geometry[] geometry, int count)
        {
            if (count == 0)
            {
                return;
            }

            for (var i = 0; i < count; i++)
            {
                var g = geometry[i];

                InsertRenderChunk(g);

                WriteToVertexBuffer(g);
            }
        }

        private void WriteToVertexBuffer(Geometry geometry)
        {
            if (geometry is Geometry.QuadGeometry)
            {
                // remove this ?
                WriteQuadGeometryToVertexBuffer((Geometry.QuadGeometry)geometry);

                return;
            }

            VertexBuffer.Bind();

            foreach (var v in geometry.Vertices())
            {
                VertexBuffer.Write(v._position);

                if (geometry.VertexFlags.HasFlag(VertexFlag.Normal0) && VertexBuffer.VertexFlags(true).HasFlag(VertexFlag.Normal0))
                {
                    VertexBuffer.Write(v._normal);
                }

                VertexBuffer.Write(v._color);

                if (geometry.VertexFlags.HasFlag(VertexFlag.Uv0) && VertexBuffer.VertexFlags(true).HasFlag(VertexFlag.Uv0))
                {
                    VertexBuffer.Write(v._uv0);
                }

                if (geometry.VertexFlags.HasFlag(VertexFlag.Uv1) && VertexBuffer.VertexFlags(true).HasFlag(VertexFlag.Uv1))
                {
                    VertexBuffer.Write(v._uv1);
                }

                if (geometry.VertexFlags.HasFlag(VertexFlag.Uv2) && VertexBuffer.VertexFlags(true).HasFlag(VertexFlag.Uv2))
                {
                    VertexBuffer.Write(v._uv2);
                }

                if (geometry.VertexFlags.HasFlag(VertexFlag.Uv3) && VertexBuffer.VertexFlags(true).HasFlag(VertexFlag.Uv3))
                {
                    VertexBuffer.Write(v._uv3);
                }
            }
        }

        private void WriteQuadGeometryToVertexBuffer(Geometry.QuadGeometry geometry)
        {
            VertexBuffer.Bind();

            foreach (var v in geometry.Vertices())
            {
                VertexBuffer.Write(v._position);

                if (geometry.VertexFlags.HasFlag(VertexFlag.Normal0) && VertexBuffer.VertexFlags(true).HasFlag(VertexFlag.Normal0))
                {
                    VertexBuffer.Write(geometry._normal);
                }

                VertexBuffer.Write(geometry._color);

                if (geometry.VertexFlags.HasFlag(VertexFlag.Uv0) && VertexBuffer.VertexFlags(true).HasFlag(VertexFlag.Uv0))
                {
                    VertexBuffer.Write(v._uv0);
                }

                if (geometry.VertexFlags.HasFlag(VertexFlag.Uv1) && VertexBuffer.VertexFlags(true).HasFlag(VertexFlag.Uv1))
                {
                    VertexBuffer.Write(v._uv1);
                }

                if (geometry.VertexFlags.HasFlag(VertexFlag.Uv2) && VertexBuffer.VertexFlags(true).HasFlag(VertexFlag.Uv2))
                {
                    VertexBuffer.Write(v._uv2);
                }

                if (geometry.VertexFlags.HasFlag(VertexFlag.Uv3) && VertexBuffer.VertexFlags(true).HasFlag(VertexFlag.Uv3))
                {
                    VertexBuffer.Write(v._uv3);
                }
            }
        }

        #region Geometry Functions

        public Geometry.QuadGeometry Quad()
        {
            var g = new Geometry.QuadGeometry(ActiveShader(), ModelMatrix);

            AddGeometry(g);

            return g;
        }

        public Geometry CreateGeometry(GeometryType geometryType)
        {
            return CreateGeometry(geometryType, ActiveShader(), ModelMatrix);
        }

        public Geometry CreateGeometry(GeometryType geometryType, Shader shader, Matrix4 modelMatrix)
        {
            return CreateGeometry(geometryType, Geometry.DefaultVertexCount, shader, modelMatrix);
        }

        public Geometry CreateGeometry(GeometryType geometryType, int vertexCount, Shader shader, Matrix4 modelMatrix)
        {
            if (shader == null)
            {
                shader = QueryStandardShader(VertexBuffer);
            }

            var g = new Geometry(geometryType, vertexCount, shader, modelMatrix);

            AddGeometry(g);

            return g;
        }

        public void AddGeometry(Geometry geometry)
        {
            if (_geometryCount + 1 >= _maxGeometryCount)
            {
                throw new InvalidOperationException($"AddGeometry(Geometry geometry) failed. _maxGeometryCount had been reached: {_geometryCount}");
            }

            _geometry[_geometryCount++] = geometry;
        }

        protected void ResetGeometry()
        {
            for (var i = 0; i < _geometryCount; i++)
            {
                _geometry[i].Dispose();
                _geometry[i] = null;
            }

            _geometryCount = 0;
        }

        protected void ResetTesselatedGeometry()
        {
            for (var i = 0; i < _tesselatedGeometryCount; i++)
            {
                _tesselatedGeometry[i].Dispose();
                _tesselatedGeometry[i] = null;
            }

            _tesselatedGeometryCount = 0;
        }

        #endregion Geometry Functions

        #region Shape Functions

        //#region Rectangle

        //public void Rectangle(float x, float y, float width, float height)
        //{
        //    InsertRenderChunk(GeometryType.TriangleStrip, 4);

        //    _vertexBuffer.Bind();

        //    var halfWidth = width * 0.5f;
        //    var halfHeight = height * 0.5f;

        //    x -= halfWidth;
        //    y -= halfHeight;

        //    _vertexBuffer.Write(x + width, y + height, 0f);           // top right
        //    _vertexBuffer.Write(_currentFillColor);

        //    _vertexBuffer.Write(x, y + height, 0f);                   // top left
        //    _vertexBuffer.Write(_currentFillColor);

        //    _vertexBuffer.Write(x + width, y, 0f);                    // bottom right
        //    _vertexBuffer.Write(_currentFillColor);

        //    _vertexBuffer.Write(x, y, 0f);                            // bottom left
        //    _vertexBuffer.Write(_currentFillColor);
        //}

        //#endregion Rectangle

        //#region Quad

        //public void Quad(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        //{
        //    InsertRenderChunk(GeometryType.TriangleStrip, 4);

        //    _vertexBuffer.Bind();

        //    _vertexBuffer.Write(x1, y1, 0f);
        //    _vertexBuffer.Write(_currentFillColor);

        //    _vertexBuffer.Write(x2, y2, 0f);
        //    _vertexBuffer.Write(_currentFillColor);

        //    _vertexBuffer.Write(x3, y3, 0f);
        //    _vertexBuffer.Write(_currentFillColor);

        //    _vertexBuffer.Write(x4, y4, 0f);
        //    _vertexBuffer.Write(_currentFillColor);
        //}

        //#endregion Quad

        //#region Line Functions

        //public void Lines(params float[] coordinates)
        //{
        //    InsertRenderChunk(GeometryType.Lines, 4);

        //    _vertexBuffer.Bind();

        //    for (var i = 0; i < coordinates.Length;)
        //    {
        //        _vertexBuffer.Write(coordinates[i++], coordinates[i++], 0f);
        //        _vertexBuffer.Write(_currentFillColor);
        //    }
        //}

        //public void LineLoop(params float[] coordinates)
        //{
        //    InsertRenderChunk(GeometryType.LineLoop, 4);

        //    _vertexBuffer.Bind();

        //    for (var i = 0; i < coordinates.Length;)
        //    {
        //        _vertexBuffer.Write(coordinates[i++], coordinates[i++], 0f);
        //        _vertexBuffer.Write(_currentFillColor);
        //    }
        //}

        //public void LineStrip(params float[] coordinates)
        //{
        //    InsertRenderChunk(GeometryType.LineStrip, 4);

        //    _vertexBuffer.Bind();

        //    for (var i = 0; i < coordinates.Length;)
        //    {
        //        _vertexBuffer.Write(coordinates[i++], coordinates[i++], 0f);
        //        _vertexBuffer.Write(_currentFillColor);
        //    }
        //}

        //#endregion Line Functions

        #endregion Shape Functions

        #region Fill Functions

        ///// <summary>
        ///// default == Color4.OrangeRed
        ///// </summary>
        //private Color4 _currentFillColor = Color4.Black;

        //public void Fill(int grey, int a = 255)
        //{
        //    var g = Utility.ToUnitSingle(grey);

        //    Fill(g, g, g, Utility.ToUnitSingle(a));
        //}

        //public void Fill(float grey, float a = 1f)
        //{
        //    Fill(grey, grey, grey, a);
        //}

        //public void Fill(int r, int g, int b, int a = 255)
        //{
        //    Fill(Utility.ToUnitSingle(r), Utility.ToUnitSingle(g), Utility.ToUnitSingle(b), Utility.ToUnitSingle(a));
        //}

        //public void Fill(float r, float g, float b, float a = 1f)
        //{
        //    _currentFillColor.R = r;
        //    _currentFillColor.G = g;
        //    _currentFillColor.B = b;
        //    _currentFillColor.A = a;
        //}

        //public void Fill(Color4 color)
        //{
        //    Fill(color.R, color.G, color.B, color.A);
        //}

        #endregion Fill Functions    

        #region Dispose

        public virtual void Dispose()
        {
            if(_frameBuffer != null)
            {
                _frameBuffer.Dispose();
                _frameBuffer = null;
            }

            if (VertexBuffer != null)
            {
                VertexBuffer.Dispose();
                VertexBuffer = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion Dispose
    }
}

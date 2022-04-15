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
    public abstract class AbstractSketch20220410 : ISketch
    {
        #region Fields and Properties

        private GameWindow _internalWindow;
        private bool disposedValue;

        private const float DefaultSketchWidth = 640f;
        private const float DefaultSketchHeight = 640f;
        public float SketchWidth { get; private set; } = DefaultSketchWidth;
        public float SketchHeight { get; private set; } = DefaultSketchHeight;
        public Dimensions Dimension { get; private set; } = Dimensions.Two;

        public int Samples { get; set; } = 8;

        //public Vector3 ViewPosition = new Vector3(0f, 0f, -1f);
        protected NearFarScrollCamera20220410 Camera = new NearFarScrollCamera20220410 { Near = 1f, Far = 1000f, Fov = 45f };

        public double ElapsedTime { get; private set; }


        public int WindowWidth { get { return _internalWindow.Size.X; } }
        public int WindowHeight { get { return _internalWindow.Size.Y; } }

        public int FrameCount { get; private set; }

        public int CurrentFrameBufferHandle = 0;

        public event Action LoadAction;
        public event Action UnLoadAction;
        public event Action RenderAction;
        public event Action UpdateAction;
        public event Action<Vector2i> ResizeAction;
        public event Action<Vector2> MouseWheelAction;

        private Matrix4 _modelMatrix = Matrix4.Identity;
        private Matrix4 _viewMatrix = Matrix4.Identity;
        private Matrix4 _originMatrix = Matrix4.Identity;

        private const int _maxModelMatrixStackCount = 64;
        private int _currentModelMatrixStackIndex = 0;
        private Matrix4?[] ModelMatrixStack = new Matrix4?[_maxModelMatrixStackCount];

        protected Shader20220413 Shader { get; set; }

        /// <summary>
        /// default == Color4.White
        /// </summary>
        protected Color4 _clearColor = Color4.White;

        /// <summary>
        /// default == Color4.OrangeRed
        /// </summary>
        protected Color4 _fillColor = Color4.Black;

        /// <summary>
        /// default == Color4.Black
        /// </summary>
        protected Color4 _strokeColor = Color4.Black;

        protected Matrix4 ModelMatrix
        {
            get
            {
                return _modelMatrix;
            }
            private set
            {
                _modelMatrix = value;
            }
        }

        protected Matrix4 ViewMatrix 
        { 
            get 
            { 
                return _viewMatrix; 
            }
            set 
            { 
                _viewMatrix = _originMatrix * value; 
            }
        }

        protected Matrix4 ProjectionMatrix;

        private SketchOrigin _sketchOrigin;

        public SketchOrigin SketchOrigin
        {
            get 
            { 
                return _sketchOrigin; 
            }
            set
            {
                _sketchOrigin = value;

                _modelMatrix = CreateModelMatrix();
                _originMatrix = CreateOriginMatrix();

            }
        }

        protected KeyboardState KeyboardState { get { return _internalWindow.KeyboardState; } }
        protected MouseState MouseState { get { return _internalWindow.MouseState; } }

        protected VertexBuffer20220413 PositionColorVertexBuffer { get; private set; }

        public bool _saveFrame { get; set; }

        /// <summary>
        /// default == 256
        /// </summary>
        private int _maxRenderChunks = 256;

        private int _renderChunkCount;

        private RenderChunk20220410[] _renderChunks;

        #endregion Fields and Properties

        #region Constructors

        public AbstractSketch20220410()
            : this(DefaultSketchWidth, DefaultSketchHeight)
        {

        }

        public AbstractSketch20220410(float sketchWidth, float sketchHeight)
        {
            SketchWidth = sketchWidth;
            SketchHeight = sketchHeight;

            var title = GetSketchName();

            var windowSize = CalculateWindowSize(sketchWidth, sketchHeight);

            var nativeWindowSettings = new NativeWindowSettings
            {
                Size = windowSize,
                Title = title,
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,
                NumberOfSamples = Samples,
            };

            _internalWindow = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);

            _internalWindow.Load += LoadInternal;
            _internalWindow.Unload += UnLoadInternal;
            _internalWindow.UpdateFrame += UpdateFrameInternal;
            _internalWindow.RenderFrame += RenderFrameInternal;
            _internalWindow.Resize += ResizeInternal;
            _internalWindow.MouseWheel += MouseWheelInternal;
        }

        #endregion Constructors

        #region Matrix Stuff

        protected void PushMatrix()
        {
            if (_currentModelMatrixStackIndex + 1 >= _maxModelMatrixStackCount)
            {
                throw new InvalidOperationException($"PushMatrix() failed. _maxModelMatrixStackCount has been reached: {_currentModelMatrixStackIndex}");
            }

            ModelMatrixStack[_currentModelMatrixStackIndex++] = _modelMatrix.Copy();
        }

        protected void PopMatrix()
        {
            if(_currentModelMatrixStackIndex == 0)
            {
                throw new InvalidOperationException($"PopMatrix() failed. _currentModelMatrixStackIndex is 0");
            }

            ModelMatrixStack[_currentModelMatrixStackIndex--] = null;
            _modelMatrix = ModelMatrixStack[_currentModelMatrixStackIndex].Value.Copy();
        }

        private Matrix4 CreateModelMatrix()
        {
            switch (_sketchOrigin)
            {
                case SketchOrigin.BottomLeft:
                case SketchOrigin.Center:
                    return Matrix4.Identity;
                case SketchOrigin.TopLeft:
                    return Matrix4.Identity;
                default:
                    throw new NotImplementedException($"CreateOriginTransformMatrix() failed. Found unknown SketchOrigin: {_sketchOrigin}");
            }
        }

        private Matrix4 CreateOriginMatrix()
        {
            switch (_sketchOrigin)
            {
                case SketchOrigin.Center:
                    return Matrix4.Identity;
                case SketchOrigin.TopLeft:
                    return Matrix4.CreateTranslation(-(SketchWidth / 2), (SketchHeight / 2), 0);
                case SketchOrigin.BottomLeft:
                    return Matrix4.CreateTranslation(-(SketchWidth / 2), -(SketchHeight / 2), 0);
                default:
                    throw new NotImplementedException($"CreateOriginTransformMatrix() failed. Found unknown SketchOrigin: {_sketchOrigin}");
            }
        }

        private Matrix4 CreateProjectionMatrix()
        {
            if(Dimension == Dimensions.Two)
            {
                return Matrix4.CreateOrthographic(SketchWidth, SketchHeight, Camera.Near, Camera.Far);
            }
            //return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.Fov), _internalWindow.Size.X / (float)_internalWindow.Size.Y, Camera.Near, Camera.Far);
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.Fov), SketchWidth / SketchHeight, Camera.Near, Camera.Far);
        }

        public static Vector2 Project(Vector3 point, ref Matrix4 world, ref Matrix4 projection)
        {
            Matrix4 transform;
            
            Matrix4.Mult(in projection, in world, out transform);

            return Project(point, ref transform);
        }

        public static Vector2 Project(Vector3 point, ref Matrix4 transform)
        {
            int[] viewport = new int[4];

            GL.GetInteger(GetPName.Viewport, viewport);

            Vector4 pos = new Vector4(point, 1f) * transform;
            
            pos /= pos.W;
            pos.Y = -pos.Y;
            
            Vector2 screenSize = new Vector2(viewport[2], viewport[3]);
            Vector2 screenCenter = new Vector2(viewport[0], viewport[1]) + screenSize / 2f;
            
            return screenCenter + pos.Xy * screenSize / 2f;
        }

        private static System.Drawing.Rectangle GetViewport()
        {
            int[] viewport = new int[4];
            
            GL.GetInteger(GetPName.Viewport, viewport);
            
            return new System.Drawing.Rectangle(viewport[0], viewport[1], viewport[2], viewport[3]);
        }

        //protected Vector2 Project(Vector3 v)
        //{
        //    Matrix4 projection = new Matrix4();
        //    Matrix4 modelView = new Matrix4();
        //    Vector4 viewPort = new Vector4();

        //    GL.GetFloat(GetPName.ModelviewMatrix, out model);
        //    GL.GetFloat(GetPName.ProjectionMatrix, out proj);
        //    GL.GetFloat(GetPName.Viewport, view);

        //    Matrix4.Transpose(ref model, out model);
        //    Matrix4.Transpose(ref proj, out proj);

        //    Vector4 posa = new Vector4(0.0f, s.Position.Y, 1.0f, s.Position.X);
        //    Vector4 posb = new Vector4(s.Position.Y, 1.0f, s.Position.X, 0.0f);
        //    Vector4 posc = new Vector4(1.0f, s.Position.X, 0.0f, s.Position.Y);

        //    Vector4 one = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        //    Matrix4 posv = new Matrix4(pos, posa, posb, posc);

        //    Matrix4 ProjPos = Matrix4.Mult(Matrix4.Mult(proj, model), posv);
        //    Matrix4.Transpose(ref ProjPos, out ProjPos);

        //    return = new Vector2(
        //       (0 + (this.glc.Width * (ProjPos.Column0.X / ProjPos.Column0.W + 1.0f)) - (this.glc.Width / 2.0f)),
        //       (0 + (this.glc.Height * (ProjPos.Column0.Y / ProjPos.Column0.W + 1.0f)) - (this.glc.Height / 2.0f))
        //    );
        //}

        #endregion Matrix Stuff

        protected void Fill(int r, int g, int b, int a = 255)
        {
            Fill(Utility.ToUnitSingle(r), Utility.ToUnitSingle(g), Utility.ToUnitSingle(b), Utility.ToUnitSingle(a));
        }

        protected void Fill(float r, float g, float b, float a = 1f)
        {
            _fillColor.R = r;
            _fillColor.G = g;
            _fillColor.B = b;
            _fillColor.A = a;
        }

        protected void Fill(Color4 color)
        {
            Fill(color.R, color.G, color.B, color.A);
        }

        protected void Stroke(int r, int g, int b, int a = 255)
        {
            Stroke(Utility.ToUnitSingle(r), Utility.ToUnitSingle(g), Utility.ToUnitSingle(b), Utility.ToUnitSingle(a));
        }

        protected void Stroke(float r, float g, float b, float a = 1f)
        {
            _strokeColor.R = r;
            _strokeColor.G = g;
            _strokeColor.B = b;
            _strokeColor.A = a;
        }

        protected void QueryGraphicsCardCapabilities()
        {
            GL.GetInteger(GetPName.MajorVersion, out int majorVersion);
            GL.GetInteger(GetPName.MinorVersion, out int minorVersion);
            Diagnostics.Log($"OpenGL version: {majorVersion}.{minorVersion}");

            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Diagnostics.Log($"Maximum number of vertex attributes supported: {maxAttributeCount}");
        }

        private void MouseWheelInternal(MouseWheelEventArgs a)
        {
            //ViewPosition.Z = a.Offset;

            if(MouseWheelAction != null)
            {
                MouseWheelAction(a.Offset);
            }
        }

        private readonly int[] AvailableSizes = { 3000, 2400, 2000, 1600, 1200, 1000, 800, 640, 480 };

        private Vector2i CalculateWindowSize(float frustumWidth, float frustumHeight)
        {
            var primaryMonitor = Monitors.GetPrimaryMonitor();

            var targetWindowWidth = AvailableSizes.First(o => o < primaryMonitor.HorizontalResolution);
            var targetWindowHeight = AvailableSizes.First(o => o < primaryMonitor.VerticalResolution);

            var windowWidth = (int)(targetWindowHeight * frustumWidth / frustumHeight);
            var windowHeight = targetWindowHeight;

            if (windowWidth > targetWindowWidth || windowHeight > targetWindowWidth)
            {
                windowWidth = targetWindowWidth;
                windowHeight = (int)(targetWindowWidth * frustumHeight / frustumWidth);
            }

            return new Vector2i(windowWidth, windowHeight);
        }

        public void Run()
        {
            _internalWindow.Run();
        }

        private void LoadInternal()
        {
            _renderChunks = new RenderChunk20220410[_maxRenderChunks];

            for(var i = 0; i < _maxRenderChunks; i++)
            {
                _renderChunks[i] = new RenderChunk20220410();
            }

            _originMatrix = CreateOriginMatrix();

            Camera.Position = CalculateInitialViewPosition();
            Camera.Near = 1f;
            Camera.Far = Dimension == Dimensions.Two ? 100f : 10000f;
            Camera.Fov = Dimension == Dimensions.Two ? 0f : 45f;

            ViewMatrix = Camera.ViewMatrix();

            ProjectionMatrix = CreateProjectionMatrix();

            PositionColorVertexBuffer = new VertexBuffer20220413(
                1024 * 1024,
                new[] {
                    new VertexBufferLayout20220413.Part { Identifier = VertexBufferLayout20220413.PartIdentifier.Position0, Type = VertexBufferLayout20220413.ElementType.Single, Count = 3 },
                    new VertexBufferLayout20220413.Part { Identifier = VertexBufferLayout20220413.PartIdentifier.Color0, Type = VertexBufferLayout20220413.ElementType.Single, Count = 4 },
                },
                VertexBufferUsage.DynamicDraw);

            EnableDefaultOpenGlStates();

            LoadAction();
        }

        private void EnableDefaultOpenGlStates()
        {
            if(Dimension == Dimensions.Two)
            {
                GL.Disable(EnableCap.DepthTest);
            }
            else
            {
                GL.Enable(EnableCap.DepthTest);
            }
            GL.Enable(EnableCap.Multisample);
        }

        private void UpdateFrameInternal(FrameEventArgs e)
        {
            ElapsedTime = e.Time;
            FrameCount++;

            ClearStatesAndCounters();

            if (_internalWindow.KeyboardState.IsKeyDown(Keys.Escape))
            {
                _internalWindow.Close();
            }

            if (_internalWindow.KeyboardState.IsKeyDown(Keys.S))
            {
                _saveFrame = true;
            }

            if (_internalWindow.MouseState.Scroll != _internalWindow.MouseState.PreviousScroll)
            {
                Camera.Scroll(_internalWindow.MouseState.ScrollDelta.Y, ElapsedTime);

                //Camera.Position.Z = Camera.Position.Z + (float)(_internalWindow.MouseState.ScrollDelta.Y * 10 * e.Time);
            }

            ModelMatrix = CreateModelMatrix();
            ViewMatrix = Camera.ViewMatrix();

            UpdateAction();
        }

        private void ClearStatesAndCounters()
        {
            PositionColorVertexBuffer.Clear();
            _renderChunkCount = 0;
        }

        private void RenderFrameInternal(FrameEventArgs e)
        {
            RenderFramePreInternal(e);

            RenderAction();

            RenderChunks();

            _internalWindow.SwapBuffers();

            RenderFramePostInternal(e);
        }

        private void RenderChunks()
        {
            for (var c = 0; c < _renderChunkCount; c++)
            {
                var renderChunk = _renderChunks[c];

                renderChunk.Shader.Use();

                Shader.Uniform("uViewMatrix", ViewMatrix);
                Shader.Uniform("uProjectionMatrix", ProjectionMatrix);
                var m = renderChunk.ModelMatrix;
                AdjustForOrigin(ref m);
                Shader.Uniform("uModelMatrix", m);

                renderChunk.VertexBuffer.Bind();
                renderChunk.VertexBuffer.EnableElements();

                GL.DrawArrays((PrimitiveType)renderChunk.GeometryType, renderChunk.ElementIndex, renderChunk.ElementCount);
            }
        }

        private void RenderFramePreInternal(FrameEventArgs e)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFrameBufferHandle);
        }

        private void RenderFramePostInternal(FrameEventArgs e)
        {
            if(_saveFrame)
            {
                _saveFrame = false;

                SaveFrame(CurrentFrameBufferHandle);
            }
        }

        private void UnLoadInternal()
        {
            PositionColorVertexBuffer.Dispose();

            UnLoadAction();
        }

        private void ResizeInternal(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, _internalWindow.Size.X, _internalWindow.Size.Y);

            ProjectionMatrix = CreateProjectionMatrix();

            if(ResizeAction != null)
            {
                ResizeAction(_internalWindow.Size);
            }
        }

        private void SaveFrame(int frameBufferHandle = 0)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferHandle);

            GL.Flush();

            // see https://gist.github.com/WernerWenz/67d6f3ebd0309498ed89bcff6c1889a7

            using (var bmp = new Bitmap((int)SketchWidth, (int)SketchHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                var name = GetSketchName() + DateTime.Now.ToString(".yyyyMMdd.HHmmss.ffff") + ".png";

                var mem = bmp.LockBits(new System.Drawing.Rectangle(0, 0, (int)SketchWidth, (int)SketchHeight), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.PixelStore(PixelStoreParameter.PackRowLength, mem.Stride / 4);
                GL.ReadPixels(0, 0, (int)SketchWidth, (int)SketchHeight, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, mem.Scan0);
                bmp.UnlockBits(mem);
                bmp.Save(@"saves/" + name, ImageFormat.Png);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private Vector3 CalculateInitialViewPosition()
        {
            if(Dimension == Dimensions.Two)
            {
                return new Vector3(0.0f, 0.0f, 10f);
            }

            // calculate the z-axis position so that the provided sketch size fits into the initial view
            var a = (180f - Camera.Fov) * 0.5f;
            var d = (float)Math.Tan(MathHelper.DegreesToRadians(a)) * (SketchWidth * 0.5f);

            return new Vector3(0.0f, 0.0f, d);
        }

        protected void Clear(ClearFlag clearFlag)
        {
            switch(clearFlag)
            {
                case ClearFlag.None:
                    break;
                case ClearFlag.ColorBuffer:
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                    break;
                default:
                    throw new NotImplementedException("Clear(ClearFlag) failed: found an unhandled ClearFlag.");
            }
        }

        protected void ClearColor(float r, float g, float b, float a = 1f)
        {
            _clearColor.R = r;
            _clearColor.G = g;
            _clearColor.B = b;
            _clearColor.A = a;

            GL.ClearColor(_clearColor);
        }

        protected void ClearColor(int r, int g, int b, int a = 255)
        {
            ClearColor(Utility.ToUnitSingle(r), Utility.ToUnitSingle(g), Utility.ToUnitSingle(b), Utility.ToUnitSingle(a));
        }

        protected void Rectangle(float x, float y, float width, float height)
        {
            InsertRenderChunk(Shader, PositionColorVertexBuffer, ModelMatrix, GeometryType20220413.TriangleStrip, PositionColorVertexBuffer.UsedElements(), 4);

            PositionColorVertexBuffer.Bind();

            var halfWidth = width * 0.5f;
            var halfHeight = height * 0.5f;

            x -= halfWidth;
            y -= halfHeight;

            PositionColorVertexBuffer.Write(new[] { x + width, y + height, 0f });           // top right
            PositionColorVertexBuffer.Write(_fillColor);

            PositionColorVertexBuffer.Write(new[] { x, y + height, 0f });                   // top left
            PositionColorVertexBuffer.Write(_fillColor);

            PositionColorVertexBuffer.Write(new[] { x + width, y, 0f });                    // bottom right
            PositionColorVertexBuffer.Write(_fillColor);

            PositionColorVertexBuffer.Write(new[] { x, y, 0f });                            // bottom left
            PositionColorVertexBuffer.Write(_fillColor);
        }

        protected void Quad(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            InsertRenderChunk(Shader, PositionColorVertexBuffer, ModelMatrix, GeometryType20220413.TriangleStrip, PositionColorVertexBuffer.UsedElements(), 4);

            PositionColorVertexBuffer.Bind();

            PositionColorVertexBuffer.Write(new[] { x1, y1, 0f });
            PositionColorVertexBuffer.Write(_fillColor);

            PositionColorVertexBuffer.Write(new[] { x2, y2, 0f });
            PositionColorVertexBuffer.Write(_fillColor);

            PositionColorVertexBuffer.Write(new[] { x3, y3, 0f });
            PositionColorVertexBuffer.Write(_fillColor);

            PositionColorVertexBuffer.Write(new[] { x4, y4, 0f });
            PositionColorVertexBuffer.Write(_fillColor);
        }

        protected void Translate(float x, float y)
        {
            _modelMatrix = _modelMatrix * Matrix4.CreateTranslation(x, y, 0f);
        }

        protected void Rotate(float degrees)
        {
            _modelMatrix = _modelMatrix * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(degrees));
        }

        //private float AdjustForOrigin(float y)
        //{
        //    return SketchOrigin == SketchOrigin.TopLeft ? -y : y;
        //}

        private void AdjustForOrigin(ref Matrix4 matrix)
        {
            if(SketchOrigin == SketchOrigin.TopLeft)
            {
                matrix.M42 = -matrix.M42;
            }
        }

        private string GetSketchName()
        {
            var typeInfo = GetType().GetTypeInfo();
            var sketchAttribute = typeInfo.GetCustomAttribute<SketchAttribute>();

            if(sketchAttribute != null && !string.IsNullOrEmpty(sketchAttribute.Name))
            {
                return sketchAttribute.Name;
            }

            return GetType().Name;
        }

        private void InsertRenderChunk(Shader20220413 shader, VertexBuffer20220413 vertexBuffer, Matrix4 modelMatrix, GeometryType20220413 geometryType, int elementIndex, int elementCount)
        {
            if(_renderChunkCount + 1 >= _maxRenderChunks)
            {
                throw new InvalidOperationException($"InsertRenderChunk(RenderChunk chunk) failed. _maxRenderChunks had been reached: {_maxRenderChunks}");
            }

            _renderChunks[_renderChunkCount].Shader = shader;
            _renderChunks[_renderChunkCount].VertexBuffer = vertexBuffer;
            _renderChunks[_renderChunkCount].ModelMatrix = modelMatrix;
            _renderChunks[_renderChunkCount].GeometryType = geometryType;
            _renderChunks[_renderChunkCount].ElementIndex = elementIndex;
            _renderChunks[_renderChunkCount].ElementCount = elementCount;

            _renderChunkCount++;
        }

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (_internalWindow != null)
                    {
                        _internalWindow.Dispose();
                        _internalWindow = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AbstractSketch()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion Dispose
    }
}

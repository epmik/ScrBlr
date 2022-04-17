using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Scrblr.Core
{
    public abstract class AbstractSketch20220317 : ISketch
    {
        private GameWindow _internalWindow;
        private bool disposedValue;

        private const float DefaultSketchWidth = 1.00f;
        private const float DefaultSketchHeight = 1.00f;
        public float SketchWidth { get; private set; } = DefaultSketchWidth;
        public float SketchHeight { get; private set; } = DefaultSketchHeight;
        public Dimensions Dimension { get; private set; } = Dimensions.Two;
        public float ProjectionFov { get; set; } = 45.00f;
        public float ProjectionNear { get; set; } = 0.10f;
        public float ProjectionFar { get; set; } = 100.00f;

        public Vector3 ViewPosition = new Vector3(0f, 0f, -1f);

        public event Action LoadAction;
        public event Action UnLoadAction;
        public event Action RenderAction;
        public event Action UpdateAction;

        //private const int DefaultResolution = 1;

        public AbstractSketch20220317()
            : this(DefaultSketchWidth, DefaultSketchHeight)
        {

        }

        public AbstractSketch20220317(float sketchWidth, float sketchHeight)
        {
            SketchWidth = sketchWidth;
            SketchHeight = sketchHeight;

            var title = GetSketchName();

            var windowSize = CalculateWindowSize(sketchWidth, sketchHeight);

            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = windowSize,
                Title = title,
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,
            };

            _internalWindow = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);

            _internalWindow.Load += InternalLoad;
            _internalWindow.Unload += InternalUnLoad;
            _internalWindow.UpdateFrame += InternalUpdateFrame;
            _internalWindow.RenderFrame += InternalRenderFrame;
            _internalWindow.Resize += InternalResize;
            _internalWindow.MouseWheel += InternalMouseWheel;
        }

        protected void QueryGraphicsCardCapabilities()
        {
            GL.GetInteger(GetPName.MajorVersion, out int majorVersion);
            GL.GetInteger(GetPName.MinorVersion, out int minorVersion);
            Diagnostics.Log($"OpenGL version: {majorVersion}.{minorVersion}");

            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Diagnostics.Log($"Maximum number of vertex attributes supported: {maxAttributeCount}");
        }

        private void InternalMouseWheel(MouseWheelEventArgs a)
        {
            //ViewPosition.Z = a.Offset;
        }

        private Vector2i CalculateWindowSize(float sketchWidth, float sketchHeight)
        {
            const int WindowPadding = 160;

            var primaryMonitor = Monitors.GetPrimaryMonitor();

            var availableHorizontalResolution = (float)(primaryMonitor.HorizontalResolution - WindowPadding);
            var availableVerticalResolution = (float)(primaryMonitor.VerticalResolution - WindowPadding);

            var horizontalFactor = availableHorizontalResolution / sketchWidth;
            var verticalFactor = availableVerticalResolution / sketchHeight;

            if(verticalFactor < horizontalFactor)
            {
                return new Vector2i((int)(availableVerticalResolution * (sketchWidth / sketchHeight)), (int)availableVerticalResolution);
            }

            return new Vector2i((int)availableHorizontalResolution, (int)((float)availableHorizontalResolution * (sketchHeight / sketchWidth)));
        }

        public void Run()
        {
            _internalWindow.Run();
        }

        private void InternalLoad()
        {
            ViewPosition = CalculateInitialViewPosition();

            ViewMatrix = CreateViewMatrix();

            ProjectionMatrix = CreateProjectionMatrix();

            LoadAction();
        }

        private void InternalUpdateFrame(FrameEventArgs e)
        {
            if (_internalWindow.KeyboardState.IsKeyDown(Keys.Escape))
            {
                _internalWindow.Close();
            }

            if(_internalWindow.MouseState.Scroll != _internalWindow.MouseState.PreviousScroll)
            {
                ViewPosition.Z += (float)(_internalWindow.MouseState.ScrollDelta.Y * 10 * e.Time);
            }

            ViewMatrix = CreateViewMatrix();

            UpdateAction();
        }

        private void InternalRenderFrame(FrameEventArgs e)
        {
            RenderAction();

            // OpenTK windows are what's known as "double-buffered". In essence, the window manages two buffers.
            // One is rendered to while the other is currently displayed by the window.
            // This avoids screen tearing, a visual artifact that can happen if the buffer is modified while being displayed.
            // After drawing, call this function to swap the buffers. If you don't, it won't display what you've rendered.
            _internalWindow.SwapBuffers();

        }

        private void InternalUnLoad()
        {
            UnLoadAction();
        }

        private void InternalResize(ResizeEventArgs e)
        {
            // When the window gets resized, we have to call GL.Viewport to resize OpenGL's viewport to match the new size.
            // If we don't, the NDC will no longer be correct.
            GL.Viewport(0, 0, _internalWindow.Size.X, _internalWindow.Size.Y);

            ProjectionMatrix = CreateProjectionMatrix();
        }


        #region Renderer

        private Vector3 CalculateInitialViewPosition()
        {
            // calculate the z-axis position so that the provided sketch size fits into the initial view
            var a = (180f - ProjectionFov) * 0.5f;
            var d = -(float)Math.Tan(MathHelper.DegreesToRadians(a)) * (SketchWidth * 0.5f);

            return new Vector3(0.0f, 0.0f, d);
        }

        private Matrix4 CreateViewMatrix()
        {
            return Matrix4.CreateTranslation(ViewPosition);
        }

        private Matrix4 CreateProjectionMatrix()
        {
            // For the matrix, we use a few parameters.
            //   Field of view. This determines how much the viewport can see at once. 45 is considered the most "realistic" setting, but most video games nowadays use 90
            //   Aspect ratio. This should be set to Width / Height.
            //   Near-clipping. Any vertices closer to the camera than this value will be clipped.
            //   Far-clipping. Any vertices farther away from the camera than this value will be clipped.

            var a = (180f - ProjectionFov) * 0.5f;
            var d = Math.Tan(MathHelper.DegreesToRadians(a)) * ((float)_internalWindow.Size.X * 0.5f);

            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(ProjectionFov), _internalWindow.Size.X / (float)_internalWindow.Size.Y, ProjectionNear, ProjectionFar);

            // return Matrix4.CreateOrthographic(SketchWidth, SketchHeight, 0.1f, 100f);
        }


        //private float[] _staticPositionColor = new float[6 * 1024 * 1024];

        // Then, we create two matrices to hold our view and projection. They're initialized at the bottom of OnLoad.
        // The view matrix is what you might consider the "camera". It represents the current viewport in the window.
        protected Matrix4 ViewMatrix;

        // This represents how the vertices will be projected. It's hard to explain through comments,
        // so check out the web version for a good demonstration of what this does.
        protected Matrix4 ProjectionMatrix;

        #endregion Renderer

        protected void Clear(ClearFlag clearFlag)
        {
            switch(clearFlag)
            {
                case ClearFlag.None:
                    break;
                case ClearFlag.ColorBuffer:
                    // This clears the image, using what you set as GL.ClearColor earlier.
                    // OpenGL provides several different types of data that can be rendered.
                    // You can clear multiple buffers by using multiple bit flags.
                    // However, we only modify the color, so ColorBufferBit is all we need to clear.
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                    break;
                default:
                    throw new NotImplementedException("Clear(ClearFlag) failed: found an unhandled ClearFlag.");
            }
        }

        protected void ClearColor(float r, float g, float b, float a = 1f)
        {
            GL.ClearColor(new Color4(r, g, b, a));
        }

        protected void ClearColor(int r, int g, int b, int a = 255)
        {
            ClearColor(Utility.ToUnitSingle(r), Utility.ToUnitSingle(g), Utility.ToUnitSingle(b), Utility.ToUnitSingle(a));
        }

        protected void PopTransform()
        {
        }

        protected Rectangle Rectangle()
        {
            var r = new Rectangle();

            return r;
        }

        protected void BorderWidth(int width)
        {
        }

        protected void BorderColor(int r, int g, int b, int a = 255)
        {
        }

        protected void Color(int r, int g, int b, int a = 255)
        {
        }

        protected void Translate(float x, float y)
        {
        }

        protected void PushTransform()
        {
            
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

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_internalWindow != null)
                    {
                        _internalWindow.Dispose();
                        _internalWindow = null;
                    }
                }

                disposedValue = true;
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
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

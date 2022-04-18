using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scrblr.Core
{
    public abstract class AbstractSketch2d20220413 : ISketch
    {
        #region Fields and Properties

        private GameWindow _internalWindow;

        private GraphicsContext2d20220413 _graphics;

        public GraphicsContext2d20220413 Graphics => _graphics;

        private const float DefaultWidth = 2f;
        private const float DefaultHeight = 2f;
        public float Width { get; private set; } = DefaultWidth;
        public float Height { get; private set; } = DefaultHeight;
        public Dimensions Dimension { get; private set; } = Dimensions.Two;

        private int _depthBits = GraphicsSettings20220413.DefaultDepthBits;

        /// <summary>
        /// default == 32
        /// </summary>
        public int DepthBits
        {
            get { return _depthBits; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be less then 0.");
                }
                else if (value > 32)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be greater then 32.");
                }
                _depthBits = value;
            }
        }

        private int _stencilBits = GraphicsSettings20220413.DefaultStencilBits;

        /// <summary>
        /// default == 24
        /// </summary>
        public int StencilBits
        {
            get { return _stencilBits; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be less then 0.");
                }
                else if (value > 32)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be greater then 32.");
                }
                _stencilBits = value;
            }
        }

        private int _samples = GraphicsSettings20220413.DefaultSamples;

        /// <summary>
        /// default == 8
        /// </summary>
        public int Samples 
        { 
            get { return _samples; } 
            set 
            {
                if(value < 0)
                {
                    throw new ArgumentOutOfRangeException("Samples failed. Samples cannot be less then 0.");
                }
                _samples = value; 
            } 
        }

        protected ScrollDragCamera Camera;

        public int WindowWidth { get { return _internalWindow != null ? _internalWindow.Size.X : 0; } }

        public int WindowHeight { get { return _internalWindow != null ? _internalWindow.Size.Y : 0; } }

        public int FrameCount { get; private set; }

        #region Action Handlers

        public event Action LoadAction;
        public event Action UnLoadAction;
        public event Action RenderAction;
        public event Action UpdateAction;
        public event Action<ResizeEventArgs> ResizeAction;
        public event Action<MouseWheelEventArgs> MouseWheelAction;
        public event Action<MouseButtonEventArgs> MouseUpAction;
        public event Action<MouseButtonEventArgs> MouseDownAction;
        public event Action<MouseMoveEventArgs> MouseMoveAction;
        public event Action MouseLeaveAction;
        public event Action MouseEnterAction;
        public event Action<KeyboardKeyEventArgs> KeyDownAction;
        public event Action<KeyboardKeyEventArgs> KeyUpAction;

        #endregion Action Handlers

        #region Save Frame Fields and Properties

        private bool _saveNextFrame { get; set; }

        private GraphicsContext2d20220413 _saveFrameGraphicsContext;

        /// <summary>
        /// A scale applied to the saved image. default == 4f
        /// <para>
        /// i.e. if the window is 600 x 800 pixels
        /// a scale will of 1 will result in a 600 x 800 pixels image
        /// a scale will of 2 will result in a 1200 x 1600 pixels image
        /// a scale will of 0.25 will result in a 150 x 200 pixels image
        /// </para>
        /// <para>
        /// Irrespective of the scale, the dimensions of the image can never be less then 1 and never be greater then the texture size limit defined by OpenGl and your graphics device.
        /// And will also be limited by the <see cref="SaveFrameMaxWidth"/> and <see cref="SaveFrameMaxHeight"/> properties.
        /// </para>
        /// </summary>
        public float SaveFrameScale = 12f;

        /// <summary>
        /// default == 4096
        /// </summary>
        public int SaveFrameMaxWidth = 1024 * 4;

        /// <summary>
        /// default == 4096
        /// </summary>
        public int SaveFrameMaxHeight = 1024 * 4;

        #endregion Save Frame Fields and Properties

        #region Keypress/board Fields and Properties

        /// <summary>
        /// default key to close the application. Set to null if you want to dissable this.
        /// </summary>
        protected Keys? CloseKey = Keys.Escape;

        /// <summary>
        /// default key to save a frame a a image. Set to null if you want to dissable this.
        /// </summary>
        protected Keys? SaveFrameKey = Keys.F5;

        #endregion Keypress/board Fields and Properties

        #endregion Fields and Properties

        #region Constructors

        public AbstractSketch2d20220413()
            : this(DefaultWidth, DefaultHeight)
        {

        }

        /// <summary>
        /// <paramref name="width"/> is not the width of the window. This is the width of sketch visible in the frustum and it is irrespective of the window width or resolution.
        /// <paramref name="height"/> is not the height of the window. This is the height of sketch visible in the frustum and it is irrespective of the window height or resolution.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public AbstractSketch2d20220413(float width, float height)
        {
            Width = width;
            Height = height;
        }

        #endregion Constructors

        protected void QueryGraphicsCardCapabilities()
        {
            GL.GetInteger(GetPName.MajorVersion, out int majorVersion);
            GL.GetInteger(GetPName.MinorVersion, out int minorVersion);
            Diagnostics.Log($"OpenGL version: {majorVersion}.{minorVersion}");

            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Diagnostics.Log($"Maximum number of vertex attributes supported: {maxAttributeCount}");

            GL.GetInteger(GetPName.MaxTextureUnits, out int maxTextureUnits);
            Diagnostics.Log($"Maximum number of texture units supported: {maxTextureUnits}");

            GL.GetInteger(GetPName.MaxTextureSize, out int maxTextureSize);
            Diagnostics.Log($"Maximum texture size supported: {maxTextureSize}");

            GL.GetInteger(GetPName.MaxClipDistances, out int maxClipDistances);
            Diagnostics.Log($"Maximum clip distance: {maxClipDistances}");

            GL.GetInteger(GetPName.MaxSamples, out int maxSamples);
            Diagnostics.Log($"Maximum samples: {maxSamples}");
        }

        private void MouseWheelInternal(MouseWheelEventArgs a)
        {
            if (MouseWheelAction != null)
            {
                MouseWheelAction(a);
            }
        }

        private void MouseDownInternal(MouseButtonEventArgs a)
        {
            if (MouseDownAction != null)
            {
                MouseDownAction(a);
            }
        }

        private void MouseUpInternal(MouseButtonEventArgs a)
        {
            if (MouseUpAction != null)
            {
                MouseUpAction(a);
            }
        }

        private void MouseMoveInternal(MouseMoveEventArgs a)
        {
            if (MouseMoveAction != null)
            {
                MouseMoveAction(a);
            }
        }

        private void MouseLeaveInternal()
        {
            if (MouseLeaveAction != null)
            {
                MouseLeaveAction();
            }
        }

        private void MouseEnterInternal()
        {
            if (MouseEnterAction != null)
            {
                MouseEnterAction();
            }
        }

        private void KeyDownInternal(KeyboardKeyEventArgs a)
        {
            if (KeyDownAction != null)
            {
                KeyDownAction(a);
            }
        }

        private void KeyUpInternal(KeyboardKeyEventArgs a)
        {
            if (a.Key == CloseKey)
            {
                _internalWindow.Close();
            }

            if (a.Key == SaveFrameKey)
            {
                _saveNextFrame = true;
            }

            if (KeyUpAction != null)
            {
                KeyUpAction(a);
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
            Initialize();

            _internalWindow.Run();
        }

        private void Initialize()
        {
            if(_internalWindow != null)
            {
                return;
            }

            var title = GetSketchName();

            var windowSize = CalculateWindowSize(Width, Height);

            var nativeWindowSettings = new NativeWindowSettings
            {
                Size = windowSize,
                Title = title,
                Flags = ContextFlags.ForwardCompatible, // This is needed to run on macos
                NumberOfSamples = Samples,
                DepthBits = DepthBits,
                StencilBits = StencilBits,
            };

            _internalWindow = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);

            _internalWindow.Load += LoadInternal;
            _internalWindow.Unload += UnLoadInternal;
            _internalWindow.UpdateFrame += UpdateFrameInternal;
            _internalWindow.RenderFrame += RenderFrameInternal;
            _internalWindow.Resize += ResizeInternal;

            _internalWindow.MouseWheel += MouseWheelInternal;
            _internalWindow.MouseDown += MouseDownInternal;
            _internalWindow.MouseUp += MouseUpInternal;
            _internalWindow.MouseEnter += MouseEnterInternal;
            _internalWindow.MouseLeave += MouseLeaveInternal;

            _internalWindow.KeyDown += KeyDownInternal;
            _internalWindow.KeyUp += KeyUpInternal;

            Camera = new ScrollDragCamera
            {
                Width = Width,
                Height = Height,
                Near = 1f,
                Far = 100f,
                Fov = 45f,
                Position = new Vector3(0, 0, 2),
            };

            _graphics = new GraphicsContext2d20220413(WindowWidth, WindowHeight,  DepthBits, stencilBits: StencilBits, samples: Samples);

            _graphics.CurrentCamera = Camera;
        }

        private void LoadInternal()
        {
            Graphics.Load();

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
            FrameCount++;

            ResetStatesAndCounters();

            if (_internalWindow.MouseState.Scroll != _internalWindow.MouseState.PreviousScroll)
            {
                Camera.Scroll(_internalWindow.MouseState.ScrollDelta.Y, e.Time);
            }

            _graphics.ClearMatrixStack();

            UpdateAction();
        }

        private void ResetStatesAndCounters()
        {
            Graphics.Reset();
        }

        private void RenderFrameInternal(FrameEventArgs e)
        {
            RenderFramePreInternal(e);

            RenderAction();

            Graphics.Flush();

            RenderFramePostInternal(e);
        }

        private void RenderFramePreInternal(FrameEventArgs e)
        {
            if(_saveNextFrame)
            {
                if(_saveFrameGraphicsContext == null)
                {
                    CreateSaveFrameGraphicsContext();

                    _saveFrameGraphicsContext.Bind();
                }
            }
        }

        private void CreateSaveFrameGraphicsContext()
        {
            var width = (int)(WindowWidth * SaveFrameScale);
            var height = (int)(WindowHeight * SaveFrameScale);

            var max = GL.GetInteger(GetPName.MaxTextureSize);
            var maxwidth = Math.Min(max, SaveFrameMaxWidth);
            var maxheight = Math.Min(max, SaveFrameMaxHeight);

            if (width > maxwidth)
            {
                var factor = (float)maxwidth / (float)width;
                width = (int)(width * factor);
                height = (int)(height * factor);
            }

            if (height > maxheight)
            {
                var factor = (float)maxheight / (float)height;
                width = (int)(width * factor);
                height = (int)(height * factor);
            }

            _saveFrameGraphicsContext = new GraphicsContext2d20220413(
                width,
                height,
                Graphics.ColorBits,
                Graphics.DepthBits,
                Graphics.StencilBits,
                Graphics.Samples);

            _saveFrameGraphicsContext.CurrentCamera = Graphics.CurrentCamera;
            _saveFrameGraphicsContext.CurrentShader = Graphics.CurrentShader;
            _saveFrameGraphicsContext.CurrentModelMatrix = Graphics.CurrentModelMatrix;
        }

        private void DisposeSaveFrameGraphicsContext()
        {
            GraphicsContext20220413.Default.Bind();

            if (_saveFrameGraphicsContext != null)
            {
                _saveFrameGraphicsContext.Dispose();

                _saveFrameGraphicsContext = null;
            }
        }

        private void RenderFramePostInternal(FrameEventArgs e)
        {
            if(_saveNextFrame)
            {
                _saveNextFrame = false;

                SaveFrame(_saveFrameGraphicsContext);

                DisposeSaveFrameGraphicsContext();

                Graphics.Bind();

                return;
            }

            _internalWindow.SwapBuffers();
        }

        private void UnLoadInternal()
        {
            if(_saveFrameGraphicsContext != null)
            {
                _saveFrameGraphicsContext.Dispose();
                _saveFrameGraphicsContext = null;
            }

            Graphics.Dispose();

            UnLoadAction();
        }

        private void ResizeInternal(ResizeEventArgs a)
        {
            GL.Viewport(0, 0, _internalWindow.Size.X, _internalWindow.Size.Y);

            if (ResizeAction != null)
            {
                ResizeAction(a);
            }
        }

        private void SaveFrame(GraphicsContext2d20220413 graphics)
        {
            graphics.Bind();

            GL.Flush();

            using (var bitmap = new System.Drawing.Bitmap(graphics.Width, graphics.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                var name = GetSketchName() + DateTime.Now.ToString(".yyyyMMdd.HHmmss.ffff") + ".png";

                var data = bitmap.LockBits(new System.Drawing.Rectangle(
                    0, 0,
                    graphics.Width, graphics.Height), 
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, 
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                
                GL.PixelStore(PixelStoreParameter.PackRowLength, data.Stride / 4);
                
                GL.ReadPixels(
                    0, 0,
                    graphics.Width, graphics.Height, 
                    PixelFormat.Bgra, 
                    PixelType.UnsignedByte, 
                    data.Scan0);
                
                bitmap.UnlockBits(data);

                bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

                bitmap.Save(@"saves/" + name, System.Drawing.Imaging.ImageFormat.Png);
            }
        }


        protected void Translate(float x, float y)
        {
            Translate(x, y, 0f);
        }

        protected void Translate(float x, float y, float z)
        {
            Graphics.CurrentModelMatrix = Graphics.CurrentModelMatrix * Matrix4.CreateTranslation(x, y, z);
        }

        protected void Rotate(float degrees)
        {
            Graphics.CurrentModelMatrix = Graphics.CurrentModelMatrix * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(degrees));
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

        public void Dispose()
        {
            if (_internalWindow != null)
            {
                _internalWindow.Dispose();
                _internalWindow = null;
            }
            GC.SuppressFinalize(this);
        }

        #endregion Dispose
    }
}

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
    public abstract class AbstractSketch : GraphicsSettings, ISketch
    {
        #region Fields and Properties

        private GameWindow _internalWindow;

        private GraphicsContext _graphics;

        public GraphicsContext Graphics => _graphics;

        private const float DefaultFrustumWidth = 2f;
        private const float DefaultFrustumHeight = 2f;
        public float FrustumWidth { get; private set; } = DefaultFrustumWidth;
        public float FrustumHeight { get; private set; } = DefaultFrustumHeight;
        public Dimensions Dimension { get; private set; } = Dimensions.Two;

        protected ScrollDragCamera Camera;

        public int WindowWidth { get { return Width; } }

        public int WindowHeight { get { return Height; } }
        protected KeyboardState KeyboardState { get { return _internalWindow.KeyboardState; } }
        protected MouseState MouseState { get { return _internalWindow.MouseState; } }

        public int FrameCount { get; private set; }
        
        public double ElapsedTime { get; private set; }

        public int FramesPerSecond { get; private set; }

        private long LastFramesPerSecondTimeStamp { get; set; }

        /// <summary>
        /// default == true
        /// </summary>
        protected bool AutoClearBuffers { get; set; } = true;

        #region Public Event Handlers

        // any public events ending with 'Action' will be bound in the Sketch.BindDelegates<TSketch>(TSketch sketch) function
        // the function that will be bound (if found) must have the same name as the event minus 'Action'.
        // i.e.: a function named 'Load' will be bound to the event 'LoadAction'
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

        #endregion Public Event Handlers

        #region Save Frame Fields and Properties

        private bool _saveNextFrame { get; set; }

        private GraphicsContext _saveFrameGraphicsContext;

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

        private readonly Stopwatch _timeStopWatch = new Stopwatch();

        #endregion Fields and Properties

        #region Constructors

        public AbstractSketch()
            : this(DefaultFrustumWidth, DefaultFrustumHeight)
        {

        }

        /// <summary>
        /// <paramref name="width"/> is not the width of the window. This is the width of sketch visible in the frustum and it is irrespective of the window width or resolution.
        /// <paramref name="height"/> is not the height of the window. This is the height of sketch visible in the frustum and it is irrespective of the window height or resolution.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public AbstractSketch(float width, float height)
            : base(0, 0)
        {
            FrustumWidth = width;
            FrustumHeight = height;
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
            MouseWheelAction?.Invoke(a);
        }

        private void MouseDownInternal(MouseButtonEventArgs a)
        {
            MouseDownAction?.Invoke(a);
        }

        private void MouseUpInternal(MouseButtonEventArgs a)
        {
            MouseUpAction?.Invoke(a);
        }

        private void MouseMoveInternal(MouseMoveEventArgs a)
        {
            MouseMoveAction?.Invoke(a);
        }

        private void MouseLeaveInternal()
        {
            MouseLeaveAction?.Invoke();
        }

        private void MouseEnterInternal()
        {
            MouseEnterAction?.Invoke();
        }

        private void KeyDownInternal(KeyboardKeyEventArgs a)
        {
            KeyDownAction?.Invoke(a);
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

            KeyUpAction?.Invoke(a);
        }

        private readonly int[] AvailableSizes = { 3600, 3000, 2400, 2000, 1600, 1200, 1000, 800, 640, 480 };

        private Vector2i CalculateWindowSize(float frustumWidth, float frustumHeight)
        {
            var primaryMonitor = Monitors.GetPrimaryMonitor();

            var targetWindowWidth = AvailableSizes.First(o => o < primaryMonitor.HorizontalResolution);
            var targetWindowHeight = AvailableSizes.First(o => o < primaryMonitor.VerticalResolution);

            var windowWidth = (int)(targetWindowHeight * frustumWidth / frustumHeight);
            var windowHeight = targetWindowHeight;

            if(windowWidth > targetWindowWidth || windowHeight > targetWindowWidth)
            {
                windowWidth = targetWindowWidth;
                windowHeight = (int)(targetWindowWidth * frustumHeight / frustumWidth);
            }

            return new Vector2i(windowWidth, windowHeight);
        }

        public void Run()
        {
            Diagnostics.Log($"Virtual memory: {(Process.GetCurrentProcess().VirtualMemorySize64 / (1024 * 1024))} MB");

            _timeStopWatch.Start();

            Initialize();

            Diagnostics.Log($"Initialize time: {_timeStopWatch.ElapsedMilliseconds} ms.");

            // see https://stackoverflow.com/a/2342090/527843
            GC.Collect();   // The first time, objects are put on the freachable queue and are later finalized.
            GC.WaitForPendingFinalizers();
            GC.Collect();   // Afterwards, they are collectable.

            Diagnostics.Log($"Virtual memory: {(Process.GetCurrentProcess().VirtualMemorySize64 / (1024 * 1024))} MB");

            _internalWindow.Run();

            _timeStopWatch.Stop();

            Diagnostics.Log($"Close time: {_timeStopWatch.ElapsedMilliseconds} ms.");

            Diagnostics.Log($"Virtual memory: {(Process.GetCurrentProcess().VirtualMemorySize64 / (1024 * 1024))} MB");

            GC.Collect();
        }

        public long CurrentTimeStamp() 
        { 
            return _timeStopWatch.ElapsedMilliseconds; 
        }

        private void Initialize()
        {
            if(_internalWindow != null)
            {
                return;
            }

            var title = GetSketchName();

            var windowSize = CalculateWindowSize(FrustumWidth, FrustumHeight);

            Width = windowSize.X;
            Height = windowSize.Y;

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
            _internalWindow.MouseMove += MouseMoveInternal;

            _internalWindow.KeyDown += KeyDownInternal;
            _internalWindow.KeyUp += KeyUpInternal;

            _graphics = new GraphicsContext(WindowWidth, WindowHeight,  DepthBits, stencilBits: StencilBits, samples: Samples);

            Camera = new ScrollDragCamera
            {
                Width = FrustumWidth,
                Height = FrustumHeight,
                Near = 1f,
                Far = 1000f,
                //Position = new Vector3(0, 0, 0),
            };

            AttachCamera(Camera);
        }

        private List<IEventComponent> EventComponents = new List<IEventComponent>();

        protected void AttachCamera(ICamera camera, bool bindEvents = true, bool makeActive = true)
        {
            EventComponents.Add(camera);

            if (bindEvents)
            {
                _internalWindow.UpdateFrame += camera.Update;
                _internalWindow.Resize += camera.Resize;
                _internalWindow.KeyUp += camera.KeyUp;
                _internalWindow.KeyDown += camera.KeyDown;
                _internalWindow.MouseDown += camera.MouseDown;
                _internalWindow.MouseEnter += camera.MouseEnter;
                _internalWindow.MouseLeave += camera.MouseLeave;
                _internalWindow.MouseMove += camera.MouseMove;
                _internalWindow.MouseUp += camera.MouseUp;
                _internalWindow.MouseWheel += camera.MouseWheel;
            }
            // TODO differentiate between attached and active camera's
            _graphics.ActiveCamera(camera);
        }

        private void LoadInternal()
        {
            Graphics.Load();

            EnableDefaultStates();

            LoadAction?.Invoke();
        }

        private void EnableDefaultStates()
        {
            if(Dimension == Dimensions.Two)
            {
                Graphics.Disable(EnableFlag.DepthTest);
            }
            else
            {
                Graphics.Enable(EnableFlag.DepthTest);
            }

            if(Samples > 1)
            {
                Graphics.Enable(EnableFlag.MultiSampling);
            }

            Graphics.Enable(EnableFlag.Blending);

            Graphics.Enable(EnableFlag.CounterClockWiseFace);

            Graphics.Enable(EnableFlag.BackFaceCulling);

            Graphics.Disable(EnableFlag.FrontFaceCulling);
        }

        private void UpdateFrameInternal(FrameEventArgs a)
        {
            PreUpdateFrameInternal(a);

            UpdateAction?.Invoke();

            PostUpdateFrameInternal(a);
        }

        private void PreUpdateFrameInternal(FrameEventArgs a)
        {
            Diagnostics.Log($"Pre frame allocated managed memory - before garbage collection: {(GC.GetTotalMemory(false) / (1024 * 1024))} MB");
            //Diagnostics.Log($"Allocated managed memory - after garbage collection: {(GC.GetTotalMemory(true) / (1024 * 1024))} MB");

            FrameCount++;
            FramesPerSecond++;

            ElapsedTime = a.Time;

            var currentTimeStamp = CurrentTimeStamp();

            if (LastFramesPerSecondTimeStamp + 2000 <= currentTimeStamp)
            {
                Diagnostics.Log($"FPS: {FramesPerSecond}");

                LastFramesPerSecondTimeStamp = currentTimeStamp;
                FramesPerSecond = 0;
            }            

            ResetStatesAndCounters();

            _graphics.ClearMatrixStack();

            foreach(var eventComponent in EventComponents)
            {
                eventComponent.KeyboardState = _internalWindow.KeyboardState;
                eventComponent.MouseState = _internalWindow.MouseState;
                eventComponent.ElapsedTime = ElapsedTime;
            }
        }

        private void PostUpdateFrameInternal(FrameEventArgs e)
        {
        }

        private void ResetStatesAndCounters()
        {
            Graphics.Reset();
        }

        private void RenderFrameInternal(FrameEventArgs e)
        {
            PreRenderFrameInternal(e);

            RenderAction();

            PostRenderFrameInternal(e);
        }

        private void PreRenderFrameInternal(FrameEventArgs e)
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

        private void PostRenderFrameInternal(FrameEventArgs e)
        {
            if (AutoClearBuffers)
            {
                Graphics.ClearBuffers();
            }

            Graphics.Flush();

            if (_saveNextFrame)
            {
                _saveNextFrame = false;

                SaveFrame(_saveFrameGraphicsContext);

                DisposeSaveFrameGraphicsContext();

                Graphics.Bind();

                return;
            }

            _internalWindow.SwapBuffers();

            Diagnostics.Log($"Post frame allocated managed memory - before garbage collection: {(GC.GetTotalMemory(false) / (1024 * 1024))} MB");
        }

        private void UnLoadInternal()
        {
            _saveFrameGraphicsContext?.Dispose();
            _saveFrameGraphicsContext = null;

            Graphics.Dispose();

            UnLoadAction?.Invoke();
        }

        private void ResizeInternal(ResizeEventArgs a)
        {
            GL.Viewport(0, 0, _internalWindow.Size.X, _internalWindow.Size.Y);

            ResizeAction?.Invoke(a);
        }

        private void SaveFrame(GraphicsContext graphics)
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

            _saveFrameGraphicsContext = new GraphicsContext(
                width,
                height,
                Graphics.ColorBits,
                Graphics.DepthBits,
                Graphics.StencilBits,
                1 /*Graphics.Samples*/);    // TODO multi sampling for frame buffer doesn't work

            _saveFrameGraphicsContext.ActiveCamera(Graphics.ActiveCamera());
            _saveFrameGraphicsContext.ModelMatrix(Graphics.ModelMatrix());
        }

        private void DisposeSaveFrameGraphicsContext()
        {
            GraphicsContext.Default.Bind();

            if (_saveFrameGraphicsContext != null)
            {
                _saveFrameGraphicsContext.Dispose();

                _saveFrameGraphicsContext = null;
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

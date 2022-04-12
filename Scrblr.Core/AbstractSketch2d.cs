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
    public abstract class AbstractSketch2d : ISketch
    {
        #region Fields and Properties

        private GameWindow _internalWindow;

        private GraphicsContext2d _graphics;

        public GraphicsContext2d Graphics => _graphics;

        private const float DefaultWidth = 2f;
        private const float DefaultHeight = 2f;
        public float Width { get; private set; } = DefaultWidth;
        public float Height { get; private set; } = DefaultHeight;
        public Dimensions Dimension { get; private set; } = Dimensions.Two;

        private int _samples = 8;

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

        protected NearFarScrollCamera Camera;

        public int WindowWidth { get { return _internalWindow != null ? _internalWindow.Size.X : 0; } }

        public int WindowHeight { get { return _internalWindow != null ? _internalWindow.Size.Y : 0; } }

        public int FrameCount { get; private set; }

        public event Action LoadAction;
        public event Action UnLoadAction;
        public event Action RenderAction;
        public event Action UpdateAction;
        public event Action<Vector2i> ResizeAction;
        public event Action<Vector2> MouseWheelAction;

        /// <summary>
        /// default == Color4.White
        /// </summary>
        protected Color4 _clearColor = Color4.White;

        //protected VertexBuffer PositionColorVertexBuffer { get; private set; }

        public bool _saveFrame { get; set; }

        /// <summary>
        /// default key to close the application. Set to null if you want to dissable this.
        /// </summary>
        protected Keys? CloseKey = Keys.Escape;

        /// <summary>
        /// default key to save a frame a a image. Set to null if you want to dissable this.
        /// </summary>
        protected Keys? SaveFrameKey = Keys.F5;

        #endregion Fields and Properties

        #region Constructors

        public AbstractSketch2d()
            : this(DefaultWidth, DefaultHeight)
        {

        }

        /// <summary>
        /// <paramref name="width"/> is not the width of the window. This is the width of sketch visible in the frustum and it is irrespective of the window width or resolution.
        /// <paramref name="height"/> is not the height of the window. This is the height of sketch visible in the frustum and it is irrespective of the window height or resolution.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public AbstractSketch2d(float width, float height)
        {
            Width = width;
            Height = height;
        }

        #endregion Constructors

        protected void QueryGraphicsCardCapabilities()
        {
            GL.GetInteger(GetPName.MajorVersion, out int majorVersion);
            GL.GetInteger(GetPName.MinorVersion, out int minorVersion);
            Debug.WriteLine($"OpenGL version: {majorVersion}.{minorVersion}");

            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Debug.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");
        }

        private void MouseWheelInternal(MouseWheelEventArgs a)
        {
            if (MouseWheelAction != null)
            {
                MouseWheelAction(a.Offset);
            }
        }

        private readonly int[] AvailableSizes = { 3000, 2400, 2000, 1600, 1200, 1000, 800, 640, 480 };

        private Vector2i CalculateWindowSize(float sketchWidth, float sketchHeight)
        {
            var primaryMonitor = Monitors.GetPrimaryMonitor();

            var targetWindowWidth = AvailableSizes.First(o => o < primaryMonitor.HorizontalResolution);
            var targetWindowHeight = AvailableSizes.First(o => o < primaryMonitor.VerticalResolution);

            if(sketchWidth <= targetWindowWidth && sketchHeight <= targetWindowHeight)
            {
                return new Vector2i((int)sketchWidth, (int)sketchHeight);
            }

            var heightFactor = sketchHeight / sketchWidth;
            var widthFactor = sketchWidth / sketchHeight;



            var windowWidth = 0;
            var windowHeight = 0;

            if (heightFactor < 1f)
            {
                // sketch width is greater then height
                windowWidth = targetWindowWidth;
                windowHeight = (int)(targetWindowWidth * heightFactor);

                if(windowHeight > targetWindowHeight)
                {
                    windowWidth = (int)(targetWindowHeight * widthFactor);
                    windowHeight = targetWindowHeight;
                }
            }
            else
            {
                // sketch width is less or equal to height
                windowWidth = (int)(targetWindowHeight * widthFactor);
                windowHeight = targetWindowHeight;

                if (windowWidth > targetWindowWidth)
                {
                    windowWidth = targetWindowWidth;
                    windowHeight = (int)(targetWindowWidth * heightFactor);
                }
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
            };

            Camera = new NearFarScrollCamera
            {
                Width = Width,
                Height = Height,
                Near = 1f,
                Far = 100f,
                Fov = 45f,
                Position = new Vector3(0, 0, 2),
            };

            _graphics = new GraphicsContext2d();
            _graphics.CurrentCamera = Camera;

            _internalWindow = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);

            _internalWindow.Load += LoadInternal;
            _internalWindow.Unload += UnLoadInternal;
            _internalWindow.UpdateFrame += UpdateFrameInternal;
            _internalWindow.RenderFrame += RenderFrameInternal;
            _internalWindow.Resize += ResizeInternal;
            _internalWindow.MouseWheel += MouseWheelInternal;
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

            ClearStatesAndCounters();

            if (CloseKey != null && _internalWindow.KeyboardState.IsKeyDown(CloseKey.Value))
            {
                _internalWindow.Close();
            }

            if (SaveFrameKey != null && _internalWindow.KeyboardState.IsKeyDown(SaveFrameKey.Value))
            {
                _saveFrame = true;
            }

            if (_internalWindow.MouseState.Scroll != _internalWindow.MouseState.PreviousScroll)
            {
                Camera.Scroll(_internalWindow.MouseState.ScrollDelta.Y, e.Time);
            }

            _graphics.ClearMatrixStack();

            UpdateAction();
        }

        private void ClearStatesAndCounters()
        {
            Graphics.Clear();
        }

        private void RenderFrameInternal(FrameEventArgs e)
        {
            RenderFramePreInternal(e);

            RenderAction();

            Graphics.Flush();

            _internalWindow.SwapBuffers();

            RenderFramePostInternal(e);
        }

        private void RenderFramePreInternal(FrameEventArgs e)
        {
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, PositionColorVertexBuffer.Handle);
        }

        private void RenderFramePostInternal(FrameEventArgs e)
        {
            if(_saveFrame)
            {
                _saveFrame = false;

                SaveFrame(Graphics);
            }
        }

        private void UnLoadInternal()
        {
            Graphics.Dispose();

            UnLoadAction();
        }

        private void ResizeInternal(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, _internalWindow.Size.X, _internalWindow.Size.Y);

            if (ResizeAction != null)
            {
                ResizeAction(_internalWindow.Size);
            }
        }

        private void SaveFrame(GraphicsContext2d graphics)
        {
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferHandle);

            //GL.Flush();

            //// see https://gist.github.com/WernerWenz/67d6f3ebd0309498ed89bcff6c1889a7

            //using (var bmp = new Bitmap((int)SketchWidth, (int)SketchHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            //{
            //    var name = GetSketchName() + DateTime.Now.ToString(".yyyyMMdd.HHmmss.ffff") + ".png";

            //    var mem = bmp.LockBits(new System.Drawing.Rectangle(0, 0, (int)SketchWidth, (int)SketchHeight), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //    GL.PixelStore(PixelStoreParameter.PackRowLength, mem.Stride / 4);
            //    GL.ReadPixels(0, 0, (int)SketchWidth, (int)SketchHeight, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, mem.Scan0);
            //    bmp.UnlockBits(mem);
            //    bmp.Save(@"saves/" + name, ImageFormat.Png);
            //}

            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        protected void Clear(ClearFlag clearFlag)
        {
            switch(clearFlag)
            {
                case ClearFlag.None:
                    break;
                case ClearFlag.Color:
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

        protected void Translate(float x, float y)
        {
            Graphics.CurrentModelMatrix = Graphics.CurrentModelMatrix * Matrix4.CreateTranslation(x, y, 0f);
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

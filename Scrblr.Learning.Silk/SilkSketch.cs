using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Scrblr.Core;
using FontStashSharp;
using Silk.NET.Maths;
using System.Numerics;

namespace Scrblr.Learning
{
    public class SilkSketch : IDisposable
    {
        public static GL GL;

        /// <summary>
        /// default == ProjectionMode.Perspective
        /// </summary>
        public ProjectionMode ProjectionMode { get; set; } = ProjectionMode.Perspective;

        protected IWindow? window;

        protected IInputContext? inputContext;

        protected IKeyboard? Keyboard;

        protected IMouse? Mouse;

        protected static FontRenderer _fontRenderer;
        protected static FontSystem _fontSystem;

        public float Width { get; set; } = 1;
        public float Height { get; set; } = 1;
        public float Near { get; set; } = 0.1f;
        public float Far { get; set; } = 100f;

        /// <summary>
        /// field of view in degrees
        /// <para>
        /// this can be the vertical (default) or horizontal field of view
        /// </para>
        /// <para>
        /// if the window is higher than it's width, then the Fov is considered horizontal
        /// </para>
        /// </summary>
        public float Fov = 45f;

        public int WindowWidth { get; private set; } = 640;
        public int WindowHeight { get; private set; } = 640;

        public double ElapsedTime { get; private set; } = 640;

        private event Action? _loadHandler;
        private event Action? _closingHandler;
        private event Action? _renderHandler;
        private event Action? _updateHandler;
        private event Action<Vector2D<int>>? _resizeHandler;
        private event Action<IKeyboard, Key, int> _keyDownHandler;

        protected Matrix4x4 _projection;

        public SilkSketch()
            : this(1f)
        {
        }

        public SilkSketch(float size)
            : this(size, size)
        {
        }

        public SilkSketch(float width, float height)
        {
            Width = width;
            Height = height;
        }

        ~SilkSketch() => Dispose(false);

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            inputContext?.Dispose();
            window?.Dispose();
        }

        public void Run()
        {
            BindDelegates();

            CreateWindow();

            window?.Run();
        }

        private void CreateFontService()
        {
            _fontRenderer = new FontRenderer();

            //var settings = new FontSystemSettings
            //{
            //    FontResolutionFactor = 2,
            //    KernelWidth = 2,
            //    KernelHeight = 2
            //};

            //_fontSystem = new FontSystem(settings);
            _fontSystem = new FontSystem();
            //_fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/droidsans.ttf"));
            //_fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/Roboto-Black.ttf"));
            _fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/Ubuntu-Regular.ttf"));
            _fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/droidsansjapanese.ttf"));
            _fontSystem.AddFont(File.ReadAllBytes(@".resources/.fonts/symbola-emoji.ttf"));
        }

        private bool BindDelegates()
        {
            //Diagnostics.Log($"Binding delegetes.");

            var sketchType = GetType();

            var eventInfo = sketchType.BaseType.GetEvent("_renderHandler", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var methodInfo = sketchType.GetMethod("Render", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var d = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);

            eventInfo.GetAddMethod(true).Invoke(this, new object[] { d });


            //var eventInfoArray = sketchType.GetEvents(BindingFlags.NonPublic | BindingFlags.Instance);

            //foreach (var eventInfo in eventInfoArray)
            //{
            //    if (!eventInfo.Name.EndsWith("Handler"))
            //    {
            //        continue;
            //    }

            //    var methodName = eventInfo.Name.Substring(0, eventInfo.Name.Length - 6);

            //    if (!BindDelegete(this, sketchType, eventInfo, methodName) && methodName.Equals("Render"))
            //    {
            //        //Diagnostics.Error($"Could not bind the Render delegate. Cannot run and quiting.");

            //        return false;
            //    }
            //}

            return true;
        }

        private bool BindDelegete(SilkSketch sketch, Type sketchType, EventInfo eventInfo, string methodName)
        {
            var existingField = sketchType.BaseType.GetField(eventInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (existingField != null)
            {
                var existingDelegate = existingField.GetValue(sketch) as Delegate;

                if (existingDelegate != null)
                {
                    Diagnostics.Log($"Found existing delegete {existingDelegate.Method.Name} for event: {eventInfo.Name}.");

                    return true;
                }
            }

            var methodInfo = sketchType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                Diagnostics.Log($"Could not find method: {methodName} in {sketchType}.");

                return false;
            }

            var d = Delegate.CreateDelegate(eventInfo.EventHandlerType, sketch, methodInfo);

            eventInfo.GetAddMethod().Invoke(sketch, new object[] { d });

            return true;
        }

        private void CreateWindow()
        {
            var options = Silk.NET.Windowing.WindowOptions.Default;

            options.Size = CalculateWindowSize(Width, Height);
            options.Title = "LearnOpenGL with Silk.NET";
            
            Context.Window = window = Window.Create(options);

            window.Load += _internalLoad;
            window.Render += _internalRender;
            window.Update += _internalUpdate;
            window.Resize += _internalResize;
            window.Closing += _internalClosing;

            Console.WriteLine($"Window size is: {options.Size.X}x{options.Size.Y}");
        }

        private unsafe void _internalLoad()
        {
            Context.GL = GL = GL.GetApi(window);

            CreateFontService();

            inputContext = window.CreateInput();

            for (int i = 0; i < inputContext.Keyboards.Count; i++)
            {
                inputContext.Keyboards[i].KeyDown += _keyDownHandler;
            }

            UpdateProjectionMatrix();

            //var fovRadians = (float)Utility.DegreesToRadians(Fov);

            //float extent = Width * 0.5f;
            //var z = extent / (float)Math.Sin(fovRadians * 0.5f);

            //_viewPosition = new Vector3(0, 0, -(z - Near));
        }

        private unsafe void _internalRender(double elapsedTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _renderHandler?.Invoke();
        }

        private void _internalUpdate(double elapsedTime)
        {
            _updateHandler?.Invoke();
        }

        private unsafe void _internalClosing()
        {
            _closingHandler?.Invoke();

            _fontRenderer.Dispose();
        }

        private void KeyDown(IKeyboard keyboard, Key key, int code)
        {
            switch (key)
            {
                case Key.Escape:
                    window.Close();
                    break;
            }

            _keyDownHandler?.Invoke(keyboard, key, code);
        }

        private void _internalResize(Vector2D<int> size)
        {
            GL.Viewport(0, 0, (uint)size.X, (uint)size.Y);

            _resizeHandler?.Invoke(size);
        }

        private readonly int[] AvailableSizes = { 3840, 3440, 2560, 2160, 2048, 1920, 1600, 1440, 1360, 1280, 1152, 1024, 800, 720, 640, 480, 360 };

        private Vector2D<int> CalculateWindowSize(float canvasWidth, float canvasHeight)
        {
            var primaryMonitor = Silk.NET.Windowing.Monitor.GetMainMonitor(null);

            var targetWindowWidth = AvailableSizes.First(o => o < primaryMonitor.Bounds.Size.X);
            var targetWindowHeight = AvailableSizes.First(o => o < primaryMonitor.Bounds.Size.Y);

            var windowWidth = (int)(targetWindowHeight * canvasWidth / canvasHeight);
            var windowHeight = targetWindowHeight;

            if (windowWidth > targetWindowWidth || windowHeight > targetWindowWidth)
            {
                windowWidth = targetWindowWidth;
                windowHeight = (int)(targetWindowWidth * canvasHeight / canvasWidth);
            }

            return new Vector2D<int>(windowWidth, windowHeight);
        }

        protected void UpdateProjectionMatrix()
        {
            var fovRadians = (float)Utility.DegreesToRadians(Fov);

            var aspectRatioHorizontal = ((float)window.Size.X / (float)window.Size.Y);
            var aspectRatioVertical = ((float)window.Size.Y / (float)window.Size.X);

            switch (ProjectionMode)
            {
                case ProjectionMode.Orthographic:
                    if (aspectRatioHorizontal >= aspectRatioVertical)
                    {
                        _projection = Matrix4x4.CreateOrthographic(Width * aspectRatioHorizontal, Height, Near, Far);
                    }
                    else
                    {
                        _projection = Matrix4x4.CreateOrthographic(Width, Height * aspectRatioVertical, Near, Far);
                    }
                    break;
                case ProjectionMode.Perspective:
                    if (aspectRatioHorizontal >= aspectRatioVertical)
                    {
                        var top = (float)Math.Tan(fovRadians * 0.5f) * Near;
                        var bottom = -top;
                        var right = top * aspectRatioHorizontal;
                        var left = -right;
                        _projection = Matrix4x4.CreatePerspective(right - left, top - bottom, Near, Far);
                    }
                    else
                    {
                        var right = (float)Math.Tan(fovRadians * 0.5f) * Near;
                        var left = -right;
                        var top = right * aspectRatioVertical;
                        var bottom = -top;
                        _projection = Matrix4x4.CreatePerspective(right - left, top - bottom, Near, Far);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
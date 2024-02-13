using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace Scrblr.Core
{
    public class Sketch : GraphicsContext, IDisposable
    {
        protected IWindow? Window;

        protected IInputContext? InputContext;

        protected IKeyboard? Keyboard;

        protected IMouse? Mouse;

        public int WindowWidth { get; private set; } = 640;
        public int WindowHeight { get; private set; } = 640;

        private event Action? _loadHandler;
        private event Action? _closingHandler;
        private event Action? _renderHandler;
        private event Action? _updateHandler;
        private event Action<Vector2D<int>>? _resizeHandler;
        private event Action<IKeyboard, Key, int> _keyDownHandler;

        public Sketch()
            : this(1f)
        {
        }

        public Sketch(float size)
            : this(size, size)
        {
        }

        public Sketch(float width, float height)
            : base(width, height)
        {
        }

        ~Sketch() => Dispose(false);

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
            {
                return;
            }

            InputContext?.Dispose();
            
            Window?.Dispose();
        }

        public void Run()
        {
            BindDelegates();

            CreateWindow();

            Window?.Run();
        }

        private void BindDelegates()
        {
            BindDelegate("_renderHandler", "Render");
            BindDelegate("_closingHandler", "Closing");
            BindDelegate("_loadHandler", "Load");
            BindDelegate("_updateHandler", "Update");
            BindDelegate("_resizeHandler", "Resize");
            BindDelegate("_keyDownHandler", "KeyDown");
        }

        private void BindDelegate(string eventName, string methodName)
        {
            var sketchType = GetType();

            var eventInfo = sketchType.BaseType.GetEvent(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var methodInfo = sketchType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var d = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);

            eventInfo.GetAddMethod(true).Invoke(this, new object[] { d });
        }

        private void CreateWindow()
        {
            var options = Silk.NET.Windowing.WindowOptions.Default;

            options.Size = CalculateWindowSize(Width, Height);
            options.Title = "LearnOpenGL with Silk.NET";

            Context.Window = Window = Silk.NET.Windowing.Window.Create(options);

            Window.Load += _internalLoad;
            Window.Render += _internalRender;
            Window.Update += _internalUpdate;
            Window.Resize += _internalResize;
            Window.Closing += _internalClosing;

            Console.WriteLine($"Window size is: {options.Size.X}x{options.Size.Y}");
        }

        private unsafe void _internalLoad()
        {
            Setup(Window);

            InputContext = Window.CreateInput();

            for (int i = 0; i < InputContext.Keyboards.Count; i++)
            {
                InputContext.Keyboards[i].KeyDown += _keyDownHandler;
            }

            _loadHandler?.Invoke();
        }

        private unsafe void _internalRender(double elapsedTime)
        {
            Clear();

            _renderHandler?.Invoke();

            Flush();
        }

        private void _internalUpdate(double elapsedTime)
        {
            _updateHandler?.Invoke();
        }

        private unsafe void _internalClosing()
        {
            _closingHandler?.Invoke();
        }

        private void _internalKeyDown(IKeyboard keyboard, Key key, int code)
        {
            switch (key)
            {
                case Key.Escape:
                    Window?.Close();
                    break;
            }

            _keyDownHandler?.Invoke(keyboard, key, code);
        }

        private void _internalResize(Vector2D<int> size)
        {
            Resize(size);

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
    }
}
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scrblr.Core
{
    public abstract class AbstractSketch : ISketch
    {
        protected static GL Gl;

        protected IWindow? window;

        protected IInputContext? inputContect;

        private readonly Stopwatch _timeStopWatch = new Stopwatch();

        public int FrameCount { get; private set; }

        public double ElapsedTime { get; private set; }

        public int FramesPerSecond { get; private set; }

        private long LastFramesPerSecondTimeStamp { get; set; }

        // any public events ending with 'Action' will be bound in the Sketch.BindDelegates<TSketch>(TSketch sketch) function
        // the function that will be bound (if found) must have the same name as the event minus 'Action'.
        // i.e.: a function named 'Load' will be bound to the event 'LoadAction'
        private event Action LoadAction;
        private event Action UnLoadAction;
        private event Action RenderAction;
        private event Action UpdateAction;
        private event Action<uint, uint> ResizeAction;

        private void Initialize()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 680);
            options.Title = "LearnOpenGL with Silk.NET";
            window = Window.Create(options);

            window.Load += LoadInternal;
            window.Render += RenderInternal;
            window.Update += UpdateInternal;
            window.Resize += ResizeInternal;
            window.Closing += UnLoadInternal;

            BindDelegates();
        }


        private bool BindDelegates()
        {
            Diagnostics.Log($"Binding delegetes.");

            var sketchType = typeof(AbstractSketch);

            var eventInfoArray = sketchType.GetEvents(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var eventInfo in eventInfoArray)
            {
                if (!eventInfo.Name.EndsWith("Action"))
                {
                    continue;
                }

                var methodName = eventInfo.Name.Substring(0, eventInfo.Name.Length - 6);

                if (!BindDelegete(this, eventInfo, methodName) && methodName.Equals("Render"))
                {
                    Diagnostics.Error($"Could not bind the Render delegate. Cannot run and quiting.");

                    return false;
                }
            }

            return true;
        }

        private static bool BindDelegete(AbstractSketch sketch, EventInfo eventInfo, string methodName)
        {
            var sketchType = sketch.GetType();

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

            eventInfo.GetAddMethod(true).Invoke(sketch, new object[] { d });

            return true;
        }

        private void LoadInternal()
        {
            PreLoadInternal();

            LoadAction?.Invoke();

            PostLoadInternal();
        }

        private void PreLoadInternal()
        {
            Gl = GL.GetApi(window);

            inputContect = window.CreateInput();

            for (int i = 0; i < inputContect.Keyboards.Count; i++)
            {
                inputContect.Keyboards[i].KeyDown += KeyDownInternal;
                inputContect.Keyboards[i].KeyUp += KeyUpInternal;
                inputContect.Keyboards[i].KeyChar += KeyCharInternal;
            }
        }

        private void PostLoadInternal()
        {

        }

        private void RenderInternal(double elapsedTime)
        {
            PreRenderFrameInternal(elapsedTime);

            RenderAction();

            PostRenderFrameInternal(elapsedTime);
        }

        private void PreRenderFrameInternal(double elapsedTime)
        {

        }

        private void PostRenderFrameInternal(double elapsedTime)
        {

        }

        private void UpdateInternal(double elapsedTime)
        {
            PreUpdateFrameInternal(elapsedTime);

            UpdateAction?.Invoke();

            PostUpdateFrameInternal(elapsedTime);
        }

        private void ResizeInternal(Vector2D<int> size)
        {
            ResizeAction?.Invoke((uint)size.X, (uint)size.Y);
        }

        private void UnLoadInternal()
        {
            UnLoadAction?.Invoke();
        }

        private void PreUpdateFrameInternal(double elapsedTime)
        {
            FrameCount++;
            FramesPerSecond++;

            ElapsedTime = elapsedTime;

            var currentTimeStamp = _timeStopWatch.ElapsedMilliseconds;

            if (LastFramesPerSecondTimeStamp + 2000 <= currentTimeStamp)
            {
                Diagnostics.Log($"FPS: {FramesPerSecond}");

                LastFramesPerSecondTimeStamp = currentTimeStamp;
                FramesPerSecond = 0;
            }

            //ResetStatesAndCounters();

            //_graphics.ClearMatrixStack();

            //foreach (var eventComponent in EventComponents)
            //{
            //    eventComponent.KeyboardState = _internalWindow.KeyboardState;
            //    eventComponent.MouseState = _internalWindow.MouseState;
            //    eventComponent.ElapsedTime = ElapsedTime;
            //}
        }

        private void PostUpdateFrameInternal(double elapsedTime)
        {
        }

        private void KeyDownInternal(IKeyboard keyboard, Key key, int arg3)
        {
            if (key == Key.Escape)
            {
                window.Close();
            }
        }

        private void KeyUpInternal(IKeyboard keyboard, Key key, int arg3)
        {
        }

        private void KeyCharInternal(IKeyboard keyboard, char c)
        {
        }

        #region Run

        public void Run(bool dispose = true)
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

            window.Run();

            if (dispose)
            {
                Dispose();
            }

            _timeStopWatch.Stop();

            Diagnostics.Log($"Close time: {_timeStopWatch.ElapsedMilliseconds} ms.");

            Diagnostics.Log($"Virtual memory: {(Process.GetCurrentProcess().VirtualMemorySize64 / (1024 * 1024))} MB");

            GC.Collect();
        }

        #endregion Run

        #region Dispose

        public void Dispose()
        {
            if (window != null)
            {
                //try
                //{
                //    // calling window.IsClosing seems to throw an exception when it' already closed
                //    if (!window.IsClosing)
                //    {
                //        window.Close();
                //    }
                //}
                //catch
                //{

                //}

                window.Dispose();
                window = null;
            }
            GC.SuppressFinalize(this);
        }

        #endregion Dispose
    }
}

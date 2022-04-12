using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkriBlur.Core
{
    internal class Window : IDisposable
    {
        private GameWindow _internalWindow;
        private bool disposedValue;

        public event Action Update;
        public event Action Load;
        public event Action Render;
        public event Action Quit;

        public Window(int width, int height, string title)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(width, height),
                Title = title,
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,
            };

            _internalWindow = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);

            _internalWindow.UpdateFrame += InternalWindowUpdateFrame;
            _internalWindow.Resize += InternalWindowResize;
        }

        public void Run()
        {
            _internalWindow.Run();
        }

        public void SwapBuffers()
        {
            _internalWindow.SwapBuffers();
        }

        private void InternalWindowUpdateFrame(FrameEventArgs e)
        {
            if (_internalWindow.KeyboardState.IsKeyDown(Keys.Escape))
            {
                _internalWindow.Close();
            }

            if(Update != null)
            {
                Update();
            }
        }

        private void InternalWindowResize(ResizeEventArgs e)
        {
            // When the window gets resized, we have to call GL.Viewport to resize OpenGL's viewport to match the new size.
            // If we don't, the NDC will no longer be correct.
            GL.Viewport(0, 0, _internalWindow.Size.X, _internalWindow.Size.Y);
        }

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if(_internalWindow != null)
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
        // ~Window()
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

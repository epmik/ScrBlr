using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;

namespace Scrblr.LearnOpenTK
{
    public class SilkSketch : IDisposable
    {
        public static GL GL;

        protected IWindow? window;

        protected IInputContext? inputContect;

        public void Dispose()
        {
            inputContect.Dispose();
            window.Dispose();
        }

        public void Run()
        {
            window.Run();
        }
    }
}
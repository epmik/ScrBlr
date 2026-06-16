
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Skrbl;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace OpenGlTutorialOrg
{
    public abstract class AbstractTutorialWindow : GameWindow
    {
        protected string ResourceDirectory = "opengl-tutorial.org/";

        // A simple constructor to let us set properties like window size, title, FPS, etc. on the window.
        public AbstractTutorialWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Title = "OpenTK Tutorials based on tutorials from opengl-tutorial.org";
        }

        protected string ResourcePath(string resource)
        {
            return System.IO.Path.Combine(ResourceDirectory, resource);
        }
    }
}
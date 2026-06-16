using OpenGlTutorialOrg;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Skrbl.Learn
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var gameWindowSettings = GameWindowSettings.Default;

            var nativeWindowSettings = new NativeWindowSettings()
            {
                //APIVersion = new System.Version(3, 3),
                ClientSize = (1024, 768),
                Title = string.Empty,
                Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug,     // needed to run on macos
                WindowBorder = WindowBorder.Hidden,
                NumberOfSamples = 4,
                DepthBits = 32,
                StencilBits = 0,
            };

            using (var sketch = new Tutorial14WindowExtented07(gameWindowSettings, nativeWindowSettings))
            {
                sketch.Focus();

                sketch.Run();
            }
        }
    }
}

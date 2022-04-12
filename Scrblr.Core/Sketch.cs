using System;
using System.Collections.Generic;
using System.Text;

namespace Scrblr.Core
{
    public class Sketch
    {
        public static void Run<TSketch>() where TSketch : ISketch, new()
        {
            Run(new TSketch());
        }

        public static void Run<TSketch>(TSketch sketch) where TSketch : ISketch
        {
            sketch.Run();
        }
    }
}

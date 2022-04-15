using System;
using System.Collections.Generic;
using System.Text;

namespace Scrblr.Core
{
    public class Sketch
    {
        /// <summary>
        /// Creates a sketch of type TSketch and calls the Run function. The Sketch is disposed afterwards
        /// </summary>
        /// <typeparam name="TSketch">The type of ISketch to create</typeparam>
        public static void Run<TSketch>() where TSketch : ISketch, new()
        {
            Run(new TSketch(), true);
        }

        /// <summary>
        /// Calls the Run function on the sketch of type TSketch disposed the sketch if <paramref name="dispose"/> is true.
        /// </summary>
        /// <typeparam name="TSketch"></typeparam>
        /// <param name="sketch">The sketch of type TSketch to run</param>
        /// <param name="dispose">Dispose <paramref name="sketch"/>: true or false</param>
        public static void Run<TSketch>(TSketch sketch, bool dispose = true) where TSketch : ISketch
        {
            sketch.Run();

            if (dispose)
            {
                sketch.Dispose();
            }
        }
    }
}

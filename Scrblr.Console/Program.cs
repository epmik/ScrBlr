using Scrblr.Core;
using System;

namespace Scrblr.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Sketch.Run<TrialSketch>();
            // Sketch.Run(new TrialSketch());
        }
    }
}

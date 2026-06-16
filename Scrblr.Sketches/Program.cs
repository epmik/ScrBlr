using Scrblr.Core;
using System;
using System.Threading;

namespace Scrblr.Sketches
{
    class Program
    {
        static void Main(string[] args)
        {
            Sketch.Run<Sketch006>();

            Console.WriteLine("Closing in 2 seconds...");

            Thread.Sleep(2000);
        }
    }
}

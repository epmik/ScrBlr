using Scrblr.Core;
using System;
using System.Threading;

namespace Scrblr.Sketches
{
    class Program
    {
        static void Main(string[] args)
        {
            Sketch.Run<Sketch001>();

            Console.WriteLine("Closing in 2 seconds...");

            Thread.Sleep(2000);
        }
    }
}

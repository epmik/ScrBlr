using Scrblr.Core;
using System;
using System.Threading;

namespace Scrblr.Leaning
{
    class Program
    {
        static void Main(string[] args)
        {
            Sketch.Run<Learn031>();

            Console.WriteLine("Closing in 5 seconds...");

            Thread.Sleep(5000);
        }
    }
}

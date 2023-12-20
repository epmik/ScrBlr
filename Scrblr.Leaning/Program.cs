using Scrblr.Core;
using System;
using System.Threading;

namespace Scrblr.Leaning
{
    class Program
    {
        static void Main(string[] args)
        {
            Sketch.Run<Learn001>();

            Console.WriteLine("Closing in 2 seconds...");

            Thread.Sleep(2000);
        }
    }
}

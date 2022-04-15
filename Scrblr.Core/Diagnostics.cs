using System;
using System.Diagnostics;

namespace Scrblr.Core
{
    public static class Diagnostics
    {
        public static bool LogToConsole = true;
        public static bool LogToDebugWindow = true;

        public static void Log(string message)
        {
#if DEBUG
            Debug.WriteLine(message);
            Console.WriteLine(message);
#endif
        }
    }
}
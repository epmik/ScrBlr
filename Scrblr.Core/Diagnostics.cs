using System;
using System.Diagnostics;

namespace Scrblr.Core
{
    public static class Diagnostics
    {
        public enum Level
        {
            Info = 0,

            Log,

            Warn,

            Error,
        }

        public static bool WriteToConsole = true;
        public static bool WriteToDebugWindow = true;
        public static Level WriteLevel = Level.Log;

        public static void Log(string message)
        {
#if DEBUG
            WriteMessage(message, Level.Log);
#endif
        }

        public static void Warn(string message)
        {
#if DEBUG
            WriteMessage(message, Level.Warn);
#endif
        }

        public static void Error(string message)
        {
#if DEBUG
            WriteMessage(message, Level.Error);
#endif
        }


        private static void WriteMessage(string message, Level level)
        {
#if DEBUG
            if ((int)level < (int)WriteLevel)
            {
                return;
            }

            if (WriteToDebugWindow)
            {
                Debug.WriteLine(message);
            }
            if (WriteToConsole)
            {
                Console.WriteLine(message);
            }
#endif
        }
    }
}
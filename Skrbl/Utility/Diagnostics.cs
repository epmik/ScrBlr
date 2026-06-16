
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Skrbl
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

        public static bool ThrowOnOpenGlDebugMessage = false;

        private static DebugProc OpenGlDebugMessageDelegate = OpenGlDebugMessageHandler;

        private static void OpenGlDebugMessageHandler(
            DebugSource source,     // Source of the debugging message.
            DebugType type,         // Type of the debugging message.
            int id,                 // ID associated with the message.
            DebugSeverity severity, // Severity of the message.
            int length,             // Length of the string in pMessage.
            IntPtr pMessage,        // Pointer to message string.
            IntPtr pUserParam)      // The pointer you gave to OpenGL, explained later.
        {
            var message = $"[{severity} source={source} type={type} id={id}] {Marshal.PtrToStringAnsi(pMessage, length)}";

            Diagnostics.Log(message);

            if (ThrowOnOpenGlDebugMessage && type == DebugType.DebugTypeError)
            {
                throw new Exception(message);
            }
        }

        public static void EnableOpenGlDebugMessages()
        {
            EnableOpenGlDebugMessages([DebugSeverityControl.DontCare]);
        }

        public static void EnableOpenGlDebugMessages(DebugSeverityControl[] severities)
        {
            // ------------------------------------------------------------------------------------
            // start debug message setup
            //
            // see
            // https://opentk.net/learn/appendix_opengl/KHR_debug.html?tabs=debug-context-4
            // and
            // https://opentk.net/learn/appendix_opengl/debug_callback.html?tabs=debug-context-3%2Cdelegate-gl%2Cenable-gl

            GL.DebugMessageCallback(Diagnostics.OpenGlDebugMessageDelegate, IntPtr.Zero);

            foreach(var severity in severities)
            {
                GL.DebugMessageControl(
                    DebugSourceControl.DontCare,
                    DebugTypeControl.DontCare,
                    severity,
                    0,
                    Array.Empty<int>(),
                    true);
            }

            EnableOpenGlDebugOutput();

            // end debug message setup
            // ------------------------------------------------------------------------------------
        }

        public static void DisableOpenGlDebugMessages()
        {
            DisableOpenGlDebugMessages([DebugSeverityControl.DontCare]);
        }

        public static void DisableOpenGlDebugMessages(DebugSeverityControl[] severities)
        {
            // ------------------------------------------------------------------------------------
            // start debug message setup
            //
            // see
            // https://opentk.net/learn/appendix_opengl/KHR_debug.html?tabs=debug-context-4
            // and
            // https://opentk.net/learn/appendix_opengl/debug_callback.html?tabs=debug-context-3%2Cdelegate-gl%2Cenable-gl

            foreach (var severity in severities)
            {
                GL.DebugMessageControl(
                    DebugSourceControl.DontCare,
                    DebugTypeControl.DontCare,
                    severity,
                    0,
                    Array.Empty<int>(),
                    false);
            }

            // end debug message setup
            // ------------------------------------------------------------------------------------
        }

        public static void EnableOpenGlDebugOutput()
        {
            // ------------------------------------------------------------------------------------
            // enable debug output
            //

            // Enable or disable debug message generation. If disabled, inhibits sending messages to your callback.
            GL.Enable(EnableCap.DebugOutput);

            // When enabled, debug messages are sent only to the thread that generated the message. Useful when debugging and viewing the callstack. Hinders performance.
            GL.Enable(EnableCap.DebugOutputSynchronous);

            // end disable debug output
            // ------------------------------------------------------------------------------------
        }

        public static void DisableOpenGlDebugOutput()
        {
            // ------------------------------------------------------------------------------------
            // disable debug output
            //

            // Enable or disable debug message generation. If disabled, inhibits sending messages to your callback.
            GL.Disable(EnableCap.DebugOutput);

            // When enabled, debug messages are sent only to the thread that generated the message. Useful when debugging and viewing the callstack. Hinders performance.
            GL.Disable(EnableCap.DebugOutputSynchronous);

            // end disable debug output
            // ------------------------------------------------------------------------------------
        }
    }
}
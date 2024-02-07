using Silk.NET.OpenGL;
using System;
using System.Diagnostics;

namespace Scrblr.Core
{
    internal static class GLUtility
    {
        public static void CheckError()
        {
            var error = (ErrorCode)Context.GL.GetError();

            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}
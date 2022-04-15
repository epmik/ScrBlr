using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace Scrblr.Core
{
    // taken from https://stackoverflow.com/a/42437504/527843
    public static class TypeSize<T>
    {
        public readonly static int Size;

        static TypeSize()
        {
            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Sizeof, typeof(T));
            il.Emit(OpCodes.Ret);
            Size = (int)dm.Invoke(null, null);
        }
    }
}
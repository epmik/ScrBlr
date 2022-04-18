using System;

namespace Scrblr.Core
{
    public static class Utility
    {
        public const float ByteToUnitSingleFactor = 0.0039215686274509803921568627451f; // 1/255

        public static float ToUnitSingle(int v)
        {
            return (float)v * ByteToUnitSingleFactor;
        }

        // taken from equilinox source
        //public static float FastSin(float min, float max, float time)
        //{
        //    float x = 1 - Math.Abs((time + 0.25f) * 2 % 2 - 1);
        //    return Maths.smoothInterpolate(min, max, x);
        //}

        //// see https://gist.github.com/XupremZero/49f82c9e21b42ac67a1f4e085c00226c
        //// Fast accurate aproximation of Sine and Cosine functions in C# by Samuel Alonso
        //public static float FastSin(float radians) //x in radians
        //{
        //    float sinn;
        //    if (radians < -3.14159265f)
        //        radians += 6.28318531f;
        //    else
        //    if (radians > 3.14159265f)
        //        radians -= 6.28318531f;

        //    if (radians < 0)
        //    {
        //        sinn = 1.27323954f * radians + 0.405284735f * radians * radians;

        //        if (sinn < 0)
        //            sinn = 0.225f * (sinn * -sinn - sinn) + sinn;
        //        else
        //            sinn = 0.225f * (sinn * sinn - sinn) + sinn;
        //        return sinn;
        //    }
        //    else
        //    {
        //        sinn = 1.27323954f * radians - 0.405284735f * radians * radians;

        //        if (sinn < 0)
        //            sinn = 0.225f * (sinn * -sinn - sinn) + sinn;
        //        else
        //            sinn = 0.225f * (sinn * sinn - sinn) + sinn;
        //        return sinn;

        //    }
        //}

        //public static float FastCos(float radians) //x in radians
        //{
        //    return FastSin(radians + 1.5707963f);
        //}
    }
}
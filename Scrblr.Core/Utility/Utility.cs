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
    }
}
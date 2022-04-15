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
    }
}
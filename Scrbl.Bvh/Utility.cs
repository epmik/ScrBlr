namespace Scrbl.Bvh
{
    using System;
    using System.Runtime.InteropServices;

    public static class Utility
    {
        /// <summary>
        /// Maps a float value from an initial range to a target range.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="fromMin">The lower bound of the initial range.</param>
        /// <param name="fromMax">The upper bound of the initial range.</param>
        /// <param name="toMin">The lower bound of the target range.</param>
        /// <param name="toMax">The upper bound of the target range.</param>
        /// <param name="clamp">If true, restricts the output to the target range boundaries.</param>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax, bool clamp = false)
        {
            // Calculate the linear proportion (0.0 to 1.0) of the value in the source range
            float t = (value - fromMin) / (fromMax - fromMin);

            // Map that proportion onto the target range
            float result = toMin + t * (toMax - toMin);

            // Optional: Clamp the result so it does not overshoot target boundaries
            if (clamp)
            {
                float minBound = Math.Min(toMin, toMax);
                float maxBound = Math.Max(toMin, toMax);
                return Math.Clamp(result, minBound, maxBound);
            }

            return result;
        }

        /// <summary>
        /// Maps a float value from an initial range to a target range between 0 and 1.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="fromMin">The lower bound of the initial range.</param>
        /// <param name="fromMax">The upper bound of the initial range.</param>
        /// <param name="clamp">If true, restricts the output to the target range boundaries.</param>
        public static float Remap(this float value, float fromMin, float fromMax, bool clamp = false)
        {
            return Remap(value, fromMin, fromMax, 0.0f, 1.0f, clamp);
        }
    }
}

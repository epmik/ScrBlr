using OpenTK.Mathematics;
using System;
using System.Drawing;

namespace Scrblr.Core
{
    public static class Utility
    {
        public const float ByteToUnitSingleFactor = 0.0039215686274509803921568627451f; // 1/255

        public static double DegreesToRadians(double degrees)
        {
            return (0.01745329251994329576923690768489) * degrees;
        }

        public static float ToUnitSingle(int v)
        {
            return (float)v * ByteToUnitSingleFactor;
        }

        public static Matrix4 ObjectLookAtMatrix(Vector3 eye, Vector3 target)
        {
            Vector3 direction = Vector3.Normalize(target - eye);
            Vector3 up = Vector3.UnitY;

            if (MathF.Abs(direction.X) < 0.00001f && MathF.Abs(direction.Z) < 0.00001f)
            {
                if (direction.Y > 0)
                    up = new Vector3(0, 0, -1.0f);
                else
                    up = new Vector3(0, 0, 1.0f);
            }

            return ObjectLookAtMatrix(eye, target, Vector3.Normalize(up));
        }

        public static Matrix4 ObjectLookAtMatrix(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 direction = Vector3.Normalize(target - eye);
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, direction));

            up = Vector3.Normalize(Vector3.Cross(direction, right));

            return new Matrix4(
                new Vector4(right.X, right.Y, right.Z, 0.0f),
                new Vector4(up.X, up.Y, up.Z, 0.0f),
                new Vector4(direction.X, direction.Y, direction.Z, 0.0f),
                new Vector4(eye.X, eye.Y, eye.Z, 1.0f));

            //return Matrix4.Invert(Matrix4.LookAt(eye, target, up));
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

        static public float ReBase(
            float value,
            float currentStart, float currentStop,
            float targetStart, float targetStop)
        {
            return targetStart + (targetStop - targetStart) * ((value - currentStart) / (currentStop - currentStart));
        }

        //public static int LerpColor(int a, int b, float factor)
        //{
        //    // see https://github.com/processing/processing/blob/master/core/src/processing/core/PGraphics.java
        //    // line 8125

        //    if (factor < 0f) 
        //        factor = 0f;
        //    else if (factor > 1f) 
        //        factor = 1f;

        //    //if (mode == RGB)
        //    //{
        //        float a1 = ((a >> 24) & 0xff);
        //        float r1 = (a >> 16) & 0xff;
        //        float g1 = (a >> 8) & 0xff;
        //        float b1 = a & 0xff;
        //        float a2 = (b >> 24) & 0xff;
        //        float r2 = (b >> 16) & 0xff;
        //        float g2 = (b >> 8) & 0xff;
        //        float b2 = b & 0xff;

        //        return (((int)MathF.Round(a1 + (a2 - a1) * factor) << 24) |
        //                ((int)MathF.Round(r1 + (r2 - r1) * factor) << 16) |
        //                ((int)MathF.Round(g1 + (g2 - g1) * factor) << 8) |
        //                ((int)MathF.Round(b1 + (b2 - b1) * factor)));

        //    //}
        //    //else if (mode == HSB)
        //    //{
        //    //    if (lerpColorHSB1 == null)
        //    //    {
        //    //        lerpColorHSB1 = new float[3];
        //    //        lerpColorHSB2 = new float[3];
        //    //    }

        //    //    float a1 = (c1 >> 24) & 0xff;
        //    //    float a2 = (c2 >> 24) & 0xff;
        //    //    int alfa = (PApplet.round(a1 + (a2 - a1) * amt)) << 24;

        //    //    Color.RGBtoHSB((c1 >> 16) & 0xff, (c1 >> 8) & 0xff, c1 & 0xff,
        //    //                   lerpColorHSB1);
        //    //    Color.RGBtoHSB((c2 >> 16) & 0xff, (c2 >> 8) & 0xff, c2 & 0xff,
        //    //                   lerpColorHSB2);

        //    //    /* If mode is HSB, this will take the shortest path around the
        //    //     * color wheel to find the new color. For instance, red to blue
        //    //     * will go red violet blue (backwards in hue space) rather than
        //    //     * cycling through ROYGBIV.
        //    //     */
        //    //    // Disabling rollover (wasn't working anyway) for 0126.
        //    //    // Otherwise it makes full spectrum scale impossible for
        //    //    // those who might want it...in spite of how despicable
        //    //    // a full spectrum scale might be.
        //    //    // roll around when 0.9 to 0.1
        //    //    // more than 0.5 away means that it should roll in the other direction
        //    //    /*
        //    //    float h1 = lerpColorHSB1[0];
        //    //    float h2 = lerpColorHSB2[0];
        //    //    if (Math.abs(h1 - h2) > 0.5f) {
        //    //      if (h1 > h2) {
        //    //        // i.e. h1 is 0.7, h2 is 0.1
        //    //        h2 += 1;
        //    //      } else {
        //    //        // i.e. h1 is 0.1, h2 is 0.7
        //    //        h1 += 1;
        //    //      }
        //    //    }
        //    //    float ho = (PApplet.lerp(lerpColorHSB1[0], lerpColorHSB2[0], amt)) % 1.0f;
        //    //    */
        //    //    float ho = PApplet.lerp(lerpColorHSB1[0], lerpColorHSB2[0], amt);
        //    //    float so = PApplet.lerp(lerpColorHSB1[1], lerpColorHSB2[1], amt);
        //    //    float bo = PApplet.lerp(lerpColorHSB1[2], lerpColorHSB2[2], amt);

        //    //    return alfa | (Color.HSBtoRGB(ho, so, bo) & 0xFFFFFF);
        //    //}
        //}

        //public static Color LerpColor(Color a, Color b, float factor)
        //{
        //    // see https://github.com/processing/processing/blob/master/core/src/processing/core/PGraphics.java
        //    // line 8125

        //    if (factor < 0f)
        //        factor = 0f;
        //    else if (factor > 1f)
        //        factor = 1f;

        //    float a1 = a.A / 255f;
        //    float r1 = a.R / 255f;
        //    float g1 = a.G / 255f;
        //    float b1 = a.B / 255f;
        //    float a2 = b.A / 255f;
        //    float r2 = b.R / 255f;
        //    float g2 = b.G / 255f;
        //    float b2 = b.B / 255f;

        //    return Color.FromArgb(
        //        ((int)MathF.Round(a1 + (a2 - a1) * factor)),
        //        ((int)MathF.Round(r1 + (r2 - r1) * factor)),
        //        ((int)MathF.Round(g1 + (g2 - g1) * factor)),
        //        ((int)MathF.Round(b1 + (b2 - b1) * factor)));
        //}

        public static Color4 LerpColor(Color4 a, Color4 b, float factor)
        {
            // see https://github.com/processing/processing/blob/master/core/src/processing/core/PGraphics.java
            // line 8125
        
            if (factor < 0f)
                factor = 0f;
            else if (factor > 1f)
                factor = 1f;

            return new Color4(
                (byte)MathF.Round((a.R + (b.R - a.R) * factor) * 255f),
                (byte)MathF.Round((a.G + (b.G - a.G) * factor) * 255f),
                (byte)MathF.Round((a.B + (b.B - a.B) * factor) * 255f),
                (byte)MathF.Round((a.A + (b.A - a.A) * factor) * 255f));
        }
    }
}
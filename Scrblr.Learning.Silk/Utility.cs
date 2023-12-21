using System;
using System.Numerics;

namespace Scrblr.Learning
{
    public static class Utility
    {
        public const float ByteToUnitSingleFactor = 0.0039215686274509803921568627451f; // 1/255

        public static double DegreesToRadians(double degrees)
        {
            return 0.01745329251994329576923690768489 * degrees;
        }

        public static double RadiansToDegrees(double radians)
        {
            return 57.295779513082320876798154814105 * radians;
        }

        public static float ToUnitSingle(int v)
        {
            return (float)v * ByteToUnitSingleFactor;
        }

        public static Matrix4x4 ObjectLookAtMatrix(Vector3 eye, Vector3 target)
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

        public static Matrix4x4 ObjectLookAtMatrix(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 direction = Vector3.Normalize(target - eye);
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, direction));
            
            up = Vector3.Normalize(Vector3.Cross(direction, right));

            return new Matrix4x4(
                right.X, right.Y, right.Z, 0.0f,
                up.X, up.Y, up.Z, 0.0f,
                direction.X, direction.Y, direction.Z, 0.0f,
                eye.X, eye.Y, eye.Z, 1.0f);

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
    }
}
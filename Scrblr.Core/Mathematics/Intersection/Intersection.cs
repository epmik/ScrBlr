using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    // copied from
    // Geometric Tools Library
    // https://www.geometrictools.com
    // Copyright (c) 2022 David Eberly
    // Distributed under the Boost Software License, Version 1.0
    // https://www.boost.org/LICENSE_1_0.txt
    // File Version: 2022.05.22
    // 
    // Compute the distance between a point and a segment in nD.
    // 
    // The segment is P0 + t * (P1 - P0) for 0 <= t <= 1. The direction D = P1-P0
    // is generally not unit length.
    // 
    // The input point is stored in closest[0]. The closest point on the segment
    // is stored in closest[1]. When there are infinitely many choices for the
    // pair of closest points, only one of them is returned.
    public static class Intersection
    {
        public enum Status
        {
            /// <summary>
            /// No intersection
            /// </summary>
            None,

            /// <summary>
            /// Intersection
            /// </summary>
            Intersection,

            /// <summary>
            /// Parallel
            /// </summary>
            Parallel,
        }

        public class Result
        {
            public Status Status = Status.None;
            //public float Distance;
            //public float DistanceSquared;
            public Vector2 Intersection0;
            public float T0;
            public Vector2 Intersection1;
            public float T1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <returns>
        /// <para>
        /// Intersection.Result.Distance == distance from <param name="point"></param> to <param name="line"></param>
        /// </para>
        /// <para>
        /// Intersection.Result.DistanceSquared == squared distance from <param name="point"></param> to <param name="line"></param>
        /// </para>
        /// <para>
        /// Intersection.Result.Intersection == the intersection point of <param name="point"></param> projected upon <param name="line"></param>. If <param name="point"></param> is on <param name="line"></param>, then Intersection.Result.Intersection == <param name="point"></param>
        /// </para>
        /// <para>
        /// Intersection.Result.T == the normalized 'distance' along <param name="line"></param> from the line origin to the intersection point.
        /// 0f == the intersection point is equal to the line origin
        /// 1f == the intersection point is equal to the line end point
        /// < 0f == the intersection point is in from of the origin
        /// > 1f == the intersection point is behind the end point
        /// </para>
        /// </returns>
        public static Result Compute(ref Vector2 point, ref Line2 line)
        {
            Result output = new Result { Status = Status.Intersection };

            var direction = line.To - line.From;
            var unitdirection = Vector2.Normalize(direction);

            var diff = point - line.From;
            output.T0 = Vector2.Dot(unitdirection, diff);
            output.Intersection0 = line.From + output.T0 * unitdirection;

            //diff = point - output.Intersection0;

            //output.DistanceSquared = Vector2.Dot(diff, diff);
            //output.Distance = MathF.Sqrt(output.DistanceSquared);
            output.T0 /= direction.Length;

            if (output.T0 < 0f || output.T0 > 1f || output.T1 < 0f || output.T1 > 1f)
            {
                output.Status = Status.None;
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <returns>
        /// <para>
        /// Intersection.Result.Distance == distance from <param name="point"></param> to <param name="line"></param>
        /// </para>
        /// <para>
        /// Intersection.Result.DistanceSquared == squared distance from <param name="point"></param> to <param name="line"></param>
        /// </para>
        /// <para>
        /// Intersection.Result.Intersection == the intersection point of <param name="point"></param> projected upon <param name="line"></param>. If <param name="point"></param> is on <param name="line"></param>, then Intersection.Result.Intersection == <param name="point"></param>
        /// </para>
        /// <para>
        /// Intersection.Result.T == the normalized 'distance' along <param name="line"></param> from the line origin to the intersection point.
        /// 0f == the intersection point is equal to the line origin
        /// 1f == the intersection point is equal to the line end point
        /// < 0f == the intersection point is in from of the origin
        /// > 1f == the intersection point is behind the end point
        /// </para>
        /// </returns>
        public static Result Compute(ref Line2 line0, ref Line2 line1)
        {
            Result output = new Result();

            var diff = line0.From - line1.From;
            var line0direction = line0.Direction(false);
            var line1direction = line1.Direction(false);
            var line0unitdirection = Vector2.Normalize(line0direction);
            var line1unitdirection = Vector2.Normalize(line1direction);
            var a00 = Vector2.Dot(line0unitdirection, line0unitdirection);
            var a01 = -Vector2.Dot(line0unitdirection, line1unitdirection);
            var a11 = Vector2.Dot(line1unitdirection, line1unitdirection);
            var b0 = Vector2.Dot(line0unitdirection, diff);
            var det = MathF.Max(a00 * a11 - a01 * a01, 0f);

            if (det > 0f)
            {
                // The lines are not parallel.
                var b1 = -Vector2.Dot(line1unitdirection, diff);
                output.T0 = (a01 * b1 - a11 * b0) / det;
                output.T1 = (a01 * b0 - a00 * b1) / det;
                output.Status = Status.Intersection;
            }
            else
            {
                // The lines are parallel. Select any pair of closest points.
                output.T0 = 0f;// -b0 / a00;
                output.T1 = 0f;
                output.Status = Status.Parallel;
            }

            output.Intersection0 = line0.From + output.T0 * line0unitdirection;
            output.Intersection1 = line1.From + output.T1 * line1unitdirection;
            //diff = output.Intersection0 - output.Intersection1;

            //output.DistanceSquared = Vector2.Dot(diff, diff);
            //output.Distance = MathF.Sqrt(output.DistanceSquared);
            output.T0 /= line0direction.Length;
            output.T1 /= line1direction.Length;

            if(output.T0 < 0f || output.T0 > 1f || output.T1 < 0f || output.T1 > 1f)
            {
                output.Status = Status.None;
            }

            return output;
        }
    }
}
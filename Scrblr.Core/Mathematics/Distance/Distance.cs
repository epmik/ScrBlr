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
    public static class Distance
    {
        public class Result
        {
            public float Distance;
            public float DistanceSquared;
            public Vector2 Intersection;
        }

        public static Result Compute(ref Vector2 point, ref Line2 line)
        {
            Result output = new Result();

            // The direction vector is not unit length. The normalization is
            // deferred until it is needed.
            var direction = line.To - line.From;
            var diff = point - line.To;
            var t = Vector2.Dot(direction, diff);
            if (t >= 0f)
            {
                //output.Distance = 1f;
                output.Intersection = line.To;
            }
            else
            {
                diff = point - line.From;
                t = Vector2.Dot(direction, diff);
                if (t <= 0f)
                {
                    //output.Distance = 0f;
                    output.Intersection = line.From;
                }
                else
                {
                    var sqrLength = Vector2.Dot(direction, direction);

                    if (sqrLength > 0f)
                    {
                        t /= sqrLength;
                        //output.Distance = t;
                        output.Intersection = line.From + t * direction;
                    }
                    else
                    {
                        //output.Distance = 0f;
                        output.Intersection = line.From;
                    }
                }
            }

            diff = point - output.Intersection;
            output.DistanceSquared = Vector2.Dot(diff, diff);
            output.Distance = MathF.Sqrt(output.DistanceSquared);

            return output;
        }
    }
}
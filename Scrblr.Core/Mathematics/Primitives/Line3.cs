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
    // File Version: 2022.05.19
    // The segment is represented by (1-t)*P0 + t*P1, where P0 and P1 are the
    // endpoints of the segment and 0 <= t <= 1. Some algorithms prefer a
    // centered representation that is similar to how oriented bounding boxes are
    // defined. This representation is C + s*D, where C = (P0 + P1)/2 is the
    // center of the segment, D = (P1 - P0)/|P1 - P0| is a unit-length direction
    // vector for the segment, and |t| <= e. The value e = |P1 - P0|/2 is the
    // extent (or radius or half-length) of the segment.
    public class Line3
    {
        public Vector3 From;
        public Vector3 To;

        public Line3()
        {
            From = Vector3.Zero;
            To = Vector3.Zero;
        }

        public Line3(ref Vector3 from, ref Vector3 to)
        {
            From = from;
            To = to;
        }

        public Line3(ref Vector3 center, ref Vector3 direction, ref float extent)
        {
            From = center - extent * direction;
            To = center + extent * direction;
        }

        public static Line3 FromCentered(ref Vector3 center, ref Vector3 direction, ref float extent)
        {
            return new Line3(ref center, ref direction, ref extent);
        }

        public static void ToCentered(ref Line3 line, out Vector3 center, out Vector3 direction, out float extent)
        {
            center = 0.5f * (line.From + line.To);
            direction = line.To - line.From;
            extent = 0.5f * direction.Length;
        }

        public Vector3 Direction(bool normalized = true)
        {
            return normalized ? Vector3.Normalize(From - To) : From - To;
        }
    }
}
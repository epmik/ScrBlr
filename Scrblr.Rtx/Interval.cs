using System;
using System.Collections.Generic;
using System.Text;

namespace Scrblr.Rtx
{

    public struct Interval
    {
        public double Min { get; set; }
        public double Max { get; set; }

        // C++: interval() : min(+infinity), max(-infinity) {}
        // Explicit parameterless constructor (Supported in modern C#)
        public Interval()
        {
            Min = double.PositiveInfinity;
            Max = double.NegativeInfinity;
        }

        // C++: interval(double min, double max) : min(min), max(max) {}
        public Interval(double min, double max)
        {
            Min = min;
            Max = max;
        }

        // C++: double size() const
        public readonly double Size() => Max - Min;

        // C++: bool contains(double x) const
        public readonly bool Contains(double x) => Min <= x && x <= Max;

        // C++: bool surrounds(double x) const
        public readonly bool Surrounds(double x) => Min < x && x < Max;

        public readonly double Clamp(double x) => Math.Clamp(x, Min, Max);

        // C++: static const interval empty, universe;
        // In C#, static read-only fields provide the closest thread-safe equivalent to C++ static const objects
        public static readonly Interval Empty = new Interval(double.PositiveInfinity, double.NegativeInfinity);
        public static readonly Interval Universe = new Interval(double.NegativeInfinity, double.PositiveInfinity);
    }
}

using System;
using System.Diagnostics;

namespace Scrblr.Core
{
    public interface IRandomGenerator
    {
        int Seed { get; }

        int ReSeed();

        int ReSeed(int seed);

        int Reset();

        double Value();

        double Value(double max);

        double Value(double min, double max);

        float Value(float max);

        float Value(float min, float max);

        int Value(int max);
        
        int Value(int min, int max);
        
        bool Bool();
    }
}
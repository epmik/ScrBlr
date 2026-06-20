namespace Scrblr.Rtx
{
    public static class Utility
    {
        // Constants

        public const double Infinity = double.MaxValue;
        public const double Pi = 3.1415926535897932385;

        // Utility Functions

        public static double ToRadians(double degrees)
        {
            return degrees * Pi / 180.0;
        }
        public static double RandomDouble()
        {
            // Returns a random real double in [0.0, 1.0)
            return Random.Shared.NextDouble();
        }

        public static double RandomDouble(double min, double max)
        {
            // Returns a random real double in [min, max)
            return min + (max - min) * RandomDouble();
        }
    }
}

namespace Scrblr.Rtx
{
    public static class Utility
    {
        // Constants

        public const double Infinity = double.MaxValue;
        public const double Pi = 3.1415926535897932385;

        private static Random Random = new Random();   

        // Utility Functions

        public static double ToRadians(double degrees)
        {
            return degrees * Pi / 180.0;
        }

        public static int random_int(int min, int max)
        {
            // Returns a random integer in [min,max].
            return (int)(RandomDouble(min, max + 1));
        }

        public static void RandomSeed(int seed)
        {
            // Returns a random real double in [0.0, 1.0)
            Utility.Random = new Random(seed);
        }

        public static double RandomDouble()
        {
            // Returns a random real double in [0.0, 1.0)
            return Utility.Random.NextDouble();
        }

        public static double RandomDouble(double min, double max)
        {
            // Returns a random real double in [min, max)
            return min + (max - min) * RandomDouble();
        }
    }
}

namespace Scrblr.Rtx
{
    /// <summary>
    /// Thread-safe wrapper around System.Random.
    /// Uses lock-based synchronization to ensure thread safety.
    /// Suitable for scenarios with moderate contention.
    /// </summary>
    public class RandomGeneratorThreadSafe : IRandomGenerator
    {
        private Random _random;

        private readonly object _syncLock = new object();

        public RandomGeneratorThreadSafe(int seed)
        {
            _random = new Random(seed);
        }

        public RandomGeneratorThreadSafe()
        {
            _random = new Random();
        }


        public int Int32()
        {
            return Int32(0, 1);
        }

        public int Int32(int max)
        {
            return Int32(0, max);
        }

        public int Int32(int min, int max)
        {
            // Returns a random integer in [min,max].
            return (int)(Double(min, max + 1));
        }

        public double Double()
        {
            return Double(0.0, 1.0);
        }

        public double Double(double max)
        {
            return Double(0.0, max);
        }

        public double Double(double min, double max)
        {
            lock (_syncLock)
            {
                return min + (max - min) * _random.NextDouble();
            }
        }

        public Color Color()
        {
            return Vector3d(0.0, 1.0);
        }

        public Vector3d Vector3d()
        {
            return Vector3d(0.0, 1.0);
        }

        public Vector3d Vector3d(double min, double max)
        {
            lock (_syncLock)
            {
                return new Vector3d(
                    min + (max - min) * _random.NextDouble(),
                    min + (max - min) * _random.NextDouble(),
                    min + (max - min) * _random.NextDouble());
            }
        }

        public Vector3d UnitVector3d()
        {
            while (true)
            {
                var p = Vector3d(-1, 1);

                var lensq = p.LengthSquared();

                if (1e-160 < lensq && lensq <= 1)
                    return p / Math.Sqrt(lensq);
            }
        }

        public Vector3d HemisphereVector3d(Vector3d normal)
        {
            var on_unit_sphere = UnitVector3d();

            if (Scrblr.Rtx.Vector3d.Dot(on_unit_sphere, normal) > 0.0) // In the same hemisphere as the normal
                return on_unit_sphere;
            else
                return -on_unit_sphere;
        }

        public Vector3d InUnitDiskVector3d()
        {
            while (true)
            {
                var p = new Scrblr.Rtx.Vector3d(Double(-1, 1), Double(-1, 1), 0);
                if (p.LengthSquared() < 1)
                    return p;
            }
        }
    }

    /// <summary>
    /// Thread-safe RandomGenerator using thread-local storage.
    /// Each thread gets its own Random instance, eliminating lock contention.
    /// OPTIMAL for Parallel.For/concurrent rendering scenarios.
    /// WARNING: Cannot be seeded uniformly across threads; each thread gets an independent seed.
    /// </summary>
    public class RandomGeneratorThreadLocal
    {
        private static readonly ThreadLocal<Random> _threadLocalRandom = 
            new ThreadLocal<Random>(() => new Random());

        public RandomGeneratorThreadLocal()
        {
        }

        /// <summary>
        /// Creates a new instance with a per-thread seed offset.
        /// This allows reproducible but varied initialization across threads.
        /// </summary>
        public RandomGeneratorThreadLocal(int baseSeed)
        {
            // Re-initialize thread-local storage with a seed based on base seed + thread ID
            _threadLocalRandom.Value = new Random(baseSeed ^ Thread.CurrentThread.ManagedThreadId);
        }

        private Random Random => _threadLocalRandom.Value;

        public int Int32()
        {
            return Int32(0, 1);
        }

        public int Int32(int max)
        {
            return Int32(0, max);
        }

        public int Int32(int min, int max)
        {
            // Returns a random integer in [min,max].
            return (int)(Double(min, max + 1));
        }

        public double Double()
        {
            return Double(0.0, 1.0);
        }

        public double Double(double max)
        {
            return Double(0.0, max);
        }

        public double Double(double min, double max)
        {
            return min + (max - min) * Random.NextDouble();
        }

        public Color Color()
        {
            return new Color(Double(), Double(), Double());
        }

        public Vector3d Vector3d()
        {
            return new Vector3d(Double(), Double(), Double());
        }

        public Vector3d Vector3d(double min, double max)
        {
            return new Vector3d(Double(min, max), Double(min, max), Double(min, max));
        }

        public Vector3d UnitVector3d()
        {
            while (true)
            {
                var p = Vector3d(-1, 1);

                var lensq = p.LengthSquared();

                if (1e-160 < lensq && lensq <= 1)
                    return p / Math.Sqrt(lensq);
            }
        }

        public Vector3d HemisphereVector3d(Vector3d normal)
        {
            var on_unit_sphere = UnitVector3d();

            if (Scrblr.Rtx.Vector3d.Dot(on_unit_sphere, normal) > 0.0)
                return on_unit_sphere;
            else
                return -on_unit_sphere;
        }

        public Vector3d InUnitDiskVector3d()
        {
            while (true)
            {
                var p = new Scrblr.Rtx.Vector3d(Double(-1, 1), Double(-1, 1), 0);
                if (p.LengthSquared() < 1)
                    return p;
            }
        }
    }
}

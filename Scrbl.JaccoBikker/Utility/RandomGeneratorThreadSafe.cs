namespace Scrbl.JaccoBikker
{
    /// <summary>
    /// Thread-safe wrapper around System.Random.
    /// Uses lock-based synchronization to ensure thread safety.
    /// Suitable for scenarios with moderate contention.
    /// </summary>
    public class RandomGeneratorThreadSafe : RandomGenerator
    {
        private readonly object _syncLock = new object();

        public override float Single(float min, float max)
        {
            lock (_syncLock)
            {
                return min + (max - min) * (float)_random.NextDouble();
            }
        }

        public override double Double(double min, double max)
        {
            lock (_syncLock)
            {
                return min + (max - min) * _random.NextDouble();
            }
        }

        public override Vector3d Vector3d(double min, double max)
        {
            lock (_syncLock)
            {
                return new Vector3d(
                    min + (max - min) * _random.NextDouble(),
                    min + (max - min) * _random.NextDouble(),
                    min + (max - min) * _random.NextDouble());
            }
        }

        public override Vector3f Vector3f(double min, double max)
        {
            lock (_syncLock)
            {
                return new Vector3f(
                    (float)(min + (max - min) * _random.NextDouble()),
                    (float)(min + (max - min) * _random.NextDouble()),
                    (float)(min + (max - min) * _random.NextDouble()));
            }
        }

        public override Vector3f Vector3f(float min, float max)
        {
            lock (_syncLock)
            {
                return new Vector3f(
                    min + (max - min) * (float)_random.NextDouble(),
                    min + (max - min) * (float)_random.NextDouble(),
                    min + (max - min) * (float)_random.NextDouble());
            }
        }
    }
}

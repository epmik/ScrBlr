namespace Scrbl.JaccoBikker
{
    public class RandomGenerator : IRandomGenerator
    {
        protected readonly Random _random;

        public RandomGenerator(int seed)
        {
            _random = new Random(seed);
        }

        public RandomGenerator()
            : this(Guid.NewGuid().GetHashCode())
        {
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

        public virtual double Double(double min, double max)
        {
            return min + (max - min) * _random.NextDouble();
        }


        public float Single()
        {
            return Single(0.0f, 1.0f);
        }

        public float Single(float max)
        {
            return Single(0.0f, max);
        }


        public virtual float Single(float min, float max)
        {
            return min + (max - min) * (float)_random.NextDouble();
        }

        public float Single(double max)
        {
            return Single(0.0, max);
        }

        public float Single(double min, double max)
        {
            return (float)Double(min, max);
        }

        public Color Color()
        {
            return Vector3d(0.0, 1.0);
        }

        public Vector3d Vector3d()
        {
            return Vector3d(0.0, 1.0);
        }

        public Vector3f Vector3f()
        {
            return Vector3f(0.0f, 1.0f);
        }

        public virtual Vector3d Vector3d(double min, double max)
        {
            return new Vector3d(Double(min, max), Double(min, max), Double(min, max));
        }

        public virtual Vector3f Vector3f(float min, float max)
        {
            return new Vector3f(Single(min, max), Single(min, max), Single(min, max));
        }

        public virtual Vector3f Vector3f(double min, double max)
        {
            return new Vector3f(Single(min, max), Single(min, max), Single(min, max));
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

            if (JaccoBikker.Vector3d.Dot(on_unit_sphere, normal) > 0.0) // In the same hemisphere as the normal
                return on_unit_sphere;
            else
                return -on_unit_sphere;
        }

        public Vector3d InUnitDiskVector3d()
        {
            while (true)
            {
                var p = new JaccoBikker.Vector3d(Double(-1, 1), Double(-1, 1), 0);
                if (p.LengthSquared() < 1)
                    return p;
            }
        }
    }
}

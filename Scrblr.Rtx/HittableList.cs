namespace Scrblr.Rtx
{
    public class HittableList : Hittable
    {
        public List<Hittable> Objects { get; set; } = new List<Hittable>();

        // Constructors
        public HittableList() { }

        public HittableList(Hittable obj)
        {
            Add(obj);
        }

        // Methods
        public void Clear()
        {
            Objects.Clear();
        }

        public void Add(Hittable obj)
        {
            Objects.Add(obj);
        }

        public override bool Hit(Ray r, Interval ray_t, HitRecord rec)
        {
            HitRecord tempRec = new HitRecord();
            bool hitAnything = false;
            double closestSoFar = ray_t.Max;

            foreach (Hittable obj in Objects)
            {
                if (obj.Hit(r, new Interval(ray_t.Min, closestSoFar), tempRec))
                {
                    hitAnything = true;
                    closestSoFar = tempRec.T;

                    // Copy the values from tempRec over to our main tracking record
                    rec.T = tempRec.T;
                    rec.P = tempRec.P;
                    rec.Normal = tempRec.Normal;
                    rec.mat = tempRec.mat;
                }
            }

            return hitAnything;
        }
    }
}

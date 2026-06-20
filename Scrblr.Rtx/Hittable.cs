namespace Scrblr.Rtx
{
    // C++: class hittable
    // Abstract base class matching the pure virtual method interface from C++
    public abstract class Hittable
    {
        public abstract bool Hit(Ray r, Interval ray_t, HitRecord rec);
    }
}

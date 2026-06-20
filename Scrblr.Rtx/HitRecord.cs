namespace Scrblr.Rtx
{
    // C++: class hit_record
    // Since hit_record is passed as a mutable reference parameter to track hit details, 
    // keeping it as a class or a mutable ref struct works. A simple class mimics C++ behavior cleanly.
    public class HitRecord
    {
        public Vector3d P { get; set; }
        public Vector3d Normal { get; set; }

        public Material mat;

        public double T { get; set; }

        public bool front_face;

        public void CalculateFaceNormal(Ray r, Vector3d outward_normal)
        {
            // Sets the hit record normal vector.
            // NOTE: the parameter `outward_normal` is assumed to have unit length.

            front_face = Vector3d.Dot(r.Direction, outward_normal) < 0;

            Normal = front_face ? outward_normal : -outward_normal;
        }
    }
}

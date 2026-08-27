
namespace Scrbl.JaccoBikker
{
    using System;
    using System.Runtime.InteropServices;

    public static class Intersection
    {
        public static void Compute(Ray ray, Triangle tri)
        {
            var edge1 = tri.vertex1 - tri.vertex0;
            var edge2 = tri.vertex2 - tri.vertex0;

            var h = Vector3d.Cross(ray.D, edge2);

            var a = Vector3d.Dot(edge1, h);
        
            if (a > -0.0001 && a < 0.0001) 
                return; // ray parallel to triangle

            var f = 1 / a;
            var s = ray.O - tri.vertex0;
            var u = f * Vector3d.Dot(s, h);
        
            if (u < 0 || u > 1) 
                return;

            var q = Vector3d.Cross(s, edge1);
            var v = f * Vector3d.Dot(ray.D, q);

            if (v < 0 || u + v > 1) 
                    return;

            var t = f * Vector3d.Dot(edge2, q);

            if (t > 0.0001) 
                    ray.T = Math.Min(ray.T, t);
        }
    }
}

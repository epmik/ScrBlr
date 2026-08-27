namespace Scrbl.JaccoBikker
{
    using System;
    using System.Runtime.InteropServices;

    public static class Intersection
    {
        public class TimeResult
        {
            public double Time { get; set; }
        }

        public static bool Compute(Ray ray, Triangle tri, out TimeResult result)
        {
            result = new TimeResult();

            var edge1 = tri.vertex1 - tri.vertex0;
            var edge2 = tri.vertex2 - tri.vertex0;

            var h = Vector3d.Cross(ray.D, edge2);

            var a = Vector3d.Dot(edge1, h);
        
            if (a > -0.0001 && a < 0.0001)
                return false; // ray parallel to triangle

            var f = 1 / a;
            var s = ray.O - tri.vertex0;
            var u = f * Vector3d.Dot(s, h);
        
            if (u < 0 || u > 1)
                return false;

            var q = Vector3d.Cross(s, edge1);
            var v = f * Vector3d.Dot(ray.D, q);

            if (v < 0 || u + v > 1) 
                    return false;

            var t = f * Vector3d.Dot(edge2, q);

            if (t > 0.0001)
                result.Time = ray.T = Math.Min(ray.T, t);

            return true;
        }

        public static bool IntersectAABB(Ray ray, Vector3d min, Vector3d max)
        {
            var tx1 = (min.x - ray.O.x) / ray.D.x;
            var tx2 = (max.x - ray.O.x) / ray.D.x;
            var tmin = Math.Min(tx1, tx2);
            var tmax = Math.Max(tx1, tx2);
            var ty1 = (min.y - ray.O.y) / ray.D.y;
            var ty2 = (max.y - ray.O.y) / ray.D.y;
            tmin = Math.Max(tmin, Math.Min(ty1, ty2));
            tmax = Math.Min(tmax, Math.Max(ty1, ty2));
            var tz1 = (min.z - ray.O.z) / ray.D.z;
            var tz2 = (max.z - ray.O.z) / ray.D.z;
            tmin = Math.Max(tmin, Math.Min(tz1, tz2));
            tmax = Math.Min(tmax, Math.Max(tz1, tz2));

            return tmax >= tmin && tmin < ray.t && tmax > 0;

            //double tmin = (min.X - ray.O.X) / ray.D.X;
            //double tmax = (max.X - ray.O.X) / ray.D.X;
            //if (tmin > tmax)
            //{
            //    var temp = tmin;
            //    tmin = tmax;
            //    tmax = temp;
            //}
            //double tymin = (min.Y - ray.O.Y) / ray.D.Y;
            //double tymax = (max.Y - ray.O.Y) / ray.D.Y;
            //if (tymin > tymax)
            //{
            //    var temp = tymin;
            //    tymin = tymax;
            //    tymax = temp;
            //}
            //if ((tmin > tymax) || (tymin > tmax))
            //    return false;
            //if (tymin > tmin)
            //    tmin = tymin;
            //if (tymax < tmax)
            //    tmax = tymax;
            //double tzmin = (min.Z - ray.O.Z) / ray.D.Z;
            //double tzmax = (max.Z - ray.O.Z) / ray.D.Z;
            //if (tzmin > tzmax)
            //{
            //    var temp = tzmin;
            //    tzmin = tzmax;
            //    tzmax = temp;
            //}
            //if ((tmin > tzmax) || (tzmin > tmax))
            //    return false;
            //if (tzmin > tmin)
            //    tmin = tzmin;
            //if (tzmax < tmax)
            //    tmax = tzmax;
            //return true;
        }
    }
}

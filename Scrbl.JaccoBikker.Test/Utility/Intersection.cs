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

        //public static bool Compute(Ray ray, Triangle tri, out TimeResult result)
        //{
        //    result = new TimeResult();

        //    var edge1 = tri.vertex1 - tri.vertex0;
        //    var edge2 = tri.vertex2 - tri.vertex0;

        //    var h = Vector3f.Cross(ray.Direction, edge2);

        //    var a = Vector3f.Dot(edge1, h);
        
        //    if (a > -0.0001 && a < 0.0001)
        //        return false; // ray parallel to triangle

        //    var f = 1 / a;
        //    var s = ray.Origin - tri.vertex0;
        //    var u = f * Vector3f.Dot(s, h);
        
        //    if (u < 0 || u > 1)
        //        return false;

        //    var q = Vector3f.Cross(s, edge1);
        //    var v = f * Vector3f.Dot(ray.Direction, q);

        //    if (v < 0 || u + v > 1) 
        //            return false;

        //    var t = f * Vector3f.Dot(edge2, q);

        //    if (t > 0.0001f)
        //        result.Time = ray.Time = MathF.Min(ray.Time, t);

        //    return true;
        //}

        public static bool Compute(in Ray ray, in Triangle tri, ref TimeResult result)
        {
            var edge1 = tri.B - tri.A;
            var edge2 = tri.C - tri.A;

            var h = Vector3f.Cross(ray.Direction, edge2);

            var a = Vector3f.Dot(edge1, h);

            if (a > -0.0001 && a < 0.0001)
                return false; // ray parallel to triangle

            var f = 1 / a;
            var s = ray.Origin - tri.A;
            var u = f * Vector3f.Dot(s, h);

            if (u < 0 || u > 1)
                return false;

            var q = Vector3f.Cross(s, edge1);
            var v = f * Vector3f.Dot(ray.Direction, q);

            if (v < 0 || u + v > 1)
                return false;

            var t = f * Vector3f.Dot(edge2, q);

            if (t > 0.0001f)
                result.Time = MathF.Min(ray.Time, t);

            return true;
        }

        //public static bool IntersectAABB(Ray ray, Vector3f min, Vector3f max)
        //{
        //    var tx1 = (float)((min.X - ray.Origin.X) / ray.Direction.X);
        //    var tx2 = (float)((max.X - ray.Origin.X) / ray.Direction.X);
        //    var tmin = MathF.Min(tx1, tx2);
        //    var tmax = MathF.Max(tx1, tx2);
        //    var ty1 = (float)((min.Y - ray.Origin.Y) / ray.Direction.Y);
        //    var ty2 = (float)((max.Y - ray.Origin.Y) / ray.Direction.Y);
        //    tmin = MathF.Max(tmin, MathF.Min(ty1, ty2));
        //    tmax = MathF.Min(tmax, MathF.Max(ty1, ty2));
        //    var tz1 = (float)((min.Z - ray.Origin.Z) / ray.Direction.Z);
        //    var tz2 = (float)((max.Z - ray.Origin.Z) / ray.Direction.Z);
        //    tmin = MathF.Max(tmin, MathF.Min(tz1, tz2));
        //    tmax = MathF.Min(tmax, MathF.Max(tz1, tz2));

        //    return tmax >= tmin && tmin < ray.Time && tmax > 0;
        //}

        public static bool IntersectAABB(in Ray ray, in Vector3f min, in Vector3f max)
        {
            var tx1 = (float)((min.X - ray.Origin.X) / ray.Direction.X);
            var tx2 = (float)((max.X - ray.Origin.X) / ray.Direction.X);
            var tmin = MathF.Min(tx1, tx2);
            var tmax = MathF.Max(tx1, tx2);
            var ty1 = (float)((min.Y - ray.Origin.Y) / ray.Direction.Y);
            var ty2 = (float)((max.Y - ray.Origin.Y) / ray.Direction.Y);
            tmin = MathF.Max(tmin, MathF.Min(ty1, ty2));
            tmax = MathF.Min(tmax, MathF.Max(ty1, ty2));
            var tz1 = (float)((min.Z - ray.Origin.Z) / ray.Direction.Z);
            var tz2 = (float)((max.Z - ray.Origin.Z) / ray.Direction.Z);
            tmin = MathF.Max(tmin, MathF.Min(tz1, tz2));
            tmax = MathF.Min(tmax, MathF.Max(tz1, tz2));

            return tmax >= tmin && tmin < ray.Time && tmax > 0;
        }

        //public static bool IntersectAABB(Ray ray, Vector3d min, Vector3d max)
        //{
        //    var tx1 = (min.x - ray.Origin.X) / ray.Direction.X;
        //    var tx2 = (max.x - ray.Origin.X) / ray.Direction.X;
        //    var tmin = Math.Min(tx1, tx2);
        //    var tmax = Math.Max(tx1, tx2);
        //    var ty1 = (min.y - ray.Origin.Y) / ray.Direction.Y;
        //    var ty2 = (max.y - ray.Origin.Y) / ray.Direction.Y;
        //    tmin = Math.Max(tmin, Math.Min(ty1, ty2));
        //    tmax = Math.Min(tmax, Math.Max(ty1, ty2));
        //    var tz1 = (min.z - ray.Origin.Z) / ray.Direction.Z;
        //    var tz2 = (max.z - ray.Origin.Z) / ray.Direction.Z;
        //    tmin = Math.Max(tmin, Math.Min(tz1, tz2));
        //    tmax = Math.Min(tmax, Math.Max(tz1, tz2));

        //    return tmax >= tmin && tmin < ray.Time && tmax > 0;

        //    //double tmin = (min.X - ray.O.X) / ray.D.X;
        //    //double tmax = (max.X - ray.O.X) / ray.D.X;
        //    //if (tmin > tmax)
        //    //{
        //    //    var temp = tmin;
        //    //    tmin = tmax;
        //    //    tmax = temp;
        //    //}
        //    //double tymin = (min.Y - ray.O.Y) / ray.D.Y;
        //    //double tymax = (max.Y - ray.O.Y) / ray.D.Y;
        //    //if (tymin > tymax)
        //    //{
        //    //    var temp = tymin;
        //    //    tymin = tymax;
        //    //    tymax = temp;
        //    //}
        //    //if ((tmin > tymax) || (tymin > tmax))
        //    //    return false;
        //    //if (tymin > tmin)
        //    //    tmin = tymin;
        //    //if (tymax < tmax)
        //    //    tmax = tymax;
        //    //double tzmin = (min.Z - ray.O.Z) / ray.D.Z;
        //    //double tzmax = (max.Z - ray.O.Z) / ray.D.Z;
        //    //if (tzmin > tzmax)
        //    //{
        //    //    var temp = tzmin;
        //    //    tzmin = tzmax;
        //    //    tzmax = temp;
        //    //}
        //    //if ((tmin > tzmax) || (tzmin > tmax))
        //    //    return false;
        //    //if (tzmin > tmin)
        //    //    tmin = tzmin;
        //    //if (tzmax < tmax)
        //    //    tmax = tzmax;
        //    //return true;
        //}

        //public static bool IntersectAABB(in Ray ray, in Vector3f min, in Vector3f max)
        //{
        //    var tx1 = (min.X - ray.Origin.X) / ray.Direction.X;
        //    var tx2 = (max.X - ray.Origin.X) / ray.Direction.X;
        //    var tmin = Math.Min(tx1, tx2);
        //    var tmax = Math.Max(tx1, tx2);
        //    var ty1 = (min.Y - ray.Origin.Y) / ray.Direction.Y;
        //    var ty2 = (max.Y - ray.Origin.Y) / ray.Direction.Y;
        //    tmin = Math.Max(tmin, Math.Min(ty1, ty2));
        //    tmax = Math.Min(tmax, Math.Max(ty1, ty2));
        //    var tz1 = (min.Z - ray.Origin.Z) / ray.Direction.Z;
        //    var tz2 = (max.Z - ray.Origin.Z) / ray.Direction.Z;
        //    tmin = Math.Max(tmin, Math.Min(tz1, tz2));
        //    tmax = Math.Min(tmax, Math.Max(tz1, tz2));

        //    return tmax >= tmin && tmin < ray.Time && tmax > 0;

        //    //double tmin = (min.X - ray.O.X) / ray.D.X;
        //    //double tmax = (max.X - ray.O.X) / ray.D.X;
        //    //if (tmin > tmax)
        //    //{
        //    //    var temp = tmin;
        //    //    tmin = tmax;
        //    //    tmax = temp;
        //    //}
        //    //double tymin = (min.Y - ray.O.Y) / ray.D.Y;
        //    //double tymax = (max.Y - ray.O.Y) / ray.D.Y;
        //    //if (tymin > tymax)
        //    //{
        //    //    var temp = tymin;
        //    //    tymin = tymax;
        //    //    tymax = temp;
        //    //}
        //    //if ((tmin > tymax) || (tymin > tmax))
        //    //    return false;
        //    //if (tymin > tmin)
        //    //    tmin = tymin;
        //    //if (tymax < tmax)
        //    //    tmax = tymax;
        //    //double tzmin = (min.Z - ray.O.Z) / ray.D.Z;
        //    //double tzmax = (max.Z - ray.O.Z) / ray.D.Z;
        //    //if (tzmin > tzmax)
        //    //{
        //    //    var temp = tzmin;
        //    //    tzmin = tzmax;
        //    //    tzmax = temp;
        //    //}
        //    //if ((tmin > tzmax) || (tzmin > tmax))
        //    //    return false;
        //    //if (tzmin > tmin)
        //    //    tmin = tzmin;
        //    //if (tzmax < tmax)
        //    //    tmax = tzmax;
        //    //return true;
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Scrblr.Rtx.Materials
{
    public class Dielectric : Material
    {
        // Refractive index in vacuum or air, or the ratio of the material's refractive index over
        // the refractive index of the enclosing media
        private readonly double _refraction_index; // refraction_index

        public Dielectric(double refractionIndex)
        {
            _refraction_index = refractionIndex;
        }

        public override bool Scatter(
            Ray incidentRay,
            HitRecord rec,
            out Vector3d attenuation,
            out Ray scattered)
        {
            attenuation = new Vector3d(1.0, 1.0, 1.0);
            double ri = rec.front_face ? (1.0 / _refraction_index) : _refraction_index;

            Vector3d unitDirection = Vector3d.UnitVector(incidentRay.Direction);

            double cos_theta = Math.Min(Vector3d.Dot(-unitDirection, rec.Normal), 1.0);
            double sin_theta = Math.Sqrt(1.0 - cos_theta * cos_theta);

            bool cannot_refract = ri * sin_theta > 1.0;
            Vector3d direction;

            //if (cannot_refract || ReflectanceSchlickApproximation(cos_theta, ri) > Utility.RandomDouble())
            if (cannot_refract)
                    direction = Vector3d.Reflect(unitDirection, rec.Normal);
            else
                direction = Vector3d.Refract(unitDirection, rec.Normal, ri);

            scattered = new Ray(rec.P, direction);

            return true;

            //attenuation = new Color(1.0, 1.0, 1.0); 
            //Vec3 outwardNormal;
            //double niOverNt;
            //double cosine;
            //var reflectedRay = Vec3.Reflect(incidentRay.Direction, rec.Normal);

            //if (Vec3.Dot(incidentRay.Direction, rec.Normal) > 0)
            //{
            //    outwardNormal = -1 * rec.Normal;
            //    niOverNt = _refraction_index;
            //    cosine = _refraction_index * Vec3.Dot(incidentRay.Direction, rec.Normal) / incidentRay.Direction.Length();
            //}
            //else
            //{
            //    outwardNormal = rec.Normal;
            //    niOverNt = 1 / _refraction_index;
            //    cosine = -Vec3.Dot(incidentRay.Direction, rec.Normal) / incidentRay.Direction.Length();
            //}

            //var reflectionProbability = Refract(incidentRay.Direction, outwardNormal, niOverNt, out var refractedRay)
            //    ? ReflectanceSchlickApproximation(cosine, _refraction_index)
            //    : 1;

            //scattered = Utility.RandomDouble() < reflectionProbability
            //    ? new Ray(rec.P, reflectedRay)
            //    : new Ray(rec.P, refractedRay);

            //return true;
        }

        protected static bool Refract(Vector3d v, Vector3d n, double niOverNt, out Vector3d refractedRay)
        {
            var uv = Vector3d.UnitVector(v);
            var dt = Vector3d.Dot(uv, n);
            var discriminant = 1 - niOverNt * niOverNt * (1 - dt * dt);

            if (discriminant > 0)
            {
                refractedRay = niOverNt * (uv - n * dt) - n * Math.Sqrt(discriminant);
                return true;
            }

            refractedRay = new Vector3d(0, 0, 0);

            return false;
        }

        static double ReflectanceSchlickApproximation(double cosine, double refraction_index)
        {
            // Use Schlick's approximation for reflectance.
            var r0 = (1 - refraction_index) / (1 + refraction_index);
            r0 = r0 * r0;
            return r0 + (1 - r0) * Math.Pow((1 - cosine), 5);
        }

        //protected static double Schlick(double cosine, double refractionIndex)
        //{
        //    var r0 = (1 - refractionIndex) / (1 + refractionIndex);
        //    r0 *= r0;
        //    return r0 + (1 - r0) * Math.Pow(1 - cosine, 5);
        //}
    }

    public class DielectricSimple : Material
    {
        double _refraction_index;

        public DielectricSimple(double refraction_index) 
        {
            _refraction_index = refraction_index; 
        }

        public override bool Scatter(
                Ray rIn,
                HitRecord rec,
                out Vector3d attenuation,
                out Ray scattered)
        {
            attenuation = new Vector3d(1.0, 1.0, 1.0);

            double ri = rec.front_face ? (1.0/ _refraction_index) : _refraction_index;

            Vector3d unit_direction = Vector3d.UnitVector(rIn.Direction);
            Vector3d refracted = Vector3d.Refract(unit_direction, rec.Normal, ri);

            scattered = new Ray(rec.P, refracted);

            return true;
        }

    };
}

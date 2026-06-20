using System;
using System.Collections.Generic;
using System.Text;

namespace Scrblr.Rtx
{
    public class Lambertian : Material
    {
        private readonly Vector3d _albedo;

        // C++: lambertian(const color& albedo) : albedo(albedo) {}
        public Lambertian(Vector3d albedo)
        {
            _albedo = albedo;
        }

        // C++: bool scatter(...) const override
        public override bool Scatter(
            Ray rIn,
            HitRecord rec,
            out Vector3d attenuation,
            out Ray scattered)
        {
            Vector3d scatterDirection = rec.Normal + Vector3d.random_unit_vector();

            // Handle the edge case where the random vector is exactly opposite the normal
            // (Which can make the direction zero and cause NaN issues later)
            if (Vector3d.NearZero(scatterDirection))
            {
                scatterDirection = rec.Normal;
            }

            scattered = new Ray(rec.P, scatterDirection);
            attenuation = _albedo;

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Scrblr.Rtx
{
    public class Metal : Material
    {
        readonly Vector3d _albedo;
        readonly double _fuzz;

        public Metal(Vector3d albedo, double fuzz)
        {
            _albedo = albedo;
            _fuzz = fuzz < 1 ? fuzz : 1;
        }

        public override bool Scatter(
            Ray rIn,
            HitRecord rec,
            out Vector3d attenuation,
            out Ray scattered)
        {
            // Calculate the perfect reflection vector using the incoming direction and surface normal
            Vector3d reflected = Vector3d.Reflect(rIn.Direction, rec.Normal);
            reflected = Vector3d.UnitVector(reflected) + (_fuzz * Vector3d.random_unit_vector());

            scattered = new Ray(rec.P, reflected);
            attenuation = _albedo;

            return (Vector3d.Dot(scattered.Direction, rec.Normal) > 0);
        }
    }
}

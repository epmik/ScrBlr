using System;
using System.Collections.Generic;
using System.Text;

namespace Scrblr.Rtx
{
    public abstract class Material
    {
        public virtual bool Scatter(
            Ray rIn,
            HitRecord rec,
            out Vector3d attenuation,
            out Ray scattered)
        {
            // Set default output values before returning false
            attenuation = new Vector3d(0, 0, 0);
            scattered = new Ray();

            return false;
        }
    }
}

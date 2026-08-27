
namespace Scrbl.JaccoBikker
{
    using System;

    public struct Ray
    {
        public Vector3d Origin { get { return O; } set { O = value; } }
        public Vector3d Direction { get { return D; } set { D = value; } }
        public double Time { get { return T; } set { T = value; } }
        public Vector3d O;
        public Vector3d D;
        public double T;

        // Parameterless constructor (Defaults to 0,0,0 vectors)
        public Ray()
        {
            Origin = new Vector3d();
            Direction = new Vector3d();
        }

        // Main constructor
        public Ray(Vector3d origin, Vector3d direction, double time = 0.0)
        {
            Origin = origin;
            Direction = direction;
            Time = time;
        }

        // Linear interpolation along the ray: orig + t * dir
        public readonly Vector3d At(double t)
        {
            return Origin + Direction * t;
        }
    }

}

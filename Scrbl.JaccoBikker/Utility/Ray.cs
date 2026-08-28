namespace Scrbl.JaccoBikker
{
    using System;

    public struct Ray
    {
        public Vector3f Origin { get { return O; } set { O = value; } }
        public Vector3f Direction { get { return D; } set { D = value; } }
        public float Time { get { return T; } set { T = value; } }
        public float t { get { return T; } set { T = value; } }
        public Vector3f O;
        public Vector3f D;
        public float T;

        // Parameterless constructor (Defaults to 0,0,0 vectors)
        public Ray()
        {
            Origin = new Vector3f();
            Direction = new Vector3f();
        }

        // Main constructor
        public Ray(Vector3f origin, Vector3f direction, float time = 0.0f)
        {
            Origin = origin;
            Direction = direction;
            Time = time;
        }

        // Linear interpolation along the ray: orig + t * dir
        public readonly Vector3f At(float t)
        {
            return Origin + Direction * t;
        }
    }

}

namespace Scrblr.Rtx
{
    public struct Ray
    {
        private Vector3d _orig;
        private Vector3d _dir;

        // Parameterless constructor (Defaults to 0,0,0 vectors)
        public Ray()
        {
            _orig = new Vector3d();
            _dir = new Vector3d();
        }

        // Main constructor
        public Ray(Vector3d origin, Vector3d direction)
        {
            _orig = origin;
            _dir = direction;
        }

        // Getters matching your original origin() and direction()
        public readonly Vector3d Origin => _orig;
        public readonly Vector3d Direction => _dir;

        // Linear interpolation along the ray: orig + t * dir
        public readonly Vector3d At(double t)
        {
            return _orig + (t * _dir);
        }
    }
}

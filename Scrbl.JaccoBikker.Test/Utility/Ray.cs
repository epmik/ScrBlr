namespace Scrbl.JaccoBikker
{
    using System;

    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Ray
    {
        public Vector3f Origin;
        public Vector3f Direction;
        public float Time;

        public readonly Vector3f At(float t)
        {
            return Origin + Direction * t;
        }
    }

}

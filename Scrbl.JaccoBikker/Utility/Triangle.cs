namespace Scrbl.JaccoBikker
{
    using System;

    public class Triangle
    {
        public Vector3d A { get { return vertex0; } set { vertex0 = value; } }
        public Vector3d B { get { return vertex1; } set { vertex1 = value; } }
        public Vector3d C { get { return vertex2; } set { vertex2 = value; } }

        public Vector3d vertex0 = new Vector3d();
        public Vector3d vertex1 = new Vector3d();
        public Vector3d vertex2 = new Vector3d();
    }
}

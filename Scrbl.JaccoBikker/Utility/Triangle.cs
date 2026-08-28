namespace Scrbl.JaccoBikker
{
    using System;

    public class Triangle
    {
        public Vector3f A { get { return vertex0; } set { vertex0 = value; } }
        public Vector3f B { get { return vertex1; } set { vertex1 = value; } }
        public Vector3f C { get { return vertex2; } set { vertex2 = value; } }

        public Vector3f vertex0 = new Vector3f();
        public Vector3f vertex1 = new Vector3f();
        public Vector3f vertex2 = new Vector3f();
    }
}

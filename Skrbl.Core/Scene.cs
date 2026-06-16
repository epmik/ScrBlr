using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class Scene
    {
        public string Name { get; set; } = string.Empty;
        public List<Camera> Cameras { get; set; } = new List<Camera>();
        public List<Light> Lights { get; set; } = new List<Light>();
        public List<Mesh> Meshes { get; set; } = new List<Mesh>();
        //public List<Material> Materials { get; set; } = new List<Material>();

        public void Add(Mesh mesh)
        {
            Meshes.Add(mesh);
        }
    }
}

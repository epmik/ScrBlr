using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrblr.Learning
{
    internal static class AssimpSceneExtensions
    {
        public static uint TotalMeshVertexCount(this Assimp.Scene scene)
        {
            return (uint)scene.Meshes.Sum(o => o.VertexCount);
        }

        public static uint TotalMeshIndexCount(this Assimp.Mesh mesh)
        {
            return (uint)(mesh.Faces.Sum(o => o.IndexCount));
        }

        public static uint TotalMeshIndexCount(this Assimp.Scene scene)
        {
            return (uint)scene.Meshes.Sum(o => TotalMeshIndexCount(o));
        }
    }
}

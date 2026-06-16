
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using StbImageSharp;
using OpenTK.Mathematics;

namespace OpenGlTutorialOrg
{
    public static class VboIndexer
    {
        public static void IndexTriangles(
            Vector3[] in_vertices,
            Vector2[] in_uvs,
            Vector3[] in_normals,

            out ushort[] out_indices,
            out Vector3[] out_vertices,
            out Vector2[] out_uvs,
            out Vector3[] out_normals)
        {
            var out_indiceList = new List<ushort>();
            var out_verticeList = new List<Vector3>();
            var out_uvList = new List<Vector2>();
            var out_normalList = new List<Vector3>();

            for (var i = 0; i < in_vertices.Length; i++)
            {
                // Try to find a similar vertex in out_XXXX

                bool found = getSimilarVertexIndex(in_vertices[i], in_uvs[i], in_normals[i], out_verticeList, out_uvList, out_normalList, out ushort index);

                if (found)
                {
                    // A similar vertex is already in the VBO, use it instead !
                    out_indiceList.Add(index);
                }
                else
                {
                    // If not, it needs to be added in the output data.
                    out_verticeList.Add(in_vertices[i]);
                    out_uvList.Add(in_uvs[i]);
                    out_normalList.Add(in_normals[i]);
                    out_indiceList.Add((ushort)(out_verticeList.Count - 1));
                }
            }

            out_indices = out_indiceList.ToArray();
            out_vertices = out_verticeList.ToArray();
            out_uvs = out_uvList.ToArray();
            out_normals = out_normalList.ToArray();
        }

        static bool getSimilarVertexIndex(
            Vector3 in_vertex,
            Vector2 in_uv,
            Vector3 in_normal,
            List<Vector3> out_vertices,
            List<Vector2> out_uvs,
            List<Vector3> out_normals,
            out ushort index)
        {
            // Lame linear search
            for (ushort i = 0; i < out_vertices.Count; i++)
            {
                if (
                    is_near(in_vertex.X, out_vertices[i].X) &&
                    is_near(in_vertex.Y, out_vertices[i].Y) &&
                    is_near(in_vertex.Z, out_vertices[i].Z) &&
                    is_near(in_uv.X, out_uvs[i].X) &&
                    is_near(in_uv.Y, out_uvs[i].Y) &&
                    is_near(in_normal.X, out_normals[i].X) &&
                    is_near(in_normal.Y, out_normals[i].Y) &&
                    is_near(in_normal.Z, out_normals[i].Z)
                    )
                {
                    index = i;
                    return true;
                }
            }
            // No other vertex could be used instead.
            // Looks like we'll have to add it to the VBO.
            index = 0;
            return false;
        }

        // Returns true iif v1 can be considered equal to v2
        static bool is_near(float v1, float v2)
        {
            return MathF.Abs(v1 - v2) < 0.01f;
        }
    }
}   
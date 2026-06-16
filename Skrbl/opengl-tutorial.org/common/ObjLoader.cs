
using ObjParser;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using OpenTK.Mathematics;

namespace OpenGlTutorialOrg
{
    public static class ObjLoader
    {
        public static void Load(string path, out Vector3[] vertices, out Vector2[] uvs, out Vector3[] normals)
        {
            var obj = new ObjModel();

            obj.Load(path);

            vertices = new Vector3[obj.Faces.Count * 3];
            uvs = new Vector2[obj.Faces.Count * 3];
            normals = new Vector3[obj.Faces.Count * 3];

            var index = 0;
            var vertex = new ObjParser.Types.Vertex();
            var uv = new ObjParser.Types.TextureVertex();
            var normal = new ObjParser.Types.VertexNormal();

            foreach (var face in obj.Faces)
            {
                vertex = obj.Vertices[face.VertexIndexList[0] - 1];
                vertices[index] = new Vector3((float)vertex.X, (float)vertex.Y, (float)vertex.Z);

                vertex = obj.Vertices[face.VertexIndexList[1] - 1];
                vertices[index + 1] = new Vector3((float)vertex.X, (float)vertex.Y, (float)vertex.Z);

                vertex = obj.Vertices[face.VertexIndexList[2] - 1];
                vertices[index + 2] = new Vector3((float)vertex.X, (float)vertex.Y, (float)vertex.Z);



                uv = obj.TextureVertices[face.TextureVertexIndexList[0] - 1];
                uvs[index] = new Vector2((float)uv.X, (float)uv.Y);

                uv = obj.TextureVertices[face.TextureVertexIndexList[1] - 1];
                uvs[index + 1] = new Vector2((float)uv.X, (float)uv.Y);

                uv = obj.TextureVertices[face.TextureVertexIndexList[2] - 1];
                uvs[index + 2] = new Vector2((float)uv.X, (float)uv.Y);



                normal = obj.Normals[face.NormalIndexList[0] - 1];
                normals[index] = new Vector3((float)normal.I, (float)normal.J, (float)normal.K);

                normal = obj.Normals[face.NormalIndexList[1] - 1];
                normals[index + 1] = new Vector3((float)normal.I, (float)normal.J, (float)normal.K);

                normal = obj.Normals[face.NormalIndexList[2] - 1];
                normals[index + 2] = new Vector3((float)normal.I, (float)normal.J, (float)normal.K);

                index += 3;
            }
        }
    }
}   
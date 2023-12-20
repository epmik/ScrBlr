using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;

namespace Scrblr.Learning.Silk
{
    internal class ObjParser
    {

        public class ParseResult
        {
            public List<Vector4> Vertices = new List<Vector4>();
            public List<Vector3> Normals = new List<Vector3>();
            public List<Vector3> Uvws = new List<Vector3>();
            public List<Face> Faces = new List<Face>();

            public ExportScene ExportScene(ExportScene.Setting setting) 
            {
                var exportResult = new ExportScene();

                var mesh = new ExportScene.Mesh();

                exportResult.Meshes = new Silk.ExportScene.Mesh[] { mesh };

                foreach (var flag in setting.Flags)
                {
                    mesh.VertexSize += (uint)(flag == Scrblr.Learning.Silk.ExportScene.Flag.Uv0 ? 2 : 3);
                }

                var pool = new float[Faces.Count * 3][];
                var pool2 = new List<float>((int)(Faces.Count * 3 * mesh.VertexSize));
                var indices = new List<uint>();

                foreach (var face in Faces)
                {
                    foreach (var faceIndex in face.Indices)
                    {
                        var i = 0;
                        var temp = new float[mesh.VertexSize];

                        foreach (var flag in setting.Flags)
                        {
                            switch (flag)
                            {
                                case Scrblr.Learning.Silk.ExportScene.Flag.Vertices:
                                    var v = Vertices[faceIndex.VertexIndex - 1];
                                    temp[i++] = v.X;
                                    temp[i++] = v.Y;
                                    temp[i++] = v.Z;
                                    break;
                                case Scrblr.Learning.Silk.ExportScene.Flag.Normals:
                                    var n = Normals[faceIndex.NormalIndex - 1];
                                    temp[i++] = n.X;
                                    temp[i++] = n.Y;
                                    temp[i++] = n.Z;
                                    break;
                                case Scrblr.Learning.Silk.ExportScene.Flag.Uv0:
                                    var u = Uvws[faceIndex.UvwIndex - 1];
                                    temp[i++] = u.X;
                                    temp[i++] = u.Y;
                                    break;
                            }
                        }

                        i = 0;

                        var match = false;
                        var matchCount  = 0;

                        for (uint j = 0; j < mesh.VertexCount && match == false; j++)
                        {
                            match = false;
                            matchCount = 0;

                            for (var k = 0; k < mesh.VertexSize && match == false; k++)
                            {
                                if (pool[j][k] == temp[k])
                                {
                                    var b = pool2[(int)(j * mesh.VertexSize + k)] == temp[k];

                                    if(!b)
                                    {
                                        var y = 0;
                                    }

                                    matchCount++;
                                }
                            }

                            if(matchCount == mesh.VertexSize)
                            {
                                // found match
                                match = true;
                                indices.Add(j);
                            }
                        }

                        if (!match)
                        {
                            indices.Add(mesh.VertexCount);

                            for (var t = 0; t < temp.Length; t++)
                            {
                                //pool2[(int)(exportResult.VertexCount * exportResult.VertexSize + t)] = temp[t];
                                pool2.Add(temp[t]);
                            }

                            pool[mesh.VertexCount++] = temp;
                        }
                    }
                }

                if (!setting.Indexed)
                {

                }

                mesh.Vertices = pool2.ToArray();
                mesh.Indices = indices.ToArray();

                return exportResult;
            }
        }

        public class Face
        {
            public List<FaceIndex> Indices = new List<FaceIndex>();
        }

        public class FaceIndex
        {
            public int VertexIndex;
            public int NormalIndex;
            public int UvwIndex;
        }

        private StreamReader _streamReader;

        private ParseResult _parseResult;

        private ObjParser(Stream stream)
        {
            _streamReader = new StreamReader(stream);
        }

        private void ParseLine()
        {
            var currentLine = _streamReader.ReadLine();

            if (string.IsNullOrWhiteSpace(currentLine) || currentLine[0] == '#')
            {
                return;
            }

            var fields = currentLine.Trim().Split(null, 2);
            var keyword = fields[0].Trim();
            var data = fields[1].Trim();

            ParseLine(keyword, data);
        }

        private void ParseLine(string keyword, string data)
        {
            switch(keyword)
            {
                case "v":
                    ParseVertex(data);
                    break;
                case "vn":
                    ParseNormal(data);
                    break;
                case "vt":
                    ParseUvw(data);
                    break;
                case "f":
                    ParseFace(data);
                    break;
                case "g":
                    // Group name
                    ParseGroup(data);
                    break;
                case "mtlib":
                    break;
                case "usemtl":
                    break;
                case "o":
                    // Object name
                    break;
                case "s":
                    // Smoothing group
                    break;
                case "mg":
                    // Merging group
                    break;
            }
        }

        private void ParseVertex(string data)
        {
            string[] parts = data.Split(' ');

            float x = ParseInvariantFloat(parts[0]);
            float y = ParseInvariantFloat(parts[1]);
            float z = ParseInvariantFloat(parts[2]);

            float w = parts.Length == 4 ? ParseInvariantFloat(parts[3]) : 1f;

            _parseResult.Vertices.Add(new Vector4(x, y, z, w));
        }

        private void ParseUvw(string data)
        {
            string[] parts = data.Split(' ');

            float x = ParseInvariantFloat(parts[0]);
            float y = parts.Length > 1 ? ParseInvariantFloat(parts[1]) : 0f;
            float z = parts.Length > 2 ? ParseInvariantFloat(parts[2]) : 0f;

            _parseResult.Uvws.Add(new Vector3(x, y, z));
        }

        private void ParseNormal(string data)
        {
            string[] parts = data.Split(' ');

            float x = ParseInvariantFloat(parts[0]);
            float y = ParseInvariantFloat(parts[1]);
            float z = ParseInvariantFloat(parts[2]);

            _parseResult.Normals.Add(new Vector3(x, y, z));
        }

        public void ParseGroup(string data)
        {
            if("off".Equals(data, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
        }

        public void ParseFace(string data)
        {
            var vertices = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var face = new Face();

            foreach (var vertexString in vertices)
            {
                var faceIndex = ParseFaceIndex(vertexString);

                face.Indices.Add(faceIndex);
            }

            _parseResult.Faces.Add(face);
        }

        private FaceIndex ParseFaceIndex(string data)
        {
            var fields = data.Split(new[] { '/' }, StringSplitOptions.None);

            var vertexIndex = ParseInvariantInt(fields[0]);
            var faceVertex = new FaceIndex { VertexIndex = vertexIndex };

            if (fields.Length > 1)
            {
                var textureIndex = fields[1].Length == 0 ? 0 : ParseInvariantInt(fields[1]);
                faceVertex.UvwIndex = textureIndex;
            }

            if (fields.Length > 2)
            {
                var normalIndex = fields.Length > 2 && fields[2].Length == 0 ? 0 : ParseInvariantInt(fields[2]);
                faceVertex.NormalIndex = normalIndex;
            }

            return faceVertex;
        }
        public static float ParseInvariantFloat(string floatString)
        {
            return float.Parse(floatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        public static int ParseInvariantInt(string intString)
        {
            return int.Parse(intString, CultureInfo.InvariantCulture.NumberFormat);
        }

        public static bool EqualsOrdinalIgnoreCase(string str, string s)
        {
            return str.Equals(s, StringComparison.OrdinalIgnoreCase);
        }

        public static ParseResult Parse(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return Parse(stream);
            }
        }

        public static ParseResult Parse(Stream stream)
        {
            var reader = new ObjParser(stream);

            reader._parseResult = new ParseResult();

            while (!reader._streamReader.EndOfStream)
            {
                reader.ParseLine();
            }

            return reader._parseResult;
        }
    }
}

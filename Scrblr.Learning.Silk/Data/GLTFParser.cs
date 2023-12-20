using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silk.NET.Input;
using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Scrblr.Learning.Silk
{
    internal class GLTFObject
    {
        public class FileHeader
        {
            public UInt32 Magic;
            public UInt32 Version;
            public UInt32 Length;
        }


        public class StructureContent
        {
            public Scene[] scenes;
            public Node[] nodes;
            public Camera[] cameras;
            public Material[] materials;
            public Mesh[] meshes;
            public Accessor[] accessors;
            public BufferView[] bufferViews;
            public Buffer[] buffers;
        }

        public class Accessor
        {
            public int bufferView;
            public int byteOffset;
            public int componentType;
            public int count;
            public string type;
            public string name;

            public bool Normalized { get; internal set; }
        }

        public class Scene
        {
            public string name;
            public int[] nodes;
        }

        public class Node
        {
            public int? mesh;
            public int? camera;
            public int[] children;
            public int? skin;
            public string name;
            public float[] translation = new float[] { 0, 0, 0 };
            public float[] scale = new float[] { 0, 0, 0 };
            public float[] rotation = new float[] { 0, 0, 0, 0 };
            public float[] matrix = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        public class Primitive
        {
            public string name;
            public Dictionary<string, int> attributes;
            //public Attributes attributes;
        }

        public class Attributes
        {
            public int POSITION;
            public int TEXCOORD_0;
            public int NORMAL;
        }

        public class Camera
        {
            public string name;
        }

        public class Mesh
        {
            public string name;
            public Primitive[] primitives;
        }

        public class Material
        {
            public string name;
        }

        public class Buffer
        {
            public string uri;
            public int byteLength;
            public string name;
        }

        public class BufferView
        {
            public int buffer;
            public int byteLength;
            public int byteOffset;
            public int target;
            public string name;
        }

        public FileHeader? Header;

        public StructureContent? Content;

        public Dictionary<string, byte[]> BinaryData = new Dictionary<string, byte[]>();

        public void AddBinaryData(string name, byte[] data)
        {
            BinaryData.Add(name, data);
        }
    }

    internal static class GLTFFileExtensions
    {
        public static ExportScene ExportScene(this GLTFObject gltfObject, ExportScene.Setting setting)
        {
            var scene = new ExportScene();

            foreach(var n in gltfObject.Content.nodes.Where(o => o.mesh != null))
            {
                var m = gltfObject.Content.meshes[n.mesh.Value];

                var p = m.primitives[0];

                foreach(var flag in setting.Flags)
                {
                    var key = AttributeKeyForFlag(flag);

                    if(!p.attributes.ContainsKey(key))
                    {
                        continue;
                    }

                    var index = p.attributes[key];

                    var a = gltfObject.Content.accessors[index];

                    var b = gltfObject.Content.bufferViews[a.bufferView];


                }
            }


            return scene;
        }

        private static string AttributeKeyForFlag(ExportScene.Flag flag)
        {
            switch(flag)
            {
                case Silk.ExportScene.Flag.Vertices:
                    return "POSITION";
                case Silk.ExportScene.Flag.Uv0:
                    return "TEXCOORD_0";
                case Silk.ExportScene.Flag.Normals:
                    return "NORMAL";
                default:
                    throw new NotImplementedException();
            }
        }
    }

    internal class GLTFParser
    {

        // https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#glb-file-format-specification-structure

        private GLTFObject _file;
        private BinaryReader _binaryReader;
        private Chunk _structuredJSONContentChunk;
        private Chunk _binaryBufferChunk;
        private string _path;

        public class Chunk
        {
            public UInt32 Length;
            public UInt32 Type;
            public Byte[] Data;
        }

        public GLTFParser(string path)
        {
            _path = path;
            _binaryReader = new BinaryReader(File.Open(path, FileMode.Open), Encoding.UTF8, false);
            _file = new GLTFObject();
        }

        public static GLTFObject Parse(string path)
        {
            var parser = new GLTFParser(path);

            return parser.Parse();
        }

        private const uint MagicHeaderIdentifier = 0x46546C67;
        private const uint StructuredJSONContentChunkTypeIdentifier = 0x4E4F534A;
        private const uint BinaryBufferChunkTypeIdentifier = 0x004E4942;

        private GLTFObject Parse()
        {
            using (_binaryReader)
            {
                _file.Header = ParseFileHeader();

                if(_file.Header != null)
                {
                    // binary file

                    ParseChunks();

                    _file.AddBinaryData("@default", _binaryBufferChunk.Data);
                }
                else
                {
                    _binaryReader.BaseStream.Position = 0;

                    _structuredJSONContentChunk = new Chunk();

                    // is the length of chunkData, in bytes.
                    _structuredJSONContentChunk.Length = (uint)_binaryReader.BaseStream.Length;

                    // indicates the type of chunk. See https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#table-chunktypes for details.
                    _structuredJSONContentChunk.Type = StructuredJSONContentChunkTypeIdentifier;

                    // is the binary payload of the chunk.
                    // Client implementations MUST ignore chunks with unknown types to enable glTF extensions to reference additional
                    // chunks with new types following the first two chunks.
                    _structuredJSONContentChunk.Data = new byte[_structuredJSONContentChunk.Length];

                    _binaryReader.Read(_structuredJSONContentChunk.Data, 0, (int)_structuredJSONContentChunk.Length);
                }

                _file.Content = JsonConvert.DeserializeObject<GLTFObject.StructureContent>(Encoding.UTF8.GetString(_structuredJSONContentChunk.Data));

                var root = Path.GetDirectoryName(_path);

                foreach(var buffer in _file.Content.buffers)
                {
                    if(string.IsNullOrEmpty(buffer.uri) || _file.BinaryData.ContainsKey(buffer.uri))
                    {
                        continue;
                    }

                    _binaryBufferChunk = new Chunk();

                    // is the length of chunkData, in bytes.
                    _binaryBufferChunk.Length = (uint)buffer.byteLength;

                    // indicates the type of chunk. See https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#table-chunktypes for details.
                    _binaryBufferChunk.Type = BinaryBufferChunkTypeIdentifier;

                    // is the binary payload of the chunk.
                    // Client implementations MUST ignore chunks with unknown types to enable glTF extensions to reference additional
                    // chunks with new types following the first two chunks.
                    _binaryBufferChunk.Data = new byte[buffer.byteLength];

                    using (var bufferReader = new BinaryReader(File.Open(Path.Combine(root, buffer.uri), FileMode.Open), Encoding.UTF8, false))
                    {
                        bufferReader.Read(_binaryBufferChunk.Data, 0, buffer.byteLength);
                    }

                    _file.AddBinaryData(buffer.uri, _binaryBufferChunk.Data);
                }

                return _file;
            }
        }

        private void ParseChunks()
        {
            while(_binaryReader.BaseStream.Position < _binaryReader.BaseStream.Length)
            {
                Align();

                var chunk = ParseChunk();

                switch(chunk.Type)
                {
                    case StructuredJSONContentChunkTypeIdentifier:
                        _structuredJSONContentChunk = chunk;
                        break;
                    case BinaryBufferChunkTypeIdentifier:
                        _binaryBufferChunk = chunk;
                        break;
                }
            }
        }

        private Chunk ParseChunk()
        {
            var chunk = new Chunk();

            // is the length of chunkData, in bytes.
            chunk.Length = _binaryReader.ReadUInt32();

            // indicates the type of chunk. See https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#table-chunktypes for details.
            chunk.Type = _binaryReader.ReadUInt32();

            // is the binary payload of the chunk.
            // Client implementations MUST ignore chunks with unknown types to enable glTF extensions to reference additional
            // chunks with new types following the first two chunks.
            chunk.Data = new byte[chunk.Length];

            _binaryReader.Read(chunk.Data, 0, (int)chunk.Length);

            return chunk;
        }

        private GLTFObject.FileHeader? ParseFileHeader()
        {
            var header = new GLTFObject.FileHeader();

            header.Magic = _binaryReader.ReadUInt32();    // 0x46546C67 or ASCII string glTF

            if (MagicHeaderIdentifier != header.Magic)
            {
                return null;
            }

            header.Version = _binaryReader.ReadUInt32();
            header.Length = _binaryReader.ReadUInt32();

            //var glTF = Encoding.ASCII.GetString(BitConverter.GetBytes(_file.Header.Magic));

            return header;
        }

        private void Align(int byteBoundary = 4)
        {
            while(_binaryReader.BaseStream.Position % byteBoundary != 0)
            {
                _binaryReader.BaseStream.Seek(1, SeekOrigin.Current);
            }
        }
    }
}

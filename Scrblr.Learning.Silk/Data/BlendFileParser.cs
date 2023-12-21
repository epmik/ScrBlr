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

namespace Scrblr.Learning
{
    internal class BlendFile
    {

        public class FileHeader
        {
            public string Identifier;
            public Int32 PointerSize;
            public Endianness Endianness;
            public Decimal Version;
        }

        public class FileBlock
        {
            public FileBlockHeader Header;

            public override string ToString()
            {
                return Header.ToString();
            }
        }

        public class Structure
        {
            public Int16 TypeIndex;

            public string TypeName;

            public List<Field> Fields;
        }

        public class Field
        {
            public Int16 TypeIndex;

            public string TypeName;

            public Int16 NameIndex;

            public string Name;
        }

        public class StructureDNA
        {
            public string Identifier;

            public List<string> Names;

            public List<string> Types;

            public List<Int16> Lengths;

            public List<Structure> Structures;

            public override string ToString()
            {
                return Identifier.ToString();
            }
        }

        public FileBlock? FindFileBlock(string name)
        {
            return FileBlocks.SingleOrDefault(o => o.Header.Identifier.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public Structure? FindStructure(string name)
        {
            return SDNA.Structures.SingleOrDefault(o => o.TypeName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public Structure? FindStructure(int index)
        {
            return SDNA.Structures[index];
        }

        public class FileBlockHeader
        {
            public String Identifier;
            public Int32 Size;
            public Int32 SdnaIndex;
            public Int32 Count;
            public Int64 Address;

            public override string ToString()
            {
                return Identifier;
            }
        }

        public enum Endianness
        {
            BigEndian = 0,
            LittleEndian = 1
        }

        public FileHeader Header { get; set; }

        public List<FileBlock> FileBlocks { get; set; }

        public StructureDNA SDNA { get; set; }
    }

    internal class BlendFileParser
    {

        // https://wiki.blender.jp/Dev:Source/Architecture/File_Format
        // https://github.com/snoozbuster/blender-file-reader/blob/master/BlenderFileReader/BlenderFile.cs
        // https://www.blendernation.com/2009/05/07/the-mystery-of-the-blend-the-blender-file-format-explained/
        // https://projects.blender.org/blender/blender/src/branch/main/doc/blender_file_format/mystery_of_the_blend.html

        // https://blenderfilereaderincsharp.blogspot.com/

        private static ASCIIEncoding DefaultStringEnc = new ASCIIEncoding();

        private Stream stream;

        private const string FileBlockHeaderIdenfierDNA1 = "DNA1";
        private const string FileBlockHeaderIdenfierENDB = "ENDB";

        private BlendFileParser(Stream stream)
        {
            this.stream = stream;
        }

        public static BlendFile Parse(Stream stream)
        {
            var reader = new BlendFileParser(stream);

            var blenderFile = new BlendFile
            {
                FileBlocks = new List<BlendFile.FileBlock>(),
            };

            blenderFile.Header = reader.ParseFileHeader(blenderFile);

            BlendFile.FileBlock? fileBlock = null;

            do
            {
                fileBlock = reader.ParseFileBlock(blenderFile);

                blenderFile.FileBlocks.Add(fileBlock);

                switch (fileBlock.Header.Identifier)
                {
                    case FileBlockHeaderIdenfierDNA1:
                        reader.ParseStructureDNA(blenderFile, fileBlock);
                        break;
                    default:
                        reader.Skip(fileBlock);
                        break;
                }
            }
            while (fileBlock != null && fileBlock.Header.Identifier != FileBlockHeaderIdenfierENDB);

            return blenderFile;
        }

        private void ParseStructureDNA(BlendFile blenderFile, BlendFile.FileBlock fileBlock)
        {
            blenderFile.SDNA = new BlendFile.StructureDNA();

            // Identifier of the file-block
            blenderFile.SDNA.Identifier = ParseString(stream, 4).TrimZeroTerminator();

            ParseStructureDNANames(blenderFile, fileBlock);

            Align();

            ParseStructureDNATypes(blenderFile, fileBlock);

            Align();

            ParseStructureDNALengths(blenderFile, fileBlock);

            Align();

            ParseStructureDNAStructures(blenderFile, fileBlock);
        }

        private void ParseStructureDNAStructures(BlendFile blenderFile, BlendFile.FileBlock fileBlock)
        {
            // STRC
            var name = ParseString(stream, 4);

            var total = ParseInt32(stream, blenderFile.Header.Endianness);

            blenderFile.SDNA.Structures = new List<BlendFile.Structure>();

            for (var i = 0; i < total; i++)
            {
                var structure = new BlendFile.Structure { Fields = new List<BlendFile.Field>() };

                blenderFile.SDNA.Structures.Add(structure);

                structure.TypeIndex = ParseInt16(stream, blenderFile.Header.Endianness);
                structure.TypeName = blenderFile.SDNA.Names[structure.TypeIndex];

                var fieldstotal = ParseInt16(stream, blenderFile.Header.Endianness);

                for (var j = 0; j < fieldstotal; j++)
                {
                    var field = new BlendFile.Field();

                    structure.Fields.Add(field);

                    field.TypeIndex = ParseInt16(stream, blenderFile.Header.Endianness);
                    field.TypeName = blenderFile.SDNA.Names[field.TypeIndex];
                    
                    field.NameIndex = ParseInt16(stream, blenderFile.Header.Endianness);
                    field.Name = blenderFile.SDNA.Names[field.NameIndex];
                }
            }
        }

        private void ParseStructureDNALengths(BlendFile blenderFile, BlendFile.FileBlock fileBlock)
        {
            // TLEN
            var name = ParseString(stream, 4);

            blenderFile.SDNA.Lengths = new List<Int16>();

            for (var i = 0; i < blenderFile.SDNA.Types.Count; i++)
            {
                blenderFile.SDNA.Lengths.Add(ParseInt16(stream, blenderFile.Header.Endianness));
            }
        }

        private void ParseStructureDNANames(BlendFile blenderFile, BlendFile.FileBlock fileBlock)
        {
            // NAME
            var name = ParseString(stream, 4);

            var total = ParseInt32(stream, blenderFile.Header.Endianness);

            blenderFile.SDNA.Names = new List<string>();

            for (var i = 0; i < total; i++)
            {
                blenderFile.SDNA.Names.Add(ParseZeroTerminatedString(stream));
            }
        }

        private void ParseStructureDNATypes(BlendFile blenderFile, BlendFile.FileBlock fileBlock)
        {
            // TYPE
            var name = ParseString(stream, 4);

            var total = ParseInt32(stream, blenderFile.Header.Endianness);

            blenderFile.SDNA.Types = new List<string>();

            for (var i = 0; i < total; i++)
            {
                blenderFile.SDNA.Types.Add(ParseZeroTerminatedString(stream));
            }
        }

        private void Align(int bytes = 4)
        {
            while(stream.Position % 4 != 0)
            {
                stream.Seek(1, SeekOrigin.Current);
            }
        }

        public static BlendFile Parse(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return Parse(stream);
            }
        }

        private BlendFile.FileHeader ParseFileHeader(BlendFile blenderFile)
        {
            var header = new BlendFile.FileHeader();

            // File identifier (always 'BLENDER')
            header.Identifier = ParseString(stream, 7);
            // Size of a pointer
            // All pointers in the file are stored in this format
            // '_'(underscore) means 4 bytes or 32 bit
            // '-'(minus) means 8 bytes or 64 bits.
            header.PointerSize = ParsePointerSize();
            // Type of byte ordering used
            // 'v' means little endian
            // 'V' means big endian.
            header.Endianness = ParseEndianness();
            // Version of Blender the file was created in
            // for Example '248' means version 2.48            
            header.Version = ParseVersion();

            return header;
        }

        private BlendFile.FileBlockHeader ParseFileBlockHeader(BlendFile blenderFile)
        {
            var header = new BlendFile.FileBlockHeader();

            // Identifier of the file-block
            header.Identifier = ParseString(stream, 4).TrimZeroTerminator();
            // Total length of the data
            // after the file - block - header
            header.Size = ParseInt32(stream, blenderFile.Header.Endianness);
            // skip
            // Memory address
            // pointer to where the structure
            // was located when written to disk
            header.Address = blenderFile.Header.PointerSize == 4 ? ParseInt32(stream, blenderFile.Header.Endianness) : ParseInt64(stream, blenderFile.Header.Endianness);
            // Index of the SDNA structure
            header.SdnaIndex = ParseInt32(stream, blenderFile.Header.Endianness);
            // Number of structure located
            // in this file - block
            header.Count = ParseInt32(stream, blenderFile.Header.Endianness);

            return header;
        }

        private BlendFile.FileBlock ParseFileBlock(BlendFile blenderFile)
        {
            var block = new BlendFile.FileBlock();

            block.Header = ParseFileBlockHeader(blenderFile);

            //Skip(stream, block.Header.Size);

            return block;
        }

        private static String ParseString(Stream stream, Int32 length)
        {
            String retValue;
            Int32 leftToRead = length;
            Byte[] tmpByteArray = new Byte[length];
            while (leftToRead > 0)
            {
                Int32 readBytes = stream.Read(tmpByteArray, length - leftToRead, length);
                leftToRead -= readBytes;
            }
            retValue = DefaultStringEnc.GetString(tmpByteArray);
            return retValue;
        }

        private static StringBuilder _zeroTerminatedStringBuilder = new StringBuilder();
        private static Byte[] _zeroTerminatedByteArray = new Byte[1];

        private static String ParseZeroTerminatedString(Stream stream)
        {
            _zeroTerminatedStringBuilder.Clear();

            while (true)
            {
                stream.Read(_zeroTerminatedByteArray, 0, 1);

                //c = Encoding.ASCII.GetChars(Data, position++, 1)[0];

                if(_zeroTerminatedByteArray[0] == '\0')
                {
                    break;
                }

                _zeroTerminatedStringBuilder.Append((Char)_zeroTerminatedByteArray[0]);
            } ;

            return _zeroTerminatedStringBuilder.ToString();
        }

        private static Int32 ParseInt32(Stream stream, BlendFile.Endianness endianType)
        {
            Int32 length = 4;
            Int32 leftToRead = length;
            Byte[] buffer = new Byte[length];
            while (leftToRead > 0)
            {
                Int32 readBytes = stream.Read(buffer, length - leftToRead, length);
                leftToRead -= readBytes;
            }
            return BitConverter.ToInt32(buffer, 0);
        }

        private static Int16 ParseInt16(Stream stream, BlendFile.Endianness endianType)
        {
            Int32 length = 2;
            Int32 leftToRead = length;
            Byte[] buffer = new Byte[length];
            while (leftToRead > 0)
            {
                Int32 readBytes = stream.Read(buffer, length - leftToRead, length);
                leftToRead -= readBytes;
            }
            return BitConverter.ToInt16(buffer, 0);
        }

        private static Int64 ParseInt64(Stream stream, BlendFile.Endianness endianType)
        {
            Int32 length = 8;
            Int32 leftToRead = length;
            Byte[] buffer = new Byte[length];
            while (leftToRead > 0)
            {
                Int32 readBytes = stream.Read(buffer, length - leftToRead, length);
                leftToRead -= readBytes;
            }
            return BitConverter.ToInt64(buffer, 0);
        }

        private Int32 ParsePointerSize()
        {
            switch (ParseString(stream, 1))
            {
                case "_":
                    return 32;
                case "-":
                    return 64;
                default:
                    return 0;
            }
        }

        private BlendFile.Endianness ParseEndianness()
        {
            switch (ParseString(stream, 1))
            {
                case "v":
                    return BlendFile.Endianness.LittleEndian;
                case "V":
                    return BlendFile.Endianness.BigEndian;
                default:
                    throw new Exception();
            }
        }

        private Decimal ParseVersion()
        {
            String version = ParseString(stream, 3);
            String decSep = System.Globalization.CultureInfo.InstalledUICulture.NumberFormat.NumberDecimalSeparator;
            String completeVersionString = version.Substring(0, 1) + decSep + version.Substring(1);
            return Decimal.Parse(completeVersionString);
        }

        private void Skip(BlendFile.FileBlock fileBlock)
        {
            stream.Seek(fileBlock.Header.Size, SeekOrigin.Current);
        }
    }

    internal static class BlendFileParserExtensions
    {
        private static StringBuilder _trimZeroTerminatorStringBuilder = new StringBuilder();

        internal static string TrimZeroTerminator(this string text)
        {
            _trimZeroTerminatorStringBuilder.Clear();

            foreach(var c in text)
            {
                if(c == '\0')
                {
                    continue;
                }
                _trimZeroTerminatorStringBuilder.Append(c);
            }

            return _trimZeroTerminatorStringBuilder.ToString();
        }
    }
}

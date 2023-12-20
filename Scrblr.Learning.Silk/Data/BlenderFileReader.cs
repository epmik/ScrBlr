using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Scrblr.Learning.Silk.Import.BlenderFile;
using static Scrblr.Learning.Silk.Import.BlenderFileReader;

namespace Scrblr.Learning.Silk.Import
{
    internal class BlenderFile
    {

        public class FileHeader
        {
            public String Identifier;
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

        public class FileBlockHeader
        {
            public String Identifier;
            public Int32 Size;
            public Int32 SdnaIndex;
            public Int32 Count;

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
    }

    internal class BlenderFileReader
    {

        // https://wiki.blender.jp/Dev:Source/Architecture/File_Format
        // https://blenderfilereaderincsharp.blogspot.com/
        // https://github.com/snoozbuster/blender-file-reader/blob/master/BlenderFileReader/BlenderFile.cs
        // https://www.blendernation.com/2009/05/07/the-mystery-of-the-blend-the-blender-file-format-explained/

        private static ASCIIEncoding DefaultStringEnc = new ASCIIEncoding();

        private Stream stream;

        private static String ReadString(Stream stream, Int32 length)
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
        private static Int32 ReadInteger(Stream stream, Endianness endianType)
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

        private static Int16 ReadShort(Stream stream, Endianness endianType)
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


        private static Int64 ReadLong(Stream stream, Endianness endianType)
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

        private Int32 ReadPointerSize()
        {
            switch (ReadString(stream, 1))
            {
                case "_":
                    return 32;
                case "-":
                    return 64;
                default:
                    return 0;
            }
        }

        private BlenderFile.Endianness ReadEndianness()
        {
            switch (ReadString(stream, 1))
            {
                case "v":
                    return BlenderFile.Endianness.LittleEndian;
                case "V":
                    return BlenderFile.Endianness.BigEndian;
                default:
                    throw new Exception();
            }
        }

        private Decimal ReadVersion()
        {
            String version = ReadString(stream, 3);
            String decSep = System.Globalization.CultureInfo.InstalledUICulture.NumberFormat.NumberDecimalSeparator;
            String completeVersionString = version.Substring(0, 1) + decSep + version.Substring(1);
            return Decimal.Parse(completeVersionString);
        }

        private FileHeader ReadFileHeader(BlenderFile blenderFile)
        {
            var header = new FileHeader();

            // File identifier (always 'BLENDER')
            header.Identifier = ReadString(stream, 7);
            // Size of a pointer
            // All pointers in the file are stored in this format
            // '_'(underscore) means 4 bytes or 32 bit
            // '-'(minus) means 8 bytes or 64 bits.
            header.PointerSize = ReadPointerSize();
            // Type of byte ordering used
            // 'v' means little endian
            // 'V' means big endian.
            header.Endianness = ReadEndianness();
            // Version of Blender the file was created in
            // for Example '248' means version 2.48            
            header.Version = ReadVersion();

            return header;
        }

        private FileBlockHeader ReadFileBlockHeader(BlenderFile blenderFile)
        {
            var header = new FileBlockHeader();

            // Identifier of the file-block
            header.Identifier = ReadString(stream, 4);
            // Total length of the data
            // after the file - block - header
            header.Size = ReadInteger(stream, blenderFile.Header.Endianness);
            // skip
            // Memory address
            // pointer to where the structure
            // was located when written to disk
            Skip(stream, blenderFile.Header.PointerSize == 32 ? 4 : 8);
            // Index of the SDNA structure
            header.SdnaIndex = ReadInteger(stream, blenderFile.Header.Endianness);
            // Number of structure located
            // in this file - block
            header.Count = ReadInteger(stream, blenderFile.Header.Endianness);

            return header;
        }

        private FileBlock ReadFileBlock(BlenderFile blenderFile)
        {
            var block = new FileBlock();

            block.Header = ReadFileBlockHeader(blenderFile);

            Skip(stream, block.Header.Size);

            return block;
        }

        private void Skip(Stream stream, long size)
        {
            stream.Seek(size, SeekOrigin.Current);
        }

        private BlenderFileReader(Stream stream)
        {
            this.stream = stream;
        }

        public static BlenderFile Parse(string path)
        {
            using(var stream = File.OpenRead(path))
            {
                return Parse(stream);
            }
        }

        private const string FileBlockHeaderIdenfierEndB = "ENDB";

        public static BlenderFile Parse(Stream stream)
        {
            var reader = new BlenderFileReader(stream);

            var blenderFile = new BlenderFile();

            blenderFile.Header = reader.ReadFileHeader(blenderFile);

            FileBlock fileBlock = null;

            do
            {
                fileBlock = reader.ReadFileBlock(blenderFile);
            }
            while(fileBlock != null && fileBlock.Header.Identifier != FileBlockHeaderIdenfierEndB);

            //while ((identifier = BinaryFileHelper.ReadString(inStream, 4)) != "ENDB")
            //{
            //    // Read size
            //    switch (identifier)
            //    {
            //        default:
            //            {
            //                Console.Write("|" + identifier);
            //                UnknownIdentifier unknown = new UnknownIdentifier(inStream, blenderHeader.Endian, blenderHeader.PointerSize);
            //                Console.Write(" - " + unknown.Index);
            //                break;
            //            }
            //    }

            //}

            return blenderFile;
        }
    }
}

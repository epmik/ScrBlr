using System;

namespace Scrblr.Core
{
    public class Tessalator
    {
        private static Tessalator Default = new Tessalator();

        public Tessalator()
        {

        }

        public static void Tessalate(Geometry geometry)
        {
            switch(geometry.GeometryType)
            {
                case GeometryType.Lines:
                case GeometryType.LineStrip:
                case GeometryType.LineLoop:
                    TessalateLines(geometry);
                    break;
                default:
                    throw new NotImplementedException($"Tessalate(Geometry geometry) failed. found unknown GeometryType: {geometry.GeometryType}");
            }
        }

        private static void TessalateLines(Geometry geometry)
        {


            //InsertRenderChunk(Shader, PositionColorVertexBuffer, ModelMatrix, GeometryType.LineLoop, PositionColorVertexBuffer.UsedElements(), 4);

            //PositionColorVertexBuffer.Bind();

            //for (var i = 0; i < coordinates.Length;)
            //{
            //    PositionColorVertexBuffer.Write(new[] { coordinates[i++], coordinates[i++], 0f });
            //    PositionColorVertexBuffer.Write(_fillColor);
            //}

        }
    }
}
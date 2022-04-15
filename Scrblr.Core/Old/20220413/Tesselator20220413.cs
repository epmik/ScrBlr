using System;

namespace Scrblr.Core
{
    public class Tesselator20220413
    {
        private static Tesselator20220413 Default = new Tesselator20220413();

        public Tesselator20220413()
        {

        }

        public static void Tessalate(Geometry20220413 geometry)
        {
            switch(geometry.GeometryType)
            {
                case GeometryType20220413.Lines:
                case GeometryType20220413.LineStrip:
                case GeometryType20220413.LineLoop:
                    TessalateLines(geometry);
                    break;
                default:
                    throw new NotImplementedException($"Tessalate(Geometry geometry) failed. found unknown GeometryType: {geometry.GeometryType}");
            }
        }

        private static void TessalateLines(Geometry20220413 geometry)
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
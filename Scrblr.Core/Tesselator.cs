using System;

namespace Scrblr.Core
{
    public class Tesselator
    {
        #region Fields and Properties

        private static Tesselator Default = new Tesselator();

        #endregion Fields and Properties

        #region Constructors

        public Tesselator()
        {

        }

        #endregion Constructors

        public static void Tessalate(Geometry geometry)
        {
            switch(geometry.GeometryType)
            {
                case GeometryType.Vertex:
                case GeometryType.VertexFan:
                case GeometryType.VertexStrip:
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
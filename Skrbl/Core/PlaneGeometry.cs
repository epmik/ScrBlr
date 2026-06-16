using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class PlaneGeometry : BufferGeometry
    {
        // https://github.com/mrdoob/three.js/blob/master/src/geometries/PlaneGeometry.js

        public float Width = 1f;
        public float Height = 1f;
        public int WidthSegments = 1;
        public int HeightSegments = 1;

        public PlaneGeometry(float width = 1f, float height = 1f, int widthSegments = 1, int heightSegments = 1)
        {
            Width = width;
            Height = height;
            WidthSegments = widthSegments;
            HeightSegments = heightSegments;

            GenerateGeometry();
        }

        private void GenerateGeometry()
        {
            // https://github.com/mrdoob/three.js/blob/master/src/geometries/PlaneGeometry.js

            var width_half = Width / 2f;
            var height_half = Height / 2f;

            var gridX = WidthSegments;
            var gridY = HeightSegments;

            var gridX1 = gridX + 1;
            var gridY1 = gridY + 1;

            var segment_width = Width / gridX;
            var segment_height = Height / gridY;

            //

            var indices = new UintArray(gridY * gridX * 6);
            var vertices = new FloatArray(gridY1* gridX1 * 3);
            var normals = new FloatArray(gridY1 * gridX1 * 3);
            var uvs = new FloatArray(gridY1 * gridX1 * 2);

            for (var iy = 0; iy < gridY1; iy++)
            {
                var y = iy * segment_height - height_half;

                for (var ix = 0; ix < gridX1; ix++)
                {
                    var x = ix * segment_width - width_half;

                    vertices.Add(x, -y, 0f);

                    normals.Add(0f, 0f, 1f);

                    uvs.Add(ix / gridX);
                    uvs.Add(1 - (iy / gridY));
                }
            }

            for (var iy = 0; iy < gridY; iy++)
            {
                for (var ix = 0; ix < gridX; ix++)
                {
                    var a = ix + gridX1 * iy;
                    var b = ix + gridX1 * (iy + 1);
                    var c = (ix + 1) + gridX1 * (iy + 1);
                    var d = (ix + 1) + gridX1 * iy;

                    indices.Add(a, b, d);
                    indices.Add(b, c, d);
                }
            }

            AddIndex(indices);
            AddAttribute("position", new Float32BufferAttribute(vertices, 3));
            AddAttribute("normal", new Float32BufferAttribute(normals, 3));
            AddAttribute("uv", new Float32BufferAttribute(uvs, 2));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class LineGeometry : BufferGeometry
    {
        private const int DefaultNumberOfLines = 8192;

        public int NumberOfLines;

        public LineGeometry(int numberOfLines = DefaultNumberOfLines)
        {
            NumberOfLines = numberOfLines;
        }

        public void Add(IEnumerable<float> positions, IEnumerable<float> colors)
        {
            AddAttribute("position", new Float32BufferAttribute(positions, 3));
            AddAttribute("color", new Float32BufferAttribute(colors, 4));
        }
    }
}

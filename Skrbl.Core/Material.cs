using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class Material
    {
        public static Material Default { get; } = new Material();

        public string VertexShaderUri { get; set; } = string.Empty;

        public string FragmentShaderUri { get; set; } = string.Empty;
    }
}

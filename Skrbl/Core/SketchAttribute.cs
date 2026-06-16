using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    [System.AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SketchAttribute : System.Attribute
    {
        public string Name { get; set; }
    }
}

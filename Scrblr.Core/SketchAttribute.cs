using System;
using System.Collections.Generic;
using System.Text;

namespace Scrblr.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SketchAttribute : Attribute
    {
        public string Name { get; set; }
    }
}

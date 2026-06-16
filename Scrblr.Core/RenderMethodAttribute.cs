using System;
using System.Collections.Generic;
using System.Text;

namespace Scrblr.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RenderMethodAttribute : Attribute
    {
        public bool First { get; set; }
        public int? Index { get; set; }
    }
}

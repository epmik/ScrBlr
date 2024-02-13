using System;
using System.Linq;
using System.Reflection;

namespace Scrblr.Core
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property)]
    public class ShaderBindAttribute : Attribute
    {
        public string InputName { get; set; }
        public int Location { get; set; }
        public string UniformName { get; set; }
        public string Uid { get; set; }
    }
}
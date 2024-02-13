using System;
using System.Linq;
using System.Reflection;

namespace Scrblr.Core
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property)]
    public class ElementCountAttribute : Attribute
    {
        public uint Count { get; set; }
    }
}
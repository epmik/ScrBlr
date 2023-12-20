using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scrblr.Core
{
    public interface ISketch : IDisposable
    {
        public void Run(bool dispose = true);
    }
}

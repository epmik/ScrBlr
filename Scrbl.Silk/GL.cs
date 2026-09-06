using Silk.NET.Core.Contexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scrbl.Silk
{
    public class GL
    {
        static Create(IGLContextSource context) 
        {
            GL = context.CreateOpenGL();
        }
    }
}

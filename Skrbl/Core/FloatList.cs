using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class FloatList : List<float>
    {
        public void Add(params float[] values)
        {
            foreach (var v in values)
            {
                this.Add(v);
            }
        }
    }
}

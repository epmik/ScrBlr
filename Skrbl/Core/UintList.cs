using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class UintList : List<uint>
    {
        public void Add(params uint[] values)
        {
            this.AddRange(values);
        }

        public void Add(params int[] values)
        {
            foreach(var v in values)
            {
                Add((uint)v);
            }
        }
    }
}

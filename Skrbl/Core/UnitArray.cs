using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class UintArray : IEnumerable<uint>
    {
        public int Length { get { return Values.Length; } }
        public int Index;
        public uint[] Values;
        
        public UintArray(int size)
        {
            Values = new uint[size];
        }

        public void Add(params int[] values)
        {
            foreach (var v in values)
            {
                Values[Index++] = (uint)v;
            }
        }

        public void Add(params uint[] values)
        {
            foreach (var v in values)
            {
                Values[Index++] = v;
            }
        }

        public IEnumerator<uint> GetEnumerator()
        {
            return ((IEnumerable<uint>)Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class FloatArray : IEnumerable<float>
    {
        public int Length { get { return Values.Length; } }
        public int Index;
        public float[] Values;
        
        public FloatArray(int size)
        {
            Values = new float[size];
        }

        public void Add(params float[] values)
        {
            foreach (var v in values)
            {
                Values[Index++] = v;
            }
        }

        public IEnumerator<float> GetEnumerator()
        {
            return ((IEnumerable<float>)Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }
    }
}

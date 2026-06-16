using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class Float32BufferAttribute : AbstractBufferAttribute<float>
    {
        // https://github.com/mrdoob/three.js/blob/master/src/core/BufferAttribute.js#L1014

        public int ItemCount;
        public bool Normalized;

        public Float32BufferAttribute(IEnumerable<float> buffer, int itemCount, bool normalized = false)
            : base(buffer)
        {
            ItemCount = itemCount;
            Normalized = normalized;
        }
    }
}

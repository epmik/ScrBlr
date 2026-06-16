using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class Uint32BufferAttribute : AbstractBufferAttribute<UInt32>
    {
        // https://github.com/mrdoob/three.js/blob/master/src/core/BufferAttribute.js#L1014

        public int ItemCount;
        public bool Normalized;

        public Uint32BufferAttribute(IEnumerable<UInt32> buffer, int itemCount, bool normalized = false)
            : base(buffer)
        {
            ItemCount = itemCount;
            Normalized = normalized;
        }
    }
}

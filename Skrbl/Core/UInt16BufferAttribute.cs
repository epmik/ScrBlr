using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class UInt16BufferAttribute : AbstractBufferAttribute<UInt16>
    {
        // https://github.com/mrdoob/three.js/blob/master/src/core/BufferAttribute.js#L1014

        public int ItemCount;
        public bool Normalized;

        public UInt16BufferAttribute(IEnumerable<UInt16> buffer, int itemCount, bool normalized = false)
            : base(buffer)
        {
            ItemCount = itemCount;
            Normalized = normalized;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public abstract class AbstractBufferAttribute
    {
        // https://github.com/mrdoob/three.js/blob/master/src/core/BufferAttribute.js

    }

    public abstract class AbstractBufferAttribute<TType> : AbstractBufferAttribute
    {
        // https://github.com/mrdoob/three.js/blob/master/src/core/BufferAttribute.js

        protected IEnumerable<TType> Buffer;

        protected AbstractBufferAttribute(IEnumerable<TType> buffer)
        {
            Buffer = buffer;
        }
    }
}

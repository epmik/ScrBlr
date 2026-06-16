using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class BufferGeometry
    {
        // https://github.com/mrdoob/three.js/blob/master/src/core/BufferGeometry.js
       
        List<AbstractBufferAttribute> _attributes = new List<AbstractBufferAttribute>();

        AbstractBufferAttribute? _index = null;

        protected void AddIndex(IEnumerable<UInt32> indices)
        {
            _index = new Uint32BufferAttribute(indices, 1);
        }

        protected void AddIndex(IEnumerable<UInt16> indices)
        {
            _index = new UInt16BufferAttribute(indices, 1);
        }

        protected void AddAttribute<TAttribute>(string name, TAttribute attribute) where TAttribute : AbstractBufferAttribute
        {
            _attributes.Add(attribute);
        }
    }
}

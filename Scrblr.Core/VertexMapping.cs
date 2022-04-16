using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrblr.Core
{
    public class VertexMapping
    {
        #region Fields and Properties

        public enum ElementType
        {
            None,

            //Byte = VertexAttribPointerType.Byte,
            //Short = VertexAttribPointerType.Short,
            //UShort = VertexAttribPointerType.UnsignedShort,
            //UInt32 = VertexAttribPointerType.UnsignedInt,
            //Double = VertexAttribPointerType.Double,
            //Int32 = VertexAttribPointerType.Int,

            // todo allow other types then float
            Single = VertexAttribPointerType.Float,
        }

        public class Map
        {
            /// <summary>
            /// default == ElementIdentifier.None
            /// </summary>
            public VertexFlag VertexFlag = VertexFlag.None;

            /// <summary>
            /// default == ElementType.None
            /// </summary>
            public ElementType ElementType = ElementType.None;

            /// <summary>
            /// default == 0
            /// </summary>
            public int Count;

            /// <summary>
            /// default == true
            /// </summary>
            // todo is this needed, not set anywhere, use VertexFlag and VertexFlags properties?
            //public bool Enabled = true;

            /// <summary>
            /// stride of this element in bytes
            /// </summary>
            public int Stride
            {
                get { return Count * Size(ElementType); }
            }

            private static int Size(ElementType elementType)
            {
                switch(elementType)
                {
                    //case ElementType.Int32:
                    //    return sizeof(Int32);
                    case ElementType.Single:
                        return sizeof(Single);
                    default:
                        throw new NotImplementedException("VertexBufferLayout.Element.Size(Element) failed: found unknown ElementType");
                }
            }
        }

        private Map[] _mapArray;

        public Map[] Maps
        {
            get { return _mapArray; }
        }

        //public Map MapFor(VertexFlag vertexFlag)
        //{
        //    return _mapDictionary.ContainsKey(vertexFlag) ? _mapDictionary[vertexFlag] : null;
        //}

        public int Handle;

        public VertexFlag VertexFlags { get; private set; }

        /// <summary>
        /// stride of all the elements that make up this vertex buffer lay-out
        /// </summary>
        public int Stride
        {
            get { return Maps.Sum(o => o.Stride); }
        }

        #endregion Fields and Properties

        #region Constructors

        public VertexMapping(Map[] maps)
        {
            Validate(maps);

            _mapArray = maps;

            VertexFlags = Maps.Select(o => o.VertexFlag).Combine();
        }

        #endregion Constructors

        private void Validate(Map[] maps)
        {
            if (maps.Any(o => o.VertexFlag == VertexFlag.None))
            {
                throw new ArgumentException($"VertexMapping() constructor failed. Map.VertexFlag cannot be VertexFlag.None.");
            }

            if (maps.Any(o => o.ElementType == ElementType.None))
            {
                throw new ArgumentException($"VertexMapping() constructor failed. Map.ElementType cannot be ElementType.None.");
            }

            if (maps.Any(o => o.Count <= 0))
            {
                throw new ArgumentException($"VertexMapping() constructor failed. Map.Count cannot be less or equal to 0.");
            }

            if (maps.Any(o => maps.Count(p => p.VertexFlag == o.VertexFlag) > 1))
            {
                throw new ArgumentException($"VertexMapping() constructor failed. Map.VertexFlag cannot be OR'd. VertexFlag = VertexFlag.Position0 | VertexFlag.Color0 for example is invalid.");
            }

            var vertexFlags = (VertexFlag[])Enum.GetValues(typeof(VertexFlag));

            if (maps.Any(o => !vertexFlags.Any(p => (int)p == (int)o.VertexFlag)))
            {
                throw new ArgumentException($"VertexMapping() constructor failed. Map.VertexFlag cannot be OR'd. VertexFlag = VertexFlag.Position0 | VertexFlag.Color0 for example is invalid.");
            }
        }

        private static Dictionary<string, VertexMapping> _defaultVertexMappingDictionary = new Dictionary<string, VertexMapping>();

        public static VertexMapping QueryDefaultMapping(VertexFlag vertexFlags, ElementType elementType = ElementType.Single, int positionSize = 3, int colorSize = 4)
        {
            if(vertexFlags == VertexFlag.None)
            {
                throw new ArgumentException($"QueryDefaultMapping(VertexFlag, ElementType, int positionSize, int colorSize) failed. vertexFlags is equal to VertexFlag.None");
            }

            if (elementType == ElementType.None)
            {
                throw new ArgumentException($"QueryDefaultMapping(VertexFlag, ElementType, int positionSize, int colorSize) failed. elementType is equal to ElementType.None");
            }

            if (positionSize < 2 || positionSize > 4)
            {
                throw new ArgumentException($"QueryDefaultMapping(VertexFlag, ElementType, int positionSize, int colorSize) failed. positionSize must be 2, 3 or 4");
            }

            if (colorSize < 3 || colorSize > 4)
            {
                throw new ArgumentException($"QueryDefaultMapping(VertexFlag, ElementType, int positionSize, int colorSize) failed. colorSize must be 3 or 4");
            }

            var key = ToDictionaryKey(vertexFlags, elementType, positionSize, colorSize);

            if(_defaultVertexMappingDictionary.ContainsKey(key))
            {
                return _defaultVertexMappingDictionary[key];
            }

            var defaultVertexMapping = CreateDefaultMapping(vertexFlags, elementType, positionSize, colorSize);

            _defaultVertexMappingDictionary.Add(key, defaultVertexMapping);

            return defaultVertexMapping;
        }

        private static string ToDictionaryKey(VertexFlag vertexFlags, ElementType elementType = ElementType.Single, int positionSize = 3, int colorSize = 4)
        {
            return string.Format($"{(int)vertexFlags}-{(int)elementType}-{positionSize}-{colorSize}");
        }

        private static VertexMapping CreateDefaultMapping(VertexFlag vertexFlags, ElementType elementType = ElementType.Single, int positionSize = 3, int colorSize = 4)
        {
            var maps = new List<Map>();

            foreach (var flag in (VertexFlag[])Enum.GetValues(typeof(VertexFlag)))
            {
                if(!vertexFlags.HasFlag(flag))
                {
                    continue;
                }

                // assume uv coordinates
                var size = 2;

                switch(flag)
                {
                    case VertexFlag.Position0:
                    case VertexFlag.Position1:
                    case VertexFlag.Position2:
                    case VertexFlag.Position3:
                        size = positionSize;
                        break;
                    case VertexFlag.Color0:
                    case VertexFlag.Color1:
                    case VertexFlag.Color2:
                    case VertexFlag.Color3:
                        size = colorSize;
                        break;
                    case VertexFlag.Normal0:
                    case VertexFlag.Normal1:
                        size = 3;
                        break;
                }

                maps.Add(new Map { VertexFlag = flag, Count = size, ElementType = elementType });
            }

            return new VertexMapping(maps.ToArray());
        }
    }
}
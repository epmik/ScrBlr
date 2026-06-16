using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Skrbl
{
    public static class EnumExtensions
    {
        /// <summary>
        ///     A generic extension method that aids in reflecting 
        ///     and retrieving any attribute that is applied to an `Enum`.
        /// </summary>
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
                where TAttribute : System.Attribute
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<TAttribute>();
        }

        public static T Next<T>(this T enumValue) where T : struct
        {
            // taken from https://stackoverflow.com/questions/642542/how-to-get-next-or-previous-enum-value-in-c-sharp

            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));
            }

            T[] Arr = (T[])Enum.GetValues(enumValue.GetType());

            var j = Array.IndexOf<T>(Arr, enumValue) + 1;

            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }

        #region Bit Mask Methods

        ///// <summary>
        ///// Includes an enumerated type and returns the new value
        ///// </summary>
        //public static T AddFlag<T>(this Enum value, T append)
        //{
        //    Type type = value.GetType();

        //    //determine the values
        //    object result = value;
        //    BitMaskValue parsed = new BitMaskValue(append, type);
        //    if (parsed.Signed is long)
        //    {
        //        result = Convert.ToInt64(value) | (long)parsed.Signed;
        //    }
        //    else if (parsed.Unsigned is ulong)
        //    {
        //        result = Convert.ToUInt64(value) | (ulong)parsed.Unsigned;
        //    }

        //    //return the final value
        //    return (T)Enum.Parse(type, result.ToString());
        //}

        public static void AddFlag<T>(this ref T value, in T flags, in bool apply) where T : unmanaged, Enum, IConvertible
        {
            //
            // https://github.com/aybe/DearImGui/blob/develop/DearImGui/Extensions/EnumExtensions.cs
            //

            var code = Type.GetTypeCode(typeof(T));

            switch (code)
            {
                case TypeCode.SByte:
                    {
                        var a = value.ToSByte(null);
                        var b = flags.ToSByte(null);
                        var c = (sbyte)(apply ? a | b : a & ~b);
                        value = Unsafe.As<sbyte, T>(ref c);
                        break;
                    }
                case TypeCode.Byte:
                    {
                        var a = value.ToByte(null);
                        var b = flags.ToByte(null);
                        var c = (byte)(apply ? a | b : a & ~b);
                        value = Unsafe.As<byte, T>(ref c);
                        break;
                    }
                case TypeCode.Int16:
                    {
                        var a = value.ToInt16(null);
                        var b = flags.ToInt16(null);
                        var c = (short)(apply ? a | b : a & ~b);
                        value = Unsafe.As<short, T>(ref c);
                        break;
                    }
                case TypeCode.UInt16:
                    {
                        var a = value.ToUInt16(null);
                        var b = flags.ToUInt16(null);
                        var c = (ushort)(apply ? a | b : a & ~b);
                        value = Unsafe.As<ushort, T>(ref c);
                        break;
                    }
                case TypeCode.Int32:
                    {
                        var a = value.ToInt32(null);
                        var b = flags.ToInt32(null);
                        var c = apply ? a | b : a & ~b;
                        value = Unsafe.As<int, T>(ref c);
                        break;
                    }
                case TypeCode.UInt32:
                    {
                        var a = value.ToUInt32(null);
                        var b = flags.ToUInt32(null);
                        var c = apply ? a | b : a & ~b;
                        value = Unsafe.As<uint, T>(ref c);
                        break;
                    }
                case TypeCode.Int64:
                    {
                        var a = value.ToInt64(null);
                        var b = flags.ToInt64(null);
                        var c = apply ? a | b : a & ~b;
                        value = Unsafe.As<long, T>(ref c);
                        break;
                    }
                case TypeCode.UInt64:
                    {
                        var a = value.ToUInt64(null);
                        var b = flags.ToUInt64(null);
                        var c = apply ? a | b : a & ~b;
                        value = Unsafe.As<ulong, T>(ref c);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        /// <summary>
        /// Removes an enumerated type and returns the new value
        /// </summary>
        public static T RemoveFlag<T>(this Enum value, T remove)
        {
            Type type = value.GetType();

            //determine the values
            object result = value;
            BitMaskValue parsed = new BitMaskValue(remove, type);
            if (parsed.Signed is long)
            {
                result = Convert.ToInt64(value) & ~(long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                result = Convert.ToUInt64(value) & ~(ulong)parsed.Unsigned;
            }

            //return the final value
            return (T)Enum.Parse(type, result.ToString());
        }

        public static bool HasFlags<T>(this ref T value, in T flags) where T : unmanaged, Enum, IConvertible
        {
            //
            // https://github.com/aybe/DearImGui/blob/develop/DearImGui/Extensions/EnumExtensions.cs
            //
            var code = Type.GetTypeCode(typeof(T));

            switch (code)
            {
                case TypeCode.SByte:
                    {
                        var a = value.ToSByte(null);
                        var b = flags.ToSByte(null);
                        return (a & b) == b;
                    }
                case TypeCode.Byte:
                    {
                        var a = value.ToByte(null);
                        var b = flags.ToByte(null);
                        return (a & b) == b;
                    }
                case TypeCode.Int16:
                    {
                        var a = value.ToInt16(null);
                        var b = flags.ToInt16(null);
                        return (a & b) == b;
                    }
                case TypeCode.UInt16:
                    {
                        var a = value.ToUInt16(null);
                        var b = flags.ToUInt16(null);
                        return (a & b) == b;
                    }
                case TypeCode.Int32:
                    {
                        var a = value.ToInt32(null);
                        var b = flags.ToInt32(null);
                        return (a & b) == b;
                    }
                case TypeCode.UInt32:
                    {
                        var a = value.ToUInt32(null);
                        var b = flags.ToUInt32(null);
                        return (a & b) == b;
                    }
                case TypeCode.Int64:
                    {
                        var a = value.ToInt64(null);
                        var b = flags.ToInt64(null);
                        return (a & b) == b;
                    }
                case TypeCode.UInt64:
                    {
                        var a = value.ToUInt64(null);
                        var b = flags.ToUInt64(null);
                        return (a & b) == b;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        #endregion

        #region Bit Mask Helper Classes

        //class to simplfy narrowing values between 
        //a ulong and long since either value should
        //cover any lesser value
        private class BitMaskValue
        {

            //cached comparisons for tye to use
            private static Type _UInt64 = typeof(ulong);
            private static Type _UInt32 = typeof(long);

            public long? Signed;
            public ulong? Unsigned;

            public BitMaskValue(object value, Type type)
            {
                // https://stackoverflow.com/a/417217
                // http://hugoware.net:4000/blog/enumeration-extensions-2-0

                //make sure it is even an enum to work with
                if (!type.IsEnum)
                {
                    throw new ArgumentException("Value provided is not an enumerated type!");
                }

                //then check for the enumerated value
                Type compare = Enum.GetUnderlyingType(type);

                //if this is an unsigned long then the only
                //value that can hold it would be a ulong
                if (compare.Equals(BitMaskValue._UInt32) || compare.Equals(BitMaskValue._UInt64))
                {
                    this.Unsigned = Convert.ToUInt64(value);
                }
                //otherwise, a long should cover anything else
                else
                {
                    this.Signed = Convert.ToInt64(value);
                }

            }

        }

        #endregion    
    }
}
/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Types {

    using System;
    using System.Collections;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Value type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Dtype : int { // CHECK // Must match `Function.h`
        /// <summary>
        /// Null or unsupported data type.
        /// </summary>
        [EnumMember(Value = @"null")]
        Null = 0,
        /// <summary>
        /// Type is a generic half-precision float.
        /// </summary>
        [EnumMember(Value = @"float16")]
        Float16 = 1,
        /// <summary>
        /// Type is `float` in C/C++/C#.
        /// </summary>
        [EnumMember(Value = @"float32")]
        Float32 = 2,
        /// <summary>
        /// Type is `double` in C/C++/C#.
        /// </summary>
        [EnumMember(Value = @"float64")]
        Float64 = 3,
        /// <summary>
        /// Type is a `int8_t` in C/C++ and `sbyte` in C#.
        /// </summary>
        [EnumMember(Value = @"int8")]
        Int8 = 4,
        /// <summary>
        /// Type is `int16_t` in C/C++ and `short` in C#.
        /// </summary>
        [EnumMember(Value = @"int16")]
        Int16 = 5,
        /// <summary>
        /// Type is `int32_t` in C/C++ and `int` in C#.
        /// </summary>
        [EnumMember(Value = @"int32")]
        Int32 = 6,
        /// <summary>
        /// Type is `int64_t` in C/C++ and `long` in C#.
        /// </summary>
        [EnumMember(Value = @"int64")]
        Int64 = 7,
        /// <summary>
        /// Type is `uint8_t` in C/C++ and `byte` in C#.
        /// </summary>
        [EnumMember(Value = @"uint8")]
        Uint8 = 8,
        /// <summary>
        /// Type is a `uint16_t` in C/C++ and `ushort` in C#.
        /// </summary>
        [EnumMember(Value = @"uint16")]
        Uint16 = 9,
        /// <summary>
        /// Type is a `uint32_t` in C/C++ and `uint` in C#.
        /// </summary>
        [EnumMember(Value = @"uint32")]
        Uint32 = 10,
        /// <summary>
        /// Type is a `uint64_t` in C/C++ and `ulong` in C#.
        /// </summary>
        [EnumMember(Value = @"uint64")]
        Uint64 = 11,
        /// <summary>
        /// Type is a `bool` in C/C++/C#.
        /// </summary>
        [EnumMember(Value = @"bool")]
        Bool = 12,
        /// <summary>
        /// Type is `std::string` in C++ and `string` in C#.
        /// </summary>
        [EnumMember(Value = @"string")]
        String = 13,
        /// <summary>
        /// Type is a generic list.
        /// </summary>
        [EnumMember(Value = @"list")]
        List = 14,
        /// <summary>
        /// Type is a generic dictionary.
        /// </summary>
        [EnumMember(Value = @"dict")]
        Dict = 15,
        /// <summary>
        /// Type is an encoded image.
        /// </summary>
        [EnumMember(Value = @"image")]
        Image = 16,
        /// <summary>
        /// Type is a binary blob.
        /// </summary>
        [EnumMember(Value = @"binary")]
        Binary = 17,
        /// <summary>
        /// Type is an encoded audio.
        /// </summary>
        [EnumMember(Value = @"audio")]
        Audio = 18,
        /// <summary>
        /// Type is an encoded video.
        /// </summary>
        [EnumMember(Value = @"video")]
        Video = 19,        
    }

    public static class DtypeExtensions {

        /// <summary>
        /// Convert a Function data type to a managed type.
        /// </summary>
        /// <param name="dtype">Function data type.</param>
        /// <returns>Managed data type.</returns>
        public static Type ToType (this Dtype dtype) => dtype switch {
            Dtype.Float16       => null, // Any support for this in C#?
            Dtype.Float32       => typeof(float),
            Dtype.Float64       => typeof(double),
            Dtype.Int8          => typeof(sbyte),
            Dtype.Int16         => typeof(short),
            Dtype.Int32         => typeof(int),
            Dtype.Int64         => typeof(long),
            Dtype.Uint8         => typeof(byte),
            Dtype.Uint16        => typeof(ushort),
            Dtype.Uint32        => typeof(uint),
            Dtype.Uint64        => typeof(ulong),
            Dtype.Bool          => typeof(bool),
            Dtype.String        => typeof(string),
            Dtype.List          => typeof(IList),
            Dtype.Dict          => typeof(IDictionary),
            _                   => null,
        };

        /// <summary>
        /// Convert a managed type to a Function data type.
        /// </summary>
        /// <param name="type">Managed type.</param>
        /// <returns>Function data type.</returns>
        public static Dtype ToDtype (this Type dtype) => dtype switch {
            var t when t == typeof(float)   => Dtype.Float32,
            var t when t == typeof(double)  => Dtype.Float64,
            var t when t == typeof(sbyte)   => Dtype.Int8,
            var t when t == typeof(short)   => Dtype.Int16,
            var t when t == typeof(int)     => Dtype.Int32,
            var t when t == typeof(long)    => Dtype.Int64,
            var t when t == typeof(byte)    => Dtype.Uint8,
            var t when t == typeof(ushort)  => Dtype.Uint16,
            var t when t == typeof(uint)    => Dtype.Uint32,
            var t when t == typeof(ulong)   => Dtype.Uint64,
            var t when t ==  typeof(bool)   => Dtype.Bool,
            _                               => Dtype.Null,
        };
    }
}
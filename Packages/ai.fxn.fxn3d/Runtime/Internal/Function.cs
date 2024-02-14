/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

namespace Function.Internal {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using Acceleration = Types.Acceleration;
    using Dtype = Types.Dtype;

    /// <summary>
    /// Function C API.
    /// </summary>
    public static unsafe class Function {

        public const string Version = @"0.0.8";
        public const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        @"__Internal";
        #else
        @"Function";
        #endif

        #region --Enumerations--
        /// <summary>
        /// Status codes.
        /// </summary>
        public enum Status : int {
            Ok                  = 0,
            InvalidArgument     = 1,
            InvalidOperation    = 2,
            NotImplemented      = 3,
        }

        /// <summary>
        /// Value flags.
        /// </summary>
        [Flags]
        public enum ValueFlags : int {
            None = 0,
            CopyData = 1,
        }
        #endregion


        #region --FXNValue--
        [DllImport(Assembly, EntryPoint = @"FXNValueRelease", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status ReleaseValue (this IntPtr value);
        [DllImport(Assembly, EntryPoint = @"FXNValueGetData", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetValueData (
            this IntPtr value,
            out IntPtr data
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueGetType", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetValueType (
            this IntPtr value,
            out Dtype type
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueGetDimensions", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetValueDimensions (
            this IntPtr value,
            out int dimensions
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueGetShape", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetValueShape (
            this IntPtr value,
            [Out] int[] shape,
            int shapeLen
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateArray", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern Status CreateArrayValue (
            void* data,
            [In] int[] shape,
            int dims,
            Dtype dtype,
            ValueFlags flags,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateString", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status CreateStringValue (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string data,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateList", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status CreateListValue (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string data,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateDict", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status CreateDictValue (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string data,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status CreateImageValue (
            byte* pixelBuffer,
            int width,
            int height,
            int channels,
            ValueFlags flags,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateBinary", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status CreateBinaryValue (
            [In] byte[] buffer,
            long bufferLen,
            ValueFlags flags,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateNull", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status CreateNullValue (out IntPtr value);
        #endregion


        #region --FXNValueMap--
        [DllImport(Assembly, EntryPoint = @"FXNValueMapCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status CreateValueMap (out IntPtr map);
        [DllImport(Assembly, EntryPoint = @"FXNValueMapRelease", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status ReleaseValueMap (this IntPtr map);
        [DllImport(Assembly, EntryPoint = @"FXNValueMapGetSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetValueMapSize (
            this IntPtr map,
            out int size
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueMapGetKey", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetValueMapKey (
            this IntPtr map,
            int index,
            [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder key,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueMapGetValue", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetValueMapValue (
            this IntPtr map,
            [MarshalAs(UnmanagedType.LPStr)] string key,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueMapSetValue", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status SetValueMapValue (
            this IntPtr map,
            [MarshalAs(UnmanagedType.LPStr)] string key,
            IntPtr value
        );
        #endregion


        #region --FXNConfiguration--
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationGetUniqueID", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetConfigurationUniqueID (
            [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder identifier,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status CreateConfiguration (out IntPtr configuration);
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationRelease", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status ReleaseConfiguration (this IntPtr configuration);
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationSetToken", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status SetConfigurationToken (
            this IntPtr configuration,
            [MarshalAs(UnmanagedType.LPStr)] string token
        );
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationSetResource", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status SetConfigurationResource (
            this IntPtr configuration,
            [MarshalAs(UnmanagedType.LPStr)] string id,
            [MarshalAs(UnmanagedType.LPStr)] string path
        );
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationSetAcceleration", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status SetConfigurationAcceleration (
            this IntPtr configuration,
            Acceleration acceleration
        );
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationSetDevice", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status SetConfigurationDevice (
            this IntPtr configuration,
            IntPtr device
        );
        #endregion


        #region --FXNProfile--
        [DllImport(Assembly, EntryPoint = @"FXNProfileRelease", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status ReleaseProfile (this IntPtr profile);
        [DllImport(Assembly, EntryPoint = @"FXNProfileGetID", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetProfileID (
            this IntPtr profile,
            [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder id,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"FXNProfileGetLatency", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetProfileLatency (
            this IntPtr profile,
            out double latency
        );
        [DllImport(Assembly, EntryPoint = @"FXNProfileGetError", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetProfileError (
            this IntPtr profile,
            [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder error,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"FXNProfileGetLogs", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetProfileLogs (
            this IntPtr profile,
            [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder logs,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"FXNProfileGetLogLength", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status GetProfileLogLength (
            this IntPtr profile,
            out int size
        );
        #endregion


        #region --FXNPredictor--
        [DllImport(Assembly, EntryPoint = @"FXNPredictorCreate", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status CreatePredictor (
            [MarshalAs(UnmanagedType.LPStr)] string tag,
            IntPtr configuration,
            out IntPtr predictor
        );

        [DllImport(Assembly, EntryPoint = @"FXNPredictorRelease", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status ReleasePredictor (this IntPtr predictor);

        [DllImport(Assembly, EntryPoint = @"FXNPredictorPredict", CallingConvention = CallingConvention.Cdecl)]
        public static extern Status Predict (
            this IntPtr predictor,
            IntPtr inputs,
            out IntPtr profile,
            out IntPtr outputs
        );
        #endregion


        #region --FXNVersion--
        [DllImport(Assembly, EntryPoint = @"FXNGetVersion", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetVersion ();
        #endregion


        #region --Utility--

        public static void Throw (this Status status) {
            switch (status) {
                case Status.Ok:                 break;
                case Status.InvalidArgument:    throw new ArgumentException();
                case Status.InvalidOperation:   throw new InvalidOperationException();
                case Status.NotImplemented:     throw new NotImplementedException();
                default:                        throw new InvalidOperationException();
            }
        }
        #endregion
    }

    /// <summary>
    /// Prevent code stripping.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    internal sealed class PreserveAttribute : Attribute { }
}
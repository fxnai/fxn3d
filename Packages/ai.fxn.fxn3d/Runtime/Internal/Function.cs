/* 
*   Function
*   Copyright © 2023 NatML Inc. All rights reserved.
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

        public const string Version = @"0.0.6";
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
        [DllImport(Assembly, EntryPoint = @"FXNValueRelease")]
        public static extern Status ReleaseValue (this IntPtr value);
        [DllImport(Assembly, EntryPoint = @"FXNValueGetData")]
        public static extern Status GetValueData (
            this IntPtr value,
            out IntPtr data
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueGetType")]
        public static extern Status GetValueType (
            this IntPtr value,
            out Dtype type
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueGetDimensions")]
        public static extern Status GetValueDimensions (
            this IntPtr value,
            out int dimensions
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueGetShape")]
        public static extern Status GetValueShape (
            this IntPtr value,
            [Out] int[] shape,
            int shapeLen
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateArray")]
        public static unsafe extern Status CreateArrayValue (
            void* data,
            [In] int[] shape,
            int dims,
            Dtype dtype,
            ValueFlags flags,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateString")]
        public static extern Status CreateStringValue (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string data,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateList")]
        public static extern Status CreateListValue (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string data,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateDict")]
        public static extern Status CreateDictValue (
            [MarshalAs(UnmanagedType.LPUTF8Str)] string data,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateImage")]
        public static extern Status CreateImageValue (
            byte* pixelBuffer,
            int width,
            int height,
            ValueFlags flags,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateBinary")]
        public static extern Status CreateBinaryValue (
            [In] byte[] buffer,
            long bufferLen,
            ValueFlags flags,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueCreateNull")]
        public static extern Status CreateNullValue (out IntPtr value);
        #endregion


        #region --FXNValueMap--
        [DllImport(Assembly, EntryPoint = @"FXNValueMapCreate")]
        public static extern Status CreateValueMap (out IntPtr map);
        [DllImport(Assembly, EntryPoint = @"FXNValueMapRelease")]
        public static extern Status ReleaseValueMap (this IntPtr map);
        [DllImport(Assembly, EntryPoint = @"FXNValueMapGetSize")]
        public static extern Status GetValueMapSize (
            this IntPtr map,
            out int size
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueMapGetKey")]
        public static extern Status GetValueMapKey (
            this IntPtr map,
            int index,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder key,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueMapGetValue")]
        public static extern Status GetValueMapValue (
            this IntPtr map,
            [MarshalAs(UnmanagedType.LPStr)] string key,
            out IntPtr value
        );
        [DllImport(Assembly, EntryPoint = @"FXNValueMapSetValue")]
        public static extern Status SetValueMapValue (
            this IntPtr map,
            [MarshalAs(UnmanagedType.LPStr)] string key,
            IntPtr value
        );
        #endregion


        #region --FXNConfiguration--
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationGetUniqueID")]
        public static extern Status GetConfigurationUniqueID (
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder identifier,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationCreate")]
        public static extern Status CreateConfiguration (out IntPtr configuration);

        [DllImport(Assembly, EntryPoint = @"FXNConfigurationRelease")]
        public static extern Status ReleaseConfiguration (this IntPtr configuration);
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationSetToken")]
        public static extern Status SetConfigurationToken (
            this IntPtr configuration,
            [MarshalAs(UnmanagedType.LPStr)] string token
        );
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationSetResource")]
        public static extern Status SetConfigurationResource (
            this IntPtr configuration,
            [MarshalAs(UnmanagedType.LPStr)] string id,
            [MarshalAs(UnmanagedType.LPStr)] string path
        );
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationSetAcceleration")]
        public static extern Status SetConfigurationAcceleration (
            this IntPtr configuration,
            Acceleration acceleration
        );
        [DllImport(Assembly, EntryPoint = @"FXNConfigurationSetDevice")]
        public static extern Status SetConfigurationDevice (
            this IntPtr configuration,
            IntPtr device
        );
        #endregion


        #region --FXNPredictor--
        [DllImport(Assembly, EntryPoint = @"FXNPredictorCreate")]
        public static extern Status CreatePredictor (
            [MarshalAs(UnmanagedType.LPStr)] string tag,
            IntPtr configuration,
            out IntPtr predictor
        );

        [DllImport(Assembly, EntryPoint = @"FXNPredictorRelease")]
        public static extern Status ReleasePredictor (this IntPtr predictor);

        [DllImport(Assembly, EntryPoint = @"FXNPredictorPredict")]
        public static extern Status Predict (
            this IntPtr predictor,
            IntPtr inputs,
            out IntPtr outputs
        );
        #endregion


        #region --FXNVersion--
        [DllImport(Assembly, EntryPoint = @"FXNGetVersion")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetVersion ();
        #endregion


        #region --Utility--
        public static void CheckStatus (this Status status) {
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
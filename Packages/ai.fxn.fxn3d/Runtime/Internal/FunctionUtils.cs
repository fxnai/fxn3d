/* 
*   Function
*   Copyright © 2024 NatML Inc. All rights reserved.
*/

#nullable enable

namespace Function.Internal {

    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using Status = Function.Status;

    /// <summary>
    /// Helpful extension methods.
    /// </summary>
    internal static class FunctionUtils {

        #region --Client API--
        public static Task Initialization {
            get {
                #if UNITY_WEBGL && !UNITY_EDITOR
                var tcs = new TaskCompletionSource<bool>();
                var context = GCHandle.Alloc(tcs, GCHandleType.Normal);
                SetInitializationHandler(OnFunctionInitialized, (IntPtr)context);
                return tcs.Task;
                [DllImport(Function.Assembly, EntryPoint = @"FXNSetInitializationHandler")]
                static extern Status SetInitializationHandler (Action<IntPtr> handler, IntPtr context);
                #else
                return Task.CompletedTask;
                #endif
            }
        }

        public static Task AddConfigurationResourceAsync (
            this IntPtr configuration,
            string type,
            string path
        ) {
            #if UNITY_WEBGL && !UNITY_EDITOR
            var tcs = new TaskCompletionSource<bool>();
            var context = GCHandle.Alloc(tcs, GCHandleType.Normal);
            AddConfigurationResource(configuration, type, path, OnAddConfigurationResource, (IntPtr)context);
            return tcs.Task;
            [DllImport(Function.Assembly, EntryPoint = @"FXNConfigurationAddResourceAsync")]
            static extern Status AddConfigurationResource (
                IntPtr configuration,
                [MarshalAs(UnmanagedType.LPUTF8Str)] string type,
                [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
                Action<IntPtr, Status> handler,
                IntPtr context
            );
            #else
            try {
                configuration.AddConfigurationResource(type, path).Throw();
                return Task.CompletedTask;
            } catch (Exception ex) {
                return Task.FromException(ex);
            }
            #endif
        }

        public static Status Throw (this Status status) {
            switch (status) {
                case Status.Ok:                 return status;
                case Status.InvalidArgument:    throw new ArgumentException();
                case Status.InvalidOperation:   throw new InvalidOperationException();
                case Status.NotImplemented:     throw new NotImplementedException();
                default:                        throw new InvalidOperationException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream ToStream (this string data) {
            var buffer = Encoding.UTF8.GetBytes(data);
            return new MemoryStream(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Stream ToStream<T> (this T[] data) where T : unmanaged {
            if (data is byte[] raw)
                return new MemoryStream(raw);
            var size = data.Length * sizeof(T);
            var array = new byte[size];
            fixed (void* src = data, dst = array)
                Buffer.MemoryCopy(src, dst, size, size);
            return new MemoryStream(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToArray (this Stream stream) {
            if (stream is MemoryStream memoryStream)
                return memoryStream.ToArray();
            using (var dstStream = new MemoryStream()) {
                stream.CopyTo(dstStream);
                return dstStream.ToArray();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T[] ToArray<T> (this MemoryStream stream) where T : unmanaged {
            var rawData = stream.ToArray();
            var data = new T[rawData.Length / sizeof(T)];
            Buffer.BlockCopy(rawData, 0, data, 0, rawData.Length);
            return data;
        }
        #endregion


        #region --Operations--

        [MonoPInvokeCallback(typeof(Action<IntPtr>))]
        private static void OnFunctionInitialized (IntPtr context) {
            var handle = (GCHandle)context;
            var tcs = handle.Target as TaskCompletionSource<bool>;
            handle.Free();
            tcs?.SetResult(true);
        }

        [MonoPInvokeCallback(typeof(Action<IntPtr, Status>))]
        private static void OnAddConfigurationResource (IntPtr context, Status status) {
            var handle = (GCHandle)context;
            var tcs = handle.Target as TaskCompletionSource<bool>;
            handle.Free();
            try {
                status.Throw();
                tcs?.SetResult(true);
            } catch (Exception ex) {
                tcs?.SetException(ex);
            }
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    internal sealed class PreserveAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class MonoPInvokeCallbackAttribute : Attribute {
        public MonoPInvokeCallbackAttribute (Type type) {}
    }
}
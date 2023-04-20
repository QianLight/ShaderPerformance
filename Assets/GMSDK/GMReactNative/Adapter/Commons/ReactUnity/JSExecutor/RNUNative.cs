using AOT;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace GSDK.RNU {
    /**
    *  负责所有和C代码的交互
    */
    public class RNUNative  {
#if (UNITY_IOS && !UNITY_EDITOR) || __IOS__
        private const string DllName = "__Internal";
#else
        private const string DllName = "rnu";
#endif

        public static void Init() {
            // log
            InitCSharpDelegate(LogMessageFromCpp);
            InitCallNativeModule(CallNativeModuleFromCpp);

            InitUnmanagedJSDataMethod(
                // start/end
                UnmanageJSDataOp.NativeStartCSharpIFromCpp,
                UnmanageJSDataOp.NativeStartCSharpBFromCpp,
                UnmanageJSDataOp.NativeStartCSharpNullFromCpp,
                UnmanageJSDataOp.NativeStartCSharpDFromCpp,
                UnmanageJSDataOp.NativeStartCSharpStrFromCpp,
                UnmanageJSDataOp.NativeStartCSharpArrayFromCpp,
                UnmanageJSDataOp.NativeStartCSharpMapFromCpp,
                UnmanageJSDataOp.NativeStartEndSharpFromCpp,

                // array
                UnmanageJSDataOp.NativeArrayAddIFromCpp,
                UnmanageJSDataOp.NativeArrayAddBFromCpp,
                UnmanageJSDataOp.NativeArrayAddNullFromCpp,
                UnmanageJSDataOp.NativeArrayAddDFromCpp,
                UnmanageJSDataOp.NativeArrayAddStrFromCpp,
                UnmanageJSDataOp.NativeCreateArrayFromArrayFromCpp,
                UnmanageJSDataOp.NativeCreateMapFromArrayFromCpp,

                // map
                UnmanageJSDataOp.NativeMapPutIFromCpp,
                UnmanageJSDataOp.NativeMapPutBFromCpp,
                UnmanageJSDataOp.NativeMapPutNullFromCpp,
                UnmanageJSDataOp.NativeMapPutDFromCpp,
                UnmanageJSDataOp.NativeMapPutStrFromCpp,
                UnmanageJSDataOp.NativeCreateArrayFromMapFromCpp,
                UnmanageJSDataOp.NativeCreateMapFromMapFromCpp
                );

            Util.Log("QuickJSNative init " + (TTTT() == 48 ? "success!" : "fail!"));
        }

#region "测试相关"
        [DllImport(DllName)]
        public static extern int TTTT();

        [DllImport(DllName)]
        public static extern void InitCSharpDelegate(LogDelegate log);

        public delegate void LogDelegate(IntPtr message);

        [MonoPInvokeCallback(typeof(LogDelegate))]
        public static void LogMessageFromCpp(IntPtr message)
        {
            Util.Log(Marshal.PtrToStringAnsi(message));
        }

#endregion

#region "RNU React/JS 执行相关"


        [DllImport(DllName)]
        public static extern int LoadApplicationScriptBytecode(string uri);

        [DllImport(DllName)]
        public static extern void CallFunction(string moduleName, string methodName, IntPtr argsPtr);

        [DllImport(DllName)]
        public static extern void InvokeCallback(int callID, IntPtr argsPtr);

        [DllImport(DllName)]
        public static extern void SetGlobalVariable(string name, IntPtr valPtr);

        [DllImport(DllName)]
        public static extern int InitRuntime();

        [DllImport(DllName)]
        public static extern void FreeRuntime();

        [DllImport(DllName)]
        public static extern void InitCallNativeModule(CallNativeModule log);

        public delegate void CallNativeModule(int moduleId, int methodId, int index);

        [MonoPInvokeCallback(typeof(CallNativeModule))]
        public static void CallNativeModuleFromCpp(int moduleId, int methodId, int index)
        {
            ArrayList args = null;
            if (moduleId == -1 && methodId == -1) {
                // no-op
            } else {
                // 获取 ArrayList类型参数
                //args = NativeDataOp.GetAndRemoveNativeDataByIndex(index);
                args = UnmanageJSDataOp.GetCSharpArrayList(index);
            }

            RNUMainCore.CallNativeModule(moduleId, methodId, args);
        }

#endregion


#region "NativeDataOp C --> C#"
        [DllImport(DllName)]
        public static extern void InitUnmanagedJSDataMethod(
            // start/end
            UnmanageJSDataOp.NativeStartCSharpI nativeStartCSharpI,
            UnmanageJSDataOp.NativeStartCSharpB nativeStartCSharpB,
            UnmanageJSDataOp.NativeStartCSharpNull nativeStartCSharpNull,
            UnmanageJSDataOp.NativeStartCSharpD nativeStartCSharpD,
            UnmanageJSDataOp.NativeStartCSharpStr nativeStartCSharpDStr,
            UnmanageJSDataOp.NativeStartCSharpArray nativeStartCSharpArray,
            UnmanageJSDataOp.NativeStartCSharpMap nativeStartCSharpMap,
            UnmanageJSDataOp.NativeStartEndSharp nativeStartEndSharp,

            // array
            UnmanageJSDataOp.NativeArrayAddI nativeArrayAddI,
            UnmanageJSDataOp.NativeArrayAddB nativeArrayAddB,
            UnmanageJSDataOp.NativeArrayAddNull nativeArrayAddNull,
            UnmanageJSDataOp.NativeArrayAddD nativeArrayAddD,
            UnmanageJSDataOp.NativeArrayAddStr nativeArrayAddStr,
            UnmanageJSDataOp.NativeCreateArrayFromArray nativeCreateArrayFromArray,
            UnmanageJSDataOp.NativeCreateMapFromArray nativeCreateMapFromArray,

            // map
            UnmanageJSDataOp.NativeMapPutI nativeMapPutI,
            UnmanageJSDataOp.NativeMapPutB nativeMapPutB,
            UnmanageJSDataOp.NativeMapPutNull nativeMapPutNull,
            UnmanageJSDataOp.NativeMapPutD nativeMapPutD,
            UnmanageJSDataOp.NativeMapPutStr nativeMapPutStr,
            UnmanageJSDataOp.NativeCreateArrayFromMap nativeCreateArrayFromMap,
            UnmanageJSDataOp.NativeCreateArrayFromMap nativeCreateMapFromMap
        );

#endregion


#region "NativeDataOp C# ---> C"
        [DllImport(DllName)]
        public static extern IntPtr CreateJSArray();

        [DllImport(DllName)]
        public static extern void PutJSArrayI(IntPtr arr, uint index, int val);

        [DllImport(DllName)]
        public static extern void PutJSArrayB(IntPtr arr, uint index, [MarshalAs(UnmanagedType.U1)] bool val);

        [DllImport(DllName)]
        public static extern void PutJSArrayStr(IntPtr arr, uint index, string val);

        [DllImport(DllName)]
        public static extern void PutJSArrayD(IntPtr arr, uint index, double val);

        [DllImport(DllName)]
        public static extern void PutJSArrayList(IntPtr arr, uint index, IntPtr val);

        [DllImport(DllName)]
        public static extern void PutJSArrayObj(IntPtr arr, uint index, IntPtr val);



        [DllImport(DllName)]
        public static extern IntPtr CreateJSObject();

        [DllImport(DllName)]
        public static extern void PutJSObjectI(IntPtr arr, string key, int val);

        [DllImport(DllName)]
        public static extern void PutJSObjectB(IntPtr arr, string key, [MarshalAs(UnmanagedType.U1)] bool val);

        [DllImport(DllName)]
        public static extern void PutJSObjectStr(IntPtr arr, string key, string val);

        [DllImport(DllName)]
        public static extern void PutJSObjectD(IntPtr arr, string key, double val);

        [DllImport(DllName)]
        public static extern void PutJSObjectList(IntPtr arr, string key, IntPtr val);

        [DllImport(DllName)]
        public static extern void PutJSObjectObj(IntPtr arr, string index, IntPtr val);

#endregion

# region "由于Unity/C# 没有官方的方便的JSON 实现。故借助quickjs的能力"
        [DllImport(DllName)]
        public static extern void ParseJSON(int outKey, string jsonEncodedValue);
        [DllImport(DllName)]
        public static extern IntPtr StringifyJSON(IntPtr p);
#endregion

    }
}

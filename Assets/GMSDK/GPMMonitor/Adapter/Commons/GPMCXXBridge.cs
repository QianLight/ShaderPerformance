using System.Runtime.InteropServices;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#if GMEnderOn && UNITY_EDITOR
using Ender;
#endif

namespace GMSDK
{
    public class GPMCXXBridge
    {
        public static void LogSceneStart(string type, string name)
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
           // GPM_CXX_LogSceneStart(type, name);   
#endif
        }

        public static void LogSceneLoaded(string type, string name)
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
           // GPM_CXX_LogSceneLoaded(type, name);
#endif
        }

        public static void LogSceneEnd(string type, string name, bool isUpload)
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
           // GPM_CXX_LogSceneEnd(type, name, isUpload);
#endif
        }

        public static void LogFrameEnd()
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
          //  GPM_CXX_LogFrameEnd();
#endif
        }

        public static void LogCPUUsage()
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
            //GPM_CXX_LogCPUUsage();
#endif
        }

        public static void LogMemoryUsage()
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
           // GPM_CXX_LogMemoryUsage();
#endif
        }

        public static void LogGlobalInfo(string type, string key, string value)
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
            GPM_CXX_LogGlobalInfoStr(type, key, value);
#endif
        }

        public static void LogGlobalInfo(string type, string key, int value)
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
            //GPM_CXX_LogGlobalInfoInt(type, key, value);
#endif
        }


        public static void LogGlobalInfo(string type, string key)
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
           // GPM_CXX_LogGlobalInfoKey(type, key);
#endif
        }

        public static void LogSceneInfo(string type, string name, string key, string value)
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
            //GPM_CXX_LogSceneInfoStr(type, name, key, value);
#endif
        }

        public static void LogSceneInfo(string type, string name, string key, int value)
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
           // GPM_CXX_LogSceneInfoInt(type, name, key, value);
#endif
        }

        public static void LogSceneInfo(string type, string name, string key)
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
           // GPM_CXX_LogSceneInfoKey(type, name, key);
#endif
        }

        public static int RequestGraphicLevel()
        {
#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR) || (GMEnderOn && UNITY_EDITOR)
            return 0;
#else
            return 0;
#endif
        }


#if UNITY_IOS
        const string libname = "__Internal";
#elif UNITY_ANDROID
        const string libname = "gpm";
#endif

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogSceneStart(string type, string name);
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogSceneLoaded(string type, string name);
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogSceneEnd(string type, string name, bool isUpload);
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogFrameEnd();
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogCPUUsage();
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogMemoryUsage();
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogGlobalInfoStr(string type, string key, string value);
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogGlobalInfoInt(string type, string key, int value);
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogGlobalInfoKey(string type, string key);
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogSceneInfoStr(string type, string name, string key, string value);
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogSceneInfoInt(string type, string name, string key, int value);
        // [DllImport(libname)]
        // private static extern void GPM_CXX_LogSceneInfoKey(string type, string name, string key);
        // [DllImport(libname)]
        // private static extern int GPM_CXX_RequestGraphicLevel();
#elif GMEnderOn && UNITY_EDITOR
        private static void GPM_CXX_LogSceneStart(string type, string name)
        {

            //StackTrace st = new StackTrace(new StackFrame(true));
            //StackFrame sf = st.GetFrame(0);
            //Console.WriteLine(" Method: {0}", sf.GetMethod().Name);

            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogSceneStart",
            //     new List<GMEnderCFuncParam>() {
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, type),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, name)
            //     }
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }
        
        private static void GPM_CXX_LogSceneLoaded(string type, string name)
        {
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogSceneLoaded",
            //     new List<GMEnderCFuncParam>() {
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, type),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, name)
            //     }
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }

        private static void GPM_CXX_LogSceneEnd(string type, string name, bool isUpload)
        {
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogSceneEnd",
            //     new List<GMEnderCFuncParam>() {
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, type),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, name),
            //         new GMEnderCFuncParam(GMEnderValueType.type_bool, isUpload)
            //     }
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }

        private static void GPM_CXX_LogFrameEnd()
        {
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogFrameEnd"
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }

        private static void GPM_CXX_LogCPUUsage()
        {
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogCPUUsage"
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }

        private static void GPM_CXX_LogMemoryUsage()
        {
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogMemoryUsage"
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }

        private static void GPM_CXX_LogGlobalInfoStr(string type, string key, string value)
        {
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogGlobalInfoStr",
            //     new List<GMEnderCFuncParam>() {
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, type),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, key),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, value)
            //     }
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }

        private static void GPM_CXX_LogGlobalInfoInt(string type, string key, int value)
        {
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogGlobalInfoInt",
            //     new List<GMEnderCFuncParam>() {
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, type),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, key),
            //         new GMEnderCFuncParam(GMEnderValueType.type_int, value)
            //     }
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }

        private static void GPM_CXX_LogGlobalInfoKey(string type, string key)
        {
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogGlobalInfoKey",
            //     new List<GMEnderCFuncParam>() {
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, type),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, key)
            //     }
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }

        private static void GPM_CXX_LogSceneInfoStr(string type, string name, string key, string value)
        {
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogSceneInfoStr",
            //     new List<GMEnderCFuncParam>() {
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, type),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, name),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, key),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, value)
            //     }
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }

        private static void GPM_CXX_LogSceneInfoInt(string type, string name, string key, int value)
        {
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogSceneInfoInt",
            //     new List<GMEnderCFuncParam>() {
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, type),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, name),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, key),
            //         new GMEnderCFuncParam(GMEnderValueType.type_int, value)
            //     }
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }

        private static void GPM_CXX_LogSceneInfoKey(string type, string name, string key)
        {
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_void,
            //     "GPM_CXX_LogSceneInfoKey",
            //     new List<GMEnderCFuncParam>() {
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, type),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, name),
            //         new GMEnderCFuncParam(GMEnderValueType.type_string, key)
            //     }
            //     );
            // GMEnderMgr.instance.callEnder(function);
        }

        private static int GPM_CXX_RequestGraphicLevel()
        {
             return 0;
            // GMEnderCFunction function = new GMEnderCFunction(
            //     GMEnderValueType.type_int,
            //     "GPM_CXX_RequestGraphicLevel"
            //     );
            // object ret = GMEnderMgr.instance.callEnder(function);
            // if (ret != null)
            // {
            //     return (int)ret;
            // }
            // else
            // {
            //     return 0;
            // }
        }
#endif
    }
}

using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GSDK.RNU {

    /**
    * 负责处理 JS引擎  JSValue 和 C# 数据的互相转换
    *     JSValue                     C#
    *      bool                      bool
    *      int                       int
    *      number                    double
    *      array                     ArrayList
    *      object                    HashTable
    */

    public class UnmanageJSDataOp {
        
        # region " C#_Collections ---> C_JSValue"
        public static IntPtr GeneJSArray(ArrayList list) {
            IntPtr arr = RNUNative.CreateJSArray();

            for(uint i = 0 ; i < list.Count; i ++ ) {
                int ii = Convert.ToInt32(i);

                object item = list[ii];
                string typeName = item.GetType().Name;
                switch(typeName) {
                    case "Int32": {
                        RNUNative.PutJSArrayI(arr, i, (int)item);
                        break;
                    }
                    case "Boolean": {
                        RNUNative.PutJSArrayB(arr, i, (bool)item);
                        break;
                    }
                    case "String": {
                        RNUNative.PutJSArrayStr(arr, i, (string)item);
                        break;
                    }
                    case "Double": {
                        RNUNative.PutJSArrayD(arr, i, (double)item);
                        break;
                    }
                    case "Single": {
                        RNUNative.PutJSArrayD(arr, i, (double)(float)item);
                        break;
                    }
                    case "ArrayList": {
                        IntPtr subArr = GeneJSArray((ArrayList)item);
                        RNUNative.PutJSArrayList(arr, i, subArr);
                        break;
                    }
                    case "Hashtable": {
                        IntPtr subObj = GeneJSObject((Hashtable)item);
                        RNUNative.PutJSArrayObj(arr, i, subObj);
                        break;
                    }
                    case "Dictionary`2": {
                        IntPtr subObj = GeneJSObject((Dictionary<string, object>)item);
                        RNUNative.PutJSArrayObj(arr, i, subObj);
                        break;
                    }
                    default: {
                        Util.LogError("UnmanageJSDataOp.GeneJSArray not support {0}", typeName);
                        break;
                    }
                }
            }
            return arr;
        }
        public static IntPtr GeneJSObject(Hashtable map) {
            IntPtr obj = RNUNative.CreateJSObject();
            
            foreach(DictionaryEntry de in map) {
                string key = (string) de.Key;
                var val = de.Value;

                string typeName = val.GetType().Name;
                switch(typeName) {
                    case "Int32": {
                        RNUNative.PutJSObjectI(obj, key, (int)val);
                        break;
                    }
                    case "Boolean": {
                        RNUNative.PutJSObjectB(obj, key, (bool)val);
                        break;
                    }
                    case "String": {
                        RNUNative.PutJSObjectStr(obj, key, (string)val);
                        break;
                    }
                    case "double": {
                        RNUNative.PutJSObjectD(obj, key, (double)val);
                        break;
                    }
                    case "Single": {
                        RNUNative.PutJSObjectD(obj, key, (double)(float)val);
                        break;
                    }
                    case "ArrayList": {
                        IntPtr subArr = GeneJSArray((ArrayList)val);
                        RNUNative.PutJSObjectList(obj, key, subArr);
                        break;
                    }
                    case "Hashtable": {
                        IntPtr subObj = GeneJSObject((Hashtable)val);
                        RNUNative.PutJSObjectObj(obj, key, subObj);
                        break;
                    }
                    case "Dictionary`2":
                    {
                        IntPtr subObj = GeneJSObject((Dictionary<string, object>)val);
                        RNUNative.PutJSObjectObj(obj, key, subObj);
                        break;
                    }
                    default: {
                        Util.LogError("UnmanageJSDataOp.GeneJSObject not support {0}", typeName);
                        break;
                    }
                }
            }
            return obj;
        }

        public static IntPtr GeneJSObject(Dictionary<string, object> map)
        {
            IntPtr obj = RNUNative.CreateJSObject();

            foreach(KeyValuePair<string, object> de in map) {
                string key = (string) de.Key;
                var val = de.Value;

                string typeName = val.GetType().Name;
                switch(typeName) {
                    case "Int32": {
                        RNUNative.PutJSObjectI(obj, key, (int)val);
                        break;
                    }
                    case "Boolean": {
                        RNUNative.PutJSObjectB(obj, key, (bool)val);
                        break;
                    }
                    case "String": {
                        RNUNative.PutJSObjectStr(obj, key, (string)val);
                        break;
                    }
                    case "double": {
                        RNUNative.PutJSObjectD(obj, key, (double)val);
                        break;
                    }
                    case "Single": {
                        RNUNative.PutJSObjectD(obj, key, (double)(float)val);
                        break;
                    }
                    case "ArrayList": {
                        IntPtr subArr = GeneJSArray((ArrayList)val);
                        RNUNative.PutJSObjectList(obj, key, subArr);
                        break;
                    }
                    case "Hashtable": {
                        IntPtr subObj = GeneJSObject((Hashtable)val);
                        RNUNative.PutJSObjectObj(obj, key, subObj);
                        break;
                    }
                    case "Dictionary`2":
                    {
                        IntPtr subObj = GeneJSObject((Dictionary<string, object>)val);
                        RNUNative.PutJSObjectObj(obj, key, subObj);
                        break;
                    }
                    default: {
                        Util.LogError("UnmanageJSDataOp.GeneJSObject not support {0}", typeName);
                        break;
                    }
                }
            }
            return obj;
        }
        # endregion

        #region "C_JSValue ---> C#_Collections"
        
        private static Hashtable resultStore = new Hashtable();
        private static Dictionary<int, object> tmpStore;
        
        public static ArrayList GetCSharpArrayList(int outKey)
        {
            if (resultStore.ContainsKey(outKey))
            {
                ArrayList r = (ArrayList) resultStore[outKey];
                resultStore.Remove(outKey);
                return r;
            }
            else
            {
                Util.Log("GetCSharpArrayList with key: {0} error!", outKey);
                return null;
            }
        }
        public static Dictionary<string, object> GetCSharpMap(int outKey)
        {
            if (resultStore.ContainsKey(outKey))
            {
                Dictionary<string, object> r = (Dictionary<string, object>) resultStore[outKey];
                resultStore.Remove(outKey);
                return r;
            }
            else
            {
                Util.Log("GetCSharpMap with key: {0} error!", outKey);
                return null;
            }
        }

        
        // start / end 
        public delegate void NativeStartCSharpI(int outKey, int val);
        [MonoPInvokeCallback(typeof(NativeStartCSharpI))]
        public static void NativeStartCSharpIFromCpp(int outKey, int val)
        {
            resultStore.Add(outKey, val);
        }
        
        public delegate void NativeStartCSharpB(int outKey, [MarshalAs(UnmanagedType.U1)] bool val);
        [MonoPInvokeCallback(typeof(NativeStartCSharpB))]
        public static void NativeStartCSharpBFromCpp(int outKey, [MarshalAs(UnmanagedType.U1)] bool val)
        {
            resultStore.Add(outKey, val);
        }
        public delegate void NativeStartCSharpNull(int outKey);
        [MonoPInvokeCallback(typeof(NativeStartCSharpNull))]
        public static void NativeStartCSharpNullFromCpp(int outKey)
        {
            resultStore.Add(outKey, null);
        }
        public delegate void NativeStartCSharpD(int outKey, double val);
        [MonoPInvokeCallback(typeof(NativeStartCSharpD))]
        public static void NativeStartCSharpDFromCpp(int outKey, double val)
        {
            resultStore.Add(outKey, val);
        }
        
        public delegate void NativeStartCSharpStr(int outKey, string val);
        [MonoPInvokeCallback(typeof(NativeStartCSharpStr))]
        public static void NativeStartCSharpStrFromCpp(int outKey, string val)
        {
            resultStore.Add(outKey, val);
        }
        
        public delegate void NativeStartCSharpArray(int outKey);
        [MonoPInvokeCallback(typeof(NativeStartCSharpArray))]
        public static void NativeStartCSharpArrayFromCpp(int outKey)
        {
            ArrayList list = new ArrayList();
            tmpStore = new Dictionary<int, object>();
            tmpStore.Add(0, list);
            resultStore.Add(outKey, list);
        }
        
        public delegate void NativeStartCSharpMap(int outKey);
        [MonoPInvokeCallback(typeof(NativeStartCSharpMap))]
        public static void NativeStartCSharpMapFromCpp(int outKey)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();
            tmpStore = new Dictionary<int, object>();
            tmpStore.Add(0, map);
            resultStore.Add(outKey, map);
        }
        
        
        public delegate void NativeStartEndSharp();
        [MonoPInvokeCallback(typeof(NativeStartEndSharp))]
        public static void NativeStartEndSharpFromCpp()
        {
            if (tmpStore != null)
            {
                tmpStore.Clear();
                tmpStore = null;
            }
        }

        // Array
        public delegate void NativeArrayAddI(int outKey, int val);
        [MonoPInvokeCallback(typeof(NativeArrayAddI))]
        public static void NativeArrayAddIFromCpp(int outKey, int val)
        {
            ArrayList list = (ArrayList)tmpStore[outKey];
            list.Add(val);
        }
        
        public delegate void NativeArrayAddB(int outKey, [MarshalAs(UnmanagedType.U1)] bool val);
        [MonoPInvokeCallback(typeof(NativeArrayAddB))]
        public static void NativeArrayAddBFromCpp(int outKey, [MarshalAs(UnmanagedType.U1)] bool val)
        {
            ArrayList list = (ArrayList)tmpStore[outKey];
            list.Add(val);
        }
        
        public delegate void NativeArrayAddNull(int outKey);
        [MonoPInvokeCallback(typeof(NativeArrayAddNull))]
        public static void NativeArrayAddNullFromCpp(int outKey)
        {
            ArrayList list = (ArrayList)tmpStore[outKey];
            list.Add(null);
        }
        public delegate void NativeArrayAddD(int outKey, double val);
        [MonoPInvokeCallback(typeof(NativeArrayAddD))]
        public static void NativeArrayAddDFromCpp(int outKey, double val)
        {
            ArrayList list = (ArrayList)tmpStore[outKey];
            list.Add(val);
        }
        public delegate void NativeArrayAddStr(int outKey, string val);
        [MonoPInvokeCallback(typeof(NativeArrayAddStr))]
        public static void NativeArrayAddStrFromCpp(int outKey, string val)
        {
            ArrayList list = (ArrayList)tmpStore[outKey];
            list.Add(val);
        }
        
        public delegate int NativeCreateArrayFromArray(int outKey);
        [MonoPInvokeCallback(typeof(NativeCreateArrayFromArray))]
        public static int NativeCreateArrayFromArrayFromCpp(int outKey)
        {
            ArrayList list = (ArrayList)tmpStore[outKey];
            ArrayList child = new ArrayList();
            int childKey = tmpStore.Count;

            list.Add(child);
            tmpStore.Add(childKey, child);
            return childKey;
        }
        public delegate int NativeCreateMapFromArray(int outKey);
        [MonoPInvokeCallback(typeof(NativeCreateMapFromArray))]
        public static int NativeCreateMapFromArrayFromCpp(int outKey)
        {
            ArrayList list = (ArrayList)tmpStore[outKey];
            Dictionary<string, object> child = new Dictionary<string, object>();
            int childKey = tmpStore.Count;

            list.Add(child);
            tmpStore.Add(childKey, child);
            return childKey;
        }
        
        // map
        public delegate void NativeMapPutI(int outKey, string key, int val);
        [MonoPInvokeCallback(typeof(NativeMapPutI))]
        public static void NativeMapPutIFromCpp(int outKey, string key, int val)
        {
            Dictionary<string, object> map = (Dictionary<string, object>)tmpStore[outKey];
            map.Add(key, val);
        }
        public delegate void NativeMapPutB(int outKey, string key, [MarshalAs(UnmanagedType.U1)] bool val);
        [MonoPInvokeCallback(typeof(NativeMapPutB))]
        public static void NativeMapPutBFromCpp(int outKey, string key, [MarshalAs(UnmanagedType.U1)] bool val)
        {
            Dictionary<string, object> map = (Dictionary<string, object>)tmpStore[outKey];
            map.Add(key, val);
        }
        public delegate void NativeMapPutNull(int outKey, string key);
        [MonoPInvokeCallback(typeof(NativeMapPutNull))]
        public static void NativeMapPutNullFromCpp(int outKey, string key)
        {
            Dictionary<string, object> map = (Dictionary<string, object>)tmpStore[outKey];
            map.Add(key, null);
        }
        public delegate void NativeMapPutD(int outKey, string key, double val);
        [MonoPInvokeCallback(typeof(NativeMapPutD))]
        public static void NativeMapPutDFromCpp(int outKey, string key, double val)
        {
            Dictionary<string, object> map = (Dictionary<string, object>)tmpStore[outKey];
            map.Add(key, val);
        }
        public delegate void NativeMapPutStr(int outKey, string key, string val);
        [MonoPInvokeCallback(typeof(NativeMapPutStr))]
        public static void NativeMapPutStrFromCpp(int outKey, string key, string val)
        {
            Dictionary<string, object> map = (Dictionary<string, object>)tmpStore[outKey];
            map.Add(key, val);
        }
        
        public delegate int NativeCreateArrayFromMap(int outKey, string key);
        [MonoPInvokeCallback(typeof(NativeCreateArrayFromMap))]
        public static int NativeCreateArrayFromMapFromCpp(int outKey, string key)
        {
            Dictionary<string, object> map = (Dictionary<string, object>)tmpStore[outKey];
            ArrayList child = new ArrayList();
            int childKey = tmpStore.Count;
            
            map.Add(key, child);
            tmpStore.Add(childKey, child);
            return childKey;
        }

        public delegate void NativeCreateMapFromMap(int outKey, string key);
        [MonoPInvokeCallback(typeof(NativeCreateMapFromMap))]
        public static int NativeCreateMapFromMapFromCpp(int outKey, string key)
        {
            Dictionary<string, object> map = (Dictionary<string, object>)tmpStore[outKey];
            Dictionary<string, object> child = new Dictionary<string, object>();
            int childKey = tmpStore.Count;
            
            map.Add(key, child);
            tmpStore.Add(childKey, child);
            return childKey;
        }
        
        #endregion
    }
}

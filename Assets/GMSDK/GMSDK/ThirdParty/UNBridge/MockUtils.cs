using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace UNBridgeLib
{
    public enum MockMode
    {
        Optional = 0,
        EnableAll = 1,
        DisableAll = 2
    }

    [Serializable]
    public class MockModule
    {
        public string name;
        public MockMode mode;
        public List<MockEvent> events;
        public List<MockMethod> methods;
        
        public MockModule()
        {
        }
    }
    [Serializable]
    public class MockMethod
    {
        public string target;
        public int type;
        public bool enable;
        public JsonData param;
        public JsonData data;
        public MockMethod()
        {
        }
    }
    [Serializable]
    public class MockEvent
    {
        public string target;
        public bool enable;
        public List<JsonData> data;
        public MockEvent()
        {
        }
    }
    
    public class MockUtils
    {
        
        private static readonly Dictionary<string, JsonData> MethodMockData = new Dictionary<string, JsonData>();
        private static readonly Dictionary<string, List<JsonData>> EventMockData = new Dictionary<string, List<JsonData>>();
        
        /// <summary>
        /// 初始化 mock 数据
        /// </summary>
        public static void InitMockData()
        {
            DirectoryInfo mockDir = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, "Mock"));
            FileInfo[] files = mockDir.GetFiles();
            foreach (var file in files)
            {
                if (Regex.IsMatch(file.FullName, @"(.json)$"))
                {
                    Debug.Log("Mock File:" + file.FullName);
                    string jsonStr = File.ReadAllText(file.FullName);
                    MockModule module = JsonMapper.ToObject<MockModule>(jsonStr);
                    if (module.mode == MockMode.DisableAll) continue;
                    if (module.methods != null)
                    {
                        foreach (var method in module.methods)
                        {
                            if (module.mode == MockMode.EnableAll || method.enable)
                            {
                                MethodMockData[method.target] = method.data;
                            }
                        }
                    }
                    if (module.events != null)
                    {
                        foreach (var mockEvent in module.events)
                        {
                            if (module.mode == MockMode.EnableAll || mockEvent.enable)
                            {
                                EventMockData[mockEvent.target] = mockEvent.data;
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 清除 mock 数据
        /// </summary>
        public static void ClearMockData()
        {
            MethodMockData.Clear();
            EventMockData.Clear();
        }

        /// <summary>
        /// 异步 mock 调用
        /// </summary>
        /// <param name="target"></param>
        /// <param name="param"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static bool Call(string target, JsonData param, BridgeCallBack callback)
        {
            if (MethodMockData.ContainsKey(target))
            {
                UNBridge.CallMock(target, param, MethodMockData[target], callback);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 同步 mock 调用
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CallSync(string target, ref object value)
        {
            if (MethodMockData.ContainsKey(target))
            {
                JsonData data = MethodMockData[target];
                if (data.IsBoolean)
                {
                    value = (bool)data;
                }
                else if (data.IsDouble)
                {
                    value = (double)data;
                }
                else if (data.IsInt)
                {
                    value = (int)data;
                }
                else if (data.IsLong)
                {
                    value = (long)data;
                }
                else if (data.IsString)
                {
                    value = (string)data;
                }
                else
                {
                    value = data;
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// 唤起 Mock Event，默认唤起数组里面第一个，可通过 tag 选取需要的 mockData
        /// </summary>
        /// <param name="target"></param>
        public static void InvokeMockEvent(string target, string tag = "")
        {
            if (EventMockData.ContainsKey(target))
            {
                List<JsonData> data = EventMockData[target];
                if (data != null && data.Count > 0)
                {
                    JsonData mockData = data[0];
                    foreach (var item in data)
                    {
                        if (item.Keys.Contains("tag"))
                        {
                            string itemTag = (string) item["tag"];
                            if (itemTag == tag)
                            {
                                mockData = item;
                                break;
                            }
                        }
                    }
                    BridgeCore.InvokeMockEvent(target, mockData);
                }
            }
        }

        /// <summary>
        /// 获取 Mock Event 的数据，默认获取数组里面第一个，可通过 tag 选取需要的 mockData
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static JsonData GetEventMockData(string target, string tag = "")
        {
            if (EventMockData.ContainsKey(target))
            {
                List<JsonData> data = EventMockData[target];
                if (data != null && data.Count > 0)
                {
                    JsonData mockData = data[0];
                    foreach (var item in data)
                    {
                        if (item.Keys.Contains("tag"))
                        {
                            string itemTag = (string) item["tag"];
                            if (itemTag == tag)
                            {
                                mockData = item;
                                break;
                            }
                        }
                    }
                    return mockData;
                }
            }
            return null;
        }

        /// <summary>
        /// 自定义数据唤起 Mock Event
        /// </summary>
        public static void InvokeMockEvent(string target, JsonData data)
        {
            if (EventMockData.ContainsKey(target))
            {
                BridgeCore.InvokeMockEvent(target, data);
            }
        }
    }
}
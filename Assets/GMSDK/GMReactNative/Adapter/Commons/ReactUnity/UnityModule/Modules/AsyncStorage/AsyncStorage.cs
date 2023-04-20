/*
 * @author qiujianpeng.m
 * 提供数据持久化方法
 * AsyncStorage是基于unity PlayerPrefs类实现的，异步的、持久化的 Key-Value 存储系统
 */
using UnityEngine;

namespace GSDK.RNU
{
    public class AsyncStorage : SimpleUnityModule
    {
        public static string NAME = "AsyncStorage";

        public override string GetName()
        {
            return NAME;
        }

        [ReactMethod(true)]
        public void getItem(string key, Promise promise)
        {
            bool hasKey = PlayerPrefs.HasKey(key);
            if (hasKey)
            {
                // key existed
                string v = PlayerPrefs.GetString(key);
                promise.Resolve(v);
            }
            else
            {
                // key doesn't existed
                promise.Resolve(false);
            }
        }

        [ReactMethod(true)]
        public void setItem(string key, string value, Promise promise)
        {
            PlayerPrefs.SetString(key, value);
            promise.Resolve(true);
        }

        [ReactMethod(true)]
        public void removeItem(string key, Promise promise)
        {
            PlayerPrefs.DeleteKey(key);
            promise.Resolve(true);
        }

        [ReactMethod(true)]
        public void clear(Promise promise)
        {
            PlayerPrefs.DeleteAll();
            promise.Resolve(true);
        }
    }
}
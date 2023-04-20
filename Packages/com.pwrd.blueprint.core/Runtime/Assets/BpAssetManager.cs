/// 蓝图资产加载单例类
/// 注意：改类的名称以及其中的某些函数名称被写死在蓝图的导出中，因此慎重修改
/// 需要修改时请在蓝图源码内搜索改类名称

using UnityEngine;
using System;
using Blueprint.Actor;

namespace Blueprint.Asset
{
    public static class BpAssetManager
    {
        private static IAssetLoader m_Loader;

        private const string c_defaultLoaderClassName = "Blueprint.Asset.BpResourcesLoader";

        public static void Init()
        {
            if (m_Loader != null)
            {
                return ;
            }

            BpSetting setting = Resources.Load<BpSetting>("BpSetting");

            if (setting == null)
            {
                Debug.LogError("Can't find BpSetting");
                return ;
            }

            string className = c_defaultLoaderClassName;

            if (!string.IsNullOrEmpty(setting.loaderClassName))
            {
                className = setting.loaderClassName;
            }

            Type loaderType = BPClassManager.Instance.GetClass(className);
            m_Loader = (IAssetLoader)Activator.CreateInstance(loaderType);

            if (m_Loader == null)
            {
                Debug.LogError("BpAssetLoader Init failed name:" + className);
            }
        }

        public static bool Inited()
        {
            if(m_Loader != null && m_Loader.Inited())
            {
                return true;
            }

            Debug.LogError("Blueprint Asset Loader not Init!!");
            return false;
        }

        public static T SpawnActor<T>(string path) where T : ActorBase
        {
            if (!Inited())
            {
                return null;
            }

            return m_Loader.SpawnActor<T>(path);
        }

        public static T SpawnActor<T>(string path, Transform parent) where T : ActorBase
        {
            if (!Inited())
            {
                return null;
            }

            return m_Loader.SpawnActor<T>(path, parent);
        }

        public static ActorBase SpawnActor(string path)
        {
            if (!Inited())
            {
                return null;
            }

            return m_Loader.SpawnActor(path);
        }

        public static ActorBase SpawnActor(string path, Transform parent)
        {
            if (!Inited())
            {
                return null;
            }

            return m_Loader.SpawnActor(path, parent);
        }

        public static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            if (!Inited())
            {
                return null;
            }

            return m_Loader.LoadAsset<T>(path);
        }

        public static T LoadAsset<T>(string path, Action release) where T : UnityEngine.Object
        {
            if (!Inited())
            {
                return null;
            }

            return m_Loader.LoadAsset<T>(path, release);
        }
    }
}
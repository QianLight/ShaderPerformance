using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    public class AssetLevelManager
    {
        private static AssetLevelManager m_instance;
        public static AssetLevelManager Instance
        {
            get
            {
                if (null == m_instance)
                {
                    m_instance = new AssetLevelManager();
                    m_instance.Init();
                }
                return m_instance;
            }
        }
        /// <summary>
        /// 注意，mappers里不包含default，default是独立存在的
        /// </summary>
        private Dictionary<string, IAssetLevelMapper> m_mappers = new Dictionary<string, IAssetLevelMapper>();
        private AssetLevelMapperDefault m_defaultMappter = new AssetLevelMapperDefault();
        private List<string> m_levelNames = new List<string>();
        private IAssetLevelMapper m_currentMapper;
        private AssetManagerSetting _setting;

        private void Init()
        {
            _setting = AssetManagerSetting.LoadSetting();
            var config = AssetLevelConfigs.Load();
            for(var i = 0; i < config.Levels.Count; i++)
            {
                var item = config.Levels[i];
                var assembly = Assembly.Load(item.AssemblyName);
                if(null == assembly)
                {
                    Debug.LogError($"load assembly {item.AssemblyName} failed, maybe scripts was updated but config wasn't refresh");
                    continue;
                }
                var mapperType = Assembly.Load(item.AssemblyName).GetType(item.ClassName);
                if(null == mapperType)
                {
                    Debug.LogError($"get type {item.ClassName} falied, maybe scripts was updated but config wasn't refresh");
                    continue;
                }
                var mapper = Activator.CreateInstance(mapperType) as IAssetLevelMapper;
                if(null == mapper)
                {
                    Debug.LogError($"Mapper for level {item.LevelName} is null, no {mapperType} found, or it's not IAssetLevelMapper");
                    continue;
                }
                m_mappers[item.LevelName] = mapper;
            }
        }

        public string GetLevelByAsset(string assetName)
        {
            foreach(var pair in m_mappers)
            {
                if(pair.Value.IsPathForThisLevel(assetName))
                {
                    return pair.Value.Level;
                }
            }
            return m_defaultMappter.Level;
        }
        
        public bool HasVariantAsset(string assetName)
        {
#if UNITY_EDITOR
            var assetMapBundles = AssetBundleUtils.GetAssetMapBundles();
            foreach (var level in GetAllLevels())
            {
                if (level == m_defaultMappter.Level)
                {
                    continue;
                }
                if (assetMapBundles.ContainsKey(GetMappedPath(level, assetName)))
                {
                    return true;
                }
            }
#endif
            return false;
        }

        public IAssetLevelMapper GetMapperByLevel(string level)
        {
            if(m_mappers.TryGetValue(level, out var mapper))
            {
                if(null == mapper)
                {
                    Debug.LogError($"mapper for level {level} is null");
                    return m_defaultMappter;
                }
                return mapper;
            }
            return m_defaultMappter;
        }

        public string GetMappedPath(string level, string assetPath)
        {
            var mapper = GetMapperByLevel(level);
            return mapper.GetAssetPathForLevel(assetPath);
        }

        public List<string> GetAllLevels()
        {
            bool enableLevel = false;
            switch (_setting.assetLoaderType)
            {
                case AssetLoaderType.AssetBundle:
                    enableLevel = _setting.bundleLoaderSetting.enableAssetLevel;
                    break;
                case AssetLoaderType.AssetDatabase:
                    enableLevel = _setting.assetDatabaseSetting.enableAssetLevel;
                    break;
            }
            if (!enableLevel)
            {
                return new List<string>();
            }
            
            if(m_levelNames.Count < 1)
            {
                foreach(var pair in m_mappers)
                {
                    m_levelNames.Add(pair.Key);
                }
                m_levelNames.Add(m_defaultMappter.Level);
            }
            return new List<string>(m_levelNames);
        }

        public void SetLevel(string level)
        {
            if(level != null)
            {
                m_currentMapper = GetMapperByLevel(level);
            }
            else
            {
                m_currentMapper = m_defaultMappter;
            }
        }

        public string GetMappedPath(string assetPath, AssetLoaderType loaderType)
        {
            if(m_currentMapper == null)
            {
                return assetPath;
            }
            else
            {
                return m_currentMapper.GetFinalAssetPathForLevel(assetPath, loaderType);
            }
        }

        public string GetDefaultLevel()
        {
            bool enableLevel = false;
            switch (_setting.assetLoaderType)
            {
                case AssetLoaderType.AssetBundle:
                    enableLevel = _setting.bundleLoaderSetting.enableAssetLevel;
                    break;
                case AssetLoaderType.AssetDatabase:
                    enableLevel = _setting.assetDatabaseSetting.enableAssetLevel;
                    break;
            }
            if (!enableLevel)
            {
                return null;
            }
            else
            {
                return m_defaultMappter.Level;
            }
        }
    }
}
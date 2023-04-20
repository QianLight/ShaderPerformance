using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace Zeus.Framework.Asset
{

    public class AssetLevelWindow : EditorWindow
    {

        private const string PreviewButtonName = "结果预览";
        [MenuItem("Zeus/Asset/AssetLevel")]
        private static void Open()
        {
            var window = GetWindow<AssetLevelWindow>(title:"AssetLevelConfigWindow");
            var config = window.GetConfigs();
            window.InitClassesForConfig(config);
            window.RefreshConfigIndices(config);
        }

        private int? m_indexToRemove;
        private AssetLevelConfigs m_configs;
        private AssetLevelConfigs GetConfigs()
        {
            if (null == m_configs)
            {
                m_configs = AssetLevelConfigs.Load();
            }
            return m_configs;
        }

        private string[] m_supportedAssemblys = new string[]
        {
            "Assembly-CSharp",
            "Zeus.Asset",
        };
        private string[] m_assemblysHasMapperTypes;
        private Dictionary<string, string[]> m_mapperTypes = new Dictionary<string, string[]>();

        private void RefreshConfigIndices(AssetLevelConfigs config)
        {
            foreach (var item in config.Levels)
            {
                item.AssemblyIndex = GetAssemblyNameIndex(item, m_assemblysHasMapperTypes);
                item.ClassNameIndex = GetClassNameIndex(item, m_mapperTypes);
            }
            var defaultConfig = AssetLevelConfigs.DefaultMapperConfig;
            defaultConfig.AssemblyIndex = GetAssemblyNameIndex(defaultConfig, m_assemblysHasMapperTypes);
            defaultConfig.ClassNameIndex= GetClassNameIndex(defaultConfig, m_mapperTypes);
        }

        private int GetAssemblyNameIndex(AssetLevelMapperConfig mapper, string[] assemblys)
        {
            for (var i = 0; i < assemblys.Length; i++)
            {

                if (mapper.AssemblyName == assemblys[i])
                {
                    return i;
                }
            }
            return 0;
        }

        private int GetClassNameIndex(AssetLevelMapperConfig mapper, Dictionary<string, string[]> mapperTypes )
        {
            foreach (var clsPair in mapperTypes)
            {
                var assemblyName = clsPair.Key;
                if (mapper.AssemblyName == assemblyName)
                {
                    var typeNames = clsPair.Value;
                    for (var i = 0; i < typeNames.Length; i++)
                    {
                        if (mapper.AssemblyName == assemblyName && mapper.ClassName == typeNames[i])
                        {
                            return i;
                        }
                    }
                }
            }
            return 0;
        }

        private bool ValidMapperType(string assemblyName, string typeName)
        {
            var assembly = Assembly.Load(assemblyName);
            if(null == assembly)
            {
                Error($"找不到程序集:{assemblyName}");
                return false;
            }
            var type = assembly.GetType(typeName);
            if(null == type)
            {
                Error($"在程序集{assemblyName}中找不到类型{type}");
                return false;
            }
            var attr = type.GetCustomAttribute<UnityEngine.Scripting.PreserveAttribute>();
            if(null == attr)
            {
                Error($"{type} 缺少 UnityEngine.Scripting.PreserveAttribute\n 请添加一下，防止被Trim掉");
                return false;
            }
            return true;
        }

        private void InitClassesForConfig(AssetLevelConfigs config)
        {
            var interfaceType = typeof(IAssetLevelMapper);
            var defaultConfig = AssetLevelConfigs.DefaultMapperConfig;
            for(var i = 0; i< m_supportedAssemblys.Length; i++)
            {
                var classNames = new List<string>();
                var assemblyName = m_supportedAssemblys[i];
                var assembly = Assembly.Load(assemblyName);
                if(null == assembly)
                {
                    Error($"找不到程序集{assemblyName}，请检查名称是否正确");
                }
                else
                {
                    var types = assembly.GetTypes();
                    foreach(var type in types)
                    {
                        //默认配置不可选，不暴露给pop
                        if(type != interfaceType 
                            && interfaceType.IsAssignableFrom(type) 
                            && !(assemblyName == defaultConfig.AssemblyName && type.FullName == defaultConfig.ClassName))
                        {
                            classNames.Add(type.FullName);
                        }
                    }
                }
                if(classNames.Count > 0)
                {
                    m_mapperTypes[assemblyName] = classNames.ToArray();
                }
            }
            m_assemblysHasMapperTypes = m_mapperTypes.Keys.ToArray();
        }

        private void Tip(string message)
        {
            Debug.Log(message);
            EditorUtility.DisplayDialog("Tip", message, "ok");
        }

        private void Error(string errorMessage)
        {
            Debug.LogError(errorMessage);
            EditorUtility.DisplayDialog("Error", errorMessage, "ok");
        }

        private void ShowTitle()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level", GUILayout.Width(LevelWidthInGUI));
            EditorGUILayout.LabelField("AssemblyName", GUILayout.Width(AssemblyWidthInGUI));
            EditorGUILayout.LabelField("Mapper TypeName", GUILayout.Width(ClassNameWidthInGUI));
            EditorGUILayout.EndHorizontal();
        }

        private const int LevelWidthInGUI = 150;
        private const int AssemblyWidthInGUI = 200;
        private const int ClassNameWidthInGUI = 500;

        private void ShowDefaultLevel()
        {
            EditorGUILayout.BeginHorizontal();
            var level = AssetLevelConfigs.DefaultMapperConfig;
            EditorGUILayout.LabelField(level.LevelName, GUILayout.Width(LevelWidthInGUI));
            EditorGUILayout.LabelField(level.AssemblyName, GUILayout.Width(AssemblyWidthInGUI));
            EditorGUILayout.LabelField(level.ClassName, GUILayout.Width(ClassNameWidthInGUI));
            ShowTryButton(level);
            EditorGUILayout.EndHorizontal();
        }

        private void ShowLevel(AssetLevelMapperConfig config, int index)
        {
            EditorGUILayout.BeginHorizontal();
            var refreshLevelNameNeeded = string.IsNullOrEmpty(config.LevelName);
            EditorGUILayout.LabelField(config.LevelName, GUILayout.Width(LevelWidthInGUI));
            var lastAssemblyIndex = config.AssemblyIndex;
            config.AssemblyIndex = EditorGUILayout.Popup(config.AssemblyIndex, m_assemblysHasMapperTypes, GUILayout.Width(AssemblyWidthInGUI));
            if (config.AssemblyIndex != lastAssemblyIndex)
            {
                config.ClassNameIndex = 0;
                refreshLevelNameNeeded = true;
            }
            var lastClasName = config.ClassName;
            config.AssemblyName = m_assemblysHasMapperTypes[config.AssemblyIndex];
            var classes = m_mapperTypes[m_assemblysHasMapperTypes[config.AssemblyIndex]];
            config.ClassNameIndex = EditorGUILayout.Popup(config.ClassNameIndex, classes, GUILayout.Width(ClassNameWidthInGUI));
            config.ClassName = classes[config.ClassNameIndex];
            if (config.ClassName != lastClasName)
            {
                refreshLevelNameNeeded = true;
            }
            if (refreshLevelNameNeeded)
            {
                var mapperType = Assembly.Load(config.AssemblyName).GetType(config.ClassName);
                var mapperInstance = Activator.CreateInstance(mapperType) as IAssetLevelMapper;
                config.LevelName = mapperInstance.Level;
            }
            ShowTryButton(config);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                m_indexToRemove = index;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowTryButton(AssetLevelMapperConfig item)
        {
            if (GUILayout.Button(PreviewButtonName, GUILayout.Width(100)))
            {
                var type = Assembly.Load(item.AssemblyName).GetType(item.ClassName);
                if(null == type)
                {
                    Error($"在程序集{item.AssemblyName}中，找不到{item.ClassName}");
                    return;
                }
                var mapper = Activator.CreateInstance(type) as IAssetLevelMapper;
                if(null == mapper)
                {
                    Error($"类型{item.AssemblyName} 没有继承自IAssetLevelMapper接口");
                    return;
                }
                Tip($"{mapper.Level} {PreviewButtonName}\n输入: \nAssets/Res/defaultAssetName \n输出: \n{mapper.GetAssetPathForLevel("Assets/Res/defaultAssetName")}");
            }
        }

        private bool ValidConfig(AssetLevelConfigs config)
        {
            var typeSet = new HashSet<string>();
            var nameSet = new HashSet<string>();
            for(var i = 0; i < config.Levels.Count; i++)
            {
                var item = config.Levels[i];
                if (string.IsNullOrEmpty(item.LevelName))
                {
                    Error("Level名称不能为空");
                    return false;
                }
                if(nameSet.Contains(item.LevelName))
                {
                    Error("选择了重复得Level配置，请检查");
                    return false;
                }
                nameSet.Add(item.LevelName);
                var typeFullName = $"{item.AssemblyName}.{item.ClassName}";
                if(typeSet.Contains(typeFullName))
                {
                    Error("不同的Level不能有相同的Mapper，请检查下拉框选择的内容是否有重复");
                    return false;
                }

                if(!ValidMapperType(item.AssemblyName, item.ClassName))
                {
                    return false;
                }
                typeSet.Add(typeFullName);
            }
            return true;
        }

        private void OnGUI()
        {
            var configs = GetConfigs();
            GUILayout.Space(10);
            EditorGUILayout.HelpBox($"  Level填对应的Level名称，然后通过下拉框选择对应的Mapper类型，如果在下拉框中没有找到，请检查是否继承自IAssetLevelMapper。\n  如果没有看到想要的程序集，并且想要添加支持，请联系Zeus组进行添加\n  点击{PreviewButtonName}可以预览Mapper的转换结果", MessageType.Info, true);
            if (null == configs)
            {
                GUILayout.Label("no config exists");
                return;
            }
            ShowTitle();
            ShowDefaultLevel();
            for(var i = 0; i< configs.Levels.Count; i++)
            {
                var item = configs.Levels[i];
                ShowLevel(item, i);
            }
            if(m_indexToRemove.HasValue)
            {
                configs.Levels.RemoveAt(m_indexToRemove.Value);
                m_indexToRemove = null;
            }
            if(GUILayout.Button("+", GUILayout.Width(20)))
            {
                configs.Levels.Add(new AssetLevelMapperConfig
                {
                    LevelName = "",
                    ClassName = "",
                }); ;
            }
            GUILayout.Space(30);
            if (GUILayout.Button("Save"))
            {
                if(ValidConfig(configs))
                {
                    configs.Save();
                }
            }
        }
    }
}

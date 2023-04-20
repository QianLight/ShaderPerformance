using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    public class HLODRecordTool
    {
        public const string configPath = "Assets/Athena/ConfigSetting";
        
        public static void SaveData(HLODSceneEditorData sceneEditorData, string path)
        {
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }
            HLODConfigSetting data = new HLODConfigSetting();
            data.globalSetting = sceneEditorData.globalSetting.Clone();
            data.proxyMapType = sceneEditorData.proxyMapType;
            data.textureChannel = sceneEditorData.textureChannel;
            data.hlodMethod = sceneEditorData.hlodMethod;
            data.useVoxel = sceneEditorData.useVoxel;
            data.rendererBakerSetting = sceneEditorData.rendererBakerSetting.Clone();
            data.shaderBindConfig = sceneEditorData.shaderBindConfig.Clone();
            foreach (var sceneNode in sceneEditorData.scenes)
            {
                if (sceneNode.targetParent)
                {
                    data.targetParentNamePath = sceneEditorData.scenes[0].targetParent.GetObjNamePath();
                    break;
                }
            }

            var roots = new HashSet<GameObject>();
            foreach (var sceneNode in sceneEditorData.scenes)
            {
                foreach (var root in sceneNode.roots)
                {
                    var fullname = root.GetObjNamePath();
                    if (data.rootNamePaths.Contains(fullname) == false) data.rootNamePaths.Add(fullname);
                }
            }

            AssetDatabase.CreateAsset(data, path);
        }

        public static List<string> GetConfigNames()
        {
            var allPath = AssetDatabase.FindAssets("t:HLODConfigSetting", new string[] {configPath});
            List<string> names = new List<string>();
            for (int i = 0; i < allPath.Length; i++)
            {
                names.Add(Path.GetFileNameWithoutExtension(allPath[i]));
            }
            return names;
        }

        public static HLODConfigSetting LoadData(string name)
        {
            var data = AssetDatabase.LoadAssetAtPath<HLODConfigSetting>(configPath + "/" + name + ".asset");
            return data;
        }
    }
}
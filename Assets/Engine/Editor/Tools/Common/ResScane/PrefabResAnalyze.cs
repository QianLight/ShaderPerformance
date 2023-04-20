using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CFEngine.Editor
{
    public class PrefabResScanPolicy : ScanPolicy
    {
        public override string ScanType
        {
            get { return "Prefab"; }
        }
        public override string ResExt
        {
            get { return "*.asset"; }
        }
        MaterialContext context = new MaterialContext();
        public override ResItem Scan(string name, string path, OrderResList result, ResScanConfig config)
        {
            name = name.Replace(".asset", ".prefab");
            string prefabPath = string.Format("{0}Prefabs/{1}", LoadMgr.singleton.BundlePath, name);
            if (File.Exists(prefabPath))
            {
                var prefabRes = AssetDatabase.LoadAssetAtPath<CFEngine.PrefabRes>(path);
                if (prefabRes != null)
                {                    
                    path = string.Format("{0}Runtime/Prefab/{1}", LoadMgr.singleton.BundlePath, name);
                    if (File.Exists(path))
                    {
                        var prefab = result.Add(null, name, path);
                        if (prefabRes.meshes != null)
                        {
                            for (int i = 0; i < prefabRes.meshes.Length; ++i)
                            {
                                var emi = prefabRes.meshes[i];
                                if (emi != null && !string.IsNullOrEmpty(emi.meshPath))
                                {
                                    AssetsPath.GetFileName(emi.meshPath, out var meshName);
                                    result.Add(prefab, meshName + ".asset", emi.meshPath + ".asset");
                                    var mat = AssetDatabase.LoadAssetAtPath<Material>(emi.matPath);
                                    if (mat != null)
                                    {
                                        context.Init();
                                        MaterialShaderAssets.ResolveMatProperty(mat, ref context);
                                        for (int j = 0; j < context.textureValue.Count; ++j)
                                        {
                                            var stpv = context.textureValue[j];
                                            string texExt = ResObject.GetExt(stpv.texType);
                                            result.Add(prefab, stpv.path + texExt, stpv.physicPath + texExt);
                                            //AddResName(stpv.path, stpv.value);
                                        }
                                    }
                                }
                            }
                        }

                        return prefab;
                    }

                }

            }
            return null;
        }
    }

}
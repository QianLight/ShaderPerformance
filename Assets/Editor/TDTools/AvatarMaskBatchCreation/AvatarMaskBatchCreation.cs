using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TDTools {
    class AvatarMaskBatchCreation  {

        static HashSet<string> GetAllPath(Transform prefab, string currentPath) {
            HashSet<string> results = new HashSet<string>();
            results.Add(currentPath);
            Transform currentTransform = prefab.Find(currentPath);
            foreach (Transform child in currentTransform)
                results.UnionWith(GetAllPath(prefab, $"{currentPath}/{child.name}"));
            return results;
        }

        [MenuItem("Assets/Create/TDTools/批量创建Avatar Mask")]
        static void CreateAvatarMask() {

            string prefabPath = "Assets/BundleRes/Prefabs/";

            string fileInputPath = EditorUtility.OpenFilePanel("选择输入文件", "", "");
            if (!File.Exists(fileInputPath))
                return;

            List<string> input = new List<string>();
            using StreamReader sr = new StreamReader(fileInputPath);
            while (!sr.EndOfStream) {
                input.Add(sr.ReadLine());
            }

            for (int i = 0; i < input.Count; i++) {
                try {
                    var path = "";
                    var obj = Selection.activeObject;
                    if (obj == null) path = "Assets/";
                    else path = AssetDatabase.GetAssetPath(obj.GetInstanceID()) + "/";

                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}{input[i]}.prefab");
                    HashSet<string> pathSet = GetAllPath(prefab.transform, "root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1");

                    Transform root = prefab.transform.Find("root");

                    AvatarMask mask = new AvatarMask();
                    mask.AddTransformPath(root, true);

                    for (int j = 0; j < mask.transformCount; j++) {
                        string tName = mask.GetTransformPath(j);
                        mask.SetTransformActive(j, pathSet.Contains(tName));
                    }

                    AssetDatabase.CreateAsset(mask, $"{path}{input[i]}.mask");
                } catch {
                    Debug.Log($"创建 {input[i]} 失败");
                }
            }
        }
    }

}
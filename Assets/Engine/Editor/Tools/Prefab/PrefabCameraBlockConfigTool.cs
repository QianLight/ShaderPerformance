using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CFEngine.Editor
{
    public class PrefabCameraBlockConfigTool : CommonToolTemplate
    {
        private enum OpType
        {
            OpNone,
            OpSaveAllPrefabProperty,
            OpResetAllState,
        }
        
        private OpType opType = OpType.OpNone;
        private CameraBlockGroup.BlockType blockType = CameraBlockGroup.BlockType.RoleOrMonster;

        private CameraBlockConfig[] blockConfigs;

        private GUILayoutOption widthOption = GUILayout.Width(300);
        
        private readonly string scriptPath = "Assets/Engine/Runtime/Scene/System/CameraBlockGroup.cs";
        private MonoScript cameraBlockGroupScrpit;

        private string[] filterPaths = new[]
        {
            "Assets/BundleRes/Curve",
            "Assets/BundleRes/EditorAssetRes/UIScene",
            "Assets/BundleRes/Effects/Prefab",
            "Assets/BundleRes/Effects/Prefabs",
            "Assets/BundleRes/Runtime/Prefab/LevelObject",
            "Assets/BundleRes/Runtime/Prefab/CutScene",
            "Assets/BundleRes/Runtime/SFX",
            "Assets/BundleRes/Scene",
            "Assets/BundleRes/Timeline",
            "Assets/BundleRes/UI/OPsystemprefab",
            "Assets/Editor",
            
            
        };
        public override void OnInit ()
        {
            base.OnInit ();
            blockConfigs = new CameraBlockConfig[2];
            for (int i = 0; i < blockConfigs.Length; i++)
            {
                blockConfigs[i] = new CameraBlockConfig(0f,0.2f,0f,0.3f);
            }
            
            cameraBlockGroupScrpit = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
        }

        public override void OnUninit ()
        {
            base.OnUninit ();
            blockConfigs = null;
            cameraBlockGroupScrpit = null;
        }

        public override void DrawGUI (ref Rect rect)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("预制键遮挡半透明配置",widthOption);

            EditorGUI.indentLevel = 2;
            
            EditorGUILayout.BeginHorizontal ();
            cameraBlockGroupScrpit = EditorGUILayout.ObjectField("引用的脚本", cameraBlockGroupScrpit, typeof(MonoScript), false, widthOption) as MonoScript;
            EditorGUILayout.EndHorizontal ();
            
            EditorGUILayout.BeginHorizontal ();
            blockType = (CameraBlockGroup.BlockType)EditorGUILayout.EnumPopup("类型选择", blockType, widthOption);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginVertical();
            
            blockConfigs[(int)blockType].enterDelayTime = EditorGUILayout.FloatField("进入延迟时间", blockConfigs[(int)blockType].enterDelayTime, widthOption);
            blockConfigs[(int)blockType].enterLerpTime = EditorGUILayout.FloatField("淡入持续时间", blockConfigs[(int)blockType].enterLerpTime, widthOption);
            blockConfigs[(int)blockType].leaveDelayTime = EditorGUILayout.FloatField("推出延迟时间", blockConfigs[(int)blockType].leaveDelayTime, widthOption);
            blockConfigs[(int)blockType].minTransparency = EditorGUILayout.Slider("透明度",blockConfigs[(int)blockType].minTransparency,0f,1f, widthOption);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical ();
            if (GUILayout.Button("批量保存", widthOption))
            {
                string desc = "";
                switch (blockType)
                {
                    case  CameraBlockGroup.BlockType.RoleOrMonster :
                        desc = "是否批量保存【角色和怪物预制件】的遮挡变透明配置，可能会花费较长时间?";
                        break;
                    case CameraBlockGroup.BlockType.SceneObject:
                        desc = "是否批量保存【场景物体预制件】的遮挡变透明配置，可能会花费较长时间?";
                        break;;
                }
                if (EditorUtility.DisplayDialog("批量保存", desc, "是", "否"))
                {
                    opType = OpType.OpSaveAllPrefabProperty;
                }
            }
            
            EditorGUILayout.Space(15);
            if (GUILayout.Button("批量恢复默认状态", widthOption))
            {
                if (EditorUtility.DisplayDialog("批量恢复默认状态", "是否批量重置所有预制件的CameraBlockGroup属性为默认状态，可能会花费较长时间?", "是", "否"))
                {
                    opType = OpType.OpResetAllState;
                }
            }
            EditorGUILayout.EndVertical ();
        }

        public override void Update ()
        {
            switch (opType)
            {
                case OpType.OpSaveAllPrefabProperty:
                    SaveAllPrefabProperty();
                    break;
                case OpType.OpResetAllState:
                    ResetAllBlockProperty();
                    break;;
            }
            opType = OpType.OpNone;
        }

        private void SaveAllPrefabProperty()
        {
            if (!GetAllAssetsPath(out string[] allAssetPaths))
            {
                return;
            }
            
            SavePrefabProperty(allAssetPaths);

            EditorUtility.DisplayDialog("所有预制体CameraBlockGroup属性批量更改", "修改完成!", "OK");
        }

        private void ResetAllBlockProperty()
        {
            if (!GetAllAssetsPath(out string[] allAssetPaths))
            {
                return;
            }
            
            RevertAllPrefabBlockProperty(allAssetPaths);
            EditorUtility.DisplayDialog("重置所有预制体CameraBlockGroup属性", "修改完成!", "OK");
        }

        private bool GetAllAssetsPath(out string[] allAssetPaths)
        {
            if (cameraBlockGroupScrpit == null)
            {
                EditorUtility.DisplayDialog("Error", "CameraBlockGroup 脚本为Null，请先添加", "OK");
                allAssetPaths = null;
                return false;
            }
            
            EditorUtility.DisplayProgressBar("查找所有引用的预制体中","查找所有引用的预制体中",0.5f);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            HashSet<string> containers = new HashSet<string>() {".prefab"};
            allAssetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(x => x.StartsWith("Assets"))
                .Where(x => !AssetDatabase.IsValidFolder(x))
                .Where(x => containers.Contains(Path.GetExtension(x).ToLower()))
                .Where(x=>FilterPath(x))
                .ToArray();

            EditorUtility.ClearProgressBar();
            
            DependencyFilter(ref allAssetPaths);
            stopwatch.Stop();
            Debug.Log("查找引用预制件用时： " + stopwatch.ElapsedMilliseconds / 1000f);
            string[] dependencyStrs = AssetDatabase.GetDependencies(allAssetPaths[0]);

            return true;
        }

        private void SavePrefabProperty(string[] allAssetPaths)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int length = allAssetPaths.Length;

            StringBuilder failRecoderStr = new StringBuilder();
            
            for (int i = 0; i < length; i++)
            {
                string tempPath = allAssetPaths[i];

                if(!CheckValid(tempPath, out GameObject tempPrefab, out CameraBlockGroup tempBlockGroup)) continue;
                
                EditorUtility.DisplayProgressBar("预制体CameraBlockGroup属性批量更改",tempPath,i * 1.0f / length );

                if (tempBlockGroup.blockType != blockType) continue;
                
                tempBlockGroup.config.enterDelayTime = blockConfigs[(int) blockType].enterDelayTime;
                tempBlockGroup.config.enterLerpTime = blockConfigs[(int) blockType].enterLerpTime;
                tempBlockGroup.config.leaveDelayTime = blockConfigs[(int) blockType].leaveDelayTime;
                tempBlockGroup.config.minTransparency = blockConfigs[(int) blockType].minTransparency;
                PrefabUtility.SavePrefabAsset(tempPrefab, out bool isOk);
                if (!isOk)
                {
                    failRecoderStr.AppendLine(tempPath);
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            stopwatch.Stop();
            
            Debug.Log("更改所有预制件CameraBlockGroup属性耗时： " + stopwatch.ElapsedMilliseconds / 1000f);

            if (failRecoderStr.Length > 0)
            {
                Debug.LogError("以下预制体更改失败，请手动更改： " + failRecoderStr.ToString());
            }
        }

        private void RevertAllPrefabBlockProperty(string[] allAssetPaths)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int length = allAssetPaths.Length;

            StringBuilder failRecoderStr = new StringBuilder();
            
            for (int i = 0; i < length; i++)
            {
                string tempPath = allAssetPaths[i];

                if(!CheckValid(tempPath, out GameObject tempPrefab, out CameraBlockGroup tempBlockGroup)) continue;
                
                EditorUtility.DisplayProgressBar("重置预制体CameraBlockGroup属性",tempPath,i * 1.0f / length );
                
                string[] nameStrs = tempPrefab.name.Split('_');
                if (nameStrs.Length <= 0) continue;
                
                if (string.Equals(nameStrs[0],"Monster"))
                {
                    tempBlockGroup.blockType = CameraBlockGroup.BlockType.RoleOrMonster;
                    tempBlockGroup.config.minTransparency = 0.3f;
                }
                else if (string.Equals(nameStrs[0],"Role"))
                {
                    tempBlockGroup.blockType = CameraBlockGroup.BlockType.RoleOrMonster;
                    tempBlockGroup.config.minTransparency = 0.3f;
                }
                else
                {
                    tempBlockGroup.blockType = CameraBlockGroup.BlockType.SceneObject;
                    tempBlockGroup.config.minTransparency = 0.4f;
                }
                
                tempBlockGroup.config.enterDelayTime = 0f;
                tempBlockGroup.config.enterLerpTime = 0.2f;
                tempBlockGroup.config.leaveDelayTime = 0f;
                tempBlockGroup.isOverrideMiscConfig = false;
                
                PrefabUtility.SavePrefabAsset(tempPrefab, out bool isOk);
                if (!isOk)
                {
                    failRecoderStr.AppendLine(tempPath);
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            stopwatch.Stop();
            
            Debug.Log("重置所有预制件CameraBlockGroup属性耗时： " + stopwatch.ElapsedMilliseconds / 1000f);

            if (failRecoderStr.Length > 0)
            {
                Debug.LogError("以下预制体重置新失败，请手动设置： " + failRecoderStr.ToString());
            }
        }

        private bool CheckValid(string assetPath,out GameObject prefabAsset, out CameraBlockGroup blockGroup)
        {
            blockGroup = null;
            prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefabAsset == null) return false;
            if (!prefabAsset.TryGetComponent(typeof(CameraBlockGroup), out Component tempComponent)) return false;;
                
            blockGroup = tempComponent as CameraBlockGroup;
            if (blockGroup == null) return false;

            return true;
        }

        private bool FilterPath(string targetPath)
        {
            foreach (string path in filterPaths)
            {
                if (targetPath.Contains(path))
                {
                    return false;
                }
            }

            return true;
        }

        private void DependencyFilter(ref string[] allAssetPaths)
        {
            List<string> tempStrList = new List<string>();
            string scrpitPath = AssetDatabase.GetAssetPath(cameraBlockGroupScrpit);

            int len = allAssetPaths.Length;
            for (int i = 0; i < len; i++)
            {
                var isDependency = false;
                EditorUtility.DisplayProgressBar("过滤未引用的预制件",allAssetPaths[i],i * 1.0f / len);
                
                var tempDependencyStrs = AssetDatabase.GetDependencies(allAssetPaths[i]);
                foreach (string itemPath in tempDependencyStrs)
                {
                    if (itemPath == scrpitPath)
                    {
                        isDependency = true;
                        break;
                    }
                }

                if (isDependency)
                {
                    tempStrList.Add(allAssetPaths[i]);
                }
            }

            allAssetPaths = tempStrList.ToArray();
            
            EditorUtility.ClearProgressBar();
        }
    }
}
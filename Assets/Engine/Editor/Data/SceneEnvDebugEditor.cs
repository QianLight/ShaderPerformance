using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine.Editor
{
    [CanEditMultipleObjects, CustomEditor (typeof (SceneEnvDebug))]
    public class SceneEnvDebugEditor : UnityEngineEditor
    {
        private void OnEnable ()
        {
            
        }

        public override void OnInspectorGUI ()
        {
            SceneEnvDebug sed = target as SceneEnvDebug;
            if (sed != null && sed.envBlock != null && sed.profile != null)
            {
                int index = EngineUtility.MaskToIndex(sed.envBlock.areaMask);
                EditorGUILayout.IntSlider("areaID", index, -1, 31);
                EditorGUILayout.Toggle("dirtyCameraFov", sed.envBlock.flag.HasFlag(EnvBlock.Flag_DirtyCameraFov));
                EditorGUILayout.ObjectField("LookAtPos", sed.lookAtPos, typeof(Transform), true);
                EditorGUILayout.Toggle("manualTrigger", sed.envBlock.flag.HasFlag(EnvBlock.Flag_ManualTrigger));
                if(sed.soList.Count>0)
                {
                    sed.soFolder = EditorGUILayout.Foldout(sed.soFolder, string.Format("SceneObjects({0})", sed.soList.Count.ToString()));
                    if (sed.soFolder)
                    {
                        foreach (var sod in sed.soList)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.ObjectField("", sod, typeof(SceneObjectData), true);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                
                EnvArea.OnEnvBlockGUI(sed.profile as EnvAreaProfile, sed.envBlock, null, true, false, null);
                if (!string.IsNullOrEmpty(sed.srcProfilePath))
                {
                    if (GUILayout.Button("Save", GUILayout.MaxWidth(100)))
                    {
                        var srcProfile = sed.srcProfile as EnvAreaProfile;
                        if (srcProfile == null)
                        {
                            srcProfile = AssetDatabase.LoadAssetAtPath<EnvAreaProfile>(sed.srcProfilePath);
                            sed.srcProfile = srcProfile;
                        }
                        if (srcProfile != null)
                        {
                            EnvArea.SaveProfileOnRuntime(srcProfile, sed.profile as EnvAreaProfile);
                        }
                    }
                }
            }
        }
    }
}
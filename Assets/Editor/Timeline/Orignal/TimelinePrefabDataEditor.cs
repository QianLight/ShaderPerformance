using CFEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace XEditor
{
    [CustomEditor(typeof(TimelinePrefabData))]
    public class TimelinePrefabDataEditor : Editor
    {
        private static OriginalSyncLoadEditor m_loader;
        private TimelinePrefabData m_data;
        public static bool m_loaded;

        private void OnEnable()
        {
            m_data = target as TimelinePrefabData;
            if(!Application.isPlaying)
            {
                m_loader = new OriginalSyncLoadEditor(m_data);
            }

            if (!Application.isPlaying && !OrignalEntrainWindow.debugMode && !m_loaded)
            {
                GlobalContex.AddEngineEvent(OnEngineLoad, true);
                m_loaded = true;
            }
        }

        public static void Init()
        {
            m_loaded = false;
            m_loader = null;
        }

        private void OnEngineLoad()
        {
            if (!Application.isPlaying)
            {
                if (m_loader != null)
                {
                    m_loader.LoadChars();
                    m_loader.LoadAllFx();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if(m_loader != null)
            {
                m_loader.OnGUI();
            }
            if(!Application.isPlaying)
            {
                GUILayout.Space(8);
                if (GUILayout.Button("Save Timeline", XEditorUtil.boldButtonStyle)) Save();
            }
        }

        private void Save()
        {
            EditorUtility.SetDirty(m_data);
            m_loader?.OnSave();
            var go = m_data.transform.parent.gameObject;
            string pat = AssetDatabase.GetAssetPath(go);
            bool saveSucc;
            string save_pat = OriginalSetting.LIB + go.name + ".prefab";
            PrefabUtility.ApplyPrefabInstance(go, InteractionMode.UserAction);
            PrefabUtility.SaveAsPrefabAsset(go, save_pat, out saveSucc);
            if (saveSucc)
            {
                AssetDatabase.SaveAssets();
            }
            else
            {
                EditorUtility.DisplayDialog("error", "Save timeline failed, check prefab first", "ok");
            }
        }
    }
}

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class ShaderKeyWord : MatEffectNode
    {
        public override MatEffectTemplate GetEffectTemplate ()
        {
            if (met == null)
            {
                met = new MatEffectTemplate ()
                {
                effectType = (int) MatEffectType.Keyword
                };
            }
            FillData(met);
            return met;
        }

        public override void OnGUI (MatEffectData data)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Keyword", GUILayout.MaxWidth(100));
            data.path = EditorGUILayout.TextField("", data.path,GUILayout.MaxWidth(300));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("KeywordMask", GUILayout.MaxWidth(100));
            KeywordFlag f = (KeywordFlag)data.param;
            EditorGUI.BeginChangeCheck();
            f = (KeywordFlag)EditorGUILayout.EnumFlagsField("", f, GUILayout.MaxWidth(300));
            if (EditorGUI.EndChangeCheck())
            {
                data.param = (uint)f;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
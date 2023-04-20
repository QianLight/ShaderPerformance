#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class RenderActive : MatEffectNode
    {
        [Editable ("active")] public bool active;
        [Editable ("activeOnStart")] public bool activeOnStart;
        public override MatEffectTemplate GetEffectTemplate ()
        {
            if (met == null)
            {
                met = new MatEffectTemplate ()
                {
                effectType = (int) MatEffectType.Active,
                };
            }
            met.v = new Vector4 (active?1 : 0, activeOnStart?1 : 0, 0, 0);
            FillData(met);
            return met;
        }

        public override void InitData (ref float x, ref float y, ref float z, ref float w, ref string path, ref uint param)
        {
            x = active?1 : 0;
            y =  activeOnStart?1 : 0;
            // z = 0;
            // w = 0;
        }

        public override void OnGUI (MatEffectData data)
        {
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Active", GUILayout.MaxWidth (120));
            bool isActive = EditorGUILayout.Toggle (data.x > 0, GUILayout.MaxWidth (300));
            data.x = isActive?1 : 0;
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("ActiveOnStart", GUILayout.MaxWidth (120));
            bool isActiveOnStart = EditorGUILayout.Toggle (data.y > 0, GUILayout.MaxWidth (300));
            data.y = isActiveOnStart?1 : 0;
            EditorGUILayout.EndHorizontal ();
        }
    }
}
#endif
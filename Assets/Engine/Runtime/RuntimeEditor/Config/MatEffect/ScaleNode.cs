#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class ScaleParam : MatEffectNode
    {
        [Editable ("start")] public float scale0;
        [Editable ("end")] public float scale1;

        public override MatEffectTemplate GetEffectTemplate ()
        {
            if (met == null)
            {
                met = new MatEffectTemplate ()
                {
                effectType = (int) MatEffectType.Scale,

                };
            }
            met.v = new Vector4 (scale0, scale1, 0, 0);
            FillData(met);
            return met;
        }
        public override void InitData (ref float x, ref float y, ref float z, ref float w, ref string path, ref uint param)
        {
            x = scale0;
            y = scale1;
        }

        public override void OnGUI (MatEffectData data)
        {
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Start", GUILayout.MaxWidth (100));
            data.x = EditorGUILayout.Slider (data.x, 0, 10, GUILayout.MaxWidth (300));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("End", GUILayout.MaxWidth (100));
            data.y = EditorGUILayout.Slider (data.y, 0, 10, GUILayout.MaxWidth (300));
            EditorGUILayout.EndHorizontal ();
        }
    }
}
#endif
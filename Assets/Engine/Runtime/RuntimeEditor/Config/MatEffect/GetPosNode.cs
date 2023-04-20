#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class GetPos : MatEffectNode
    {
        [Editable("objectPos")] public bool objectPos;
        [Editable("hitPos")] public bool hitPos;

        public override MatEffectTemplate GetEffectTemplate()
        {
            if (met == null)
            {
                met = new MatEffectTemplate()
                {
                    effectType = (int)MatEffectType.GetPos,
                };
            }
            met.flag.SetFlag(MatEffectTemplate.Flag_EntityPos, objectPos);
            met.flag.SetFlag(MatEffectTemplate.Flag_HitPos, hitPos);
            FillData(met);
            return met;
        }

        private void OnValueGUI(string desc, ref float v)
        {
            //EditorGUILayout.LabelField(desc, GUILayout.MaxWidth(100));
            v = EditorGUILayout.FloatField(desc, v, GUILayout.MaxWidth(200));
        }
        public override void OnGUI(MatEffectData data)
        {
            if(hitPos)
            {
                EditorGUILayout.LabelField("Offset");
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                Vector3 pos = new Vector3(data.x, data.y, data.z);
                EditorGUI.BeginChangeCheck();
                pos = EditorGUILayout.Vector3Field("", pos, GUILayout.MaxWidth(300));
                if (EditorGUI.EndChangeCheck())
                {
                    data.x = pos.x;
                    data.y = pos.y;
                    data.z = pos.z;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

        }

        public override void UpdateEffect(IEntityHandler e, MatEffectData med)
        {
            if(e is EffectPreviewContext)
            {
                (e as EffectPreviewContext).localPos = new Vector3(med.x, med.y, med.z);
            }
        }
    }
}
#endif
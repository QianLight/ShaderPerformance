#if UNITY_EDITOR
using CFUtilPoolLib;
using EditorEcs;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VirtualSkill
{
    public class SkillTargetSelectSceneUI : MonoBehaviour
    {
    }

    [CustomEditor(typeof(SkillTargetSelectSceneUI))]
    public class SkillTargetSelectEditor : Editor
    {
        public static EcsData.XTargetSelectData targetSelectData;
        public static bool targetSelectCalPos = false;
        Vector3 targetSelectPosition;
        Quaternion targetSelectRotation;

        private void OnSceneGUI()
        {
            
            Draw(targetSelectData, ref targetSelectCalPos, ref targetSelectPosition, ref targetSelectRotation);
        }

        private void Draw(EcsData.XTargetSelectData data,ref bool calPos,ref Vector3 position,ref Quaternion rotation)
        {
            if (!SkillHoster.targetSelectSceneUI) return;

            if (data == null) return;

            Transform trans = (target as SkillTargetSelectSceneUI).gameObject.transform;
            if (calPos)
            {
                calPos = false;
                position = trans.position;
                rotation = trans.rotation;
            }

            Vector3 m = XCommon.singleton.HorizontalRotateVetor3(rotation * Vector3.forward, 0);
            Vector3 offset = rotation * new Vector3(data.OffsetX, 0, data.OffsetZ);
            if (data.SectorType)
            {
                m = RotateRad(-data.Scope / 2.0f * Mathf.Deg2Rad, trans.up) * m;
                Handles.color = new Color(1f, 0, 0, 0.18f);
                Handles.DrawSolidArc(position + offset,
                                           trans.up,
                                            m,
                                            data.Scope,
                                            data.RangeUpper);
                Handles.color = new Color(0f, 0f, 0f, 0.18f);
                Handles.DrawSolidArc(position + offset,
                                           trans.up,
                                            m,
                                            data.Scope,
                                            data.RangeLower);
            }
            else
            {
                Handles.color = new Color(1f, 0, 0, 0.18f);
                Vector3[] vecs = new Vector3[4];
                Quaternion q = XCommon.singleton.VectorToQuaternion(m);

                vecs[0] = position + offset + q * new Vector3(-data.Scope / 2.0f, 0, (-data.RangeUpper / 2.0f));
                vecs[1] = position + offset + q * new Vector3(-data.Scope / 2.0f, 0, data.RangeUpper / 2.0f);
                vecs[2] = position + offset + q * new Vector3(data.Scope / 2.0f, 0, data.RangeUpper / 2.0f);
                vecs[3] = position + offset + q * new Vector3(data.Scope / 2.0f, 0, (-data.RangeUpper / 2.0f));

                Handles.DrawSolidRectangleWithOutline(vecs, new Color(1, 0, 0, 1), new Color(0, 0, 0, 1));

                Handles.color = Color.green;
            }
        }

        private Quaternion RotateRad(float rad, Vector3 axis)
        {
            float r = rad / 2.0f;

            float x = axis.x * Mathf.Sin(r);
            float y = axis.y * Mathf.Sin(r);
            float z = axis.z * Mathf.Sin(r);
            float w = Mathf.Cos(r);

            return new Quaternion(x, y, z, w);
        }
    }
}
#endif
#if UNITY_EDITOR
using CFUtilPoolLib;
using EditorEcs;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VirtualSkill
{
    public class SkillResultSceneUI : MonoBehaviour
    {
    }

    [CustomEditor(typeof(SkillResultSceneUI))]
    public class SkillResultEditor : Editor
    {
        public static EcsData.XResultData rangeData;
        public static bool rangeCalPos = false;
        Vector3 rangePosition;
        Quaternion rangeRotation;

        public static EcsData.XResultData resultData;
        public static bool resultCalPos = false;
        Vector3 resultPosition;
        Quaternion resultRotation;

        private void OnSceneGUI()
        {
            if (!string.IsNullOrEmpty(SkillHoster.GetHoster.attackRange))
            {
                try
                {
                    rangeData = new EcsData.XResultData();
                    string[] range = SkillHoster.GetHoster.attackRange.Split('|');
                    rangeData.Range = range.Length > 0 ? float.Parse(range[0]) : 0;
                    rangeData.LowRange = range.Length > 1 ? float.Parse(range[1]) : 0;
                    rangeData.SectorType = range.Length > 2 ? range[2] == "1" : true;
                    rangeData.Scope = range.Length > 3 ? float.Parse(range[3]) : 0;
                    rangeData.OffsetX = range.Length > 4 ? float.Parse(range[4]) : 0;
                    rangeData.OffsetZ = range.Length > 5 ? float.Parse(range[5]) : 0;
                    rangeData.AngleShift = range.Length > 6 ? int.Parse(range[6]) : 0;
                    rangeCalPos = true;
                    Draw(rangeData, ref rangeCalPos, ref rangePosition, ref rangeRotation);
                }
                catch
                {

                }
            }
            Draw(resultData, ref resultCalPos, ref resultPosition, ref resultRotation);
        }

        private void Draw(EcsData.XResultData data,ref bool calPos,ref Vector3 position,ref Quaternion rotation)
        {
            if (SkillHoster.hideSceneUI) return;

            if (data == null) return;

            Transform trans = (target as SkillResultSceneUI).gameObject.transform;
            if (calPos)
            {
                calPos = false;
                if (data.GetPositionIndex != -1)
                {
                    float y = Xuthus_VirtualServer.getPosFromGetterY(SkillHoster.PlayerIndex, data.GetPositionIndex);
                    float x = Xuthus_VirtualServer.getPosFromGetterX(SkillHoster.PlayerIndex, data.GetPositionIndex);
                    float z = Xuthus_VirtualServer.getPosFromGetterZ(SkillHoster.PlayerIndex, data.GetPositionIndex);
                    position = new Vector3(x, y, z);
                }
                else position = trans.position;
                if (data.GetDirectionIndex != -1)
                {
                    float dir = Xuthus_VirtualServer.getFaceFromGetter(SkillHoster.PlayerIndex, data.GetDirectionIndex);
                    rotation = Quaternion.Euler(0, dir, 0);

                }
                else rotation = trans.rotation;
            }

            Vector3 m = XCommon.singleton.HorizontalRotateVetor3(rotation * Vector3.forward, data.AngleShift);
            Vector3 offset = rotation * new Vector3(data.OffsetX, data.OffsetY, data.OffsetZ);
            if (data.SectorType)
            {
                m = RotateRad(-data.Scope / 2.0f * Mathf.Deg2Rad, trans.up) * m;
                Handles.color = new Color(1f, 1f, 1f, 0.18f);
                Handles.DrawSolidArc(position + offset,
                                           trans.up,
                                            m,
                                            data.Scope,
                                            data.Range);
                Handles.color = new Color(0f, 0f, 0f, 0.18f);
                Handles.DrawSolidArc(position + offset,
                                           trans.up,
                                            m,
                                            data.Scope,
                                            data.LowRange);
            }
            else
            {
                Handles.color = new Color(1f, 1f, 1f, 0.18f);
                Vector3[] vecs = new Vector3[4];
                Quaternion q = XCommon.singleton.VectorToQuaternion(m);

                vecs[0] = position + offset + q * new Vector3(-data.Scope / 2.0f, 0, (-data.Range / 2.0f));
                vecs[1] = position + offset + q * new Vector3(-data.Scope / 2.0f, 0, data.Range / 2.0f);
                vecs[2] = position + offset + q * new Vector3(data.Scope / 2.0f, 0, data.Range / 2.0f);
                vecs[3] = position + offset + q * new Vector3(data.Scope / 2.0f, 0, (-data.Range / 2.0f));

                Handles.DrawSolidRectangleWithOutline(vecs, new Color(1, 1, 1, 1), new Color(0, 0, 0, 1));

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
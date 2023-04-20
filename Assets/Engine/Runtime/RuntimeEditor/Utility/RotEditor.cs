#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public class RotContext
    {
        public float[] EulerFloats = new float[3];
        public string label = "";
    }
    public class RotEditor
    {
        private Vector3 m_EulerAngles;
        private Vector3 m_OldEulerAngles = new Vector3 (1000000, 10000000, 1000000);
        private int m_OldRotationOrder = 4; //OrderZXY

        public Transform trans;
        static ReflectFun transGetLocalEulerAngles;
        static object[] GetLocalEulerAnglesParam = new object[1];
        static ReflectFun transSetLocalEulerAngles;
        static object[] SetLocalEulerAnglesParam = new object[2];
        static ReflectFun transSendTransformChangedScale;
        static ReflectFun editorMultiFieldPrefixLabel;
        static ReflectFun transGetRotationOrderInternal;
        static object[] MultiFieldPrefixLabelParam = new object[4];
        public void OnInit (Transform t)
        {
            trans = t;
            if (transGetLocalEulerAngles == null)
            {
                transGetLocalEulerAngles = EditorCommon.GetInternalFunction (typeof (Transform), "GetLocalEulerAngles", false, true, true, false);
            }
            if (transSetLocalEulerAngles == null)
            {
                transSetLocalEulerAngles = EditorCommon.GetInternalFunction (typeof (Transform), "SetLocalEulerAngles", false, true, true, false);
            }
            if (transSendTransformChangedScale == null)
            {
                transSendTransformChangedScale = EditorCommon.GetInternalFunction (typeof (Transform), "SendTransformChangedScale", false, true, true, false);
            }
            if (transGetRotationOrderInternal == null)
            {
                transGetRotationOrderInternal = EditorCommon.GetInternalFunction (typeof (Transform), "GetRotationOrderInternal", false, true, true, false);
            }
            if (editorMultiFieldPrefixLabel == null)
            {
                editorMultiFieldPrefixLabel = EditorCommon.GetInternalFunction (typeof (EditorGUI), "MultiFieldPrefixLabel", true, true, false, false);
            }
        }

        public void BeginRotationField (RotContext context)
        {
            if (trans != null && transGetLocalEulerAngles != null)
            {
                int rotationOrder = (int) transGetRotationOrderInternal.Call (trans, null);
                GetLocalEulerAnglesParam[0] = rotationOrder;
                Vector3 localEuler = (Vector3) transGetLocalEulerAngles.Call (trans, GetLocalEulerAnglesParam);
                if (
                    m_OldEulerAngles.x != localEuler.x ||
                    m_OldEulerAngles.y != localEuler.y ||
                    m_OldEulerAngles.z != localEuler.z ||
                    m_OldRotationOrder != rotationOrder
                )
                {
                    GetLocalEulerAnglesParam[0] = rotationOrder;
                    m_EulerAngles = (Vector3) transGetLocalEulerAngles.Call (trans, GetLocalEulerAnglesParam);
                    m_OldRotationOrder = rotationOrder;
                }
                context.EulerFloats[0] = m_EulerAngles.x;
                context.EulerFloats[1] = m_EulerAngles.y;
                context.EulerFloats[2] = m_EulerAngles.z;
                string rotationLabel = "";
                if (AnimationMode.InAnimationMode () && rotationOrder != 4)
                {
                    rotationLabel = rotationOrder.ToString ();
                    rotationLabel = rotationLabel.Substring (rotationLabel.Length - 3);

                    context.label = "Rotation (" + rotationLabel + ")";
                }
            }

        }
        public void EndRotationField (RotContext context)
        {
            if (trans != null && transSetLocalEulerAngles != null)
            {
                m_EulerAngles = new Vector3 (context.EulerFloats[0], context.EulerFloats[1], context.EulerFloats[2]);
                Undo.RecordObject (trans, "RotEditor");
                int rotationOrder = (int) transGetRotationOrderInternal.Call (trans, null);
                SetLocalEulerAnglesParam[0] = m_EulerAngles;
                SetLocalEulerAnglesParam[1] = rotationOrder;
                transSetLocalEulerAngles.Call (trans, SetLocalEulerAnglesParam);
                if (trans.parent != null && transSendTransformChangedScale != null)
                    transSendTransformChangedScale.Call (trans, null);
            }
        }

        public static void MultiFieldPrefixLabel (Rect totalPosition, int id, GUIContent label, int columns)
        {
            if (editorMultiFieldPrefixLabel != null)
            {
                MultiFieldPrefixLabelParam[0] = totalPosition;
                MultiFieldPrefixLabelParam[1] = id;
                MultiFieldPrefixLabelParam[2] = label;
                MultiFieldPrefixLabelParam[3] = columns;
                editorMultiFieldPrefixLabel.Call (null, MultiFieldPrefixLabelParam);
            }
        }
    }
}
#endif
//#if UNITY_EDITOR
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngineEditor = UnityEditor.Editor;
#endif
namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class DynamicLightRender : MonoBehaviour
    {
        public bool loop = true;
        public FloatAnim intensity;
        public FloatAnim range;
        public FloatAnim rangeBias;
        public ColorAnim color;

        public float length = 1;

        public bool isVoxelLight = false;
        [NonSerialized]
        public int intLength = 0;
        private Transform trans;
        public EditorFlagMask flag = new EditorFlagMask ();
        public static uint LightAnimMask_Intensity = 0x00000001;
        public static uint LightAnimMask_Range = 0x00000002;
        public static uint LightAnimMask_RangeBias = 0x00000004;
        public static uint LightAnimMask_Color = 0x00000008;

#if UNITY_EDITOR
        private void OnEnable ()
        {
            FloatAnim.InitCreate (ref intensity, 0, 10, 1);
            FloatAnim.InitCreate (ref range, 0.1f, 64, 1);
            FloatAnim.InitCreate (ref rangeBias, 0.1f, 10, 1);
            ColorAnim.InitCreate (ref color, Color.white);
            OnStart ();
        }
#endif
        // public ISFXOwner Owner { get; set; }
        private Transform GetTrans ()
        {
            if (trans == null)
            {
                trans = this.transform;
            }
            return trans;
        }

        public bool IsEnable ()
        {
            return this.enabled && this.gameObject.activeInHierarchy;
        }
        public void OnStart ()
        {
            GetTrans ();
            intensity.animHelper.InitCreate (LightAnimMask_Intensity, flag);
            range.animHelper.InitCreate (LightAnimMask_Range, flag);
            rangeBias.animHelper.InitCreate (LightAnimMask_RangeBias, flag);
            color.animHelper.InitCreate (LightAnimMask_Color, flag);
            UpdateIntTime ();
        }
        public void OnStop () { }
        public void UpdateIntTime ()
        {
            intLength = (int) (length * 1000);
        }
        public void OnUpdate (float time, EngineContext context)
        {
#if UNITY_EDITOR
            if (!this.enabled || !this.gameObject.activeInHierarchy)
            {
                return;
            }
#endif
            if (loop && time > length)
            {
                int intTime = (int) (time * 1000);
                int count = intTime / intLength;
                int realIntTime = intTime - count * intLength;
                time = realIntTime * 0.001f;
            }
            if (trans != null && time <= length)
            {
                Vector4 posWithBias = trans.position;
                posWithBias.w = rangeBias.Evaluate (time);
                float i = intensity.Evaluate (time);
                Color c = color.Evaluate (time / length);
                float r = range.Evaluate (time);

                float oneOverLightRangeSqr = 1.0f / Mathf.Max (0.0001f, r * r);
                Vector4 cVec = new Vector4 (
                    Mathf.Pow (c.r * i, 2.2f),
                    Mathf.Pow (c.g * i, 2.2f),
                    Mathf.Pow (c.b * i, 2.2f), oneOverLightRangeSqr);

                // if (isVoxelLight)
                // {
                //     VoxelLightingSystem.SetPointLight (ref posWithBias, ref cVec);
                // }
                // else if (context.simpleLightCount < 4)
                // {
                //     int index = context.simpleLightCount;
                //     context.dynamicLightPos[index] = posWithBias;
                //     context.dynamicLightColor[index] = cVec;
                //     context.simpleLightCount++;
                //     context.SetFlag (EngineContext.ScriptSimpleLight, true);
                //     context.SetFlag (EngineContext.SimpleLightDirty, true);
                // }
            }

        }

#if UNITY_EDITOR
        void OnDrawGizmos ()
        {
            // if (trans != null && Owner != null)
            // {
            //     Color c = Handles.color;
            //     if (Owner.IsEnable ())
            //     {
            //         var cc = color.current;
            //         cc.a = 1;
            //         Handles.color = cc;
            //         Handles.RadiusHandle (Quaternion.identity, trans.position, range.current);
            //     }
            //     else
            //     {
            //         var cc = color.value;
            //         cc.a = 1;
            //         Handles.color = cc;
            //         range.value = Handles.RadiusHandle (Quaternion.identity, trans.position, range.value);
            //     }

            //     Handles.color = c;
            // }

        }
#endif
    }
#if UNITY_EDITOR
    [CustomEditor (typeof (DynamicLightRender))]
    public class DynamicLightRenderEditor : UnityEngineEditor
    {
        SerializedProperty loop;
        SerializedProperty length;
        SerializedProperty intensity;
        SerializedProperty range;
        SerializedProperty rangeBias;
        SerializedProperty color;
        SerializedProperty isVoxelLight;
        private void OnEnable ()
        {
            loop = serializedObject.FindProperty ("loop");
            length = serializedObject.FindProperty ("length");
            intensity = serializedObject.FindProperty ("intensity");
            range = serializedObject.FindProperty ("range");
            rangeBias = serializedObject.FindProperty ("rangeBias");
            color = serializedObject.FindProperty ("color");
            isVoxelLight = serializedObject.FindProperty ("isVoxelLight");
            DynamicLightRender dlr = target as DynamicLightRender;
            dlr.intensity.animHelper.Reset ();
            dlr.range.animHelper.Reset ();
            dlr.rangeBias.animHelper.Reset ();
            dlr.color.animHelper.Reset ();
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            DynamicLightRender dlr = target as DynamicLightRender;
            EditorGUILayout.PropertyField (loop);
            EditorGUI.BeginChangeCheck ();
            EditorGUILayout.PropertyField (length);
            if (EditorGUI.EndChangeCheck ())
            {
                if (length.floatValue < 0)
                {
                    length.floatValue = 0;
                }
                dlr.UpdateIntTime ();
            }
            dlr.intensity.DrawGUI (intensity, "Intensity");
            dlr.range.DrawGUI (range, "Range");
            dlr.rangeBias.DrawGUI (rangeBias, "RangeBias");
            dlr.color.DrawGUI (color, "Color");
            EditorGUILayout.PropertyField (isVoxelLight);
            serializedObject.ApplyModifiedProperties ();
        }
    }
#endif
}
//#endif
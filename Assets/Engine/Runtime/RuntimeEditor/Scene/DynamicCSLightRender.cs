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
    public class DynamicCSLightRender : MonoBehaviour
    {
        [Min(0)]
        public float intensity;
        [Min(0.001f)]
        public float range;
        [Min(0.001f)]
        public float rangeBias;
        public Color color;
        private Transform trans;

#if UNITY_EDITOR
        private void OnEnable ()
        {
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
        }
        public void OnStop () { }
        public void OnUpdate (float time, EngineContext context)
        {
#if UNITY_EDITOR
            if (!this.enabled || !this.gameObject.activeInHierarchy)
            {
                return;
            }
#endif
            if (trans != null)
            {
                Vector4 posWithBias = trans.position;
                posWithBias.w = rangeBias;

                float oneOverLightRangeSqr = 1.0f / Mathf.Max (0.0001f, range * range);
                Vector4 cVec = new Vector4 (
                    Mathf.Pow (color.r * Mathf.Abs(intensity), 2.2f),
                    Mathf.Pow (color.g * Mathf.Abs(intensity), 2.2f),
                    Mathf.Pow (color.b * Mathf.Abs(intensity), 2.2f), oneOverLightRangeSqr);

                //if (context.simpleLightCount < 4)
                //{
                //    int index = context.simpleLightCount;
                //    context.dynamicLightPos[index] = posWithBias;
                //    context.dynamicLightColor[index] = cVec;
                //    context.simpleLightCount++;
                //    context.SetFlag (EngineContext.ScriptSimpleLight, true);
                //    context.SetFlag (EngineContext.SimpleLightDirty, true);
                //}
            }

        }

#if UNITY_EDITOR
        void OnDrawGizmos ()
        {
            if (trans != null)
            {
                Color c = Handles.color;
                var cc = color;
                cc.a = 1;
                Handles.color = cc;
                Handles.RadiusHandle (Quaternion.identity, trans.position, range);
                Handles.color = c;
            }

        }
#endif
    }
}
//#endif
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
    public class DynamicDirectionLight : MonoBehaviour
    {
        // [HideInInspector]
        // public Vector4 lightDir;
        public Color lightColor;
        [Min (0)]
        public float intensity;
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
            if (trans != null)
            {
                Vector4 dir = -trans.forward;
                dir.w = IsEnable () ? 1 : 0;
                context.addLightDir = dir;
                //Shader.SetGlobalVector (LightingModify._AddLightDir1, dir);
            }
            else
            {
                //Shader.SetGlobalVector (LightingModify._AddLightDir1, Vector4.zero);
            }
            // Vector4 lightColorIntensity = new Vector4 (
            //     Mathf.Pow (lightColor.r * intensity, 2.2f),
            //     Mathf.Pow (lightColor.g * intensity, 2.2f),
            //     Mathf.Pow (lightColor.b * intensity, 2.2f), 0);
            // context.addLightColorIntensity = lightColorIntensity;
            // context.SetFlag (EngineContext.ScriptSimpleLight, true);
            // context.SetFlag (EngineContext.SimpleLightDirty, true);
            //Shader.SetGlobalVector (LightingModify._AddLightColor1, lightColorIntensity);
        }

        // #if UNITY_EDITOR
        //         void Update ()
        //         {

        //         }
        // #endif
    }
#if UNITY_EDITOR
    [CustomEditor (typeof (DynamicDirectionLight))]
    public class DynamicDirectionLightEditor : UnityEngineEditor
    {
        public void OnSceneGUI ()
        {
            DynamicDirectionLight ddl = target as DynamicDirectionLight;
            if (ddl != null)
            {
                Color temp = Handles.color;
                if (SceneView.lastActiveSceneView != null &&
                    SceneView.lastActiveSceneView.camera != null)
                {

                    Transform t = SceneView.lastActiveSceneView.camera.transform;
                    Vector3 pos = t.position + t.forward * 10;
                    Color c = ddl.lightColor;
                    RuntimeUtilities.DrawLightHandle (t, ref pos, 4, 3, ref c, ddl.transform, "AddLight");

                }

                Handles.color = temp;
            }

        }
    }
#endif
}
//#endif
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    [RequireComponent(typeof(Light))]
    [AddComponentMenu("Light/Static Light Render")]
    public class SceneLightRender : MonoBehaviour
    {
        [Header("Spec Intensity"), Range(0f, 1f)]
        public float specLerp = 0f;

        [System.NonSerialized]
        public Light pointLight;

        public bool IsValid
        {
            get
            {
                Update();
                return enabled && gameObject.activeInHierarchy&&
                    pointLight!=null&& pointLight.enabled;
            }
        }
        private void Update()
        {
            if (pointLight == null)
            {
                this.TryGetComponent(out pointLight);
            }
        }
        void OnDrawGizmos()
        {
            if (pointLight != null)
            {
                Color c = Gizmos.color;
                Gizmos.color = pointLight.color;
                Gizmos.DrawWireSphere(transform.position, pointLight.range);
                Gizmos.color = c;
            }
        }

        public Vector4 GetPosRange()
        {
            Update();
            Vector4 posRange = this.transform.position;
            posRange.w = pointLight != null ? pointLight.range : 4;
            return posRange;
        }
        public LightingInfo GetLigthInfo()
        {
            Update();
            var color = pointLight != null ? pointLight.color : Color.white;
            float intensity = pointLight != null ? pointLight.intensity : 1;
            float range = pointLight != null ? pointLight.range : 4;
            var pos = this.transform.position;
            color.r *= intensity;
            color.g *= intensity;
            color.b *= intensity;
            //color.a = VoxelLightingSystem.GetInvSqrRange(range);
            return new LightingInfo()
            {
                posCoverage = new Vector4(pos.x, pos.y, pos.z, specLerp),
                color = color,
                param = Vector4.zero,
            };
        }
    }

    [CustomEditor(typeof(SceneLightRender))]
    public class SceneLightRenderEditor : UnityEngineEditor
    {

        void OnEnable()
        {
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var slr = target as SceneLightRender;
            var pointLight = slr.pointLight;

            var color = pointLight != null ? pointLight.color : Color.white;
            float intensity = pointLight != null ? pointLight.intensity : 1;
            float range = pointLight != null ? pointLight.range : 4;
            color.r *= intensity;
            color.g *= intensity;
            color.b *= intensity;
            //color.a = VoxelLightingSystem.GetInvSqrRange(range);
            Vector4 c = color;
            EditorGUILayout.LabelField(string.Format("R:{0:F3}", c.x));
            EditorGUILayout.LabelField(string.Format("G:{0:F3}", c.y));
            EditorGUILayout.LabelField(string.Format("B:{0:F3}", c.z));
            EditorGUILayout.LabelField(string.Format("A:{0:F3}", c.w));

        }
    }
}
#endif
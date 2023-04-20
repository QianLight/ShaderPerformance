
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    public struct LightProperty
    {
        public Color color;
        public Color ambient;
        public float range;
        public float intensity;
    }

    [System.Serializable]
    public class CLightProperty
    {
        public CColor color = new CColor(Color.white);
        public CColor ambient = new CColor(Color.black);
        public CFloat range = new CFloat(1);
        public CFloat intensity = new CFloat(1);

        public LightProperty Evaluate(float t)
        {
            return new LightProperty()
            {
                color = color.Evaluate(t),
                ambient = ambient.Evaluate(t),
                range = range.Evaluate(t),
                intensity = intensity.Evaluate(t),
            };
        }
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class LightRender : MonoBehaviour
    {
        [Header("优先级")]
        public int priority = 0;
        [Header("索引")]
        public int index = -1;
        [Header("延时")]
        public float delay = 0;
        [Header("淡入时长")]
        public float fadeInLength = -1;
        [Header("循环时长")]
        public float loopLength = 1;
        [Header("循环次数")]
        public float loopTimes = 1;
        [Header("淡出时长")]
        public float fadeOutLength = 1;
        [Header("受光软度"), Range(0.001f, 1f)]
        public float softness = 0.001f;
        [Header("灯光叠加"), Range(0f, 1f)]
        public float coverage = 0.1f;

        [Header("淡入效果")]
        public CLightProperty fadeIn = new CLightProperty();
        [Header("循环效果")]
        public CLightProperty loop = new CLightProperty();
        [Header("淡出效果")]
        public CLightProperty fadeOut = new CLightProperty();

        //public SavedFloat previewProgress = new SavedFloat($"{nameof(LightRender)}.{nameof(previewProgress)}");

        void OnDrawGizmos()
        {
            /*if (SFXLightData.Evaluate(this, previewProgress.Value, out LightProperty p))
            {
                Color c = Gizmos.color;
                Gizmos.color = p.color;
                Gizmos.DrawWireSphere(transform.position, p.range);
                Gizmos.color = c;
            }*/
        }

        public float GetCullRange()
        {
            float max = float.MinValue;
            GetMaxRange(ref max, fadeIn.range);
            GetMaxRange(ref max, loop.range);
            GetMaxRange(ref max, fadeOut.range);
            return max;
        }

        private void GetMaxRange(ref float maxValue, CFloat cFloat)
        {
            if (cFloat.useCurve)
            {
                Keyframe[] keys = cFloat.curve.keys;
                foreach (Keyframe key in keys)
                {
                    maxValue = Mathf.Max(key.value, maxValue);
                }
            }
            else
            {
                maxValue = Mathf.Max(cFloat.value, maxValue);
            }
        }
    }

    public class SFXLightDataProcessor : SFXProcessData
    {
        public override SFXData Process(Transform t, SFXData parent, out float duration, out bool processChind)
        {
            duration = 0;
            processChind = true;
            if (t.TryGetComponent<LightRender>(out var lr))
            {
                SFXData sfxData = new SFXLightData();
                sfxData.Refresh(t, lr, parent, 0, out var d);
                return sfxData;
            }
            return null;
        }
    }

    public partial class SFXLightData : SFXData
    {
        private LightRender lr;
        public override string CompType { get { return "PointLight"; } }
        public override Component Comp { get { return lr; } }

        public override void Refresh(Transform t, Component comp, SFXData parent, float time,
           out float duration)
        {
            base.Refresh(t, comp, parent, time, out duration);
            lr = comp as LightRender;
        }

        public override void OnUpdate(float time, float deltaTime, bool restart, bool lockTime)
        {
            if (!lr)
                return;

            //if (Evaluate(lr, time, out LightProperty p))
            //{
               // Vector3 position = lr.transform.position;
                //VoxelLightingSystem.UpdateDynamicLight(EngineContext.instance, ref position, p.range, p.intensity, ref p.color, ref lr.softness, ref p.ambient, ref lr.coverage);
           // }
        }

        public static bool Evaluate(LightRender lr, float time, out LightProperty p)
        {
            float t = time - lr.delay;
            if (t >= 0)
            {
                if (t < lr.fadeInLength)
                {
                    t /= lr.fadeInLength;
                    p = lr.fadeIn.Evaluate(t);
                    return true;
                }

                t -= lr.fadeInLength;
                if (lr.loopTimes > 0 && t < lr.loopLength * lr.loopTimes)
                {
                    t /= lr.loopLength;
                    t -= (int)t;
                    p = lr.loop.Evaluate(t);
                    return true;
                }

                t -= lr.loopLength * lr.loopTimes;
                if (t < lr.fadeOutLength)
                {
                    t /= lr.fadeOutLength;
                    p = lr.fadeOut.Evaluate(t);
                    return true;
                }
            }

            p = default;
            return false;
        }
    }
}
#endif
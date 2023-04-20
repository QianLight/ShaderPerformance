using System;
namespace UnityEngine.Rendering.Universal
{

    [Serializable, VolumeComponentMenu("Post-processing/GodRay")]
    public class GodRay : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Strength of the god ray filter.")]
        public MinFloatParameter Intensity = new MinFloatParameter(0f, 0f);

        [Tooltip("Global tint of the bloom filter.")]
        public ColorParameter color = new ColorParameter(Color.white, true, false, true);

        [Tooltip("Use Light Scatter Dir")]
        public Vector3Parameter LightDir = new Vector3Parameter(Vector3.down);

        [Tooltip("Use Light Scatter Position")]
        public Vector3Parameter LightPosition = new Vector3Parameter(Vector3.zero);

        [Tooltip("Threshold.")]
        public ClampedFloatParameter Threshold = new ClampedFloatParameter(0, 0, 2);

        [Tooltip("Linear Distance.")]
        public ClampedFloatParameter LinearDistance = new ClampedFloatParameter(0.99f, 0, 1);

        [Tooltip("Power.")]
        public ClampedFloatParameter Power = new ClampedFloatParameter(0.01f, 0.01f, 1);

        [Tooltip("MaxPower.")]
        public ClampedFloatParameter MaxPower = new ClampedFloatParameter(0.01f, 0.01f, 1);

        [Tooltip("Radius.")]
        public ClampedFloatParameter Radius = new ClampedFloatParameter(0.25f, 0, 5);

        [Tooltip("Offset.")]
        public ClampedFloatParameter Offset = new ClampedFloatParameter(0.02f, 0, 0.2f);

        [Tooltip("Blur Times.")]
        public ClampedIntParameter BlurTimes = new ClampedIntParameter(1, 1, 4);

        [Tooltip("Down Sample.")]
        public ClampedIntParameter DownSample = new ClampedIntParameter(2, 0, 6);

        [Tooltip("Bias.")]
        public ClampedFloatParameter Bias = new ClampedFloatParameter(0, -1, 1);

        [Tooltip("Use Noise.")]
        public BoolParameter UseNoise = new BoolParameter(false);

#if UNITY_EDITOR
        [Tooltip("Light Dir.")]
        public StringParameter LightTransform = new StringParameter(null);
        public Transform LightTransformObject = null;
#endif
        public bool IsActive() => Intensity.value > 0;

        public bool IsTileCompatible() => false;
    }
#if UNITY_EDITOR
    [Serializable]
    public sealed class TransformParameter : VolumeParameter<UnityEngine.Transform>
    {
        public TransformParameter(UnityEngine.Transform value, bool overrideState = false) : base(value, overrideState) { }
    }
#endif
    [Serializable]
    public sealed class StringParameter : VolumeParameter<string>
    {
        public StringParameter(string value, bool overrideState = true) : base(value, overrideState) { }
        public string GetRootName(UnityEngine.Transform obj)
        {
            System.Collections.Generic.List<string> names = new System.Collections.Generic.List<string>();
            GetRootName(obj, names);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < names.Count; i++)
            {
                if (i == 0)
                {
                    sb.Append(names[i]);
                }
                else
                {
                    sb.Append("/");
                    sb.Append(names[i]);
                }
            }
            return sb.ToString();
        }
        public void GetRootName(UnityEngine.Transform obj, System.Collections.Generic.List<string> names)
        {
            if (obj.parent != null)
            {
                GetRootName(obj.parent, names);
            }
            names.Add(obj.name);
        }
        public void SetValue(string v)
        {
            value = v;
            if(string.IsNullOrEmpty(v))
            {
                obj = null;
            }
        }
        public Transform SetObject(bool enable)
        {
            Transform tmp = GetObjectByName();
#if UNITY_EDITOR
            //if (UnityEditor.EditorApplication.isPlaying)
            {
#endif                    
                if (tmp != null && tmp.gameObject.activeSelf != enable)
                {
                    tmp.gameObject.SetActive(enable);
                }
#if UNITY_EDITOR
            }
#endif
            return tmp;
        }
        private Transform obj = null;
        private Transform FindObj(string name)
        {
            Transform result = null;
            if (!string.IsNullOrEmpty(name))
            {
                string[] ps = name.Split(new char[] { '/' });
                SceneManagement.Scene scene;
                if (EnvironmentVolume.LastVolumeRoot == null)
                {
                    scene = SceneManagement.SceneManager.GetActiveScene();
                }
               else
                {
                    scene = EnvironmentVolume.LastVolumeRoot.gameObject.scene;
                }
                if(scene != null)
                {
                    GameObject[] objs = scene.GetRootGameObjects();
                    for (int i = 0; i < objs.Length; i++)
                    {
                        GameObject tmp = objs[i];
                        if (tmp.name == ps[0])
                        {
                            if (ps.Length == 1)
                            {
                                result = tmp.transform;
                            }
                            else
                            {
                                result = FindObjByName(tmp.transform, ps, 1);
                            }
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private Transform FindObjByName(Transform root, string[] names, int index)
        {
            Transform result = null;
            int maxIndex = names.Length - 1;
            Transform tmpResult = root;
            for (; index <= maxIndex; index++)
            {
                string newName = names[index];
                bool find = false;
                foreach (Transform o in tmpResult)
                {
                    if (o.name == newName)
                    {
                        tmpResult = o;
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    break;
                }
                if (index == maxIndex)
                {
                    result = tmpResult;
                }
            }
            return result;
        }
        public Transform GetObjectByName()
        {

            if (obj == null)
            {
                UnityEngine.Transform tmp = FindObj(value);
                if (tmp != null)
                {
                    obj = tmp;
                }
            }
            else
            {
                if (obj.name != value)
                {
                    UnityEngine.Transform tmp = FindObj(value);
                    if (tmp != null)
                    {
                        obj = tmp;
                    }
                }
            }
            return obj;
        }
    }
}

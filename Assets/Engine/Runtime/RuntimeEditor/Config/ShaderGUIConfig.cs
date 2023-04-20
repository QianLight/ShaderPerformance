#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CFEngine
{
    public enum LightShadowType
    {
        None,
        Scene,
        Role,
    }
    [System.Serializable]
    public class ShaderGUIConfig : ScriptableObject
    {
        public Shader shader;
        public EDummyMatType dummyMatType = EDummyMatType.SceneMat;
        public LightShadowType lsType = LightShadowType.None;
        public List<ShaderFeatureInstance> shaderFeatures = new List<ShaderFeatureInstance> ();

        public bool HasShaderFeatureConfig (ShaderFeatureGroup sfg)
        {
            for (int i = 0; i < shaderFeatures.Count; ++i)
            {
                if (shaderFeatures[i].sfg == sfg)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddConfig (ShaderFeatureGroup sfg)
        {
            if (!HasShaderFeatureConfig (sfg))
            {
                shaderFeatures.Add (new ShaderFeatureInstance ()
                {
                    sfg = sfg
                });
            }
        }

        public void RemoveConfig (ShaderFeatureGroup sfg)
        {
            for (int i = 0; i < shaderFeatures.Count; ++i)
            {
                if (shaderFeatures[i].sfg == sfg)
                {
                    shaderFeatures.RemoveAt (i);
                    return;
                }
            }
        }

        public void Sort ()
        {
            shaderFeatures.Sort ((x, y) => x.sfg.index.CompareTo (y.sfg.index));
        }

    }
}
#endif
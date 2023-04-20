using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShadowMaskSharpness : MonoBehaviour
{
    [Range(1,4)]
    public float m_sharpness = 2;
    [Range(0,4)]
    public float m_bias = 1;

    void OnEnable()
    {
        UpdateValue();
    }

#if UNITY_EDITOR
    void Update()
    {
        UpdateValue();
    }
#endif

    void UpdateValue()
    {
        Shader.SetGlobalFloat("_shadowMaskSharpness", m_sharpness);
        Shader.SetGlobalFloat("_shadowMaskBias", m_bias);
    }
}

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LightingSourceSettings : MonoBehaviour
{
    public VolumeProfile profile;
    public LightFieldType type;

    private void Start()
    {
        if (type == LightFieldType.MainLight)
        {
            foreach (var component in profile.components)
            {
                if (component is Lighting lighting)
                {
                    lighting.mainLight.light = GetComponent<Light>();
                    break;
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightmapShadow : MonoBehaviour
{
    // Start is called before the first frame update
    [Range(0,1)]
    public float LightmapShadowIntensity=0;
    public float _LightmapShadowRange=1;
    private Vector4 ambientParamTest;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ambientParamTest=new Vector4(LightmapShadowIntensity,_LightmapShadowRange,0,0);
        Shader.SetGlobalVector("_AmbientParamTest",ambientParamTest);
    }
}

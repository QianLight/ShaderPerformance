using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TestTargetFrameRate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public int nTargetFrameRate =60;

    public int renderFrameInterval=3;
    // Update is called once per frame
    void Update()
    {
        Application.targetFrameRate = nTargetFrameRate;
        OnDemandRendering.renderFrameInterval = renderFrameInterval;
    }
}

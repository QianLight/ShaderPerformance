using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UrpGameDebug : MonoBehaviour
{
    
    public void SetRenderFrameInterval(int nRenderFrameInterval)
    {
        OnDemandRendering.renderFrameInterval = nRenderFrameInterval;
    }
}

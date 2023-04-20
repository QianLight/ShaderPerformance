using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityControlObject : MonoBehaviour
{
    public List<GameObject> HiddenLOD2 = new List<GameObject>();
    public List<GameObject> HiddenLOD3 = new List<GameObject>();
    public int DelayFrame = 0;
    public int frame = 0;

    void OnEnable()
    {
        if(DelayFrame == 0)
        {
            frame = -99999;
            hidden();
            enabled = false;
        }
    }
    void Update()
    {
        frame++;
        if(frame >= DelayFrame)
        {
            frame = 99999;
            hidden();
            enabled = false;
        }
    }
    void hidden()
    {
        if(UnityEngine.Rendering.Universal.GameQualitySetting.ResolutionLevel <= RenderQualityLevel.Low)
        {
            hiddenList(HiddenLOD2);
            hiddenList(HiddenLOD3);
        }
        else if(UnityEngine.Rendering.Universal.GameQualitySetting.ResolutionLevel == RenderQualityLevel.Medium)
        {
            hiddenList(HiddenLOD2);
        }
    }
    void hiddenList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject g = list[i];
            if (g.activeSelf)
            {
                g.SetActive(false);
            }
        }
    }
}

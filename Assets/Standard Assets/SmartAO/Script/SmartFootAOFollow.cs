using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SmartFootAOFollow : MonoBehaviour
{
    public GameObject LeftFootShadow, RightFootShadow;
    public void OnEnable()
    {
        if(!UnityEngine.Rendering.Universal.GameQualitySetting.UniversalForwardHigh)
        {
            enabled = false;
        }
    }
    public void ShowFootShadow(bool show = false)
    {
        if (LeftFootShadow != null && RightFootShadow != null)
        {
            LeftFootShadow.SetActive(show);
            RightFootShadow.SetActive(show);
        }
    }
    void OnBecameVisible()
    {
        ShowFootShadow(true);
    }
    void OnBecameInvisible()
    {
        ShowFootShadow(false);
    }
}

using System;
using System.Collections;
using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PostTreatmentSystem:XSingleton<PostTreatmentSystem>,IXPostTreatment
{
    public bool Deprecated { get; set; }

    public int OverrideParam(bool useDefaultValue,object[] param)
    {
        if(useDefaultValue)
            return URPRadialBlur.instance.AddParam(RadialBlurParam.GetDefualtValue(), URPRadialBlurSource.DefualtValue, 1);
        else
            return URPRadialBlur.instance.AddParam(RadialBlurParam.InitValue(param), URPRadialBlurSource.DefualtValue, 1);
    }

    public bool ModifyParam(int id,bool useDefaultValue,object[] param)
    {
        if(useDefaultValue)
            return URPRadialBlur.instance.ModifyParam(id, RadialBlurParam.GetDefualtValue());
        else
            return URPRadialBlur.instance.ModifyParam(id, RadialBlurParam.InitValue(param));
    }

    public bool RemoveParam(int id)
    {
        return URPRadialBlur.instance.RemoveParam(id);
    }
}
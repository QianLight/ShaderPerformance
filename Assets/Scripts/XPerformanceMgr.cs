using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class XPerformanceMgr : IPerformanceMgr
{
    public bool Deprecated { get; set; }
    PerformanceSetting.SettingInfo systemInfo;
    public void SetPerformanceLevel(XRenderSettingParam param)
    {
        if(systemInfo == null)
        {
            systemInfo = new PerformanceSetting.SettingInfo();
        }
        SettingParam2SettingInfo(param, systemInfo);
        PerformanceMgr.SetPerformanceLevel(systemInfo);
    }
    public void SetIfEmulator(bool isTheEmulator)
    {
        PerformanceMgr.SetIfEmulator(isTheEmulator);
    }
    public void SettingParam2SettingInfo(XRenderSettingParam param, PerformanceSetting.SettingInfo info)
    {
        info.Level = (RenderQualityLevel)param.Level;
        info.MatLevel = (RenderQualityLevel)param.MatLevel;
        info.ShadowLevel = (RenderQualityLevel)param.ShadowLevel;
        info.SFXLevel = (RenderQualityLevel)param.SFXLevel;
        info.TextureLevel = (RenderQualityLevel)param.TextureLevel;
        info.AfterEffectLevel = (RenderQualityLevel)param.AfterEffectLevel;
        info.AAEnable = (RenderQualityLevel)param.AAEnabel;
        //info.TessEnable = (RenderQualityLevel)param.TessEnable;
    }
    public int GetDefaultDeviceLevel()
    {
        return PerformanceMgr.DefaultDeviceLevel;
    }

    public void OnEnterBattle()
    {
        PerformanceMgr.EnterWar();
    }

    public void OnEnterNonBattle()
    {
        PerformanceMgr.EnterUI();
    }
    public void TimelineEnter()
    {
        PerformanceMgr.TimelineEnter();
    }
    public void TimelineLeave()
    {
        PerformanceMgr.TimelineLeave();
    }
}


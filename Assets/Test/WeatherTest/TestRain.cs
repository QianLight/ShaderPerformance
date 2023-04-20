using System.Collections;
using System.Collections.Generic;
using CFClient.React;
using UnityEngine;

public class TestRain : MonoBehaviour
{

    [ContextMenu("TestBigRain")]
    public void TestBigRain()
    {
        RainEnvManager.RainParams @params = new RainEnvManager.RainParams()
        {
            useTime = 1f,
            DarkValueInRain = 0.75f,
            RainRoughness = 0.08f,
            RippleTilling = 6,
            RippleIntensity = 0.7f,
            RippleSpeed = 3f,
            NormalTSScale = 0.65f,
            useLighting = true,
            isRain = true,
            rainPrefabName = "fx_UI_rain01_heavy"
        };

        RainEnvManager.Instance.SetRainParams(@params);
    }
    
    [ContextMenu("TestSmallRain")]
    public void TestSmallRain()
    {
        RainEnvManager.RainParams @params = new RainEnvManager.RainParams()
        {
            useTime = 1f,
            DarkValueInRain = 0.9f,
            RainRoughness = 0.08f,
            RippleTilling = 6,
            RippleIntensity = 0.45f,
            RippleSpeed = 3f,
            NormalTSScale = 0.85f,
            useLighting = false,
            isRain = true,
            rainPrefabName = "fx_UI_rain01"
        };

        RainEnvManager.Instance.SetRainParams(@params);
    }

    [ContextMenu("TestStopRain")]
    public void StopRain()
    {
        RainEnvManager.RainParams @params = new RainEnvManager.RainParams()
        {
            useTime = 1f,
            DarkValueInRain = 0.9f,
            RainRoughness = 0.08f,
            RippleTilling = 6,
            RippleIntensity = 0.45f,
            RippleSpeed = 3f,
            NormalTSScale = 0.85f,
            useLighting = false,
            isRain = false,
            rainPrefabName = "fx_UI_rain01"
        };
        RainEnvManager.Instance.SetRainParams(@params);
    }
}

using System;
using UnityEngine;

public enum LightStatus
{
    LIGHTING,
    ON,
    CLOSING,
    OFF
}

[RequireComponent(typeof(Light)), ExecuteInEditMode]
public class SpotLightAsShadow : MonoBehaviour
{
    private Light _shadowLight;
    private float _spotLightDelayRate;
    [SerializeField] private float spotLightIntensity = 3f;
    [SerializeField] private float spotLightDelayTime = .2f;
    [SerializeField] private float spotCloseLightDelayTime = .2f;
    [SerializeField] private float shadowStrength = 2.0f;
    [SerializeField] private bool isCasterShadow = true;
    private LightStatus _lightStatus;

    public LightStatus LightStatus
    {
        get { return _lightStatus; }
        set
        {
            _lightStatus = value;
            var isActive = (value != LightStatus.OFF);
            if (value == LightStatus.ON)
            {
                _spotLightDelayRate = 1f;
                _shadowLight.intensity = spotLightIntensity;
            }
            else if (value == LightStatus.OFF)
            {
                _spotLightDelayRate = 0f;
                _shadowLight.intensity = 0f;
            }
            if (_shadowLight.enabled != isActive)
            {
                _shadowLight.enabled = isActive;
            }



        }
    }

    public static readonly int SpotLightShadowIntensity = Shader.PropertyToID("_SpotLightShadowIntensity");

#if UNITY_EDITOR
    private float _cachespotLightIntensity;
    private void OnValidate()
    {
        if (Math.Abs(_cachespotLightIntensity - spotLightIntensity) > .0001f)
        {
            _cachespotLightIntensity = spotLightIntensity;
            if (_shadowLight == null)
            {
                _shadowLight = GetComponent<Light>();
            }

            _shadowLight.intensity = spotLightIntensity;
            // spotLightIntensity = _shadowLight.intensity ;
        }
    }
#endif


    public bool IsCasterShadow
    {
        get { return isCasterShadow; }
    }

    public void SetLightStatus(LightStatus status, bool enableShadow)
    {
        var isActive = (status != LightStatus.OFF);
        
        if (_shadowLight == null)
        {
            _shadowLight = GetComponent<Light>();
        }
        _shadowLight.shadows = enableShadow && isCasterShadow ? LightShadows.Soft : LightShadows.None;
        if (isActive && enableShadow && isCasterShadow)
        {
            SetShaderValue();
        }
        LightStatus = status;
    }

    public void UpdateLight()
    {
        if (LightStatus == LightStatus.ON || LightStatus == LightStatus.OFF)
        {
            return;
        }
        
        if (_spotLightDelayRate >= 1f && LightStatus == LightStatus.LIGHTING)
        {
            LightStatus = LightStatus.ON;
            return;
        }

        if (spotLightIntensity <= 0&& LightStatus == LightStatus.CLOSING )
        {
            LightStatus = LightStatus.OFF;
            return;
        }

        if (LightStatus == LightStatus.LIGHTING)
        {
            var rate = Time.deltaTime / Mathf.Max(spotLightDelayTime, 0.000001f);
            _shadowLight.intensity = Mathf.Min(_spotLightDelayRate * spotLightIntensity, spotLightIntensity);
            _spotLightDelayRate += rate;
            if (_spotLightDelayRate >= 1f)
            {
                LightStatus = LightStatus.ON;
                return;
            }
        }
        if (LightStatus == LightStatus.CLOSING)
        {
            var rate = Time.deltaTime / Mathf.Max(spotCloseLightDelayTime, 0.000001f);
            _shadowLight.intensity = Mathf.Max(_spotLightDelayRate * spotLightIntensity, 0);
            _spotLightDelayRate -= rate;
            if (_spotLightDelayRate <= 0f)
            {
                LightStatus = LightStatus.OFF;
                return;
            }
        }
    }
    
    public void SetShaderValue()
    {
        Shader.SetGlobalFloat(SpotLightShadowIntensity, shadowStrength);
    }
    

    void Awake()
    {
        _spotLightDelayRate = 0f;
        _shadowLight = GetComponent<Light>();
        _shadowLight.enabled = false;
#if UNITY_EDITOR
        _shadowLight.lightmapBakeType = LightmapBakeType.Realtime;
        _shadowLight.type = UnityEngine.LightType.Spot;
        _shadowLight.shadows = LightShadows.Soft;
#endif
    }
}
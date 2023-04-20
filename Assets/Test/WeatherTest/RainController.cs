
using UnityEngine;

using Random = UnityEngine.Random;

public class RainController : MonoBehaviour
{
    
    public AnimationCurve m_thunderLightingCurve;
    public AnimationCurve m_skyBoxLightingCurve;
    public Light m_light;
    float _originLightIntensity;
    float _curveTime=0;
    [Header("下雨和雨停的渐变时间")]
    public float m_useTime = 1;
    [Range(0.1f, 1)]
    public float m_darkValueInRain = 0.5f;
    [Range(0, 1)]
    public float m_rainRoughness = 0.2f;

    [Range(0,5)]
    public float m_rippleIntensity = 1;

    [Range(0, 10)]
    public float _RippleDensity=2;
    [Range(0, 1)]
    public  float _RippleRadius=0.3f;
    [Range(0, 20)]
    public float _RingCount=5.5f;
    [Range(0, 5)]
    public float _DropSpeed=1;
    [Range(0, 1)]
    public  float _RippleFade=0.5f;
    
    [Range(0, 1)]
    public float m_normalTS_Scale = 0.25f;
    //public Texture m_rippleTex;

    public bool m_lighting;
    [Range(-220, -200)]
    public float m_lightingRotX = -210;
    [Range(0, 360)]
    public float m_lightingRotY = 0;
    [Range(0.1f, 0.5f)]
    public float m_lightingPTSize = 0.2f;
    
    [Range(1, 10)]
    public float m_flashFrequency = 2;
    [ColorUsageAttribute(true,true)]
    public Color m_lightingColor=new Color(1,1,1,1);
    bool blinkOnce=false;

    private int _darkValueInRainID,
        _rippleIntensityID,
        _LightingPTRotXID,
        _LightingPTRotYID,
        _LightingPTSizeID,
        _FlashFrequencyID,
        _LightingColorID,
        _RainRoughnessID,
        _NormalScaleID,
        _RippleDensityID,
        _RippleRadiusID,
        _RingCountID,
        _DropSpeedID;
      public int  _RippleFadeID;
  

    static bool _startRain, _stopRain;
    static float _timer = 0;
    static float _currentRainRoughness = 1, _currentDarkValueInRain = 0, _currenRippleIntensity = 0;
    void Start()
    {
        _startRain = false;
        _stopRain = false;
        if (m_light)
        {
            _originLightIntensity = m_light.intensity;
        }

        GetGlobalPropertyID();        
        SetGlobalRainShaderProperty(1,1,0);
        SetGlobalLightingShaderProperty();
    }

    // Update is called once per frame
    void Update()
    {
        if (Shader.IsKeywordEnabled("_RAIN"))
        {
            if (_startRain)
            {
                if (_timer < m_useTime)
                {
                    _timer += Time.deltaTime;
                    _currentRainRoughness = Mathf.MoveTowards(1, m_rainRoughness, _timer / m_useTime);
                    _currentDarkValueInRain = Mathf.MoveTowards(1, m_darkValueInRain, _timer / m_useTime);
                    _currenRippleIntensity = Mathf.MoveTowards(0, m_rippleIntensity, _timer / m_useTime);
                }
                else
                {
                    _currentDarkValueInRain = m_darkValueInRain;
                    _currentRainRoughness = m_rainRoughness;
                    _currenRippleIntensity = m_rippleIntensity;
                }
            }

            if (_stopRain)
            {
                if (_timer < m_useTime)
                {
                    _timer += Time.deltaTime;
                    _currentRainRoughness = Mathf.MoveTowards(m_rainRoughness, 1, _timer / m_useTime);
                    _currentDarkValueInRain = Mathf.MoveTowards(m_darkValueInRain, 1, _timer / m_useTime);
                    _currenRippleIntensity = Mathf.MoveTowards(m_rippleIntensity, 0, _timer / m_useTime);
                }
                else
                {
                    _currentDarkValueInRain = 1;
                    _currentRainRoughness = 1;
                    _currenRippleIntensity = 0;
                    _stopRain = false;
                    Shader.DisableKeyword("_RAIN");
                }
            }

            SetGlobalRainShaderProperty(_currentRainRoughness, _currentDarkValueInRain, _currenRippleIntensity);
        }

        if (m_lighting)
        {
            ThunderLighting();
        }
        else
        {
            if (Shader.IsKeywordEnabled("_LIGHTING_ON"))
            {
                Shader.DisableKeyword("_LIGHTING_ON");
            }
        }
    }

    public static void StartRain()
    {
        _startRain = true;
        _stopRain = false;
        _timer = 0;     
        _currentRainRoughness = 1; 
        _currentDarkValueInRain = 1;
        _currenRippleIntensity = 0;
        Shader.EnableKeyword("_RAIN");
    }
  
    public static void StopRain()
    {
        _stopRain=true;
        _startRain = false;
        _timer = 0;
       
    }
    void GetGlobalPropertyID()
    {
        _darkValueInRainID = Shader.PropertyToID("_darkValueInRain");
       
        _rippleIntensityID = Shader.PropertyToID("_RippleIntensity");

        
        _RippleDensityID= Shader.PropertyToID("_RippleDensity");
        _RippleRadiusID = Shader.PropertyToID("_RippleRadius");
        _RingCountID    = Shader.PropertyToID("_RingCount");
        _DropSpeedID    = Shader.PropertyToID("_DropSpeed");
        _RippleFadeID         = Shader.PropertyToID("_RippleFade");
        
        _LightingPTRotXID = Shader.PropertyToID("_LightingPTRotX");
        _LightingPTRotYID = Shader.PropertyToID("_LightingPTRotY");
        _LightingPTSizeID = Shader.PropertyToID("_LightingPTSize");
        _FlashFrequencyID = Shader.PropertyToID("_FlashFrequency");
        _LightingColorID = Shader.PropertyToID("_LightingColor");
        _RainRoughnessID = Shader.PropertyToID("_RainRoughness");
        _NormalScaleID = Shader.PropertyToID("_NormalTSScale"); 
        //Shader.SetGlobalTexture("_RippleTex", m_rippleTex);
    }
    void SetGlobalRainShaderProperty(float rainRoughness,float darkValueInRain,float rippleIntensity)
    {        
        //Shader.SetGlobalFloat(_RainRoughnessID, m_rainRoughness);
        //Shader.SetGlobalFloat(_darkValueInRainID, m_darkValueInRain);

        ////涟漪
        //Shader.SetGlobalFloat(_rippleIntensityID, m_rippleIntensity);


        Shader.SetGlobalFloat(_RainRoughnessID, rainRoughness);
        Shader.SetGlobalFloat(_darkValueInRainID, darkValueInRain);

        //涟漪
        Shader.SetGlobalFloat(_rippleIntensityID, rippleIntensity);

        Shader.SetGlobalFloat(_NormalScaleID, m_normalTS_Scale);

        Shader.SetGlobalFloat(_RippleDensityID, _RippleDensity);
        Shader.SetGlobalFloat(_RippleRadiusID, _RippleRadius);
        Shader.SetGlobalFloat(_RingCountID, _RingCount);
        Shader.SetGlobalFloat(_DropSpeedID, _DropSpeed);
        Shader.SetGlobalFloat(_RippleFadeID, _RippleFade);
    }
  
    void SetGlobalLightingShaderProperty()
    {
        //闪电
        Shader.SetGlobalFloat(_LightingPTRotXID, m_lightingRotX);
        Shader.SetGlobalFloat(_LightingPTRotYID, m_lightingRotY);
        Shader.SetGlobalFloat(_LightingPTSizeID, m_lightingPTSize);
        Shader.SetGlobalFloat(_FlashFrequencyID, m_flashFrequency);
        Shader.SetGlobalColor(_LightingColorID, m_lightingColor);
    }
    void ThunderLighting()
    {
        SetGlobalLightingShaderProperty();
        _curveTime += Time.deltaTime;
        m_light.intensity = _originLightIntensity + m_thunderLightingCurve.Evaluate(_curveTime);
        float skyboxCurve= m_skyBoxLightingCurve.Evaluate(_curveTime);
        if (skyboxCurve > 0.1f)
        {
            blinkOnce = true;
            Shader.EnableKeyword("_LIGHTING_ON");

        }
        else
        {
            if (blinkOnce)
            {
                blinkOnce = false;
                m_lightingRotX = Random.Range(-220, -201);
                m_lightingRotY = Random.Range(0, 361);
                m_lightingPTSize = Random.Range(0.15f, 0.2f);
                m_flashFrequency = Random.Range(2f, 4f);
            }
            Shader.DisableKeyword("_LIGHTING_ON");
        }

        if (_curveTime > 100)
        {
            _curveTime = 0;
        }
    }
    private void OnDisable()
    {
        _startRain = false;
        _stopRain = false;
        m_lighting = false;
        Shader.DisableKeyword("_LIGHTING_ON");
        Shader.DisableKeyword("_RAIN");
    }
}

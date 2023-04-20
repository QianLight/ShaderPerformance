using System;
using UnityEngine;
using UnityEngine.Rendering;
using CFClient.React;

namespace CFEngine.WeatherSystem
{
    public struct LightParamData
    {
        public float Intensity;
        public float BounceIntensity;
        public Color LightColor;

        public LightParamData(Light light)
        {
            Intensity = light.intensity;
            BounceIntensity = light.bounceIntensity;
            LightColor = light.color;
        }
    }
    
    // [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class WeatherBehaviour : MonoBehaviour
    {
        private static readonly int _ExplorationCurSpecCube = Shader.PropertyToID("_ExplorationCurSpecCube");
        private static readonly int _ExplorationTargetSpecCube = Shader.PropertyToID("_ExplorationTargetSpecCube");
        private static readonly int _ExplorationParams = Shader.PropertyToID("_ExplorationParams");
        private static readonly int _Wave1Param = Shader.PropertyToID("_Wave1Param");

        private static readonly int _TestCurrentBaseTex = Shader.PropertyToID("_TestCurrentBaseTex");
        private static readonly int _TestTargetBaseTex = Shader.PropertyToID("_TestTargetBaseTex");
        private static readonly int _TestCurrentMaskTex = Shader.PropertyToID("_TestCurrentMaskTex");
        private static readonly int _TestTargetMaskTex = Shader.PropertyToID("_TestTargetMaskTex");
        

        #region SunParam
        private Light _sunLight;
        private bool _timeRun;
        private float _oldTime;
        private float _sunTimer;

        //设置水平线日出日落的时间
        private int m_sunriseHour = 6; //6点日出
        private int m_sunsetHour = 20; //20点日落

        private float m_targetTime, m_needSecond;
        private float _currentTime;

        #endregion

        #region VolumeParam

        [SerializeField] private WeatherConfigData[] _weatherConfigDatas;
        [SerializeField] private Renderer seaRender;

        private Volume m_firstVolume, m_secondVolume;
        private float _volumeTimer;
        private bool _isNeedChangeVolume;
        private float _changeTime;
        private int _currentIndex;
        private int _prevIndex;
        private bool _firstOnce;

        private Vector4[] _seaWaves;
        private LightParamData[] _lightParamDatas;

        #endregion

        private void Awake()
        {
            if (_weatherConfigDatas.Length > 1)
            {
                _weatherConfigDatas[0].volume.weight = 1f;
                _weatherConfigDatas[1].volume.weight = 0f;

                m_firstVolume = _weatherConfigDatas[0].volume;
                m_secondVolume = _weatherConfigDatas[1].volume;
            }
        }

        private void Start()
        {
            int environmentLen = _weatherConfigDatas.Length;
            _seaWaves = new Vector4[environmentLen];
            _lightParamDatas = new LightParamData[environmentLen];
            
            for (int i = 0; i < environmentLen; i++)
            {
                WeatherConfigData tempWeatherConfigData = _weatherConfigDatas[i];
                if (tempWeatherConfigData == null)
                {
                    continue;
                }
                
                if (tempWeatherConfigData.seaMaterial != null)
                {
                    _seaWaves[i] = tempWeatherConfigData.seaMaterial.GetVector(_Wave1Param);
                }
                
                _lightParamDatas[i] = new LightParamData(tempWeatherConfigData.light);
            }

            if (environmentLen > 1)
            {
                for (int i = 1; i < environmentLen; i++)
                {
                    _weatherConfigDatas[i].light.gameObject.SetActive(false);
                }
            }

            _sunLight = _weatherConfigDatas[0].light;
        }

        private void OnEnable()
        {
            WeatherManager.Instance.Register(this);
        }

        private void OnDisable()
        {
            WeatherManager.Instance.UnRegister(this);

            Shader.SetGlobalVector(_ExplorationParams, Vector4.zero);
        }

        private void OnDestroy()
        {
            Shader.SetGlobalVector(_ExplorationParams, Vector4.zero);
        }

        private void LateUpdate()
        {
            // UpdateSun();
            UpdateVolumeBlend();
        }

        public void SetWeatherParams(WeatherSystemExtend.WeatherParams weatherParams)
        {
            SetTime(weatherParams.Clock, weatherParams.TransitionTime);
            SetProfile(weatherParams.targetVolumeIndex, weatherParams.TransitionTime);
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="index">profile的索引</param>
        /// <param name="needSecond">变化时间</param>
        private void SetProfile(int index, float needSecond)
        {
            if (_weatherConfigDatas == null || index < 0 || _currentIndex == index || index >= _weatherConfigDatas.Length)
            {
                return;
            }

            Shader.SetGlobalTexture(_ExplorationCurSpecCube, _weatherConfigDatas[_currentIndex].reflectCubemap);
            Shader.SetGlobalTexture(_ExplorationTargetSpecCube, _weatherConfigDatas[index].reflectCubemap);

            Shader.SetGlobalTexture(_TestCurrentBaseTex, _weatherConfigDatas[_currentIndex].skyboxBaseMap);
            Shader.SetGlobalTexture(_TestTargetBaseTex, _weatherConfigDatas[index].skyboxBaseMap);
            Shader.SetGlobalTexture(_TestCurrentMaskTex, _weatherConfigDatas[_currentIndex].skyboxMaskMap);
            Shader.SetGlobalTexture(_TestTargetMaskTex, _weatherConfigDatas[index].skyboxMaskMap);
            
            Shader.SetGlobalVector(_ExplorationParams, new Vector4(1, 0, 0, 0));

            _firstOnce = true;
            _changeTime = needSecond;
            _prevIndex = _currentIndex;
            _currentIndex = index;
            _isNeedChangeVolume = true;
            _volumeTimer = 0;
        }

        private void UpdateVolumeBlend()
        {
            if (_isNeedChangeVolume)
            {
                VolumeManager.instance.isForceUpdateVolume = true;
                if (_firstOnce)
                {
                    m_secondVolume.profile = _weatherConfigDatas[_currentIndex].volumeProfile;
                    m_firstVolume.weight = 1;
                    m_secondVolume.weight = 1;
                    _firstOnce = false;
                }

                _volumeTimer += Time.deltaTime;
                float t = _volumeTimer / _changeTime;

                m_firstVolume.weight = Mathf.Lerp(1f, 0f, t);

                UpdateSeaParam(t);
                UpdateSunParam(t);

                //这里低于0.01会出现闪烁，故设置成0.02
                if (m_firstVolume.weight <= 0.01f)
                {
                    UpdateSeaParam(1);
                    UpdateSunParam(1);
                    
                    _isNeedChangeVolume = false;
                    m_firstVolume.weight = 0f;
                    m_firstVolume.profile = m_secondVolume.profile;
                    VolumeManager.instance.isForceUpdateVolume = false;

                    Shader.SetGlobalVector(_ExplorationParams, new Vector4(0, 1, 0, 0));
                    RenderSettings.customReflection = _weatherConfigDatas[_currentIndex].reflectCubemap;
                }
            }
        }

        private void UpdateSeaParam(float t)
        {
            seaRender.material.Lerp(_weatherConfigDatas[_prevIndex].seaMaterial, _weatherConfigDatas[_currentIndex].seaMaterial, t);
            Shader.SetGlobalVector(_ExplorationParams, new Vector4(1, t, 0, 0));
            seaRender.material.SetVector(_Wave1Param, _seaWaves[_currentIndex]);
        }

        private void UpdateSunParam(float t)
        {
            _sunLight.intensity = Mathf.Lerp(_lightParamDatas[_prevIndex].Intensity, _lightParamDatas[_currentIndex].Intensity, t);
            _sunLight.color = Color.Lerp(_lightParamDatas[_prevIndex].LightColor, _lightParamDatas[_currentIndex].LightColor, t);
        }

        #region Sun

        private void UpdateSun()
        {
            if (_timeRun)
            {
                UpdateTimeOfDay();
                RotateSunLight();
            }
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="targetTime">目标时间24小时制</param>
        /// <param name="needSecond">所需时间</param>    
        private void SetTime(float targetTime, float needSecond)
        {
            if (Math.Abs(_currentTime - targetTime) > 0.0001f && (targetTime >= 0 && targetTime <= 24))
            {
                _timeRun = true;
                _sunTimer = 0;
                if (targetTime < _currentTime)
                {
                    targetTime -= _currentTime;
                    if (targetTime < 0)
                    {
                        m_targetTime = targetTime + 24 + _currentTime;
                    }
                }
                else
                {
                    m_targetTime = targetTime;
                }

                _oldTime = _currentTime;
                m_needSecond = needSecond;
            }
        }

        private void UpdateTimeOfDay()
        {
            _sunTimer += Time.deltaTime;
            _currentTime = Mathf.Lerp(_oldTime, m_targetTime, _sunTimer / m_needSecond);

            if (_currentTime >= 24)
            {
                _currentTime = 0;
                m_targetTime -= 24;
                _oldTime = 0;
            }

            if (_sunTimer - m_needSecond >= 0.01f)
            {
                _currentTime = m_targetTime;
                _timeRun = false;
                _sunTimer = 0;
            }
        }

        private void RotateSunLight()
        {
            if (_sunLight == null)
            {
                return;
            }

            float sunLightRotation;
            if (_currentTime > m_sunriseHour && _currentTime < m_sunsetHour)
            {
                float sunriseToSunsetDuration = CalculateTimeDifference(m_sunriseHour, m_sunsetHour);
                float timSinceSunrise = CalculateTimeDifference(m_sunriseHour, _currentTime);

                sunLightRotation = Mathf.Lerp(0, 180, timSinceSunrise / sunriseToSunsetDuration);
            }
            else
            {
                float sunsetToSunriseDuration = CalculateTimeDifference(m_sunsetHour, m_sunriseHour);
                float timeSinceSunset = CalculateTimeDifference(m_sunsetHour, _currentTime);

                sunLightRotation = Mathf.Lerp(180, 360, timeSinceSunset / sunsetToSunriseDuration);
            }

            _sunLight.transform.rotation = Quaternion.AngleAxis(sunLightRotation, Vector3.right);
        }

        private float CalculateTimeDifference(float fromTime, float toTime)
        {
            float diff = toTime - fromTime;
            if (diff < 0)
            {
                diff += 24;
            }

            return diff;
        }

        #endregion
    }
}
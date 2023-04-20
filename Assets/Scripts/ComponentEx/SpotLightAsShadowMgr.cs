using System;
using System.Collections.Generic;
using System.Linq;
using CFClient;
using CFEngine;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
#endif
public class SpotLightAsShadowMgr : MonoBehaviour
{
    public static readonly string SpotLightAsShader = "_ADDITIONAL_LIGHTS_AS_SHADOW";
    public static readonly int SpotLightShadowIntensity = Shader.PropertyToID("_SpotLightShadowIntensity");


    [SerializeField] public List<SpotLightAsShadow> shadowLights;
    private readonly static int LIGHT_COUNT = 2;
    private readonly SpotLightAsShadow[] _lightListNow = new SpotLightAsShadow[LIGHT_COUNT + 1];
    private readonly float[] _distanceNow = new float[LIGHT_COUNT + 1];
    
    private Transform _targetTransform;
    
    //尝试获取characterController
    private CharacterController _characterController1;
    private bool _isNeedFind = false;
    private float _remainTime = 1f;
    private float _remainCount = 40;
    private static int UPDATE_TICK = 6;
    private int _tick;

    private void OnDestroy()
    {
        Shader.DisableKeyword(SpotLightAsShader);
    }


#if UNITY_EDITOR
    [ContextMenu("Refesh")]
    private void Refesh()
    {
        Scene sceneCopy = SceneManager.GetActiveScene();
        GameObject[] roots = sceneCopy.GetRootGameObjects();
        shadowLights = new List<SpotLightAsShadow>();

        foreach (var root in roots)
        {
            var shadows = root.GetComponentsInChildren<SpotLightAsShadow>(true);
            shadowLights.AddRange(shadows);
        }

        Init();
    }

#endif
    private void Init()
    {
        if (shadowLights == null || shadowLights.Count == 0)
        {
            return;
        }

        shadowLights?[0]?.SetLightStatus(LightStatus.ON, true);
        var count = Mathf.Min(LIGHT_COUNT, shadowLights.Count);
        for (int i = 1; i < count; i++)
        {
            shadowLights[i]?.SetLightStatus(LightStatus.ON, true);
        }

        for (int i = count; i < shadowLights.Count; i++)
        {
            shadowLights[i]?.SetLightStatus(LightStatus.OFF, false);

        }
    }

    private void Start()
    {
        Init();
    }

    private void TryGetCharacterController()
    {
        if (_remainCount < 0)
        {
            if (EngineContext.instance == null || !EngineContext.instance.CameraRef)
            {
                return;
            }

            _targetTransform = EngineContext.instance.CameraRef.transform;
            return;
        }

        if (_characterController1 == null)
        {
            _isNeedFind = true;
        }

        if (_isNeedFind)
        {
            _remainTime -= Time.deltaTime;

            if (_remainTime > 0)
            {
                return;
            }
            
            _remainCount -= 1;
            _remainTime = 1f;
            CharacterController tempCharacterController = GameObject.FindObjectOfType<CharacterController>();
            if (tempCharacterController != null)
            {
                _remainCount = 40;
                _isNeedFind = false;
                _characterController1 = tempCharacterController;
                _targetTransform = _characterController1.transform;
            }
        }
    }

    void GetMax(Vector3 pos)
    {
        for (int i = 0; i < LIGHT_COUNT + 1; i++)
        {
            _distanceNow[i] = float.MaxValue;
            _lightListNow[i] = null;
        }


        for (int i = 0; i < shadowLights.Count; i++)
        {
            var lightComp = shadowLights[i];
            if (lightComp == null)
            {
                continue;
            }

            var dis = Vector3.SqrMagnitude(lightComp.transform.position - pos);
            for (int j = LIGHT_COUNT; j >= 0; j--)
            {
                if (dis < _distanceNow[j])
                {
                    if (j == LIGHT_COUNT)
                    {
                        _distanceNow[j] = dis;
                        _lightListNow[j] = lightComp;
                    }
                    else
                    {
                        _distanceNow[j + 1] = _distanceNow[j];
                        _lightListNow[j + 1] = _lightListNow[j];
                        _distanceNow[j] = dis;
                        _lightListNow[j] = lightComp;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (shadowLights == null || shadowLights.Count == 0)
        {
            return;
        }

        if(TimelinePlayContext.context && TimelinePlayContext.context.playing)
        {
            Shader.SetGlobalFloat(SpotLightShadowIntensity, 0);
            return;
        }

        TryGetCharacterController();

        if (_targetTransform == null || shadowLights == null)
        {
            return;
        }
        
        _tick = (_tick++) % UPDATE_TICK;
        if (_tick != 0)
        {
            return;
        }

        var characterControllerPosition = _targetTransform.position;
        GetMax(characterControllerPosition);
        var maxDistanceLight = _lightListNow[LIGHT_COUNT-1];
        var minDistanceLight = _lightListNow[0];
        var hasShadow = false;
        if (minDistanceLight)
        {
            var status = minDistanceLight.LightStatus == LightStatus.ON ? LightStatus.ON : LightStatus.LIGHTING;
            var shadow = minDistanceLight.IsCasterShadow;
            minDistanceLight.SetLightStatus(status, shadow);
            minDistanceLight.UpdateLight();
            hasShadow = shadow;
        }

        for (int i = 1; i < LIGHT_COUNT; i++)
        {
            var lightComp = _lightListNow[i];
            if (lightComp)
            {
                var status = lightComp.LightStatus == LightStatus.ON ? LightStatus.ON : LightStatus.LIGHTING;
                var shadow = lightComp.IsCasterShadow && !hasShadow;
                lightComp.SetLightStatus(status, shadow);
                maxDistanceLight.UpdateLight();
                if (shadow)
                {
                    hasShadow = true;
                }
            }
        }

        var hasClosing = false;
        if (maxDistanceLight)
        {
            var status = maxDistanceLight.LightStatus == LightStatus.OFF ? LightStatus.OFF : LightStatus.CLOSING;
            if (status == LightStatus.CLOSING)
            {
                hasClosing = true;
                var shadow = maxDistanceLight.IsCasterShadow && !hasShadow;
                maxDistanceLight.SetLightStatus(status, shadow);
                maxDistanceLight.UpdateLight();
                if (shadow)
                {
                    hasShadow = true;
                }
            }
        }

        for (int i = 0; i < shadowLights.Count ; i++)
        {
            var lightComp = shadowLights[i];

            if (lightComp == null || _lightListNow.Contains(lightComp) || lightComp.LightStatus == LightStatus.OFF )
            {
                continue;
            }

            if (!hasClosing)
            {
                hasClosing = true;
                var shadow = lightComp.IsCasterShadow && !hasShadow;
                lightComp.SetLightStatus(LightStatus.CLOSING, shadow);
                lightComp.UpdateLight();
                if (shadow)
                {
                    hasShadow = true;
                }
            }
            else
            {
                var shadow = lightComp.IsCasterShadow && !hasShadow;
                lightComp.SetLightStatus(LightStatus.OFF, shadow);
                lightComp.UpdateLight();
                if (shadow)
                {
                    hasShadow = true;
                }
            }

        }
    }
}
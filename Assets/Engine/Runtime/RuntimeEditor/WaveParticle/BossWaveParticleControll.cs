using System;
using System.Collections;
using System.Collections.Generic;
using CFEngine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class BossWaveParticleControll : WaveParticleControll
{
    public int m_emitCountOnce = 2;
    
    public  Vector3 m_skillEmitPosition = Vector3.zero;
    public  bool m_canEmit = true;
    public float m_emissionTimeSkip = 1f;
    private float _tempTime = 0;
    void Start()
    {
        base.Init();
    }

    private void Update()
    {
        if (GameQualitySetting.SFXLevel != RenderQualityLevel.Ultra 
        && GameQualitySetting.SFXLevel != RenderQualityLevel.High)
        {
            return;
        }

        this.transform.position = m_skillEmitPosition;
        if (m_canEmit)
        {
            if (_tempTime > 0)
            {
                _tempTime -= Time.deltaTime;
            }
            else
            {
                _particle.Play();
                Shader.EnableKeyword("_PPWave");
                _tempTime = m_emissionTimeSkip;
            }
        }
        else
        {
            _particle.Stop();
        }
    }

    private void OnDestroy()
    {
        if (GameQualitySetting.SFXLevel != RenderQualityLevel.Ultra 
            && GameQualitySetting.SFXLevel != RenderQualityLevel.High)
        {
            return;
        }
        Shader.DisableKeyword("_PPWave");
    }
}
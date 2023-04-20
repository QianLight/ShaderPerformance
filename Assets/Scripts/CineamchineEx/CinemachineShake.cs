using CFUtilPoolLib;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using CFEngine;

public class CinemachineShake : XSingleton<CinemachineShake>
{
    public float DefaultDissipationDistance = 50;
    public float DefaultPropagationSpeed = 300;

    private const int impulseCount = 5;
    private CinemachineImpulseDefinition[] impulse = new CinemachineImpulseDefinition[impulseCount];
    private NoiseSettings[] shakeAsset = new NoiseSettings[impulseCount];
    private CinemachineImpulseManager.ImpulseEvent[] impulseEvent = new CinemachineImpulseManager.ImpulseEvent[impulseCount];
    private int index = 0;

    public CinemachineShake()
    {
        for (int i = 0; i < impulseCount; ++i)
        {
            impulse[i] = new CinemachineImpulseDefinition();
            shakeAsset[i] = ScriptableObject.CreateInstance<NoiseSettings>();

            impulse[i].m_Randomize = false;
            impulse[i].m_DissipationDistance = DefaultDissipationDistance;
            impulse[i].m_PropagationSpeed = DefaultPropagationSpeed;
            impulse[i].m_DissipationMode = CinemachineImpulseManager.ImpulseEvent.DissipationMode.ExponentialDecay;
        }
    }

    public void OnDestory()
    {
    }

    public int SetImpulse(string path, float amplitudeGain, float frequencyGain, float time, float attackTime, float decayTime, Vector3 pos, float radius)
    {
        if (string.IsNullOrEmpty(path))
            return -1;
        NoiseSettings source = LoadMgr.singleton.LoadAssetImmediate<NoiseSettings>(path, ResObject.ResExt_Asset);
        if (source == null)
        {
            LoadMgr.singleton.DestroyImmediate();
            return -1;
        }

        CinemachineImpulseDefinition impulseData = impulse[index % impulseCount];
        NoiseSettings shakeAssetData = shakeAsset[index % impulseCount];

        NoiseSettings.TransformNoiseParams[] PositionNoise = new NoiseSettings.TransformNoiseParams[source.PositionNoise.Length];
        NoiseSettings.TransformNoiseParams[] OrientationNoise = new NoiseSettings.TransformNoiseParams[source.OrientationNoise.Length];
        for (int i = 0; i < source.PositionNoise.Length; ++i)
        {
            PositionNoise[i] = source.PositionNoise[i];
        }
        for (int i = 0; i < source.OrientationNoise.Length; ++i)
        {
            OrientationNoise[i] = source.OrientationNoise[i];
        }
        shakeAssetData.PositionNoise = PositionNoise;
        shakeAssetData.OrientationNoise = OrientationNoise;
        impulseData.m_RawSignal = shakeAssetData;
        impulseData.m_AmplitudeGain = amplitudeGain;
        impulseData.m_FrequencyGain = frequencyGain;
        impulseData.m_TimeEnvelope.m_SustainTime = Mathf.Max(0, time - attackTime - decayTime);
        impulseData.m_TimeEnvelope.m_AttackTime = attackTime;
        impulseData.m_TimeEnvelope.m_DecayTime = decayTime;
        impulseData.m_TimeEnvelope.m_ScaleWithImpact = false;
        impulseData.m_ImpactRadius = radius;
        impulseEvent[index % impulseCount] = impulseData.CreateAndReturnEvent(pos, Vector3.down);
        LoadMgr.singleton.DestroyImmediate();
        return index++;
    }

    public void Cancel(int id)
    {
        if (id >= 0 && id + impulseCount >= index)
        {
            impulseEvent[id % impulseCount].Cancel(0, true);
        }
    }
}

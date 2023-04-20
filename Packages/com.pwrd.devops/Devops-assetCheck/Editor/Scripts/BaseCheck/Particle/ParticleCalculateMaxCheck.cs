using AssetCheck;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CheckRuleDescription("ParticleSystem", "峰值粒子数", "t:Prefab", "")]
public class ParticleCalculateMaxCheck : RuleBase
{
    [PublicParam("最大粒子发射数量", eGUIType.Input)]
    public int particleeMissionCount = 100;

    [PublicMethod]
    public bool Check(string path, out string output)
    {
        output = string.Empty;
        GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (gameObject == null)
            return true;
        ParticleSystem[] particles = gameObject.GetComponentsInChildren<ParticleSystem>();
        if (particles == null)
            return true;

        int currentParticleeMissionCount = 0;
        foreach (ParticleSystem particle in particles)
        {
            int count = CalculateMaxParticleAmount(particle);
            currentParticleeMissionCount += count;
        }
        output = $"{currentParticleeMissionCount}--标准值--{particleeMissionCount}--超出占比{(float)(currentParticleeMissionCount - particleeMissionCount) / particleeMissionCount}%";
        return currentParticleeMissionCount < particleeMissionCount;
    }

    public static int CalculateMaxParticleAmount(ParticleSystem particle)
    {
        if (!particle.emission.enabled) return 0;
        bool loop = particle.main.loop;
        float maxParticleAmount = 0;
        float lifeTime = 0;
        float duration = 0;
        lifeTime = CalculateParticleLifeTime(particle);
        float emitRate = 0;
        if (particle.emission.rateOverTime.mode == ParticleSystemCurveMode.Constant)
        {
            emitRate = particle.emission.rateOverTime.constant;
        }
        if (particle.emission.rateOverTime.mode == ParticleSystemCurveMode.Curve)
        {
            var curve = particle.emission.rateOverTime.curve;
            float maxValue = GetCurveMaxValue(curve);
            emitRate = maxValue * particle.emission.rateOverTime.curveMultiplier;
        }
        if (particle.emission.rateOverTime.mode == ParticleSystemCurveMode.TwoConstants)
        {
            emitRate = (particle.emission.rateOverTime.constantMin + particle.emission.rateOverTime.constantMax) / 2;
        }
        if (particle.emission.rateOverTime.mode == ParticleSystemCurveMode.TwoCurves)
        {
            var curve = particle.emission.rateOverTime.curveMax;
            float maxValue = GetCurveMaxValue(curve);
            emitRate = maxValue * particle.emission.rateOverTime.curveMultiplier;
        }
        if (particle.emission.burstCount > 0)
        {
            ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[particle.emission.burstCount];
            particle.emission.GetBursts(bursts);
            float burstCountOverLife = 0;
            foreach (var burst in bursts)
            {
                if (burst.cycleCount == 0)
                {
                    //infinitely
                    burstCountOverLife += particle.main.maxParticles;
                }
                else
                {
                    if (burst.count.mode == ParticleSystemCurveMode.Constant)
                    {
                        burstCountOverLife += burst.maxCount * burst.cycleCount;
                    }
                    else if (burst.count.mode == ParticleSystemCurveMode.Curve)
                    {
                        var curve = burst.count.curve;
                        float maxValue = GetCurveMaxValue(curve);
                        burstCountOverLife += maxValue * burst.count.curveMultiplier * burst.cycleCount;
                    }
                    else if (burst.count.mode == ParticleSystemCurveMode.TwoConstants)
                    {
                        float count = (burst.count.constantMin + burst.count.constantMax) / 2;
                        burstCountOverLife += count * burst.cycleCount;
                    }
                    else if (burst.count.mode == ParticleSystemCurveMode.TwoCurves)
                    {
                        var curve = burst.count.curveMax;
                        float maxValue = GetCurveMaxValue(curve);
                        burstCountOverLife += maxValue * burst.count.curveMultiplier * burst.cycleCount;
                    }
                }
            }
            maxParticleAmount += burstCountOverLife;
        }
        if (!loop)
        {
            duration = particle.main.duration;
            float period = Mathf.Min(lifeTime, duration);
            maxParticleAmount += period * emitRate;
        }
        if (loop)
        {
            maxParticleAmount += lifeTime * emitRate;
        }
        if (maxParticleAmount > particle.main.maxParticles)
            maxParticleAmount = particle.main.maxParticles;
        return (int)maxParticleAmount;
    }
    public static float CalculateParticleLifeTime(ParticleSystem particle)
    {
        float lifeTime = 0;
        if (particle.main.startLifetime.mode == ParticleSystemCurveMode.Constant)
        {
            lifeTime = particle.main.startLifetime.constant;
        }
        if (particle.main.startLifetime.mode == ParticleSystemCurveMode.Curve)
        {
            var curve = particle.main.startLifetime.curve;
            lifeTime = GetCurveMaxValue(curve);
        }
        if (particle.main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
        {
            lifeTime = (particle.main.startLifetime.constantMin + particle.main.startLifetime.constantMax) / 2;
        }
        if (particle.main.startLifetime.mode == ParticleSystemCurveMode.TwoCurves)
        {
            var curve = particle.main.startLifetime.curveMax;
            lifeTime = GetCurveMaxValue(curve);
        }
        return lifeTime;
    }

    public static float GetCurveMaxValue(AnimationCurve curve)
    {
        float max = 0;
        for (int i = 0; i < curve.length; i++)
        {
            max = Mathf.Max(max, curve[i].value);
        }
        return max;
    }
}

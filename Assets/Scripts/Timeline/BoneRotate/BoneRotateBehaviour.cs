using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Timeline;
#endif


/// <summary>
/// BoneRotateBehaviour为控制BoneRotateAsset执行时机，主要在ProcessFrame中进行插值，在OnBehaviourPause停止插值。
/// 其关键点是使用四元数进行插值，进行旋转赋值，而不是使用欧拉角插值。使用欧拉角会出现人物头部旋转出错的问题。
/// 所以将配置的角度，转换成四元数之后，与人物的当前的旋转值，进行插值。
/// </summary>
public class BoneRotateBehaviour : DirectBaseBehaviour
{
    private string m_headPath = "root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Neck/Bip001 Head";
    private Transform m_head;
    private LaterUpdateRotation m_laterUpdateRotation;
    public BoneRotateAsset.TweenType m_tweenType;

    private Quaternion m_startQuaternion = Quaternion.identity;
    private Quaternion m_endQuaternion = Quaternion.identity;
    public BoneRotateAsset RotateAsset
    {
        get
        {
            return asset as BoneRotateAsset;
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.PrepareFrame(playable, info);

        BoneRotateAsset boneRotateAsset = asset as BoneRotateAsset;
        m_tweenType = boneRotateAsset.m_tweenType;

        if (m_head == null)
        {
            GameObject go = playerData as GameObject;
            if (go != null)
            {
                m_head = go.transform.Find(m_headPath);
                m_laterUpdateRotation = m_head.transform.GetComponent<LaterUpdateRotation>();
            }
        }

        if (m_head == null)
        {
            Debug.LogError("can not find head bone!");
            return;
        }

        if(m_laterUpdateRotation == null && m_head != null)
        {
            m_laterUpdateRotation = m_head.gameObject.AddComponent<LaterUpdateRotation>();
        }

        if (boneRotateAsset.m_bone == BoneRotateAsset.RotateType.HEAD_OFF)
        {
            m_laterUpdateRotation.enabled = false;
            return;
        }
        else
        {
            m_laterUpdateRotation.enabled = true;
        }

        Quaternion blendedRotation = new Quaternion(0f, 0f, 0f, 0f);
        double curTime = playable.GetTime();
        float normalisedTime = (float)(curTime / playable.GetDuration());
        float tweenProgress = EvaluateCurrentCurve(normalisedTime);

        m_endQuaternion = Quaternion.Euler(boneRotateAsset.m_endRot);

        if (boneRotateAsset.m_startRotEnable)
        {
            m_startQuaternion = Quaternion.Euler(boneRotateAsset.m_startRot);
        }
        else
        {
            m_startQuaternion = m_laterUpdateRotation.GetCurQuaternion();
        }

        Quaternion desiredRotation = Quaternion.Lerp(m_startQuaternion, m_endQuaternion, tweenProgress);

        m_laterUpdateRotation.SetLaterQuaternion(desiredRotation);
        Vector3 angles = m_head.transform.localEulerAngles;
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
        if (m_laterUpdateRotation != null)
        {
            m_laterUpdateRotation.enabled = false;
            GameObject.DestroyImmediate(m_laterUpdateRotation);
        }
    }

    public float EvaluateCurrentCurve(float time)
    {
        if (m_tweenType == BoneRotateAsset.TweenType.Custom && !IsCustomCurveNormalised())
        {
            Debug.LogError("Custom Curve is not normalised.  Curve must start at 0,0 and end at 1,1.");
            return 0f;
        }

        switch (m_tweenType)
        {
            case BoneRotateAsset.TweenType.Linear:
                return RotateAsset.m_LinearCurve.Evaluate(time);
            case BoneRotateAsset.TweenType.Deceleration:
                return RotateAsset.m_DecelerationCurve.Evaluate(time);
            case BoneRotateAsset.TweenType.Harmonic:
                return RotateAsset.m_HarmonicCurve.Evaluate(time);
            default:
                return RotateAsset.customCurve.Evaluate(time);
        }
    }

    bool IsCustomCurveNormalised()
    {
        if (!Mathf.Approximately(RotateAsset.customCurve[0].time, 0f))
            return false;

        if (!Mathf.Approximately(RotateAsset.customCurve[0].value, 0f))
            return false;

        if (!Mathf.Approximately(RotateAsset.customCurve[RotateAsset.customCurve.length - 1].time, 1f))
            return false;

        return Mathf.Approximately(RotateAsset.customCurve[RotateAsset.customCurve.length - 1].value, 1f);
    }
}


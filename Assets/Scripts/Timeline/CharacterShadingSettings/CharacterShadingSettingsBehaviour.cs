using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;
using System.Drawing;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Timeline;
#endif


public class CharacterShadingSettingsBehaviour : DirectBaseBehaviour
{
    public CharacterShadingSettingsAsset ControlPartInfo
    {
        get
        {
            return asset as CharacterShadingSettingsAsset;
        }
    }

    private GameObject m_go;
    private Renderer[] m_Renderers;
    private static readonly int OlzWrite = Shader.PropertyToID("_OLZWrite");
    private bool m_executed = false;

    public UnityEngine.Color preShadowColorMultiply = UnityEngine.Color.gray;
    public Texture2D preCustomGradientTexture = null;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        BeginEffect();
    }

    /// <summary>
    /// 有可能在OnBehaviourPlay的时候m_go还没有加载好，比如点击下一句对话的时候，直接跳转。
    /// 此时在PrepareFrame中检测m_go，并且保证只执行一次，m_executed记录是否执行过。
    /// </summary>
    /// <param name="playable"></param>
    /// <param name="info"></param>
    public override void PrepareFrame(Playable playable, FrameData info)
    {
        base.PrepareFrame(playable, info);

        if (m_go == null && !m_executed)
        {
            BeginEffect();
        }
    }

    private void BeginEffect()
    {
        m_go = RTimeline.singleton.Director.GetGenericBinding(ControlPartInfo.m_trackAsset) as GameObject;
        if (m_go == null)
        {
            Animator animator = RTimeline.singleton.Director.GetGenericBinding(ControlPartInfo.m_trackAsset) as Animator;
            if (animator != null) m_go = animator.gameObject;
        }

        if (m_go == null) return;

        if (m_Renderers == null)
        {
            m_Renderers = m_go.GetComponentsInChildren<Renderer>();
        }

        for (int i = 0; i < m_Renderers.Length; i++)
        {
            var mat = m_Renderers[i].sharedMaterial;

            if (ControlPartInfo.outlineZWrite)
                mat.SetFloat(OlzWrite, 1f);
            if (ControlPartInfo.darkEffect)
                mat.SetColor("_Color", ControlPartInfo.darkEffectColor);

            if (m_Renderers[i].name.Contains("_face"))
            {
                if(ControlPartInfo.shadowColorEffect)
                {
                    preShadowColorMultiply = mat.GetColor("_Color3");
                    mat.SetColor("_Color3", ControlPartInfo.shadowColorMultiply);
                }
                if(ControlPartInfo.customGradientEffect && ControlPartInfo.customGradientTexture != null)
                {
                    preCustomGradientTexture = (Texture2D)mat.GetTexture("_ProcedureTex2");
                    mat.SetTexture("_ProcedureTex2", ControlPartInfo.customGradientTexture);
                }
            }
        }
        m_executed = true;
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
        EndEffect();
    }

    /// <summary>
    /// 跳过的时候，m_go销毁了，所以要判断m_go是否为null，否则会报错
    /// </summary>
    private void EndEffect()
    {
        if (m_go == null || m_Renderers == null) return;
        for (int i = 0; i < m_Renderers.Length; i++)
        {
            var mat = m_Renderers[i].sharedMaterial;

            if (ControlPartInfo.outlineZWrite)
                mat.SetFloat(OlzWrite, 0);
            if (ControlPartInfo.darkEffect)
                mat.SetColor("_Color", UnityEngine.Color.white);

            // reset
            if (m_Renderers[i].name.Contains("_face"))
            {
                if (ControlPartInfo.shadowColorEffect)
                {
                    mat.SetColor("_Color3", preShadowColorMultiply);
                }
                if (ControlPartInfo.customGradientEffect && preCustomGradientTexture != null)
                {
                    mat.SetTexture("_ProcedureTex2", preCustomGradientTexture);
                }
            }
        }
        m_executed = false;
    }
}

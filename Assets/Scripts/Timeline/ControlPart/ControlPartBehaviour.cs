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

public class ControlPartBehaviour : DirectBaseBehaviour
{
    public ControlPartAsset ControlPartInfo
    {
        get
        {
            return asset as ControlPartAsset;
        }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        PlayableDirector director = (playable.GetGraph().GetResolver() as PlayableDirector);
        if (director == null)
        {
            return;
        }

        GameObject go = RTimeline.singleton.Director.GetGenericBinding(ControlPartInfo.m_trackAsset) as GameObject;
        if (go == null)
        {
            return;
        }


        if (ControlPartInfo != null)
        {
            List<ControlPartAsset.PartInfo> parts = ControlPartInfo.m_partInfos;
            ControlPartInfo.m_cachedEnables.Clear();
            for (int i = 0; i < parts.Count; ++i)
            {
                Transform part = go.transform.Find(parts[i].m_path);
                if (part != null)
                {
                    ControlPartInfo.m_cachedEnables.Add(part.gameObject.activeSelf);
                    part.gameObject.SetActive(parts[i].m_enable);
                    SkinnedMeshRenderer skinnedMeshRenderer = part.gameObject.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedMeshRenderer != null) skinnedMeshRenderer.enabled = parts[i].m_enable;
                }
            }
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
        PlayableDirector director = (playable.GetGraph().GetResolver() as PlayableDirector);
        if (director == null)
        {
            return;
        }
        GameObject go = RTimeline.singleton.Director.GetGenericBinding(ControlPartInfo.m_trackAsset) as GameObject;
        if (go == null)
        {
            return;
        }

        if (director.time == 0)
        {
            return;
        }

        if (ControlPartInfo != null)
        {
            List<ControlPartAsset.PartInfo> parts = ControlPartInfo.m_partInfos;
            for (int i = 0; i < parts.Count; ++i)
            {
                Transform part = go.transform.Find(parts[i].m_path);
                if (part != null)
                {
                    part.gameObject.SetActive(ControlPartInfo.m_cachedEnables[i]);
                    SkinnedMeshRenderer skinnedMeshRenderer = part.gameObject.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedMeshRenderer != null) skinnedMeshRenderer.enabled = ControlPartInfo.m_cachedEnables[i];
                }
            }
        }
    }
}


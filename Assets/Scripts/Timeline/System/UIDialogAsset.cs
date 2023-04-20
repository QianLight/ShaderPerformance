using CFEngine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using System.IO;
[System.Serializable]
#endif

public class UIDialogAsset : DirectBasePlayable<UIDialogBehaviour>, ITimelineClipAsset
{

#if UNITY_EDITOR
    [Header("对话")]
#endif
    public string content;

#if UNITY_EDITOR
    [Header("NPC名字")]
#endif
    public string speaker;

#if UNITY_EDITOR
    [Header("头像")]
#endif
    public string m_head;

#if UNITY_EDITOR
    [Header("表情特效")]
#endif
    public string m_face;

#if UNITY_EDITOR
    [Header("是否暂停")]
#endif
    public bool m_isPause = true;

#if UNITY_EDITOR
    [Header("是否黑背景")]
#endif
    public bool m_blackVisible = true;

#if UNITY_EDITOR
    [Header("ID")]
#endif
    public int idx;

#if UNITY_EDITOR
    [Header("Empty")]
#endif
    public bool m_isEmpty = false;

#if UNITY_EDITOR
    [Header("是否显示自动按钮")]
#endif
    public bool m_autoEnable = false;

#if UNITY_EDITOR
    [Header("是否显示跳过按钮")]
#endif
    public bool m_skipEnable = false;

#if UNITY_EDITOR
    [Header("对话框类型")]
#endif
    [System.NonSerialized]
    public DialogType m_DialogType = DialogType.Normal;

    public enum DialogType
    {
        Empty,
        Normal,
        Bubble,
        Fierce
    }

    [System.NonSerialized]
    public TimelineClip m_clip;

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override void Reset()
    {
        base.Reset();
        content = null;
    }

    //public override void Load(CFBinaryReader reader)
    //{
    //    base.Load(reader);
    //    content = DirectorHelper.singleton.ReadStr(reader);
    //}

#if UNITY_EDITOR
    public static void SaveAsset(BinaryWriter bw, PlayableAsset asset, bool presave)
    {
        UIDialogAsset upa = asset as UIDialogAsset;
        DirectorHelper.SaveStringIndex(bw, upa.content, presave);
    }
#endif
}

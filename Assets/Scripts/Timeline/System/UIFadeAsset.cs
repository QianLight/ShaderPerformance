using UnityEngine;
using UnityEngine.Timeline;


public class UIFadeAsset : DirectBasePlayable<UIFadeBehaviour>, ITimelineClipAsset
{

//#if UNITY_EDITOR
//    [Header("key-ID")]
//#endif
    public string key;


//#if UNITY_EDITOR
//    [Header("此字段配置在fade.txt表格导入即可")]
//#endif
    public string content;


    public string clip1;


    public string clip2;

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public string m_bgTexturePath;

}

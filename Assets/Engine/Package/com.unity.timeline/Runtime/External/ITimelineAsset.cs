#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using CFEngine;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// 实现此接口的track 其引用的资源可以预加载
    /// 否则的话 对应的track资源是及时记载的
    /// </summary>
    public interface ITimelineAsset
    {

        PlayableAssetType assetType { get; }

        /// <summary>
        /// Used in editor
        /// </summary>
        List<string> ReferenceAssets(PlayableBinding pb);

    };
}
#endif
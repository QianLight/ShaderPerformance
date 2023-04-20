using System;
using UnityEngine.Timeline;

[Serializable]
[CSDiscriptor("视频录制")]
[TrackClipType(typeof(RecorderClip))]
[TrackColor(0f, 0.53f, 0.08f)]
public class RecorderTrack : TrackAsset
{
}


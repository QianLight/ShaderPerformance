using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


[DisplayName("Recorder Clip")]
public class RecorderClip : PlayableAsset, ITimelineClipAsset, ISerializationCallbackReceiver
{

    [SerializeField]
    public RecorderSettings settings;

    internal bool needsDuplication;

    static readonly Dictionary<RecorderSettings, RecorderClip> s_SettingsLookup = new Dictionary<RecorderSettings, RecorderClip>();

    readonly SceneHook m_SceneHook = new SceneHook(Guid.NewGuid().ToString());

    Type recorderType
    {
        get { return settings == null ? null : RecordersInventory.GetRecorderInfo(settings.GetType()).recorderType; }
    }

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<RecorderPlayableBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        if (recorderType != null && UnityHelpers.IsPlaying())
        {
            behaviour.session = m_SceneHook.CreateRecorderSession(settings);
        }
        return playable;
    }
    
    public void OnBeforeSerialize()
    {
        if (settings != null)
        {
            RecorderClip clip;
            if (s_SettingsLookup.TryGetValue(settings, out clip))
            {
                if (clip != this)
                {
                    needsDuplication = true;
                }
            }
            else
            {
                s_SettingsLookup[settings] = this;
            }
        }
    }

    internal TimelineAsset FindTimelineAsset()
    {
        if (!AssetDatabase.Contains(this))
            return null;

        var path = AssetDatabase.GetAssetPath(this);
        var objs = AssetDatabase.LoadAllAssetsAtPath(path);

        foreach (var obj in objs)
        {
            if (obj != null && AssetDatabase.IsMainAsset(obj))
                return obj as TimelineAsset;
        }
        return null;
    }

    void PushTimelineIntoRecorder(TimelineAsset timelineAsset)
    {
        if (settings == null || timelineAsset == null)
            return;
        settings.FrameRate = timelineAsset.editorSettings.fps;
        settings.FrameRatePlayback = FrameRatePlayback.Constant;
        settings.CapFrameRate = true;
    }

    private void OnEnable()
    {
        PushTimelineIntoRecorder(FindTimelineAsset());
    }

    public void OnAfterDeserialize()
    {
    }
}

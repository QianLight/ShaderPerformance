using CFEngine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using System.IO;
#endif
[System.Serializable]
public class CameraPlayableAsset : DirectBasePlayable<CameraPlayableBehaviour>, ITimelineClipAsset
{
    private float dur = 0f;

    public AnimationClip _clip, _shakeClip;
    private AssetHandler animRes;
    private AssetHandler shakeRes;
    static ResLoadCb cb = LoadFinish;
    public override double duration
    {
        get { return dur; }
    }

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject go)
    {
        var behavior = GetBehavior ();
        //set param
        behavior.asset = this;
        var director = DirectorHelper.GetDirector ();
        behavior.Set (director != null?director.duration : 0);
        return ScriptPlayable<CameraPlayableBehaviour>.Create (graph, behavior);
    }
    public override void Reset ()
    {
        base.Reset ();
        LoadMgr.singleton.Destroy (ref animRes);
        LoadMgr.singleton.Destroy (ref shakeRes);
    }

    private static void LoadFinish (AssetHandler ah, LoadInstance li)
    {
        var asset = li.loadHolder as CameraPlayableAsset;
        if (asset != null)
        {
            if (li.param.y == 0)
            {
                asset._clip = ah.obj as AnimationClip;
            }
            else if (li.param.y == 1)
            {
                asset._shakeClip = ah.obj as AnimationClip;
            }
        }
    }
    
    public override void Load (CFBinaryReader reader)
    {
        base.Load(reader);
        LoadAsset<AnimationClip>(reader, 0, ResObject.ResExt_Anim, ref animRes, this, cb);
        LoadAsset<AnimationClip>(reader, 1, ResObject.ResExt_Anim, ref animRes, this, cb);
        dur = reader.ReadSingle ();
    }

#if UNITY_EDITOR

    public string path;
    public string shake;

    public void SetEditorClip (AnimationClip clip)
    {
        if (clip != null)
        {
            var dir = DirectorHelper.GetDirector();
            if (dir.playableAsset)
            {
                var fps = ((TimelineAsset) (dir.playableAsset)).editorSettings.fps;
                dur = clip.length * clip.frameRate / fps;
            }
        }
    }
    public static void SaveAsset (BinaryWriter bw, PlayableAsset asset, bool presave)
    {
        CameraPlayableAsset cpa = asset as CameraPlayableAsset;
        DirectorHelper.SaveAsset (bw, cpa._clip, ".anim", presave);
        DirectorHelper.SaveAsset (bw, cpa._shakeClip, ".anim", presave);
        if (!presave)
        {
            bw.Write (cpa.dur);
        }
    }
#endif

}
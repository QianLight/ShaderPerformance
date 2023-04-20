using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using XEditor;


/*
 *  Timeline 里使用的资源统计
 */

public class TimelineStatistic : EditorWindow
{
    internal const string menu = @"XEditor/Timeline/Statistic";
    PlayableDirector mDirector;
    const string warn = " 当前场景未发现任何Timeline, 请先加载一个timeline";

    const int ANI_MAX = 6;
    const int RECD_MAX = 5;
    const int FX_MAX = 8;
    const int BONE_MAX = 5;
    const int SUBT_MAX = 20;
    const int VM_MAX = 20;

    private GUIStyle errorStyle, normalStyle;
    private Vector2 scroll;
    
    HashSet<string> set_ani = new HashSet<string>();
    HashSet<string> set_sub = new HashSet<string>();
    HashSet<string> set_recd = new HashSet<string>();
    HashSet<string> set_fx = new HashSet<string>();
    HashSet<string> set_bone = new HashSet<string>();
    List<string> list_vm = new List<string>();

    // int act_cnt, vm_cnt;
    int subtitle_invalid_cnt;
    bool fd_ani, fd_recd, fd_fx, fd_sub, fd_vm;

    void OnEnable()
    {
        // act_cnt = 0;
        subtitle_invalid_cnt = 0;
        fd_ani = true;
        fd_fx = true;
        fd_recd = true;
        fd_sub = true;
        fd_vm = true;
        Clear();
        var director = GameObject.FindObjectOfType<PlayableDirector>();
        Analy(director);
    }

    private void OnFocus()
    {
        OnEnable();
    }

    public void Analy(PlayableDirector director)
    {
        mDirector = director;
        if (director != null && director.playableAsset != null)
        {
            foreach (PlayableBinding pb in director.playableAsset.outputs)
            {
                switch (pb.sourceObject)
                {
                    case AnimationTrack _:
                        AnalyAnim(pb.sourceObject as AnimationTrack, director);
                        break;
                    case ControlTrack _:
                        AnalyFx(pb.sourceObject as ControlTrack);
                        break;
                    case UISubtitleTrack _:
                        AnalySubtitle(pb.sourceObject as UISubtitleTrack);
                        break;
                    case CinemachineTrack _:
                        AnalyCine(pb.sourceObject as CinemachineTrack);
                        break;
                }
            }
        }
    }

    public override string ToString()
    {
        string rst = "ani count: " + set_ani.Count;
        rst += "\n record count:" + set_recd.Count;
        rst += "\n subtitle count:" + subtitle_invalid_cnt;
        return rst;
    }

    private void SetupStyle()
    {
        if (errorStyle == null)
        {
            errorStyle = new GUIStyle();
            errorStyle.normal.textColor = Color.red;
        }
        if (normalStyle == null)
        {
            normalStyle = new GUIStyle();
            normalStyle.normal.textColor = Color.white;
        }
    }
    

    public void OnGUI()
    {
        SetupStyle();
        OriginalSetting.DrawLogo(this);
        GUILayout.BeginVertical();
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.Space(4);
       
        if (mDirector && mDirector.playableAsset)
        {
            GUILayout.Label(" Timeline 资源引用统计", XEditorUtil.titleLableStyle);
        }
        else
        {
            GUILayout.Space(30);
            GUILayout.Label(warn, XEditorUtil.titleLableStyle);
            GUIUtility.ExitGUI();
        }

        
        GUILayout.Space(4);

        EditorGUILayout.LabelField(mDirector.playableAsset.name);
        fd_ani = EditorGUILayout.Foldout(fd_ani, "animation");
        if (fd_ani)
        {
            GUISet(set_ani, "animation capcity:", ANI_MAX);
        }
        fd_recd = EditorGUILayout.Foldout(fd_recd, "record");
        if (fd_recd)
        {
            GUISet(set_recd, "record capcity:", RECD_MAX);
        }
        fd_fx = EditorGUILayout.Foldout(fd_fx, "fx");
        if (fd_fx)
        {
            GUISet(set_fx, "scene fx:", FX_MAX);
            GUISet(set_bone, "bone fx:", BONE_MAX);
        }
        fd_sub = EditorGUILayout.Foldout(fd_sub, "subtitle");
        if (fd_sub)
        {
            if (subtitle_invalid_cnt > 0)
                EditorGUILayout.LabelField("   empty subtitle: " + subtitle_invalid_cnt, errorStyle);
            GUISet(set_sub, "subtitle capcity: ", SUBT_MAX);
        }
        fd_vm = EditorGUILayout.Foldout(fd_vm, "virtual camera");
        if(fd_vm)
        {
            GUISet(list_vm, "virtual camera capcity: ", VM_MAX);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }


    private void GUISet(ICollection<string> set, string title, int suggest)
    {
        int i = 0;
        int cnt = set.Count;
        var style = cnt > suggest ? errorStyle : normalStyle;
        string desc = "   -" + title + cnt;
        if (cnt > suggest) desc += string.Format("  (overflow, suggest: {0})", suggest);
        EditorGUILayout.LabelField(desc, style);
        foreach (var it in set)
        {
            EditorGUILayout.LabelField("      " + (++i) + ": " + it);
        }
    }

    void AnalyAnim(AnimationTrack track, PlayableDirector director)
    {
        if (track.infiniteClip != null)
        {
            var bind = director.GetGenericBinding(track);
            if (bind) set_recd.Add(bind.name);
        }
        else
        {
            foreach (var clip in track.GetClips())
            {
                set_ani.Add(clip.animationClip.name);
            }
        }
    }

    void AnalySubtitle(UISubtitleTrack track)
    {
        var clips = track.GetClips();
        subtitle_invalid_cnt = 0;
        foreach (var clip in clips)
        {
            UISubtitleAsset asset = clip.asset as UISubtitleAsset;
            if (string.IsNullOrEmpty(asset.subTitle))
            {
                subtitle_invalid_cnt++;
            }
            else
            {
                set_sub.Add(asset.subTitle);
            }
        }
    }

    void AnalyFx(ControlTrack track)
    {
        var clips = track.GetClips();
        foreach (var clip in clips)
        {
            set_fx.Add(clip.displayName);
        }
    }

    void AnalyCine(CinemachineTrack track)
    {
        var clips = track.GetClips();
        foreach(var clip in clips)
        {
            list_vm.Add(clip.displayName);
        }
    }

    public void Dispose()
    {
        Clear();
    }

    private void Clear()
    {
        set_sub.Clear();
        set_ani.Clear();
        set_fx.Clear();
        set_bone.Clear();
    }
}

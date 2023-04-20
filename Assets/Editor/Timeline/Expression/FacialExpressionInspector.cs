using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CustomEditor(typeof(FacialExpression))]
public class FacialExpressionInspector : Editor
{
    public static Dictionary<string, string> m_propertyDict = new Dictionary<string, string>() {
        { "m_idle",         "idle"},
        { "m_a",            "A"},
        { "m_e",            "E"},
        { "m_i",            "I"},
        { "m_o",            "O"},
        { "m_u",            "U"},

        { "m_fennu",        "angry"},
        { "m_danu",         "furious"},
        { "m_daxiao",       "giggle"},
        { "m_kaixin",       "happy"},
        { "m_shangxin",     "low"},
        { "m_xiao",         "laugh"},
        { "m_weixiao",      "smile"},
        { "m_beishang",     "pain"},
        { "m_yansu",        "serious"},
        { "m_jingya",       "surprise"},
        { "m_shengqi",      "angry"},
        { "m_zhayan",       "normal"},
        { "m_yaoyaliechi",  "teether"},
        { "m_haipa",        "afraid"},
        { "m_jinzhang",     "nervous"},
        { "m_nanguo",       "sad"},
    };

    public static Dictionary<FacialClipType, string> m_typeToClipNameDict = new Dictionary<FacialClipType, string>() {
        {FacialClipType.idle,   "idle"  },
        {FacialClipType.A,      "A"     },
        {FacialClipType.E,      "E"     },
        {FacialClipType.I,      "I"     },
        {FacialClipType.O,      "O"     },
        {FacialClipType.U,      "U"     },

        {FacialClipType.eyebrow_sad,        "eyebow_sad"   },
        {FacialClipType.eyebrow_happy,      "eyebow_smile" },
        {FacialClipType.eyebrow_angry,      "eyebow_angry" },

        {FacialClipType.eye_Squint,         "eye_smile"    },
        {FacialClipType.eye_Earnest,        "eye_serious"   },
        {FacialClipType.eye_Stare,          "eye_stare"     },

        {FacialClipType.mouth_smile,        "mouth_smile"   },
        {FacialClipType.mouth_laugh,        "mouth_laugh"   },
        {FacialClipType.mouth_sad,          "mouth_sad"     },
        {FacialClipType.mouth_angry,        "mouth_angry"   },

        {FacialClipType.blink,              "normal"         },

        //{"surprise",    FacialClipType.surprise},
        //{"getangry",    FacialClipType.getangry},
        //{"normal",      FacialClipType.normal},
        //{"teether",     FacialClipType.teether},
        //{"afraid",      FacialClipType.afraid},
        //{"nervous",     FacialClipType.nervous},
        //{"sad",         FacialClipType.sad},
    };
   
    private void OnEnable()
    {
        FacialExpression obj = (FacialExpression)target;
        if (obj != null)
        {
            obj.m_roleDir = obj.gameObject.name;
            obj.m_roleName = obj.gameObject.name;
        }
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        FacialExpression obj = (FacialExpression)target;
        EditorGUILayout.Space();
        obj.activeFlush = EditorGUILayout.Toggle("刷新角色位置大小", obj.activeFlush);
        EditorGUILayout.Space();

        if (obj.m_clips != null)
        {
            for (int i = 0; i < obj.m_clips.Count; ++i)
            {
                FacialAnimationClip clip = obj.m_clips[i];
                int index = (int)clip.m_clipType;
                if (index >= FacialExpression.m_clipNames.Length)
                {
                    obj.m_clips.RemoveAt(i);
                    obj.Init();
                    break;
                }
                EditorGUILayout.BeginHorizontal();
                clip.m_fold = EditorGUILayout.Foldout(clip.m_fold, FacialExpression.m_clipNames[index]);
                if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                {
                    obj.m_clips.RemoveAt(i);
                    obj.Init();
                    break;
                }
                EditorGUILayout.EndHorizontal();
                if (clip.m_fold)
                {
                    clip.m_clipType = (FacialClipType)EditorGUILayout.Popup(new GUIContent("type"), (int)clip.m_clipType, FacialExpression.m_clipNames);
                    clip.m_clip = (AnimationClip)EditorGUILayout.ObjectField("clip", clip.m_clip, typeof(AnimationClip), true);
                }
            }
        }
        obj.m_curve = (FacialExpressionCurve)EditorGUILayout.ObjectField("cuve", obj.m_curve, typeof(FacialExpressionCurve), true);

        if (GUILayout.Button("AddClip"))
        {
            if (obj.m_clips == null) obj.m_clips = new List<FacialAnimationClip>();
            FacialAnimationClip clip = new FacialAnimationClip();
            clip.m_clipType = FacialClipType.idle;
            clip.m_clip = null;
            clip.m_fold = true;
            obj.m_clips.Add(clip);
            obj.Init();
        }

        GUILayout.Space(10);

        obj.m_roleDir = EditorGUILayout.TextField("roleDir", obj.m_roleDir);
        obj.m_roleName = EditorGUILayout.TextField("roleName", obj.m_roleName);
        if (GUILayout.Button("LoadClip"))
        {
            LoadAllClip(obj);
            //FacialExpressionCurve comp = obj.m_curve;
            //if (comp == null) return;
            //GameObject timelineGo = GameObject.Find("timeline");
            //PlayableDirector director = timelineGo.GetComponent<PlayableDirector>();
            //foreach (var pb in director.playableAsset.outputs)
            //{
            //    if (pb.sourceObject is AnimationTrack)
            //    {
            //        var go = director.GetGenericBinding(pb.sourceObject) as GameObject;
            //        Animator animator = null;
            //        if (go != null)
            //        {
            //            animator = go.GetComponent<Animator>();
            //        }
            //        else
            //        {
            //            animator = director.GetGenericBinding(pb.sourceObject) as Animator;
            //        }
            //        if (animator == null) continue;

            //        string roleName = animator.name;
            //        FacialExpressionCurve curve = animator.GetComponent<FacialExpressionCurve>();

            //        TrackAsset trackAsset = pb.sourceObject as TrackAsset;
            //        if (trackAsset == null) continue;
            //        BindAnimation(obj, roleName, "idle");
            //        IEnumerable<TimelineClip> clips = trackAsset.GetClips();
            //        foreach (var item in clips)
            //        {
            //            AnimationClip animClip = item.animationClip;
            //            //Debug.LogError(animClip.name);
            //            EditorCurveBinding[] curveBinding = AnimationUtility.GetCurveBindings(animClip);
            //            for (int i = 0; i < curveBinding.Length; ++i)
            //            {
            //                //Debug.LogError(curveBinding[i].propertyName);
            //                string propName = curveBinding[i].propertyName;
            //                BindAnimation(obj, roleName, propName);
            //            }
            //        }
            //        obj.Init();
            //    }
            //}
        }

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("DisableGraph"))
        {
            obj.DisableGraph();
        }
        if (GUILayout.Button("EnableGraph"))
        {
            obj.EnableGraph();
        }
        GUILayout.EndHorizontal();
    }

    public static void LoadAllClip(FacialExpression obj)
    {
        if (obj.m_clips == null) obj.m_clips = new List<FacialAnimationClip>();
        obj.m_clips.Clear();

        if(string.IsNullOrEmpty(obj.m_roleDir))
            obj.m_roleDir = obj.gameObject.name;
        if (string.IsNullOrEmpty(obj.m_roleName))
            obj.m_roleName = obj.gameObject.name;

        foreach (FacialClipType clipType in Enum.GetValues(typeof(FacialClipType)))
        {
            FacialAnimationClip clip = new FacialAnimationClip();
            clip.m_clipType = clipType;
            clip.m_clip = LoadAnimationClip(clipType, obj.m_roleDir, obj.m_roleName);
            clip.m_fold = true;
            obj.m_clips.Add(clip);
        }
        obj.Init();
    }

    private static AnimationClip LoadAnimationClip(FacialClipType clipType, string m_roleDir, string m_roleName)
    {
        string filePath = @"Assets/BundleRes/Animation/{0}/{1}_facial_{2}.anim";

        string path = string.Format(filePath, m_roleDir, m_roleName, m_typeToClipNameDict[clipType]);
        Debug.Log(path);
        AnimationClip clip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
        return clip;
    }

    //private static string m_animationPath = "Assets/BundleRes/Animation/{0}/{1}.anim";
    //private void BindAnimation(FacialExpression obj, string roleName, string propName)
    //{
    //    //if (m_propertyDict.ContainsKey(propName))
    //    {
    //        //roleName += "_facial";
    //        string clipName = string.Format(roleName + "_{0}", propName/*m_propertyDict[propName]*/);
    //        //Debug.LogError(animationClip);
    //        if (string.IsNullOrEmpty(m_dirName))
    //        {
    //            string[] strs = roleName.Split('_');
    //            if (strs.Length >= 2)
    //            {
    //                m_dirName = strs[0] + '_' + strs[1];
    //            }
    //        }
    //        string path = string.Format(m_animationPath, m_dirName, clipName);
    //        //Debug.LogError(path);
    //        AnimationClip clip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
    //        if(clip == null)
    //        {
    //            Debug.LogError("clip not found path=" + path);
    //        }
    //        BindAnimation(obj, propName, clip);
    //    }
    //}

    //private void BindAnimation(FacialExpression obj, string propName, AnimationClip clip)
    //{
    //    FacialClipType clipType = FacialClipType.idle;
    //    if (m_propertyTypeDict.ContainsKey(propName)) clipType = m_propertyTypeDict[propName];
    //    if (obj.m_clips == null) obj.m_clips = new List<FacialAnimationClip>();

    //    FacialAnimationClip facialAnimationClip = null;
    //    for (int i = 0; i < obj.m_clips.Count; ++i)
    //    {
    //        if (obj.m_clips[i].m_clipType == clipType)
    //        {
    //            facialAnimationClip = obj.m_clips[i];
    //            break;
    //        }
    //    }

    //    if (facialAnimationClip == null)
    //    {
    //        facialAnimationClip = new FacialAnimationClip();
    //        facialAnimationClip.m_clipType = clipType;
    //        obj.m_clips.Add(facialAnimationClip);
    //    }

    //    if (facialAnimationClip.m_clip == null) facialAnimationClip.m_clip = clip;
    //}
}

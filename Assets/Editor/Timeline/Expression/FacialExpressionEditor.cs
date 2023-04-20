using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

public class FacialExpressionEditor : EditorWindow
{
    private static string m_animationName = string.Empty;
    private static string[] m_variables = new string[] { "m_a", "m_e", "m_i", "m_o", "m_u" };

    /// <summary>
    /// 导出Track的AnimationClip
    /// </summary>
    [MenuItem("Tools/Timeline/ExportFacialCurve")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(FacialExpressionEditor), true, "FacialExpressionEditor");
    }

    /// <summary>
    /// 导出Track的AnimationClip
    /// </summary>
    [MenuItem("Tools/Timeline/ConvertMouth")]
    static void ConvertMouthAnimationClip()
    {
        CloneAnimationClip();
    }

    /// <summary>
    /// 导出Track的AnimationClip
    /// </summary>
    [MenuItem("Tools/Timeline/ConvertSelectMouth")]
    static void ConvertSelectMouth()
    {
        CloneSelectAnimationClip();
    }

    private static void CloneAnimationClip()
    {
        DirectoryInfo dir = Directory.CreateDirectory("Assets/Animation");
        FileInfo[] files = dir.GetFiles("*.anim");
        for (int i = 0; i < files.Length; ++i)
        {
            string path = files[i].FullName;
            int index = path.IndexOf("Assets");
            path = path.Substring(index);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
            CloneOneAnimationClip(clip);
        }
        AssetDatabase.Refresh();
    }

    private static void CloneSelectAnimationClip()
    {
        AnimationClip clip = Selection.activeObject as AnimationClip;
        CloneOneAnimationClip(clip);
        AssetDatabase.Refresh();
    }

    private static void CloneOneAnimationClip(AnimationClip source)
    {
        if (source == null) return;
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(source);
        AnimationClip destClip = new AnimationClip();
        destClip.legacy = false;
        for (int i = 0; i < bindings.Length; i++)
        {
            EditorCurveBinding sourceBinding = bindings[i];
            AnimationCurve sourceCurve = AnimationUtility.GetEditorCurve(source, sourceBinding);
            AnimationCurve destCurve = new AnimationCurve();
            Keyframe[] keyframes = sourceCurve.keys;
            for (int j = 0; j < keyframes.Length; j++)
            {
                Keyframe kf = keyframes[j];
                Keyframe keyframe = new Keyframe(kf.time, kf.value * 0.01f, kf.inTangent, kf.outTangent, kf.inWeight, kf.outWeight);
                keyframe.weightedMode = kf.weightedMode;
                destCurve.AddKey(keyframe);
            }
            destClip.SetCurve(string.Empty, typeof(FacialExpressionCurve), m_variables[i], destCurve);
        }
        AssetDatabase.CreateAsset(destClip, "Assets/BundleRes/Animation/FacialMouth/" + source.name + ".anim");
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();       
        EditorGUILayout.Space();
        m_animationName = EditorGUILayout.TextField("AnimationName", m_animationName);
        if (GUILayout.Button("Export", GUILayout.MaxWidth(160)))
        {
            ExportFacialAnimation();
        }
        if (GUILayout.Button("Convert", GUILayout.MaxWidth(160)))
        {
            ConvertMouthAnimation();
        }
        GUILayout.EndVertical();
    }

    private static void ConvertMouthAnimation()
    {
        AnimationClip animationClip = Selection.activeObject as AnimationClip;
        Debug.LogError("name="+animationClip.name);
    }

   
    private static void ExportFacialAnimation()
    {
        AnimationTrack track = Selection.activeObject as AnimationTrack;
        if (track == null)
        {
            Debug.LogError("select track first!");
            return;
        }

        AnimationClip clip = track.infiniteClip;
        if(clip == null)
        {
            Debug.LogError("record first!");
            return;
        }

        if(string.IsNullOrEmpty(m_animationName))
        {
            Debug.LogError("give a name fist!");
            return;
        }

        AnimationClip tempclip = new AnimationClip();
        EditorUtility.CopySerialized(clip, tempclip);

        string path = "Assets/BundleRes/Animation/FacialExpression/" + m_animationName + ".anim";
        if(File.Exists(path))
        {
            Debug.LogError("same file exists in " + path);
            return;
        }
        AssetDatabase.CreateAsset(tempclip, path);
        AssetDatabase.Refresh();
        Selection.activeObject = tempclip;
        Debug.Log(path);
    }
}

using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Timeline;
using UnityEngine;
using XEditor;

public class TimelineSettings : EditorWindow
{
    private static int rate = 30;
    private static bool enableMarker = false;
    private static bool enablePostprocess = false;
    private static bool showAsFrames = true;
    private static bool previewBlend = true;
    private static bool recordShow = true;
    private static bool verboseShow = true;
    private static int solutionSelect = 5;
    private static string recordDir;
    private static int soluX, soluY;
    internal static VideoBitrateMode bitRate;
    private GUIStyle labelStyle = null;

    public enum FrameRate
    {
        Film_24 = 24,
        PAL_25 = 25,
        NTSC_29 = 29,
        Rate_30 = 30,
        Rate_60 = 60
    }

    private static string[] heightToName;

    internal static ImageHeight solution
    {
        get
        {
            if (heightToName != null)
            {
                var dic = ImageHeightSelector.HeightToName;
                string dep = heightToName[solutionSelect];
                foreach (var it in dic)
                {
                    if (it.Value == dep) return it.Key;
                }
            }
            return ImageHeight.Window;
        }
    }

    const string PREF_FRATE = "PREF_FRATE";
    const string PREF_MARKER = "PREF_MARKER";
    const string PREF_POST = "PREF_POST";
    const string PREF_SHOWAS = "PREF_SHOWAS";
    const string PREF_PREV = "PREF_PREV";
    const string PREF_RECD = "PREF_RECD";
    const string PREF_SOLUTION = "PREF_SOLUTION";
    const string PREF_SOLUX = "PREF_SOLUX";
    const string PREF_SOLUY = "PREF_SOLUY";
    const string PREF_BRATE = "PREF_BITRATE";

    private void OnEnable()
    {
        var dic = ImageHeightSelector.HeightToName;
        heightToName = new string[dic.Count];
        int i = 0;
        foreach (var it in dic)
        {
            heightToName[i] = it.Value;
            i++;
        }
        recordShow = RecorderOptions.ShowRecorderGameObject;
        verboseShow = RecorderOptions.VerboseMode;
        ApplySetting();
    }

    private void OnFocus()
    {
        OnEnable();
    }

    private void SetupStyle()
    {
        if (labelStyle == null)
        {
            if (labelStyle == null)
                labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 18;
        }
    }

    public void OnGUI()
    {
        SetupStyle();
        EditorGUILayout.BeginVertical();

        GUILayout.Space(24);
        GUILayout.Label(" Timeline 窗口设置", XEditorUtil.titleLableStyle);
        GUILayout.Space(12);

        EditorGUI.BeginChangeCheck();

        enableMarker = EditorGUILayout.Toggle("显示Marker", enableMarker);
        enablePostprocess = EditorGUILayout.Toggle("显示后处理", enablePostprocess);
        showAsFrames = EditorGUILayout.Toggle("帧(默认)/秒", showAsFrames);
        previewBlend = EditorGUILayout.Toggle("虚拟相机结束Blend", previewBlend);
        EditorGUILayout.Space();

        FrameRate rt = (FrameRate)EditorGUILayout.EnumPopup("帧率", (FrameRate)rate);
        rate = (int)rt;

        GUILayout.Space(12);
        EditorGUILayout.LabelField("视频录制", labelStyle);
        EditorGUILayout.Space();
        verboseShow = EditorGUILayout.Toggle("日志输出", verboseShow);
        recordShow = EditorGUILayout.Toggle("显示Session", recordShow);
        solutionSelect = EditorGUILayout.Popup("视频分辨率", solutionSelect, heightToName);
        if (solutionSelect == 6) // ImageHeight.Custom
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            soluX = EditorGUILayout.IntField("W", soluX);
            soluY = EditorGUILayout.IntField("H", soluY);
            EditorGUILayout.EndHorizontal();
        }

        bitRate = (VideoBitrateMode)EditorGUILayout.EnumPopup("编码质量", bitRate);
        if (EditorGUI.EndChangeCheck())
        {
            SaveSetting();
            ApplySetting();
        }

        GUILayout.Space(16);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("视频导出目录", GUILayout.MaxWidth(140)))
        {
            recordDir = EditorUtility.OpenFolderPanel("record save path", recordDir, recordDir);
            EditorPrefs.SetString(PREF_RECD, recordDir);
            RecorderSettings.OutputDir = recordDir;
        }
        GUILayout.Space(4);
        EditorGUILayout.LabelField(recordDir);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("恢复默认设置", GUILayout.MaxWidth(140)))
        {
            EditorPrefs.DeleteAll();
            ApplySetting();
            ShowNotification(new GUIContent("Delete All"));
        }

        EditorGUILayout.EndVertical();
        OriginalSetting.DrawLogo(this);
    }

    public void SaveSetting()
    {
        EditorPrefs.SetInt(PREF_FRATE, rate);
        EditorPrefs.SetBool(PREF_MARKER, enableMarker);
        EditorPrefs.SetBool(PREF_POST, enablePostprocess);
        EditorPrefs.SetBool(PREF_SHOWAS, showAsFrames);
        EditorPrefs.SetBool(PREF_PREV, previewBlend);
        EditorPrefs.SetInt(PREF_SOLUTION, solutionSelect);
        EditorPrefs.SetInt(PREF_SOLUX, soluX);
        EditorPrefs.SetInt(PREF_SOLUY, soluY);
        EditorPrefs.SetInt(PREF_BRATE, (int)bitRate);
    }

    public static void ApplySetting()
    {
        rate = EditorPrefs.GetInt(PREF_FRATE, 30);
        enableMarker = EditorPrefs.GetBool(PREF_MARKER, false);
        enablePostprocess = EditorPrefs.GetBool(PREF_POST, false);
        showAsFrames = EditorPrefs.GetBool(PREF_SHOWAS, true);
        previewBlend = EditorPrefs.GetBool(PREF_PREV, false);
        recordDir = EditorPrefs.GetString(PREF_RECD, Application.dataPath);
        solutionSelect = EditorPrefs.GetInt(PREF_SOLUTION, 5);
        soluX = EditorPrefs.GetInt(PREF_SOLUX, 1024);
        soluY = EditorPrefs.GetInt(PREF_SOLUY, 1024);
        bitRate = (VideoBitrateMode)EditorPrefs.GetInt(PREF_BRATE, (int)VideoBitrateMode.High);
        EditorStateHelper.ApplySetting(rate, enableMarker, enablePostprocess, showAsFrames);
        EditorStateHelper.PreviewEndCameraBlend(previewBlend, GetEndDuration);
        RecorderOptions.ShowRecorderGameObject = recordShow;
        RecorderOptions.VerboseMode = verboseShow;
        RecorderSettings.OutputDir = recordDir;
    }


    private static OrignalTimelineData orignalData;
    private static float GetEndDuration
    {
        get
        {
            if (orignalData == null)
                orignalData = GameObject.FindObjectOfType<OrignalTimelineData>();
            return orignalData?.blendOutTime ?? 0;
        }
    }

}

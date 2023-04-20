using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XUpdater;


[ExecuteInEditMode]
internal sealed class PatchPresent : EditorWindow
{

    [MenuItem(@"Tools//Patch/Present")]
    static void Execute()
    {
        GetWindowWithRect<PatchPresent>(new Rect(0, 0, 1360, 620), true, @"Patch Present");
    }

    private bool enabled = false, rebuild = false;
    private Vector2 scrollPos;
    private GUIStyle style1, style2;
    private bool bundle_foldOut, native_foldout;


    void OnEnable()
    {
        enabled = XBundleTools.singleton.OnInit();
        if (enabled)
        {
            native_foldout = true;
        }
    }

    void Update()
    {
        if (!enabled) Close();
    }

    void OnGUI()
    {
        InitStyle();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        EditorGUILayout.Space();
        string platform = "Target Platform: ";
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case UnityEditor.BuildTarget.Android: platform += "Android"; break;
            case UnityEditor.BuildTarget.iOS: platform += "IOS"; break;
        }
        EditorGUILayout.LabelField(platform, style1, GUILayout.Height(25));
        EditorGUILayout.BeginHorizontal();
        Color r = GUI.color;
        GuiVersion("Version: ", XBundleTools.singleton.CurrentVersion, 60);
        GuiVersion("Next Version: ", XBundleTools.singleton.NextVersion, 90);
        rebuild = GUILayout.Toggle(rebuild, " Rebuild");
        if (rebuild != XBundleTools.singleton.Rebuild)
        {
            XBundleTools.singleton.Rebuild = rebuild;
            XBundleTools.singleton.FetchNewlyUpdate();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(400));
        GUILayout.Label("Location", style2, GUILayout.Width(500));
        GUILayout.Label("Status", style2, GUILayout.Width(200));
        GUILayout.Label("Size", style2, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        bundle_foldOut = EditorGUILayout.Foldout(bundle_foldOut, "Detailed BundleRes");
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(TotalString(XBundleTools.singleton.BundleUpdateFiles));
        EditorGUILayout.EndHorizontal();
        if (bundle_foldOut) GUIList(XBundleTools.singleton.BundleUpdateFiles);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        native_foldout = EditorGUILayout.Foldout(native_foldout, "Detailed dll fmod Lua");
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(TotalString(XBundleTools.singleton.NativeUpdateFiles));
        EditorGUILayout.EndHorizontal();
        if (native_foldout) GUIList(XBundleTools.singleton.NativeUpdateFiles);


        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(400));
        if (GUILayout.Button("Build", GUILayout.Width(150)))
        {
            string tip = "Build version to v" + XBundleTools.singleton.NextVersion + " ?";
            if (EditorUtility.DisplayDialog("Notice", tip, "OK", "Cancel"))
            {
                BuildBundle();
            }
        }
        GUILayout.Label("", GUILayout.Width(120));
        if (GUILayout.Button("Push", GUILayout.Width(150)))
        {
            string tip = "Confirm your push: next version is v" + XBundleTools.singleton.NextVersion;
            if (EditorUtility.DisplayDialog("Notice", tip, "OK", "Cancel"))
            {
                PushBundle();
            }
            GUIUtility.ExitGUI();
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.EndScrollView();
    }


    void BuildBundle()
    {
        var v = XBundleTools.singleton.NextVersion;
        XBundleTools.singleton.CleanEnv(v);
        if (XBundleTools.singleton.Rebuild)
            RebuildPatch(v);
        else
            BuildPatch();
        GUIUtility.ExitGUI();
    }

    static void PushBundle()
    {
        string next = XBundleTools.singleton.NextVersion;
        XBundleTools.singleton.UpdateVersion(next);
        XGitExtractor.Push(next);
        XGitExtractor.TagSrc(next);
        XBundleTools.singleton.FetchNewlyUpdate();
    }

    void GuiVersion(string title, XVersionData data, int width)
    {
        GUILayout.Label(title, style2, GUILayout.Width(width));
        Color r = GUI.color;
        GUI.color = new Color(255, 0, 0);
        GUILayout.Label(data, style2, GUILayout.Width(90));
        GUI.color = r;
    }

    string TotalString(List<XBundleTools.UpdatedFile> list)
    {
        string str = "total assets count:" + list.Count;
        str += ", size:";
        long size = 0;
        foreach (var it in list) size += it.size;
        str += " " + XBundleTools.UpdatedFile.GetCapacityValue(size);
        return str;
    }

    void GUIList(List<XBundleTools.UpdatedFile> list)
    {
        EditorGUILayout.BeginVertical(EditorStyles.textField);
        int i = 0;
        foreach (var m in list)
        {
            GuiItem(m, ++i);
        }
        EditorGUILayout.EndVertical();
    }

    void GuiItem(XBundleTools.UpdatedFile m, int i)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(i + ": " + m.name, GUILayout.Width(400));
        GUILayout.Label(m.location, GUILayout.Width(500));
        GUILayout.Label(m.status.ToString(), GUILayout.Width(200));
        GUILayout.Label(m.Size, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();
    }

    void RebuildPatch(XVersionData v)
    {
        CFEngine.Editor.BuildBundle.BuildAllAssetBundlesWithList();
        XBundleTools.singleton.CreateNewVersion(v);
        v++;
        XBundleTools.singleton.CreateNewVersion(v);
        XBundleTools.singleton.BuildExternal(v);
        AssetDatabase.Refresh();
    }

    public static void BuildPatch()
    {
        // bundleres & dll, fmod, lua, bytes..
        if (XBundleTools.singleton.BundleUpdateFiles.Count > 0)
        {
            XBundleTools.BuildAllAssetBundlesWithList();
        }
        var next = XBundleTools.singleton.NextVersion;
        XBundleTools.singleton.GenerateVersion(next);
        AssetDatabase.Refresh();
    }

    // php shell interface, don't rename 
    [MenuItem("Tools/Build/Php-Build", priority = 2)]
    public static void BuildHotPatch()
    {
        if (XBundleTools.singleton.OnInit())
        {
            BuildPatch();
        }
    }

    [MenuItem("Tools/Build/Php-Push", priority = 2)]
    public static void ShellPushBundle()
    {
        if (XBundleTools.singleton.OnInit())
        {
            PushBundle();
        }
    }


    void InitStyle()
    {
        if (style1 == null)
        {
            style1 = new GUIStyle(EditorStyles.boldLabel);
            style1.fontStyle = FontStyle.Bold;
            style1.fontSize = 13;
        }
        if (style2 == null)
        {
            style2 = new GUIStyle(EditorStyles.boldLabel);
            style2.fontStyle = FontStyle.Bold;
            style2.fontSize = 11;
        }
    }
}
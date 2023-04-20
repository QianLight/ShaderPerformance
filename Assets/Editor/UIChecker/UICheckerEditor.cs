using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.CFUI;
using CFEngine;
using System.IO;
using System.Windows;

class AtlasUseInfo
{
    public string atlasName;
    public int count;

    public AtlasUseInfo(string atlas, int c)
    {
        atlasName = atlas;
        count = c;
    }
}

public class UICheckerEditor : EditorWindow
{
    static Dictionary<string, List<AtlasUseInfo> > TotalAtlasUseInfo = new Dictionary<string, List<AtlasUseInfo> >();

    static Dictionary<string, bool> EditorPrefabInfo = new Dictionary<string, bool>(); // prefabname -> bExpand

    public Vector2 scrollPosition;

    public static UICheckerEditor Instance { get; set; }

    [MenuItem("XEditor/UIChecker")]
    public static void InitEmpty()
    {
        var window = (UICheckerEditor)GetWindow(typeof(UICheckerEditor));
        window.titleContent = new GUIContent("UI Checker");
        window.wantsMouseMove = true;
        window.Show();
        window.Repaint();
        Instance = window;
    }

    public void OnEnable()
    {

    }

    public void OnDisable()
    {
       
    }

    public void OnGUI()
    {
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("CheckAll",  GUILayout.MaxWidth(235)))
        {
            CheckAllUIPrefab();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(20);

        ShowTotalAtlasUseInfo();
    }

    private void CheckAllUIPrefab()
    {
        string rootpath = Application.dataPath + "/BundleRes/UI/OPsystemprefab/";

        TotalAtlasUseInfo.Clear();
        EditorPrefabInfo.Clear();
        _CheckPrefabForSingleFolder(rootpath);
    }

    private void _CheckPrefabForSingleFolder(string folder)
    {
        DirectoryInfo direction = new DirectoryInfo(folder);
        FileSystemInfo[] fs = direction.GetFileSystemInfos();

        for (int i = 0; i < fs.Length; i++)
        {
            if (fs[i].Name.EndsWith(".meta")) continue;

            if (fs[i] is DirectoryInfo)
            {
                _CheckPrefabForSingleFolder(fs[i].FullName);
            }
            else
            {
                if (fs[i].FullName.EndsWith(".meta")) continue;
                string name = fs[i].Name.ToLower();
                string fullName = fs[i].FullName.Replace('\\', '/');
                int index = fullName.IndexOf("Assets/");
                string path = fullName.Substring(index);

                List<AtlasUseInfo> PrefabUseAtlasInfo = _CheckSingleUIPrefab(path);

                string PrefabName = fs[i].Name;

                TotalAtlasUseInfo.Add(PrefabName, PrefabUseAtlasInfo);
                EditorPrefabInfo.Add(PrefabName, false);
            }
        }
    }

    

    private void ShowTotalAtlasUseInfo()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width));
        
        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Prefab", GUILayout.Width(235));
        //GUILayout.Label("Use Atlas", GUILayout.Width(100));
        //GUILayout.EndHorizontal();

        GUILayout.Box("",new GUILayoutOption[] { GUILayout.Width(position.width), GUILayout.Height(2) });
        

        foreach (var pair in TotalAtlasUseInfo)
        {
             GUI.color = Color.white;
            int useAtlas = pair.Value.Count;
            if (useAtlas >= 5) GUI.color = Color.red;

            string prefabName = pair.Key;
            int dotPos = prefabName.LastIndexOf('.');
            string prefabNameNoExt = prefabName.Substring(0, dotPos);
            string atlasTitle = pair.Key + "(" + pair.Value.Count + ")";
            EditorPrefabInfo[prefabName] = EditorGUILayout.Foldout(EditorPrefabInfo[prefabName], atlasTitle);
            if(EditorPrefabInfo[prefabName])
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                if (GUILayout.Button("CopyName", GUILayout.Width(80)))
                {
                    EditorGUIUtility.systemCopyBuffer = prefabNameNoExt;
                }
                GUILayout.EndHorizontal();

                for (int i = 0; i < useAtlas; ++i)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label(pair.Value[i].atlasName, GUILayout.Width(235));
                    GUILayout.Label(pair.Value[i].count.ToString(), GUILayout.Width(30));
                    GUILayout.EndHorizontal();
                }
                
            }

        }
        GUILayout.EndScrollView();
    }

    [MenuItem("Assets/UI/CheckUI")]
    public static void CheckSingleUIPrefab()
    {
        var select = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
        if (select.Length > 0)
        {
            TotalAtlasUseInfo.Clear();
            EditorPrefabInfo.Clear();
            //atlasUseCondition.Clear();
            foreach (var item in select)
            {
                var path = AssetDatabase.GetAssetPath(item);
                if (!path.Contains("BundleRes/UI")) continue;
                if (!path.EndsWith(".prefab")) continue;

                List<AtlasUseInfo> PrefabUseAtlasInfo =  _CheckSingleUIPrefab(path);
                //Debug.Log(item.name + "use " + PrefabUseAtlasInfo.Count + " atlas");
            }
        }
    }

    private static List<AtlasUseInfo> _CheckSingleUIPrefab(string path)
    {
        List<AtlasUseInfo> PrefabUseAtlasInfo = new List<AtlasUseInfo>();
        GameObject item = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        CFImage[] images = item.GetComponentsInChildren<CFImage>();
        for (int i = 0; i < images.Length; ++i)
        {
            AddAtlasUseCount(PrefabUseAtlasInfo, images[i].m_AtlasName);
        }

        return PrefabUseAtlasInfo;
    }

    private static void AddAtlasUseCount(List<AtlasUseInfo> container, string atlasName)
    {
        if (string.IsNullOrEmpty(atlasName)) return;

        for(int i = 0; i < container.Count; ++i)
        {
            if(container[i].atlasName == atlasName)
            {
                container[i].count++;
                return;
            }
        }

        container.Add(new AtlasUseInfo(atlasName, 1));
    }

}

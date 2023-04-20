using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.CFUI;
using CFUtilPoolLib;


public class UIAniamtionChange : EditorWindow {

    string target_path = "";
    
    //string[] options = new string[]{ "Button", "Toggle"};
    //int select_index = 0;

    //string target_audio = "";
    string message = "";
    [MenuItem("Tools/UI/Animation Change")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<UIAniamtionChange>();
    }

    private void OnGUI()
    {
        message = string.Empty;
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Target:",GUILayout.Width(60));
        
        target_path = EditorGUILayout.TextField(target_path);
        if (GUILayout.Button("Browse")){
            string path = EditorUtility.OpenFolderPanel("Select UI Prefab Folder!",Application.dataPath,"");
            string need = "Assets/";
            int index = path.IndexOf(need);
            if(index >= 0)
            {
                target_path = path.Substring(index + need.Length);
            }
            else
            {
                message = "无效的路径!";
            }      
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();


        bool disable =  string.IsNullOrEmpty(target_path);
        EditorGUI.BeginDisabledGroup(disable);
        if (GUILayout.Button("Apply"))
        {
            FlushAll();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
        if (disable || string.IsNullOrEmpty(message))
        {
            message = "此操作为批量修改，请设置UI文件夹路径!";
        }
        if (!string.IsNullOrEmpty(message))
        {
            EditorGUILayout.HelpBox(message,MessageType.Warning,true);
        }
        
        EditorGUILayout.EndVertical();
    }

    private void FlushAll()
    {
        string genPath = Application.dataPath + "/"+ target_path;
        string[] files = Directory.GetFiles(genPath, "*.prefab",SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = files[i].Substring(files[i].IndexOf("Assets"));
            GameObject _prefab = AssetDatabase.LoadAssetAtPath(files[i], typeof(GameObject)) as GameObject;
            if (_prefab == null) continue;
            int flag = 0;
            ChangePrefab(_prefab.transform, ref flag);
            if(flag > 0){
                EditorUtility.SetDirty(_prefab);
                Debug.Log(string.Format("Change Prefab Success! {0},  anim count:{1}", _prefab.name,flag));
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Change Success!");
    }



    [MenuItem("Assets/UI/AnimCheck")]
    public static void CheckSingleUIPrefab()
    {
        var select = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
        if (select.Length > 0)
        {
            foreach (var item in select)
            {
                GameObject _prefab = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(item), typeof(GameObject)) as GameObject;
                if (_prefab == null) continue;
                int flag = 0;
                ChangePrefab(_prefab.transform, ref flag);
                if (flag > 0)
                {
                    EditorUtility.SetDirty(_prefab);
                    Debug.Log(string.Format("Change Prefab Success! {0},  anim count:{1}", _prefab.name, flag));
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Change Success!");
        }
    }
    
      private static string m_prefab_url = "BundleRes/UI/OPsystemprefab";
    [MenuItem("Assets/UI/Chang Source Flag")]
    public static void ChangeSourceFlag()
    {
            string[] files = Directory.GetFiles(Application.dataPath + "/" + m_prefab_url, "*.prefab", SearchOption.AllDirectories);
            if (files == null || files.Length == 0) return;
            for (int i = 0; i < files.Length; i++)
            {

                 string target = files[i].Substring(files[i].IndexOf("Assets"));
                GameObject _prefab = AssetDatabase.LoadAssetAtPath(target, typeof(GameObject)) as GameObject;
                if (_prefab == null) continue;
                int flag = 0;
                ChangePrefabForce(_prefab.transform);
                     EditorUtility.SetDirty(_prefab);
                if (flag > 0)
                {
                  
                    Debug.Log(string.Format("Change Prefab Success! {0},  anim count:{1}", _prefab.name, flag));
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Change Success!");
    }

    private static void ChangePrefabForce(Transform trans)
    {
        CFRawImage[] raws = trans.GetComponentsInChildren<CFRawImage>(true);
        for(int i =0; i < raws.Length;i++){
            raws[i].m_StaticForce = true;
        }
     
    }


    private static void ChangePrefab(Transform trans,ref int changeStatus)
    {
        ChangeAnimation(trans.gameObject, ref changeStatus);
        if (trans.childCount == 0) return;
        for(int i = 0; i < trans.childCount;i++)
        {
            Transform child = trans.GetChild(i);
            ChangePrefab(child,ref changeStatus);
        }
    }


    private static void ChangeAnimation( GameObject prefab,ref int changeStatus){

        CFAnimation anim = prefab.GetComponent<CFAnimation>();
        if (anim == null) return ;

        XAnimation xanim = prefab.GetComponent<XAnimation>();
        if (xanim == null) xanim = prefab.AddComponent<XAnimation>();

        CopyAnimation(anim, xanim);

        DestroyImmediate(anim,true);
        changeStatus++;
    }


    private static void CopyAnimation( CFAnimation ca, XAnimation xa )
    {
        switch (ca.PlayType)
        {
            case CFAnimPlayType.OnEnable:
                xa.PlayType = XAnimPlayType.Start;
                break;
            case CFAnimPlayType.Custom:
                xa.PlayType = XAnimPlayType.Custom;
                break;
            case CFAnimPlayType.Once:
                xa.PlayType = XAnimPlayType.Reset;
                break;
        }

        xa.IgnoreTimeScale = ca.IgnoreTimeScale;
        xa.maskUpdate = ca.maskUpdate;
        for(int i = 0;i < ca.AnimList.Count; i++)
        {
            XAnimUnit xu = new XAnimUnit();
            CopyAnimUnit(ca.AnimList[i], xu);
            xa.AnimList.Add(xu);
        }
    }

    private static void CopyAnimUnit(CFAnimUnit ca , XAnimUnit xu)
    {
        xu.Target = ca.Target;
        xu.m_Curve = ca.m_Curve;
        
        xu.m_Type = (XAnimType)XFastEnumIntEqualityComparer<CFAnimType> .ToInt(ca.m_Type);
        xu.m_LoopType = (XAnimLoopType)XFastEnumIntEqualityComparer<CFAnimLoopType>.ToInt(ca.m_LoopType);
        xu.m_WhenFinished = (XAnimPlayFinish)XFastEnumIntEqualityComparer<CFAnimPlayFinish>.ToInt(ca.m_WhenFinished);
        xu.m_From = ca.m_From;
        xu.m_To = ca.m_To;
        xu.m_From_C = ca.m_From_C;
        xu.m_To_C = ca.m_To_C;
        xu.m_Duration = ca.m_Duration;
        xu.m_Delay = ca.m_Delay;
        xu.IncludeChildren = ca.IncludeChildren;
        xu.DoNotInitStartValue = ca.DoNotInitStartValue;
    }
}

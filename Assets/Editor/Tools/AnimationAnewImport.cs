using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CFEngine.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AssetGraph.DataModel.Version2;
using UnityEngine.Profiling;
using Object = System.Object;
public class AnimationAnewImport : EditorWindow
{
    private enum CompressionMode
    {
        KeyframeReduction,
        Optimal
    }
    enum AinType
    {
        角色普通动画,
        剧情动画
    }
    private static string suff = "FBX";
    private static string Animation_folderName = "Animation";
    private static string Aniamtion_folderName = "Aniamtion";
    private static string CutScene_folderName = "CutScene";
    private const string ainmationPath = "Assets/Creatures";
    
    private List<string> AllPathList,OFFPathList,KeyframePathList,OptimalPathList;
    private int AllCount, OFFCount, KeyframeCount,OptimalCount;
    private bool isCollect,isRefresh,isOFF,isKeyframeReduction,isDebugOFF,isOptimalRTS,isGetMemorySize,isGetFileSize,isGetBlobSize,isFlaot3;
    private CompressionMode _compressionMode;
    private float rotationError = 0.2f;
    private float positionError = 0.2f;
    private float scaleError = 0.4f;
    
    public List<AnimationClip> AnimationClipList;
    private long OFFMemorySize,KeyframeMemorySize,OptimalMemorySize;
    private long OFFFileSize,KeyframeFileSize,OptimalFileSize;
    private long OFFBlobSize,KeyframeBlobSize,OptimalBlobSize;
    private bool isTest;
    public AnimationClip m_clip;
    
    // TODO:
    private string[] ainPaths;
    private List<GameObject> AinGameObjectList;
    private AinType _ainType;
    
    private static AnimationAnewImport window;
    [MenuItem("Tools/批量工具/动画批量重导出工具")] 
    private static void ShowWindow()
    {
        window = EditorWindow.GetWindow<AnimationAnewImport>("动画批量重导出工具");
        window.maxSize = new Vector2(550, 1000);
        window.Show();
    }
    private void GetFiles()
    {
        GetPath();
        if (isOFF&&OFFPathList.Count>0)
        {
            ExportAinmationClip(OFFPathList);
        }
        if (isKeyframeReduction&&KeyframePathList.Count>0)
        {
            ExportAinmationClip(KeyframePathList);
        }
        if (isOptimalRTS&&OptimalPathList.Count>0)
        {
            ExportAinmationClip(OptimalPathList);
        }

        AllPathList.Clear();
        OFFPathList.Clear();
        KeyframePathList.Clear();
        OptimalPathList.Clear();
    }
    private void RefreshAnimation()
    {
        GetPath();
        if (isGetFileSize)
        {
            GetFileSize(OFFPathList,ref OFFFileSize);
            GetFileSize(KeyframePathList,ref KeyframeFileSize);
            GetFileSize(OptimalPathList,ref OptimalFileSize);
        }
        if (isGetMemorySize)
        {
            GetMemorySize(OFFPathList,ref OFFMemorySize);
            GetMemorySize(KeyframePathList,ref KeyframeMemorySize);
            GetMemorySize(OptimalPathList,ref OptimalMemorySize);
        }
        if (isGetBlobSize)
        {
            
        }

        AllPathList.Clear();
        OFFPathList.Clear();
        KeyframePathList.Clear();
        OptimalPathList.Clear();
    }
    private void ExportAinmationClip(List<string> path)
    {
        int nub = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            //GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path[i]);
            GameObject fbx = null;
            ModelImporter modelImporter = AssetImporter.GetAtPath(path[i]) as ModelImporter;
            if (modelImporter != null)
            {
                FBXAssets.Fbx_ExportAnimFuc(fbx,modelImporter,path[i].Replace('\\','/'),rotationError,positionError,scaleError,isFlaot3);
            }
            EditorUtility.DisplayCancelableProgressBar("设置剔除压缩", "设置"+path[i], (float)i/nub);
        }
        EditorUtility.ClearProgressBar();
    }
    
    /// <summary>
    /// 获取MemorySize
    /// </summary>
    /// <param name="path"></param>
    private void GetMemorySize(List<string> path,ref long ShowMemorySize)
    {
        Debug.Log(path.Count);
        AnimationClipList = new List<AnimationClip>();
        for (int i = 0; i < path.Count; i++)
        {
            UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(path[i]);
            foreach (UnityEngine.Object o in allObjects)
            {
                AnimationClip oClip = o as AnimationClip;
                if(oClip!=null) AnimationClipList.Add(oClip);
            }
        }
        long MemorySize = 0;
        for (int i = 0; i < AnimationClipList.Count; i++)
        {
            MemorySize += Profiler.GetRuntimeMemorySize(AnimationClipList[i]);
        }
        ShowMemorySize = MemorySize;
        
        AnimationClipList.Clear();
    }
    private void GetFileSize(List<string> pathList, ref long ShowFileSize)
    {
        long FileSize = 0;
        foreach (string Path in pathList)
        {
            long fileInfo = new System.IO.FileInfo(Path).Length;
            FileSize += fileInfo;
        }
        ShowFileSize = FileSize;
    }
    private void GetFileSize(AnimationClip animationClip, ref long FileSize)
    {
        
    }
    private void GetBlobSize(List<AnimationClip> animationClipList, ref long ShowBlobSize)
    {
        ShowBlobSize = 0;
        foreach (AnimationClip clip in animationClipList)
        {
            long blobSize;
            GetBlobSize(clip,ref ShowBlobSize);
        }
    }
    private void GetBlobSize(AnimationClip animationClip, ref long ShowBlobSize)
    {
        Assembly asm = Assembly.GetAssembly(typeof(Editor));
        MethodInfo getAnimationClipStats = typeof(AnimationUtility).GetMethod("GetAnimationClipStats", BindingFlags.Static | BindingFlags.NonPublic);
        Type aniclipstats = asm.GetType("UnityEditor.AnimationClipStats");
        FieldInfo sizeInfo = aniclipstats.GetField ("size", BindingFlags.Public | BindingFlags.Instance);
        object stats = getAnimationClipStats.Invoke(null, new object[]{animationClip});
        //object obj = EditorUtility.FormatBytes((int)sizeInfo.GetValue(stats));
        Debug.Log((int)sizeInfo.GetValue(stats));
        ShowBlobSize += (int) sizeInfo.GetValue(stats);
    }
    public string DirectoryInfoPath(string str)
    {
        int c = str.LastIndexOf(@"Assets\");
        str = str.Substring(c, str.Length - str.LastIndexOf(@"Assets\"));
        return str;
    }
    void GetPath()
    {
        AllPathList = new List<string>();
        OFFPathList = new List<string>();
        KeyframePathList = new List<string>();
        OptimalPathList = new List<string>();
        
        DirectoryInfo direction = new DirectoryInfo(ainmationPath);
        DirectoryInfo[] mDirectoryInfos = direction.GetDirectories();
        if (_ainType == AinType.角色普通动画)
        {
            foreach (var d in mDirectoryInfos)
            {
                DirectoryInfo[] mmDirectoryInfos = d.GetDirectories();
                foreach (var dd in mmDirectoryInfos)
                {
                    if (dd.Name == "Animation"||dd.Name == "Aniamtion")
                    {
                        FileInfo[] files = dd.GetFiles("*");
                        foreach (FileInfo f in files)
                        {
                            if (f.Name.EndsWith(".meta"))
                                continue;
                            if (f.Name.EndsWith(suff))
                            {
                                string ainPath = DirectoryInfoPath(f.FullName);
                                AllPathList.Add(ainPath);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            foreach (var d in mDirectoryInfos)
            {
                DirectoryInfo[] mmDirectoryInfos = d.GetDirectories();
                foreach (var dd in mmDirectoryInfos)
                {
                    if (dd.Name == "CutScene")
                    {
                        FileInfo[] files = dd.GetFiles("*");
                        foreach (var f in files)
                        {
                            if (f.Name.EndsWith(".meta"))
                                continue;
                            if (f.Name.EndsWith(suff))
                            {
                                string ainPath = DirectoryInfoPath(f.FullName);
                                AllPathList.Add(ainPath);
                            }
                        }
                    }
                }
            }
        }
        for (int i = 0; i < AllPathList.Count; i++)
        {
            ModelImporter modelImporter = AssetImporter.GetAtPath(AllPathList[i]) as ModelImporter;
            if (modelImporter.animationCompression == ModelImporterAnimationCompression.Off&&modelImporter.importAnimation)
            {
                OFFPathList.Add(AllPathList[i]);
            }
            else if(modelImporter.animationCompression == ModelImporterAnimationCompression.KeyframeReduction&&modelImporter.importAnimation)
            {
                KeyframePathList.Add(AllPathList[i]);
            }
            else
            {
                OptimalPathList.Add(AllPathList[i]);
            }
            EditorUtility.DisplayCancelableProgressBar("OFF and KR", "设置"+AllPathList[i], (float)i/AllPathList.Count);
        }
        EditorUtility.ClearProgressBar();
        AllCount = AllPathList.Count;
        OFFCount = OFFPathList.Count;
        KeyframeCount = KeyframePathList.Count;
        OptimalCount = OptimalPathList.Count;
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal("box");
        {
            EditorGUILayout.LabelField("文件夹目录:", EditorStyles.boldLabel);
            _ainType = (AinType)EditorGUILayout.EnumPopup(_ainType);
            EditorGUILayout.LabelField(ainmationPath);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal("box");
        {
            EditorGUILayout.LabelField("压缩OFF选项的文件:");
            isOFF = EditorGUILayout.Toggle(isOFF);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal("box");
        {
            EditorGUILayout.LabelField("压缩KeyframeReduction选项的文件:");
            isKeyframeReduction = EditorGUILayout.Toggle(isKeyframeReduction);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal("box");
        {
            EditorGUILayout.LabelField("重置OptimalRTS精度:");
            isOptimalRTS = EditorGUILayout.Toggle(isOptimalRTS);
        }
        EditorGUILayout.EndHorizontal();
        
        if (isOFF||isKeyframeReduction)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("选择压缩模式:");
                if (isOFF&&isKeyframeReduction||isKeyframeReduction)
                {
                    EditorGUILayout.EnumPopup(CompressionMode.Optimal);
                }
                else
                {
                    _compressionMode = (CompressionMode) EditorGUILayout.EnumPopup(_compressionMode);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("是否进行Float精度压缩");
                isFlaot3 = EditorGUILayout.Toggle(isFlaot3);
            }
            EditorGUILayout.EndHorizontal();
        }
        if (isOFF || isKeyframeReduction || isOptimalRTS)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("RTS精确度:");
                rotationError = EditorGUILayout.FloatField(rotationError);
                positionError = EditorGUILayout.FloatField(positionError);
                scaleError = EditorGUILayout.FloatField(scaleError);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        isRefresh = GUILayout.Button("刷新数据");
        if (isRefresh) RefreshAnimation();
        isCollect = GUILayout.Button("一键重导动画-压缩");
        if (isCollect)
        {
            if (isOFF == false && isKeyframeReduction == false && isOptimalRTS==false)
            {
                Debug.LogError("请选择优化项！！！");
            }
            else
            {
                if (EditorUtility.DisplayDialog("重新导出动画", "确认导出", "确认", "取消"))
                {
                    GetFiles();
                }
            }
        }
        
        EditorGUILayout.LabelField("数量", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("AnimationClip(.FBX): "+AllCount);
        EditorGUILayout.LabelField("AnimationClipOFF(.FBX): "+OFFCount);
        EditorGUILayout.LabelField("AnimationClipKeyframeReduction(.FBX): "+KeyframeCount);
        EditorGUILayout.LabelField("AnimationClipOptimal(.FBX): "+OptimalCount);
        
        EditorGUILayout.LabelField("内存", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("GetFileSize:");
            isGetFileSize = EditorGUILayout.Toggle(isGetFileSize);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("GetMemorySize:");
            isGetMemorySize = EditorGUILayout.Toggle(isGetMemorySize);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("GetBlobSize:");
            isGetBlobSize = EditorGUILayout.Toggle(isGetBlobSize);
        }
        EditorGUILayout.EndHorizontal();
        m_clip = (AnimationClip)EditorGUILayout.ObjectField("clip", m_clip, typeof(AnimationClip), true);
        isTest = GUILayout.Button("Test");
        if (isTest&&m_clip)
        {
            OFFBlobSize = 0;
            GetBlobSize(m_clip,ref OFFBlobSize);
        }
        else
        {
            EditorGUILayout.LabelField("请选择文件!!!");
        }

        
        EditorGUILayout.LabelField("OFFMemorySize: "+EditorUtility.FormatBytes(OFFMemorySize));
        EditorGUILayout.LabelField("KeyframeMemorySize: "+EditorUtility.FormatBytes(KeyframeMemorySize));
        EditorGUILayout.LabelField("OptimalMemorySize: "+EditorUtility.FormatBytes(OptimalMemorySize));
            
        EditorGUILayout.LabelField("OFFFileSize: "+EditorUtility.FormatBytes(OFFFileSize));
        EditorGUILayout.LabelField("KeyframeFileSize: "+EditorUtility.FormatBytes(KeyframeFileSize));
        EditorGUILayout.LabelField("OptimalFileSize: "+EditorUtility.FormatBytes(OptimalFileSize));
            
        EditorGUILayout.LabelField("OFFBlobSize: "+EditorUtility.FormatBytes(OFFBlobSize));
        EditorGUILayout.LabelField("KeyframeBlobSize: "+EditorUtility.FormatBytes(KeyframeBlobSize));
        EditorGUILayout.LabelField("OptimalBlobSize: "+EditorUtility.FormatBytes(OptimalBlobSize));
    }
    private List<GameObject> GetAnimations(string ainmationPath)
    {
        AinGameObjectList.Clear();
        ainPaths = AssetDatabase.FindAssets("t:model", new []{ainmationPath} );
        for (int i = 0; i < ainPaths.Length; i++)
        {
            string tempPath = AssetDatabase.GUIDToAssetPath(ainPaths[i]);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(tempPath);
            if (obj.GetComponentInChildren<AnimationClip>() != null)
            {
                AinGameObjectList.Add(obj);
            }
        }
        return AinGameObjectList;
    }
    
}
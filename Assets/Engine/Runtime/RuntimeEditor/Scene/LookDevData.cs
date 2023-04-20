#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
// using System.Collections;
// using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using CFEngine;

[ExecuteInEditMode]
public class LookDevData : MonoBehaviour
{
    [Tooltip("填入场景所在的路径（场景需要在同一个路径下）， 例如: Assets/Engine/LookDev")]
    public string scenePath ="Assets/";
    // public string SceneForImprotingModels;
    [Tooltip("填入场景名称")]
    //public string[] listOfScenes={" "};
    public SceneAsset[] listOfScenes;
    public Texture2D[] iconOfScenes;
    [HideInInspector]
    public string presentSceneName;
    // Scene presentScene;
    // public bool loaded;

    void Awake()
    {
        // presentSceneName = LookDevSceneSwap.presentSceneName;

        // if (presentSceneName != null) // Esc from Play Mode.
        // {
        //     EditorSceneManager.OpenScene( ScenePath + "/" +presentSceneName + ".unity",OpenSceneMode.Additive);

            
        //     // print("We are In "+presentSceneName+" now.");
        // }
        Invoke("FixLigtMaps",0.05f);
        // SaveSetting();
    }
    void Update()
    {
        //Replace Error String which copy from Windows Resource Manager.
        scenePath = scenePath.Replace(@"\", "/");
        // Debug.Log("Replace string");    
    }
    [ContextMenu("Set Icons")]
    void IconSet()
    {
        iconOfScenes = new Texture2D[listOfScenes.Length];
    }
    [ContextMenu("Save Setting")]
    void SaveSetting()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
    void FixLigtMaps()
    {
       LightmapVolumn.LoadRenderLightmaps();
    }
   
}
#endif
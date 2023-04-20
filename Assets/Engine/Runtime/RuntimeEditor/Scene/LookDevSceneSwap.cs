#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;
using CFEngine;
using Object = UnityEngine.Object;

public class LookDevSceneSwap : EditorWindow
{
    [MenuItem("Tools/场景/切换LookDev场景")]
    static void Create()
    {
        EditorWindow.GetWindow<LookDevSceneSwap>("❏ Look Dev");
    }

    public string scenePath ;
    // public string SceneForImprotingModels;
    public string[] listOfScenes;
    public Texture2D[] sceneIcon;
    [HideInInspector]
    public string presentSceneName;
    public string beforeScene;//记录play前的场景名

    [SerializeField]private string _pathOfTheScenceBeforeUsingLookDev;

    Vector2 _scrolPosition;
    // Scene presentScene;
    void Awake()
    {
        presentSceneName = null;
        if (!Application.isPlaying)
        {
            Scene ScenceBeforeUsingLookDev = EditorSceneManager.GetActiveScene();
            _pathOfTheScenceBeforeUsingLookDev = ScenceBeforeUsingLookDev.path;
            try
            {
                Scene lookDevScene = EditorSceneManager.OpenScene("Assets/Scenes/Scenelib/LookDev/lookdev_Scene.unity", OpenSceneMode.Additive);
                GameObject ob;
                if (Selection.objects.Length >0)
                {
                    ob = (GameObject)Selection.objects[0];
                    if(!ob.transform.parent)
                        EditorSceneManager.MoveGameObjectToScene(ob,lookDevScene);
                    else
                    {
                        GameObject obClone = Instantiate(ob);
                        obClone.name = ob.name;
                        EditorSceneManager.MoveGameObjectToScene(obClone,lookDevScene);
                    }
                }
                EditorSceneManager.CloseScene(ScenceBeforeUsingLookDev,true);
            }
            catch
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            }
        }
    }

    private void OnGUI()
    {   
        //////////////////////////////////////////////////
        //// ----------------GUI STYLE----------------////
        
        GUIStyle playButton = "IN EditColliderButton";
        playButton.fontSize =12;
        GUILayoutOption playButtonHeight = GUILayout.Height(25);

        GUIStyle listHead = "dragtabdropwindow";
        
        GUIStyle sceneButton = "label";

        GUIStyle debugFont = "HeaderLabel";
        debugFont.fontSize =10;
        GUILayoutOption dubugFontHeight = GUILayout.Height(12);

        GUIStyle warnFont = "CN StatusWarn";
        warnFont.fontSize =10;
        GUILayoutOption warnFontHeight = GUILayout.Height(13);

        //// ----------------GUI STYLE----------------////
        //////////////////////////////////////////////////

        // Notification of Creating a new Data when there isn't a Look Dev Data
        if (GameObject.Find("Look Dev Data") == null)
        {
            GUILayout.Space(10);
            GUILayout.Label("⚠场景中没有 Look Dev Data");
            if (GUILayout.Button("✎ 新建 Look Dev Data"))
            {
                GameObject lookDevData = new GameObject("Look Dev Data");
                lookDevData.AddComponent<LookDevData>();

            }
            
        }
        
        if(GameObject.Find("Look Dev Data") != null)
        {
            scenePath = GameObject.Find("Look Dev Data").GetComponent<LookDevData>().scenePath;
            SceneAsset[] sceneAss = GameObject.Find("Look Dev Data").GetComponent<LookDevData>().listOfScenes;
            listOfScenes = new string[sceneAss.Length];
            //Replace Error String which copy from Windows Resource Manager.
            scenePath = scenePath.Replace(@"\", "/");

            for (int i = 0; i < listOfScenes.Length; i++)
            {
                listOfScenes[i] = sceneAss[i].name;
            }
            //listOfScenes = GameObject.Find("Look Dev Data").GetComponent<LookDevData>().listOfScenes;
            sceneIcon= GameObject.Find("Look Dev Data").GetComponent<LookDevData>().iconOfScenes;
            

            if(presentSceneName == null)
            {

                GUILayout.Space(15);
                
                        
                if (!EditorApplication.isPlaying)
                {
                    if (GUILayout.Button("启动 Look Dev"))
                    {
                        for (int i = 0; i < listOfScenes.Length; i++)
                        {
                            EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath( scenePath + "/" + listOfScenes[i] + ".unity"), true);
                            presentSceneName = listOfScenes[0];
                            Debug.Log("Mark "+presentSceneName);
                        }
                        // EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath( ScenePath + "/" + presentSceneName + ".unity"), true);
                        // Open New Scene.
                        EditorSceneManager.OpenScene( scenePath + "/" +listOfScenes[0] + ".unity",OpenSceneMode.Additive);

                        // Fix Light Map
                        FixLigtMaps();
                        //     // Debug.Log("Open new Scene");                  
                         // // Debug.Log("Close old Scene");
                        
                    }

                }

                if (EditorApplication.isPlaying)
                {

                    GUILayout.Label( "⚠未激活，请结束运行，点击“启动 Look Dev”");
                }
        
            }
            else
            {
                

                // 安全play Mode 切换 PlayMode CallBack
                EditorApplication.playModeStateChanged += LogPlayModeState;

                GUILayout.Space(20);
                GUILayout.BeginHorizontal("label");
                GUILayout.Space(20);
                // 安全play
                if (!EditorApplication.isPlaying)
                {
                    // GUILayout.Label( "⚠请勿直接运行项目");
                    // GUILayout.Label( "若要运行请 安全 Play");
                    if (GUILayout.Button("▶ 安全 Play",playButton,playButtonHeight))
                    {
                        // Mark the name of scene before play
                        beforeScene = presentSceneName;
                        EditorApplication.isPlaying = true;
                    }
                }
                // 安全退出play
                else
                {
                    // GUILayout.Label( "⚠请勿直接结束运行");
                    // GUILayout.Label( "若要运行请 安全 Exit Play");
                    if (GUILayout.Button("▶ 安全退出 Play",playButton,playButtonHeight))
                    {
                        // Return the name of scene before play
                        presentSceneName = beforeScene;
                        EditorApplication.isPlaying = false;
                    }    
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                GUILayout.Space(15);
                GUILayout.Label( "Scene List ",listHead);

                // ScrolView
                _scrolPosition = GUILayout.BeginScrollView(_scrolPosition,GUILayout.Width(0),GUILayout.Height(0));
                

                for (int i = 0; i < listOfScenes.Length; i++)
                {
                    GUILayout.BeginHorizontal("Button");
                    
                    if (GUILayout.Button(new GUIContent(sceneIcon[i]),sceneButton,GUILayout.Width(35),GUILayout.Height(35))
                    ||GUILayout.Button(listOfScenes[i],sceneButton,GUILayout.Height(35)))
                    {
                        // Edit Mode
                        if (!EditorApplication.isPlaying)
                        {
                            EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath( scenePath + "/" + presentSceneName + ".unity"), true);
                            // Open New Scene.
                            EditorSceneManager.OpenScene( scenePath + "/" +listOfScenes[i] + ".unity",OpenSceneMode.Additive);

                            // Fix Light Map
                            FixLigtMaps();

                        }
                        // Play Mode
                        else
                        {
                            // Open New Scene with AsyncOperation
                            AsyncOperation asyncLoad = EditorSceneManager.LoadSceneAsyncInPlayMode( scenePath + "/" +listOfScenes[i] + ".unity", new LoadSceneParameters(LoadSceneMode.Additive) );
                            
                            // Close Old Scene.
                            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByPath( scenePath + "/" + presentSceneName + ".unity"));
                        
                            // Get screne loading asyncstate
                            asyncLoad.completed += onCompleted;

                        }
                        
                        // Mark New Present Scene.
                        presentSceneName = listOfScenes[i];
                        Debug.Log("Mark "+presentSceneName);

                    }
                    GUILayout.EndHorizontal();
                    
                    // GUILayout.Space(1);
                }
                GUILayout.EndScrollView();

                

                GUILayout.Label( "CURRENT: "+presentSceneName,debugFont,dubugFontHeight);
                GUILayout.Label("PATH: "+scenePath,debugFont,dubugFontHeight);
                // ScenePath = GUILayout.TextField(ScenePath);
                
                GUILayout.Space(15);
                // GUILayout.BeginHorizontal("Space"); 
                // GUILayout.EndHorizontal(); 
                GUILayout.FlexibleSpace();
                if (!EditorApplication.isPlaying)
                {
                    // GUILayout.Label( "⚠请勿直接关闭窗口");
                    GUILayout.Label( "  若要关闭请退出 Look Dev",warnFont,warnFontHeight);
                    if (GUILayout.Button("↷ 退出 Look Dev"))
                    {

                        EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath( scenePath + "/" + presentSceneName + ".unity"), true);
                        Close();//Close Window
                        EditorSceneManager.OpenScene(_pathOfTheScenceBeforeUsingLookDev, OpenSceneMode.Single);
                    }
                    
                }
                else
                {
                    // GUILayout.Label( "⚠请勿直接关闭窗口");
                    GUILayout.Label( "  若要关闭请先安全退出 Play",warnFont,warnFontHeight);
                }
                GUILayout.Space(5);
            }
        }

        // EndScrollView
        
        // GUI.EndScrollView();

    }
    void FixLigtMaps()
    {
        LightmapVolumn.LoadRenderLightmaps();
    }
    private void onCompleted(AsyncOperation asyncLoad)
    {
        // print("Fix Lightmap Start!");
        FixLigtMaps();  
    }

    // static PlayModeStateChangedExample()
    // {
    //     EditorApplication.playModeStateChanged += LogPlayModeState;
    // }

    // Fix Error information of registered Scene when Play Mode State Changed 修复运行状态切换时场景信息登记错误
    private void LogPlayModeState(PlayModeStateChange state)
    {
        Debug.Log(state);
        if (!EditorApplication.isPlaying)
        {
            beforeScene = presentSceneName;
        }
        if (EditorApplication.isPlaying)
        {           
            presentSceneName = beforeScene;
        }
    }

    private void OnDisable()
    {
        EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath( scenePath + "/" + presentSceneName + ".unity"), true);
        EditorSceneManager.OpenScene(_pathOfTheScenceBeforeUsingLookDev, OpenSceneMode.Single);
    }
}
#endif    


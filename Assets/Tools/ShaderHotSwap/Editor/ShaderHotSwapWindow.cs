using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace UsingTheirs.ShaderHotSwap
{

    public partial class ShaderHotSwapWindow : EditorWindowBase
    {
        [MenuItem("Window/Using Theirs/Shader HotSwap", false, 0)]
        static void Init()
        {
            var win = EditorWindow.GetWindow<ShaderHotSwapWindow>();
            win.Show();
        }

        [SerializeField] public ShaderData[] shaderDataList = null;
        private Editor editor = null;
        public static string urlPrefix { get; private set; }

        bool needToSetTitle = true;
        void SetTitle()
        {
            if (needToSetTitle)
            {
                titleContent = new GUIContent("HotSwap", styles.icon);
                needToSetTitle = false;
            }
        }
        
        private Styles styles;
        private Vector2 scrollPos;

        void OnGUI()
        {
            if (styles == null)
                styles = new Styles();
            styles.Build(this);

            SetTitle();
            
            ShowToolbarUI();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, styles.scroll, GUILayout.ExpandHeight(true));
            
            if (editor == null)
                editor = Editor.CreateEditor(this);
            if (editor != null)
                editor.OnInspectorGUI();

            GUILayout.FlexibleSpace();

            ShowLogUI();

            //ShowLink();
            
            EditorGUILayout.EndScrollView();
            
            ShowHelpMessage();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        private void ShowToolbarUI()
        {

            if (urlPrefix == null)
                urlPrefix = EditorPrefs.GetString("ShaderHotSwap_UrlPrefix", "http://localhost:8090");

            try
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                string newUrlPrefix = EditorGUILayout.TextField(urlPrefix, EditorStyles.toolbarTextField);
                if (newUrlPrefix != urlPrefix)
                {
                    urlPrefix = newUrlPrefix;
                    EditorPrefs.SetString("ShaderHotSwap_UrlPrefix", urlPrefix);
                }

                if (GUILayout.Button("Swap Shaders", EditorStyles.toolbarButton))
                {
                    RequestSwapShader();
                }

                if (GUILayout.Button("Clear Cache", EditorStyles.toolbarButton))
                {
                    var dir = GetAssetBundleOutputDir();
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);
                    SetHelpMessage("Cleared Cache", MessageType.Info);
                    log = "Cleared Cache.";
                }
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button(GetToggleShowHasHiddle(HandlerToggleShow.particlesystem) ? "开启特效" : "屏蔽特效",
                    EditorStyles.toolbarButton))
            {
                RequestToggleShow(HandlerToggleShow.particlesystem);
            }

            if (GUILayout.Button(GetToggleShowHasHiddle(HandlerToggleShow.meshrender) ? "开启网格渲染" : "屏蔽网格渲染",
                    EditorStyles.toolbarButton))
            {
                RequestToggleShow(HandlerToggleShow.meshrender);
            }

            if (GUILayout.Button(GetToggleShowHasHiddle(HandlerToggleShow.skinrender) ? "开启蒙皮渲染" : "屏蔽蒙皮渲染",
                    EditorStyles.toolbarButton))
            {
                RequestToggleShow(HandlerToggleShow.skinrender);
            }

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button(GetToggleShowHasHiddle(HandlerToggleShow.skybox) ? "开启天空盒" : "屏蔽天空盒",
                    EditorStyles.toolbarButton))
            {
                RequestToggleShow(HandlerToggleShow.skybox);
            }

            if (GUILayout.Button(GetToggleShowHasHiddle(HandlerToggleShow.ui) ? "开启UI" : "屏蔽UI",
                    EditorStyles.toolbarButton))
            {
                RequestToggleShow(HandlerToggleShow.ui);
            }

            if (GUILayout.Button(GetToggleShowHasHiddle(HandlerToggleShow.log) ? "开启Log" : "屏蔽Log",
                    EditorStyles.toolbarButton))
            {
                RequestToggleShow(HandlerToggleShow.log);
            }
            
            GUILayout.EndHorizontal();
        }


        public Dictionary<string,bool> toggleShow=new Dictionary<string, bool>();

        private bool GetToggleShowHasHiddle(string tag)
        {
            if (toggleShow.ContainsKey(tag)) return toggleShow[tag];
            
            return false;
        }

        private void RequestToggleShow(string tag)
        {
            ShaderHotSwapClient.QueryToggleShow(urlPrefix, GetToggleCommonJson(tag),
                (queryEnvResJson, queryEnvError) => 
                { 
                    SetHelpMessage(tag + ":" + queryEnvResJson, MessageType.Info);
                    log = tag + ":" + queryEnvResJson;
                });
        }

        private string GetToggleCommonJson(string tag)
        {
            var req = new QueryCommonReq();

            if (toggleShow.ContainsKey(tag))
            {
                toggleShow[tag] = !toggleShow[tag];
            }
            else
            {
                toggleShow[tag] = true;
            }
            
            req.hidden = toggleShow[tag];
            req.msg = tag;
            
            var reqContent = JsonUtility.ToJson(req);
            return reqContent;
        }

        private string log = string.Empty;
        private void ShowLogUI()
        {
            GUILayout.Label("Log:");
            GUILayout.TextArea(log, styles.log, GUILayout.ExpandHeight(true));
        }

        private void RequestSwapShader()
        {
            log = string.Empty;
            SetHelpMessage("Querying Environment...", MessageType.Info);

            ShaderHotSwapClient.QueryEnv(urlPrefix, (queryEnvResJson, queryEnvError) =>
            {
                if (!string.IsNullOrEmpty(queryEnvError))
                {
                    SetHelpMessage(queryEnvError, MessageType.Error);
                    log = queryEnvError;
                    return;
                }
                
                var queryEnvRes = JsonUtility.FromJson<QueryEnvRes>(queryEnvResJson);

                if (!string.IsNullOrEmpty(queryEnvRes.error))
                {
                    SetHelpMessage(queryEnvRes.error, MessageType.Error);
                    log = queryEnvRes.error;
                    return;
                }

                var buildTarget = ConvertRuntimePlatformToBuildTarget(queryEnvRes.platform);
                
                SetHelpMessage("Packing Shaders...", MessageType.Info);

                var swapShaderReqJson = ComposeSwapShaderReq(buildTarget, shaderDataList);

                SetHelpMessage("Swapping...", MessageType.Info);

                ShaderHotSwapClient.SwapShaders(urlPrefix, swapShaderReqJson, (swapShadersResJson, swapShadersError) =>
                {
                    if (!string.IsNullOrEmpty(swapShadersError))
                    {
                        SetHelpMessage(swapShadersError, MessageType.Error);
                        log = swapShadersError;
                        return;
                    }

                    var swapShadersRes = JsonUtility.FromJson<SwapShadersRes>(swapShadersResJson);
                    
                    Logger.Log("[ShaderHotSwapWindow] Swap Shader Remote Log\n{0}", swapShadersRes.log);

                    if (!string.IsNullOrEmpty(swapShadersRes.error))
                    {
                        SetHelpMessage(swapShadersRes.error, MessageType.Error);
                        log = swapShadersRes.error;
                        return;
                    }

                    SwapShadersResToString(swapShadersRes);

                    SetHelpMessage("Success!", MessageType.Info);
                    Repaint();
                });
            });
        }

        private void SwapShadersResToString(SwapShadersRes res)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Swap Shaders OK.\n");
            
            foreach (var shader in res.shaders)
            {
                sb.AppendFormat("Shader <b>[{0}]</b>\n", shader.shader.name);
                
                foreach (var mat in shader.materials)
                {
                    sb.AppendFormat("  Material <b>[{0}]</b>\n", mat.material.name);
                    
                    foreach (var renderer in mat.renderers)
                    {
                        sb.AppendFormat("    Renderer <b>[{0}]</b>\n", renderer.name);
                    }
                }
            }
            
            log = sb.ToString();
        }

        static BuildTarget ConvertRuntimePlatformToBuildTarget(string runtimePlatform)
        {
            var buildTarget = BuildTarget.StandaloneWindows64;
            switch (runtimePlatform)
            {
                case "OSXEditor":
                case "OSXPlayer":
                    buildTarget = BuildTarget.StandaloneOSX;
                    break;
                case "WindowsEditor":
                case "WindowsPlayer":
                    buildTarget = BuildTarget.StandaloneWindows;
                    break;
                case "IPhonePlayer":
                    buildTarget = BuildTarget.iOS;
                    break;
                case "Android":
                    buildTarget = BuildTarget.Android;
                    break;
                case "LinuxEditor":
                case "LinuxPlayer":
                    buildTarget = BuildTarget.StandaloneLinux;
                    break;
                case "WebGLPlayer":
                    buildTarget = BuildTarget.WebGL;
                    break;
                case "WSAPlayerX86":
                case "WSAPlayerX64":
                case "WSAPlayerARM":
                    buildTarget = BuildTarget.WSAPlayer;
                    break;
                case "PS4":
                    buildTarget = BuildTarget.PS4;
                    break;
                case "XboxOne":
                    buildTarget = BuildTarget.XboxOne;
                    break;
                case "tvOS":
                    buildTarget = BuildTarget.tvOS;
                    break;
                case "Switch":
                    buildTarget = BuildTarget.Switch;
                    break;
            }
            
            Logger.Log("[ShaderHotSwap] runtimePlatform:{0}, buildTarget:{1}", runtimePlatform,
                buildTarget);

            return buildTarget;
        }

        static string ComposeSwapShaderReq(BuildTarget buildTarget, ShaderData[] shaderDataList1)
        {
            List<ShaderData> allUsed = new List<ShaderData>();
            foreach (var shaderData in shaderDataList1)
            {
                if (!shaderData.enable) continue;
                allUsed.Add(shaderData);
            }

            ShaderData[] shaderDataList = allUsed.ToArray();
            
            var req = new SwapShadersReq();
            req.shaders = new List<RemoteShader>();

            foreach (var shaderData in shaderDataList)
            {
                if (shaderData == null)
                {
                    Logger.LogError("[ShaderHotSwap] shaderData is null.");
                    continue;
                }
                
                if (!shaderData.enable) continue;

                req.shaders.Add(new RemoteShader()
                {
                    name = shaderData.shader.name,
                    guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(shaderData.shader)),
                });
            }

            if (req.shaders.Count == 0)
                throw new Exception("[ShaderHotSwap] No Shader");
            
            req.assetBundleBase64 = ShaderPack.PackShaders(buildTarget, GetAssetBundleOutputDir(), shaderDataList);
                    
            var reqContent = JsonUtility.ToJson(req);
            return reqContent;
        }

        static string GetAssetBundleOutputDir()
        {
            var outputDir = Path.Combine(Application.dataPath, "..");
            outputDir = Path.Combine(outputDir, "Library");
            outputDir = Path.Combine(outputDir, "UsingTheirsShaderHotSwap");
            return outputDir;
        }
        
    }
}

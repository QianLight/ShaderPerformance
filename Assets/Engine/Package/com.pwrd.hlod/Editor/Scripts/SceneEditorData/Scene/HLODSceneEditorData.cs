#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using Athena.MeshSimplify;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace com.pwrd.hlod.editor
{
    // HLOD编辑器数据
    [ExecuteInEditMode]
    public class HLODSceneEditorData : MonoBehaviour
    {
        public const string sceneNodeName = "HLODSceneNodeEditorData";
        public const string hlodDefaultSettingDataPath = "Assets/Athena/ConfigSetting/HLODDefaultSetting.asset";
        
        //global setting
        public HlodMethod hlodMethod = HlodMethod.AthenaSimplify;
        public bool useVoxel = false;
        public ProxyMapType proxyMapType = ProxyMapType.LODGroup;
        public TextureChannel textureChannel = TextureChannel.Albedo;
        public RendererBakerSetting rendererBakerSetting;
        public ShaderBindConfig shaderBindConfig;
        public SceneSetting globalSetting;
        
        [HideInInspector] public State state;
        public bool hasMergeAtlas = false;
        public List<SceneNode> scenes = new List<SceneNode>();

        public bool m_init = false;
        public bool debug;

        private void Awake()
        {
            this.tag = "EditorOnly";
            InitDefaultData();
        }
        
        /// <summary>
        /// 初始化默认全局数据
        /// </summary>
        public void InitDefaultData()
        {
            var defaultData = GetOrCreateShaderStripperData();
            InitData(defaultData, false);
        }

        /// <summary>
        /// 初始化配置数据
        /// </summary>
        /// <param name="updateExistingNode">更新已存在场景节点，覆盖原始信息</param>
        public void InitData(HLODConfigSetting configSetting, bool updateExistingNode = false, bool forceInit = false)
        {
            if (!forceInit && m_init) return;
            m_init = true;
            globalSetting = configSetting.globalSetting.Clone();
            proxyMapType = configSetting.proxyMapType;
            textureChannel = configSetting.textureChannel;
            hlodMethod = configSetting.hlodMethod;
            useVoxel = configSetting.useVoxel;
            rendererBakerSetting = configSetting.rendererBakerSetting.Clone();
            shaderBindConfig = configSetting.shaderBindConfig.Clone();
            scenes.Clear();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var openScene = SceneManager.GetSceneAt(i);
                if (openScene.isLoaded && openScene.IsValid())
                {
                    var sceneNodeEditorData = GetSceneNode(openScene);
                    if (sceneNodeEditorData == null)
                    {
                        sceneNodeEditorData = CreateSceneNode(openScene, configSetting);
                        if (Application.isEditor) EditorSceneManager.MarkSceneDirty(openScene);
                    }
                    else if (updateExistingNode)
                    {
                        UpdateSceneNodeData(sceneNodeEditorData.sceneNode, configSetting);
                        if (Application.isEditor) EditorSceneManager.MarkSceneDirty(openScene);
                    }
                    scenes.Add(sceneNodeEditorData.sceneNode);
                }
            }
        }

        /// <summary>
        /// 更新配置数据
        /// </summary>
        public void UpdateSceneNodeData(HLODConfigSetting configSetting)
        {
            foreach (var sceneNode in scenes)
            {
                UpdateSceneNodeData(sceneNode, configSetting);
            }
        }

        private HLODSceneNodeEditorData GetSceneNode(Scene scene)
        {
            HLODSceneNodeEditorData sceneNodeEditorData = null;
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                if (rootGameObject.name.Equals(sceneNodeName) && rootGameObject.GetComponent<HLODSceneNodeEditorData>())
                {
                    sceneNodeEditorData = rootGameObject.GetComponent<HLODSceneNodeEditorData>();
                    break;
                }
            }
            return sceneNodeEditorData;
        }

        private HLODSceneNodeEditorData CreateSceneNode(Scene scene, HLODConfigSetting defaultData)
        {
            HLODSceneNodeEditorData sceneNodeEditorData = null;
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                if (rootGameObject)
                {
                    var obj = new GameObject(sceneNodeName) { hideFlags = HideFlags.DontSaveInBuild, tag = "EditorOnly" };
                    obj.transform.SetParent(rootGameObject.transform);
                    obj.transform.SetParent(null);
                    sceneNodeEditorData = obj.AddComponent<HLODSceneNodeEditorData>();
                    sceneNodeEditorData.sceneNode = new SceneNode();
                    var sceneNode = sceneNodeEditorData.sceneNode;
                    sceneNode.sceneName = scene.name;
                    sceneNode.scenePath = scene.path;
                    UpdateSceneNodeData(sceneNode, defaultData);
                    break;
                }
            }
            return sceneNodeEditorData;
        }
        
        private void UpdateSceneNodeData(SceneNode sceneNode, HLODConfigSetting data)
        {
            var scene = sceneNode.scene;
            sceneNode.targetParent = GetSceneObj(scene, data.targetParentNamePath);
            if (data.rootNamePaths != null && data.rootNamePaths.Count > 0)
            {
                sceneNode.roots = new List<GameObject>();
                for (int i = 0; i < data.rootNamePaths.Count; i++)
                {
                    sceneNode.roots.Add(GetSceneObj(scene, data.rootNamePaths[i]));
                }
            }
            sceneNode.layers = new List<Layer>();
            for (int i = 0; i < data.globalSetting.layerSettings.Count; i++)
            {
                sceneNode.layers.Add(new Layer() { });
            }
        }

        private HLODConfigSetting GetOrCreateShaderStripperData()
        {
            var dirName = Path.GetDirectoryName(hlodDefaultSettingDataPath);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
            if (!File.Exists(hlodDefaultSettingDataPath))
            {
                HLODConfigSetting data = ScriptableObject.CreateInstance<HLODConfigSetting>();
                data.rendererBakerSetting = new RendererBakerSetting();
                data.shaderBindConfig = new ShaderBindConfig()
                {
#if HLOD_USE_URP
                    defaultBakeShader = Shader.Find("Athena/Bake/Lit"),
#else
                    defaultBakeShader = Shader.Find("Athena/Bake/Unlit"),
#endif
                    bakeShaderList = new List<ShaderBindConfig.Tuple>(),
                };
                var defaultSettings = LayerSetting.GetDefaultSetting();
                if (defaultSettings.Count > 0) data.globalSetting.layerSettings = new List<LayerSetting>() { defaultSettings[0].Clone() };
                AssetDatabase.CreateAsset(data, hlodDefaultSettingDataPath);
            }
            return AssetDatabase.LoadAssetAtPath<HLODConfigSetting>(hlodDefaultSettingDataPath);
        }
        
        private GameObject GetSceneObj(Scene scene, string fullname)
        {
            GameObject obj = null;
            if (!string.IsNullOrEmpty(fullname) && scene.isLoaded && scene.IsValid())
            {
                var names = fullname.Split('/');
                var rootname = names[0];
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    if (rootGameObject.name.Equals(rootname))
                    {
                        var subPath = "";
                        if (names.Length > 1)
                        {
                            subPath = fullname.Remove(0, rootname.Length + 1);
                        }
                        try
                        {
                            obj = string.IsNullOrEmpty(subPath)
                                ? rootGameObject
                                : rootGameObject.transform.Find(subPath).gameObject;
                            break;
                        }
                        catch (Exception e)
                        {
                            HLODDebug.LogWarning(e);
                        }
                    }
                }
            }
            return obj;
        }

        private void OnDestroy()
        {
            HLODMessageCenter.SendMessage(HLODMesssages.SCENE_EDITOR_DATA_DESTORYED);
        }

        private void OnDrawGizmos()
        {
            if (!debug)
                return;
            Bounds bounds = new Bounds();
            bool notset = true;
            foreach (var obj in Selection.objects)
            {
                var go = obj as GameObject;
                if (go != null)
                {
                    var renderers = go.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers)
                    {
                        if (notset)
                        {
                            bounds = r.bounds;
                            notset = false;
                        }
                        else
                        {
                            bounds.Encapsulate(r.bounds);
                        }
                    }
                }
            }

            var color = Gizmos.color;
            Gizmos.color = Color.green;
            Gizmos.DrawCube(bounds.center, bounds.size);
            Gizmos.color = color;
        }

        private void OnGUI()
        {
            if (!debug)
                return;
            Bounds bounds = new Bounds();
            bool notset = true;
            foreach (var obj in Selection.objects)
            {
                var go = obj as GameObject;
                if (go != null)
                {
                    var renderers = go.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers)
                    {
                        if (notset)
                        {
                            bounds = r.bounds;
                            notset = false;
                        }
                        else
                        {
                            bounds.Encapsulate(r.bounds);
                        }
                    }
                }
            }

            var b = bounds;
            if (Vector3.Distance(Vector3.zero, b.size) > 0 && debug)
            {
                GUILayout.Label("center:" + b.center + "size:" + b.size);
                HLODDebug.Log("center:" + b.center + "size:" + b.size);
            }
        }
    }
}

#endif
#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace Engine.Editor
{
    public class RenderSettings : EditorWindow
    {
        private UniversalRenderPipelineAsset _urpAsset;
        private RenderLevelConfig _renderLevelConfig;
        private string _urpAssetPath = "Assets/Engine/Runtime/Shaders/Shading/URPAsset.asset";
        private string _renderLevelConfigPath = "Assets/BundleRes/Config/RenderLevelConfig.asset";
        private NativeArray<VisibleLight> _visibleLights;
    
        private Volume[] _volumes;
        private List<string> _volumeNames;
        private int _selectedVolume;
    
        private Vector2 _scrollPosition;
        private Vector2 _volumeOptions;
        private Vector2 _volumEditor;

        private List<Light> _directLights = new List<Light>();
        private List<Light> _additionalLights = new List<Light>();
        private Light[] _allLights = new Light[]{};

        enum PageType
        {
            UrpAsset,
            RenderLevelConfig,
            Volume,
            Lighting
        }
    
        private string[] _pageOptions = new[] {"Universal Render Pipeline Asset", "Render Level Config","Volumes In Current Scene","Lightings In Current Scene"};
        private PageType _pageType;
        
        [MenuItem("Tools/引擎/Render Settings #&r")]
        private static void ShowWindow()
        {
            var window = GetWindow<RenderSettings>();
            window.titleContent = new GUIContent("Render Settings");
            window.Show();
        }
    
        private void Awake()
        {
            Refresh();
        }

        void Refresh()
        {
            _urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(_urpAssetPath);
            _renderLevelConfig = AssetDatabase.LoadAssetAtPath<RenderLevelConfig>(_renderLevelConfigPath);
            GetCurrentVolumes();
        }
    
        private void OnGUI()
        {
            _pageType = (PageType)GUILayout.Toolbar((int)_pageType,_pageOptions);
            
            GUILayout.Space(10);
            
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,GUILayout.Width(0),GUILayout.Height(0));
            {
                if(_pageType == PageType.UrpAsset)
                {
                    var urpAssetEditor = UnityEditor.Editor.CreateEditor(_urpAsset);
                    urpAssetEditor.OnInspectorGUI();
                }
                
                else if(_pageType == PageType.RenderLevelConfig)
                {
                    var renderLevelConfigEditor = UnityEditor.Editor.CreateEditor(_renderLevelConfig);
                    renderLevelConfigEditor.OnInspectorGUI();
                }
    
                else if (_pageType == PageType.Volume)
                {
                    if (_volumes.Length > 0 && _volumes[_selectedVolume])
                    {
                        GUILayout.BeginHorizontal();
                        {
                            _volumeOptions = GUILayout.BeginScrollView(_volumeOptions,GUILayout.Width(180),GUILayout.Height(0));
                            {
                                GUILayout.Label("Volumes List");
                                _selectedVolume = GUILayout.SelectionGrid(_selectedVolume,_volumeNames.ToArray(),1);
                            }
                            GUILayout.EndScrollView();
                            
                            _volumEditor = GUILayout.BeginScrollView(_volumEditor,GUILayout.Width(0),GUILayout.Height(0));
                            {
                                GUILayout.Space(5);
                                var volumeEditor = UnityEditor.Editor.CreateEditor(_volumes[_selectedVolume]);
                                EditorGUILayout.InspectorTitlebar(true,volumeEditor);
                                volumeEditor.OnInspectorGUI();
                            }
                            GUILayout.EndScrollView();
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                else if (_pageType == PageType.Lighting)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GetCurrentLightInRender();
                    GUILayout.EndVertical();
                    GUILayout.Space(10);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GetActiveRealtimeLight();
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndScrollView();
            
            GUILayout.FlexibleSpace();
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh"))
                {
                    Refresh();
                }
            }
            GUILayout.EndHorizontal();
        }
    
        void GetCurrentVolumes()
        {
            _volumes = GameObject.FindObjectsOfType<Volume>();
            _volumeNames = new List<string>();
            if (_volumes.Length >0)
            {
                for (int i = 0; i < _volumes.Length; i++)
                {
                    _volumeNames.Add(_volumes[i].name);
                }
            }
        }

        void GetCurrentLightInRender()
        {
            _directLights.Clear();
            _additionalLights.Clear();
            if (ForwardLights.DebugInfo.DebugVisibleLights.Count > 0)
            {
                foreach (var visibleLight in ForwardLights.DebugInfo.DebugVisibleLights)
                {
                    if (visibleLight.type == LightType.Directional)
                    {
                        _directLights.Add(visibleLight);
                    }
                    else if(_additionalLights.Count < ForwardLights.DebugInfo.DebugAdditionalLightCount)
                    {
                        _additionalLights.Add(visibleLight);
                    }
                }
            }

            GUILayout.Label("Current Directinal Lights in Render",EditorStyles.boldLabel);
            GUILayout.Space(5);
            foreach (var directLight in _directLights)
            {
                EditorGUILayout.ObjectField(directLight,typeof(Light));
            }
            GUILayout.Space(5);
            GUILayout.Label("Current Additional Lights in Render",EditorStyles.boldLabel);
            GUILayout.Space(5);
            int count = math.min(ForwardLights.DebugInfo.DebugAdditionalLightCount, _additionalLights.Count);
            if (count>0)
            {
                for (int i = 0; i < count; i++)
                {
                    EditorGUILayout.ObjectField(_additionalLights[i],typeof(Light));
                }
            }
            else
            {
                GUILayout.Label("[Null]");
            }
            GUILayout.Space(5);
            Repaint();
            
        }

        void GetActiveRealtimeLight()
        {
            GUILayout.Label("Active Realtime Lights in Scene",EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (_allLights.Length > 0)
            {
                foreach (var light in _allLights)
                {
                    if (light && light.lightmapBakeType != LightmapBakeType.Baked)
                    {
                        if (_additionalLights.Contains(light)||_directLights.Contains(light))
                        {
                            EditorGUILayout.ObjectField("Is Rendering",light,typeof(Light));
                        }
                        else
                        {
                            EditorGUILayout.ObjectField("------",light,typeof(Light));
                        }
                    }
                } 
            }
            else
            {
                _allLights = FindObjectsOfType<Light>();
                GUILayout.Label("[Null]");
            }
            GUILayout.Space(5);
            
        }

        private void OnDisable()
        {
            _volumes = null;
        }
    }
}
#endif
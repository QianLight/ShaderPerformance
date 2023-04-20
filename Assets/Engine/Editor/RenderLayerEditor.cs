using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CFEngine.Editor
{
    using EXRFlags = Texture2D.EXRFlags;

    [CanEditMultipleObjects, CustomEditor(typeof(RenderLayer))]
    public sealed class RenderLayerEditor : BaseEditor<RenderLayer>
    {
        private static readonly SavedBool captureSettingFolder = new SavedBool($"{nameof(RenderLayerEditor)}.{nameof(captureSettingFolder)}", false);
        private static readonly SavedBool captureSkybox = new SavedBool($"{nameof(RenderLayerEditor)}.{nameof(captureSkybox)}", true);
        private static readonly SavedInt captureFormat = new SavedInt($"{nameof(RenderLayerEditor)}.{nameof(captureFormat)}", 0);
        private static readonly SavedBool captureSRgb = new SavedBool($"{nameof(RenderLayerEditor)}.{nameof(captureSRgb)}", false);
        private static readonly SavedBool captureLut = new SavedBool($"{nameof(RenderLayerEditor)}.{nameof(captureLut)}", true);
        private static readonly GUIContent captureColorContent = new GUIContent("Background Color");
        private static readonly SavedSceneComponent<Transform> captureTarget = new SavedSceneComponent<Transform>($"{nameof(RenderLayerEditor)}.{nameof(captureTarget)}");
        HashSet<Renderer> captureDisabledRenderers = new HashSet<Renderer>();

        private enum CaptureFormat
        {
            EXR,
            JPG,
        }

        EffectListEditor m_EffectList;
        private void UpdateProfile()
        {
            if (m_EffectList == null)
            {
                if (RenderLayer.envProfile != null)
                {
                    m_EffectList = new EffectListEditor(this);

                    m_EffectList.Init(RenderLayer.envProfile, new SerializedObject(RenderLayer.envProfile));

                }
            }
        }

        void OnEnable()
        {
            UpdateProfile();
            EditorApplication.update += EditorUpdate;
        }

        void OnDisable()
        {
            if (m_EffectList != null)
                m_EffectList.Clear();
            EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate()
        {
            PostExportFrameToExr();
        }

        public override void OnInspectorGUI()
        {
            RenderLayer renderLayer = target as RenderLayer;
            serializedObject.Update();
            EditorUtilities.DrawSplitter();
            UpdateProfile();
            if (m_EffectList == null)
            {
                if (GUILayout.Button("Refresh"))
                {
                    UpdateProfile();
                }
            }

            captureSettingFolder.Value = EditorGUILayout.Foldout(captureSettingFolder.Value, "Capture Setting");
            if (captureSettingFolder.Value)
            {
                captureSkybox.Value = EditorGUILayout.Toggle("Capture Skybox", captureSkybox.Value);
                captureFormat.Value = (int)(CaptureFormat)EditorGUILayout.EnumPopup("Capture Format", (CaptureFormat)captureFormat.Value);
                captureSRgb.Value = EditorGUILayout.Toggle("sRGB", captureSRgb.Value);
                renderLayer.captureColor = EditorGUILayout.ColorField(captureColorContent, renderLayer.captureColor, true, false, false);
                captureTarget.Value = EditorGUILayout.ObjectField("Capture Target", captureTarget.Value, typeof(Transform), true) as Transform;
            }
            if (GUILayout.Button("Export Frame Tex"))
                ExportFrameToExr();

            if (m_EffectList != null)
            {
                m_EffectList.OnHeadGUI(OnWillSaveLayer);
                m_EffectList.OnGUI();
            }
            else
            {
                EditorGUILayout.LabelField("env effect not init!");
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnWillSaveLayer(EffectListEditor.Operation operation, string path)
        {
            if (operation == EffectListEditor.Operation.Save)
            {
                ScatterEditor.SaveCurrentProfileScatter(path);
            }
        }

        public void OnSceneGUI()
        {
            if (m_EffectList != null)
            {
                m_EffectList.OnSceneGUI();
            }
        }

        int w;
        int h;
        Texture2D texOut;
        RenderTexture targetRT;
        string path;
        int request = 0;

        void ExportFrameToExr()
        {
            EngineContext engineContex = EngineContext.instance;
            if (engineContex != null)
            {
                RenderContext.customClearColorGetter = CustomClearColorGetter;

                var camera = engineContex.CameraRef;
                if (camera != null)
                {
                    string desc;
                    string ext;
                    RenderTextureFormat rtFormat;
                    switch ((CaptureFormat)captureFormat.Value)
                    {
                        case CaptureFormat.EXR:
                            desc = "EXR";
                            ext = "jpg";
                            rtFormat = RenderTextureFormat.ARGBFloat;
                            break;
                        case CaptureFormat.JPG:
                            desc = "JPG";
                            ext = "jpg";
                            rtFormat = RenderTextureFormat.ARGB32;
                            break;
                        default:
                            desc = string.Empty;
                            ext = string.Empty;
                            rtFormat = RenderTextureFormat.Default;
                            break;
                    }
                    path = EditorUtility.SaveFilePanel($"Export {desc}...", "", "Frame", ext);

                    if (string.IsNullOrEmpty(path))
                        return;

                    EditorUtility.DisplayProgressBar($"Export {desc}", "Rendering...", 0f);

                    w = camera.pixelWidth;
                    h = camera.pixelHeight;

                    RenderTextureReadWrite readWrite = captureSRgb.Value ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;
                    texOut = new Texture2D(w, h, TextureFormat.RGBAFloat, false, false);
                    targetRT = RenderTexture.GetTemporary(w, h, 24, rtFormat, readWrite);

                    EditorUtility.DisplayProgressBar($"Export {desc}", "Reading...", 0.25f);
                    RenderContext.capturing = true;
                    ColorGrading.breakLUT = !captureLut.Value;
                    RenderLayer.caputerRT = targetRT;
                    SRP.DrawSkyboxPass.drawSkybox = captureSkybox.Value;
                    request = 5;

                    // Disable
                    if (captureTarget.Value)
                    {
                        HashSet<Renderer> targetRenderers = new HashSet<Renderer>(captureTarget.Value.GetComponentsInChildren<Renderer>());
                        captureDisabledRenderers.Clear();
                        Scene scene = SceneManager.GetActiveScene();
                        GameObject[] roots = scene.GetRootGameObjects();
                        foreach (GameObject root in roots)
                        {
                            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
                            foreach (Renderer renderer in renderers)
                            {
                                if (renderer.enabled && !targetRenderers.Contains(renderer))
                                {
                                    captureDisabledRenderers.Add(renderer);
                                    renderer.enabled = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool CustomClearColorGetter(Camera camera, out Color color)
        {
            color = (target as RenderLayer).captureColor;
            color.a = 0;
            return camera.name == "Main Camera";
        }

        void PostExportFrameToExr()
        {
            if (request > 0)
            {
                if (!RenderContext.capturing)
                {
                    if (request == 1)
                    {
                        var lastActive = RenderTexture.active;
                        RenderTexture.active = targetRT;
                        texOut.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                        texOut.Apply();
                        RenderTexture.active = lastActive;

                        byte[] bytes;
                        string desc;
                        switch ((CaptureFormat)captureFormat.Value)
                        {
                            case CaptureFormat.EXR:
                                bytes = texOut.EncodeToEXR(EXRFlags.OutputAsFloat | EXRFlags.CompressZIP);
                                desc = "EXR";
                                break;
                            case CaptureFormat.JPG:
                                bytes = texOut.EncodeToJPG(100);
                                desc = "JPG";
                                break;
                            default:
                                bytes = null;
                                desc = string.Empty;
                                break;
                        }

                        EditorUtility.DisplayProgressBar($"Export {desc}", "Saving...", 0.75f);

                        File.WriteAllBytes(path, bytes);
                        RenderLayer.caputerRT = null;
                        EditorUtility.ClearProgressBar();
                        AssetDatabase.Refresh();

                        RenderTexture.ReleaseTemporary(targetRT);
                        DestroyImmediate(texOut);

                        RenderContext.customClearColorGetter -= CustomClearColorGetter;
                        CFEngine.SRP.DrawSkyboxPass.drawSkybox = true;

                        foreach (Renderer renderer in captureDisabledRenderers)
                        {
                            renderer.enabled = true;
                        }
                        captureDisabledRenderers.Clear();
                    }
                    request--;
                }
            }

        }
    }
}
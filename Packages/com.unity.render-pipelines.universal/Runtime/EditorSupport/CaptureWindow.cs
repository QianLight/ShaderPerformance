#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum CaptureFormat
{
    EXR,
    JPG,
    TGA,
    PNG,
}

public class CaptureWindow : EditorWindow
{

    #region Serialized

    public static readonly SavedBool skybox =
        new SavedBool($"{nameof(Capture)}.{nameof(skybox)}", true);
    private static readonly SavedInt format = 
        new SavedInt($"{nameof(Capture)}.{nameof(format)}", (int)CaptureFormat.JPG);
    public static readonly SavedColor backgroundColor = 
        new SavedColor($"{nameof(Capture)}.{nameof(backgroundColor)}", Color.white);
    private static readonly SavedSceneComponent<Transform> captureTarget =
        new SavedSceneComponent<Transform>($"{nameof(Capture)}.{nameof(captureTarget)}");

    #endregion


    #region Temp

    public static int captureRenderCounter = -1;
    public static int captureImportCounter;
    private static HashSet<Renderer> captureDisabledRenderers = new HashSet<Renderer>();
    private static readonly GUIContent captureColorContent = new GUIContent("Background Color");
    private static string path;
    private static CameraClearFlags clearFlagsBackup;
    private static Camera cameraBackup;
    private static Color backgroundColorBackup;
    private static Action<string> callback;

    #endregion

    private void OnGUI()
    {
        skybox.Value = EditorGUILayout.Toggle("渲染天空盒", skybox.Value);
        format.Value =
            (int) (CaptureFormat) EditorGUILayout.EnumPopup("截图格式", (CaptureFormat) format.Value);
        backgroundColor.Value =
            EditorGUILayout.ColorField(captureColorContent, backgroundColor.Value, true, false, false);
        captureTarget.Value =
            EditorGUILayout.ObjectField("只截指定物体", captureTarget.Value, typeof(Transform), true) as
                Transform;

        if (GUILayout.Button("截图 (Ctrl + Shift + Alt + O)"))
        {
            Capture();
        }
    }

    [MenuItem("ArtTools/打开截图窗口 %&O")]
    public static void OpenWindow()
    {
        CaptureWindow window = GetWindow<CaptureWindow>();
        window.titleContent = new GUIContent("截图工具");
        window.Show();
    }
    
    [MenuItem("ArtTools/截图 %#&O")]
    private static void Capture()
    {
        string desc = ((CaptureFormat) format.Value).ToString().ToUpper();
        string ext = ((CaptureFormat) format.Value).ToString().ToLower();

        path = EditorUtility.SaveFilePanel($"截图 {desc}", "Assets", "Frame", ext);
        if (string.IsNullOrEmpty(path))
            return;

        BeginCapture();
    }
    
    public static void Capture(string savePath, Action<string> callback)
    {
        path = savePath;
        CaptureWindow.callback = callback;
    }

    private static void BeginCapture()
    {
        captureRenderCounter = 5;

        Scene scene = SceneManager.GetActiveScene();

        if (captureTarget.Value)
        {
            HashSet<Renderer> targetRenderers =
                new HashSet<Renderer>(captureTarget.Value.GetComponentsInChildren<Renderer>());
            captureDisabledRenderers.Clear();
            GameObject[] roots = scene.GetRootGameObjects();
            cameraBackup = Camera.main;
            if (!skybox.Value && cameraBackup)
            {
                clearFlagsBackup = cameraBackup.clearFlags;
                cameraBackup.clearFlags = CameraClearFlags.SolidColor;
                backgroundColorBackup = cameraBackup.backgroundColor;
                cameraBackup.backgroundColor = backgroundColor.Value;
            }

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

        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }

    public static void OnRenderFinish()
    {
        ScreenCapture.CaptureScreenshot(path);

        if (skybox.Value && cameraBackup)
        {
            cameraBackup.clearFlags = clearFlagsBackup;
            cameraBackup.backgroundColor = backgroundColorBackup;
        }

        foreach (Renderer renderer in captureDisabledRenderers)
            renderer.enabled = true;
        captureDisabledRenderers.Clear();
        captureRenderCounter = -1;

        captureImportCounter = 5;
        EditorApplication.update += ImportTexture;
    }

    private static void ImportTexture()
    {
        if (string.IsNullOrEmpty(path))
        {
            EditorApplication.update -= ImportTexture;
            return;    
        }
        
        if (captureImportCounter-- > 0)
            return;
        
        EditorApplication.update -= ImportTexture;

        string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
        if (path.StartsWith(projectPath))
        {
            int subLength = Application.dataPath.Length - "Assets".Length;
            string relativePath = path.Substring(subLength);
            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
            Selection.activeObject = texture;            
            Debug.Log($"Capture Finish: {path}", texture);
        }
        else
        {
            Debug.Log($"Capture Finish: {path}");
        }

        callback?.Invoke(path);

        cameraBackup = null;
        path = null;
        callback = null;
    }
}

#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace SceneTools
{
    public class LightmapPaintWindow : EditorWindow
    {
        private const float MAX_BRUSH_SIZE = 100f;
        private const float MIN_BRUSH_SIZE = 1f;
        private const int CEIL_WIDTH = 64;
        private const int CEIL_HEIGHT = 64;
        private const int CEIL_PADDING = 2;
        private const int CEIL_BORDER = 4;

        private const string BRUSH_MASK_PATH = "Assets/Engine/Editor/Tools/Scene/LayerBrush/Content/Icons/";

        private bool isSuckColor;
        private bool isExitPaint;

        private Color paintColor;
        private Color paintColorCopy;

        private Texture2D lightmapTex;
        private Texture2D lightmapCopy;
        private string lightmapPath;
        private string lightmapCopyPath;
        private string orginLightmapName;


        private float burshHardness = 1f;
        private Texture2D brushMaskTexCopy;
        private List<Texture2D> brushMaskTexList;
        private int curSelectMaskTexIndex;

        private string[] brushMaskStrs = new[]
        {
            "Brush/Brush_0",
            "Brush/Brush_1",
            "Brush/Brush_2",
            "Brush/Brush_3",
        };

        private Vector3 hitPos = Vector3.zero;
        private Vector3 hitNormal = Vector3.one;
        private RaycastHit[] hitInfos = new RaycastHit[20];

        private float brushSize = 15f;

        private int layerMask;

        private MeshCollider meshCollider;
        private GameObject selectObj;

        [MenuItem("Tools/场景/光照贴图绘制工具", false, 100)]
        public static void OpenWindow()
        {
            GetWindow<LightmapPaintWindow>().Show();
        }


        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            layerMask = LayerMask.NameToLayer("Default");
            layerMask = 1 << layerMask;

            brushMaskTexList = new List<Texture2D>();
            string tempPath = String.Empty;
            for (int i = 0; i < brushMaskStrs.Length; i++)
            {
                tempPath = BRUSH_MASK_PATH + brushMaskStrs[i] + ".png";
                Texture2D tempMakTex = AssetDatabase.LoadAssetAtPath<Texture2D>(tempPath);
                if (tempMakTex != null)
                {
                    brushMaskTexList.Add(tempMakTex);
                }
            }

            OnChangeMaskTex();
        }


        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnDestroy()
        {
            if (meshCollider != null)
            {
                meshCollider.enabled = false;
            }

            DeleteLightmapCopy();

            if (brushMaskTexList != null)
            {
                brushMaskTexList.Clear();
                brushMaskTexList = null;
            }

            RecoverLightmapName();
            lightmapTex = null;
            lightmapPath = null;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            isSuckColor = EditorGUILayout.ToggleLeft("吸色(快捷键:按住P)", isSuckColor);
            isExitPaint = EditorGUILayout.ToggleLeft("退出绘制", isExitPaint);
            EditorGUILayout.EndHorizontal();
            
            brushSize = EditorGUILayout.Slider("笔刷大小(快捷键:[ ]键)", brushSize, MIN_BRUSH_SIZE, MAX_BRUSH_SIZE);
            burshHardness = EditorGUILayout.Slider("笔刷硬度", burshHardness, 0.1f, 2f);

            DrawMaskTex();

            if (GUILayout.Button("初始化绘制数据"))
            {
                InitData();
            }

            if (GUILayout.Button("Baked"))
            {
                Baked();
            }
        }

        private void RecoverLightmapName()
        {
            if (lightmapTex != null)
            {
                AssetDatabase.RenameAsset(lightmapPath, orginLightmapName);
                AssetDatabase.Refresh();
            }
        }

        private void DeleteLightmapCopy()
        {
            if (lightmapCopy != null)
            {
                AssetDatabase.DeleteAsset(lightmapCopyPath);
                lightmapCopy = null;
                lightmapCopyPath = null;
            }
        }

        private void OnChangeMaskTex()
        {
            if (curSelectMaskTexIndex < 0 || brushMaskTexList == null || curSelectMaskTexIndex > brushMaskTexList.Capacity)
            {
                return;
            }

            Texture2D curMaskTex = brushMaskTexList[curSelectMaskTexIndex];
            if (curMaskTex != null)
            {
                brushMaskTexCopy = new Texture2D(curMaskTex.width, curMaskTex.height, curMaskTex.format, curMaskTex.mipmapCount > 0);
                Graphics.CopyTexture(curMaskTex, brushMaskTexCopy);
                brushMaskTexCopy.Apply(true);
            }
        }

        private void DrawMaskTex()
        {
            int row = 1;
            int col = 4;

            Rect rect = GUILayoutUtility.GetLastRect();
            float orginX = rect.x;
            float orginY = rect.y + CEIL_HEIGHT * 0.5f - 10f;

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    rect.y = orginY + i * CEIL_HEIGHT + i * CEIL_BORDER;
                    rect.x = orginX + j * CEIL_WIDTH + j * CEIL_BORDER;

                    rect.width = CEIL_WIDTH;
                    rect.height = CEIL_HEIGHT;
                    Color color = (curSelectMaskTexIndex == j) ? new Color(0f, 0.5f, 0f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
                    EditorGUI.DrawRect(rect, color); //BG

                    rect.width = CEIL_WIDTH - 4;
                    rect.height = CEIL_HEIGHT - 4;
                    rect.x += CEIL_PADDING;
                    rect.y += CEIL_PADDING;
                    EditorGUI.DrawPreviewTexture(rect, brushMaskTexList[j]);
                    if (GUI.Button(rect, "", GUIStyle.none))
                    {
                        if (curSelectMaskTexIndex != j)
                        {
                            curSelectMaskTexIndex = j;
                            OnChangeMaskTex();
                        }
                    }
                }
            }

            GUILayoutUtility.GetRect(CEIL_WIDTH, CEIL_HEIGHT + 10);
        }

        private void OnSceneGUI(SceneView obj)
        {
            if (isExitPaint)
            {
                return;
            }
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            switch (e.type)
            {
                case EventType.Repaint:
                    if (isSuckColor)
                    {
                        Handles.color = new Color(Mathf.Pow(paintColor.r, 0.4545f), Mathf.Pow(paintColor.g, 0.4545f), Mathf.Pow(paintColor.b, 0.4545f), 1f);
                        Handles.DrawSolidDisc(hitPos, hitNormal, 0.1f);
                    }
                    else
                    {
                        Handles.color = Color.red;
                        Handles.DrawWireDisc(hitPos, hitNormal, (brushSize * 0.5f * 10f) / 100f);
                        Handles.color = new Color(Mathf.Pow(paintColor.r, 0.4545f), Mathf.Pow(paintColor.g, 0.4545f), Mathf.Pow(paintColor.b, 0.4545f), 1f);
                        Handles.DrawLine(hitPos, hitPos + hitNormal * 0.8f, 1f);
                    }

                    break;
                case EventType.Layout: //屏蔽鼠标左键操作导致的重新绘制
                    HandleUtility.AddDefaultControl(controlID);
                    break;
                case EventType.KeyDown:
                    ProgressKeyDown(e.keyCode);
                    break;
                case EventType.KeyUp:
                    ProgressKeyUp(e.keyCode);
                    break;
                case EventType.MouseDown:
                    if (lightmapTex != null)
                    {
                        Undo.RegisterCompleteObjectUndo(lightmapTex, "Undo LightmapPaint");
                    }

                    if (lightmapCopy != null)
                    {
                        Undo.RegisterCompleteObjectUndo(lightmapCopy, "Undo LightmapCopyPaint");
                    }

                    break;
                case EventType.MouseUp:
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != 0 && GUIUtility.hotControl != controlID)
                    {
                        //屏蔽 鼠标中键和右键拖拽操作
                        //屏蔽 alt+鼠标左键拖拽操作
                        break;
                    }

                    //只允许鼠标左键拖拽操作处理
                    RaycastHit hitInfo = GetRaycast(e.mousePosition);
                    if (hitInfo.collider == null)
                    {
                        break;
                    }

                    ProgressLightmapColor(hitInfo.lightmapCoord, lightmapTex, false);
                    ProgressLightmapColor(hitInfo.lightmapCoord, lightmapCopy, true);
                    break;
                case EventType.MouseMove:
                    hitInfo = GetRaycast(e.mousePosition);
                    if (hitInfo.collider == null)
                    {
                        break;
                    }

                    if (isSuckColor)
                    {
                        paintColor = SuckColor(hitInfo.lightmapCoord, lightmapTex);
                        paintColorCopy = SuckColor(hitInfo.lightmapCoord, lightmapCopy);
                    }

                    break;
            }

            SceneView.RepaintAll();
        }

        private void ProgressKeyDown(KeyCode eKeyCode)
        {
            switch (eKeyCode)
            {
                case KeyCode.P:
                    isSuckColor = true;
                    break;
                case KeyCode.LeftBracket:
                    brushSize -= 1f;
                    brushSize = Mathf.Clamp(brushSize, MIN_BRUSH_SIZE, MAX_BRUSH_SIZE);
                    break;
                case KeyCode.RightBracket:
                    brushSize += 1f;
                    brushSize = Mathf.Clamp(brushSize, MIN_BRUSH_SIZE, MAX_BRUSH_SIZE);
                    break;
            }
        }

        private void ProgressKeyUp(KeyCode eKeyCode)
        {
            switch (eKeyCode)
            {
                case KeyCode.P:
                    isSuckColor = false;
                    break;
            }
        }

        private RaycastHit GetRaycast(Vector2 mousePos)
        {
            RaycastHit hitInfoResult = default;
            if (selectObj == null)
            {
                return hitInfoResult;
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            int hitCount = Physics.RaycastNonAlloc(ray, hitInfos, 100f, layerMask);
            if (hitCount <= 0)
            {
                return hitInfoResult;
            }

            for (int i = 0; i < hitCount; i++)
            {
                if (hitInfos[i].collider == null)
                {
                    continue;
                }

                if (hitInfos[i].collider.gameObject == selectObj)
                {
                    hitInfoResult = hitInfos[i];
                    hitPos = hitInfoResult.point;
                    hitNormal = hitInfoResult.normal;
                    return hitInfoResult;
                }
            }

            return hitInfoResult;
        }

        private void ProgressLightmapColor(Vector2 lightmapUV, Texture2D tex, bool isCopy)
        {
            if (tex == null)
            {
                return;
            }

            int width = tex.width;
            int height = tex.height;

            Color curPaintColor = isCopy ? paintColorCopy : paintColor;

            int texXCenter = Mathf.RoundToInt(lightmapUV.x * width);
            int texYCenter = Mathf.RoundToInt(lightmapUV.y * height);
            int minX = Mathf.Clamp(Mathf.RoundToInt(texXCenter - brushSize), 0, tex.width - 1);
            int minY = Mathf.Clamp(Mathf.RoundToInt(texYCenter - brushSize), 0, tex.height - 1);
            int maxX = Mathf.Clamp(Mathf.RoundToInt(texXCenter + brushSize), 0, tex.width - 1);
            int maxY = Mathf.Clamp(Mathf.RoundToInt(texYCenter + brushSize), 0, tex.height - 1);
            int xLen = maxX - minX;
            int yLen = maxY - minY;
            Vector2 centerPos = new Vector2(texXCenter, texYCenter);
            Vector2 finalPos = centerPos;
            float lerpV = 0;
            for (int x = 0; x < xLen; x++)
            {
                for (int y = 0; y < yLen; y++)
                {
                    int finalX = minX + x;
                    int finalY = minY + y;
                    finalPos.x = finalX;
                    finalPos.y = finalY;
                    lerpV = GetLerpV((x * 1.0f / xLen), (y * 1.0f / yLen));
                    Color orginColor = tex.GetPixel(finalX, finalY);
                    Color finalColor = Color.Lerp(orginColor, curPaintColor, lerpV);
                    finalColor.a = orginColor.a;

                    tex.SetPixel(finalX, finalY, finalColor);
                }
            }

            tex.Apply(false);
        }

        private float GetLerpV(float u, float v)
        {
            if (brushMaskTexCopy == null)
            {
                return 0;
            }

            Color maskColor = brushMaskTexCopy.GetPixelBilinear(u, v);
            maskColor.a = Mathf.Clamp01(maskColor.a * burshHardness);
            return maskColor.a;
        }

        private Color SuckColor(Vector2 lightmapUV, Texture2D tex)
        {
            if (tex == null)
            {
                return Color.white;
            }

            int width = tex.width;
            int height = tex.height;

            int textureX = Mathf.RoundToInt(lightmapUV.x * width);
            int textureY = Mathf.RoundToInt(lightmapUV.y * height);
            return tex.GetPixel(textureX, textureY);
        }

        private void InitData()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.Log("未选择物体");
                return;
            }

            RecoverLightmapName();
            DeleteLightmapCopy();
            selectObj = Selection.activeGameObject;

            Renderer renderer = selectObj.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.Log("未获取到Renderer组件");
                return;
            }

            if (renderer.lightmapIndex < 0)
            {
                Debug.Log("该选择物体没有烘焙光照贴图");
                return;
            }

            layerMask = (1 << selectObj.layer);

            meshCollider = selectObj.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = selectObj.AddComponent<MeshCollider>();
            }

            if (!meshCollider.enabled)
            {
                meshCollider.enabled = true;
            }

            LightmapData[] lightmapDatas = LightmapSettings.lightmaps;
            lightmapTex = lightmapDatas[renderer.lightmapIndex].lightmapColor;
            if (lightmapTex == null)
            {
                Debug.Log("未获取到光照贴图");
                return;
            }

            orginLightmapName = lightmapTex.name;
            lightmapPath = AssetDatabase.GetAssetPath(lightmapTex);
            lightmapCopyPath = lightmapPath.Replace(lightmapTex.name, "~~temp_Copy_" + lightmapTex.name);
            AssetDatabase.CopyAsset(lightmapPath, lightmapCopyPath);
            lightmapCopy = AssetDatabase.LoadAssetAtPath<Texture2D>(lightmapCopyPath);

            AssetDatabase.RenameAsset(lightmapPath, "~~temp_" + lightmapTex.name);
            lightmapPath = AssetDatabase.GetAssetPath(lightmapTex);

            TextureImporter textureImporter = AssetImporter.GetAtPath(lightmapPath) as TextureImporter;
            if (textureImporter == null)
            {
                Debug.Log("TextureImporter为空");
                return;
            }

            textureImporter.isReadable = true;
            TextureImporterPlatformSettings platformSettings = default;
#if UNITY_IOS
            platformSettings = textureImporter.GetPlatformTextureSettings("iPhone");
#elif UNITY_ANDROID
        platformSettings = textureImporter.GetPlatformTextureSettings("Android");
#elif UNITY_EDITOR_WIN
        platformSettings = textureImporter.GetPlatformTextureSettings("Standalone");
#endif

            //The options for the platform string are "Standalone", "Web", "iPhone", "Android", "WebGL", "Windows Store Apps", "PS4", "PSM", "XboxOne", "Nintendo 3DS" and "tvOS".

            platformSettings.format = TextureImporterFormat.RGBA32;
            platformSettings.overridden = true;
            textureImporter.SetPlatformTextureSettings(platformSettings);
            textureImporter.SaveAndReimport();

            textureImporter = AssetImporter.GetAtPath(lightmapCopyPath) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.isReadable = true;


#if UNITY_IOS
            platformSettings = textureImporter.GetPlatformTextureSettings("iPhone");
#elif UNITY_ANDROID
        platformSettings = textureImporter.GetPlatformTextureSettings("Android");
#elif UNITY_EDITOR_WIN
        platformSettings = textureImporter.GetPlatformTextureSettings("Standalone");
#endif
            platformSettings.overridden = true;
            platformSettings.format = TextureImporterFormat.RGB9E5;
            textureImporter.SetPlatformTextureSettings(platformSettings);
            textureImporter.SaveAndReimport();
        }

        private void Apply()
        {
            lightmapTex.Apply(true);
            lightmapCopy.Apply(true);
        }

        private void Baked()
        {
            Apply();

            byte[] bytes = lightmapCopy.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);
            //Assets/Projets/LightmapPaintTool/LightmapPaintTool/Lightmap-0_comp_light.exr
            string writePath = lightmapPath.Replace("Assets", "");
            string path = Application.dataPath + "/" + writePath;
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }
    }
}
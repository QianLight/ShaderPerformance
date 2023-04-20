using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.CFUI;

public class TransCFUITool
{
    [MenuItem("Assets/路径转换/CFUIRes2Path")]
    public static void TransCFUIRes2Path()
    {
        var select = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
        if (select.Length > 0)
        {
            foreach (var item in select)
            {
                var path = AssetDatabase.GetAssetPath(item);
                if (!path.TrimStart().StartsWith("Assets/BundleRes/UI/system") && !path.TrimStart().StartsWith("Assets/BundleRes/UI/common"))
                {
                    EditorUtility.DisplayDialog("warning", "selected object is not ui prefab!", "ok");
                    return;
                }
                Debug.Log("trans ui prefab: " + path);
                GameObject origin = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                GameObject prefab = PrefabUtility.InstantiatePrefab(origin) as GameObject;
                Res2Path(prefab);
                // PrefabUtility.CreatePrefab(path, prefab);
                AssetDatabase.ImportAsset(path);
                GameObject.DestroyImmediate(origin, true);
                GameObject.DestroyImmediate(prefab, true);
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("note", "modify prefab finish!", "ok");
        }
        else
        {
            EditorUtility.DisplayDialog("warning", "you didn't select ui prefab", "ok");
        }
    }

    [MenuItem("Assets/路径转换/FilesRes2Path")]
    public static void TransFilesRes2Path()
    {
        bool success = _TransFilesRes2Path();

        if (success)
        {

            EditorUtility.DisplayDialog("note", "modify ui prefabs finish!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("warning", "Path: Assets\\BundleRes\\UI\\system\\ not Exists.", "OK");
        }
    }

    public static bool _TransFilesRes2Path()
    {
#if UNITY_EDITOR_OSX
        string fullPath = "Assets/BundleRes/UI/system/";
#else
        string fullPath = "Assets\\BundleRes\\UI\\system\\";
#endif

        if (Directory.Exists(fullPath))
        {
            DirectoryInfo direction = new DirectoryInfo(fullPath);

            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (file.Name.EndsWith(".meta"))
                    continue;

                Debug.Log(file.Name);

                var path = file.FullName;
                int index = path.IndexOf(fullPath);
                if (index == -1)
                {
                    EditorUtility.DisplayDialog("warning", "selected object is not ui prefab! " + path, "OK");
                    return false;
                }
                path = path.Substring(index);
                //Debug.Log("trans ui prefab: " + path);

                GameObject origin = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                path = AssetDatabase.GetAssetPath(origin);

                GameObject prefab = PrefabUtility.InstantiatePrefab(origin) as GameObject;
                Res2Path(prefab);
                // PrefabUtility.CreatePrefab(path, prefab);
                AssetDatabase.ImportAsset(path);
                GameObject.DestroyImmediate(origin, true);
                GameObject.DestroyImmediate(prefab, true);
            }
            AssetDatabase.Refresh();
            return true;
        }

        return false;

    }

    [MenuItem("Assets/路径转换/CFUIFindAllUI_AtlasOrPng")]
    public static void TransCFUIFindAllUI_AtlasName()
    {
        var select = Selection.GetFiltered<Object>(SelectionMode.Unfiltered);
        if (select.Length == 1)
        {
            foreach (var item in select)
            {
                SearchUIForSelected(item);
            }
        }
    }

    public static void SearchUIForSelected(Object item)
    {
        if (item != null)
        {
            var path = AssetDatabase.GetAssetPath(item);
            int index = path.TrimStart().IndexOf("atlas");

            if (index == -1)
            {
                index = path.IndexOf("png");
            }

            if (index == -1)
            {
                EditorUtility.DisplayDialog("warning", "selected object is not atlas or png!", "ok");
                return;
            }

            index = path.LastIndexOf("/");
            if (index == -1)
            {
                EditorUtility.DisplayDialog("warning", "selected path is not atlas/xxxx.spriteatlas or /.png !", "ok");
                return;
            }
            string atlasName = path.Substring(index + 1);
            index = atlasName.LastIndexOf(".");
            atlasName = atlasName.Substring(0, index);

            SearchText(atlasName);
        }
    }


    public const string fullPath = "Assets\\BundleRes\\UI\\system\\";
    public static bool SearchText(string searchStr)
    {
        string _DebugStr = string.Empty;
        bool find = false;
        //string fullPath = "Assets\\BundleRes\\UI\\system\\";
        if (Directory.Exists(fullPath))
        {
            DirectoryInfo direction = new DirectoryInfo(fullPath);

            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            int count = 0;
            int uiCount = files.Length;
            foreach (var file in files)
            {
                if (file.Name.EndsWith(".meta"))
                    continue;

                EditorUtility.DisplayProgressBar("搜索图集、图片引用关系", "搜索中...", count * 1.0f / uiCount);
                count++;

                string strPrefabFile = file.FullName;
                try
                {
                    using (FileStream fs = new FileStream(strPrefabFile, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buff = new byte[fs.Length];
                        fs.Read(buff, 0, (int)fs.Length);
                        string strText = System.Text.Encoding.Default.GetString(buff);
                        if (strText.IndexOf(searchStr) != -1)
                        {
                            _DebugStr += strPrefabFile + "\r\n";
                            find = true;
                        }
                    }
                }
                catch (System.Exception)
                {
                    EditorUtility.DisplayDialog("Error", "read Prefab error ! UI :" + strPrefabFile, "ok");
                    return false;
                }

            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("查找", find ? _DebugStr : "没有用到该图集、图片、字体的UI", "ok");
            return true;
        }
        else
        {
            EditorUtility.DisplayDialog("查找", "UI资源路径不对。", "ok");
            return false;
        }
    }



    private static void Res2Path(GameObject go)
    {
        if (go == null)
            return;
        UnityEngine.CFEventSystems.UIBehaviour[] behs = go.transform.GetComponentsInChildren<UnityEngine.CFEventSystems.UIBehaviour>(true);
        for (int i = 0; i < behs.Length; i++)
        {
            var beh = behs[i];

            if (beh is CFImage)
            {
                ImageRes2Path(beh as CFImage, go);
            }

            if (beh is CFRawImage)
            {
                RawImageRes2Path(beh as CFRawImage, go);
            }

            if (beh is CFToggle)
            {
                ToggleRes2Path(beh as CFToggle, go);
            }
        }
    }

    private static void ImageRes2Path(CFImage image, GameObject go)
    {
        if (image.sprite != null)
        {
            string path = GetPath(image.sprite, ".png");

            if (!string.IsNullOrEmpty(path))
            {
                string spriteName = string.Empty;
                string atlasName = string.Empty;
                GetAtlasAndSpriteFromPath(path, ref spriteName, ref atlasName, go);
                image.m_SpriteName = spriteName;
                image.m_AtlasName = atlasName;

                image.material.mainTexture = null;
                image.sprite = null;
            }
        }
    }

    private static void GetAtlasAndSpriteFromPath(string path, ref string spriteName, ref string atlasName, GameObject go)
    {
        if (!string.IsNullOrEmpty(path))
        {
            int last_index = path.LastIndexOf('/');
            if (last_index == -1)
            {
                Debug.LogError("TransCFUITool SavePath Error, get sprite name fail!  path: " + path + "  prefab: " + go.name);
                return;
            }
            spriteName = path.Substring(last_index + 1);

            string pre = path.Substring(0, last_index);

            last_index = pre.LastIndexOf('/');
            if (last_index == -1)
            {
                Debug.LogError("TransCFUITool SavePath Error , get atlas name fail! path: " + path + "  prefab: " + go.name);
                return;
            }
            atlasName = pre.Substring(last_index + 1);
        }
        else
        {
            spriteName = string.Empty;
            atlasName = string.Empty;
        }
    }

    private static void RawImageRes2Path(CFRawImage rimge, GameObject go)
    {
        if (rimge.texture != null)
        {
            string path = GetPath(rimge.texture, ".png");
            rimge.m_TexPath = path;
            rimge.material.mainTexture = null;
            rimge.texture = null;
        }
    }

    private static void ToggleRes2Path(CFToggle toggle, GameObject go)
    {
        //if (toggle != null && toggle.transition == UnityEngine.CFUI.Selectable.Transition.SpriteSwap)
        //{
        //    UnityEngine.CFUI.SpriteState spriteState = toggle.spriteState;

        //    string path = GetPath(spriteState.highlightedSprite, ".png");

        //    string highlightedSpriteName = string.Empty;
        //    string highlightedAtlasName = string.Empty;
        //    GetAtlasAndSpriteFromPath(path, ref highlightedSpriteName, ref highlightedAtlasName, go);
        //    spriteState.m_HighlightedSpriteName = highlightedSpriteName;
        //    spriteState.m_HighlightedAtlasName = highlightedAtlasName;

        //    path = GetPath(spriteState.pressedSprite, ".png");

        //    string pressedSpriteName = string.Empty;
        //    string pressedAtlasName = string.Empty;
        //    GetAtlasAndSpriteFromPath(path, ref pressedSpriteName, ref pressedAtlasName, go);
        //    spriteState.m_PressedSpriteName = pressedSpriteName;
        //    spriteState.m_PressedAtlasName = pressedAtlasName;

        //    path = GetPath(spriteState.disabledSprite, ".png");

        //    string disabledSpriteName = string.Empty;
        //    string disabledAtlasName = string.Empty;
        //    GetAtlasAndSpriteFromPath(path, ref disabledSpriteName, ref disabledAtlasName, go);
        //    spriteState.m_DisabledSpriteName = disabledSpriteName;
        //    spriteState.m_DisabledAtlasName = disabledAtlasName;
        //}
    }

    private static string GetPath(Object obj, string fixname)
    {
        if (obj == null) return "";

        if (obj.name.Contains("Clone"))
        {
            //Debug.Log("Clone");
            return string.Empty;
        }

        string path = AssetDatabase.GetAssetPath(obj);
        path = path.Replace("Assets/BundleRes/", "");
        path = path.Replace(fixname, "");
        return path;
    }

    [InitializeOnLoadMethod]
    static void StartInitializeOnLoadMethod()
    {
        PrefabUtility.prefabInstanceUpdated = delegate (GameObject go)
        {
            //if (go.GetComponent<RectTransform>())
            //{
            //    //string assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(go));
            //    //Debug.Log(assetPath);

            //    UnityEngine.CFEventSystems.UIBehaviour[] behs = go.transform.GetComponentsInChildren<UnityEngine.CFEventSystems.UIBehaviour>(true);
            //    for (int i = 0; i < behs.Length; i++)
            //    {
            //        var beh = behs[i];

            //        if (beh is CFImage)
            //        {
            //            CFImage img = beh as CFImage;
            //            //img.Bind(img.m_SpriteName, img.m_AtlasName);
            //            img.LoadImg();
            //        }

            //        if (beh is CFRawImage)
            //        {
            //            CFRawImage img = beh as CFRawImage;
            //            //img.SetTexturePath(img.m_TexPath);
            //            img.LoadResource();
            //        }

            //        if (beh is CFToggle && (beh as CFToggle).transition == UnityEngine.CFUI.Selectable.Transition.SpriteSwap)
            //        {
            //            var toggle = beh as UnityEngine.CFUI.Selectable;
            //            toggle.LoadSpriteState();
            //        }
            //    }
            //}
        };
    }
}
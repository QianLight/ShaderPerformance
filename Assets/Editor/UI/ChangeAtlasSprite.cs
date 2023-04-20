using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using UnityEngine.CFUI;

public class ChangeAtlasSprite : MonoBehaviour
{
    [MenuItem(@"XEditor/Tools/ChangeAtlasSprite")]
    static void Execute()
    {
        EditorWindow.GetWindowWithRect<ChangeAtlasSpriteEditor>(new Rect(300, 300, 350, 350), true, @"ChangeAtlasSprite");
    }
}
[ExecuteInEditMode]
internal class ChangeAtlasSpriteEditor : EditorWindow
{
    private Sprite _CurrentSprite;
    private Sprite _TargetSprite;

    void OnGUI()
    {
        _CurrentSprite = EditorGUILayout.ObjectField(_CurrentSprite, typeof(Sprite), true) as Sprite;
        _TargetSprite = EditorGUILayout.ObjectField(_TargetSprite, typeof(Sprite), true) as Sprite;

        if (GUILayout.Button("替换", GUILayout.MaxWidth(80)))
        {
            if (_CurrentSprite != null && _TargetSprite != null)
            {
                ChangeAtlasSprite();
            }
            else
            {
                ShowNotification(new GUIContent("替换参数不完整！！！"));
            }
        }
    }

    private void ChangeAtlasSprite()
    {
        foreach (Object o in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            string path = AssetDatabase.GetAssetPath(o);
            if (path.Contains(".prefab"))
            {
                bool change = false;
                GameObject go = PrefabUtility.InstantiatePrefab(o) as GameObject;

                CFImage[] imageList = go.GetComponentsInChildren<CFImage>(true);
                foreach (CFImage image in imageList)
                {
                    if (image.sprite == _CurrentSprite)
                    {
                        change = true;
                        image.sprite = _TargetSprite;
                    }
                }

                if (change)
                {
                    Debug.Log(go.name);
#if UNITY_2019_1_OR_NEWER
                    PrefabUtility.SaveAsPrefabAsset(go, path);
#else
                    Object prefab = (o == null) ? PrefabUtility.CreateEmptyPrefab(AssetDatabase.GetAssetPath(o)) : (o as GameObject);

                    PrefabUtility.ReplacePrefab(go, prefab);
#endif
                }
                DestroyImmediate(go);
                
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ShowNotification(new GUIContent("替换成功！！！"));
    }
}
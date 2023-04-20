using System.Linq;
using UnityEngine;
using UnityEditor;

/*
  *  表情工具
  *  对timeline使用到的表情动画进行清理、修改工具
  */
public class ExpressAnimTool
{

    [MenuItem("Assets/Timeline/ExpressionClean")]
    public static void CleanSelections()
    {
        var objs = Selection.objects;
        var anims = objs.Where(x => x is AnimationClip);
        foreach (var ani in anims)
        {
            Debug.Log(ani.name);
            Clean(ani as AnimationClip);
        }
    }

    public static void Clean(string path)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        Clean(clip);
    }

    public static void Clean(AnimationClip clip)
    {
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        for (int i = 0; i < bindings.Length; i++)
        {
            var bind = bindings[i];
            Debug.Log(bind.propertyName + " " + bind.type);

            if (bindings[i].type != typeof(SkinnedMeshRenderer))
            {
                AnimationUtility.SetEditorCurve(clip, bind, null);
            }
        }

        AssetDatabase.SaveAssets();
    }



    /*
     *  对嘴型-语音的支持 
     */
    public static void ApplyMouseClip(string path)
    {

    }

}

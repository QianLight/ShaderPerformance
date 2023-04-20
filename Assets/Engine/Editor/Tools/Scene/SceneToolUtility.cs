using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CFEngine.Editor
{
    public enum ESceneTool
    {
        None = 0,
        SceneList,
        SceneEdit,
        LayerBrush,
        LayerObject,        
        Num
    }

    public class SceneToolUtility
    {
        private static SceneToolUtility g_SceneToolUtility;

        public static SceneToolUtility Instance
        {
            get
            {
                if (g_SceneToolUtility == null)
                    g_SceneToolUtility = new SceneToolUtility ();
                return g_SceneToolUtility;
            }
        }

        public SceneToolUtility ()
        {
        }

        public static CommonToolTemplate GetToolInstance (ESceneTool tool)
        {
            switch (tool)
            {
                case ESceneTool.LayerBrush:
                    return ScriptableObject.CreateInstance<LayerBrushTool> ();
                case ESceneTool.LayerObject:
                    return ScriptableObject.CreateInstance<LayerObjectTool> ();
                case ESceneTool.SceneEdit:
                    return ScriptableObject.CreateInstance<SceneEditTool> ();
                case ESceneTool.SceneList:
                    return ScriptableObject.CreateInstance<SceneListTool> ();
                    
            }

            return null;
        }

        public static SceneConfig GetCurrentSceneConfig()
        {
            SceneContext sceneContext = new SceneContext();
            SceneAssets.GetCurrentSceneContext(ref sceneContext);
            string sceneConfigPath = string.Format("{0}/{1}{2}.asset",
                sceneContext.configDir, sceneContext.name, SceneContext.SceneConfigSuffix);
            return AssetDatabase.LoadAssetAtPath<SceneConfig>(sceneConfigPath);
        }
        public void OnEnable()
        {

        }

        public void OnDisable ()
        {
        }

        

        public void SaveEditorSceneData ()
        {

        }
    }
}
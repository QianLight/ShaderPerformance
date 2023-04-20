using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;

namespace CFEngine.Editor
{
    static class ResourceAssetFactory
    {
#if !POSTFX_DEBUG_MENUS
        [MenuItem("Tools/Post-processing/Create Resources Asset")]
#endif
        static void CreateAsset()
        {
            var asset = ScriptableObject.CreateInstance<PostProcessResources>();
            AssetDatabase.CreateAsset(asset, "Assets/PostProcessResources.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        public static string GetPostprocessResDir(out string relativeDir,out string editorDir)
        {
            var path = AssetsConfig.instance.ResourcePath;

            relativeDir = "";
            editorDir = path;
            if (Selection.activeGameObject != null)
            {
                Scene scene = Selection.activeGameObject.scene;
                RenderLayer layer = Selection.activeGameObject.GetComponent<RenderLayer>();
                if (layer != null)
                {
                    // PostProcessResources resource = layer.GetResource();
                    // if (resource != null)
                    // {
                    //     path += "/" + resource.postprocessResRoot;
                    //     relativeDir = resource.postprocessResRoot;
                    // }
                }
                var pathTmp = path;
                string scenePath = scene.path;
                string sceneName = AssetsPath.GetParentFolderName(scenePath);
                path += "/" + sceneName;
                relativeDir += "/" + sceneName;
                if (!AssetDatabase.IsValidFolder(path))
                    AssetDatabase.CreateFolder(pathTmp, sceneName);

                editorDir = path + "/Editor";
                if (!AssetDatabase.IsValidFolder(editorDir))
                    AssetDatabase.CreateFolder(path, "Editor");
            }

            return path;
        }


    }
}

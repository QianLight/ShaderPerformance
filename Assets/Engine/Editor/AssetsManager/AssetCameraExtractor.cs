using CFUtilPoolLib;
using ClientEcsData;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    class AssetCameraExtractor
    {
        [MenuItem(@"Assets/Tool/Fbx_CameraFovCurveExtractor")]
        private static void CameraFovCurveExtractor()
        {
            CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
            {
                string folderName;
                if (AssetsPath.GetCreatureFolderName(path, out folderName))
                {
                    if (!Directory.Exists(AssetsConfig.instance.ResourceAnimationPath))
                    {
                        AssetDatabase.CreateFolder(AssetsConfig.instance.ResourcePath, AssetsConfig.instance.ResourceAnimation);
                    }

                    UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(path);
                    string parentPath = "Assets/BundleRes/Curve";
                    string targetPath = string.Format("{0}/{1}", parentPath, folderName);

                    if (!Directory.Exists(targetPath))
                    {
                        AssetDatabase.CreateFolder(parentPath, folderName);
                    }

                    foreach (UnityEngine.Object o in allObjects)
                    {
                        AnimationClip oClip = o as AnimationClip;

                        if (oClip == null || oClip.name.StartsWith("__preview__Take 001")) continue;
                        EditorCurveBinding[] data = AnimationUtility.GetCurveBindings(oClip);
                        XCameraCurveObj camData = new XCameraCurveObj();
                        for (int i = 0; i < data.Length; ++i)
                        {
                            switch (data[i].path)
                            {
                                case "Main Camera":
                                    LoadCurveFromAnim(oClip, data[i], ref camData);
                                    break;
                            }
                        }
                        if(camData.fov != null)
                        {
                            string curvePath = string.Format("{0}/{1}.asset", targetPath, oClip.name);
                            AssetDatabase.CreateAsset(camData, curvePath);
                        }
                    }
                }
                return false;
            };
            CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "ExportCameraCurve");
        }
        private static void LoadCurveFromAnim(AnimationClip clip, EditorCurveBinding data, ref XCameraCurveObj animData)
        {
            switch (data.propertyName)
            {
                case "field of view":
                    animData.fov = AnimationUtility.GetEditorCurve(clip, data);
                    break;
            }
        }

    }
}

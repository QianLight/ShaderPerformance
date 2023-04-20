using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using CFEngine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace CFEngine.Editor
{
    public class MeshRenderPair
    {
        public Renderer r;
        public Mesh m;
    }
    internal class FBXAssets
    {
        public static bool export = false;
        public static bool removeUV2 = false;

        public static bool removeUV3 = true;
        public static bool removeUV4 = true;
        public static bool removeColor = false;
        public static bool renameMesh = true;
        public static void ResetAttribute()
        {
            removeUV2 = false;
            removeColor = false;
            renameMesh = true;
        }

        [MenuItem(@"Assets/Tool/Fbx_CreateFbxConfig")]
        private static void Fbx_CreateFbxConfig()
        {
            CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
            {
                string dirpath = Path.GetDirectoryName(path);
                string configPath = string.Format("{0}/{1}.asset", dirpath, fbx.name);
                if (!File.Exists(configPath))
                {
                    BandposeData bd = ScriptableObject.CreateInstance<BandposeData>();
                    bd.fbxRef = fbx;
                    bd.RefreshMaterial(true);
                    CommonAssets.CreateAsset<BandposeData>(dirpath, fbx.name, ".asset", bd);
                }
                return false;
            };
            CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "CreateFbxConfig");
        }

        [MenuItem("Assets/Tool/Fbx_ExportAnim")]
        static void Fbx_ExportAnim()
        {
            Fbx_ExportAnimFuc(true);
        }

        [MenuItem("Assets/Tool/Fbx_ExportAnimCamera")]
        static void Fbx_ExportAnimForCamera()
        {
            Fbx_ExportAnimFucForCamera(true);
        }

        static void Fbx_ExportAnimFucForCamera(bool compress)
        {
            CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
            {
                //Debug.Log(path);
                string folderName;
                if (AssetsPath.GetCreatureFolderName(path, out folderName))
                {
                    //Debug.Log(folderName);
                    if (Directory.Exists(AssetsConfig.instance.ResourceAnimationPath) == false)
                    {
                        AssetDatabase.CreateFolder(AssetsConfig.instance.ResourcePath, AssetsConfig.instance.ResourceAnimation);
                    }
                    if (modelImporter.animationCompression != ModelImporterAnimationCompression.Optimal && compress != false)
                    {
                        modelImporter.animationCompression = ModelImporterAnimationCompression.Optimal;
                        modelImporter.animationPositionError = 0.1f;
                        modelImporter.animationRotationError = 0.2f;
                        modelImporter.animationScaleError = 0.4f;
                        AssetDatabase.ImportAsset(path);
                    }

                    UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(path);

                    string targetPath = string.Format("{0}/{1}", AssetsConfig.instance.ResourceAnimationPath, folderName);

                    int lastSep = path.LastIndexOf('/');
                    int fileExtensionDot = path.LastIndexOf('.');
                    string defaultName = path.Substring(lastSep + 1, fileExtensionDot - lastSep - 1);

                    if (!Directory.Exists(targetPath))
                    {
                        AssetDatabase.CreateFolder(AssetsConfig.instance.ResourceAnimationPath, folderName);
                    }
                    string editorWrapperPath = string.Format("{0}/{1}",
                        AssetsConfig.instance.ResourceEditorAnimationPath,
                        folderName);
                    if (!Directory.Exists(editorWrapperPath))
                    {
                        AssetDatabase.CreateFolder(AssetsConfig.instance.ResourceEditorAnimationPath, folderName);
                    }

                    foreach (UnityEngine.Object o in allObjects)
                    {
                        AnimationClip oClip = o as AnimationClip;

                        if (oClip == null || oClip.name.StartsWith("__preview__Take 001")) continue;
                        int sizeAin = Profiler.GetRuntimeMemorySize(oClip);
                        Debug.Log(oClip.name + "内存大小：" + sizeAin);
                        string clipName = oClip.name;
                        if (clipName == "Take 001") clipName = defaultName;
                        AnimtionWrap animWrap = AnimtionWrap.CreateInstance<AnimtionWrap>();
                        animWrap.clip = oClip;
                        animWrap.name = clipName;
                        CommonAssets.CreateAsset<AnimtionWrap>(editorWrapperPath, clipName, ".asset", animWrap);
                        string copyPath = targetPath + "/" + clipName + ".anim";

                        if (compress)
                        {
                            OptmizeAnimationFloatForCamera(oClip);
                        }

                        AnimationClip newClip = AssetDatabase.LoadMainAssetAtPath(copyPath) as AnimationClip;

                        if (newClip != null)
                        {
                            //EditorUtility.CopySerialized(oClip, newClip);
                            AssetDatabase.DeleteAsset(copyPath);
                        }

                        newClip = new AnimationClip();
                        newClip.name = clipName;

                        CreateCameraClip(oClip, newClip);
                        AssetDatabase.CreateAsset(newClip, copyPath);
                    }
                }

                return false;
            };
            CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "ExportAnim");
        }

        private static UnityEngine.GameObject _dummyObject = null;
        private static UnityEngine.Transform _dummyCamera = null;

        private static void SetupDummy()
        {
            if (_dummyObject == null)
            {
                _dummyObject = UnityEngine.GameObject.Find("DummyCamera");
            }
            if (_dummyObject == null)
            {
                UnityEngine.GameObject go = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(
                    string.Format("{0}/Prefabs/Cinemachine/DummyCamera.prefab", AssetsConfig.instance.ResourcePath));
                _dummyObject = (UnityEngine.GameObject)PrefabUtility.InstantiatePrefab(go);
            }
            if (_dummyObject != null)
            {
                _dummyCamera = _dummyObject.transform.GetChild(0);
            }
        }

        private static void RecordAnim(UnityEngine.AnimationClip clip, float time, out UnityEngine.Vector3 pos, out UnityEngine.Vector3 rot)
        {
            SetupDummy();
            pos = UnityEngine.Vector3.zero;
            rot = UnityEngine.Vector3.zero;
            if (clip != null)
            {
                clip.SampleAnimation(_dummyObject, time);
                UnityEngine.Vector3 forward = -_dummyCamera.right;
                UnityEngine.Quaternion q = UnityEngine.Quaternion.LookRotation(forward, _dummyCamera.up);
                pos = _dummyCamera.position;
                rot = q.eulerAngles;
            }
        }

        //参考PlayableCameraEditor.cs
        static void CreateCameraClip(AnimationClip cameraClip, AnimationClip newClip)
        {
            float dur = (float)cameraClip.length;
            int frames = (int)(30 * dur);
            AnimationCurve p_x = new AnimationCurve();
            AnimationCurve p_y = new AnimationCurve();
            AnimationCurve p_z = new AnimationCurve();
            AnimationCurve r_x = new AnimationCurve();
            AnimationCurve r_y = new AnimationCurve();
            AnimationCurve r_z = new AnimationCurve();
            float _x = 0, _y = 0, _z = 0, _qx = 0, _qy = 0, _qz = 0;
            for (int i = 0; i < frames; i++)
            {
                float t = i / 30.0f;
                UnityEngine.Vector3 pos = UnityEngine.Vector3.zero, rot = UnityEngine.Vector3.zero;
                RecordAnim(cameraClip, t, out pos, out rot);
                //rot += RotOffset;
                //pos += offset;
                //t = t + (float)xclip.start;
                Debug.Log("add frame: " + t + " pos: " + pos + " rot: " + rot);

                Add(pos.x, ref _x, t, p_x);
                Add(pos.y, ref _y, t, p_y);
                Add(pos.z, ref _z, t, p_z);
                Add(rot.x, ref _qx, t, r_x);
                Add(rot.y, ref _qy, t, r_y);
                Add(rot.z, ref _qz, t, r_z);
            }
            newClip.SetCurve("", typeof(UnityEngine.Transform), "localPosition.x", p_x);
            newClip.SetCurve("", typeof(UnityEngine.Transform), "localPosition.y", p_y);
            newClip.SetCurve("", typeof(UnityEngine.Transform), "localPosition.z", p_z);
            newClip.SetCurve("", typeof(UnityEngine.Transform), "localEulerAngles.x", r_x);
            newClip.SetCurve("", typeof(UnityEngine.Transform), "localEulerAngles.y", r_y);
            newClip.SetCurve("", typeof(UnityEngine.Transform), "localEulerAngles.z", r_z);
        }

        private static bool Add(float x, ref float _x, float t, AnimationCurve curve)
        {
            if (Mathf.Abs(x - _x) > 1e-4)
            {
                _x = x;
                var frame = new Keyframe(t, _x, 0, 0);
                curve.AddKey(frame);
                return true;
            }
            return false;
        }



        [MenuItem("Assets/Tool/Fbx_ExportAnim_UnCompress")]
        static void Fbx_ExportAnimUnCompress()
        {
            Fbx_ExportAnimFuc(false);
        }
        static void Fbx_ExportAnimFuc(bool compress)
        {
            CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
            {
                //Debug.Log(path);
                string folderName;
                if (AssetsPath.GetCreatureFolderName(path, out folderName))
                {
                    //Debug.Log(folderName);
                    if (Directory.Exists(AssetsConfig.instance.ResourceAnimationPath) == false)
                    {
                        AssetDatabase.CreateFolder(AssetsConfig.instance.ResourcePath, AssetsConfig.instance.ResourceAnimation);
                    }
                    if (modelImporter.animationCompression != ModelImporterAnimationCompression.Optimal && compress != false)
                    {
                        modelImporter.animationCompression = ModelImporterAnimationCompression.Optimal;
                        modelImporter.animationPositionError = 0.1f;
                        modelImporter.animationRotationError = 0.2f;
                        modelImporter.animationScaleError = 0.4f;
                        AssetDatabase.ImportAsset(path);
                    }
                      
                    UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(path);
        
                    string targetPath = string.Format("{0}/{1}", AssetsConfig.instance.ResourceAnimationPath, folderName);
        
                    int lastSep = path.LastIndexOf('/');
                    int fileExtensionDot = path.LastIndexOf('.');
                    string defaultName = path.Substring(lastSep + 1, fileExtensionDot - lastSep - 1);
        
                    if (!Directory.Exists(targetPath))
                    {
                        AssetDatabase.CreateFolder(AssetsConfig.instance.ResourceAnimationPath, folderName);
                    }
                    string editorWrapperPath = string.Format("{0}/{1}",
                        AssetsConfig.instance.ResourceEditorAnimationPath,
                        folderName);
                    if (!Directory.Exists(editorWrapperPath))
                    {
                        AssetDatabase.CreateFolder(AssetsConfig.instance.ResourceEditorAnimationPath, folderName);
                    }
        
                    foreach (UnityEngine.Object o in allObjects)
                    {
                        AnimationClip oClip = o as AnimationClip;
        
                        if (oClip == null || oClip.name.StartsWith("__preview__Take 001")) continue;
                        int sizeAin = Profiler.GetRuntimeMemorySize(oClip);
                        Debug.Log(oClip.name+"内存大小："+sizeAin);
                        string clipName = oClip.name;
                        if (clipName == "Take 001") clipName = defaultName;
                        AnimtionWrap animWrap = AnimtionWrap.CreateInstance<AnimtionWrap>();
                        animWrap.clip = oClip;
                        animWrap.name = clipName;
                        CommonAssets.CreateAsset<AnimtionWrap>(editorWrapperPath, clipName, ".asset", animWrap);
                        string copyPath = targetPath + "/" + clipName + ".anim";

                        if (compress)
                        {
                            OptmizeAnimation(oClip);
                        }

                        AnimationClip newClip = AssetDatabase.LoadMainAssetAtPath (copyPath) as AnimationClip;

                        if (newClip != null)
                        {
                            EditorUtility.CopySerialized (oClip, newClip);
                            newClip.name = clipName;
                        }
                        else
                        {
                            newClip = new AnimationClip ();
                            newClip.name = clipName;
                            EditorUtility.CopySerialized (oClip, newClip);
                            AssetDatabase.CreateAsset(newClip, copyPath);
                        }

                        if (newClip.name.Contains("_facial_"))
                        {
                            DeleteKeyFrameExceptFacial(newClip); //删除非表情的关键帧
                        }
/*        
                        AnimationClip newClip = new AnimationClip();
                        newClip.name = clipName;
                        //Debug.Log("创建了");
                        //ReduceKeyFrame(newClip, path);
                        if(compress)
                        {
                            OptmizeAnimation(oClip);
                        }
                        EditorUtility.CopySerializedIfDifferent(oClip, newClip);
                        AssetDatabase.CreateAsset(newClip, copyPath);
                        if (newClip.name.Contains("_facial_"))
                        {
                            DeleteKeyFrameExceptFacial(newClip); //删除非表情的关键帧
                        }
*/
                    }
                }
        
                return false;
            };
            CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "ExportAnim");
        }
        public static void Fbx_ExportAnimFuc(GameObject fbx,ModelImporter modelImporter,string path,float rotError = 0.1f,float PosError = 0.1f,float scaError = 0.3f,bool compress = true)
        {
            //AnimationAnewImport
            string folderName;
            Debug.Log("Fbx_ExportAnimFuc进来了");
            if (AssetsPath.GetCreatureFolderName(path, out folderName))
            {
                if (Directory.Exists(AssetsConfig.instance.ResourceAnimationPath) == false)
                {
                    AssetDatabase.CreateFolder(AssetsConfig.instance.ResourcePath, AssetsConfig.instance.ResourceAnimation);
                }
                if (modelImporter.animationCompression != ModelImporterAnimationCompression.Optimal)
                {
                    modelImporter.animationCompression = ModelImporterAnimationCompression.Optimal;
                    modelImporter.animationPositionError = rotError;
                    modelImporter.animationRotationError = PosError;
                    modelImporter.animationScaleError = scaError;
                    AssetDatabase.ImportAsset(path);
                }
                  
                UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(path);
                // 动画片段保存路径
                string targetPath = string.Format("{0}/{1}", AssetsConfig.instance.ResourceAnimationPath, folderName);
        
                int lastSep = path.LastIndexOf('/');
                int fileExtensionDot = path.LastIndexOf('.');
                string defaultName = path.Substring(lastSep + 1, fileExtensionDot - lastSep - 1);
        
                if (!Directory.Exists(targetPath))
                {
                    AssetDatabase.CreateFolder(AssetsConfig.instance.ResourceAnimationPath, folderName);
                }
                string editorWrapperPath = string.Format("{0}/{1}",
                    AssetsConfig.instance.ResourceEditorAnimationPath,
                    folderName);
                if (!Directory.Exists(editorWrapperPath))
                {
                    AssetDatabase.CreateFolder(AssetsConfig.instance.ResourceEditorAnimationPath, folderName);
                }
        
                foreach (UnityEngine.Object o in allObjects)
                {
                    AnimationClip oClip = o as AnimationClip;
        
                    if (oClip == null || oClip.name.StartsWith("__preview__Take 001")) continue;
                    //oClip
                    int sizeAin = Profiler.GetRuntimeMemorySize(oClip);
                    Debug.Log(oClip.name+"内存大小："+sizeAin);
                    string clipName = oClip.name;
                    if (clipName == "Take 001") clipName = defaultName;
                    AnimtionWrap animWrap = AnimtionWrap.CreateInstance<AnimtionWrap>();
                    animWrap.clip = oClip;
                    animWrap.name = clipName;
                    CommonAssets.CreateAsset<AnimtionWrap>(editorWrapperPath, clipName, ".asset", animWrap);
                    string copyPath = targetPath + "/" + clipName + ".anim";
        
                    AnimationClip newClip = new AnimationClip();
                    newClip.name = clipName;
                    //Debug.Log("创建了");
                    //ReduceKeyFrame(newClip, path);
                    if(compress)
                    {
                        OptmizeAnimation(oClip);
                    }
                    EditorUtility.CopySerializedIfDifferent(oClip, newClip);
                    AssetDatabase.CreateAsset(newClip, copyPath);
                    if (newClip.name.Contains("_facial_"))
                    {
                        DeleteKeyFrameExceptFacial(newClip); //删除非表情的关键帧
                    }
                }
            }
        }

        [MenuItem("Assets/Tool/Fbx_CheckHasBoneProperty")]
        static void Fbx_CheckHasBoneProperty()
        {
            AnimationClip clip = Selection.activeObject as AnimationClip;
            EditorCurveBinding[] curveBinding = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < curveBinding.Length; ++i)
            {
                if (curveBinding[i].path.Contains("Bone_cap"))
                {
                    Debug.LogError(curveBinding[i].path);
                    break;
                }
            }
        }

        [MenuItem("Assets/Tool/Fbx_ConvertAnimationVariable")]
        static void Fbx_ConvertAnimationVariable()
        {
            AnimationClip clip = Selection.activeObject as AnimationClip;
            //EditorCurveBinding[] curveBinding = AnimationUtility.GetCurveBindings(clip);
            //for (int i = 0; i < curveBinding.Length; ++i)
            //{
            //    curveBinding[i].propertyName = FacialExpressionInspector.m_propertyDict[curveBinding[i].propertyName];
            //    AnimationUtility.SetEditorCurve(clip, curveBinding[i], null);
            //}
            CloneOneAnimationClip(clip);
        }

        static void CloneOneAnimationClip(AnimationClip source)
        {
            if (source == null) return;
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(source);
            AnimationClip destClip = new AnimationClip();
            destClip.legacy = false;
            for (int i = 0; i < bindings.Length; i++)
            {
                EditorCurveBinding sourceBinding = bindings[i];
                AnimationCurve sourceCurve = AnimationUtility.GetEditorCurve(source, sourceBinding);
                destClip.SetCurve(string.Empty, typeof(FacialExpressionCurve), FacialExpressionInspector.m_propertyDict[sourceBinding.propertyName], sourceCurve);
            }
            AssetDatabase.CreateAsset(destClip, "Assets/BundleRes/Animation/FacialExpression/" + source.name + "2.anim");
        }


        [MenuItem("Assets/Tool/Fbx_DeleteKeyFrameExceptFacial")]
        /// <summary>
        /// 删除除了脸部的其他骨骼的关键帧信息
        /// </summary>
        static void DeleteKeyFrameExceptFacial()
        {
            AnimationClip clip = Selection.activeObject as AnimationClip;
            DeleteKeyFrameExceptFacial(clip);
        }

        static void DeleteKeyFrameExceptFacial(AnimationClip animClip)
        {
            if (animClip == null) return;
            EditorCurveBinding[] curveBinding = AnimationUtility.GetCurveBindings(animClip);
            for (int i = 0; i < curveBinding.Length; ++i)
            {
                var binding = curveBinding[i];
                string path = binding.path;
                string[] strs = path.Split('/');
                if (strs.Length > 0 && !strs[strs.Length - 1].StartsWith("Bone_"))
                {
                    AnimationUtility.SetEditorCurve(animClip, binding, null);
                }
            }
        }

        [MenuItem("Assets/Tool/Fbx_ExportAnimWrap")]
        static void Fbx_ExportAnimWrap()
        {
            CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
            {
                string folderName;
                if (AssetsPath.GetCreatureFolderName(path, out folderName))
                {
                    UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(path);

                    string editorWrapperPath = string.Format("{0}/{1}",
                        AssetsConfig.instance.ResourceEditorAnimationPath,
                        folderName);
                    if (!Directory.Exists(editorWrapperPath))
                    {
                        AssetDatabase.CreateFolder(AssetsConfig.instance.ResourceEditorAnimationPath, folderName);
                    }

                    foreach (UnityEngine.Object o in allObjects)
                    {
                        AnimationClip oClip = o as AnimationClip;

                        if (oClip == null || oClip.name.StartsWith("__preview__Take 001")) continue;

                        AnimtionWrap animWrap = AnimtionWrap.CreateInstance<AnimtionWrap>();
                        animWrap.clip = oClip;
                        CommonAssets.CreateAsset<AnimtionWrap>(editorWrapperPath, oClip.name, ".asset", animWrap);
                    }
                }

                return false;
            };
            CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "ExportAnim");
        }

        private static bool ComputerKeyDerivative(Keyframe preKey, Keyframe midKey, Keyframe currentKey)
        {
            float ddx0 = (midKey.value - preKey.value);
            float ddx1 = (currentKey.value - midKey.value);
            float derivativeValue = Mathf.Abs(ddx1 - ddx0);

            float ddxInTangent0 = midKey.inTangent - preKey.inTangent;
            float ddxInTangent1 = currentKey.inTangent - midKey.inTangent;
            float derivativeInTangent = Mathf.Abs(ddxInTangent1 - ddxInTangent0);

            float ddxOutTangent0 = midKey.outTangent - preKey.outTangent;
            float ddxOutTangent1 = currentKey.outTangent - midKey.outTangent;
            float derivativeOutTangent = Mathf.Abs(ddxOutTangent1 - ddxOutTangent0);

            return derivativeValue < 0.01f &&
                derivativeInTangent < 0.01f &&
                derivativeOutTangent < 0.01f;
        }

        private static void OptmizeAnimationScaleCurve(AnimationClip _clip)
        {
            if (_clip != null)
            {
                foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(_clip))
                {
                    /*
                    string[] paths = theCurveBinding.path.Split(new char[] { '/' });
                    string bone = paths[paths.Length - 1];

                    if (bone.StartsWith("Bip") && theCurveBinding.propertyName.Contains("Scale"))
                    {
                        AnimationUtility.SetEditorCurve(_clip, theCurveBinding, null);
                    }
                    */
                    
                    // 张欣颖 关闭于 2021年9月9日 
                    // 删除scale数据会导致scale值采用其他的状态数据，从而取到了一个错误的值。
                    // BUG单号：https://www.tapd.cn/30772257/prong/stories/edit/1130772257001003953?
                    // 复现方式：
                    //  1. 使用下面代码导出Monster_Blackcat_xiaobing1_idle
                    //  2. 播放技能Monster_BlackCat_xiaobing1_Rage_1
                    // if(theCurveBinding.propertyName.ToLower().Contains("scale"))
                    // {
                    //     AnimationCurve curve = AnimationUtility.GetEditorCurve(_clip, theCurveBinding);
                    //     if (curve.length == 2)
                    //     {
                    //         Keyframe[] keyFrames = curve.keys;
                    //         if (Mathf.Abs(keyFrames[0].value - 1) < 0.001f && Mathf.Abs(keyFrames[1].value - 1) < 0.001f)
                    //         {
                    //             AnimationUtility.SetEditorCurve(_clip, theCurveBinding, null);
                    //         }
                    //     }
                    // }
                }
            }
        }

        private static void OptmizeAnimationFloat(AnimationClip _clip)
        {
            if (_clip != null)
            {
                AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves(_clip);
                Keyframe key;
                Keyframe[] keyFrames;
                string floatFormat = "f3";
                if (curves != null && curves.Length > 0)
                {
                    for (int ii = 0; ii < curves.Length; ++ii)
                    {
                        AnimationClipCurveData curveDate = curves[ii];
                        if (curveDate.curve == null || curveDate.curve.keys == null)
                        {
                            continue;
                        }
                        keyFrames = curveDate.curve.keys;
                        for (int i = 0; i < keyFrames.Length; i++)
                        {
                            key = keyFrames[i];
                            key.value = float.Parse(key.value.ToString(floatFormat));
                            key.inTangent = float.Parse(key.inTangent.ToString(floatFormat));
                            key.outTangent = float.Parse(key.outTangent.ToString(floatFormat));
                            key.inWeight = float.Parse(key.inWeight.ToString(floatFormat));
                            key.outWeight = float.Parse(key.outWeight.ToString(floatFormat));
                            keyFrames[i] = key;
                        }
                        curveDate.curve.keys = keyFrames;
                        _clip.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curveDate.curve);
                    }
                }
            }
        }

        private static void OptmizeAnimationFloatForCamera(AnimationClip _clip)
        {
            if (_clip != null)
            {
                AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves(_clip);
                Keyframe key;
                Keyframe[] keyFrames;
                string floatFormat = "f3";
                if (curves != null && curves.Length > 0)
                {
                    for (int ii = 0; ii < curves.Length; ++ii)
                    {
                        AnimationClipCurveData curveDate = curves[ii];
                        if (curveDate.curve == null || curveDate.curve.keys == null)
                        {
                            continue;
                        }
                        keyFrames = curveDate.curve.keys;
                        for (int i = 0; i < keyFrames.Length; i++)
                        {
                            key = keyFrames[i];
                            key.value = float.Parse(key.value.ToString(floatFormat));
                            key.inTangent = float.Parse(key.inTangent.ToString(floatFormat));
                            key.outTangent = float.Parse(key.outTangent.ToString(floatFormat));
                            key.inWeight = float.Parse(key.inWeight.ToString(floatFormat));
                            key.outWeight = float.Parse(key.outWeight.ToString(floatFormat));
                            keyFrames[i] = key;
                        }
                        curveDate.curve.keys = keyFrames;
                        _clip.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curveDate.curve);
                    }
                }
            }
        }

        private static void OptmizeAnimation(AnimationClip animClip)
        {
            /*
             * 2022、3、18
             * 缩放动画片段占用过多，不进行删除缩放曲线
             * 精度压缩效果最明显
             */
            //OptmizeAnimationScaleCurve(animClip);
            OptmizeAnimationFloat(animClip);
        }

        private static void ReduceKeyFrame(AnimationClip animClip, string path, bool log = false)
        {
            string errorLog = "";
            int reduceCurveCount = 0;
            //List<int> removeIndex = ListPool<int>.Get();
            List<int> removeIndex = new List<int>();
            EditorCurveBinding[] curveBinding = AnimationUtility.GetCurveBindings(animClip);
            for (int i = 0; i < curveBinding.Length; ++i)
            {
                var binding = curveBinding[i];
                AnimationCurve curve = AnimationUtility.GetEditorCurve(animClip, binding);
                if (curve.keys.Length > 1)
                {
                    removeIndex.Clear();
                    bool scale = binding.propertyName.StartsWith("m_LocalScale");
                    bool pos = binding.propertyName.StartsWith("m_LocalPosition");

                    Keyframe[] keys = new Keyframe[curve.keys.Length];
                    for (int j = 0; j < curve.keys.Length; ++j)
                    {
                        Keyframe key = curve.keys[j];
                        if (scale || pos)
                        {
                            key.value = (float)Math.Round(key.value, 4);
                        }
                        else
                        {
                            key.value = key.value;
                        }
                        key.inTangent = (float)Math.Round(key.inTangent, 3);
                        key.outTangent = (float)Math.Round(key.outTangent, 3);
                        keys[j] = key;
                    }

                    Keyframe preKey = keys[0];
                    Keyframe midKey = keys[1];

                    float defaultValue = scale ? 1 : 0;
                    bool isDefaultValue = Mathf.Abs(preKey.value - defaultValue) < 0.01f && Mathf.Abs(midKey.value - defaultValue) < 0.01f;
                    string floatFormat = "f3";
                    for (int j = 2; j < keys.Length; ++j)
                    {
                        Keyframe key = keys[j];
                        key.value = float.Parse(key.value.ToString(floatFormat));
                        key.inTangent = float.Parse(key.inTangent.ToString(floatFormat));
                        key.outTangent = float.Parse(key.outTangent.ToString(floatFormat));

                        if (ComputerKeyDerivative(preKey, midKey, key))
                        {
                            removeIndex.Add(j - 1);
                        }

                        float defaultError = Mathf.Abs(key.value - defaultValue);
                        float defaultErrorPercent = scale ? defaultError / defaultValue : defaultError;
                        if (defaultErrorPercent > 0.01f)
                            isDefaultValue = false;

                        preKey = midKey;
                        midKey = key;
                    }
                    curve.keys = keys;

                    if (isDefaultValue)
                    {
                        if (binding.propertyName == "m_LocalPosition.x" ||
                            binding.propertyName == "m_LocalPosition.y" ||
                            binding.propertyName == "m_LocalPosition.z" ||
                            binding.propertyName == "m_LocalScale.x" ||
                            binding.propertyName == "m_LocalScale.y" ||
                            binding.propertyName == "m_LocalScale.x")
                        {
                            //Debug.Log ("not opt curve" + binding.path);
                        }
                        else
                        {
                            AnimationUtility.SetEditorCurve(animClip, binding, null);
                            reduceCurveCount++;
                            if (!(pos || scale))
                            {
                                errorLog += string.Format("{0}:{1}\r\n", binding.path, binding.propertyName);
                            }
                        }
                    }
                    else
                    {
                        for (int j = removeIndex.Count - 1; j >= 0; --j)
                        {
                            curve.RemoveKey(removeIndex[j]);
                        }
                        if (removeIndex.Count > 0)
                            reduceCurveCount++;

                        AnimationUtility.SetEditorCurve(animClip, binding, curve);
                    }

                }
                else
                {
                    AnimationUtility.SetEditorCurve(animClip, binding, null);
                }
            }
            Debug.LogWarning(string.Format("{0} reduceCurveCount/total:{1}/{2}\r\n{3}", path, reduceCurveCount, curveBinding.Length, errorLog));
        }

        [MenuItem("Assets/Tool/Fbx_ReduceKeyFrame")]
        private static void ReduceKeyFrame()
        {
            CommonAssets.enumAnimationClip.cb = (animClip, path, context) =>
            {
                ReduceKeyFrame(animClip, path, true);
                //ListPool<int>.Release(removeIndex);
            };
            CommonAssets.EnumAsset<AnimationClip>(CommonAssets.enumAnimationClip, "ReduceKeyFrame");
        }

        public static Mesh GetMesh(Renderer renderer)
        {
            if (renderer is MeshRenderer)
            {
                MeshFilter mf = renderer.GetComponent<MeshFilter>();
                return mf != null ? mf.sharedMesh : null;
            }
            else if (renderer is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer smr = renderer as SkinnedMeshRenderer;
                return smr != null ? smr.sharedMesh : null;
            }
            return null;
        }
         
        public static void GetMesh(Renderer render, out Mesh newMesh, ref ExportContext context)
        {
            Mesh mesh = GetMesh(render);

            newMesh = null;
            if (mesh != null)
            {
                newMesh = UnityEngine.Object.Instantiate<Mesh>(mesh);
                newMesh.name = mesh.name;
                if (context.preExportCb != null)
                {
                    context.preExportCb(newMesh, render, ref context);
                }
                if (!context.flagOverride.HasFlag(ExportContext.Flag_KeepUV2))
                {
                    newMesh.uv2 = null;
                }
                if (!context.flagOverride.HasFlag(ExportContext.Flag_KeepColor))
                    newMesh.colors = null;
                if (!string.IsNullOrEmpty(context.srcDir))
                {
                    string vcmeshPath = string.Format("{0}/{1}_{2}_vc.asset", context.srcDir, context.baseName, mesh.name);
                    if (File.Exists(vcmeshPath))
                    {
                        var vcmesh = AssetDatabase.LoadAssetAtPath<Mesh>(vcmeshPath);
                        if (vcmesh.vertexCount == mesh.vertexCount)
                        {
                            newMesh.colors = vcmesh.colors;
                        }
                        else
                        {
                            DebugLog.AddErrorLog2("fbx:{0} mesh:{1} vertex count not same with vc mesh:{2}",
                                context.baseName, mesh.name, vcmeshPath);
                        }

                    }
                }
                if (!context.flagOverride.HasFlag(ExportContext.Flag_KeepUV3))
                    newMesh.uv3 = null;
                if (!context.bandpose || (!context.bandpose.facemapParts.Contains(mesh.name) && !context.bandpose.cloakParts.Contains(mesh.name)))//!mesh.name.ToLower().Contains("_cloth")
                    newMesh.uv4 = null;
                //if (mirror)
                //{
                //    int[] index = newMesh.triangles;
                //    for (int i = 0; i < index.Length; i += 3)
                //    {
                //        int tmp = index[i + 2];
                //        index[i + 2] = index[i + 1];
                //        index[i + 1] = tmp;
                //    }
                //    newMesh.triangles = index;
                //}
                if (context.rot90)
                {
                    Matrix4x4 obj2world = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);
                    Vector3[] vertices = newMesh.vertices;
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = obj2world.MultiplyPoint(vertices[i]);
                    }
                    newMesh.vertices = vertices;
                }
            }
        }
        public static void GetMeshMat(Renderer render, out Mesh newMesh, out Material newMat,
            bool exportMesh = true, bool exportMat = true, bool mirror = false, MeshOptimizeConfig config = null,
            string dir = "", string fbxName = "")
        {
            Mesh mesh = null;
            Material mat = null;
            newMat = null;
            newMesh = null;
            if (render is MeshRenderer)
            {
                MeshRenderer mr = render as MeshRenderer;
                MeshFilter mf = render.GetComponent<MeshFilter>();
                mesh = mf != null ? mf.sharedMesh : null;
                mat = mr.sharedMaterial;
            }
            else if (render is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer smr = render as SkinnedMeshRenderer;
                mesh = smr != null ? smr.sharedMesh : null;
                mat = smr.sharedMaterial;
            }
            if (exportMesh && mesh != null)
            {
                newMesh = UnityEngine.Object.Instantiate<Mesh>(mesh);
                newMesh.name = mesh.name;
                if (config != null)
                {
                    if (config.removeUV2)
                        newMesh.uv2 = null;

                    if (config.removeColor)
                        newMesh.colors = null;
                }
                else
                {
                    if (removeUV2)
                        newMesh.uv2 = null;

                    if (removeColor)
                        newMesh.colors = null;
                }
                if (!string.IsNullOrEmpty(dir))
                {
                    string vcmeshPath = string.Format("{0}/{1}_{2}_vc.asset", dir, fbxName, mesh.name);
                    if (File.Exists(vcmeshPath))
                    {
                        var vcmesh = AssetDatabase.LoadAssetAtPath<Mesh>(vcmeshPath);
                        if (vcmesh.vertexCount == mesh.vertexCount)
                        {
                            newMesh.colors = vcmesh.colors;
                        }
                        else
                        {
                            DebugLog.AddErrorLog2("fbx:{0} mesh:{1} vertex count not same with vc mesh:{2}", fbxName, mesh.name, vcmeshPath);
                        }

                    }
                }
                if (removeUV3)
                    newMesh.uv3 = null;
                if (removeUV4)
                    newMesh.uv4 = null;
                if (mirror)
                {
                    int[] index = newMesh.triangles;
                    for (int i = 0; i < index.Length; i += 3)
                    {
                        int tmp = index[i + 2];
                        index[i + 2] = index[i + 1];
                        index[i + 1] = tmp;
                    }
                    newMesh.triangles = index;
                }
                if (config != null && config.rotate90)
                {
                    Matrix4x4 obj2world = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);
                    Vector3[] vertices = newMesh.vertices;
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = obj2world.MultiplyPoint(vertices[i]);
                    }
                    newMesh.vertices = vertices;
                }
                MeshUtility.SetMeshCompression(newMesh, ModelImporterMeshCompression.Low);
                MeshUtility.Optimize(newMesh);
                bool outputReadable = config != null ? !config.exportReadable : true;
                Debug.Log($"OutputReadable:{outputReadable}");
                newMesh.UploadMeshData(outputReadable);
            }
            if (exportMat && mat != null)
            {
                newMat = new Material(mat);
                newMat.name = mat.name;
                MaterialShaderAssets.ClearMat(newMat);
            }
        }

        public delegate bool MeshExportCb(Mesh m, Renderer render, bool post, BandposeData bandpose);
        public delegate void MeshPreExportCb(Mesh srcMesh, Renderer render, ref ExportContext context);
        public struct ExportContext
        {
            public string exportDir;
            public string baseName;
            public MeshExportCb exportCb;
            public MeshPreExportCb preExportCb;
            public string partFliter;
            public MeshOptimizeConfig config;
            public bool renameMesh;
            public string srcDir;
            public bool readable;
            public bool rot90;
            public FlagMask flag;
            public FlagMask flagOverride;
            public BandposeData bandpose;

            public static uint Flag_KeepUV2 = 0x00000001;
            public static uint Flag_KeepColor = 0x00000002;
            public static uint Flag_KeepUV3 = 0x00000004;
            public static uint Flag_KeepUV4 = 0x00000008;
            public static uint Flag_UnCompress = 0x00000010;
        }

        internal static bool ExportMesh(GameObject prefab, string assetPath, ref ExportContext context, EditorMessageType errorLog)
        {
            if (CheckModelSubAssetNames(prefab, errorLog))
                return false;

            if (!string.IsNullOrEmpty(assetPath))
                context.srcDir = AssetsPath.GetDir(assetPath);
            List<Renderer> renders = EditorCommon.GetRenderers(prefab);
            context.baseName = context.baseName.ToLower();
            int lodLevel = 0;
            if (context.baseName.EndsWith("_lod1"))
            {
                context.baseName = context.baseName.Replace("_lod1", "");
                lodLevel = 1;
            }
            else if (context.baseName.EndsWith("_lod2"))
            {
                context.baseName = context.baseName.Replace("_lod2", "");
                lodLevel = 2;
            }

            for (int i = 0; i < renders.Count; ++i)
            {
                Renderer render = renders[i];
                if (string.IsNullOrEmpty(context.partFliter) || context.partFliter == render.name)
                {
                    GetMesh(render, out Mesh newMesh, ref context);

                    if (newMesh != null)
                    {
                        string meshName = newMesh.name;
                        if (context.renameMesh)
                        {
                            switch (lodLevel)
                            {
                                case 0:
                                    {
                                        meshName = string.Format("{0}_{1}", context.baseName, i.ToString());
                                    }
                                    break;
                                case 1:
                                    {
                                        meshName = string.Format("{0}_{1}_lod1", context.baseName, i.ToString());
                                    }
                                    break;
                                case 2:
                                    {
                                        meshName = string.Format("{0}_{1}_lod2", context.baseName, i.ToString());
                                    }
                                    break;
                            }
                        }
                        newMesh.name = meshName;
                        bool export = true;
                        if (context.exportCb != null)
                            export = context.exportCb(newMesh, render, false, context.bandpose);
                        Mesh m = null;
                        if (export)
                        {
                            MeshUtility.SetMeshCompression(newMesh, context.flagOverride.HasFlag(ExportContext.Flag_UnCompress) ?
                                ModelImporterMeshCompression.Off : ModelImporterMeshCompression.Low);
                            MeshUtility.Optimize(newMesh);
                            newMesh.UploadMeshData(!context.readable);

                            string exportPath = string.Format("{0}/{1}.asset", context.exportDir, meshName);

                            m = EditorCommon.CreateAsset<Mesh>(exportPath, ".asset", newMesh);
                        }
                        else
                        {
                            m = newMesh;
                        }
                        if (context.exportCb != null)
                            context.exportCb(m, render, true, context.bandpose);

                    }
                }

            }

            return true;
        }

        internal static void ExportMeshMat(GameObject prefab, string dir, MeshOptimizeConfig config = null, bool changeMeshName = false, bool exportMat = false, bool instantiate = true, bool mirror = false)
        {
            GameObject go = instantiate ? PrefabUtility.InstantiatePrefab(prefab) as GameObject : prefab;
            List<Renderer> renders = EditorCommon.GetRenderers(prefab);
            for (int i = 0; i < renders.Count; ++i)
            {
                Renderer render = renders[i];
                Mesh newMesh = null;
                Material newMat = null;
                GetMeshMat(render, out newMesh, out newMat, true, true, mirror, config);
                if (newMesh != null)
                {
                    string meshName = newMesh.name;
                    if (changeMeshName)
                    {
                        meshName += "_" + i.ToString();
                    }
                    if (mirror)
                    {
                        meshName += "_mirror";
                    }
                    newMesh.name = meshName;
                    CommonAssets.CreateAsset<Mesh>(dir, meshName, ".asset", newMesh);
                }
                if (newMat != null && exportMat)
                {
                    CommonAssets.CreateAsset<Material>(dir, newMat.name, ".mat", newMat);
                }
            }
            if (instantiate)
                GameObject.DestroyImmediate(go);
        }
        internal static Avatar ExportAvatar(GameObject prefab, string dir, string name)
        {
            Animator ator;
            if (prefab.TryGetComponent(out ator))
            {
                var avatar = ator.avatar;
                if (avatar != null)
                {
                    var newAvatar = UnityEngine.Object.Instantiate(avatar);
                    newAvatar.name = name;
                    string desPath = string.Format("{0}/{1}.asset", dir, name);
                    if (File.Exists(desPath))
                    {
                        AssetDatabase.DeleteAsset(desPath);
                    }
                    EditorCommon.SaveAsset(desPath, ".asset", newAvatar);
                    return newAvatar;
                }
            }
            return null;
        }

        public static void ProcessFbx(string assetPath, ModelImporter modelImporter, EditorMessageType errorLog, bool exportMesh = false)
        {
            MeshOptimizeConfig config = FindMeshConfig(assetPath);
            if (config != null)
            {
                ProcessModelImporterByConfig(modelImporter, config);

                if (exportMesh)
                {
                    if (!string.IsNullOrEmpty(config.exportDir)&&config.isExport)
                    {
                        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        if (fbx != null)
                        {
                            ExportContext ec = new ExportContext();
                            ec.exportDir = config.exportDir;
                            ec.baseName = fbx.name;
                            ec.config = config;
                            ec.flagOverride.SetFlag(ExportContext.Flag_KeepUV2, !config.removeUV2);
                            ec.flagOverride.SetFlag(ExportContext.Flag_KeepColor, !config.removeColor);
                            ec.renameMesh = renameMesh;
                            ExportMesh(fbx, assetPath, ref ec, errorLog);
                        }
                    }
                }
            }
            //else
            //{
            //    if (modelImporter != null)
            //        modelImporter.isReadable = false;
            //}
        }

        public static void ProcessModelImporterByConfig(ModelImporter modelImporter, MeshOptimizeConfig config)
        {
            if (modelImporter == null || modelImporter == null)
                return;
            
            modelImporter.isReadable = config.isReadable;
            modelImporter.resampleCurves = config.resample;
            modelImporter.importCameras = config.importCamera;
            if (config.overrideImportNormals)
            {
                modelImporter.importNormals = config.importNormal;
            }

            if (config.overrideImportTangents)
            {
                modelImporter.importTangents = config.importTangent;
            }

            if (config.overrideImportBlendShapeNormals)
            {
                modelImporter.importBlendShapeNormals = config.importBlendShapeNormals;
            }
        }

        public static MeshOptimizeConfig FindMeshConfig(string assetPath)
        {
            AssetsPath.GetFileName(assetPath, out string fileName);
            fileName = fileName.ToLower();
            
            List<MeshOptimizeConfig> configs = AssetsConfig.instance.meshConfig.meshConfigs;
            foreach (MeshOptimizeConfig config in configs)
            {
                foreach (string filter in config.filters)
                {
                    if (CommonAssets.StrFilter(filter, assetPath, fileName))
                    {
                        return config;
                    }
                }
            }

            return null;
        }

        private static void ApplyModfy(string path)
        {
            AssetDatabase.WriteImportSettingsIfDirty(path);
            AssetDatabase.StartAssetEditing();
            AssetDatabase.ImportAsset(path);
            AssetDatabase.StopAssetEditing();
        }

        [MenuItem("Assets/Tool/Fbx_ExportSceneMesh")]
        static void Fbx_ExportSceneMesh()
        {
            CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
            {
                ProcessFbx(path, modelImporter, EditorMessageType.LogError, true);
                return false;
            };
            CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "ExportSceneMesh");
        }

        [MenuItem("Assets/Tool/Fbx_ExportMesh")]
        static void Fbx_ExportMesh()
        {
            CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
            {
                ExportMeshMat(fbx, Path.GetDirectoryName(path), null, true);
                return false;
            };
            CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "ExportMesh");
        }
        
        [MenuItem("Assets/Tool/Fbx_ExportMeshReadable")]
        static void Fbx_ExportMeshReadable()
        {
            Debug.Log("RunReadable");
            CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
            {
                MeshOptimizeConfig config = AssetsConfig.instance.meshConfig.meshConfigs.Find(
                    delegate(MeshOptimizeConfig optimizeConfig) { return optimizeConfig.name == "SFXMesh"; });
                Debug.Log(config.exportReadable);
                ExportMeshMat(fbx, Path.GetDirectoryName(path), config, true);
                return false;
            };
            CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "ExportMesh");
        }

        //[MenuItem ("Assets/Tool/Fbx_ExportMeshGrass")]
        //static void Fbx_ExportMeshGrass ()
        //{
        //    CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
        //    {
        //        String grassExportPath = "Assets/BundleRes/EditorAssetRes";
        //        removeUV3 = false;
        //        removeColor = true;
        //        ExportMeshMat (fbx, grassExportPath, true);
        //        removeUV3 = true;
        //        removeColor = false;
        //        return false;

        //    };
        //    CommonAssets.EnumAsset<GameObject> (CommonAssets.enumFbx, "ExportMesh");
        //}

        [MenuItem("Assets/Tool/Fbx_ExportMeshMirror")]
        static void Fbx_ExportMeshMirror()
        {
            CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
            {
                ExportMeshMat(fbx, Path.GetDirectoryName(path), null, true, false, true, true);
                return false;
            };
            CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "ExportMesh");
        }

        [MenuItem("Assets/Tool/Fbx_ExportMeshAdvance")]
        static void Fbx_ExportMeshAdvance()
        {
            AssetExportConfig.ShowConfig(Fbx_ExportMesh);
        }

        [MenuItem(@"Assets/Tool/Fbx_ExportAvatar")]
        private static void Fbx_ExportAvatar()
        {
            CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
            {
                Animator ator = fbx.GetComponent<Animator>();
                if (ator != null && ator.avatar != null)
                {
                    string avatarPath = string.Format("{0}/Avatar/{1}.asset", AssetsConfig.instance.Creature_Path, ator.avatar.name);
                    Avatar avatar = UnityEngine.Object.Instantiate<Avatar>(ator.avatar);
                    avatar.name = ator.avatar.name;
                    CommonAssets.CreateAsset<Avatar>(avatarPath, ".asset", avatar);
                }
                return false;
            };
            CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "ExportAvatar");
        }

        internal static void Fbx_BindAvatar(string dir, GameObject srcFbx, bool isHuman)
        {
            if (Directory.Exists(dir))
            {
                Animator ator = srcFbx.GetComponent<Animator>();
                if (ator != null && ator.avatar != null)
                {
                    CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
                    {
                        if (srcFbx != fbx)
                        {
                            string srcAssetPath = AssetDatabase.GetAssetPath(srcFbx);
                            ModelImporter importer = AssetImporter.GetAtPath(srcAssetPath) as ModelImporter;
                            if (importer != null)
                            {
                                SerializedObject so = new SerializedObject(modelImporter);
                                SerializedProperty sp = CommonAssets.GetSerializeProperty(so, "m_AnimationType");
                                sp.intValue = isHuman ? (int)ModelImporterAnimationType.Human : (int)ModelImporterAnimationType.Generic;
                                if (isHuman)
                                {
                                    sp = CommonAssets.GetSerializeProperty(so, "m_CopyAvatar");
                                    sp.boolValue = true;
                                    sp = CommonAssets.GetSerializeProperty(so, "m_LastHumanDescriptionAvatarSource");
                                    sp.objectReferenceValue = ator.avatar;
                                }

                                SerializedObject srcSo = new SerializedObject(importer);
                                so.CopyFromSerializedProperty(srcSo.FindProperty("m_HumanDescription"));
                                so.ApplyModifiedPropertiesWithoutUndo();
                                ApplyModfy(path);
                            }

                        }

                        return false;
                    };
                    CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "BindAvatar", dir);
                }
            }
        }

        internal static void Fbx_BindAnimation(string dir, AvatarMask avatarMask, bool isHuman)
        {
            CommonAssets.enumFbx.cb = (fbx, modelImporter, path, context) =>
            {
                if (path.ToLower().Contains("/animation/"))
                {
                    SerializedObject so = new SerializedObject(modelImporter);
                    SerializedProperty sp = CommonAssets.GetSerializeProperty(so, "m_ClipAnimations");
                    if (sp.arraySize == 0)
                    {
                        sp.InsertArrayElementAtIndex(0);
                    }
                    if (sp.arraySize == 1)
                    {
                        var takes = modelImporter.defaultClipAnimations;
                        if (takes.Length > 0)
                        {
                            var takeInfo = takes[0];

                            SerializedProperty clip = sp.GetArrayElementAtIndex(0);
                            SerializedProperty start = clip.FindPropertyRelative("firstFrame");
                            start.floatValue = takeInfo.firstFrame;
                            SerializedProperty end = clip.FindPropertyRelative("lastFrame");
                            end.floatValue = takeInfo.lastFrame;
                        }
                        SerializedProperty clipSp = sp.GetArrayElementAtIndex(0);
                        var subsp = clipSp.FindPropertyRelative("name");
                        subsp.stringValue = fbx.name;
                        if (isHuman)
                        {
                            subsp = clipSp.FindPropertyRelative("loopBlendOrientation");
                            subsp.boolValue = true;
                            subsp = clipSp.FindPropertyRelative("loopBlendPositionY");
                            subsp.boolValue = true;
                            subsp = clipSp.FindPropertyRelative("loopBlendPositionXZ");
                            subsp.boolValue = true;
                            subsp = clipSp.FindPropertyRelative("keepOriginalOrientation");
                            subsp.boolValue = true;
                            subsp = clipSp.FindPropertyRelative("keepOriginalPositionY");
                            subsp.boolValue = true;
                            subsp = clipSp.FindPropertyRelative("keepOriginalPositionXZ");
                            subsp.boolValue = true;
                            subsp = clipSp.FindPropertyRelative("heightFromFeet");
                            subsp.boolValue = false;
                            subsp = clipSp.FindPropertyRelative("maskType");
                            subsp.intValue = 1;
                            subsp = clipSp.FindPropertyRelative("maskSource");
                            subsp.objectReferenceValue = avatarMask;

                            SerializedProperty bodyMask = clipSp.FindPropertyRelative("bodyMask");

                            if (bodyMask != null && bodyMask.isArray)
                            {
                                for (AvatarMaskBodyPart i = 0; i < AvatarMaskBodyPart.LastBodyPart; i++)
                                {
                                    if ((int)i >= bodyMask.arraySize) bodyMask.InsertArrayElementAtIndex((int)i);
                                    bodyMask.GetArrayElementAtIndex((int)i).intValue = avatarMask.GetHumanoidBodyPartActive(i) ? 1 : 0;
                                }
                            }

                            SerializedProperty transformMask = clipSp.FindPropertyRelative("transformMask");
                            EditorCommon.CallInternalFunction(typeof(ModelImporter), "UpdateTransformMask", true, true, false, null, new object[] { avatarMask, transformMask });
                        }

                        so.ApplyModifiedPropertiesWithoutUndo();
                        ApplyModfy(path);
                    }
                }
                return false;
            };
            CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "BindAnimation", dir);
        }
        // [MenuItem(@"Assets/Tool/Fbx_BindAvatar")]
        // private static void Fbx_BindAvatar()
        // {
        //     CommonAssets.enumFbx.cb = (fbx, modelImporter, path) =>
        //     {
        //         SerializedObject so = new SerializedObject(modelImporter);
        //         SerializedProperty sp = CommonAssets.GetSerializeProperty(so,"m_AnimationType");
        //         sp.intValue = (int)ModelImporterAnimationType.Human;
        //         sp = CommonAssets.GetSerializeProperty(so,"m_CopyAvatar");
        //         sp.boolValue = true;
        //         sp = CommonAssets.GetSerializeProperty(so, "m_LastHumanDescriptionAvatarSource");
        //         string avatarPath = string.Format("{0}/Avatar/Player_female_bandposeAvatar.asset", AssetsConfig.GlobalAssetsConfig.Creature_Path);
        //         sp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
        //         so.ApplyModifiedPropertiesWithoutUndo();
        //         ApplyModfy(path);
        //         return false;
        //     };
        //     CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "BindAvatar");
        // }

        // [MenuItem(@"Assets/Tool/Fbx_BindAnimation")]
        // private static void Fbx_BindAnimation()
        // {
        //     CommonAssets.enumFbx.cb = (fbx, modelImporter, path) =>
        //     {
        //         SerializedObject so = new SerializedObject(modelImporter);
        //         SerializedProperty sp = CommonAssets.GetSerializeProperty(so, "m_ClipAnimations");
        //         if (sp.arraySize == 1)
        //         {
        //             SerializedProperty clipSp = sp.GetArrayElementAtIndex(0);
        //             var subsp = clipSp.FindPropertyRelative("name");
        //             subsp.stringValue = fbx.name;
        //             subsp = clipSp.FindPropertyRelative("loopBlendOrientation");
        //             subsp.boolValue = true;
        //             subsp = clipSp.FindPropertyRelative("loopBlendPositionY");
        //             subsp.boolValue = true;
        //             subsp = clipSp.FindPropertyRelative("loopBlendPositionXZ");
        //             subsp.boolValue = true;
        //             subsp = clipSp.FindPropertyRelative("keepOriginalOrientation");
        //             subsp.boolValue = true;
        //             subsp = clipSp.FindPropertyRelative("keepOriginalPositionY");
        //             subsp.boolValue = true;
        //             subsp = clipSp.FindPropertyRelative("keepOriginalPositionXZ");
        //             subsp.boolValue = true;
        //             subsp = clipSp.FindPropertyRelative("heightFromFeet");
        //             subsp.boolValue = false;
        //             subsp = clipSp.FindPropertyRelative("maskType");
        //             subsp.intValue = 1;
        //             subsp = clipSp.FindPropertyRelative("maskSource");
        //             string avatarMaskPath = string.Format("{0}/Avatar/Player_female_bandposeAvatar.asset", AssetsConfig.GlobalAssetsConfig.Creature_Path);
        //             subsp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<AvatarMask>(avatarMaskPath);
        //             so.ApplyModifiedPropertiesWithoutUndo();
        //             ApplyModfy(path);

        //         }
        //         return true;
        //     };
        //     CommonAssets.EnumAsset<GameObject>(CommonAssets.enumFbx, "BindAnimation");
        // }

        public static bool CheckModelSubAssetNames(GameObject model, EditorMessageType logError)
        {
            string assetPath = AssetDatabase.GetAssetPath(model);
            return CheckModelSubAssetNames(assetPath, logError);
        }

        public static bool CheckModelSubAssetNames(string assetPath, EditorMessageType logError)
        {
            if (!string.IsNullOrEmpty(assetPath) && assetPath.ToLower().EndsWith(".fbx"))
            {
                bool filter(UnityEngine.Object x) => x is Mesh;
                List<UnityEngine.Object> invalidSubAssets = EditorCommon.CheckInvalidSubAssetNames(assetPath, logError, filter);
                return invalidSubAssets.Count > 0;
            }
            return true;
        }
    }
}

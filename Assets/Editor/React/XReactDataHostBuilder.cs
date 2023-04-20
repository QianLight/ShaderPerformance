#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Xml.Serialization;
using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using CFClient;

namespace XEditor {

    //public class XReactDataSet
    //{
    //    [SerializeField]
    //    private XReactData _xData = null;
    //    [SerializeField]
    //    private XReactDataExtra _xDataExtra = null;
    //    [SerializeField]
    //    private XReactConfigData _xConfigData = null;
    //    [SerializeField]
    //    private XReactEditorData _xEditorData = null;

    //    public XReactData ReactData
    //    {
    //        get
    //        {
    //            if (_xData == null) _xData = new XReactData();
    //            return _xData;
    //        }
    //        set
    //        {
    //            //for load data from file.
    //            _xData = value;
    //        }
    //    }

    //    public XReactDataExtra ReactDataExtra
    //    {
    //        get
    //        {
    //            if (_xDataExtra == null) _xDataExtra = new XReactDataExtra();
    //            return _xDataExtra;
    //        }
    //    }

    //    public XReactConfigData ConfigData
    //    {
    //        get
    //        {
    //            if (_xConfigData == null) _xConfigData = new XReactConfigData();
    //            return _xConfigData;
    //        }
    //        set
    //        {
    //            //for load data from file.
    //            _xConfigData = value;
    //        }
    //    }

    //    public XReactEditorData EditorData
    //    {
    //        get
    //        {
    //            if (_xEditorData == null) _xEditorData = new XReactEditorData();
    //            return _xEditorData;
    //        }
    //    }

    //}

    public class XReactDataHostBuilder : XSingleton<XReactDataHostBuilder>
    {
        public static DateTime Time;
        public static GameObject hoster = null;

        public static BoneShake boneShake;


        public bool Load(string pathwithname, out XReactDataSet DataSet)
        {
            try
            {
                XReactConfigData conf = XDataIO<XReactConfigData>.singleton.DeserializeData(XEditorPath.GetCfgFromSkp(pathwithname));

                DataSet = new XReactDataSet();

                ColdBuild(conf, ref DataSet);
                var prefixPath = pathwithname.Substring(0, pathwithname.IndexOf("/ReactPackage"));
                Time = File.GetLastWriteTime(pathwithname);
                return true;
            }
            catch (Exception e)
            {
                DataSet = null;
                Debug.Log("<color=red>Error occurred during loading config file: " + pathwithname + " with error " + e.Message + "</color>");
            }

            return false;
        }

        void ColdBuild(XReactConfigData conf, ref XReactDataSet DataSet)
        {
            string directory = conf.Directory[conf.Directory.Length - 1] == '/' ? conf.Directory.Substring(0, conf.Directory.Length - 1) : conf.Directory;
            string path = XEditorPath.GetPath("ReactPackage" + "/" + directory);

            DataSet.ConfigData = conf;
            DataSet.ReactData = XDataIO<XReactData>.singleton.DeserializeData(path + conf.ReactName + ".bytes");

            DataSet.ReactDataExtra.ScriptPath = path;
            DataSet.ReactDataExtra.ScriptFile = conf.ReactName;

            DataSet.ReactDataExtra.ReactClip = RestoreClip(conf.ReactClip, conf.ReactClipName);
            DataSet.ReactDataExtra.ReactClip2 = RestoreClip(conf.ReactClip2, conf.ReactClipName2);

            if (DataSet.ReactData.Time == 0 && DataSet.ReactDataExtra.ReactClip != null)
                DataSet.ReactData.Time = DataSet.ReactDataExtra.ReactClip.length;


            //

            XReactDataExtra edata = DataSet.ReactDataExtra;
            XReactData data = DataSet.ReactData;

            edata.Fx.Clear();
            edata.Audio.Clear();

            if (data.Fx != null)
            {
                foreach (XFxData fx in data.Fx)
                {
                    XFxDataExtra fxe = new XFxDataExtra();
                    fxe.Fx = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>(fx.Fx);
                    fxe.Ratio = fx.At / data.Time;
                    fxe.End_Ratio = fx.End / data.Time;

                    edata.Fx.Add(fxe);
                }
            }

            if (data.Audio != null)
            {
                foreach (XAudioData au in data.Audio)
                {
                    XAudioDataExtra aue = new XAudioDataExtra();
                    aue.audio = XResourceHelper.LoadEditorResourceAtBundleRes<AudioClip>(au.Clip);
                    aue.Ratio = au.At / data.Time;

                    edata.Audio.Add(aue);
                }
            }

            if (data.BoneShakeData != null)
            {
                foreach (XBoneShakeData fx in data.BoneShakeData)
                {
                    XBoneShakeExtra fxe = new XBoneShakeExtra();
                    fxe.Ratio = fx.At / data.Time;
                    edata.BoneShake.Add(fxe);
                    if (fx.IgnoreBones != null) fxe.IgnoreBones = new System.Collections.Generic.List<GameObject>();
                }
            }

            if (!string.IsNullOrEmpty(data.AvatarMask))
            {
                edata.Mask = RestoreAvatarMask(data.AvatarMask);
            }
        }

        public bool HotLoad(XReactDataSet DataSet)
        {
            var conf = DataSet.ConfigData;

            GameObject prefab = XAnimationLibrary.GetDummy((uint)conf.Player);

            if (prefab == null)
            {
                Debug.Log("<color=red>Prefab not found by id: " + conf.Player + "</color>");
                return false;
            }

            if (hoster != null) GameObject.DestroyImmediate(hoster);

            hoster = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            hoster.transform.localScale = Vector3.one * XAnimationLibrary.AssociatedAnimations((uint)conf.Player).Scale;
            //XDestructionLibrary.AttachDress((uint)conf.Player, hoster);
            hoster.AddComponent<XReactEntity>();

            CharacterController cc = hoster.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            XReactEntity component = hoster.GetComponent<XReactEntity>();
            component.ReactDataSet = DataSet;

            SetBoneShake(DataSet, hoster.transform);
            component.boneShake = boneShake;
            HotBuildEx(conf, ref DataSet);

            EditorGUIUtility.PingObject(hoster);
            Selection.activeObject = hoster;

            return true;
        }
        public Transform GetBone(XReactDataSet DataSet, string boneName, Transform transform)
        {
            XEntityPresentation.RowData xEntity = XAnimationLibrary.AssociatedAnimations((uint)DataSet.ConfigData.Player);
            var data = XReactLibrary.PresentConfig.GetPresentConfig(xEntity.Prefab, true, boneName, xEntity.BoneType);

            if (data != null)
            {
                return transform.Find(data.Value);
            }
            else
            {
                return transform.Find(boneName);
            }

        }
        void SetBoneShake(XReactDataSet DataSet, Transform hoster)
        {

            List<BoneShakeRuntimeTemplate> runtimeTemplates = new List<BoneShakeRuntimeTemplate>();

            var templates = DataSet.ReactData.BoneShakeData;
                int count = templates.Count;
            for (int i = 0; i < count; i++)
            {
                XBoneShakeData template = templates[i];
                BoneShakeRuntimeTemplate boneShakeRuntimeTemplate = new BoneShakeRuntimeTemplate();
                boneShakeRuntimeTemplate.bone = GetBone(DataSet, template.BoneName, hoster);
                boneShakeRuntimeTemplate.direction = new Vector3(template.AmplitudeX, template.AmplitudeY, template.AmplitudeZ);
                boneShakeRuntimeTemplate.timeFadeIntensity = template.timeFadeIntensity;
                boneShakeRuntimeTemplate.boneChainIntensity = template.boneChainIntensity;
                boneShakeRuntimeTemplate.timeFadeIntensityDown = template.timeFadeIntensityDown;
                boneShakeRuntimeTemplate.boneChainIntensityDown = template.boneChainIntensityDown;
                boneShakeRuntimeTemplate.randomIntensity = template.randomIntensity;
                boneShakeRuntimeTemplate.randomPercent = template.randomPercent;
                boneShakeRuntimeTemplate.frequancy = template.Frequency;
#if UNITY_EDITOR
                //boneShakeRuntimeTemplate.name = template.Bone;
#endif
                runtimeTemplates.Add(boneShakeRuntimeTemplate);
            }

            if (!BoneShake.Create(new BoneShakeCreateParam(hoster, runtimeTemplates, DataSet.ReactData.Name), out boneShake,
                out string failReason))
            {
                Debug.LogError($"Bone shake create fail, reason = {failReason}");
            }
        }

        private static AnimationClip RestoreClip(string path, string name)
        {
            if (path == null || name == null || path == "" || name == "") return null;

            int last = path.LastIndexOf('.');
            string subfix = path.Substring(last, path.Length - last).ToLower();

            if (subfix == ".fbx")
            {
                UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (UnityEngine.Object obj in objs)
                {
                    AnimationClip clip = obj as AnimationClip;
                    if (clip != null && clip.name == name)
                        return clip;
                }
            }
            else if (subfix == ".anim")
            {
                return AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;

            }
            else
                return null;

            return null;
        }

        public static void AttachHost(GameObject gb)
        {
            hoster = gb;
        }

        public static void HotBuildEx(XReactConfigData conf, ref XReactDataSet DataSet)
        {
            XReactDataExtra edata = DataSet.ReactDataExtra;
            XReactData data = DataSet.ReactData;

            if (data.Fx != null)
            {
                for (int i = 0; i < data.Fx.Count; ++i)
                {
                    XFxData fx = data.Fx[i];
                    XFxDataExtra fxe = edata.Fx[i];
                    fxe.Fx = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>(fx.Fx);
                    if (fx.Bone != null && fx.Bone.Length > 0)
                    {
                        Transform attachPoint = hoster.gameObject.transform.Find(fx.Bone);
                        if (attachPoint != null)
                        {
                            fxe.BindTo = attachPoint.gameObject;
                        }
                        else
                        {
                            int index = fx.Bone.LastIndexOf("/");
                            if (index >= 0)
                            {
                                string bone = fx.Bone.Substring(index + 1);
                                attachPoint = hoster.gameObject.transform.Find(bone);
                                if (attachPoint != null)
                                {
                                    fxe.BindTo = attachPoint.gameObject;
                                }
                            }

                        }
                    }
                }
            }

            if (data.Audio != null)
            {
                for (int i = 0; i < data.Audio.Count; ++i)
                {
                    var au = data.Audio[i];
                    XAudioDataExtra aue = edata.Audio[i];
                    aue.audio = XResourceHelper.LoadEditorResourceAtBundleRes<AudioClip>(au.Clip);
                }
            }

            if (data.BoneShakeData != null)
            {
                for (int i = 0; i < data.BoneShakeData.Count; ++i)
                {
                    XBoneShakeData fx = data.BoneShakeData[i];
                    XBoneShakeExtra fxe = edata.BoneShake[i];
                    if (fx.Bone != null && fx.Bone.Length > 0)
                    {
                        Transform attachPoint = hoster.gameObject.transform.Find(fx.Bone);
                        if (attachPoint != null)
                        {
                            fxe.BindTo = attachPoint.gameObject;
                        }
                        else
                        {
                            int index = fx.Bone.LastIndexOf("/");
                            if (index >= 0)
                            {
                                string bone = fx.Bone.Substring(index + 1);
                                attachPoint = hoster.gameObject.transform.Find(bone);
                                if (attachPoint != null)
                                {
                                    fxe.BindTo = attachPoint.gameObject;
                                }
                            }

                        }
                    }

                    //igonre bones.
                    if (fx.IgnoreBones != null && fx.IgnoreBones.Count > 0)
                    {
                        if (fxe.IgnoreBones == null) fxe.IgnoreBones = new System.Collections.Generic.List<GameObject>();
                        fxe.IgnoreBones.Clear();

                        for (int j = 0; j < fx.IgnoreBones.Count; ++j)
                        {
                            if (string.IsNullOrEmpty(fx.IgnoreBones[j])) continue;

                            Transform attachPoint = hoster.gameObject.transform.Find(fx.IgnoreBones[j]);
                            if (attachPoint != null)
                            {
                                fxe.IgnoreBones.Add(attachPoint.gameObject);
                            }
                            else
                            {
                                int index = fx.IgnoreBones[j].LastIndexOf("/");
                                if (index >= 0)
                                {
                                    string bone = fx.IgnoreBones[j].Substring(index + 1);
                                    attachPoint = hoster.gameObject.transform.Find(bone);
                                    if (attachPoint != null)
                                    {
                                        fxe.IgnoreBones.Add(attachPoint.gameObject);
                                    }
                                }
                            }
                        }
                    }
                }
            }            
        }

        static private AvatarMask RestoreAvatarMask(string path)
        {
            if (path == null) return null;

            return AssetDatabase.LoadAssetAtPath("Assets/BundleRes/" + path + ".mask", typeof(AvatarMask)) as AvatarMask;
        }

    }

}
#endif
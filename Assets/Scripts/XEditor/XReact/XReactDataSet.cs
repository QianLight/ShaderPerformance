#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using CFUtilPoolLib;
using System;

namespace XEditor
{

    [Serializable]
    public class XReactConfigData
    {
        [SerializeField]
        public string ReactName;
        [SerializeField]
        public bool Lock = true;

        [SerializeField]
        public string ReactClip;
        [SerializeField]
        public string ReactClipName;

        [SerializeField]
        public string ReactClip2;
        [SerializeField]
        public string ReactClipName2;

        [SerializeField]
        public string Directory = null;
        [SerializeField]
        public int Player = 0;

    }

    [Serializable]
    public class XBoneShakeExtra : XBaseDataExtra
    {
        public XBoneShakeExtra()
        {
            Ratio = 0;
            IgnoreBones = new List<GameObject>();
        }

        [SerializeField]
        public GameObject BindTo = null;
        [SerializeField]
        public float Ratio = 0;
        [SerializeField]
        public List<GameObject> IgnoreBones = null;
        [SerializeField]
        public string BonesName = "";

        [SerializeField]
        public float intensity;
        [SerializeField]
        public float duration;
        [SerializeField]
        public Vector3 position = new Vector3();
        [SerializeField]
        public Vector3 direction = new Vector3 ();
        [SerializeField]
        public bool isDrawBone = false;

    }

    [Serializable]
    public class XReactDataExtra
    {
        [SerializeField]
        public AnimationClip ReactClip;
        [SerializeField]
        public float ReactClip_Frame;
        [SerializeField]
        public string ScriptPath;
        [SerializeField]
        public string ScriptFile;

        [SerializeField]
        public AnimationClip ReactClip2;

        [SerializeField]
        public float PoseFrameRatio = 0f;

        [SerializeField]
        public AvatarMask Mask;

        [SerializeField]
        public bool SettedAdditiveRefrencePose;

        [SerializeField]
        public List<XFxDataExtra> Fx = new List<XFxDataExtra>();
        [SerializeField]
        public List<XAudioDataExtra> Audio = new List<XAudioDataExtra>();
        [SerializeField]
        public List<XBoneShakeExtra> BoneShake = new List<XBoneShakeExtra>();

        public void Add<T>(T data) where T : XBaseDataExtra
        {
            Type t = typeof(T);

            if (t == typeof(XFxDataExtra)) Fx.Add(data as XFxDataExtra);
            else if (t == typeof(XAudioDataExtra)) Audio.Add(data as XAudioDataExtra);
            else if (t == typeof(XBoneShakeExtra)) BoneShake.Add(data as XBoneShakeExtra);
        }
    }

    [Serializable]
    public class XReactDataSet
    {
        [SerializeField]
        private XReactData _xData = null;
        [SerializeField]
        private XReactDataExtra _xDataExtra = null;
        [SerializeField]
        private XReactConfigData _xConfigData = null;

        public XReactData ReactData
        {
            get
            {
                if (_xData == null) _xData = new XReactData();
                return _xData;
            }
            set
            {
                //for load data from file.
                _xData = value;
            }
        }

        public XReactDataExtra ReactDataExtra
        {
            get
            {
                if (_xDataExtra == null) _xDataExtra = new XReactDataExtra();
                return _xDataExtra;
            }
        }

        public XReactConfigData ConfigData
        {
            get
            {
                if (_xConfigData == null) _xConfigData = new XReactConfigData();
                return _xConfigData;
            }
            set
            {
                //for load data from file.
                _xConfigData = value;
            }
        }

    }
    

}
#endif
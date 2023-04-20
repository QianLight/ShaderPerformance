#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using CFEngine;
using CFUtilPoolLib;
using FMODUnity;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace XEditor
{
    public class XReactBoneShake
    {
        private bool m_bShaking = false;

        XBoneShakeData ShakeData = null;

        Transform[] bones = new Transform[5];

        private float _interval_time = 0f;
        private float _total_time = 0f;
        private float _cur_time = 0f;

        private float tIntervalTime = 0f;

        private Vector3 _shake_delta;

        public Transform EngineTrans;

        public bool EnableIgnoreBone = true;

        //private string fixBone1 = "root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 L Thigh/Bip001 L Calf/Bip001 L Foot";
        //private string fixBone2 = "root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 R Thigh/Bip001 R Calf/Bip001 R Foot";

        private List<Transform> boneList;
        private List<Vector3> posList;
        private List<Vector3> eulerList;

        //>所选骨骼节点，在设置有动作时并且clip中没有对该节点位置进行更改时，会发生偏移，需要修正
        private bool modifyFirst = true;
        private Vector3[] lastLocalPos = new Vector3[5];
        private Vector3 lastShakeDelta;
        private bool[] modify = new bool[5];

        // Start is called before the first frame update
        public void Init(Transform ts)
        {
            m_bShaking = false;
            EngineTrans = ts;
        }

        // Update is called once per frame
        public void LateUpdate(float fDeltaT)
        {
            if (m_bShaking)
            {
                if (_cur_time < _total_time)
                {
                    _cur_time += (_total_time - _cur_time) > fDeltaT ? fDeltaT : (_total_time - _cur_time);
                    _interval_time += fDeltaT;

                    _shake_delta = Shake(ShakeData.AmplitudeX, 
                        ShakeData.AmplitudeY, 
                        ShakeData.AmplitudeZ, 
                        _interval_time, 
                        tIntervalTime);

                    if (EnableIgnoreBone)
                    {
                        for (int i = 0; i < boneList.Count; ++i)
                        {
                            posList[i] = boneList[i].position;
                            eulerList[i] = boneList[i].eulerAngles;
                        }
                    }

                    for (int i = 0; i < bones.Length; ++i)
                    {
                        var ts = bones[i];
                        if (ts != null)
                        {
                            if (!modifyFirst)
                            {
                                modify[i] = IsSame(lastLocalPos[i], ts.localPosition);
                            }
                            
                            if (modify[i])
                                ts.position += _shake_delta -lastShakeDelta;
                            else
                                ts.position += _shake_delta;

                            if (modifyFirst || modify[i])
                                lastLocalPos[i] = ts.localPosition;
                            
                        }
                    }
                    modifyFirst = false;
                    lastShakeDelta = _shake_delta;

                    if (EnableIgnoreBone)
                    {
                        for (int i = 0; i < boneList.Count; ++i)
                        {
                            boneList[i].eulerAngles = eulerList[i];
                            boneList[i].position = posList[i];                            
                        }
                    }

                    if (_interval_time > tIntervalTime)
                    {
                        _interval_time = 0f;
                    }

                }
                else
                {
                    OnTimeOut();
                }
            }

        }

        private bool IsSame(Vector3 a, Vector3 b)
        {
            return ((a - b).sqrMagnitude < 0.01f);
        }


        public bool OnShakeEvent(int ShakeState, XBoneShakeData shake = null)
        {
            bool prev = m_bShaking;
            m_bShaking = ShakeState == 1;
            ShakeData = shake;
            modifyFirst = true;
            for (int i = 0; i < modify.Length; ++i)
            {
                modify[i] = false;
            }

            // Find Bone.
            for (int i = 0; i < bones.Length; ++i)
            {
                bones[i] = null;
            }

            if (ShakeData == null || string.IsNullOrEmpty(ShakeData.Bone) || EngineTrans == null)
            {
                m_bShaking = false;
            }
            else
            {
                _total_time = ShakeData.LifeTime;
                _cur_time = 0f;
                _interval_time = 0f;
                if (ShakeData.Frequency > 0)
                    tIntervalTime = _total_time / ShakeData.Frequency;
                else
                    tIntervalTime = 0.0333f;

                if (boneList == null) boneList = new List<Transform>();
                boneList.Clear();
                if (posList == null) posList = new List<Vector3>();
                posList.Clear();
                if (eulerList == null) eulerList = new List<Vector3>();
                eulerList.Clear();

                if (ShakeData.IgnoreBones != null && ShakeData.IgnoreBones.Count > 0)
                {
                    for (int i = 0; i < ShakeData.IgnoreBones.Count; ++i)
                    {
                        var tBone = EngineTrans.Find(ShakeData.IgnoreBones[i]);
                        if (tBone == null)
                        {
                            XDebug.singleton.AddErrorLog("Not Find Ignore Bone :  Bone = " + ShakeData.IgnoreBones[i].ToString());
                            continue;
                        }

                        boneList.Add(tBone);
                        posList.Add(tBone.position);
                        eulerList.Add(tBone.eulerAngles);
                    }
                }
            }

            if (prev && !m_bShaking)
                Stop();

            if (m_bShaking)
                Begin();

            return true;
        }

        void Begin()
        {
            bones[0] = EngineTrans.Find(ShakeData.Bone);

            //root/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 L Thigh/Bip001 L Calf/Bip001 L Foot

            if (bones[0] == null)
            {
                XDebug.singleton.AddErrorLog("Not Find Bone :  Bone = " + ShakeData.Bone.ToString());
                Stop();
                return;
            }

        }

        //private Vector3 Shake(float Frequency, float AmplitudeX, float AmplitudeY, float AmplitudeZ, float delta)
        //{
        //    float factor = delta * Frequency;
        //    float factorX = (Mathf.Min(1, Mathf.PerlinNoise(factor, factor)) - 0.5f) * 2;
        //    factor += Mathf.PI * Frequency;
        //    float factorY = (Mathf.Min(1, Mathf.PerlinNoise(factor, factor)) - 0.5f) * 2;
        //    factor += Mathf.PI * Frequency;
        //    float factorZ = (Mathf.Min(1, Mathf.PerlinNoise(factor, factor)) - 0.5f) * 2;

        //    return new Vector3(AmplitudeX * factorX, AmplitudeY * factorY, AmplitudeZ * factorZ);
        //}

        private Vector3 Shake(float AmplitudeX, float AmplitudeY, float AmplitudeZ, float t, float time)
        {
            float d = Mathf.Clamp01(t / time);
            float s = Mathf.Sin(Mathf.PI * d) ;// Mathf.PingPong(d, 0.5f);

            //XDebug.singleton.AddErrorLog(" = " + t.ToString() + "  time=" + time.ToString()+"  d="+d.ToString()+"  s="+s.ToString());

            float factorX = s;// (Mathf.Min(1, Mathf.PerlinNoise(factor, factor)) - 0.5f) * 2;
            float factorY = s;
            float factorZ = s;
            return new Vector3(AmplitudeX * factorX, AmplitudeY * factorY, AmplitudeZ * factorZ);
        }

        void OnTimeOut()
        {
            Cease();
        }

        void Stop()
        {
            Cease();
        }

        void Cease()
        {
            m_bShaking = false;
            ShakeData = null;
        }
    }
}

#endif
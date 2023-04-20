using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using CFEngine;
using UnityEngine.Rendering.Universal;


    [ExecuteAlways]
    public class PostprogressPreviewMgr:MonoBehaviour
    {
        private XCustomDataBase previewData;
        private float time = 0;
        private bool set = false;
        private int id = -1;

        public void Preview(string path)
        {
            ResetData();
            ReadBlurData(string.Format("{0}/BundleRes/CommonData/{1}.bytes", Application.dataPath, path));
        }

        private void ReadDataCallback(XCustomDataBase data,uint id)
        {
            previewData = data;
        }

        private void Update()
        {
            if (previewData == null)
                return;
            time += Time.deltaTime;            
            if(previewData.loop)
            {
                switch ((DataType)previewData.type)
                {
                    case DataType.Blur:
                        var data = previewData as XBlurData;
                        if (time >= data.fadeInTime + data.loopTime)
                            time -= data.loopTime;
                        break;
                }
            }
            if(previewData.IsEnd(time))
            {
                ResetData();
                return;
            }
            switch ((DataType)previewData.type)
            {
                case DataType.Blur:
                    var param = GetBlurParam(previewData as XBlurData, time);
                    if(!set)
                    {
                        id = URPRadialBlur.instance.AddParam(RadialBlurParam.InitValue(param), URPRadialBlurSource.DefualtValue, 1);
                        set = true;
                    }
                    else
                    {
                        URPRadialBlur.instance.ModifyParam(id, RadialBlurParam.InitValue(param));
                    }
                    break;
            }
        }

        public void Stop()
        {
            if(previewData!=null)
            {
                URPRadialBlur.instance.RemoveParam(id);
                ResetData();
            }
        }

        private void ResetData()
        {
            previewData = null;
            time = 0;
            set = false;
            id = -1;
        }

        private object[] GetBlurParam(XBlurData data, float time)
        {
            Vector3 center = new Vector3(data.xPos + data.xPosOffset.Evaluate(time), data.yPos + data.yPosOffset.Evaluate(time), 0);
            float scale = data.scale + data.scaleOffset.Evaluate(time);
            float innerRadius = data.innerRadius.Evaluate(time);
            float innerFadeout = data.innerFadeout.Evaluate(time);
            float outerRadius = data.outerRadius.Evaluate(time);
            float outerFadeout = data.outerFadeout.Evaluate(time);
            float intensity = data.intensity + data.intensityOffset.Evaluate(time);
            object[] param = new object[] { true, center, scale, innerRadius, innerFadeout, outerRadius, outerFadeout, intensity, data.useScreenPos };
            return param;
        }

        private void ReadBlurData(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryReader reader = new BinaryReader(fs);
            reader.ReadByte();//版本号 编辑器里不用
            var type = reader.ReadByte();
            var priority = reader.ReadInt32();
            var loop = reader.ReadBoolean();
            switch ((DataType)type)
            {
                case DataType.Blur:
                    previewData = new XBlurData()
                    {
                        type = type,
                        priority = priority,
                        loop = loop
                    };
                    var bData = previewData as XBlurData;
                    bData.fadeInTime = reader.ReadSingle();
                    bData.loopTime = reader.ReadSingle();
                    bData.fadeOutTime = reader.ReadSingle();
                    bData.xPos = reader.ReadSingle();
                    bData.yPos = reader.ReadSingle();
                    bData.intensity = reader.ReadSingle();
                    bData.scale = reader.ReadSingle();
                    bData.useScreenPos = reader.ReadBoolean();

                    bData.xPosOffset = ReadCurve(reader);
                    bData.yPosOffset = ReadCurve(reader);
                    bData.scaleOffset = ReadCurve(reader);
                    bData.innerRadius = ReadCurve(reader);
                    bData.innerFadeout = ReadCurve(reader);
                    bData.outerRadius = ReadCurve(reader);
                    bData.outerFadeout = ReadCurve(reader);
                    bData.intensityOffset = ReadCurve(reader);
                    break;
            }
            reader.Close();
        }

        private AnimationCurve ReadCurve(BinaryReader reader)
        {
            reader.ReadBoolean();
            var curve = new AnimationCurve();
            int len = reader.ReadByte();
            var keys = new Keyframe[len];
            for (var j = 0; j < len; j++)
            {
                keys[j] = new Keyframe(
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()
                    );
            }
            curve.keys = keys;
            curve.postWrapMode = (WrapMode)reader.ReadByte();
            curve.preWrapMode = (WrapMode)reader.ReadByte();
            return curve;
        }
    }


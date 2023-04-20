using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CFEngine;

namespace DataEditor
{
    class BlurData:DataBase
    {
        #region 基础数据
        public float fadeInTime;
        public float loopTime;
        public float fadeOutTime;
        public float xPos;
        public float yPos;
        public float intensity;
        public float scale;
        public bool useScreenPos;
        #endregion

        #region 淡入阶段数据

        public FloatOrCurveData fadeInPosXOffset=new FloatOrCurveData();
        public FloatOrCurveData fadeInPosYOffset=new FloatOrCurveData();
        public FloatOrCurveData fadeInScale=new FloatOrCurveData();
        public FloatOrCurveData fadeInInnerRadius=new FloatOrCurveData();
        public FloatOrCurveData fadeInInnerFadeOut=new FloatOrCurveData();
        public FloatOrCurveData fadeInOuterRadius=new FloatOrCurveData();
        public FloatOrCurveData fadeInOuterFadeOut=new FloatOrCurveData();
        public FloatOrCurveData fadeInIntensity=new FloatOrCurveData();

        #endregion

        #region 循环阶段数据

        public FloatOrCurveData loopPosXOffset=new FloatOrCurveData();
        public FloatOrCurveData loopPosYOffset=new FloatOrCurveData();
        public FloatOrCurveData loopScale=new FloatOrCurveData();
        public FloatOrCurveData loopInnerRadius=new FloatOrCurveData();
        public FloatOrCurveData loopInnerFadeOut=new FloatOrCurveData();
        public FloatOrCurveData loopOuterRadius=new FloatOrCurveData();
        public FloatOrCurveData loopOuterFadeOut=new FloatOrCurveData();
        public FloatOrCurveData loopIntensity=new FloatOrCurveData();

        #endregion

        #region 淡出阶段数据

        public FloatOrCurveData fadeOutPosXOffset=new FloatOrCurveData();
        public FloatOrCurveData fadeOutPosYOffset=new FloatOrCurveData();
        public FloatOrCurveData fadeOutScale=new FloatOrCurveData();
        public FloatOrCurveData fadeOutInnerRadius=new FloatOrCurveData();
        public FloatOrCurveData fadeOutInnerFadeOut=new FloatOrCurveData();
        public FloatOrCurveData fadeOutOuterRadius=new FloatOrCurveData();
        public FloatOrCurveData fadeOutOuterFadeOut=new FloatOrCurveData();
        public FloatOrCurveData fadeOutIntensity=new FloatOrCurveData();

        #endregion

        public BlurData()
        {
            type = (byte)DataType.Blur;
        }


        public override void ReadData(BinaryReader reader)
        {
            base.ReadData(reader);

            fadeInTime = reader.ReadSingle();
            loopTime = reader.ReadSingle();
            fadeOutTime = reader.ReadSingle();
            xPos = reader.ReadSingle();
            yPos = reader.ReadSingle();
            intensity = reader.ReadSingle();
            scale = reader.ReadSingle();
            useScreenPos = reader.ReadBoolean();

            ConvertCurveBack(reader, ref fadeInPosXOffset,ref loopPosXOffset,ref fadeOutPosXOffset);
            ConvertCurveBack(reader,ref fadeInPosYOffset,ref loopPosYOffset,ref fadeOutPosYOffset);
            ConvertCurveBack(reader,ref fadeInScale,ref loopScale,ref fadeOutScale);
            ConvertCurveBack(reader,ref fadeInInnerRadius,ref loopInnerRadius,ref fadeOutInnerRadius);
            ConvertCurveBack(reader,ref fadeInInnerFadeOut,ref loopInnerFadeOut,ref fadeOutInnerFadeOut);
            ConvertCurveBack(reader,ref fadeInOuterRadius,ref loopOuterRadius,ref fadeOutOuterRadius);
            ConvertCurveBack(reader,ref fadeInOuterFadeOut,ref loopOuterFadeOut,ref fadeOutOuterFadeOut);
            ConvertCurveBack(reader,ref fadeInIntensity,ref loopIntensity,ref fadeOutIntensity);
        }

        public override void WriteData(BinaryWriter writer)
        {
            base.WriteData(writer);            

            writer.Write(fadeInTime);
            writer.Write(loopTime);
            writer.Write(fadeOutTime);
            writer.Write(xPos);
            writer.Write(yPos);
            writer.Write(intensity);
            writer.Write(scale);
            writer.Write(useScreenPos);

            WriteCurve(writer, ConvertToCurve(fadeInPosXOffset, loopPosXOffset, fadeOutPosXOffset,writer));
            WriteCurve(writer, ConvertToCurve(fadeInPosYOffset, loopPosYOffset, fadeOutPosYOffset, writer));
            WriteCurve(writer, ConvertToCurve(fadeInScale, loopScale, fadeOutScale, writer));
            WriteCurve(writer, ConvertToCurve(fadeInInnerRadius, loopInnerRadius, fadeOutInnerRadius, writer));
            WriteCurve(writer, ConvertToCurve(fadeInInnerFadeOut, loopInnerFadeOut, fadeOutInnerFadeOut, writer));
            WriteCurve(writer, ConvertToCurve(fadeOutOuterRadius, loopOuterRadius, fadeOutOuterRadius, writer));
            WriteCurve(writer, ConvertToCurve(fadeOutOuterFadeOut, loopOuterFadeOut, fadeOutOuterRadius, writer));
            WriteCurve(writer, ConvertToCurve(fadeInIntensity, loopIntensity, fadeOutIntensity, writer));
        }

        private bool fadeInFold;
        private bool fadeOutFold;
        private bool loopFold;


        public override void DrawDataArea()
        {
            base.DrawDataArea();
            priority = EditorGUILayout.IntField("优先级",priority);
            fadeInTime=EditorGUILayout.FloatField("Fade In Time", fadeInTime);
            loopTime = EditorGUILayout.FloatField("Loop Time", loopTime);
            fadeOutTime = EditorGUILayout.FloatField("Fade Out Time", fadeOutTime);

            xPos = EditorGUILayout.FloatField("XPos", xPos);
            yPos = EditorGUILayout.FloatField("YPos", yPos);
            intensity = EditorGUILayout.FloatField("Intensity", intensity);
            scale = EditorGUILayout.FloatField("Scale", scale);
            useScreenPos = EditorGUILayout.Toggle("Use Screen Pos", useScreenPos);
            loop = EditorGUILayout.Toggle("loop", loop);

            fadeInFold=EditorGUILayout.BeginFoldoutHeaderGroup(fadeInFold, new GUIContent("淡入阶段"));

            if(fadeInFold)
            {
                fadeInPosXOffset.DrawDataArea("X Pos Offset");
                fadeInPosYOffset.DrawDataArea("Y Pos Offset");
                fadeInScale.DrawDataArea("Scale");
                fadeInInnerRadius.DrawDataArea("Inner Radius");
                fadeInInnerFadeOut.DrawDataArea("Inner Fade Out");
                fadeInOuterRadius.DrawDataArea("Outer Radius");
                fadeInOuterFadeOut.DrawDataArea("Outer Fade Out");
                fadeInIntensity.DrawDataArea("Intensity");
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            loopFold = EditorGUILayout.BeginFoldoutHeaderGroup(loopFold, "循环阶段");

            if(loopFold)
            {
                loopPosXOffset.DrawDataArea("X Pos Offset");
                loopPosYOffset.DrawDataArea("Y Pos Offset");
                loopScale.DrawDataArea("Scale");
                loopInnerRadius.DrawDataArea("Inner Radius");
                loopInnerFadeOut.DrawDataArea("Inner Fade Out");
                loopOuterRadius.DrawDataArea("Outer Radius");
                loopOuterFadeOut.DrawDataArea("Outer Fade Out");
                loopIntensity.DrawDataArea("Intensity");
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();

            fadeOutFold = EditorGUILayout.BeginFoldoutHeaderGroup(fadeOutFold, "淡出阶段");

            if(fadeOutFold)
            {
                fadeOutPosXOffset.DrawDataArea("X Pos Offset");
                fadeOutPosYOffset.DrawDataArea("Y Pos Offset");
                fadeOutScale.DrawDataArea("Scale");
                fadeOutInnerRadius.DrawDataArea("Inner Radius");
                fadeOutInnerFadeOut.DrawDataArea("Inner Fade Out");
                fadeOutOuterRadius.DrawDataArea("Outer Radius");
                fadeOutOuterFadeOut.DrawDataArea("Outer Fade Out");
                fadeOutIntensity.DrawDataArea("Intensity");
            }
           
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        /// <summary>
        /// 为了方便运行时处理和减少存储的数据量 把三个阶段的相同参数合并成一条曲线 按淡入循环淡出顺序传递参数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>

        private AnimationCurve ConvertToCurve(FloatOrCurveData x,FloatOrCurveData y,FloatOrCurveData z,BinaryWriter writer)
        {
            AnimationCurve curve = new AnimationCurve();
            int xLen = x.isCurve ? x.curve.length : 1,
                yLen = y.isCurve ? y.curve.length : 1, 
                zLen = z.isCurve ? z.curve.length : 2;//如果fadeout是个固定值的话 需要添加曲线的尾点
            int keyCount = xLen+yLen+zLen;
            Keyframe[] keys = new Keyframe[keyCount];
            for(var i=0;i<keyCount;i++)
            {
                if(i<xLen)//淡入
                {
                    if (!x.isCurve)
                        keys[i] = new Keyframe(0, x.value, 0, 0);
                    else
                        keys[i] = x.curve.keys[i];
                }
                else if(i>=xLen+yLen)//淡出
                {
                    if (!z.isCurve)
                    {
                        if (i == xLen + yLen)
                            keys[i] = new Keyframe(fadeInTime + loopTime, z.value, Mathf.Infinity, 0);
                        else
                            keys[i] = new Keyframe(fadeInTime + loopTime+fadeOutTime, z.value, 0, 0);
                    }
                    else
                    {
                        int index = i - xLen - yLen;
                        if (i == xLen + yLen)
                            keys[i] = new Keyframe(fadeInTime + loopTime + z.curve.keys[index].time,
                            z.curve.keys[index].value, Mathf.Infinity, z.curve.keys[index].outTangent);
                        else
                            keys[i] = new Keyframe(fadeInTime + loopTime + z.curve.keys[index].time,
                                z.curve.keys[index].value, keys[i - 1].outTangent, z.curve.keys[index].outTangent);
                    }
                }
                else//循环
                {
                    if(!y.isCurve)
                    {                        
                        keys[i] = new Keyframe(fadeInTime, y.value, Mathf.Infinity, 0);
                    }
                    else
                    {
                        int index = i - xLen;
                        if (i == xLen)
                            keys[i] = new Keyframe(fadeInTime + y.curve.keys[index].time, y.curve.keys[index].value,
                                Mathf.Infinity, y.curve.keys[index].outTangent);
                        else
                            keys[i] = new Keyframe(fadeInTime + y.curve.keys[index].time, y.curve.keys[index].value,
                                keys[i - 1].outTangent,y.curve.keys[index].outTangent);
                    }
                }
            }
            curve.keys = keys;
            curve.preWrapMode = WrapMode.Default;
            curve.postWrapMode = WrapMode.Default;
            writer.Write(z.isCurve);//为了在编辑器里读取原本数据 得加上
            return curve;
        }
        /// <summary>
        ///  从bytes还原数据时按淡入循环淡出顺序填充
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>

        private void ConvertCurveBack(BinaryReader reader,ref FloatOrCurveData fadein,ref FloatOrCurveData loop,ref FloatOrCurveData fadeout)
        {            
            fadeout.isCurve = reader.ReadBoolean();
            AnimationCurve curve = ReadCurve(reader);
            int xLen = 0, yLen = 0;
            for(var i=0;i<curve.length;i++)
            {
                var key = curve.keys[i];
                if(Mathf.Abs(key.time-fadeInTime)<0.01f&&xLen==0)
                {
                    if(curve.keys[i+1].time==key.time)//fadein为曲线 否则不会存在两个time均为fadeintime的关键帧
                    {
                        fadein.isCurve = true;
                        for(var j=0;j<=i;j++)
                        {
                            fadein.curve.AddKey(curve.keys[j]);
                        }
                    }
                    else
                    {
                        fadein.isCurve = false;
                        fadein.value = curve.keys[0].value;
                    }
                    xLen = i ;
                }
                if(Mathf.Abs(key.time - fadeInTime - loopTime)<0.01f &&yLen==0)
                {
                    loop.isCurve = curve.keys[i + 1].time == key.time;
                    yLen = loop.isCurve ? (i - xLen + 1 ): 1;
                    if(loop.isCurve)
                    {
                        for(var j=xLen;j<=i;j++)
                        {
                            loop.curve.AddKey(curve.keys[j]);
                            loop.curve.keys[j - xLen].time -= fadeInTime;
                        }
                    }
                    else
                        loop.value = key.value;
                }
                if(Mathf.Abs(key.time - fadeInTime - loopTime - fadeOutTime)<0.01f)//当z原来为曲线时这是最后一个点了 若有下一个点说明z原本为常值
                {
                    if(!fadeout.isCurve)
                    {
                        fadeout.value = key.value;
                        break;
                    }
                    else
                    {
                        for(var j=yLen+xLen;j<=i;j++)
                        {
                            fadeout.curve.AddKey(curve.keys[j]);
                            fadeout.curve.keys[j - yLen-xLen].time -= fadeInTime + loopTime;
                        }
                        break;
                    }
                }
            }
        }
        

        public override bool CheckData()
        {
            bool check = true;
            if(fadeInTime<=0||loopTime<=0||fadeOutTime<=0)
            {
                Debug.LogError("淡入循环或淡出时间不能为0");
                check = false;
                return check;
            }
            var fi = GetType().GetFields();
            for(var i=0;i<fi.Length;i++)
            {
                var value = fi[i].GetValue(this);
                if(value is FloatOrCurveData&&(value as FloatOrCurveData).isCurve)
                {
                    var data = value as FloatOrCurveData;
                    var curveTime = data.curve.keys[data.curve.length - 1].time;
                    float time = 0;
                    if (fi[i].Name.StartsWith("fadeIn"))
                        time = fadeInTime;
                    else if (fi[i].Name.StartsWith("loop"))
                        time = loopTime;
                    else
                        time = fadeOutTime;
                    bool valid = time == curveTime;
                    if (!valid)
                        Debug.LogError(string.Format("{0}的曲线长度不合法", fi[i].Name));
                    check &= valid;
                }
            }
            return check;
        }
    }
}

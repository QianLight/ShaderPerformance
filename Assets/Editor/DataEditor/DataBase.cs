using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;
using CFEngine;

namespace DataEditor
{
    class DataBase
    {
        public byte type;
        public string name;
        public int priority;
        public bool loop;
        public byte version;

        public virtual void ReadData(BinaryReader reader)
        {
            
        }

        public virtual void WriteData(BinaryWriter writer)
        {
            writer.Write(CFEngine.XDataLoader.DataVersion);
            writer.Write(type);
            writer.Write(priority);
            writer.Write(loop);
        }

        public virtual void DrawDataArea()
        {
            EditorGUILayout.BeginHorizontal();
            name = EditorGUILayout.TextField("文件名(必填)", name);
            if(GUILayout.Button("CopyPath",GUILayout.Width(75f)))
            {
                TextEditor te = new TextEditor
                {
                    text = string.Format("{0}/{1}", ((DataType)type).ToString(), name)
                };
                te.OnFocus();
                te.Copy();
            }
            EditorGUILayout.EndHorizontal();
        }

        protected AnimationCurve ReadCurve(BinaryReader reader)
        {
            var curve = new AnimationCurve();
            int len = reader.ReadByte();
            var keys = new Keyframe[len];
            for (var j = 0; j < len; j++)
            {
                keys[j] = new Keyframe(
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()
                    );
                keys[j].weightedMode = (WeightedMode)reader.ReadByte();
            }
            curve.keys = keys;
            curve.postWrapMode = (WrapMode)reader.ReadByte();
            curve.preWrapMode = (WrapMode)reader.ReadByte();
            return curve;
        }

        protected void WriteCurve(BinaryWriter writer, AnimationCurve curve)
        {
            writer.Write((byte)curve.length);
            for (var j = 0; j < curve.length; j++)
            {
                var key = curve.keys[j];
                writer.Write(key.time);
                writer.Write(key.value);
                writer.Write(key.inTangent);
                writer.Write(key.outTangent);
                writer.Write(key.inWeight);
                writer.Write(key.outWeight);
                writer.Write((byte)key.weightedMode);
            }
            writer.Write((byte)curve.postWrapMode);
            writer.Write((byte)curve.preWrapMode);
        }

        public virtual bool CheckData()
        {
            return true;
        }
    }

    class FloatOrCurveData
    {
        public bool isCurve;
        public float value;
        public AnimationCurve curve=new AnimationCurve();

        public void DrawDataArea(string label)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            isCurve = EditorGUILayout.Toggle(isCurve);
            if (isCurve)
                curve = EditorGUILayout.CurveField(curve);           
            else
                value = EditorGUILayout.FloatField(value);
            EditorGUILayout.EndHorizontal();
        }

        public bool CheckData(float time)
        {
            bool check = true;
            if (isCurve)
                check = curve.keys[curve.length - 1].time == time;
            return check;
        }
            
    }
}

using System.IO;
using UnityEngine;
using UnityEditor;
using CFEngine;

namespace DataEditor
{
    class AnimCurveData:DataBase
    {
        private AnimationCurve curve = new AnimationCurve();

        public AnimCurveData()
        {
            type = (byte)DataType.AnimationCurve;
        }

        public override void DrawDataArea()
        {
            base.DrawDataArea();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("曲线",GUILayout.Width(50f));
            curve = EditorGUILayout.CurveField(curve);
            EditorGUILayout.EndHorizontal();
        }

        public override void ReadData(BinaryReader reader)
        {
            base.ReadData(reader);
            curve = ReadCurve(reader);
        }

        public override void WriteData(BinaryWriter writer)
        {
            base.WriteData(writer);
            WriteCurve(writer, curve);
        }


    }
}

using System.IO;
using UnityEngine;
using UnityEditor;
using CFEngine;

namespace DataEditor
{
    class WeatherData:DataBase
    {
        public float useTime;
        public float darkValueInRain;
        public float rainRoughness;
        public float rippleTilling;
        public float rippleIntensity;
        public float rippleSpeed;
        public bool useLighting;
        public string rainPrefab;

        public float normalTSScale;

        public WeatherData()
        {
            type = (byte)DataType.Weather;
        }

        public override void DrawDataArea()
        {
            base.DrawDataArea();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UseTime", GUILayout.Width(100f));
            useTime = EditorGUILayout.FloatField(useTime, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("DarkValueInRain", GUILayout.Width(100f));
            darkValueInRain = EditorGUILayout.FloatField(darkValueInRain, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("RainRoughness", GUILayout.Width(100f));
            rainRoughness = EditorGUILayout.FloatField(rainRoughness, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("RippleTilling", GUILayout.Width(100f));
            rippleTilling = EditorGUILayout.FloatField(rippleTilling, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("RippleIntensity", GUILayout.Width(100f));
            rippleIntensity = EditorGUILayout.FloatField(rippleIntensity, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("RippleSpeed", GUILayout.Width(100f));
            rippleSpeed = EditorGUILayout.FloatField(rippleSpeed, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("NormalTSScale", GUILayout.Width(100f));
            normalTSScale = EditorGUILayout.FloatField(normalTSScale, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UseLighting", GUILayout.Width(100f));
            useLighting = EditorGUILayout.Toggle(useLighting, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("RainPrefab", GUILayout.Width(100f));
            rainPrefab = EditorGUILayout.TextArea(rainPrefab, GUILayout.Width(125f));
            EditorGUILayout.EndHorizontal();
        }

        public override void ReadData(BinaryReader reader)
        {
            base.ReadData(reader);
            useTime = reader.ReadSingle();
            darkValueInRain=reader.ReadSingle();
            rainRoughness = reader.ReadSingle();
            rippleTilling = reader.ReadSingle();
            rippleIntensity = reader.ReadSingle();
            rippleSpeed = reader.ReadSingle();
            useLighting = reader.ReadBoolean();
            rainPrefab = reader.ReadString();
            if (version == 2)
                normalTSScale = reader.ReadSingle();
        }

        public override void WriteData(BinaryWriter writer)
        {
            base.WriteData(writer);
            writer.Write(useTime);
            writer.Write(darkValueInRain);
            writer.Write(rainRoughness);
            writer.Write(rippleTilling);
            writer.Write(rippleIntensity);
            writer.Write(rippleSpeed);
            writer.Write(useLighting);
            writer.Write(rainPrefab);

            writer.Write(normalTSScale);
        }
    }
}

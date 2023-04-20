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
    class DataEditor:EditorWindow
    {
        public static DataEditor Instance { get; set; }
        private const string ConfigPath = "/Editor/DataEditor/DataConfig/DataConfig.xml";
        private const string BytesPath = "{0}CommonData/{1}/{2}.txt";

        private DataBase curData;
        private int curSelectType;

        [MenuItem("Window/DataEditor")]
        public static void InitWindow()
        {
            DataEditor window = EditorWindow.GetWindowWithRect<DataEditor>(new Rect(0, 0, 800, 600));
            window.titleContent = new GUIContent("DataEditor");
            window.Show();
            Instance = window;
        }

        private void OnGUI()
        {
            DrawToolBarArea();
            DrawDataArea();
        }

        private void DrawToolBarArea()
        {
            GUILayout.BeginHorizontal(BluePrint.BlueprintStyles.Toolbar());
            if (GUILayout.Button("Create"))
                CreateNewData();
            curSelectType = (int)(DataType)EditorGUILayout.EnumPopup("数据类型", (DataType)curSelectType);
            if (GUILayout.Button("Save"))
                SaveData();
            if (GUILayout.Button("Load"))
                ReadData();
            GUILayout.EndHorizontal();
        }
        private AnimationCurve test = new AnimationCurve();
        private FloatOrCurveData test1 = new FloatOrCurveData();
        private FloatOrCurveData test2 = new FloatOrCurveData();
        private FloatOrCurveData test3 = new FloatOrCurveData();

        //private AnimationCurve ConvertToCurve(FloatOrCurveData x, FloatOrCurveData y, FloatOrCurveData z,
        //    float fadeInTime,float loopTime,float fadeOutTime)
        //{
        //    AnimationCurve curve = new AnimationCurve();
        //    int xLen = x.isCurve ? x.curve.length : 1,
        //        yLen = y.isCurve ? y.curve.length : 1,
        //        zLen = z.isCurve ? z.curve.length : 2;//如果fadeout是个固定值的话 需要添加曲线的尾点
        //    int keyCount = xLen + yLen + zLen;
        //    Keyframe[] keys = new Keyframe[keyCount];
        //    for (var i = 0; i < keyCount; i++)
        //    {
        //        if (i < xLen)//淡入
        //        {
        //            if (!x.isCurve)
        //                keys[i] = new Keyframe(0, x.value, 0, 0);
        //            else
        //                keys[i] = x.curve.keys[i];
        //        }
        //        else if (i >= xLen + yLen)//淡出
        //        {
        //            if (!z.isCurve)
        //            {
        //                if (i == xLen + yLen)
        //                    keys[i] = new Keyframe(fadeInTime + loopTime, z.value, Mathf.Infinity, 0);
        //                else
        //                    keys[i] = new Keyframe(fadeInTime + loopTime+fadeOutTime, z.value, 0, 0);
        //            }
        //            else
        //            {
        //                int index = i - xLen - yLen;
        //                if (i == xLen + yLen)
        //                    keys[i] = new Keyframe(fadeInTime + loopTime + z.curve.keys[index].time,
        //                    z.curve.keys[index].value, Mathf.Infinity, z.curve.keys[index].outTangent);
        //                else
        //                    keys[i] = new Keyframe(fadeInTime + loopTime + z.curve.keys[index].time,
        //                        z.curve.keys[index].value, keys[i - 1].outTangent, z.curve.keys[index].outTangent);
        //            }
        //        }
        //        else//循环
        //        {
        //            if (!y.isCurve)
        //            {
        //                keys[i] = new Keyframe(fadeInTime, y.value, Mathf.Infinity, 0);
        //            }
        //            else
        //            {
        //                int index = i - xLen;
        //                if (i == xLen)
        //                    keys[i] = new Keyframe(fadeInTime + y.curve.keys[index].time, y.curve.keys[index].value,
        //                        Mathf.Infinity, y.curve.keys[index].outTangent);
        //                else
        //                    keys[i] = new Keyframe(fadeInTime + y.curve.keys[index].time, y.curve.keys[index].value,
        //                        keys[i - 1].outTangent, y.curve.keys[index].outTangent);
        //            }
        //        }
        //    }
        //    curve.keys = keys;
        //    curve.preWrapMode = WrapMode.Default;
        //    curve.postWrapMode = WrapMode.Default;
        //    return curve;
        //}

        private void DrawDataArea()
        {
            if (curData == null)
                return;
            EditorGUILayout.LabelField(string.Format("数据类型：   {0}", (DataType)curData.type));
            scrollPos =EditorGUILayout.BeginScrollView(scrollPos);
            curData.DrawDataArea();
            EditorGUILayout.EndScrollView();
        }

        private Vector2 scrollPos;

        private void DrawBlurDataArea()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            EditorGUILayout.EndScrollView();
        }

        private void CreateNewData()
        {
            switch ((DataType)curSelectType)
            {
                case DataType.Blur:
                    curData = new BlurData();
                    break;
                case DataType.AnimationCurve:
                    curData = new AnimCurveData();
                    break;
                case DataType.Weather:
                    curData = new WeatherData();
                    break;
            }
        }

        private void SaveData()
        {
            if(string.IsNullOrEmpty(curData.name))
            {
                ShowNotification(new GUIContent("文件名不能为空"), 3);
                return;
            }
            if(!curData.CheckData())
            {
                ShowNotification(new GUIContent("请检查数据合法性"), 3);
                return;
            }
            string path = string.Format(BytesPath, LoadMgr.singleton.BundlePath, (DataType)curData.type, curData.name);
            FileStream fs = new FileStream(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fs);
            curData.WriteData(writer);
            writer.Close();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        public static void ReadData(ref DataBase data,string path)
        {
            ReadData(path, ref data);
        }

        private void ReadData()
        {
            string path = EditorUtility.OpenFilePanel("加载数据", "Assets/BundleRes/CommonData", "txt");
            if(!string.IsNullOrEmpty(path))
            {
                ReadData(path,ref curData);
            }
        }

        public static void ReadData(string path,ref DataBase data)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryReader reader = new BinaryReader(fs);
            var version = reader.ReadByte();
            var type = reader.ReadByte();
            var priority = reader.ReadInt32();
            var loop = reader.ReadBoolean();
            switch ((DataType)type)
            {
                case DataType.Blur:
                    data = new BlurData()
                    {
                        version=version,
                        type = type,
                        name = path.Substring(path.LastIndexOf('/') + 1).Replace(".txt", string.Empty),
                        priority = priority,
                        loop = loop
                    };
                    break;
                case DataType.AnimationCurve:
                    data = new AnimCurveData()
                    {
                        version = version,
                        type = type,
                        name = path.Substring(path.LastIndexOf('/') + 1).Replace(".txt", string.Empty),
                        priority = priority,
                        loop = loop
                    };
                    break;
                case DataType.Weather:
                    data = new WeatherData()
                    {
                        version = version,
                        type = type,
                        name = path.Substring(path.LastIndexOf('/') + 1).Replace(".txt", string.Empty),
                        priority = priority,
                        loop = loop
                    };
                    break;
            }
            data.ReadData(reader);
            reader.Close();
        }

    }
}

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TDTools
{

    public class CurveHelperWindow : EditorWindow
    {
        private static readonly string CurveFolderPath = "Editor/EditorResources/Curve";
        private static readonly string CurveHelperWindowUxmlPath = "Assets/Editor/TDTools/CurveHelper/CurveHelperWindow.uxml";
        private static readonly string CurveItemUxmlPath = "Assets/Editor/TDTools/CurveHelper/CurveItem.uxml";
        private VisualTreeAsset itemAsset;
        private CurveHelperData chd;
        private FileSelectorData SelectFsd;
        private FileSelectorData SaveFsd;
        private List<CurveItemData> list = new List<CurveItemData>();
        private int curveDim = 0;
        private List<float[]> result = new List<float[]>();
        private ListView listView;
        private Button btn;
        private HelpBox helpBox;
        [MenuItem("Tools/TDTools/关卡相关工具/曲线txt切割工具")]
        public static void ShowWindow()
        {
            var win = GetWindowWithRect<CurveHelperWindow>(new Rect(0, 0, 600, 720));
            win.Show();
        }

        private void OnEnable()
        {
            result.Clear();
            BindMainData();
            itemAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CurveItemUxmlPath);
            RegisterCallback();
            InitTemplate();
            ChangeShow();
        }

        private void InitTemplate()
        {
            var select = rootVisualElement.Q<TemplateContainer>("FileSelect");
            SelectFsd = CreateInstance<FileSelectorData>();
            SelectFsd.FileTitle = "曲线路径";
            select.BindAndRegister(SelectFsd, obj => { SelectFile(); });

            var save = rootVisualElement.Q<TemplateContainer>("SaveSelect");
            SaveFsd = CreateInstance<FileSelectorData>();
            SaveFsd.FileTitle = "保存路径";
            save.BindAndRegister(SaveFsd, obj => { SelectSavePath(); });
        }

        private void ChangeShow()
        {
            if(CheckBtnShow(out string desc))
            {
                btn.visible = true;
                helpBox.visible = false;
            }
            else
            {
                helpBox.text = desc;
                helpBox.visible = true;
                btn.visible = false;
            }
        }

        private void RegisterCallback()
        {
            rootVisualElement.Q<Button>("Btn").RegisterCallback<MouseUpEvent>(obj => { SaveFile(); });
            rootVisualElement.Q<Button>("CurveBtn").RegisterCallback<MouseUpEvent>(obj => { XEditor.XCurveExtractor.CurveOpener(); });
            rootVisualElement.Q<FloatField>("StartPos").RegisterCallback<ChangeEvent<float>>(evt => { ChangeShow(); });
            rootVisualElement.Q<FloatField>("EndPos").RegisterCallback<ChangeEvent<float>>(evt => { ChangeShow(); });
        }

        private void BindMainData()
        {
            chd = CreateInstance<CurveHelperData>();
            var so = new SerializedObject(chd);
            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CurveHelperWindowUxmlPath);
            vta.CloneTree(rootVisualElement);
            rootVisualElement.Bind(so);
            btn = rootVisualElement.Q<Button>("Btn");
            helpBox = rootVisualElement.Q<HelpBox>("HelpBox");
            listView = rootVisualElement.Q<ListView>("ShowPoint");
            listView.makeItem = () => { return itemAsset.CloneTree(); };
            listView.bindItem = (e, i) => CreateListViewItem(e, list[i]);
            listView.itemHeight = 18;
        }

        private bool CheckBtnShow(out string desc)
        {
            bool show = true;
            StringBuilder help = new StringBuilder();
            if(string.IsNullOrEmpty(SelectFsd.FilePath))
            {
                help.AppendLine("请选择要分割的曲线文件！");
                show = false;
            }
            if (chd.StartPos < 0f)
            {
                help.AppendLine("请选择正确的起始时间点！");
                show = false;
            }
            if (chd.EndPos <= chd.StartPos)
            {
                help.AppendLine("请选择正确的结束时间点！");
                show = false;
            }
            if (string.IsNullOrEmpty(SaveFsd.FilePath))
            {
                help.AppendLine("请选择要保存的路径！");
                show = false;
            }
            desc = help.ToString();
            return show;
        }

        public void SelectFile()
        {
            SelectFsd.FilePath = EditorUtility.OpenFilePanel("选择曲线文件", $"{Application.dataPath}/{CurveFolderPath}", "txt");
            LoadFile();
            ChangeShow();
        }

        public void SelectSavePath()
        {
            string defaultPath = string.IsNullOrEmpty(SelectFsd.FilePath) ? $"{Application.dataPath}/{CurveFolderPath}" :
                Path.GetDirectoryName(SelectFsd.FilePath);
            string defaultName = string.IsNullOrEmpty(SelectFsd.FilePath) ? "" : Path.GetFileNameWithoutExtension(SelectFsd.FilePath);
            SaveFsd.FilePath = EditorUtility.SaveFilePanel("选择保存文件夹", defaultPath, defaultName, "txt");
            ChangeShow();
        }

        public void LoadFile()
        {
            if (string.IsNullOrEmpty(SelectFsd.FilePath))
            {
                OutError("请选择曲线文件！");
                return;
            }
            if (!File.Exists(SelectFsd.FilePath))
            {
                OutError("选择的曲线文件不存在！");
                return;
            }
            if (Path.GetExtension(SelectFsd.FilePath).ToLower() != ".txt")
            {
                OutError("选择的曲线文件格式错误！");
                return;
            }
            string name = Path.GetFileNameWithoutExtension(SelectFsd.FilePath);
            list.Clear();
            using (StreamReader reader = new StreamReader(File.Open(SelectFsd.FilePath, FileMode.Open)))
            {
                while(true)
                {
                    var data = reader.ReadLine();
                    if (data == null) break;
                    string[] datas = data.Split('\t');
                    curveDim = datas.Length;
                    var itemData = CreateInstance<CurveItemData>();
                    itemData.TimePoint = float.Parse(datas[0]);
                    itemData.Forward = float.Parse(datas[1]);
                    if (datas.Length > 2)
                        itemData.Side = float.Parse(datas[2]);
                    if (datas.Length > 3)
                        itemData.Up = float.Parse(datas[3]);
                    if (datas.Length > 4)
                        itemData.Rotate = float.Parse(datas[4]);
                    list.Add(itemData);
                }
            }
            listView.itemsSource = list;
        }

        private void DoDivide()
        {
            float[] reference = new float[curveDim];
            bool output = false;
            foreach(var item in list)
            {
                if(float.Equals(item.TimePoint, chd.StartPos))
                {
                    reference = GetCurveFloat(item);
                    output = true;
                }

                if (output)
                {
                    result.Add(GetNewCurveRes(GetCurveFloat(item), reference));
                }

                if (float.Equals(item.TimePoint, chd.EndPos))
                {
                    output = false;
                    return;
                }
            }
        }

        private float[] GetNewCurveRes(float[] item, float[] reference)
        {
            float[] result = new float[curveDim];
            for (int i = 0; i < item.Length; ++i)
            {
                result[i] = item[i] - reference[i];
            }
            return result;
        }

        private float[] GetCurveFloat(CurveItemData item)
        {
            float[] result = new float[curveDim];
            result[0] = item.TimePoint;
            result[1] = item.Forward;
            if (curveDim > 2)
                result[2] = item.Side;
            if (curveDim > 3)
                result[3] = item.Up;
            if (curveDim > 4)
                result[4] = item.Rotate;
            return result;
        }

        private void SaveFile()
        {
            DoDivide();
            using (var writer = File.CreateText(SaveFsd.FilePath))
            {
                foreach (var item in result)
                {
                    string res = string.Join<float>("\t", item);
                    writer.WriteLine(res);
                }
            }
            EditorUtility.DisplayDialog("成功", "曲线切割完成", "确定");
            result.Clear();
        }

        private void OutError(string content)
        {
            Debug.Log(content);
            EditorUtility.DisplayDialog("出错了", content, "确定");
        }

        private void CreateListViewItem(VisualElement item, CurveItemData data)
        {
            var so = new SerializedObject(data);
            item.Bind(so);
            item.Q<Button>("LeftBtn").RegisterCallback<MouseUpEvent>(obj => { chd.StartPos = data.TimePoint; });
            item.Q<Button>("RightBtn").RegisterCallback<MouseUpEvent>(obj => { chd.EndPos = data.TimePoint; });
        }
    }

    public class CurveHelperData : ScriptableObject
    {
        public float StartPos = 0f;
        public float EndPos = 0f;
        public CurveHelperData()
        {
            StartPos = 0f;
            EndPos = 0f;
        }
    }

    public class CurveItemData : ScriptableObject
    {
        public float TimePoint = 0f;
        public float Forward = 0f;
        public float Side = 0f;
        public float Up = 0f;
        public float Rotate = 0f;
    }
}
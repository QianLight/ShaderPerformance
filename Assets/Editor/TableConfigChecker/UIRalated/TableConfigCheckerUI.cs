using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace TDTools
{
    public class TableConfigCheckerUI : EditorWindow
    {
        public static List<string> FinnalResult;
        public static List<MethordInfo> AllMethords = new List<MethordInfo>();           
        public static Dictionary<int, MethordInfo> FinnalMethords = new Dictionary<int, MethordInfo>();
        public static Dictionary<int,string> FinnalParam = new Dictionary<int, string>();
        public static ScrollView TableNameScroller;
        public static ScrollView MethordsScroller;
        public static int MethordIndex = 0;
        

        static TableConfigCheckerUI()
        {
            FinnalResult = new List<string>();
        }

        [MenuItem("Tools/Table/配置检查工具 %&,")]
        public static void ShowWindow()
        {
            var win = GetWindowWithRect<TableConfigCheckerUI>(new Rect(0, 0, 900, 700), true, "配置检查工具");
            win.Show();
            win.Focus();
        }
        public void OnEnable()
        {
            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TableConfigChecker/UIRalated/TableConfigCheckerUI.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/TableConfigChecker/UIRalated/TableConfigCheckerUI.uss");
            labelFromUXML.styleSheets.Add(styleSheet);
            root.Add(labelFromUXML);

            var SelectButton = rootVisualElement.Q<Button>("SelectButton");
            SelectButton.RegisterCallback<MouseUpEvent>(obj => ChooseCheckObject());
            var ResultButton = rootVisualElement.Q<Button>("ResultButton");
            ResultButton.RegisterCallback<MouseUpEvent>(obj => ShowResultWindow());
            var CheckButton = rootVisualElement.Q<Button>("CheckButton");
            CheckButton.RegisterCallback<MouseUpEvent>(obj => DoCheck());
            var AllSelectToggle = rootVisualElement.Q<ToolbarToggle>("AllSelectToggle");
            AllSelectToggle.RegisterCallback<MouseUpEvent>(obj => SelectAll());
            var ConfigCheckButton = rootVisualElement.Q<Button>("ConfigCheckButton");
            ConfigCheckButton.RegisterCallback<MouseUpEvent>(obj => RunTableCheckByConfig.DoCheckByFile());

            TableNameScroller = rootVisualElement.Q<ScrollView>("TableNameScroller");
            MethordsScroller = rootVisualElement.Q<ScrollView>("MethordsScroller");

        }

        void ChooseCheckObject()
        {
            SelectCheckObjectUI.ShowExample();          
        }

        public static void PlaceCheckItem()   //全部写成静态函数了，好不好啊？
        {
            foreach (var methord in AllMethords)
            {
                MethordsScroller.Add(GenerateCheckItemBar(methord));
            }
        }
        public static void RemoveCheckItem(string ObjectName)  
        {
            List<VisualElement> transferlist = new List<VisualElement>();
            foreach (var methordbar in MethordsScroller.Children())
            {
                if(methordbar.name == ObjectName)
                {
                    transferlist.Add(methordbar);
                }
            }
            foreach (var methordbar in transferlist)
            {
                MethordsScroller.Remove(methordbar);
            }
        }

        public static VisualElement GenerateCheckItemBar(MethordInfo TheMethord)
        {
            int Indexvalue = MethordIndex;
            VisualElement MethordBar = new VisualElement{name = TheMethord.ObjectName};
            MethordBar.style.flexDirection = FlexDirection.Row;
            MethordBar.style.justifyContent = Justify.SpaceBetween;
            MethordBar.style.width = 900;
            Toggle EnableToggle = new Toggle();
            EnableToggle.RegisterCallback<MouseUpEvent>(obj => SwitchState(Indexvalue, EnableToggle.value,TheMethord));
            Label label_Name = new Label($"{TheMethord.ObjectName}: {TheMethord.MethordName}");
            Label label_Tip = new Label($"{TheMethord.Tips}");
            if (TheMethord.MethordName == "ForeignkeyCheck")
                label_Tip.tooltip = "请依次输入：本表列名|目标表名|目标列名";
            if (TheMethord.MethordName == "SkillNameExistCheck"|| TheMethord.MethordName == "SkillScriptExistCheck")
                label_Tip.tooltip = "可以输入All检查常规的八个技能列";
            TextField textField = new TextField();
            textField.RegisterCallback<FocusOutEvent>(obj => UpdataParam(Indexvalue, TheMethord.ObjectName, textField.value));
            textField.tooltip = "输入目标列名，需要输入多个参数的请用 | 进行分隔";
            //textField.style.width = 50;
            Label label_Importance = new Label($"{TheMethord.Importance}");
            VisualElement WantNear = new VisualElement();
            WantNear.style.flexDirection = FlexDirection.Row;
            WantNear.style.marginRight = 20;
            WantNear.Add(EnableToggle);
            WantNear.Add(label_Name);
            MethordBar.Add(WantNear);
            MethordBar.Add(label_Tip);
            MethordBar.Add(textField);
            MethordBar.Add(label_Importance);
            MethordIndex++;

            return MethordBar;
        }

        public static void SwitchState(int index, bool enable, MethordInfo TheMethord)
        {
            if (enable)
            {
                FinnalMethords.Add(index,TheMethord); 
            }
            else
            {
                FinnalMethords.Remove(index);
            }
        }

        public static void UpdataParam(int index , string objectname , string colunmname)
        {
            if (FinnalParam.ContainsKey(index))
            {
                FinnalParam[index] = $"{objectname},{colunmname}";
            }
            else
            {
                FinnalParam.Add(index, $"{objectname},{colunmname}");
            }   
        }

        void DoCheck()
        {
            foreach (var Methord in FinnalMethords)
            {
                string[] param = FinnalParam[Methord.Key].Split(',');
                Methord.Value.TheMethord($"{param[0]}", $"{param[1]}");
            }
        }
       
        static public void ResultOutput(string result)
        {
            FinnalResult.Add(result);
        }

        void ShowResultWindow()
        {
            ResultUI.ShowExample();
        }

        void SelectAll()
        {

        }
    }
}

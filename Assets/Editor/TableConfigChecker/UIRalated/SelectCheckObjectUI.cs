using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace TDTools
{
    public class SelectCheckObjectUI : EditorWindow
    {
        string[] AllObjects = new string[]
        {
         "SceneList",
         "UnitAITable",
         "XEntityStatistics"
        };
        static SelectCheckObjectUI win;
        static ScrollView ChoosenArea;
        static ScrollView StandbyArea;
        List<string> ChoosenObjects = new List<string>();    //还是要把数据和行为拆开，组件的contain函数不方便通过name来搜索，所以要这个东西
        List<string> NowObjects = new List<string>();         //这个逻辑也有问题
        ScenelistChecker SLC;
        UnitAITableChecker UATC;
        XEntityStatisticsChecker ESC;


        public static void ShowExample()
        {
            win = GetWindowWithRect<SelectCheckObjectUI>(new Rect(0, 0, 450, 300), true, "检查对象筛选");
            win.Show();
            win.Focus();
        }

        public void OnEnable()
        {
            SLC = new ScenelistChecker();
            UATC = new UnitAITableChecker();
            ESC = new XEntityStatisticsChecker();

            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TableConfigChecker/UIRalated/SelectCheckObjectUI.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/TableConfigChecker/UIRalated/SelectCheckObjectUI.uss");
            labelFromUXML.styleSheets.Add(styleSheet);
            root.Add(labelFromUXML);

            StandbyArea = rootVisualElement.Q<ScrollView>("StandbyArea");
            ChoosenArea = rootVisualElement.Q<ScrollView>("ChoosenArea");
            var ConfimButton = rootVisualElement.Q<Button>("ConfimButton");
            ConfimButton.RegisterCallback<MouseUpEvent>(obj => ConfimObjects());

            foreach (Label obj in TableConfigCheckerUI.TableNameScroller.Children())
            {
                NowObjects.Add(obj.text);
            }
            foreach (var obj in NowObjects)
            {
                Button SwitchStatebutton = new Button();
                SwitchStatebutton.name = SwitchStatebutton.text = obj;
                SwitchStatebutton.RegisterCallback<MouseUpEvent>(obj => SwitchState(SwitchStatebutton));
                ChoosenArea.Add(SwitchStatebutton);
                ChoosenObjects.Add(obj);
            }
            foreach (var obj in AllObjects)
            {
                if (!ChoosenObjects.Contains(obj))
                {
                    Button SwitchStatebutton = new Button();
                    SwitchStatebutton.name = SwitchStatebutton.text = obj;
                    SwitchStatebutton.RegisterCallback<MouseUpEvent>(obj => SwitchState(SwitchStatebutton));
                    StandbyArea.Add(SwitchStatebutton);
                }                
            }
        }

        void SwitchState(Button Child)
        {
            if (StandbyArea.Contains(Child))
            {
                StandbyArea.Remove(Child);
                ChoosenArea.Add(Child);
                ChoosenObjects.Add(Child.name);
            }
            else
            {
                ChoosenArea.Remove(Child);
                StandbyArea.Add(Child);
                ChoosenObjects.Remove(Child.name);
            }
        }
        void ConfimObjects()
        {
            List<Label> tensferlist = new List<Label>();
            foreach (Label obj in TableConfigCheckerUI.TableNameScroller.Children()) //看foreach的底层原理
            {
                if (!ChoosenObjects.Contains(obj.text))
                {
                    tensferlist.Add(obj);
                    TableConfigCheckerUI.RemoveCheckItem($"{obj.text}");  
                    NowObjects.Remove(obj.text);        
                }
            }
            foreach (Label obj in tensferlist)
            {
                TableConfigCheckerUI.TableNameScroller.Remove(obj);
            }

            foreach (var obj in ChoosenObjects)
            {
                if (!NowObjects.Contains(obj))  
                {
                    TableConfigCheckerUI.TableNameScroller.Add(new Label($"{obj}"));
                    if (obj == "SceneList")
                        SLC.PassMethords();
                    else if (obj == "UnitAITable")
                        UATC.PassMethords();
                    else if (obj == "XEntityStatistics")
                        ESC.PassMethords();
                }                
            }
            TableConfigCheckerUI.PlaceCheckItem();
            //List<VisualElement> list = new List<VisualElement>();
            //foreach (var obj in TableConfigCheckerUI.TableNameScroller.Children())
            //{
            //    list.Add(obj);
            //}
            //foreach (var obj in list)   //不中转一下会从循环里面跳出来，和foreach机制有关，回头看一下
            //{
            //    TableConfigCheckerUI.TableNameScroller.Remove(obj);
            //}
            win.Close();
        }
    }
}


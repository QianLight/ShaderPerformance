using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TDTools
{
    public class ResultUI : EditorWindow
    {
        static ResultUI win;
        ScrollView ResultScroller;

        [MenuItem("Window/UI Toolkit/ResultUI")]
        public static void ShowExample()
        {
            
            win = GetWindow<ResultUI>();
            win.title = "¼ì²â½á¹û";
            win.Show();
            win.Focus();
        }

        public void OnEnable()
        {
            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TableConfigChecker/UIRalated/ResultUI.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/TableConfigChecker/UIRalated/ResultUI.uss");
            labelFromUXML.styleSheets.Add(styleSheet);
            root.Add(labelFromUXML);

            var ClearButton = rootVisualElement.Q<Button>("ClearButton");
            ClearButton.RegisterCallback<MouseUpEvent>(obj => ClearResult());
            ResultScroller = rootVisualElement.Q<ScrollView>("ResultScroller");
            
            foreach (string result in TableConfigCheckerUI.FinnalResult)
            {
                Label label = new Label(result);
                //label.style.height = 20;
                label.style.fontSize = 15;
                ResultScroller.Add(label);
            }
        }

        void ClearResult()
        {
            ResultScroller.Clear();
        }
    }
}
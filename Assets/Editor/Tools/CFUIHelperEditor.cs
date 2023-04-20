using UnityEditor;
using UnityEngine;
using UnityEngine.CFUI;
using System.IO;

public class CFUIHelperEditor : EditorWindow 
{
    [MenuItem("Assets/UI/HelperWindow")]
    static void AddWindow()
    {
        //创建窗口
        Rect wr = new Rect(0, 0, 500, 500);
        CFUIHelperEditor window = (CFUIHelperEditor)EditorWindow.GetWindowWithRect(typeof(CFUIHelperEditor), wr, true, "UI 帮助工具");
        window.Show();
    }

    public void Awake()
    {

    }

    private string text;

    void OnGUI()
    {

        GUILayout.BeginVertical();

        GUILayout.Space(10);
        GUI.skin.label.fontSize = 24;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("资源名查找");

        GUILayout.Space(10);
        EditorGUILayout.LabelField("UI Prefab 路径： " + TransCFUITool.fullPath);
        GUILayout.Space(10);
        text = EditorGUILayout.TextField("输入要在UI中搜索的文字:", text);

        if (GUILayout.Button("查找文本"))
        {
            TransCFUITool.SearchText(text);
        }
        
        GUILayout.Space(20);
        if (GUILayout.Button("查找选中的资源引用(图集、图片)"))
        {
            var select = Selection.GetFiltered<Object>(SelectionMode.Unfiltered);
            string selectNames = "";
            if (select.Length == 1)
            {
                for (int i = 0;i <select.Length; ++i)
                {
                    selectNames += "\n" + select[i].name;
                }

                this.ShowNotification(new GUIContent(selectNames));

                TransCFUITool.SearchUIForSelected(select[0]);
            }
            else
            {
                this.ShowNotification(new GUIContent(select.Length == 0? "没有选中资源":"选中的资源不止一个."));
            }            
        }

        //if (GUILayout.Button("关闭通知", GUILayout.Width(200)))
        //{
        //    //关闭通知栏
        //    this.RemoveNotification();
        //}

        GUILayout.Space(10);
        if (GUILayout.Button("关闭窗口", GUILayout.Width(200)))
        {
            //关闭窗口
            this.Close();
        }

        GUILayout.EndVertical();

    }

    //更新
    //void Update()
    //{

    //}

    //void OnFocus()
    //{
    //    Debug.Log("当窗口获得焦点时调用一次");
    //}

    //void OnLostFocus()
    //{
    //    Debug.Log("当窗口丢失焦点时调用一次");
    //}

    //void OnHierarchyChange()
    //{
    //    Debug.Log("当Hierarchy视图中的任何对象发生改变时调用一次");
    //}

    //void OnProjectChange()
    //{
    //    Debug.Log("当Project视图中的资源发生改变时调用一次");
    //}

    //void OnInspectorUpdate()
    //{
    //    //Debug.Log("窗口面板的更新");
    //    //这里开启窗口的重绘，不然窗口信息不会刷新
    //    this.Repaint();
    //}

    //void OnSelectionChange()
    //{
    //    //当窗口出去开启状态，并且在Hierarchy视图中选择某游戏对象时调用
    //    foreach (Transform t in Selection.transforms)
    //    {
    //        //有可能是多选，这里开启一个循环打印选中游戏对象的名称
    //        Debug.Log("OnSelectionChange" + t.name);
    //    }
    //}

    //void OnDestroy()
    //{
    //    Debug.Log("当窗口关闭时调用");
    //}
}
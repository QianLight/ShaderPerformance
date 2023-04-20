using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using CustomPropertyDrawer = UnityEditor.CustomPropertyDrawer;
using CFUtilPoolLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class XGuideSoUtility
{
    private static readonly string func_desc_format = "格式:{0}";
    public static string[] func_strs = {
        "不使用","触发条件","完成条件","失败条件","操作指令","显示提示","附加功能","执行函数"};
    public static string[] func_keys_default = { "空" };
    public static string[] func_begin = { "系统开启", "玩家等级", "伙伴等级","航海船等级","exstring" };
    public static string[] func_begin_desc = {"系统ID(int)", "玩家等级(int)", "伙伴等级(int)","船等级(int)" , "字符串[关卡等触发 string]"};
    public static string[] func_cmd = { "系统开启", "点击UI" };
    public static string[] func_cmd_desc = {"系统ID(int)","点击的UI节点路径(string)"};
    public static string[] func_end = { "时间", "完成点击" };
    public static string[] func_end_desc = {"时间[float]", "空"};
    public static string[] func_method = { "showskill", "setskill" };
    public static string[] func_method_desc = {"技能ID[string]","技能ID[string]"};
    // public static string[] func_shin = { "提示框" };
    // public static string[] func_skin_desc = {"坐标+内容[x:y:z,string]"};
    public static string[] func_extend = { "播放声音", "显示跳过", "时间延迟", "成功Next", "失败Next" };
    public static string[] func_extend_dec= {"声音名称[string]","空","时间[float]","引导节点ID[int]","引导节点ID[int]"};

    public static void LoadSkin(){
        string file = Application.dataPath + "/Table/Guide/XGuideSoContent.txt";
        if(!File.Exists(file)){
            Debug.LogError("not found file = "+file);
            return;
        }
        IEnumerable<string> table = File.ReadAllLines(file, Encoding.Default);
        IEnumerator<string> data = table.GetEnumerator();
        string[] reader = null;
        string line =  string.Empty;
        while(data.MoveNext()){
            line = data.Current;
            reader = line.Split('=');
            switch(reader[0]){
                case "func_strs":
                    func_strs = reader[1].Split('|');
                    break;
                case "func_keys_default":
                    func_keys_default = reader[1].Split('|');
                    break;
                       case "func_begin":
                    func_begin = reader[1].Split('|');
                    break;
                case "func_begin_desc":
                    func_begin_desc = reader[1].Split('|');
                    break;
                       case "func_cmd":
                    func_cmd = reader[1].Split('|');
                    break;
                case "func_cmd_desc":
                    func_cmd_desc = reader[1].Split('|');
                    break;
                           case "func_end":
                    func_end = reader[1].Split('|');
                    break;
                case "func_end_desc":
                    func_end_desc = reader[1].Split('|');
                    break;
                case "func_method":
                    func_method = reader[1].Split('|');
                    break;
                case "func_method_desc":
                    func_method_desc = reader[1].Split('|');
                    break;
                // case "func_shin":
                //     func_shin = reader[1].Split('|');
                //     break;
                // case "func_skin_desc":
                //     func_skin_desc = reader[1].Split('|');
                //     break;
                case "func_extend":
                    func_extend = reader[1].Split('|');
                    break;
                case "func_extend_dec":
                    func_extend_dec = reader[1].Split('|');
                    break;
                default:
                    Debug.LogWarning("not found key:"+ reader[0]);
                   break;
            }
        }
    }

    public static string[] GetFunKeys(int index)
    {
        string[] funs = null;
        switch (index)
        {
            case XGuideID.Guide_Cmd_BeginCond:
                funs = func_begin;
                break;
            case XGuideID.Guide_Cmd_FinishCond:
            case XGuideID.Guide_Cmd_FailCond:
                funs = func_end;
                break;
            case XGuideID.Guide_Cmd_Handler:
                funs = func_cmd;
                break;
            // case XGuideID.Guide_Cmd_Skin:
            //     funs = func_shin;
            //     break;
            case XGuideID.Guide_Cmd_Method:
                funs = func_method;
                break;
            case XGuideID.Guide_Cmd_Extends:
                funs = func_extend;
                break;
            default:
                funs = func_keys_default;
                break;
        }
        return funs;
    }

    private static GUIContent content = new GUIContent();
    public static GUIContent GetFunDesc(int fuc,int id){
        string desc = "None";
        switch (fuc)
        {
            case XGuideID.Guide_Cmd_BeginCond:
                GetDescString(id,func_begin_desc,ref desc);
                break;
            case XGuideID.Guide_Cmd_FinishCond:
            case XGuideID.Guide_Cmd_FailCond:
                GetDescString(id,func_end_desc,ref desc);
                break;
            case XGuideID.Guide_Cmd_Handler:
                GetDescString(id,func_cmd_desc,ref desc);
                break;
            // case XGuideID.Guide_Cmd_Skin:
            //     GetDescString(id,func_skin_desc,ref desc);
            //     break;
            case XGuideID.Guide_Cmd_Method:
                 GetDescString(id,func_method_desc,ref desc);
                break;
            case XGuideID.Guide_Cmd_Extends:
                GetDescString(id,func_extend_dec,ref desc);
                break;
        }
        content.text = string.Format( func_desc_format,desc);
        return content;    
    }

    private static void GetDescString(int index,string[] values , ref string value){
        if(index < values.Length) 
            value = values[index];
        else 
            value = "None";
    }
}

[CustomEditor(typeof(XGuideProcessListSo))]
public class XGuideProcessSoEditor:XGuideSoEditor
{
    //[MenuItem("Assets/Guide/Create ProcessSo")]
    //public static void CreateGuideProcessSO()
    //{
    //    try
    //    {
    //        var asset = ScriptableObject.CreateInstance<XGuideProcessListSo>();
    //        if (asset == null)
    //        {
    //            XDebug.singleton.AddErrorLog("无法识别！");
    //            return;
    //        }

    //        string fullpath = string.Format(XGuideUtility.assets_path, "NewProcess");
    //        AssetDatabase.CreateAsset(asset, fullpath);
    //        AssetDatabase.SaveAssets();
    //        AssetDatabase.Refresh();
    //    }
    //    catch (Exception e)
    //    {
    //        XDebug.singleton.AddErrorLog(e.ToString());
    //    }
    //}

    protected virtual void OnEnable()
    {
        XGuideSoUtility.LoadSkin();
        SetSerializedObject(serializedObject.FindProperty("processSo"));
    }

    protected override void drawHeaderCallback(Rect rect)
    {
        base.drawHeaderCallback(rect);
        Rect bar = new Rect {
            height = EditorGUIUtility.singleLineHeight,
            x = rect.width - 200,
            width = 100
        };

        if(GUI.Button(bar, "导出Binary"))
        {
            (target as XGuideProcessListSo).EditorToData();
        }
    }
}

[CustomPropertyDrawer(typeof(XGuideProcessSO))]
public class XGuideProcessSoDrawer:XGuideDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUIUtility.labelWidth = 60;
        position.height = EditorGUIUtility.singleLineHeight;
        Rect stepRect = new Rect(position)
        {
            width = 200,
            x = position.x + 10
        };
      
        Rect othersRect = new Rect(stepRect)
        {
            y = stepRect.y + EditorGUIUtility.singleLineHeight + 2,
            width = position.width - 80
        };
        //找到每个属性的序列化值
        SerializedProperty stepProperty = property.FindPropertyRelative("step");
        SerializedProperty othersProperty = property.FindPropertyRelative("others");
        stepProperty.intValue = EditorGUI.IntField(stepRect, "步骤:", stepProperty.intValue);
        DrawOthers(othersRect, othersProperty);
    }
}

[CustomEditor(typeof(XGuideDataListSo))]
public class XGuideDataSoEditor: XGuideSoEditor
{
   
    //[MenuItem("Assets/Guide/Create DataSo")]
    //public static void CreateGuideDataSO()
    //{
    //    try
    //    {
    //        var asset = ScriptableObject.CreateInstance<XGuideDataListSo>();
    //        if (asset == null)
    //        {
    //            XDebug.singleton.AddErrorLog("无法识别！");
    //            return;
    //        }

    //        string fullpath = string.Format(XGuideUtility.assets_path, "GuideTable");
    //        AssetDatabase.CreateAsset(asset, fullpath);
    //        AssetDatabase.SaveAssets();
    //        AssetDatabase.Refresh();
    //    }
    //    catch (Exception e)
    //    {
    //        XDebug.singleton.AddErrorLog(e.ToString());
    //    }
    //}
    protected virtual void OnEnable()
    {
        XGuideSoUtility.LoadSkin();
        SetSerializedObject(serializedObject.FindProperty("dataSo"));
    }


    protected override void drawHeaderCallback(Rect rect)
    {
        EditorGUI.LabelField(rect,"目录  说明:[不使用,触发条件,完成条件,失败条件]有效");
        Rect bar = new Rect
        {
            height = EditorGUIUtility.singleLineHeight,
            x = rect.width - 200,
            width = 100
        };

        if (GUI.Button(bar, "导出Binary"))
        {
             (target as XGuideDataListSo).EditorToData();
        }
    }
}

[CustomPropertyDrawer(typeof(XGuideDataSO))]
public class XGuideDataSoDrawer : XGuideDrawer
{
    static class Styles
    {
        public static GUIStyle background = "RL Background";
        public static GUIStyle headerBackground = "RL Header";
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUIUtility.labelWidth = 60;
        position.height = EditorGUIUtility.singleLineHeight;

        Rect idRect = new Rect(position)
        {
            width = 100,
            x = position.x+10
        };
        Rect moduleRect = new Rect(idRect)
        {
            width = 400, 
            x = idRect.width  +10  + idRect.x ,
        };
        Rect scriptUrlRect = new Rect(moduleRect)
        {
            x = position.x+10,
            y = moduleRect.y + EditorGUIUtility.singleLineHeight + 2,
            width = position.width - 80
        };

        Rect othersRect = new Rect(scriptUrlRect)
        {
            y = scriptUrlRect.y + EditorGUIUtility.singleLineHeight + 2,
            width = position.width - 80
        };

        //找到每个属性的序列化值
        SerializedProperty idProperty = property.FindPropertyRelative("id");
        SerializedProperty moduleProperty = property.FindPropertyRelative("module");
        SerializedProperty scriptProperty = property.FindPropertyRelative("scriptObject");
        SerializedProperty othersProperty = property.FindPropertyRelative("others");
        idProperty.intValue = EditorGUI.IntField(idRect,idProperty.displayName,idProperty.intValue);
        moduleProperty.stringValue = EditorGUI.TextField(moduleRect, moduleProperty.displayName, moduleProperty.stringValue);
        scriptProperty.objectReferenceValue = EditorGUI.ObjectField(scriptUrlRect, "Script:" ,scriptProperty.objectReferenceValue,typeof(XGuideProcessListSo),false);
        DrawOthers(othersRect, othersProperty);
    }
  }
[CustomPropertyDrawer(typeof(XTriDataSo))]
public class XGuideTriDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty func = property.FindPropertyRelative("func");
        SerializedProperty funcKey = property.FindPropertyRelative("func_key");
        SerializedProperty funcValue = property.FindPropertyRelative("func_value");
        Rect fr = position;
        fr.width = 120;
        func.intValue = EditorGUI.Popup(fr, func.intValue, XGuideSoUtility.func_strs);
        Rect fvr = fr;
        fvr.x += fr.width + 10;
        funcKey.intValue = EditorGUI.Popup(fvr, funcKey.intValue, XGuideSoUtility.GetFunKeys(func.intValue));
        Rect fsr = fvr;
        fsr.x += fr.width + 10;
        fsr.width = position.width - 350;
        funcValue.stringValue = EditorGUI.TextField(fsr, funcValue.stringValue);
        GUIContent content = XGuideSoUtility.GetFunDesc(func.intValue,funcKey.intValue);
        Rect fss = fsr;
        fss.x += fsr.width+10;
        fsr.width = 250;
        EditorGUI.LabelField(fss,content);
    }
}
public class XGuideDrawer: PropertyDrawer
{
    protected void DrawOthers(Rect Position, SerializedProperty property)
    {

        Rect rect = new Rect(Position)
        {
            width = 100,
            height = EditorGUIUtility.singleLineHeight

        };
        EditorGUI.LabelField(rect, "其他:");

        Rect addRect = new Rect(Position)
        {
            x = rect.x + 120,
            height = 20,
            width = 20,
        };

        if (GUI.Button(addRect, "+"))
        {
            property.arraySize++;
        }
        Rect prRect = new Rect(addRect);
        for (int i = 0; i < property.arraySize; i++)
        {
            prRect.y += addRect.height + 3;
            prRect.x = Position.x;
            prRect.height = 20;
            prRect.width = 20;
            if (GUI.Button(prRect, "-"))
            {
                property.DeleteArrayElementAtIndex(i);
                break;
            }
            prRect.x += prRect.width + 10;
            prRect.width = Position.width - 100;
            SerializedProperty op = property.GetArrayElementAtIndex(i);
            EditorGUI.PropertyField(prRect, op);

        }

    }
}

public class XGuideSoEditor : Editor
{
    public static float space = 2f;
    private SerializedProperty dataSo = null;
    private ReorderableList reorderableList = null;


    protected virtual void OnDisable()
    {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
    }
    protected void SetSerializedObject(SerializedProperty so)
    {
        dataSo = so;
        reorderableList = new ReorderableList(serializedObject, dataSo, true, true, true, false);
        reorderableList.elementHeight = 100;
        reorderableList.footerHeight = 10;
        reorderableList.drawElementBackgroundCallback = drawElementBackgroundCallback;
        reorderableList.drawElementCallback = drawElementCallback;
        reorderableList.elementHeightCallback = elementHeightCallback;
        reorderableList.drawHeaderCallback = drawHeaderCallback;
    }

    protected virtual void drawHeaderCallback(Rect rect)
    {
        EditorGUI.LabelField(rect, dataSo.displayName);
        
    }

     protected virtual float elementHeightCallback(int index)
    {
        float size = 80;
        var ele = dataSo.GetArrayElementAtIndex(index);
        if (ele == null) return size;
        SerializedProperty other = ele.FindPropertyRelative("others");
        if (other != null)
        {
            size += other.arraySize * (EditorGUIUtility.singleLineHeight + space);
        }
        return size;
    }

    private void drawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        Rect back = new Rect(rect)
        {
            x = rect.x - space,
            y = rect.y - space,
            width = rect.width - space,
            height =rect.height - space
        };
        if(isFocused)
            EditorGUI.DrawRect(back, Color.yellow);
        else 
            EditorGUI.DrawRect(back, Color.gray);
    }
    public int m_deleteIndex = -1;
    private void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {

        var ele = dataSo.GetArrayElementAtIndex(index);
        rect.height -= 3f;
        EditorGUI.BeginProperty(rect, new GUIContent(ele.displayName), ele);
        EditorGUIUtility.labelWidth = 60;
  
        rect.height = EditorGUIUtility.singleLineHeight;
        Rect b = rect;
        b.x = rect.width - 10;
        b.width = 20;
        b.height = 20;
        if (GUI.Button(b, new GUIContent("X")))
        {
            m_deleteIndex = index;
            return;
        }
        b.x -= 120;
        EditorGUI.PropertyField(rect, ele);
        EditorGUI.EndProperty();

    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if(m_deleteIndex >= 0)
        {
            dataSo.DeleteArrayElementAtIndex(m_deleteIndex);
            m_deleteIndex = -1;
        }
        reorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }



    
}
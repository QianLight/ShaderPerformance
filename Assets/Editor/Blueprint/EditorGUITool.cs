using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEditor;
using System;

public class EditorGUITool
{
    public static Dictionary<string, string> TranslateDic
    {
        get
        {
            if (_translate_dic.Count == 0) LoadTranslateDic();
            return _translate_dic;
        }
    }
    private static Dictionary<string, string> _translate_dic = new Dictionary<string, string>();

    private static void LoadTranslateDic()
    {
        string path = Application.dataPath + "/Editor/Blueprint/SkillEditor/TranslateDic.txt";
        _translate_dic.Clear();

        using (StreamReader reader = File.OpenText(path))
        {
            if (reader == null) return;

            while (!reader.EndOfStream)
            {
                string[] keys = reader.ReadLine().Split('\t');
                _translate_dic[keys[0]] = keys[1];
            }
        }
    }

    public static void Vector3Field(string label, ref float x, ref float y, ref float z, float maxValue = float.MaxValue, [CallerFilePath] string path = "")
    {
        Vector3 tmp = new Vector3(x, y, z);
        tmp = EditorGUILayout.Vector3Field(new GUIContent(label, Translate(GetTypeString(path) + "_" + label)), tmp);
        x = Mathf.Min(maxValue, tmp.x);
        y = Mathf.Min(maxValue, tmp.y);
        z = Mathf.Min(maxValue, tmp.z);
    }

    public static string[] Translate<T>() where T : System.Enum
    {
        string[] source = Enum.GetNames(typeof(T));
        int length = source.Length - (source[source.Length - 1] == "Max" ? 1 : 0);
        string[] tmp = new string[length];
        for (int i = 0; i < length; ++i)
        {
            TranslateDic.TryGetValue(source[i], out tmp[i]);
            if (tmp[i] == null) tmp[i] = source[i];
        }

        return tmp;
    }

    public static string[] Translate(string[] source)
    {
        string[] tmp = new string[source.Length];
        for (int i = 0; i < source.Length; ++i)
        {
            TranslateDic.TryGetValue(source[i], out tmp[i]);
            if (tmp[i] == null) tmp[i] = source[i];
        }

        return tmp;
    }

    public static string Translate(string source)
    {
        string tmp = null;

        TranslateDic.TryGetValue(source, out tmp);
        if (tmp == null) tmp = source;

        return tmp;
    }

    public static string GetTypeString([CallerFilePath] string path = "")
    {
        string from = Path.GetFileNameWithoutExtension(path);
        if (from.EndsWith("Node")) from = from.Remove(from.Length - 4);
        else if (from.EndsWith("Graph")) from = from.Remove(from.Length - 5);
        return from;
    }
    public static GUIContent[] BuildGUIContent(string[] label, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        GUIContent[] content = new GUIContent[label.Length];
        for (int i = 0; i < label.Length; ++i)
            content[i] = new GUIContent(label[i], Translate(from + "_" + label[i]));
        return content;
    }
    public static int IntDataFloatField(string label, int value, float accurate = 100f, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        return Convert.ToInt32(accurate * EditorGUITool.FloatField(label, value / accurate, path));
    }
    public static float FloatField(string label, float value, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);        
        return EditorGUILayout.FloatField(new GUIContent(label, Translate(from + "_" + label)), value);
    }
    public static int IntField(string label, int value, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        return EditorGUILayout.IntField(new GUIContent(label, Translate(from + "_" + (label.IndexOf('(') != -1 ? label.Remove(label.IndexOf('(')) : label))), value);
    }
    public static int MaskField(string label, int value, string[] displayList, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        return EditorGUILayout.MaskField(new GUIContent(label, Translate(from + "_" + label)), value, displayList);
    }
    public static string TextField(string label, string value, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        return EditorGUILayout.TextField(new GUIContent(label, Translate(from + "_" + label)), value);
    }
    public static void LabelField(string label, GUIStyle style, GUILayoutOption options = null, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        if (options != null) EditorGUILayout.LabelField(new GUIContent(label, Translate(from + "_" + label)), style, options);
        else EditorGUILayout.LabelField(new GUIContent(label, Translate(from + "_" + label)), style);
    }

    public static void LabelField(string label, GUILayoutOption options = null, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        if (options != null) EditorGUILayout.LabelField(new GUIContent(label, Translate(from + "_" + label)), options);
        else EditorGUILayout.LabelField(new GUIContent(label, Translate(from + "_" + label)));
    }
    public static void LabelField(string label, string value, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        EditorGUILayout.LabelField(new GUIContent(label, Translate(from + "_" + label)), value);
    }
    public static bool Toggle(string label, bool value, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        return EditorGUILayout.Toggle(new GUIContent(label, Translate(from + "_" + label)), value);
    }
    public static float Slider(string label, float value, float leftValue, float rightValue, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        return EditorGUILayout.Slider(new GUIContent(label, Translate(from + "_" + label)), value, leftValue, rightValue);
    }
    public static int Popup(string label, int value, string[] displayList, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        return EditorGUILayout.Popup(new GUIContent(label, Translate(from + "_" + label)), value, BuildGUIContent(displayList, path));
    }
    public static int Popup(int value, string[] displayList, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        return EditorGUILayout.Popup(value, BuildGUIContent(displayList, path));
    }
    public static UnityEngine.Object ObjectField(string label, UnityEngine.Object obj, Type type, bool allowSceneObj, [CallerFilePath] string path = "")
    {
        string from = GetTypeString(path);
        return EditorGUILayout.ObjectField(new GUIContent(label, Translate(from + "_" + label)), obj, type, allowSceneObj);
    }
    public static int FloatArrayField(string lable, int len, ref float[] array, ref bool expand)
    {
        if (array == null)
            array = new float[0];
        Rect rect = EditorGUILayout.GetControlRect(true);
        GUIContent configLabel = new GUIContent(lable, lable);

        float lensLabelWidth = GUI.skin.label.CalcSize(configLabel).x;
        float foldoutLabelWidth = lensLabelWidth;

        expand = EditorGUI.Foldout(
            new Rect(rect.x, rect.y, foldoutLabelWidth, rect.height),
            expand, configLabel, true);
        if (expand)
        {
            //EditorGUITool.LabelField(lable);

            len = EditorGUITool.IntField("☞Size", len);
            if (len != array.Length)
            {
                float[] tmpArray = new float[len];
                for (int i = 0; i < Math.Min(len, array.Length); ++i)
                {
                    tmpArray[i] = array[i];
                }
                array = tmpArray;
            }
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = EditorGUITool.FloatField(string.Format("☞☞Element {0}", i), array[i]);
            }
        }
        return array.Length;
    }
}

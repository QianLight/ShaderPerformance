using EditorNode;
using System.Collections.Generic;
using UnityEngine;
using BluePrint;
using UnityEditor;
using EcsData;
using System;
using System.Runtime.CompilerServices;

public abstract class BaseSkillGraph : BluePrintGraph
{
    public bool NeedInitRes = true;

    public List<string> FunctionName = new List<string>();
    public List<string> FunctionFlag = new List<string>();
    public List<int> FunctionHash = new List<int>();
    public Dictionary<string, int> FunctionName2Hash = new Dictionary<string, int>();
    public Dictionary<int, string> FunctionHash2Name = new Dictionary<int, string>();

    public static BaseSkillNode CacheNode = null;
    public static List<BaseSkillNode> CacheNodeList = new List<BaseSkillNode>();

    public BluePrintNode PreSelectNode = null;

    public string DataPath;

    public Dictionary<string, string> resMap = new Dictionary<string, string>();

    public string GetPath(string name, string ext)
    {
        if (!string.IsNullOrEmpty(name) &&
            resMap.TryGetValue(name + ext, out var path))
        {
            return path;
        }
        return name;
    }
    public BaseSkillGraph()
    {
    }

    public virtual T GetConfigData<T>() where T : XConfigData { return default(T); }

    protected override void OnKeyBoardEvent(Event e)
    {
        base.OnKeyBoardEvent(e);

        if (e.control)
        {
            switch (e.keyCode)
            {
                case KeyCode.A:
                    {
                        selectNodeList.Clear();
                        foreach(BaseSkillNode node in widgetList)
                        {
                            selectNodeList.Add(node);
                            node.IsSelected = true;
                        }
                    }
                    break;
                case KeyCode.Z:
                    {
                        if (e.shift) RedoSnapshot();
                        else UndoSnapshot();
                    }
                    break;
                case KeyCode.C:
                    {
                        BaseSkillGraph.CacheNode = selectNode as BaseSkillNode;
                        BaseSkillGraph.CacheNodeList.Clear();
                        foreach (BaseSkillNode node in selectNodeList)
                        {
                            BaseSkillGraph.CacheNodeList.Add(node);
                        }
                    }
                    break;
                case KeyCode.V:
                    {
                        if (CacheNode != null || CacheNodeList.Count != 0)
                        {
                            e.mousePosition -= scrollPosition;
                            OnPasteClicked(e);
                        }
                    }
                    break;
            }
        }
        else
        {
            switch (e.keyCode)
            {
                case KeyCode.Delete:
                    {
                        if (selectNode != null) selectNode.OnDeleteClicked();
                        for (int i = selectNodeList.Count - 1; i >= 0; --i)
                        {
                            if (i < selectNodeList.Count) (selectNodeList[i] as BaseSkillNode).OnDeleteClicked();
                        }
                        selectNodeList.Clear();
                        selectNode = null;
                    }
                    break;
            }
        }
    }

    public virtual void Snapshot() { }
    public virtual void UndoSnapshot() { }
    public virtual void RedoSnapshot() { }

    protected virtual void OnPasteClicked(object o) { }


    public virtual float Length { get { return 0f; } }
    public virtual float PosXToTime(float posX){ return 0f; }
    public virtual float TimeToFrame(float time) { return 0f; }
    public virtual float TimeToPosX(float time) { return 0f; }
    public virtual float FrameToTime(float time) { return 0f; }
    public virtual void DrawLine(float posX, Color color) { }

    public float GetDelta()
    {
        return 10;
    }

    public float TimeFrameField(string label, float value, bool useInt = false, bool delay = false, [CallerFilePath] string path = "")
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUITool.LabelField(label, GUILayout.Width(100), path);
        GUILayout.FlexibleSpace();
        value = EditorGUILayout.FloatField(value, GUILayout.Width(50));
        EditorGUITool.LabelField("(s)", GUILayout.Width(20));
        if (useInt)
            if(!delay) value = FrameToTime(EditorGUILayout.IntField((int)TimeToFrame(value), GUILayout.Width(30)));
            else value = FrameToTime(EditorGUILayout.DelayedIntField((int)TimeToFrame(value), GUILayout.Width(30)));
        else
            if (!delay) value = FrameToTime(EditorGUILayout.FloatField(TimeToFrame(value), GUILayout.Width(30)));
            else value = FrameToTime(EditorGUILayout.DelayedFloatField(TimeToFrame(value), GUILayout.Width(30)));
        EditorGUITool.LabelField("(frame)", GUILayout.Width(45));
        EditorGUILayout.EndHorizontal();

        return Mathf.Min(60f, value);
    }

    public float GetCurveDuration(TextAsset Curve)
    {
        try
        {
            string[] curve = Curve.text.Replace("\r\n", "\t").Replace("\n", "\t").Split('\t');
            if (curve.Length > 3) return float.Parse(float.Parse(curve[curve.Length - 3]).ToString("f3"));
        }
        catch
        {
            return 0f;
        }
        return 0;
    }

    public BaseSkillNode GetNodeByIndex(int index)
    {
        for (int i = 0; i < widgetList.Count; ++i)
        {
            if ((widgetList[i] as BaseSkillNode).GetHosterData<XBaseData>().Index == index)
            {
                return (widgetList[i] as BaseSkillNode);
            }
        }

        return null;
    }

    protected void GetNodeByIndex<T>(ref BaseSkillNode node, ref int index, bool showgui = false, string label = "", [CallerFilePath] string path = "") where T : BaseSkillNode
    {
        if (!(node is T)) node = GetNodeByIndex(index);
        else node = widgetList.Contains(node) ? node : null;

        if (!(node is T)) index = -1;
        else index = node.GetHosterData<XBaseData>().Index;

        if (showgui)
        {
            string showlabel = label == "" ? (typeof(T).ToString().Replace("EditorNode.", "") + "Index") : label;
            index = EditorGUILayout.IntField(new GUIContent(showlabel, EditorGUITool.Translate(EditorGUITool.GetTypeString(path) + "_" + showlabel)), index);
        }

        if (node != null && index != node.GetHosterData<XBaseData>().Index)
            node = null;
    }

    public string[] GetFunctionTranslate(string start, string without = "@")
    {
        List<string> tmp = new List<string>();
        for (int i = 0; i < FunctionName.Count; ++i)
        {
            if (!FunctionName[i].StartsWith(start)) continue;
            if (FunctionFlag[i].Contains(without)) continue;
            string s = null;
            EditorGUITool.TranslateDic.TryGetValue(FunctionName[i], out s);
            tmp.Add(s == null ? FunctionName[i] : s);
        }

        return tmp.ToArray();
    }

    public string[] GetFunctionTips(string start,string without="@")
    {
        List<string> tmp = new List<string>();
        for (int i = 0; i < FunctionName.Count; ++i)
        {
            if (!FunctionName[i].StartsWith(start)) continue;
            if (FunctionFlag[i].Contains(without)) continue;
            if (!FunctionFlag[i].Contains("tips"))
                tmp.Add("");
            else
            {
                tmp.Add(FunctionFlag[i].Substring(FunctionFlag[i].IndexOf("tips") + 4));
            }
        }

        return tmp.ToArray();
    }

    public int GetFunctionIndex(string name, string start, string without = "@")
    {
        int index = 0;
        for (int i = 0; i < FunctionName.Count; ++i)
        {
            if (!FunctionName[i].StartsWith(start)) continue;
            if (FunctionName[i] == name) return index;
            if (!FunctionFlag[i].Contains(without)) ++index;
        }
        return 0;
    }

    public virtual N AddNodeInGraphByScript<T, N>(Vector2 pos, ref List<T> list, bool absPos = false) where T : XBaseData where N : BaseSkillNode, new()
    {
        return null;
    }

    public override void DrawMultiDataInspector()
    {
        base.DrawMultiDataInspector();

        float minPlayAt = float.MaxValue;
        for (int i = 0; i < selectNodeList.Count; ++i)
        {
            if ((selectNodeList[i] as BaseSkillNode).GetHosterData<XBaseData>().TimeBased)
            {
                minPlayAt = Mathf.Min(minPlayAt, (selectNodeList[i] as BaseSkillNode).GetHosterData<XBaseData>().At);
            }
        }
        if (minPlayAt != float.MaxValue)
        {
            float targetPlayAt = TimeFrameField("PlayAt", minPlayAt);
            for (int i = 0; i < selectNodeList.Count; ++i)
            {
                BaseSkillNode node = selectNodeList[i] as BaseSkillNode;
                if (node.GetHosterData<XBaseData>().TimeBased)
                {
                    node.GetHosterData<XBaseData>().At += targetPlayAt - minPlayAt;
                    node.Bounds = node.Bounds.SetPos(TimeToPosX(node.GetHosterData<XBaseData>().At) + node.OffsetX, node.Bounds.y);
                }
            }
        }
    }
}

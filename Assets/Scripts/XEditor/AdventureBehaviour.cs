#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CFUtilPoolLib;
using System.Reflection;
using System.Linq;
using System.Collections;

public class StringUtility
{
    public static string ToString(IEnumerable array)
    {
        if (array == null)
            return string.Empty;

        int i = 0;
        string s = string.Empty;
        foreach (var v in array)
        {
            if (i == 0)
                s = v.ToString();
            else
                s += string.Format("|{0}", v.ToString());

            ++i;
        }

        return s;
    }

    public static string ToStringIgnoreDefault<T>(T v) where T : IComparable<T>
    {
        return v.CompareTo(default(T)) != 0 ? v.ToString() : string.Empty;
    }

    public static MethodInfo ToStringIgnoreDefaultInfo 
        = typeof(StringUtility).GetMethod(nameof(StringUtility.ToStringIgnoreDefault));
}

public struct FakeSeqList<T> : ISeqListRef<T> where T : IComparable<T>
{
    public T this[int index, int key]
    {
        get
        {
            return m_Values[index * m_Dim + key];
        }
    }

    public int Count => m_Values.Count / Dim;

    public int Dim => m_Dim;

    public T GetValue(int index, int key)
    {
        return this[index, key];
    }

    List<T> m_Values;
    public List<T> Values => m_Values;

    int m_Dim;

    public void Init(List<T> list, int dim)
    {
        if (list.Count % dim != 0)
        {
            Debug.LogError("invalid param: " + list.ToString());
            return;
        }
        m_Values = list;
        m_Dim = dim;
    }

    public override string ToString()
    {
        return CVSReader.ToString(this);
    }
}

[ExecuteInEditMode]
[SelectionBase]
public class AdventureNodeBehaviour : MonoBehaviour
{
    public delegate void GameObjectDestroyed(AdventureNodeBehaviour bhv);

    public GameObjectDestroyed destroyCallback = null;

    short _id;
    public short id { get { return _id; }set { _id = value; gameObject.name = "Node" + _id; } }
    public string events;
    public uint type;
    public int roleType;
    public int landType;
    public uint end;
    public string bg;
    public string transitionOffset;
    public string check;
    ////public AdventureNodeBehaviour portal;

    ////private short _tempPortalID;

    static Type behaviorType = typeof(AdventureNodes.RowData);

    List<Tuple<string, string, Type>> rowDatas = new List<Tuple<string, string, Type>>();
    void _TrySetRowData(string key, string value)
    {
        for (int i = 0; i < rowDatas.Count; ++i)
        {
            var t = rowDatas[i];
            if (t.Item1 == key && t.Item2 != value)
            {
                rowDatas[i] = new Tuple<string, string, Type>(key, value, t.Item3);
                break;
            }
        }
    }

    public AdventureNodeBehaviour()
    {
        foreach (var field in behaviorType.GetFields())
        {
            rowDatas.Add(new Tuple<string, string, Type>(field.Name, string.Empty, field.FieldType));
        }
    }
    void OnDestroy()
    {
        if (destroyCallback != null)
        {
            destroyCallback(this);
        }
    }

    static string _ToString(FieldInfo field, AdventureNodes.RowData rowData)
    {
        if (field.FieldType.IsArray)
            return StringUtility.ToString(field.GetValue(rowData) as IEnumerable);
        else
        {
            var interfaces = field.FieldType.GetInterfaces();
            if (interfaces.Any(
                i =>
                i.IsGenericType
                && (i.GetGenericTypeDefinition() == typeof(ISeqRef<>)
                || i.GetGenericTypeDefinition() == typeof(ISeqListRef<>))))
            {
                return field.GetValue(rowData).ToString();
            }
            else
            {
                var exe = StringUtility.ToStringIgnoreDefaultInfo.MakeGenericMethod(field.FieldType);
                return exe.Invoke(null, new object[] { field.GetValue(rowData) }) as string;
            }
        }
    }
    public void Load(AdventureNodes.RowData rowData)
    {
        //rowDatas.Clear();
        //foreach (var field in behaviorType.GetFields())
        //{
        //    rowDatas.Add(new Tuple<string, string, Type>(field.Name, _ToString(field, rowData), field.FieldType));
        //}

        id = rowData.NodeID;
        events = rowData.Events.ToString();
        type = rowData.Type;
        roleType = rowData.RoleType;
        landType = rowData.LandType;
        end = rowData.End;
        bg = StringUtility.ToString(rowData.ChatBg);
        transitionOffset = rowData.TransitionOffset.ToString();
        check = rowData.Check.ToString();
        ////_tempPortalID = rowData.Portal;
    }

    public void PostLoad(GameObject nodeRoot)
    {
        ///////> 配表方便起见，先定义0为非法值。这样起点就不能是传送点
        ////if (_tempPortalID != 0)
        ////{
        ////    if (_tempPortalID == _id)
        ////    {
        ////        XDebug.singleton.AddErrorLog("Portal ID == my ID");
        ////        return;
        ////    }
        ////    for (int i = 0; i < nodeRoot.transform.childCount; ++i)
        ////    {
        ////        GameObject go = nodeRoot.transform.GetChild(i).gameObject;
        ////        var bhv = go.GetComponent<AdventureNodeBehaviour>();
        ////        if (bhv.id == _tempPortalID)
        ////        {
        ////            portal = bhv;
        ////            break;
        ////        }
        ////    }
        ////}
    }

    public static void Save(AdventureNodes.RowData rowData, string[] content)
    {
        int idx = 0;
        foreach (var field in behaviorType.GetFields())
        {
            content[idx++] = _ToString(field, rowData);
        }
    }

    ///> NodeTable
    public void Save(string[] content, uint configID, short nodeid, HashSet<short> nextNodes, HashSet<short> watchNodes)
    {
        _TrySetRowData("ConfigID", StringUtility.ToStringIgnoreDefault(configID));
        _TrySetRowData("NodeID", StringUtility.ToStringIgnoreDefault(nodeid));
        _TrySetRowData("NextNodes", StringUtility.ToString(nextNodes));
        _TrySetRowData("Events", events);
        //content[idx++] = mapID.ToString();
        //content[idx++] = index.ToString();
        //content[idx++] = StringUtility.ToString(nextNodes);
        //content[idx++] = events;

        Vector3 pos = transform.position;
        _TrySetRowData("Pos", string.Format("{0:0.###}={1:0.###}={2:0.###}", pos.x, pos.y, pos.z));
        //content[idx++] = string.Format("{0:0.##}={1:0.##}={2:0.##}", pos.x, pos.y, pos.z);

        SeqRef<float> r = new SeqRef<float>(3);
        Vector3 rot = transform.rotation.eulerAngles;
        r.bufferRef = new float[3] {rot.x, rot.y, rot.z};
        _TrySetRowData("Rot", r.ToString());
        //content[idx++] = rot.Equals(Vector3.zero) ? string.Empty : string.Format("{0:0.##}={1:0.##}={2:0.##}", rot.x, rot.y, rot.z);

        _TrySetRowData("WatchNodes", StringUtility.ToString(watchNodes));
        _TrySetRowData("Type", StringUtility.ToStringIgnoreDefault(type));
        _TrySetRowData("RoleType", StringUtility.ToStringIgnoreDefault(roleType));
        _TrySetRowData("LandType", StringUtility.ToStringIgnoreDefault(landType));
        _TrySetRowData("End", StringUtility.ToStringIgnoreDefault(end));
        _TrySetRowData("ChatBg", bg);
        _TrySetRowData("TransitionOffset", transitionOffset);
        _TrySetRowData("Check", StringUtility.ToStringIgnoreDefault(check));
        ////_TrySetRowData("Portal", portal == null ? string.Empty : portal.id.ToString());

        //content[idx++] = StringUtility.ToString(watchNodes);
        //content[idx++] = StringUtility.ToString(type);
        //content[idx++] = StringUtility.ToString(roleType);
        //content[idx++] = StringUtility.ToString(landType);

        //content[idx++] = StringUtility.ToString(end);
        //content[idx++] = bg;
        //content[idx++] = portal == null ? string.Empty : portal.id.ToString();

        for (int i = 0; i < rowDatas.Count; ++i)
        {
            content[i] = rowDatas[i].Item2;
        }

    }

    public static readonly int NODES_CONTENT_LENGTH = 14;
}

[ExecuteInEditMode]
public class AdventureLineBehaviour : MonoBehaviour
{
    public delegate void GameObjectDestroyed(AdventureLineBehaviour bhv);

    public GameObjectDestroyed destroyCallback = null;

    AdventureNodeBehaviour _startNode;
    AdventureNodeBehaviour _endNode;
    public AdventureNodeBehaviour startNode { get { return _startNode; } }
    public AdventureNodeBehaviour endNode { get { return _endNode; } }

    public float Length { get { return Vector3.Distance(_endNode.transform.position, _startNode.transform.position); } }
    public bool hide = false;

    public void Set(AdventureNodeBehaviour start, AdventureNodeBehaviour end)
    {
        _startNode = start;
        _endNode = end;

        RefreshName();
        RefreshPosition();
    }

    public void RefreshName()
    {
        gameObject.name = string.Format("Line_{0}_{1}", _startNode.id, _endNode.id);
    }

    public void RefreshPosition()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, _startNode.transform.position);
        lineRenderer.SetPosition(1, _endNode.transform.position);
        transform.position = (_startNode.transform.position + _endNode.transform.position) * 0.5f;
        Vector3 dis = (_endNode.transform.position - _startNode.transform.position);
        transform.rotation = Quaternion.LookRotation(dis, Vector3.up);
    }

    void OnDestroy()
    {
        if (destroyCallback != null)
        {
            destroyCallback(this);
        }
    }
    public void Load(AdventureLines.RowData rowData)
    {
        hide = rowData.Hide;
        for (int i = 0; i < rowData.Corners.Count; ++i)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.parent = this.transform;
            go.transform.position = new Vector3(rowData.Corners[i, 1], rowData.Corners[i, 2], rowData.Corners[i, 3]);
            var s = rowData.Corners[i, 0];
            go.transform.localScale = new Vector3(s, s, s);
        }
    }

    public static void Save(AdventureLines.RowData rowData, string[] content)
    {
        int idx = 0;
        content[idx++] = rowData.ConfigID.ToString();
        content[idx++] = rowData.NodeID.ToString();
        content[idx++] = rowData.Hide ? "TRUE" : "FALSE";
        content[idx++] = rowData.Corners.ToString();
    }

    ///> NodeTable
    public void Save(string[] content, uint configID)
    {
        int idx = 0;
        content[idx++] = configID.ToString();
        content[idx++] = string.Format("{0}={1}", startNode.id, endNode.id);
        content[idx++] = hide ? "TRUE" : "FALSE";

        if (transform.childCount > 0)
        {
            FakeSeqList<float> c = new FakeSeqList<float>();
            c.Init(new List<float>(), 4);
            for (int i = 0, childCount = transform.childCount; i < childCount; i++)
            {
                var t = transform.GetChild(i);
                c.Values.Add(t.localScale.x);
                c.Values.Add(t.transform.position.x);
                c.Values.Add(t.transform.position.y);
                c.Values.Add(t.transform.position.z);
            }
            content[idx++] = c.ToString();
        }
        else
        {
            content[idx++] = String.Empty;
        }
    }

    public static readonly int LINES_CONTENT_LENGTH = 4;
}

#endif
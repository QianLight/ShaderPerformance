/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zeus.Build;
using Zeus.Framework.Asset;

public enum TagNodeType
{
    Root,
    Normal,
    Parent
}

[Serializable]
public class TagAsset
{
    private const string TagAssetName = "SubpackageTag.json";

    [SerializeField]
    public List<TagNodeArg> args;
    
    [SerializeField]
    public bool manuallyOtherTag;

    public TagAsset()
    {
        args = new List<TagNodeArg>();
        manuallyOtherTag = false;
    }
    
    /// <summary>
    /// ParentTag + ChildTag
    /// </summary>
    public List<string> TagListWithoutRoot
    {
        get
        {
            var tagListWithoutRoot = new List<string>();
            foreach (TagNodeArg arg in args)
            {
                tagListWithoutRoot.Add(arg.title);
            }
            tagListWithoutRoot.RemoveAt(0);
            return tagListWithoutRoot;
        }
    }
    /// <summary>
    /// 首包Tag + ParentTag
    /// </summary>
    public List<string> TagListWithoutChild
    {
        get
        {
            var tagListWithoutChild = new List<string>();
            foreach (TagNodeArg arg in args)
            {
                if (arg.type != TagNodeType.Normal)
                {
                    tagListWithoutChild.Add(arg.title);
                }
            }
            return tagListWithoutChild;
        }
    }
    
    /// <summary>
    /// 只有ParentTag
    /// </summary>
    public List<string> TagListOnlyParent
    {
        get
        {
            var tagListOnlyParent = new List<string>();
            foreach (TagNodeArg arg in args)
            {
                if (arg.type == TagNodeType.Parent)
                {
                    tagListOnlyParent.Add(arg.title);
                }
            }
            return tagListOnlyParent;
        }
    }

    /// <summary>
    /// 只有ChildTag
    /// </summary>
    public List<string> TagListOnlyChild
    {
        get
        {
            var tagListOnlyChild = new List<string>();
            foreach (TagNodeArg arg in args)
            {
                if (arg.type == TagNodeType.Normal)
                {
                    tagListOnlyChild.Add(arg.title);
                }
            }
            return tagListOnlyChild;
        }
    }

    /// <summary>
    /// 首包Tag + ChildTag (录制用)
    /// </summary>
    public List<string> TagListForRecord
    {
        get
        {
            var tagListToRecord = new List<string>();
            foreach (TagNodeArg arg in args)
            {
                if (arg.type != TagNodeType.Parent)
                {
                    tagListToRecord.Add(arg.title);
                }
            }
            return tagListToRecord;
        }
    }
    
    public List<string> tagsAddToFirstPackage
    {
        get
        {
            var tags = new List<string>();
            foreach (TagNodeArg arg in args)
            {
                if (arg.isFirstPackageTag)
                {
                    tags.Add(arg.title);
                }
            }
            return tags;
        }
        set
        {
            foreach (var arg in args)
            {
                arg.isFirstPackageTag = value.Contains(arg.title);
            }
        }
    }

    public static TagAsset LoadTagAsset()
    {
        string path = "Assets/" + Zeus.Core.FileSystem.VFileSystem.GetEditorSettingPath(TagAssetName);
        if (!File.Exists(path))
        {
            var tagAsset = new TagAsset();
            File.WriteAllText(path,JsonUtility.ToJson(tagAsset));
            return new TagAsset();
        }

        var result = JsonUtility.FromJson<TagAsset>(File.ReadAllText(path));
        
        if (CommandLineArgs.Inited)
        {
            string defineSymbolStr = null;
            if (CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.MARK_TAG_TO_FIRST_PACKAGE, ref defineSymbolStr))
            {
                if (defineSymbolStr == "null")
                {
                    result.tagsAddToFirstPackage = new List<string>();
                }
                else
                {
                    var tagList = defineSymbolStr.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
                    result.tagsAddToFirstPackage = tagList.ToList();
                }
            }
        }

        return result;
    }

    public static void SaveTagAsset(TagAsset asset)
    {
        string path = "Assets/" + Zeus.Core.FileSystem.VFileSystem.GetEditorSettingPath(TagAssetName);
        File.WriteAllText(path, JsonUtility.ToJson(asset, true));
        AssetDatabase.ImportAsset(path);
    }
    
    public TagNodeArg GetArgByIndex(int index)//index不是数组索引 是构建时候的顺序索引
    {
        foreach (var arg in args)
        {
            if (arg.index != index) 
                continue;
            return arg;
        }
        return null;
    }

    public TagNodeArg GetArgByName(string tagName)
    {
        foreach (var arg in args)
        {
            if (arg.title == tagName)
            {
                return arg;
            }
        }
        return null;
    }

    public string[] GetChildNames(string parentName)
    {
        var parentArg = GetArgByName(parentName);
        if (parentArg == null || parentArg.type != TagNodeType.Parent)
            return null;
        
        var result = new string[parentArg.childNodesIndex.Count];
        for (int i = 0; i < parentArg.childNodesIndex.Count; i++)
        {
            result[i] = GetArgByIndex(parentArg.childNodesIndex[i]).title;
        }
        return result;
    }

    public void ChangeFirstPackageTagMark(string tagName)
    {
        foreach (var arg in args)
        {
            if (arg.title == tagName)
            {
                arg.isFirstPackageTag = !arg.isFirstPackageTag;
                break;
            }
        }
    }

    public List<string> GetSignableTags()
    {
        if (args?[0] == null)
        {
            LoadTagAsset();
        }

        var result = new List<string>();
        var parent = args[0];
        bool isSignable = true;
        while (parent.nextNode != null && isSignable)
        {
            isSignable = true;
            parent = parent.nextNode.arg;
            foreach (var child in parent.childNodes)
            {
                result.Add(child.arg.title);
                if (!child.arg.isFirstPackageTag)
                {
                    isSignable = false;
                }
            }
        }

        return result;
    }
}

[Serializable]
public class TagNodeArg
{
    public Rect rect;
    public string title;
    public bool isFirstPackageTag;
    public TagNodeType type;
    public int index;
    // public List<int> childNodesIndex = new List<int>();
    // public int parentNodeIndex = -1;
    // public int nextNodeIndex = -1;
    
    public List<TagNode> childNodes= new List<TagNode>();
    public TagNode parentNode = null;
    public TagNode nextNode = null;
    
    public List<int> childNodesIndex= new List<int>();
    public int parentNodeIndex = -1;
    public int nextNodeIndex = -1;

    public HashSet<TagNode> childSet = new HashSet<TagNode>();

    public TagNodeArg(int index, TagNodeType type, Vector2 position, float width, float height, string title)
    {
        this.index = index;
        this.type = type;
        this.title = title;
        rect = new Rect(position.x, position.y, width, height);
    }

    public void Clear()
    {
        nextNodeIndex = -1;
        parentNodeIndex = -1;
        childNodesIndex.Clear();

        nextNode = null;
        parentNode = null;
        childNodes?.Clear();
        childSet?.Clear();
        
        if (childSet == null)
        {
            childSet = new HashSet<TagNode>();
        }
        else
        {
            childSet.Clear();
        }
    }

    public void AddChild(TagNode node)
    {
        if (childNodes == null)
        {
            childNodes = new List<TagNode>();
            childSet = new HashSet<TagNode>();
        }
        if (childSet.Add(node))
        {
            // childNodesIndex.Add(node.arg.index);
            childNodes.Add(node);
        }
    }

    public void SetParent(TagNode node)
    {
        // parentNodeIndex = node.arg.index;
        parentNode = node;
        // Debug.Log(index+"->"+node.arg.index);
    }
    
    public void SetNext(TagNode node)
    {
        // nextNodeIndex = node.arg.index;
        nextNode = node;
    }
}

public class TagNode
{
    public TagNodeArg arg;

    public bool isDragged;
    public bool isSelected;

    private ConnectionPoint inPoint;
    public ConnectionPoint InPoint
    {
        get
        {
            return inPoint;
        }
    }

    private ConnectionPoint outPoint;
    public ConnectionPoint
        OutPoint
    {
        get
        {
            return outPoint;
        }
    }

    private GUIStyle style;
    private GUIStyle defaultNodeStyle;
    private GUIStyle selectedNodeStyle;

    private Action<TagNode> OnRemoveNode;
    private Action<TagNode> OnSelectNode;

    public TagNode(TagNodeArg arg, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle,
        GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<TagNode> OnClickRemoveNode, Action<TagNode> OnClickSelectNode)
    {
        this.arg = arg;
        style = nodeStyle;
        if (arg.type != TagNodeType.Root)
        {
            inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        }
        if (arg.type != TagNodeType.Normal)
        {
            outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        }
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        OnRemoveNode = OnClickRemoveNode;
        OnSelectNode = OnClickSelectNode;
    }

    public void Drag(Vector2 delta, bool isAll)
    {
        if (isAll)
        {
            arg.rect.position += delta;
            return;
        }
        var window = EditorWindow.focusedWindow as TagNodeEditor;
        List<TagNode> list = new List<TagNode>();
        if (arg.type != TagNodeType.Normal)
        {
            if (arg.childNodes != null)
            {
                list.AddRange(arg.childNodes);
            }
            if (!list.Contains(this))
            {
                list.Add(this);
            }
        }
        else if(arg.type == TagNodeType.Normal)
        {
            var parent = arg.parentNode;
            list.AddRange(parent.arg.childNodes);
            if (!list.Contains(parent))
            {
                list.Add(parent);
            }
        }
        foreach (var node in list)
        {
            node.arg.rect.position += delta;
        }
    }

    public void Draw()
    {
        Rect rect = arg.rect;
        if (arg.type == TagNodeType.Parent)
        {
            inPoint.Draw();
        }
        if (arg.type != TagNodeType.Normal)
        {
            outPoint.Draw();
        }
        else
        {
            rect = new Rect(arg.rect.position, new Vector2(arg.rect.width*0.8f,arg.rect.height));
        }
        GUI.Box(rect, arg.title, style);
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (Smaller(arg.rect).Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;


                        var str = "this" + arg.index + " parent" + arg.parentNodeIndex+" next"+ arg.nextNodeIndex + " child";
                        foreach (var num in arg.childNodesIndex)
                        {
                            str += num;
                        }
                        // Debug.Log(str);
                        str = "";
                        foreach (var conn in (EditorWindow.focusedWindow as TagNodeEditor).connections)
                        {
                            str += conn.outPoint.node.arg.index + "->" + conn.inPoint.node.arg.index+conn.isShow+" | ";
                        }
                        // Debug.Log(str);
                        
                        style = selectedNodeStyle;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                }

                if (e.button == 1 && isSelected && Smaller(arg.rect).Contains(e.mousePosition))
                {
                    ProcessContextMenu(arg.type);
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                if (isSelected)
                {
                    if (OnSelectNode != null)
                    {
                        OnSelectNode(this);
                    }
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    Drag(e.delta, false);
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }

    private Rect Smaller(Rect oriRect)
    {
        return new Rect(oriRect.x, oriRect.y + 5, oriRect.width, oriRect.height - 10);
    }

    private void ProcessContextMenu(TagNodeType type)
    {
        if (type == TagNodeType.Parent)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
            genericMenu.AddItem(new GUIContent("Add child"), false, OnClickAddChildNode);
            genericMenu.ShowAsContext();
        }
        else
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
            genericMenu.ShowAsContext();
        }
    }

    private void OnClickRemoveNode()
    {
        if (OnRemoveNode != null)
        {
            OnRemoveNode(this);
        }
    }

    private void OnClickAddChildNode()
    {
        var window = EditorWindow.focusedWindow as TagNodeEditor;
        window.OnClickAddChildNode(this);
    }

    public void ChangeStyle(GUIStyle defaultStyle, GUIStyle selectedStyle)
    {
        this.defaultNodeStyle = defaultStyle;
        this.selectedNodeStyle = selectedStyle;
    }
}

internal class NodeSelector : ScriptableObject
{
    public TagNode selectedNode;
}

[CustomEditor(typeof(NodeSelector))]
internal class NodeInspector : Editor
{
    public static bool isDirty;
    private static string lastArgTitle;
    private static TagNode lastNode;
    NodeSelector selector;

    public void OnEnable()
    {
        selector = target as NodeSelector;
        isDirty = false;
    }

    public override bool RequiresConstantRepaint()
    {
        return isDirty;
    }
    
    public override void OnInspectorGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("TagName: ");
        var newTitle = GUILayout.TextField(selector.selectedNode.arg.title);
        if (lastArgTitle != newTitle && lastNode == selector.selectedNode)
        {
            if (lastArgTitle == AssetBundleLoaderSetting.OthersTag)
            {
                selector.selectedNode.arg.title = newTitle;
                TagNodeEditor.editorWindow._tagAsset.manuallyOtherTag = false;
            }
            else if (newTitle == AssetBundleLoaderSetting.OthersTag)
            {
                if (TagNodeEditor.editorWindow._tagAsset.manuallyOtherTag)
                {
                    EditorUtility.DisplayDialog("", "当前列表中已包含" + AssetBundleLoaderSetting.OthersTag + "请勿重复添加!", "OK");
                }
                else
                {
                    if (selector.selectedNode.arg.type != TagNodeType.Normal)
                    {
                        EditorUtility.DisplayDialog("", AssetBundleLoaderSetting.OthersTag + " 为保留关键字, 仅可用于子节点!", "OK");
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("", "使用此命名时将调整 " + AssetBundleLoaderSetting.OthersTag + " 的优先级, 是否确认?", "OK", "Cancel"))
                        {
                            selector.selectedNode.arg.title = newTitle;
                            TagNodeEditor.editorWindow._tagAsset.manuallyOtherTag = true;
                        }
                    }
                }
            }
            else
            {
                selector.selectedNode.arg.title = newTitle;
            }
        }

        GUILayout.EndHorizontal();

        if (selector.selectedNode.arg.type == TagNodeType.Normal)
        {
            GUI.enabled = TagNodeEditor.editorWindow._tagAsset.GetSignableTags().Contains(selector.selectedNode.arg.title);
            var flag = GUILayout.Toggle(selector.selectedNode.arg.isFirstPackageTag, "Mark to FirstPackage: ");
            if (flag != selector.selectedNode.arg.isFirstPackageTag)
            {
                selector.selectedNode.arg.isFirstPackageTag = flag;
                TagNodeEditor.editorWindow.isDirty = true;
            }
            GUI.enabled = true;
        }

        isDirty = false;
        lastArgTitle = newTitle;
        lastNode = selector.selectedNode;
    }
}

public enum ConnectionPointType { In, Out }

public class ConnectionPoint
{
    public Rect rect;

    public ConnectionPointType type;

    public TagNode node;

    public GUIStyle style;

    public Action<ConnectionPoint> OnClickConnectionPoint;

    public ConnectionPoint(TagNode node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
    {
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }

    public void Draw()
    {
        rect.y = node.arg.rect.y + (node.arg.rect.height * 0.5f) - rect.height * 0.5f;

        switch (type)
        {
            case ConnectionPointType.In:
                rect.x = node.arg.rect.x - rect.width + 8f;
                break;

            case ConnectionPointType.Out:
                rect.x = node.arg.rect.x + node.arg.rect.width - 8f;
                break;
        }

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }
}

internal class Connection
{
    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;
    public bool isShow;
    public Action<Connection> OnClickRemoveConnection;

    public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, bool isShow, Action<Connection> OnClickRemoveConnection)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.isShow = isShow;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
    }

    public void Draw()
    {
        Handles.DrawBezier(
            inPoint.rect.center,
            outPoint.rect.center,
            inPoint.rect.center + Vector2.left * 50f,
            outPoint.rect.center - Vector2.left * 50f,
            Color.white,
            null,
            2f
        );

        if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
        {
            if (OnClickRemoveConnection != null)
            {
                OnClickRemoveConnection(this);
            }
        }
    }
}

internal class TagNodeEditor : EditorWindow
{
    public static TagNodeEditor editorWindow;
    public TagAsset _tagAsset;

    private TagNode _rootNode;
    public List<TagNode> nodes;
    public List<Connection> connections;

    //private Dictionary<TagNode, int> _node2InDegree;

    private GUIStyle nodeStyle;
    private GUIStyle parentNodeStyle;
    private GUIStyle firstPackageNodeStyle;
    
    private GUIStyle selectedNodeStyle;
    private GUIStyle parentSelectedNodeStyle;
    private GUIStyle firstPackageSelectedNodeStyle;
    
    private GUIStyle inPointStyle;
    private GUIStyle outPointStyle;

    private ConnectionPoint selectedInPoint;
    private ConnectionPoint selectedOutPoint;

    private Vector2 offset;
    private Vector2 drag;

    private NodeSelector selector;

    public bool isDirty = false;

    [MenuItem("Zeus/Asset/Tag Node Editor", false, 5)]
    private static void OpenWindow()
    {
        editorWindow = GetWindow<TagNodeEditor>();
        editorWindow.titleContent = new GUIContent("Tag Node Editor");
    }

    private void OnEnable()
    {
        GUIStyle fontStyle = new GUIStyle();
        fontStyle.alignment = TextAnchor.MiddleCenter;
        fontStyle.fontSize = 30;

        selector = new NodeSelector();

        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);
        nodeStyle.alignment = TextAnchor.MiddleCenter;
        nodeStyle.fontSize = 20;
        nodeStyle.fontStyle = FontStyle.Bold;

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
        selectedNodeStyle.alignment = TextAnchor.MiddleCenter;
        selectedNodeStyle.fontSize = 20;
        selectedNodeStyle.fontStyle = FontStyle.Bold;

        parentNodeStyle = new GUIStyle();
        parentNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
        parentNodeStyle.border = new RectOffset(12, 12, 12, 12);
        parentNodeStyle.alignment = TextAnchor.MiddleCenter;
        parentNodeStyle.fontSize = 20;
        parentNodeStyle.fontStyle = FontStyle.Bold;

        parentSelectedNodeStyle = new GUIStyle();
        parentSelectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5 on.png") as Texture2D;
        parentSelectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
        parentSelectedNodeStyle.alignment = TextAnchor.MiddleCenter;
        parentSelectedNodeStyle.fontSize = 20;
        parentSelectedNodeStyle.fontStyle = FontStyle.Bold;
        
        firstPackageNodeStyle = new GUIStyle();
        firstPackageNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node4.png") as Texture2D;
        firstPackageNodeStyle.border = new RectOffset(12, 12, 12, 12);
        firstPackageNodeStyle.alignment = TextAnchor.MiddleCenter;
        firstPackageNodeStyle.fontSize = 20;
        firstPackageNodeStyle.fontStyle = FontStyle.Bold;

        firstPackageSelectedNodeStyle = new GUIStyle();
        firstPackageSelectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node4 on.png") as Texture2D;
        firstPackageSelectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
        firstPackageSelectedNodeStyle.alignment = TextAnchor.MiddleCenter;
        firstPackageSelectedNodeStyle.fontSize = 20;
        firstPackageSelectedNodeStyle.fontStyle = FontStyle.Bold;

        inPointStyle = new GUIStyle();
        inPointStyle.normal.background = (EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D);
        inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        inPointStyle.border = new RectOffset(4, 4, 12, 12);

        outPointStyle = new GUIStyle();
        outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        outPointStyle.border = new RectOffset(4, 4, 12, 12);

        _tagAsset = TagAsset.LoadTagAsset();
        Init(out nodes, out connections);
        Refresh();
    }

    private void Init(out List<TagNode> tags, out List<Connection> connections)
    {
        //_node2InDegree = new Dictionary<TagNode, int>();
        if (_tagAsset.args.Count == 0)
        {
            tags = null;
            connections = null;
            return;
        }

        TagNode[] tagArray = new TagNode[_tagAsset.args.Count];
        List<Connection> connList = new List<Connection>();
        foreach (TagNodeArg arg in _tagAsset.args)
        {
            TagNode node = CreateTagNodeByArg(arg);
            tagArray[arg.index] = node;
            //_node2InDegree.Add(node, 0);
        }
        var tagList = tagArray.ToList();
        foreach (TagNodeArg arg in _tagAsset.args)
        {
            if (arg.childNodesIndex != null)
            {
                foreach (var i in arg.childNodesIndex)
                {
                    if (i != -1)
                    {
                        connList.Add(new Connection(tagList[i].InPoint, tagList[arg.index].OutPoint, false, OnClickRemoveConnection));
                    }
                    // _node2InDegree[node.InPoint.node]++;
                }
            }
            if (arg.nextNodeIndex != -1)
            {
                connList.Add(new Connection(tagList[arg.nextNodeIndex].InPoint, tagList[arg.index].OutPoint, true, OnClickRemoveConnection));
            }
        }
        if (tagList.Count > 0)
        {
            tags = tagList;
            _rootNode = tagList[0];
        }
        else
        {
            tags = null;
        }
        connections = connList;

        
        // foreach (TagNodeArg arg in _tagAsset.args)
        // {
        //     if (arg.childNodesIndex != null)
        //     {
        //         foreach (var i in arg.childNodesIndex)
        //         {
        //             try
        //             {
        //                 arg.childNodes.Add(tags[i]);
        //                 arg.childSet.Add(tags[i]);
        //             }
        //             catch (Exception)
        //             {
        //             }
        //         }
        //     }
        //
        //     try
        //     {
        //         arg.parentNode = tags[arg.parentNodeIndex];
        //         arg.nextNode = tags[arg.nextNodeIndex];
        //     }
        //     catch (ArgumentOutOfRangeException)
        //     {
        //     }
        //
        // }
    }

    private TagNode CreateTagNodeByArg(TagNodeArg arg)
    {
        TagNode node;
        if (arg.type == TagNodeType.Parent)
        {
            node = new TagNode(arg, parentNodeStyle, parentSelectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickSelectNode);
        }
        else if (arg.title == Zeus.Framework.Asset.AssetBundleLoaderSetting.FirstPackageTag || _tagAsset.tagsAddToFirstPackage.Contains(arg.title))
        {
            node = new TagNode(arg, firstPackageNodeStyle, firstPackageSelectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickSelectNode);
        }
        else
        {
            node = new TagNode(arg, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickSelectNode);
        }
        return node;
    }

    private void OnGUI()
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);
        if (GUILayout.Button("Save"))
        {
            SaveTags();
            Init(out nodes, out connections);
        }

        RefreshStyle();
        DrawNodes();
        DrawConnections();
        DrawConnectionLine(Event.current);

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
    }

    private void Refresh()
    {
        _tagAsset.args.Clear();
        List<TagNode> nodeList = new List<TagNode>();
        if (_rootNode != null)
        {
            foreach (TagNode node in nodes)
            {
                node.arg.Clear();
                nodeList.Add(node);
                node.arg.index = nodeList.Count - 1;
            }

            if (null != connections)
            {
                foreach (Connection conn in connections)
                {
                    if (conn.isShow)
                    {
                        //conn.inPoint.node.arg.SetParent(conn.outPoint.node);
                        conn.outPoint.node.arg.SetNext(conn.inPoint.node);
                    }
                    else
                    {
                        conn.inPoint.node.arg.SetParent(conn.outPoint.node);
                        conn.outPoint.node.arg.AddChild(conn.inPoint.node);
                    }
                }
            }
            

            //排序填充_tagAsset
             TagNodeArg tagPointer = _rootNode.arg;
             _tagAsset.args.Add(tagPointer);
             while (null != tagPointer.nextNode)
             {
                 tagPointer = tagPointer.nextNode.arg;
                 _tagAsset.args.Add(tagPointer);
                 if (tagPointer.childNodes != null)
                 {
                     foreach (var childNode in tagPointer.childNodes)
                     {
                         _tagAsset.args.Add(childNode.arg);
                     }
                 }
             }
            
            //更新Index
             foreach (TagNodeArg arg in _tagAsset.args)
             {
                 if (arg.childNodes != null)
                 {
                     foreach (var node in arg.childNodes)
                     {
                         arg.childNodesIndex.Add(node.arg.index);
                     }
                 }
                 if (arg.parentNode != null)
                 {
                     arg.parentNodeIndex = arg.parentNode.arg.index;
                 }
                 if (arg.nextNode != null)
                 {
                     arg.nextNodeIndex = arg.nextNode.arg.index;
                 }
             }
        }
    }
    private void SaveTags()
    {
        Refresh();
        if (_rootNode != null)
        {
            TagAsset.SaveTagAsset(_tagAsset);
        }
        else
        {
            Debug.LogError("There is no root node.");
        }
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes()
    {
        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }
        }
    }

    private void RefreshStyle()
    {
        if (!isDirty)
            return;
        foreach (var node in nodes)
        {
            if (node.arg.type == TagNodeType.Parent)
            {
                node.ChangeStyle(parentNodeStyle, parentSelectedNodeStyle);
            }
            else if (node.arg.title == Zeus.Framework.Asset.AssetBundleLoaderSetting.FirstPackageTag || _tagAsset.tagsAddToFirstPackage.Contains(node.arg.title))
            {
                node.ChangeStyle(firstPackageNodeStyle, firstPackageSelectedNodeStyle);
            }
            else
            {
                node.ChangeStyle(nodeStyle, selectedNodeStyle);
            }
        }

        isDirty = false;
    }

    private void DrawConnections()
    {
        if (connections == null) return;
        try
        {
            foreach (var conn in connections)
            {
                if (conn.isShow)
                {
                    conn.Draw();
                }
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    ClearConnectionSelection();
                }

                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                }
                break;
        }
    }

    private void ProcessNodeEvents(Event e)
    {
        if (nodes != null)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }

    private void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.rect.center,
                e.mousePosition,
                selectedInPoint.rect.center + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center,
                e.mousePosition,
                selectedOutPoint.rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        if (_rootNode == null)
        {
            genericMenu.AddItem(new GUIContent("Add root node"), false, () => OnClickAddNode(mousePosition, TagNodeType.Root));
        }
        genericMenu.AddItem(new GUIContent("Add parent node"), false, () => OnClickAddNode(mousePosition, TagNodeType.Parent));
        genericMenu.ShowAsContext();
    }

    private void AddChildNode(TagNode parentNode)
    {
        TagNodeArg arg = new TagNodeArg(_tagAsset.args.Count, TagNodeType.Normal, parentNode.arg.rect.position - Vector2.down * (40 * (parentNode.arg.childNodesIndex.Count + 1)), 200, 50, GenerateTagTitle("Tag"));
        _tagAsset.args.Add(arg);
        TagNode node = new TagNode(arg, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickSelectNode);
        nodes.Add(node);
        //_node2InDegree.Add(node, 0);
        CreateConnection(node.InPoint, parentNode.OutPoint, false);
    }

    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(delta, true);
            }
        }

        GUI.changed = true;
    }

    private void OnClickAddNode(Vector2 mousePosition, TagNodeType type)
    {
        if (nodes == null)
        {
            if (type != TagNodeType.Root)
            {
                Debug.LogError("Please add root tag first.");
                return;
            }
            nodes = new List<TagNode>();
        }
        else if (_rootNode != null && type == TagNodeType.Root)
        {
            Debug.LogError("Only one root node can exist.");
            return;
        }

        TagNodeArg arg = new TagNodeArg(_tagAsset.args.Count, type, mousePosition, 200, 50, GenerateTagTitle("TagGroup"));
        _tagAsset.args.Add(arg);
        TagNode node;
        if (type == TagNodeType.Parent)
        {
            node = new TagNode(arg, parentNodeStyle, parentSelectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickSelectNode);
            nodes.Add(node);
            //新建Node时默认带一个ChildNode
            AddChildNode(node);
        }
        else
        {
            node = new TagNode(arg, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickSelectNode);
            nodes.Add(node);
            if (type == TagNodeType.Root)
            {
                node.arg.title = "FirstPackage";
                _rootNode = node;
            }
        }
        //_node2InDegree.Add(node, 0);
        Refresh();
    }

    public void OnClickAddChildNode(TagNode parentNode)
    {
        AddChildNode(parentNode);
    }
    
    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection(true);
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection(true);
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickRemoveNode(TagNode node)
    {
        if (node.arg.type == TagNodeType.Root)
        {
            if (nodes.Count > 1)
            {
                Debug.LogError("Can't remove root node.");
                return;
            }
            else
            {
                _rootNode = null;
            }
        }
        if (connections != null)
        {
            List<Connection> inConnections = new List<Connection>();
            List<Connection> outConnections = new List<Connection>();

            foreach (var conn in connections)
            {
                if (conn.inPoint == node.InPoint)
                {
                    inConnections.Add(conn);
                }
                else if (conn.outPoint == node.OutPoint)
                {
                    outConnections.Add(conn);
                }
            }
            
            foreach (Connection inConn in inConnections)
            {
                connections.Remove(inConn);
            }
            foreach (Connection outConn in outConnections)
            {
                connections.Remove(outConn);
                //_node2InDegree[outConn.inPoint.node]--;
            }
        }
        nodes.Remove(node);
        if (node.arg.title == AssetBundleLoaderSetting.OthersTag)
        {
            editorWindow._tagAsset.manuallyOtherTag = false;
        }
        //_node2InDegree.Remove(node);
    }

    private void OnClickSelectNode(TagNode node)
    {
        selector.selectedNode = node;
        Selection.activeObject = selector;
        //Debug.Log("Select node");
        NodeInspector.isDirty = true;
    }

    private void OnClickRemoveConnection(Connection connection)
    {
        //_node2InDegree[connection.inPoint.node]--;
        connections.Remove(connection);
    }

    private void CreateConnection(bool isShow)
    {
        CreateConnection(selectedInPoint, selectedOutPoint, isShow);
    }

    public void CreateConnection(ConnectionPoint inPoint, ConnectionPoint outPoint, bool isShow)
    {
        CreateConnection_Data(inPoint, outPoint, isShow);
        connections.Add(new Connection(inPoint, outPoint, isShow, OnClickRemoveConnection));
        Refresh();
    }

    private void CreateConnection_Data(ConnectionPoint inPoint, ConnectionPoint outPoint, bool isShow)
    {
        if (connections == null)
        {
            connections = new List<Connection>();
        }

        if (isShow)
        {
            inPoint.node.arg.SetParent(outPoint.node);
            outPoint.node.arg.AddChild(inPoint.node);
        }
        else
        {
            inPoint.node.arg.SetParent(outPoint.node);
            outPoint.node.arg.SetNext(inPoint.node);
        }
        
        //_node2InDegree[inPoint.node]++;
    }

    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }

    private string GenerateTagTitle(string title)
    {
        int index = 1;
        string result = title + index;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].arg.title == result)
            {
                result = title + (++index);
            }
        }

        return result;
    }
}

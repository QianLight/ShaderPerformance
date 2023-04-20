using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;

namespace com.pwrd.hlod.editor
{
    public enum Column
    {
        Name,
        Visible,
        OverrideSetting,
        IgnoreGenerator,
        ProxyVisible,
        Batches,
        OriginalTris,
        OptimizedTris,
        Ratio,
        MeshSize,
        AlbedoSize,
        ColumnCount,
    }

    public class HLODTreeViewItem : TreeViewItem
    {
        public float[] values = new float[(int) Column.ColumnCount];
        public bool[] hasData = new bool[(int) Column.ColumnCount];
        public UnityEngine.Object obj;
        public string path;
        public int priority;

        public virtual bool selfVisible
        {
            get
            {
                if (children == null)
                    return true;
                foreach (var viewitem in children)
                {
                    var item = viewitem as HLODTreeViewItem;
                    if (item == null)
                        HLODDebug.Log(" [HLODTreeViewItem] " + viewitem.GetType());
                    if (item != null && !item.selfVisible)
                        return false;
                }

                return true;
            }
        }

        public virtual int visibleState
        {
            get
            {
                if (children == null)
                    return -1;

                int activeCount = 0;
                int unactiveCount = 0;
                foreach (var viewitem in children)
                {
                    var item = viewitem as HLODTreeViewItem;
                    if (item == null)
                        HLODDebug.Log(" [HLODTreeViewItem] " + viewitem.GetType());
                    if (item != null && !item.selfVisible)
                        unactiveCount++;
                    else
                        activeCount++;
                }

                if (activeCount != 0 && unactiveCount != 0)
                    return 0;
                return unactiveCount == 0 ? 1 : -1;
            }
        }

        public int layerIndex = -1;
        public Action<int> onDoubleClick;
        public Action<int> onContexClick;
        public SelectChange onSelectChange;

        public delegate List<UnityEngine.Object> SelectChange();


        private static MultiColumnHeaderState.Column ColumnSetup(Column col, GUIContent content, int width, int maxWidth,
            bool sortedAscending = false)
        {
            return new MultiColumnHeaderState.Column
            {
                headerContent = content,
                width = width,
                maxWidth = maxWidth,
                sortingArrowAlignment = TextAlignment.Left,
                sortedAscending = sortedAscending,
            };
        }

        public static MultiColumnHeaderState.Column[] columnSetup =
        {
            ColumnSetup(Column.Name, new GUIContent("名称"), 200, 800, sortedAscending: true),
            ColumnSetup(Column.Visible, new GUIContent("场景显隐"), 50, 100),
            ColumnSetup(Column.OverrideSetting, new GUIContent("重载设置", "开启重载后，可单独指定面板设定；关闭后使用全局设置"), 50, 100),
            ColumnSetup(Column.IgnoreGenerator, new GUIContent("忽略生成", "开启后，生成时忽略此项"), 50, 70),
            ColumnSetup(Column.ProxyVisible, new GUIContent("代理网格"), 100, 100),
            ColumnSetup(Column.Batches, new GUIContent("对象数量", "Renderers数量"), 50, 100),
            ColumnSetup(Column.OriginalTris, new GUIContent("原始面数", "原始三角形面数总和"), 100, 200),
            ColumnSetup(Column.OptimizedTris, new GUIContent("优化后面数", "hlod资源三角形面数总和"), 100, 200),
            ColumnSetup(Column.Ratio, new GUIContent("优化比例", "优化三角形面数百分比"), 50, 100),
            ColumnSetup(Column.MeshSize, new GUIContent("网格 RuntimeSize", "hlod资源Mesh运行时占用内存"), 100, 200),
            ColumnSetup(Column.AlbedoSize, new GUIContent("贴图 RuntimeSize", "hlod资源贴图运行时占用内存"), 100, 200),
        };

        public HLODTreeViewItem(HLODTreeViewItem parent, UnityEngine.Object obj, string displayName, int layerIndex)
        {
            this.displayName = displayName;
            this.layerIndex = layerIndex;
            //this.priority = priority;
            this.obj = obj;
            SetData(Column.Name, 0);

            if (parent != null)
            {
                parent.AddChild(this);
                depth = parent.depth + 1;
                path = parent.path + "/" + displayName;
                if (obj != null)
                    path += obj.GetInstanceID();
                id = path.GetHashCode();
            }
            else
            {
                depth = -1;
            }
        }

        public void SetData(Column column, float value)
        {
            values[(int) column] = value;
            hasData[(int) column] = true;
        }

        public float GetData(Column column)
        {
            return values[(int) column];
        }

        protected void OnSelectChange()
        {
            HLODMessageCenter.SendMessage(HLODMesssages.HLOD_TREE_LAYER_ITEM_CHANGED, layerIndex);
        }

        public virtual void SetVisible(bool visible)
        {
            var go = obj as GameObject;
            if (go != null)
            {
                if (visible) SceneVisibilityManager.instance.Show(go, true);
                else SceneVisibilityManager.instance.Hide(go, true);
            }
        }
    }
    
    public class SceneTreeViewItem : HLODTreeViewItem
    {
        public SceneNode scene;
        
        private List<UnityEngine.Object> GetSelectChanged()
        {
            base.OnSelectChange();
            return null;
        }

        public SceneTreeViewItem(HLODTreeViewItem parent, SceneNode sceneNode, UnityEngine.Object obj, string displayName, int layerIndex)
            : base(parent, obj, displayName, layerIndex)
        {
            this.scene = sceneNode;
            // onDoubleClick = null
            onContexClick = (int id) =>
            {
                return;
                // GenericMenu menu = new GenericMenu();
                // menu.AddItem(EditorGUIUtility.TrTextContent("重生成Cluster"), false,
                //     () => HLODDebug.Log("重生成Cluster"));
                // menu.AddItem(EditorGUIUtility.TrTextContent("重生成ProxyMesh"), false,
                //     () => HLODDebug.Log("重生成ProxyMesh"));
                // menu.AddItem(EditorGUIUtility.TrTextContent("删除Cluster"), false,
                //     () => HLODDebug.Log("删除Cluster"));
                // menu.ShowAsContext();
            };

            onSelectChange = GetSelectChanged;
        }
    }

    public class LayerTreeViewItem : HLODTreeViewItem
    {
        public Layer layer;
        
        private List<UnityEngine.Object> GetSelectChanged()
        {
            base.OnSelectChange();
            return null;
        }

        public LayerTreeViewItem(HLODTreeViewItem parent, Layer layer, UnityEngine.Object obj, string displayName, int layerIndex)
            : base(parent, obj, displayName, layerIndex)
        {
            this.layer = layer;
            // onDoubleClick = null
            onContexClick = (int id) =>
            {
                return;
                // GenericMenu menu = new GenericMenu();
                // menu.AddItem(EditorGUIUtility.TrTextContent("重生成Cluster"), false,
                //     () => HLODDebug.Log("重生成Cluster"));
                // menu.AddItem(EditorGUIUtility.TrTextContent("重生成ProxyMesh"), false,
                //     () => HLODDebug.Log("重生成ProxyMesh"));
                // menu.AddItem(EditorGUIUtility.TrTextContent("删除Cluster"), false,
                //     () => HLODDebug.Log("删除Cluster"));
                // menu.ShowAsContext();
            };

            onSelectChange = GetSelectChanged;
        }
    }

    public class ClusterTreeViewItem : HLODTreeViewItem
    {
        public  Cluster cluster;

        private List<UnityEngine.Object> GetSelectChanged()
        {
            base.OnSelectChange();
            List<GameObject> objs = new List<GameObject>();
            cluster.CollectManifests(objs);
            return new List<UnityEngine.Object>(objs.Cast<GameObject>());
        }

        public ClusterTreeViewItem(HLODTreeViewItem parent,  Cluster cluster, UnityEngine.Object obj,
            string displayName, int layerIndex)
            : base(parent, obj, displayName, layerIndex)
        {
            this.cluster = cluster;
            onDoubleClick = (int id) =>
            {
                SceneView.FrameLastActiveSceneView();
                if (obj != null || obj is GameObject)
                {
                    var go = obj as GameObject;
                    SceneView.lastActiveSceneView.LookAt(go.transform.position);
                }
            };

            onSelectChange = GetSelectChanged;
        }
    }

    public class SubClusterTreeViewItem : HLODTreeViewItem
    {
        public  Cluster cluster;
        public ClusterTreeViewItem clusterItem;

        public override bool selfVisible
        {
            get { return clusterItem.selfVisible; }
        }

        public override int visibleState
        {
            get { return clusterItem.visibleState; }
        }

        private List<UnityEngine.Object> GetSelectChanged()
        {
            base.OnSelectChange();
            List<GameObject> objs = new List<GameObject>();
            cluster.CollectManifests(objs);
            return new List<UnityEngine.Object>(objs.Cast<GameObject>());
        }

        public SubClusterTreeViewItem(HLODTreeViewItem parent, ClusterTreeViewItem clusterItem,
             Cluster cluster, string displayName, int layerIndex)
            : base(parent, null, displayName, layerIndex)
        {
            this.clusterItem = clusterItem;
            this.cluster = cluster;
            onDoubleClick = (int id) =>
            {
                HLODMessageCenter.SendMessage(HLODMesssages.HLOD_SUB_CLUSTER_DOUBLE_CLICK, clusterItem.parent.id, clusterItem.id);
            };

            onSelectChange = GetSelectChanged;
        }
    }

    public class GameObjectTreeViewItem : HLODTreeViewItem
    {
        public override bool selfVisible
        {
            get { return obj == null ? false : !SceneVisibilityManager.instance.IsHidden(obj as GameObject); }
        }

        public override int visibleState
        {
            get { return selfVisible ? 1 : -1; }
        }

        private List<UnityEngine.Object> GetSelectChanged()
        {
            base.OnSelectChange();
            List<UnityEngine.Object> objs = new List<UnityEngine.Object>();
            objs.Add(obj);
            return objs;
        }

        public GameObjectTreeViewItem(HLODTreeViewItem parent, UnityEngine.Object obj, string displayName,
            int layerIndex)
            : base(parent, obj, displayName, layerIndex)
        {
            onDoubleClick = (int id) =>
            {
                if (obj != null || obj is GameObject)
                {
                    var go = obj as GameObject;
                    SceneView.lastActiveSceneView.LookAt(go.transform.position);
                    SceneView.FrameLastActiveSceneView();
                }
            };

            onSelectChange = GetSelectChanged;
        }
    }
}
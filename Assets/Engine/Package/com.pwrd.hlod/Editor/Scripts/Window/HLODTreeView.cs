using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using com.pwrd.hlod;
using Object = UnityEngine.Object;

namespace com.pwrd.hlod.editor
{
    public class HLODTreeView : TreeView
    {
        internal class ItemComparer : IComparer<TreeViewItem>
        {
            internal int col;
            internal bool isAscending;

            public int Compare(TreeViewItem x, TreeViewItem y)
            {
                var sign = isAscending ? 1 : -1;
                var lhs = (HLODTreeViewItem) x;
                var rhs = (HLODTreeViewItem) y;
                if (lhs.priority != rhs.priority)
                {
                    return lhs.priority - rhs.priority;
                }

                var result = (lhs.values[col] - rhs.values[col]) * sign;
                if (result > 0) return 1;
                else return -1;
            }
        }

        public Action<HLODTreeViewItem, bool> onChangeVisibility;
        private readonly ItemComparer comparer;
        private GUIStyle lineStyle;
        public HLODSceneEditorData data;
        public List<TreeViewItem> items = new List<TreeViewItem>();
        private ClusterTreeViewItem curSelectionTreeViewItem;

        public HLODTreeView(TreeViewState state) : base(state)
        {
            multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(HLODTreeViewItem.columnSetup));
            multiColumnHeader.sortingChanged += header => { Reload(); };
            multiColumnHeader.sortedColumnIndex = (int) Column.Batches;
            multiColumnHeader.ResizeToFit();
            onChangeVisibility += OnChangeVisibility;

            comparer = new ItemComparer();
            showBorder = false;
            showAlternatingRowBackgrounds = true;
            lineStyle = null;
        }

        public ClusterTreeViewItem GetSelectionTreeViewItem()
        {
            return curSelectionTreeViewItem;
        }

        protected override TreeViewItem BuildRoot()
        {
            return new HLODTreeViewItem(null, null, "根节点", -1);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            curSelectionTreeViewItem = null;
            items.Clear();
            if (data == null)
                return items;

            var rows = RebuildTree((HLODTreeViewItem) root);

            comparer.col = 0;
            if (multiColumnHeader.sortedColumnIndex != -1)
                comparer.col = multiColumnHeader.sortedColumnIndex;
            comparer.isAscending = multiColumnHeader.GetColumn(comparer.col).sortedAscending;

            if (null != rows)
            {
                foreach (var viewItem in rows)
                    CollectAndSortRows(viewItem, items);
            }

            return items;
        }
        
        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.selected)
            {
                if (args.item is ClusterTreeViewItem)
                {
                    curSelectionTreeViewItem = (ClusterTreeViewItem) args.item;
                }
                else
                {
                    curSelectionTreeViewItem = null;
                }
            }
            int numVisibleColumns = args.GetNumVisibleColumns();
            for (int i = 0; i < numVisibleColumns; ++i)
            {
                int column = args.GetColumn(i);
                Rect cellRect = args.GetCellRect(i);
                if (column == 0)
                {
                    if (lineStyle == null)
                        lineStyle = new GUIStyle(DefaultStyles.label);
                    cellRect.xMin += lineStyle.margin.left + GetContentIndent(args.item);

                    const int width = 16;
                    const int padding = 2;

                    Rect position = cellRect;
                    position.width = width;
                    Texture icon = args.item.icon;
                    if (icon != null)
                        GUI.DrawTexture(position, icon, ScaleMode.ScaleToFit);

                    if (Event.current.type == EventType.Repaint)
                    {
                        lineStyle.padding.left = !(icon == null) ? width + padding : 0;
                        lineStyle.Draw(cellRect, args.item.displayName, false, false, args.selected, args.focused);
                    }
                    var item = args.item as ClusterTreeViewItem;
                    if (item != null)
                    {
                        var deletePos = cellRect;
                        deletePos.x = deletePos.x + deletePos.width - 20;
                        deletePos.width = 10;
                        if (GUI.Button(deletePos, "  ", "OL Minus"))
                        {
                            HLODProvider.Instance.RemoveCluster(item.cluster);
                            Reload();
                        }
                    }
                }
                else if (column == (int) Column.Visible)
                {
                    var item = (HLODTreeViewItem) args.item;

                    var visible = EditorGUI.Toggle(cellRect, item.selfVisible);
                    if (visible != item.selfVisible)
                        SetVisibleRecursive(item, visible);

                    int state = item.visibleState;
                    if (!visible && state == 0)
                    {
                        Vector2 offset = new Vector2(2, 5);
                        Vector2 size = new Vector2(10, 7);
                        var rect = new Rect(cellRect.x + offset.x, cellRect.y + offset.y, size.x, size.y);
                        EditorGUI.LabelField(rect, new GUIContent(GetColorTexture(Color.black)));
                    }
                }
                else if (column == (int) Column.OverrideSetting)
                {
                    if (args.item is SceneTreeViewItem)
                    {
                        var item = args.item as SceneTreeViewItem;
                        item.scene.useOverrideSetting = EditorGUI.Toggle(cellRect, item.scene.useOverrideSetting);
                    }
                    else if (args.item is LayerTreeViewItem)
                    {
                        var item = args.item as LayerTreeViewItem;
                        item.layer.useOverrideSetting = EditorGUI.Toggle(cellRect, item.layer.useOverrideSetting);
                    }
                    else if (args.item is ClusterTreeViewItem)
                    {
                        var item = args.item as ClusterTreeViewItem;
                        item.cluster.useOverrideSetting = EditorGUI.Toggle(cellRect, item.cluster.useOverrideSetting);
                    }
                }
                else if (column == (int) Column.IgnoreGenerator)
                {
                    if (args.item is SceneTreeViewItem)
                    {
                        var item = args.item as SceneTreeViewItem;
                        var ignoreGenerator = EditorGUI.Toggle(cellRect, item.scene.ignoreGenerator);
                        if (ignoreGenerator != item.scene.ignoreGenerator)
                            item.scene.UpdateIgnoreGenerator(ignoreGenerator);
                    }
                    else if (args.item is LayerTreeViewItem)
                    {
                        var item = args.item as LayerTreeViewItem;
                        var ignoreGenerator = EditorGUI.Toggle(cellRect, item.layer.ignoreGenerator);
                        if (ignoreGenerator != item.layer.ignoreGenerator)
                            item.layer.UpdateIgnoreGenerator(ignoreGenerator);
                    }
                    else if (args.item is ClusterTreeViewItem)
                    {
                        var item = args.item as ClusterTreeViewItem;
                        var ignoreGenerator = EditorGUI.Toggle(cellRect, item.cluster.ignoreGenerator);
                        if (ignoreGenerator != item.cluster.ignoreGenerator)
                            item.cluster.UpdateIgnoreGenerator(ignoreGenerator);
                    }
                }
                else if (column == (int) Column.ProxyVisible)
                {
                    var item = args.item as ClusterTreeViewItem;
                    if (item != null)
                    {
                        var splitRect1 = new Rect(cellRect.x, cellRect.y, cellRect.width * 0.5f, cellRect.height);
                        var splitRect2 = new Rect(cellRect.x + cellRect.width * 0.5f, cellRect.y, cellRect.width * 0.5f, cellRect.height);
                        var r = item.cluster.hlodResult;
                        var proxyGO = r == null ? null : r.instance;
                        if (proxyGO == null)
                        {
                            EditorGUI.LabelField(splitRect1, "未生成");
                        }
                        else
                        {
                            var proxy = proxyGO.GetComponent<Proxy>();
                            var style = new GUIStyle();
                            var rect = new Rect(splitRect1.position + new Vector2(splitRect1.size.x / 2 - 20, 0),
                                new Vector2(40, splitRect1.size.y));
                            if (GUI.Button(rect, "查看"))
                            {
                                Selection.objects = new Object[] {proxy.proxyRenderer.gameObject};
                                SceneView.FrameLastActiveSceneView();
                                //SceneView.lastActiveSceneView.LookAt(proxy.proxyRenderer.transform.position);
                            }
                        }

                        if (GUI.Button(splitRect2, "生成"))
                        {
                            HLODProvider.Instance.BuildProxyMesh(item.cluster);
                        }
                    }
                }
                else if (column == (int) Column.MeshSize ||
                         column == (int) Column.AlbedoSize)
                {
                    string content = GetItemSizeContent(args, column);
                    if (content != null)
                        DefaultGUI.LabelRightAligned(cellRect, content, args.selected, args.focused);
                }
                else
                {
                    string content = GetItemContent(args, column);
                    if (content != null)
                        DefaultGUI.LabelRightAligned(cellRect, content, args.selected, args.focused);
                }
            }
        }

        private string GetItemContent(RowGUIArgs args, int column)
        {
            var item = (HLODTreeViewItem) args.item;
            if (!item.hasData[column])
                return "-";

            if (item.values[column] < 1) return item.values[column].ToString("0.00");
            return item.values[column].ToString();
        }

        private string GetItemSizeContent(RowGUIArgs args, int column)
        {
            var item = (HLODTreeViewItem) args.item;
            if (!item.hasData[column])
                return "-";

            var size = item.values[column];// * 4;
            if (size < 1024)
                return size.ToString("0B");
            else if (size < 1024 * 1024f)
                return (size / 1024).ToString("0.0KB");
            else if (size < 1024 * 1024 * 1024f)
                return (size / (1024 * 1024f)).ToString("0.0MB");
            return "-";
        }

        private void SetVisibleRecursive(HLODTreeViewItem item, bool visible)
        {
            if (onChangeVisibility != null)
            {
                if (item.children != null && item.children.Count > 0)
                {
                    foreach (HLODTreeViewItem child in item.children)
                    {
                        if (child != null)
                        {
                            SetVisibleRecursive(child, visible);
                        }
                    }
                }

                if (item is SubClusterTreeViewItem)
                {
                    SetVisibleRecursive((item as SubClusterTreeViewItem).clusterItem, visible);
                }

                if (item is ClusterTreeViewItem)
                {
                    var clusterItem = item as ClusterTreeViewItem;
                    var r = clusterItem.cluster.hlodResult;
                    if (r != null && r.instance != null)
                    {
                        var proxy = r.instance;
                        proxy.gameObject.SetActive(!visible);
                    }
                }


                OnChangeVisibility(item, visible);
                //item.selfVisible = visible;
            }
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as HLODTreeViewItem;
            if (item == null)
                return;
            item.onContexClick?.Invoke(id);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            List<UnityEngine.Object> selectObjects = new List<UnityEngine.Object>();
            foreach (var id in selectedIds)
            {
                var item = FindItem(id, rootItem) as HLODTreeViewItem;
                if (item == null)
                    return;
                if (item.onSelectChange != null)
                {
                    var objs = item.onSelectChange.Invoke();
                    if (objs != null)
                        selectObjects.AddRange(objs);
                }
            }

            Selection.objects = selectObjects.ToArray();
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as HLODTreeViewItem;
            if (item == null)
                return;
            item.onDoubleClick?.Invoke(id);
        }

        private void CollectAndSortRows(TreeViewItem item, IList<TreeViewItem> rows)
        {
            rows.Add(item);
            if (!item.hasChildren || !IsExpanded(item.id))
                return;

            //item.children.Sort(comparer);
            foreach (TreeViewItem child in item.children)
            {
                if (child == null)
                {
                    HLODDebug.LogError($"one children of {item.displayName} is null");
                    continue;
                }

                CollectAndSortRows(child, rows);
            }
        }

        private void OnChangeVisibility(HLODTreeViewItem item, bool visible)
        {
            item.SetVisible(visible);
        }

        public SceneTreeViewItem[] sceneItems;
        private Dictionary<string, ClusterTreeViewItem> m_itemMap = new Dictionary<string, ClusterTreeViewItem>();

        public HLODTreeViewItem[] RebuildTree(HLODTreeViewItem root)
        {
            if (data == null)
                return null;

            Dictionary<Cluster, ClusterTreeViewItem> cache =
                new Dictionary<Cluster, ClusterTreeViewItem>();

            sceneItems = new SceneTreeViewItem[data.scenes.Count];
            for (int i = 0; i < data.scenes.Count; i++)
            {
                int layerTriangleCount = 0, layerMaterialCount = 0;

                float resultTriangleCount = 0, resultMeshSize = 0, resultAlbedoSize = 0;// resultLightmapSize = 0;
                var sceneNode = data.scenes[i];
                var sceneNodeItem = new SceneTreeViewItem(root, sceneNode, null, sceneNode.sceneName, i);
                
                for (int j = 0; j < sceneNode.layers.Count; j++)
                {
                    var layer = sceneNode.layers[j];
                    layer.index = j;
                    var clusterItem = BuildLayerViewItem(sceneNodeItem, layer, j, ref layerTriangleCount,
                        ref layerMaterialCount);
                    resultTriangleCount += clusterItem.GetData(Column.OptimizedTris);
                    resultMeshSize += clusterItem.GetData(Column.MeshSize);
                    resultAlbedoSize += clusterItem.GetData(Column.AlbedoSize);
                }
                sceneNodeItem.SetData(Column.OriginalTris, layerTriangleCount);
                sceneNodeItem.SetData(Column.Batches, layerMaterialCount);
                sceneNodeItem.SetData(Column.OptimizedTris, resultTriangleCount);
                sceneNodeItem.SetData(Column.MeshSize, resultMeshSize);
                sceneNodeItem.SetData(Column.AlbedoSize, resultAlbedoSize);
                sceneNodeItem.SetData(Column.Ratio, resultTriangleCount / layerTriangleCount);
                sceneItems[i] = sceneNodeItem;
            }
            return sceneItems;
        }

        private LayerTreeViewItem BuildLayerViewItem(HLODTreeViewItem sceneItem, Layer layer, int layerIndex,
            ref int layerTriangleCount, ref int layerMaterialCount)
        {
            float resultTriangleCount = 0, resultMeshSize = 0, resultAlbedoSize = 0;// resultLightmapSize = 0;
            var layerItem = new LayerTreeViewItem(sceneItem, layer, null, "Layer" + layerIndex, layerIndex);
            for (int j = 0; j < layer.clusters.Count; j++)
            {
                var cluster = layer.clusters[j];
                var clusterItem = BuildClusterViewItem(layerItem, cluster, ref layerTriangleCount,
                    ref layerMaterialCount);
                resultTriangleCount += clusterItem.GetData(Column.OptimizedTris);
                resultMeshSize += clusterItem.GetData(Column.MeshSize);
                resultAlbedoSize += clusterItem.GetData(Column.AlbedoSize);
            }

            layerItem.SetData(Column.OriginalTris, layerTriangleCount);
            layerItem.SetData(Column.Batches, layerMaterialCount);
            layerItem.SetData(Column.OptimizedTris, resultTriangleCount);
            layerItem.SetData(Column.MeshSize, resultMeshSize);
            layerItem.SetData(Column.AlbedoSize, resultAlbedoSize);
            layerItem.SetData(Column.Ratio, resultTriangleCount / layerTriangleCount);
            
            return layerItem;
        }

        private ClusterTreeViewItem BuildClusterViewItem(HLODTreeViewItem layerItem, Cluster cluster,
            ref int layerTriangleCount, ref int layerMaterialCount)
        {
            int triangleCount = 0, materialCount = 0;
            cluster.CollectStatics(ref triangleCount, ref materialCount);
            var itemCluster = new ClusterTreeViewItem(layerItem, cluster, null, cluster.name, layerItem.layerIndex);
            m_itemMap[cluster.name] = itemCluster;
            itemCluster.SetData(Column.OriginalTris, triangleCount);
            itemCluster.SetData(Column.Batches, materialCount);
            layerTriangleCount += triangleCount;
            layerMaterialCount += materialCount;

            HLODResultData hlodResult = cluster.hlodResult;

            if (hlodResult != null && hlodResult.prefab != null)
            {
                var prefab = hlodResult.prefab;

                var meshFilter = prefab.GetComponentInChildren<MeshFilter>();
                var mesh = meshFilter.sharedMesh;
                var render = prefab.GetComponentInChildren<Renderer>();
                var mat = render.sharedMaterial;

                itemCluster.SetData(Column.OptimizedTris, ((float) mesh.triangles.Length / 3));
                itemCluster.SetData(Column.Ratio, (float) mesh.triangles.Length / 3 / triangleCount);
                itemCluster.SetData(Column.MeshSize, GetAssetSize(mesh));
                itemCluster.SetData(Column.AlbedoSize, GetAssetSize(mat.HasProperty("_BaseMap")? mat.GetTexture("_BaseMap") : null));
            }

            var removeSubClusters = new List<Cluster>();
            foreach (var subCluster in cluster.clusters)
            {
                ClusterTreeViewItem originItem = null;
                m_itemMap.TryGetValue(subCluster.name, out originItem);
                if (originItem != null)
                {
                    var subClusterTreeViewItem = new SubClusterTreeViewItem(itemCluster, originItem, subCluster, subCluster.name, layerItem.layerIndex);
                    int subTriangleCount = 0, subMaterialCount = 0;
                    originItem.cluster.CollectStatics(ref subTriangleCount, ref subMaterialCount);
                    subClusterTreeViewItem.SetData(Column.OriginalTris, subTriangleCount);
                    subClusterTreeViewItem.SetData(Column.Batches, subMaterialCount);
                    if (cluster.ignoreGenerator == false && originItem.cluster.ignoreGenerator == true)
                    {
                        originItem.cluster.ignoreGenerator = false;
                    }
                }
                else
                {
                    cluster.AddEntites(subCluster);
                    removeSubClusters.Add(subCluster);
                }
            }
            foreach (var removeSubCluster in removeSubClusters)
            {
                cluster.clusters.Remove(removeSubCluster);
            }
            removeSubClusters = null;

            foreach (var entity in cluster.entities)
            {
                if (null != entity.manifest)
                {
                    var gameObjectTreeViewItem = new GameObjectTreeViewItem(itemCluster, entity.manifest, entity.manifest.name, layerItem.layerIndex);
                    gameObjectTreeViewItem.SetData(Column.OriginalTris, entity.extraData.triangleCount);
                    gameObjectTreeViewItem.SetData(Column.Batches, entity.extraData.materialCount);
                }
                else
                {
                    // HLODDebug.LogWarning($"[HLOD][Manifest] manifest is null. cluster: {cluster.name}, index of manifest: {cluster.entities.IndexOf(entity)}");
                }
            }

            return itemCluster;
        }

        private long GetAssetSize(Object asset)
        {
            if (asset == null)
                return 0;
            try
            {
                var size = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(asset);
                return size;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        Dictionary<Color, Texture> m_texMap = new Dictionary<Color, Texture>();

        Texture GetColorTexture(Color color)
        {
            if (m_texMap.ContainsKey(color))
                return m_texMap[color];
            var tex = CreateTexture(color);
            m_texMap[color] = tex;
            return tex;
        }

        Texture2D CreateTexture(Color color)
        {
            int nWidth = 2;
            int nHeight = 2;
            Color[] aPixels = new Color[nWidth * nHeight];
            for (int nPixel = 0; nPixel < aPixels.Length; ++nPixel)
            {
                aPixels[nPixel] = color;
            }

            Texture2D texResult = new Texture2D(nWidth, nHeight);
            texResult.SetPixels(aPixels);
            texResult.wrapMode = TextureWrapMode.Repeat;
            texResult.Apply();
            return texResult;
        }
    }
}
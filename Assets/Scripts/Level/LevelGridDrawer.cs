using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CFUtilPoolLib;
using CFEngine;

namespace XLevel
{
    class LevelBlockDrawUnit
    {
        public GameObject blockRoot;
        public List<GameObject> blockGameObjects = new List<GameObject>();

        public GameObject FetchGO(int index)
        {
            if (index < blockGameObjects.Count)
            {
                return blockGameObjects[index];
            }
            else
            {
                GameObject go = new GameObject();
                go.transform.parent = blockRoot.transform;
                go.name = index.ToString();
                blockGameObjects.Add(go);

                return go;
            }
        }
    }

    public delegate bool IsGridSelect(QuadTreeElement grid);
    public delegate void CheckPoint(double MinX, double MaxX, double MinZ, double MaxZ);

    public class LevelGridDrawer
    {
        //public List<QuadTreeElement> m_SelectGrid;
        //public static List<QuadTreeElement> m_debugGrid;

        public IsGridSelect IsCurrentGridSelectCb;
        public IsGridSelect IsCurrentGridDebugCb;
        public CheckPoint CheckPointCb;
        public ViewMode viewMode = ViewMode.None;

        private int perGameObjectGrid = 10000;
        private LevelMapData m_data;
        //private LevelGridTool m_Tool;
        //public LevelAreaSetting m_AreaConfig;

        private Material m_material;
        private Material m_lineMaterial;
        private GameObject m_lineGo = null;
        private LineRenderer m_lr;

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color32> cols = new List<Color32>();
        List<int> indecies = new List<int>();

        GameObject root;

        //List<QuadTreeElement> ClampGrids = new List<QuadTreeElement>();
        Dictionary<LevelBlock, LevelBlockDrawUnit> m_BlockMesh = new Dictionary<LevelBlock, LevelBlockDrawUnit>();

        //public LevelGridDrawer(LevelGridTool tool)
        //{
        //    m_Tool = tool;
        //}

        public void SetData(LevelMapData data)
        {
            m_data = data;
        }

        public void Clear()
        {
            foreach (KeyValuePair<LevelBlock, LevelBlockDrawUnit> pair in m_BlockMesh)
            {
                pair.Value.blockGameObjects.Clear();
                GameObject.DestroyImmediate(pair.Value.blockRoot);
            }

            m_BlockMesh.Clear();

            if (root != null)
            {
                GameObject.DestroyImmediate(root);
                root = null;
            }

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public bool HasDrawGrid
        {
            get { return root != null; }
        }

        public void SmartDrawGrid()
        {
            if (HasDrawGrid)
                ReDrawMpaColors();
            else
                DrawMap();
        }

        public void DrawMap()
        {
            if (!m_data.IsDataReady) return;

            Clear();

            root = new GameObject();
            root.name = "GridRoot";
            root.transform.position = Vector3.zero;

            //ClimbEditor.InitCheck();
            for (int i = 0; i < m_data.m_Blocks.Count; ++i)
            {
                DrawBlock(m_data.m_Blocks[i]);
            }
            //ClimbEditor.CheckResult();
        }

        public void ReDrawMpaColors()
        {
            if (!m_data.IsDataReady) return;
            if (root == null) return;

            for (int i = 0; i < m_data.m_Blocks.Count; ++i)
            {
                ReDrawBlockColors(m_data.m_Blocks[i]);
            }
        }

        //public void DrawBlock(int x, int z)
        //{

        //}

        public void DrawBlock(LevelBlock block)
        {
            LevelBlockDrawUnit lbUnit = null;
            if (!m_BlockMesh.TryGetValue(block, out lbUnit))
            {
                lbUnit = new LevelBlockDrawUnit();
                lbUnit.blockRoot = new GameObject();
                lbUnit.blockRoot.transform.parent = root.transform;
                lbUnit.blockRoot.name = block.x + "_" + block.z;
                m_BlockMesh.Add(block, lbUnit);
            }

            //if (viewMode == ViewMode.DrawRuntimeGrid)
            //{
            //    m_material = new Material(Shader.Find("Unlit/Color"));
            //    if(m_material.HasProperty("_Color"))
            //    {
            //        m_material.SetColor("_Color", Color.red);
            //    }
            //}
#if UNITY_EDITOR
            if (m_material == null)
                m_material = AssetDatabase.LoadAssetAtPath("Assets/Tools/Common/Editor/Level/Res/GridMat.mat", typeof(Material)) as Material;
#else
            if (m_material == null) m_material = new Material(Shader.Find("Standard"));
            m_material.SetColor("_Color", Color.green);
#endif


            int goIndex = 0;
            Vector3 offset = new Vector3(m_data.m_Generator.offset.x, 0, m_data.m_Generator.offset.z)
;            Vector3 blockBasePositon = new Vector3(LevelMapData.MapMin.x +offset.x+ block.x * LevelMapData.BlockSize, 
                0, LevelMapData.MapMin.z + offset.z + block.z * LevelMapData.BlockSize);
            GameObject go = null;

            int index = 0;
            int gpb = LevelMapData.GridPerBlock;
            float gs = LevelMapData.GridSize;

            MeshFilter mf = null;
            MeshRenderer mr = null;
            Mesh mMesh = null;

            for (int i = 0; i < block.m_Grids.Count; ++i)
            {
                if (i % perGameObjectGrid == 0)
                {
                    go = lbUnit.FetchGO(goIndex);
                    goIndex++;

                    mf = go.GetComponent<MeshFilter>();
                    if (mf == null) mf = go.AddComponent<MeshFilter>();
                    mr = go.GetComponent<MeshRenderer>();
                    if (mr == null) mr = go.AddComponent<MeshRenderer>();
                    mr.sharedMaterials = new Material[] { m_material };
                    mMesh = mf.sharedMesh;
                    if (mMesh == null) mMesh = new Mesh();
                    mMesh.Clear();
                    mMesh.hideFlags = HideFlags.DontSave;
                    mMesh.name = go.name;

                    verts.Clear();
                    uvs.Clear();
                    cols.Clear();
                    indecies.Clear();
                    //ClampGrids.Clear();

                    index = 0;
                    go.transform.position = blockBasePositon;
                }

                //float height = block.m_Grids[i].pos.y;
                //Vector3 gridLocalPos = block.m_Grids[i].pos - blockBasePositon;
                //float IllustrationGridSize = LevelMapData.GridSize * 0.95f;
                //verts.Add(gridLocalPos + new Vector3(-IllustrationGridSize / 2, 0.1f, -IllustrationGridSize / 2));
                //verts.Add(gridLocalPos + new Vector3(IllustrationGridSize / 2, 0.1f, -IllustrationGridSize / 2));
                //verts.Add(gridLocalPos + new Vector3(IllustrationGridSize / 2, 0.1f, IllustrationGridSize / 2));
                //verts.Add(gridLocalPos + new Vector3(-IllustrationGridSize / 2, 0.1f, IllustrationGridSize / 2));

                float IllustrationGridSize = gs * 0.95f;
                float h1 = block.m_Grids[i].pos.y;
                float h2 = h1 + Mathf.Tan(block.m_Grids[i].alpha * Mathf.PI / 180) * gs;
                float h3 = h1 + Mathf.Tan(block.m_Grids[i].beta * Mathf.PI / 180) * gs;
                float h4 = h1 + Mathf.Tan(block.m_Grids[i].alpha * Mathf.PI / 180) * gs + Mathf.Tan(block.m_Grids[i].beta * Mathf.PI / 180) * gs;
                Vector3 v1 = new Vector3(block.m_Grids[i].pos.x - IllustrationGridSize / 2, h1 + 0.1f, block.m_Grids[i].pos.z - IllustrationGridSize / 2) 
                    - blockBasePositon +offset;
                Vector3 v2 = new Vector3(block.m_Grids[i].pos.x + IllustrationGridSize / 2, h2 + 0.1f, block.m_Grids[i].pos.z - IllustrationGridSize / 2)
                    - blockBasePositon +offset;
                Vector3 v3 = new Vector3(block.m_Grids[i].pos.x + IllustrationGridSize / 2, h4 + 0.1f, block.m_Grids[i].pos.z + IllustrationGridSize / 2) 
                    - blockBasePositon+offset;
                Vector3 v4 = new Vector3(block.m_Grids[i].pos.x - IllustrationGridSize / 2, h3 + 0.1f, block.m_Grids[i].pos.z + IllustrationGridSize / 2) 
                    - blockBasePositon+offset;
                verts.Add(v1);
                verts.Add(v2);
                verts.Add(v3);
                verts.Add(v4);

                AddQuadIndex(index, index + 1, index + 2, index + 3);

                index += 4;

                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(0f, 1f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(1f, 0f));

                FillGridColor(block.m_Grids[i], cols);

                if (i % perGameObjectGrid == perGameObjectGrid - 1 || i == block.m_Grids.Count - 1)
                {
                    mMesh.vertices = verts.ToArray();
                    mMesh.triangles = indecies.ToArray();
                    mMesh.uv = uvs.ToArray();
                    mMesh.colors32 = cols.ToArray();

                    mf.mesh = mMesh;
                }

                //ClimbEditor.CheckPoint(block.m_Grids[i].pos.x - 0.5 * LevelMapData.GridSize, block.m_Grids[i].pos.x + 0.5 * LevelMapData.GridSize, block.m_Grids[i].pos.z - 0.5 * LevelMapData.GridSize, block.m_Grids[i].pos.z + 0.5 * LevelMapData.GridSize);

                if (CheckPointCb != null)
                {
                    CheckPointCb(block.m_Grids[i].pos.x - 0.5 * LevelMapData.GridSize, block.m_Grids[i].pos.x + 0.5 * LevelMapData.GridSize, block.m_Grids[i].pos.z - 0.5 * LevelMapData.GridSize, block.m_Grids[i].pos.z + 0.5 * LevelMapData.GridSize);
                }
            }
        }

        public void ReDrawBlockColors(LevelBlock block)
        {
            LevelBlockDrawUnit lbUnit = null;
            if (!m_BlockMesh.TryGetValue(block, out lbUnit))
            {
                return;
            }

            GameObject go = null;
            int goIndex = 0;

            MeshFilter mf = null;
            Mesh mMesh = null;

            for (int i = 0; i < block.m_Grids.Count; ++i)
            {
                if (i % perGameObjectGrid == 0)
                {
                    go = lbUnit.blockGameObjects[goIndex];
                    goIndex++;
                    mf = go.GetComponent<MeshFilter>();
                    mMesh = mf.sharedMesh;
                    cols.Clear();
                }

                FillGridColor(block.m_Grids[i], cols);

                if (i % perGameObjectGrid == perGameObjectGrid - 1 || i == block.m_Grids.Count - 1)
                {
                    mMesh.colors32 = cols.ToArray();
                    mf.mesh = mMesh;
                }
            }
        }

        private void FillGridColor(QuadTreeElement grid, List<Color32> cols)
        {
            Color color = Color.blue;
            if (viewMode == ViewMode.WalkableMode)
            {
                if (!m_data.IsAudioSurface)
                {
                    color = Color.green;
                    if (grid.pos.y < 0) color = Color.red;
                    if (IsGridSelected(grid)) color = Color.red;
                    if (IsGridDebug(grid)) color = Color.red;
                    if ((grid.info & 0x01) > 0) color = Color.red;
                    if ((grid.info & 0x02) > 0) color = Color.yellow;
                    if ((grid.info & 0x08) > 0) color = Color.black;
                }
                else
                {
                    color = Color.red;
                    AudioSurfacesDefines.CheckFlagDebugColor(grid.info, ref color);
                }
            }
            
            else if (viewMode == ViewMode.AreaMode)
            {
                //color = m_AreaConfig.GetAreaColor(grid.area);
            }
            else if (viewMode == ViewMode.PatrolMode)
            {

            }
            cols.Add(color);
            cols.Add(color);
            cols.Add(color);
            cols.Add(color);
        }

        private void AddQuadIndex(int leftlower, int rightlower, int rightupper, int leftupper)
        {
            indecies.Add(leftlower);
            indecies.Add(rightupper);
            indecies.Add(rightlower);

            indecies.Add(leftlower);
            indecies.Add(leftupper);
            indecies.Add(rightupper);
        }

        private bool IsGridSelected(QuadTreeElement grid)
        {
            //if (m_Tool != null && m_Tool.CurrentMode != null)
            //    return m_Tool.CurrentMode.IsCurrentGridSelect(grid);
            if (IsCurrentGridSelectCb != null)
            {
                return IsCurrentGridSelectCb(grid);
            }
            return false;
        }

        private bool IsGridDebug(QuadTreeElement grid)
        {
            //if (m_Tool != null && m_Tool.CurrentMode != null)
            //    return m_Tool.CurrentMode.IsCurrentGridDebug(grid);
            if (IsCurrentGridDebugCb != null)
            {
                return IsCurrentGridDebugCb(grid);
            }
            return false;
        }

        public void DrawMapGrid(string sceneName)
        {
            LevelMapData mapData = new LevelMapData();
            mapData.LoadFromServerFile(sceneName);
            Clear();
            viewMode = ViewMode.WalkableMode;
            root = new GameObject();
            root.name = "GridRoot";
            root.transform.position = Vector3.zero;
            viewMode = ViewMode.DrawRuntimeGrid;
            SetData(mapData);
            
            for (int i = 0; i < mapData.m_Blocks.Count; ++i)
            {
                DrawBlock(mapData.m_Blocks[i]);
            }
        }

        public void DrawMapGrid(List<SceneChunk> chunks)
        {
            if (chunks.Count <= 0) return;
            LevelMapData mapData = new LevelMapData();

            for (int i = 0; i < chunks.Count; ++i)
            {
                LevelBlock blockData = new LevelBlock();
                blockData.x = chunks[i].x;
                blockData.z = chunks[i].z;
                blockData.BlockMin = new Vector3(chunks[i].chunkRect.min.x, 0, chunks[i].chunkRect.min.y);
                blockData.BlockMax = new Vector3(chunks[i].chunkRect.max.x, 0, chunks[i].chunkRect.max.y);
                blockData.m_QuadTree = chunks[i].externalData as QuadTreeMap;
                blockData.m_QuadTree.ExtractElments(blockData.m_Grids);
                mapData.m_Blocks.Add(blockData);
            }

            Clear();
            viewMode = ViewMode.DrawRuntimeGrid;
            root = new GameObject();
            root.name = "GridRoot";
            root.transform.position = Vector3.zero;

            for (int i = 0; i < mapData.m_Blocks.Count; ++i)
            {
                DrawBlock(mapData.m_Blocks[i]);
            }
        }



        public void DrawWayPoints(List<Vector3> points)
        {
            if (m_lineGo == null)
            {
                m_lineGo = new GameObject("waypoints");
                m_lr = m_lineGo.AddComponent<LineRenderer>();
            }

            if (m_lineMaterial == null)
                m_lineMaterial = Resources.Load<Material>("Line");
            m_lr.sharedMaterials = new Material[] { m_lineMaterial };
            m_lr.positionCount = points.Count;
            m_lr.SetPositions(points.ToArray());
            m_lr.startWidth = 0.1f;
            m_lr.endWidth = 0.1f;
        }

        public void ClearWayPoints()
        {
            GameObject.DestroyImmediate(m_lineGo);
            m_lineGo = null;
            m_lr = null;
        }
    }
}

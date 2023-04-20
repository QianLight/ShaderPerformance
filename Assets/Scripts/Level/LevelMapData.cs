using System;
using System.Collections.Generic;
using System.IO;
using CFEngine;
using CFUtilPoolLib;
using UnityEngine;

namespace XLevel
{
    public enum ViewMode
    {
        None,
        WalkableMode,
        AreaMode,
        PatrolMode,
        DrawRuntimeGrid,
    }

    public enum GridType
    {
        UpperSurface,
        LowerSurface,
        AimThrowSurface,
        MonsterSurface
    }

    public enum MapGridType
    {
        Normal=0,
        SeaBlock
    }

    public class LevelBlock
    {
        public int x;
        public int z;
        public Vector3 BlockMin;
        public Vector3 BlockMax;

        public List<QuadTreeElement> m_Grids;

        public QuadTreeMap m_QuadTree;

        public LevelBlock()
        {
            m_Grids = new List<QuadTreeElement>();
        }

        public void Clear()
        {
            x = 0;
            z = 0;
            if (m_Grids != null) m_Grids.Clear();

            BlockMin = Vector3.zero;
            BlockMax = Vector3.zero;

            m_QuadTree = null;
        }

        public QuadTreeElement GetGridByCoord(Vector3 pos, float heightDiff = 2.0f, byte flag = 0x0)
        {
            if (m_QuadTree == null)
            {
                m_QuadTree = new QuadTreeMap(16, LevelMapData.GridSize);

                QuadTreeBoundingBox box = new QuadTreeBoundingBox(BlockMin.x, BlockMax.x, BlockMin.z, BlockMax.z, LevelMapData.GridSize);

                m_QuadTree.BuildTree(m_Grids, box);
            }

            Vector3 gridcenter = Vector3.zero;
            QuadTreeElement.GetGridCentre(pos, ref gridcenter);

            QuadTreeElement search = new QuadTreeElement();
            search.pos = gridcenter;

            QuadTreeElement ret = m_QuadTree.FindElement(search, heightDiff, (MoveFlag)flag);

            if (ret != null)
            {
                for (int i = 0; i < m_Grids.Count; ++i)
                {
                    if (m_Grids[i].pos == ret.pos)
                        return m_Grids[i];
                }
            }

            return null;
        }

        public int GridCount
        {
            get
            {
                return m_Grids.Count;
            }
        }
        public QuadTreeElement GetGrid(int index)
        {
            return m_Grids[index];
        }

        bool CompareFunc(QuadTreeElement input, QuadTreeElement element, float h)
        {
            if (Math.Abs(input.X - element.X) <= LevelMapData.GridSize / 2 &&
                Math.Abs(input.Y - element.Y) <= LevelMapData.GridSize / 2)
                return true;

            return false;
        }

        public void SaveToFile(BinaryWriter sw)
        {
            int i = 0;

            List<float> height = new List<float>();
            List<byte> infos = new List<byte>();
            List<byte> alphas = new List<byte>();
            List<byte> betas = new List<byte>();

            while (i < m_Grids.Count)
            {
                short num = 1;
                height.Clear();
                infos.Clear();
                alphas.Clear();
                betas.Clear();
                height.Add(m_Grids[i].pos.y);
                infos.Add(m_Grids[i].info);
                alphas.Add(m_Grids[i].alpha);
                betas.Add(m_Grids[i].beta);

                int j = i + 1;
                while (j < m_Grids.Count && m_Grids[j].GetGridIndex() == m_Grids[i].GetGridIndex())
                {
                    height.Add(m_Grids[j].pos.y);
                    infos.Add(m_Grids[j].info);
                    alphas.Add(m_Grids[j].alpha);
                    betas.Add(m_Grids[j].beta);
                    i++;
                    j++;
                    num++;
                }

                sw.Write(m_Grids[i].GetGridIndex());
                sw.Write(num);
                for (int k = 0; k < height.Count; ++k)
                {
                    short h = (short)(height[k] * 100);
                    sw.Write(h);
                    sw.Write(infos[k]);
                    sw.Write(infos[k]);
                    sw.Write(alphas[k]);
                    sw.Write(betas[k]);
                }

                i++;
            }
        }
#if UNITY_EDITOR
        public void SaveToBlockFile(ref SceneContext context)
        {
            if (m_QuadTree == null)
            {
                m_QuadTree = new QuadTreeMap(16, LevelMapData.GridSize);

                QuadTreeBoundingBox box = new QuadTreeBoundingBox(BlockMin.x, BlockMax.x, BlockMin.z, BlockMax.z, LevelMapData.GridSize);

                m_QuadTree.BuildTree(m_Grids, box);
            }

            try
            {
                string clientDataPath = string.Format("{0}/{1}_{2}.mapheight", context.terrainDir, x.ToString(), z.ToString());
                using (FileStream fs = new FileStream(clientDataPath, FileMode.Create))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    m_QuadTree.WriteToFile(bw);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public void LoadFromBlockFile(ref SceneContext context)
        {
            if (m_QuadTree == null)
            {
                m_QuadTree = new QuadTreeMap(16, LevelMapData.GridSize);
            }

            m_QuadTree.Reset();

            try
            {
                string clientDataPath = string.Format("{0}/{1}_{2}.mapheight", context.terrainDir, x.ToString(), z.ToString());
                using (FileStream fs = new FileStream(clientDataPath, FileMode.Open))
                {
                    BinaryReader br = new BinaryReader(fs);
                    m_QuadTree.LoadFromFile(br);
                    m_QuadTree.ExtractElments(m_Grids);

                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
#endif
    }

    public class LevelMapData
#if UNITY_EDITOR
        : IMoveGrid
#endif
    {
        public static float GridSize { get; set; }
        public static int iGridSize { get; set; }
        public static float BlockSize { get; set; } // consistent with Terrain Tool
        public static int GridPerBlock { get { return DividGridSize(BlockSize); } }

        public static Vector3 MapMin { get; set; }
        public static Vector3 MapMax { get; set; }

        public static int GridRowCount { get; set; }
        public static int GridColCount { get; set; }
        public static int BlockRowCount { get; set; }
        public static int BlockColCount { get; set; }
        public static int MapType { get; set; }
        public static float UniqueHeight { get; set; }

        //private List<LevelGrid> m_RawData;
        public List<LevelBlock> m_Blocks;
        public string fileName;

        public LevelDataGenerator m_Generator = new LevelDataGenerator();

        //从1开始的版本号 复用原bytes文件的blocksize blocksize固定128
        //版本号1：添加maptype字段（0：普通格子 1：海域格子） 添加unique_height（海域平面唯一高度）
        public static int LastestVersion = 1;
        
        public enum VersionNumber
        {
            Version1=1
        }

        public LevelMapData()
        {
            iGridSize = 20;
            GridSize = (float)iGridSize / 100;
            BlockSize = 128.0f;
            //m_RawData = new List<LevelGrid>();
            m_Blocks = new List<LevelBlock>();
        }

        public bool IsDataReady
        {
            get { return m_Generator.IsGenerateFinish(); }
        }


        public bool IsAudioSurface;
        public bool IsDebugAudioSurface;

        public void Clear()
        {
            GridRowCount = 0;
            GridColCount = 0;

            BlockRowCount = 0;
            BlockColCount = 0;

            //m_RawData.Clear();
            for (int i = 0; i < m_Blocks.Count; ++i)
                m_Blocks[i].Clear();

            m_Blocks.Clear();
        }

        public static int DividGridSize(float f)
        {
            return (int)(f * 100 / iGridSize);
        }

#if UNITY_EDITOR
        public void OnUpdate()
        {
            m_Generator.OnUpdate();
        }

        public bool GenerateData(int gridSize, bool isScNull, int widthCount, int heightCount,bool isSeaBlock,
            float uniqueHeight,Vector3 offset,string objName="",bool ignoreRoot=false)
        {
            m_Generator.FindRoot(ignoreRoot);
            Clear();
            MapType = (int)(isSeaBlock ? MapGridType.SeaBlock : MapGridType.Normal);
            UniqueHeight = uniqueHeight;
            fileName = objName;
            iGridSize = gridSize;
            GridSize = ((float)gridSize) / 100;

            //if (bDevelop)
            //{
            //    iGridSize = 200;
            //    GridSize = 2.0f;
            //}
            m_Generator.offset = offset;
            return m_Generator.GeneratorData(this, isScNull, widthCount, heightCount);
        }

        public bool IsGenerateFinish()
        {
            return m_Generator.IsGenerateFinish();
        }

        public bool IsGenerateJustFinish()
        {
            return m_Generator.IsGenerateJustFinish();
        }
#if UNITY_EDITOR
        public bool SaveToFile(ref SceneContext context)
        {
            // save server data
            bool objGrid = !string.IsNullOrEmpty(fileName);
            string editorDataPath = string.Format("{0}/{1}_TerrainCollider.mapheight", context.terrainDir, objGrid?fileName:context.name);

            try
            {
                using (FileStream fs = new FileStream(editorDataPath, FileMode.Create))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(GridSize);
                    bw.Write(LastestVersion);
                    bw.Write(GridRowCount);
                    bw.Write(GridColCount);
                    bw.Write(MapMin.x);
                    bw.Write(MapMin.z);
                    bw.Write(MapMax.x);
                    bw.Write(MapMax.z);
                    bw.Write(MapType);
                    bw.Write(UniqueHeight);
                    for (int i = 0; i < m_Blocks.Count; i++)
                    {
                        m_Blocks[i].SaveToFile(bw);
                    }                    
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            string serverPath = string.Format("{0}/{1}_TerrainCollider.mapheight", "Assets/BundleRes/Table/SceneBlock",
                objGrid?fileName:context.name);
            File.Copy(editorDataPath, serverPath, true);

            // save client data
            //for (int i = 0; i < m_Blocks.Count; i++)
            //{
            //    m_Blocks[i].SaveToBlockFile (ref context);
            //}
            return true;
        }

        public bool LoadFromServerFile(ref SceneContext context)
        {
            try
            {
                m_Blocks.Clear();

                string serverPath = string.Format("{0}/{1}_TerrainCollider.mapheight", context.terrainDir, context.name);

                using (FileStream fs = new FileStream(serverPath, FileMode.Open))
                {
                    BinaryReader br = new BinaryReader(fs);

                    GridSize = br.ReadSingle();
                    iGridSize = (int)(GridSize * 100);
                    BlockSize = 128.0f;//br.ReadSingle();
                    var version = br.ReadInt32();//读取版本号
                    GridRowCount = br.ReadInt32();
                    GridColCount = br.ReadInt32();

                    float x = br.ReadSingle();
                    float z = br.ReadSingle();
                    MapMin = new Vector3(x, 0, z);

                    x = br.ReadSingle();
                    z = br.ReadSingle();

                    if (version == (int)VersionNumber.Version1)
                    {
                        MapType = br.ReadInt32();
                        UniqueHeight = br.ReadSingle();
                    }
                    MapMax = new Vector3(x, 0, z);

                    BlockRowCount = (GridRowCount + GridPerBlock - 1) / GridPerBlock;
                    BlockColCount = (GridColCount + GridPerBlock - 1) / GridPerBlock;

                    QuadTreeElement.gridSize = GridSize;
                    QuadTreeElement.gridcolcount = GridColCount;

                    for (int i = 0; i < GridRowCount * GridColCount; ++i)
                    {
                        LoadGrid(br);
                    }                    
                    m_Generator.state = GenerateState.LoadFinish;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }

            return true;
        }

        public bool LoadFromClientFile(ref SceneContext context)
        {
            try
            {
                m_Blocks.Clear();
                string serverPath = string.Format("{0}/{1}_TerrainCollider.mapheight",
                    context.terrainDir, context.name);

                using (FileStream fs = new FileStream(serverPath, FileMode.Open))
                {
                    BinaryReader br = new BinaryReader(fs);

                    GridSize = br.ReadSingle();
                    BlockSize = 128.0f;//br.ReadSingle();
                    var version = br.ReadInt32();
                    GridRowCount = br.ReadInt32();
                    GridColCount = br.ReadInt32();

                    float x = br.ReadSingle();
                    float z = br.ReadSingle();
                    MapMin = new Vector3(x, 0, z);

                    x = br.ReadSingle();
                    z = br.ReadSingle();
                    MapMax = new Vector3(x, 0, z);

                    BlockRowCount = (GridRowCount + GridPerBlock - 1) / GridPerBlock;
                    BlockColCount = (GridColCount + GridPerBlock - 1) / GridPerBlock;

                    QuadTreeElement.gridSize = GridSize;
                    QuadTreeElement.gridcolcount = GridColCount;
                }

                for (int z = 0; z < BlockRowCount; ++z)
                    for (int x = 0; x < BlockColCount; ++x)
                    {
                        LevelBlock blockData = new LevelBlock();
                        blockData.x = x;
                        blockData.z = z;
                        blockData.BlockMin = new Vector3(LevelMapData.MapMin.x + x * LevelMapData.BlockSize, 0, LevelMapData.MapMin.z + z * LevelMapData.BlockSize);
                        blockData.BlockMax = new Vector3(LevelMapData.MapMin.x + (x + 1) * LevelMapData.BlockSize, 0, LevelMapData.MapMin.z + (z + 1) * LevelMapData.BlockSize);

                        blockData.LoadFromBlockFile(ref context);

                        m_Blocks.Add(blockData);
                    }

                m_Generator.state = GenerateState.LoadFinish;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            return true;
        }
#endif
#endif

        public void LoadFromServerFile(string sceneName)
        {
            try
            {
                m_Blocks.Clear();
                string filePath = string.Format("Assets/BundleRes/Table/SceneBlock/{0}_TerrainCollider.mapheight", sceneName);
                if (!File.Exists(filePath))
                {
                    Debug.LogError("file not found :" + filePath);
                    return;
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    BinaryReader br = new BinaryReader(fs);

                    GridSize = br.ReadSingle();
                    BlockSize = 128.0f;//br.ReadSingle();
                    var version = br.ReadInt32();
                    GridRowCount = br.ReadInt32();
                    GridColCount = br.ReadInt32();

                    float x = br.ReadSingle();
                    float z = br.ReadSingle();
                    MapMin = new Vector3(x, 0, z);

                    x = br.ReadSingle();
                    z = br.ReadSingle();
                    MapMax = new Vector3(x, 0, z);

                    if (version == (int)VersionNumber.Version1)
                    {
                        MapType = br.ReadInt32();
                        UniqueHeight = br.ReadSingle();
                    }


                    BlockRowCount = (GridRowCount + GridPerBlock - 1) / GridPerBlock;
                    BlockColCount = (GridColCount + GridPerBlock - 1) / GridPerBlock;

                    QuadTreeElement.gridSize = GridSize;
                    QuadTreeElement.gridcolcount = GridColCount;

                    for (int i = 0; i < GridRowCount * GridColCount; ++i)
                    {
                        LoadGrid(br);
                    }
                    
                    m_Generator.state = GenerateState.LoadFinish;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public LevelBlock GetBlockByCoord(Vector3 pos)
        {
            for (int i = 0; i < m_Blocks.Count; i++)
            {
                if (pos.x >= m_Blocks[i].x * BlockSize && pos.x < (m_Blocks[i].x + 1) * BlockSize &&
                    pos.z >= m_Blocks[i].z * BlockSize && pos.z < (m_Blocks[i].z + 1) * BlockSize)

                    return m_Blocks[i];
            }

            return null;
        }

        public QuadTreeElement GetPosHeight(Vector3 queryPos, float heightDiff = 2.0f, byte flag = 0x0)
        {
            LevelBlock selectBlock = GetBlockByCoord(queryPos);
            if (selectBlock != null)
            {
                return selectBlock.GetGridByCoord(queryPos, heightDiff, flag);
            }

            return null;
        }

        //public QuadTreeElement GetGridByCoord(Vector3 pos)
        //{
        //    LevelBlock block = GetBlockByCoord(pos);

        //    return block == null ? null : block.GetGridByCoord(pos);
        //}

        public void LoadGrid(BinaryReader br)
        {
            if (br.BaseStream.Position >= br.BaseStream.Length) return;

            int index = br.ReadInt32();

            int z = index / GridColCount;
            int x = index - z * GridColCount;

            int blockX = x / GridPerBlock;
            int blockZ = z / GridPerBlock;

            LevelBlock block = FindBlock(blockX, blockZ);

            short num = br.ReadInt16();
            float grid_center_x = x * GridSize + GridSize / 2;
            float grid_center_z = z * GridSize + GridSize / 2;

            for (int i = 0; i < num; ++i)
            {
                short height = br.ReadInt16();
                byte info = br.ReadByte();
                br.ReadByte();
                byte alpha = br.ReadByte();
                byte beta = br.ReadByte();

                QuadTreeElement grid = new QuadTreeElement();
                grid.pos = new Vector3(grid_center_x, (float)height / 100, grid_center_z);
                grid.info = info;
                grid.alpha = alpha;
                grid.beta = beta;
                block.m_Grids.Add(grid);
            }
        }

        private LevelBlock FindBlock(int blockx, int blockz)
        {
            for (int i = 0; i < m_Blocks.Count; ++i)
            {
                if (m_Blocks[i].x == blockx && m_Blocks[i].z == blockz)
                    return m_Blocks[i];
            }

            LevelBlock blockData = new LevelBlock();
            blockData.x = blockx;
            blockData.z = blockz;
            blockData.BlockMin = new Vector3(MapMin.x + blockx * BlockSize, 0, MapMin.z + blockz * BlockSize);
            blockData.BlockMax = new Vector3(MapMin.x + (blockx + 1) * BlockSize, 0, MapMin.z + (blockz + 1) * BlockSize);
            m_Blocks.Add(blockData);

            return blockData;
        }

        public QuadTreeElement QueryGrid(Vector3 pos)
        {
            pos -= m_Generator.offset;
            LevelBlock selectBlock = GetBlockByCoord(pos);
            if (selectBlock != null)
            {
                QuadTreeElement selectGrid = selectBlock.GetGridByCoord(pos);
                return selectGrid;
            }

            return null;
        }

        #region test
        public void TestRaycast(Vector3 pos)
        {
            List<QuadTreeElement> test = new List<QuadTreeElement>();

            //Vector3 t = new Vector3(681.5f, 500, 280.5f);
            m_Generator.SingleGridRaycast(pos, test);
        }
#if UNITY_EDITOR

        public void TestSaveBlock(ref SceneContext context)
        {
            for (int i = 0; i < m_Blocks.Count; i++)
            {
                m_Blocks[i].SaveToBlockFile(ref context);
            }
        }

        public void TestLoadBlock(ref SceneContext context)
        {
            if (!IsDataReady) return;

            for (int i = 0; i < m_Blocks.Count; i++)
            {
                m_Blocks[i].LoadFromBlockFile(ref context);
            }

        }
#endif
        #endregion
#if UNITY_EDITOR
        #region interface
        public void Load(ref SceneContext context)
        {
            LoadFromServerFile(ref context);
        }

        public int BlockCount
        {
            get
            {
                return m_Blocks.Count;
            }
        }
        public void GetBlockOffset(int blockIndex, out float x, out float z)
        {
            var lb = m_Blocks[blockIndex];
            x = lb.x;
            z = lb.z;
        }

        public int GetGridCount(int blockIndex)
        {
            var lb = m_Blocks[blockIndex];
            return lb.GridCount;
        }

        public bool QueryGrid(int blockIndex, int gridIndex, ref Vector3 pos)
        {
            var lb = m_Blocks[blockIndex];
            var grid = lb.GetGrid(gridIndex);
            if (grid != null)
            {
                pos.x = grid.X;
                pos.y = grid.H;
                pos.z = grid.Y;
                return true;
            }
            return false;
        }
        #endregion
#endif
    }

    public class NaviPoint
    {
        public uint Index;
        public Vector3 Pos;

        public int linkedIndex;
        public GameObject go;

        public bool IsValid()
        {
            return go != null;
        }

        public void Destroy()
        {
            if (go != null)
                GameObject.DestroyImmediate(go);

            go = null;
            Pos = Vector3.zero;
            linkedIndex = -1;
        }

        public Material selectMat;
        public Material mat;

        public void SetSelected(bool selected)
        {
            if (go != null)
            {
                Renderer r = go.transform.GetChild(0).gameObject.GetComponent<Renderer>();

                if (selected)
                    r.sharedMaterial = selectMat; //LevelNavigation.NaviPointSelectMaterial;
                else
                    r.sharedMaterial = mat; //LevelNavigation.NaviPointMaterial;
            }
        }
    }

    public class NaviLine
    {
        public int startIndex;
        public int endIndex;

        public GameObject go;

        public void Destroy()
        {
            if (go != null)
                GameObject.DestroyImmediate(go);

            go = null;
            startIndex = endIndex = 0;
        }

        public void SetDirection(Vector3 start, Vector3 end, bool twoway, Material onewayMat, Material lineMat)
        {
            LineRenderer lr = go.GetComponent<LineRenderer>();

            lr.positionCount = 2;
            lr.SetPosition(0, start + new Vector3(0, 1, 0));
            lr.SetPosition(1, end + new Vector3(0, 1, 0));

            if (!twoway)
                lr.sharedMaterial = onewayMat; // LevelNavigation.NaviLineOnewayMaterial;
            else
                lr.sharedMaterial = lineMat; // LevelNavigation.NaviLineMaterial;

        }
    }

    public class LinkLine
    {
        public int startIndex;
        public int endIndex;

        public GameObject go;

        public void Destroy()
        {
            if (go != null)
                GameObject.DestroyImmediate(go);

            go = null;
            startIndex = endIndex = 0;
        }

        public void SetDirection(Vector3 start, Vector3 end)
        {
            LineRenderer lr = go.GetComponent<LineRenderer>();

            lr.positionCount = 2;
            lr.SetPosition(0, start + new Vector3(0, 1, 0));
            lr.SetPosition(1, end + new Vector3(0, 1, 0));
        }
    }
}
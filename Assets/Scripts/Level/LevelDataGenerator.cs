using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CFUtilPoolLib;
using CFEngine;

namespace XLevel
{
    class GridData : IComparable<GridData>
    {
        public float height;
        public float gradientHeight;
        public string GOname;
        public bool IsTerrain;
        public bool IsAirWall;
        public bool IsGridAirWall;
        public bool IsAimThrowGrid;
        public bool IsMonsterGrid;
        public GridType type;

        public byte alpha;
        public byte beta;

        public GridData(float f, GridType t, string name)
        {
            height = f;
            type = t;
            GOname = name;
        }

        public int CompareTo(GridData other)
        {
            if (this.height > other.height)
                return -1;
            else if (this.height < other.height)
                return 1;
            else
                return 0;
        }
    }

    class GridSegment
    {
        public static float threshold = 1.0f;

        public GridData upper;
        public GridData lower;

        public bool upperfaceIsAirWall = false;
        public bool lowerfaceisGridAirWall = false;

        public bool IsValid()
        {
            return upper != null && lower != null && upper.height > lower.height;
        }

        // true: two segment overlap, combine into one segment, data store in this
        // false: two segment not overlap, return two segment
        public bool Union(GridSegment m)
        {
            if(Math.Abs(this.lower.height - m.upper.height) < threshold)
            {
                this.lower = m.lower;
                return true;
            }
            
            if(Math.Abs(this.upper.height - m.lower.height) < threshold)
            {
                this.upper = m.upper;
                this.upperfaceIsAirWall = m.upperfaceIsAirWall;
                return true;
            }

            if (this.lower.height > m.upper.height || this.upper.height < m.lower.height) return false;

            this.upper = this.upper.height > m.upper.height ? this.upper : m.upper;
            this.lower = this.lower.height < m.lower.height ? this.lower : m.lower;
            if (this.upper == m.upper) this.upperfaceIsAirWall = m.upperfaceIsAirWall;
            return true;
        }
    }


    public enum GenerateState
    {
        NotGenerate,
        InGenerateProcessing,
        GenerateFinish,
        LoadFinish,
    }

    public class LevelDataGenerator
    {
        LevelMapData m_data;
        public static float height_max = 500.0f;

        private int generateIndex = -1;

        public static int layer_mask = (1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Terrain") | 1 << LayerMask.NameToLayer("Airwall") | 1 << LayerMask.NameToLayer("Camera"));
        public static int terrain_mask = LayerMask.NameToLayer("Terrain");

        List<GridData> upperface = new List<GridData>();
        List<GridData> lowerface = new List<GridData>();
        List<List<GridData>> tempData = new List<List<GridData>>();

        //List<GridSegment> segments = new List<GridSegment>();
        List<GridSegment> finalSegments = new List<GridSegment>();

        public GenerateState state;
        public Vector3 offset;

        private Transform root1;
        private Transform root2;
        private Transform root3;
        private bool ignoreRoot;


        public LevelDataGenerator()
        {
            state = GenerateState.NotGenerate;
        }

        public void FindRoot(bool ignoreRoot)
        {
            this.ignoreRoot = ignoreRoot;
            if(!ignoreRoot)
            {
                root1 = GameObject.Find("TempCollider").transform;
                root2 = GameObject.Find("TempUnityTerrain").transform;
                root3 = GameObject.Find("TempMeshTerrain").transform;
            }          
        }

        private LevelBlock FillBlockData(int x, int z)
        {
            LevelBlock blockData = new LevelBlock();
            blockData.x = x;
            blockData.z = z;
            blockData.BlockMin = new Vector3(LevelMapData.MapMin.x + x * LevelMapData.BlockSize, 0, LevelMapData.MapMin.z + z * LevelMapData.BlockSize);
            blockData.BlockMax = new Vector3(LevelMapData.MapMin.x + (x + 1) * LevelMapData.BlockSize, 0, LevelMapData.MapMin.z + (z + 1) * LevelMapData.BlockSize);

            Vector3 blockBase = new Vector3(LevelMapData.MapMin.x + x * LevelMapData.BlockSize + offset.x,
                height_max,
                LevelMapData.MapMin.z + z * LevelMapData.BlockSize + offset.z);

            float gs = LevelMapData.GridSize;
            for (int _z = 0; _z < LevelMapData.GridPerBlock; ++_z)
            {
                for (int _x = 0; _x < LevelMapData.GridPerBlock; ++_x)
                {
                    if (_z + LevelMapData.GridPerBlock * z < LevelMapData.GridRowCount && _x + LevelMapData.GridPerBlock * x < LevelMapData.GridColCount)
                    {
                        Vector3 grid_center = blockBase + new Vector3(_x * gs + gs / 2, 0, _z * gs + gs / 2);

                        SingleGridRaycast(grid_center, blockData.m_Grids);
                    }
                }
            }

            return blockData;
        }

        public void SingleGridRaycast(Vector3 grid_center, List<QuadTreeElement> result)
        {
            List<QuadTreeElement> SingleGridResult = new List<QuadTreeElement>();
            float gs = LevelMapData.GridSize;

            Vector3[] corners = new Vector3[5];

            corners[0] = grid_center;
            corners[1] = grid_center + new Vector3(-gs / 2, 0, -gs / 2);
            corners[2] = grid_center + new Vector3(gs / 2, 0, -gs / 2);
            corners[3] = grid_center + new Vector3(-gs / 2, 0, gs / 2);
            corners[4] = grid_center + new Vector3(gs / 2, 0, gs / 2);

            float terrainHeight = -200.0f;
            RaycastHit hitInfo;
            Transform target = null;
            bool bHit = false;
            tempData.Clear();

            for (int i = 0; i < 5; ++i)
            {
                tempData.Add(new List<GridData>());
                Vector3 rayStartPos = new Vector3(corners[i].x, height_max, corners[i].z);
                bHit = Physics.Raycast(rayStartPos, Vector3.down, out hitInfo, height_max + 100, layer_mask);

                upperface.Clear();
                while (bHit)
                {
                    float firstHit = hitInfo.point.y;
                    target = hitInfo.transform;
                    if(ignoreRoot||target.IsChildOf(root1)||target.IsChildOf(root2)||target.IsChildOf(root3))
                    {
                        GridData o = new GridData(firstHit, GridType.UpperSurface, hitInfo.collider.gameObject.name);
                        o.gradientHeight = firstHit;

                        Terrain t = hitInfo.collider.gameObject.GetComponent<Terrain>();

                        o.IsTerrain = (t != null);
                        if (t != null) terrainHeight = firstHit;
                        o.IsAirWall = (hitInfo.collider.gameObject.tag == "AirWall") || (hitInfo.collider.gameObject.tag == "AirWallNotCamera");
                        o.IsGridAirWall = (hitInfo.collider.gameObject.tag == "AirWallGrid");
                        o.IsAimThrowGrid = (hitInfo.collider.gameObject.tag == "AimThrowGrid");
                        o.IsMonsterGrid = hitInfo.collider.gameObject.tag == "MonsterGrid";

                        float baseHeight = firstHit;
                        //LevelGradientHelper.GetGradientForGrid(hitInfo, ref o.alpha, ref o.beta, ref baseHeight);
                        o.gradientHeight = baseHeight;

                        upperface.Add(o);
                    }
                    rayStartPos = new Vector3(corners[i].x, firstHit - 0.001f, corners[i].z);

                    bHit = Physics.Raycast(rayStartPos, Vector3.down, out hitInfo, height_max, layer_mask);
                }

                lowerface.Clear();
                rayStartPos = new Vector3(corners[i].x, -100, corners[i].z);
                bHit = Physics.Raycast(rayStartPos, Vector3.up, out hitInfo, height_max + 100, layer_mask);

                while (bHit)
                {
                    float firstHit = hitInfo.point.y;
                    target = hitInfo.transform;
                    if(ignoreRoot||target.IsChildOf(root1) || target.IsChildOf(root2) || target.IsChildOf(root3))
                    {
                        GridData o = new GridData(firstHit, GridType.LowerSurface, hitInfo.collider.gameObject.name);
                        o.gradientHeight = firstHit;
                        o.IsGridAirWall = (hitInfo.collider.gameObject.tag == "AirWallGrid");
                        o.IsAimThrowGrid = (hitInfo.collider.gameObject.tag == "AimThrowGrid");
                        o.IsMonsterGrid = hitInfo.collider.gameObject.tag == "MonsterGrid";

                        Terrain t = hitInfo.collider.gameObject.GetComponent<Terrain>();
                        if (t == null)
                            lowerface.Add(o);
                    }

                    rayStartPos = new Vector3(corners[i].x, firstHit + 0.001f, corners[i].z);
                    bHit = Physics.Raycast(rayStartPos, Vector3.up, out hitInfo, height_max + 100, layer_mask);
                }

                tempData[i].Clear();
                if (upperface.Count > 0)
                    CalculateGrid(upperface, lowerface, tempData[i], terrainHeight);
            }

            //LevelGradientHelper.ReduceGrid(grid_center, tempData);

            for (int i = 0; i < tempData[0].Count; ++i)
            {
                QuadTreeElement grid = new QuadTreeElement();
                grid.pos = new Vector3(grid_center.x-offset.x, tempData[0][i].height-offset.y, grid_center.z-offset.z);

                 if (!m_data.IsAudioSurface)
                {
                    byte flag =  0;
                    if (tempData[0][i].type == GridType.LowerSurface) flag |= 0x01;
                    if (tempData[0][i].type == GridType.AimThrowSurface) flag |= 0x02;
                    if (tempData[0][i].type == GridType.MonsterSurface) flag |= 0x08;
                    grid.info = flag;
                }
                
                grid.alpha = tempData[0][i].alpha;
                grid.beta = tempData[0][i].beta;
                SingleGridResult.Add(grid);
            }

            //LevelGradientHelper.ExtendGrid(grid_center, SingleGridResult, tempData);

            if (result != null)
            {
                for (int i = 0; i < SingleGridResult.Count; ++i)
                    result.Add(SingleGridResult[i]);
            }
                
        }

        

        private void CalculateGrid(List<GridData> upper, List<GridData> lower, List<GridData> output, float terrainHeight)
        {
            output.AddRange(upper);
            output.AddRange(lower);
            output.Sort();
            finalSegments.Clear();

            int count = 0;
            GridData cachedStart = null;

            for (int i = 0; i < output.Count; ++i)
            {
                if (output[i].IsAimThrowGrid||output[i].IsMonsterGrid) continue;

                if (output[i].type == GridType.UpperSurface)
                {
                    count++;
                    if (count == 1)
                    {
                        cachedStart = output[i];
                    }

                    if (output[i].IsTerrain)
                    {
                        GridData l = new GridData(-height_max, GridType.LowerSurface, "Terrain");
                        output.Add(l);
                    }
                }
                else if (output[i].type == GridType.LowerSurface)
                {
                    count--;
                    if (count == 0)
                    {
                        GridSegment mp = new GridSegment();
                        mp.upper = cachedStart;
                        mp.upperfaceIsAirWall = cachedStart.IsAirWall;
                        mp.lowerfaceisGridAirWall = output[i].IsGridAirWall;
                        mp.lower = output[i];
                        finalSegments.Add(mp);
                        cachedStart = null;
                    }
                    else if (count < 0)
                    {
                        count = 0;
                    }
                }
            }

            if (count > 0 && cachedStart != null)
            {
                GridSegment mp = new GridSegment();
                mp.upper = cachedStart;
                mp.upperfaceIsAirWall = cachedStart.IsAirWall;
                mp.lower = new GridData(-height_max, GridType.LowerSurface, "FakeLower");
                finalSegments.Add(mp);
                cachedStart = null;
            }

            output.Clear();

            for (int i = 0; i < finalSegments.Count; ++i)
            {
                if (!finalSegments[i].upperfaceIsAirWall)
                {
                    GridData gd = new GridData(finalSegments[i].upper.gradientHeight, 
                        GridType.UpperSurface, finalSegments[i].upper.GOname);
                    gd.alpha = finalSegments[i].upper.alpha;
                    gd.beta = finalSegments[i].upper.beta;
                    output.Add(gd);
                }
                else
                {
                    float maxHeight= finalSegments[i].upper.gradientHeight, minHeight= finalSegments[i].lower.gradientHeight;
                    upper.RemoveAll((o) => 
                    ( o.IsMonsterGrid && (Mathf.Max(terrainHeight, o.height) < maxHeight) && (Mathf.Max(terrainHeight, o.height) > minHeight)));
                }

                if (finalSegments[i].lowerfaceisGridAirWall)
                {
                    GridData gd = new GridData(finalSegments[i].lower.gradientHeight, GridType.LowerSurface, finalSegments[i].lower.GOname);
                    output.Add(gd);
                }
            }

            bool bNeedResort = false;
            for (int i = 0; i < upper.Count; ++i)
            {
                if (upper[i].IsAimThrowGrid)
                {
                    float h = Math.Max(terrainHeight, upper[i].height);
                    output.Add(new GridData(h, GridType.AimThrowSurface, upper[i].GOname));
                    bNeedResort = true;
                }
                if (upper[i].IsMonsterGrid)
                {
                    float h = Math.Max(terrainHeight, upper[i].height);
                    output.Add(new GridData(h, GridType.MonsterSurface, upper[i].GOname));
                    bNeedResort = true;
                }
            }

            if (bNeedResort)
                output.Sort();
        }

        public bool IsGenerateFinish()
        {
            return state == GenerateState.GenerateFinish || state == GenerateState.LoadFinish;
        }

#if UNITY_EDITOR
        public bool GeneratorData(LevelMapData data, bool isScNull, int widthCount, int heightCount)
        {
            m_data = data;
            if (Prepare(isScNull, widthCount, heightCount))
            {
                state = GenerateState.InGenerateProcessing;
                generateIndex = 0;
            }

            return true;
        }
        private bool Prepare(bool isScNull, int widthCount, int heightCount)
        {
            if (!isScNull)
            {
                LevelMapData.MapMin = Vector3.zero;
                LevelMapData.MapMax = new Vector3(widthCount * EngineContext.ChunkSize, 0, heightCount * EngineContext.ChunkSize);

                if (widthCount == 0 || heightCount == 0)
                {
                    EditorUtility.DisplayDialog("Data generate error", "SceneConfig error: sc.widthCount == 0 || sc.heightCount == 0", "OK");
                    return false;
                }
            }
            else
            {
                Terrain[] maps = Terrain.activeTerrains;

                if (maps.Length == 0)
                {
                    EditorUtility.DisplayDialog("Data generate error", " No Terrain Found!", "OK");
                    return false;
                }

                if (maps.Length > 1)
                {
                    EditorUtility.DisplayDialog("Data generate error", " Please Make Terrain Combine First", "OK");
                    return false;
                }

                LevelMapData.MapMin = maps[0].gameObject.transform.position;
                LevelMapData.MapMax = maps[0].gameObject.transform.position + maps[0].terrainData.size;
            }

            LevelMapData.GridRowCount = LevelMapData.DividGridSize((LevelMapData.MapMax.z - LevelMapData.MapMin.z));
            LevelMapData.GridColCount = LevelMapData.DividGridSize((LevelMapData.MapMax.x - LevelMapData.MapMin.x));

            LevelMapData.BlockRowCount = (LevelMapData.GridRowCount + LevelMapData.GridPerBlock - 1) / LevelMapData.GridPerBlock;
            LevelMapData.BlockColCount = (LevelMapData.GridColCount + LevelMapData.GridPerBlock - 1) / LevelMapData.GridPerBlock;

            QuadTreeElement.gridSize = LevelMapData.GridSize;
            QuadTreeElement.gridcolcount = LevelMapData.GridColCount;

            return true;
        }

        public void OnUpdate()
        {
            if (state == GenerateState.InGenerateProcessing)
            {
                int z = generateIndex / LevelMapData.BlockColCount;
                int x = generateIndex - z * LevelMapData.BlockColCount;
                
                if((MapGridType)LevelMapData.MapType!=MapGridType.SeaBlock)
                {
                    LevelBlock blockData = FillBlockData(x, z);
                    m_data.m_Blocks.Add(blockData);
                }               

                generateIndex++;

                EditorUtility.DisplayProgressBar("生成地图数据中", z.ToString() + "_" + x.ToString(), (float)generateIndex / (LevelMapData.BlockRowCount * LevelMapData.BlockColCount));

                if(generateIndex == LevelMapData.BlockRowCount * LevelMapData.BlockColCount)
                {
                    state = GenerateState.GenerateFinish;
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        // 只是生成完成的那一刻返回true
        public bool IsGenerateJustFinish()
        {
            if (generateIndex == LevelMapData.BlockRowCount * LevelMapData.BlockColCount)
            {
                generateIndex = -1;
                return true;
            }

            return false;
        }
#endif
    }
}

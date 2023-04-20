using System;
using System.Collections.Generic;
using UnityEngine;
using CFUtilPoolLib;

namespace XLevel
{
    struct GridLineDesc
    {
        public int dx;
        public int dy;

        public GridLineDesc(int x, int y)
        {
            dx = x;
            dy = y;
        }
    }

    public class LevelNaviMapGenerator
    {
        private List<NaviPoint> m_NaviPoints;
        private LevelMapData m_Data;

        public float[,] ConnectedGraph = null;
        public int[,] PathMatrix = null;

        public LevelNaviMapGenerator(LevelMapData data)
        {
            m_Data = data;
        }

        public void Reset()
        {
            m_NaviPoints = null;
            ConnectedGraph = null;
            PathMatrix = null;
        }

        public bool PathMatrixReady()
        {
            return PathMatrix != null;
        }

        public void SetNaviPoints(List<NaviPoint> allPoints)
        {
            m_NaviPoints = allPoints;

            ConnectedGraph = new float[allPoints.Count, allPoints.Count];

            PathMatrix = new int[allPoints.Count, allPoints.Count];
        }

        public void GenerateNaviMap()
        {
            //LevelDisplayWalkableMode.m_debugGrid.Clear();

            for (int i = 0; i < m_NaviPoints.Count; ++i)
                for(int j = 0; j < m_NaviPoints.Count; ++j)
                {
                    if(i != j)
                    {
                        float distance = Vector3.Distance(m_NaviPoints[i].Pos, m_NaviPoints[j].Pos);
                        if (distance < 50)
                        {
                            if (GetConnection(m_NaviPoints[i], m_NaviPoints[j]))
                                ConnectedGraph[i, j] = distance;
                            else
                                ConnectedGraph[i, j] = float.MaxValue;
                        }
                        else
                        {
                            ConnectedGraph[i, j] = float.MaxValue;
                        }
                    }
                }

            for(int i = 0; i < m_NaviPoints.Count; ++i)
            {
                if(m_NaviPoints[i].linkedIndex > -1 && m_NaviPoints[i].linkedIndex < m_NaviPoints.Count)
                {
                    ConnectedGraph[m_NaviPoints[i].Index, m_NaviPoints[i].linkedIndex] = 0.001f;
                    ConnectedGraph[m_NaviPoints[i].linkedIndex, m_NaviPoints[i].Index] = 0.001f;
                }
            }
        }

        private bool GetConnection(NaviPoint np1, NaviPoint np2)
        {
            float x0 = np1.Pos.x / LevelMapData.GridSize;
            float y0 = np1.Pos.z / LevelMapData.GridSize;

            float x1 = np2.Pos.x / LevelMapData.GridSize;
            float y1 = np2.Pos.z / LevelMapData.GridSize;

            List<GridLineDesc> line;
            NaviPoint start;

            line = Bresenham.GetLine(x0, y0, x1, y1);
            start = np1;

            QuadTreeElement element = m_Data.QueryGrid(start.Pos);
            //LevelDisplayWalkableMode.m_debugGrid.Add(element);

            QuadTreeElement q = new QuadTreeElement();
            q.pos = element.pos;

            bool bConnected = true;
            for (int i = 0; i < line.Count; ++i)
            {
                q.pos = new Vector3(q.pos.x + line[i].dx * LevelMapData.GridSize, q.pos.y, q.pos.z + line[i].dy * LevelMapData.GridSize);

                element = m_Data.QueryGrid(q.pos);

                if (element != null && element.pos.y - q.pos.y < 2.0f)
                {
                    //LevelDisplayWalkableMode.m_debugGrid.Add(element);
                    // compare height 
                    q.pos = element.pos;
                }
                else
                {
                    bConnected = false;
                    break;
                }

                if(i == line.Count - 1 && Math.Abs(element.pos.y - np2.Pos.y) > 1.0f)
                {
                    bConnected = false;
                }
            }

           

            if (!bConnected) return false;

            
            return true;
        }

        public void GeneratePathMatrix()
        {
            int n = m_NaviPoints.Count;

            float[,] d = new float[n, n];

            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    d[i, j] = ConnectedGraph[i, j];
                    PathMatrix[i, j] = j;
                    if (i == j) PathMatrix[i, j] = -1;
                }
            }

            // floyd
            for (int k = 0; k < n; ++k)
                for (int i = 0; i < n; ++i)
                    for (int j = 0; j < n; ++j)
                    {
                        if (d[i, j] > d[i, k] + d[k, j])
                        {
                            d[i, j] = d[i, k] + d[k, j];
                            PathMatrix[i, j] = PathMatrix[i, k];
                        }
                    }
        }

        public List<int> GetNearestPath(Vector3 start, Vector3 end)
        {
            float min1 = float.MaxValue;
            float min2 = float.MaxValue;

            if (m_NaviPoints == null || PathMatrix == null) return null;

            int startIndex = -1;
            int endIndex = -1;

            for(int i = 0; i < m_NaviPoints.Count; ++i)
            {
                float distance1 = Vector3.Distance(m_NaviPoints[i].Pos, start);
                float distance2 = Vector3.Distance(m_NaviPoints[i].Pos, end);

                if(distance1 < min1)
                {
                    min1 = distance1;
                    startIndex = (int)m_NaviPoints[i].Index;
                }

                if(distance2 < min2)
                {
                    min2 = distance2;
                    endIndex = (int)m_NaviPoints[i].Index;
                }
            }

            if(startIndex >= 0 && endIndex >= 0)
            {
                List<int> ret = new List<int>();
                ret.Add(startIndex);

                int node = PathMatrix[startIndex, endIndex];
                while(node != -1)
                {
                    ret.Add(node);
                    node = PathMatrix[node, endIndex];
                }

                return ret;
            }

            return null;
        }


    }
}

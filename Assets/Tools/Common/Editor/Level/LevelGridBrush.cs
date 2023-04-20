using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CFUtilPoolLib;
using XLevel;


namespace XEditor.Level
{
    class LevelGridBrush
    {
        public delegate void BrushUpdateEvent(QuadTreeElement grid);
        private Projector m_Projector;

        private LevelMapData m_Data;

        private int m_Radius;
        private Vector3 m_PreBrushPos = Vector3.zero;
        private List<Vector3> brushPoint = new List<Vector3>();

        public int Radius 
        {
            get { return m_Radius;  }
            set
            {
                m_Radius = value;
                if (m_Projector != null)
                    m_Projector.orthographicSize = m_Radius;
            }
        }
        
        public LevelGridBrush()
        {
            LevelGridTool.ClearUnusedRoot("BrushProjector");

            GameObject o = AssetDatabase.LoadAssetAtPath(LevelGridTool.ResPath + "BrushProjector.prefab", typeof(GameObject)) as GameObject;
            if (o != null)
            {
                GameObject go = GameObject.Instantiate(o) as GameObject;
                go.name = "BrushProjector";
                go.transform.position = new Vector3(-100, 0, -100);
                m_Projector = go.GetComponent<Projector>();
                m_Projector.enabled = true;

                Radius = 2;
            }
        }

        public void SetData(LevelMapData data)
        {
            m_Data = data;
        }

        public void Reset()
        {
            m_Projector.transform.position = new Vector3(-100, 0, -100);
        }

        public void Clear()
        {
            if(m_Projector != null)
            {
                GameObject.DestroyImmediate(m_Projector.gameObject);
                m_Projector = null;
            }
        }

        public void SetViewPos(Vector3 pos)
        {
            m_PreBrushPos = Vector3.zero;
            m_Projector.transform.position = new Vector3(pos.x, 500, pos.z);
        }

        public void OnBrush(Vector3 pos, BrushUpdateEvent udpateEvent)
        {
            brushPoint.Clear();
            LerpBrushPoint(m_PreBrushPos, pos, brushPoint);

            for(int i = 0; i < brushPoint.Count; ++i)
            {
                LevelBlock selectBlock = m_Data.GetBlockByCoord(brushPoint[i]);
                if (selectBlock != null)
                {
                    QuadTreeElement selectGrid = selectBlock.GetGridByCoord(brushPoint[i]);
                    if (selectGrid != null)
                    {
                        udpateEvent(selectGrid);
                    }
                }
            }

            m_Projector.transform.position = new Vector3(pos.x, 500, pos.z);
            m_PreBrushPos = pos;
            
        }

        private void LerpBrushPoint(Vector3 prePoint, Vector3 curPoint, List<Vector3> output)
        {
            if(prePoint != Vector3.zero)
            {
                float length = Vector3.Magnitude(curPoint - prePoint);

                float cosTheta = (curPoint.x - prePoint.x) / length;
                float sinTheta = (curPoint.y - prePoint.y) / length;

                int lerpCount = (int)(length * 2 / Radius);

                if (prePoint.x == curPoint.x)
                {
                    for (int i = 1; i < lerpCount; ++i)
                    {
                        output.Add(prePoint + new Vector3(curPoint.x, 0, prePoint.z + i * Radius / 2));
                    }
                }
                else
                {
                    float k = (curPoint.z - prePoint.z) / (curPoint.x - prePoint.x);

                    for (int i = 1; i < lerpCount; ++i)
                    {
                        output.Add(prePoint + new Vector3(cosTheta * i * Radius / 2, 0, sinTheta * i * Radius / 2));
                    }
                }
            }
            
            output.Add(curPoint);
        }

    }
}

/********************************************************************
	created:	2022年6月20日 19:22:41
	file base:	StreamerManager.cs
	author:		c a o   f e n g
	
	purpose:	主要用来管理场景的资源 生成配置 作为策划配置世界的数据来源 
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CFEngine.WorldStreamer
{

    public enum EnumSteamerActionType
    {
        None = 0,
        Loading = 1,
        Unloading = 2,
        Hidden = 3,
    }

    public enum EnumStreamerLODType
    {
        None = 0,
        LOD0 = 1,
        LOD1 = 2,
        LOD2 = 3,
        All = 4,
    }

    public class StreamerManager :MonoBehaviour
    {

        public StreamerData[] m_StreamerData;

        public StreamerLoader[] GetLoadersByData(Transform parent)
        {
            StreamerLoader[] loaders = new StreamerLoader[m_StreamerData.Length];
            for (int i = 0; i < m_StreamerData.Length; i++)
            {
                loaders[i] = new StreamerLoader(m_StreamerData[i], parent);
            }

            return loaders;
        }
        


        public void RefreshData()
        {
            if (m_StreamerData == null) return;

            Array.Sort(m_StreamerData, delegate (StreamerData entry1, StreamerData entry2)
            {
                if (entry1.m_EnumSteamerLODType > entry2.m_EnumSteamerLODType) return 1;
                if (entry1.m_EnumSteamerLODType < entry2.m_EnumSteamerLODType) return -1;

                return 0;
            });

            for (int i = 0; i < m_StreamerData.Length; i++)
            {
                m_StreamerData[i].RefreshData();
            }

            // BoundingSphere bs = GetBoundingSphere();
            // transform.position = bs.position;
        }


#if UNITY_EDITOR

        public bool bIsDebug=false;
        public Color[] gizmosCeilColors=new Color[3]{Color.blue, Color.green, Color.red};
        void OnDrawGizmos()
        {
            if(!bIsDebug) return;
            
            Color lastColor = Gizmos.color;
            
            GUIStyle textStyle = new GUIStyle(EditorStyles.textField);

            textStyle.contentOffset = new Vector2(1.4f, 1.4f);

            for (int i = 0; i < m_StreamerData.Length; i++)
            {
                StreamerData sp = m_StreamerData[i];

                int nIndex = (int) sp.m_EnumSteamerLODType;
                Color color1 = nIndex <= gizmosCeilColors.Length ? gizmosCeilColors[nIndex - 1] : Color.black;
                Gizmos.color = color1;
                textStyle.normal.textColor = color1;

                Bounds bound = sp.m_ShowBounds;
                Gizmos.DrawWireCube(bound.center, bound.size);
                bound.size += sp.m_LoadingRange;
                Gizmos.DrawWireCube(bound.center, bound.size);
                
                Handles.Label(bound.min, sp.m_EnumSteamerLODType.ToString(),
                    textStyle);


            }

            Gizmos.color = lastColor;
        }
#endif

    }
}
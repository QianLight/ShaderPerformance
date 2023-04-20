using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine.WorldStreamer
{

    public class LightmapManager
    {
        private static LightmapManager Instance { get;  set; }

        public static void Init()
        {
            Instance = new LightmapManager();
        }

        public static void Destory()
        {
            Instance = null;
        }
        

        public static void RegisterLightmapVolumn(LightmapVolumn lv)
        {
            if (Instance == null) return;
            if (Instance.m_AllVolumns.Contains(lv)) return;
            Instance.m_AllVolumns.Add(lv);
        }


        public static void UnRegisterLightmapVolumn(LightmapVolumn lv)
        {
            if (Instance == null) return;
            if (!Instance.m_AllVolumns.Contains(lv)) return;
            Instance.m_AllVolumns.Remove(lv);
        }

        private List<LightmapVolumn> m_AllVolumns = new List<LightmapVolumn>();

        public static void RefreshAllVolumns()
        {
            if (Instance == null) return;
            
            LightmapVolumn.RenderLightmaps(Instance.m_AllVolumns.ToArray());
        }

        public static void RefreshAllVolumnsInEditor()
        {
            LightmapVolumn[] volumnsAll = GameObject.FindObjectsOfType<LightmapVolumn>();
            LightmapVolumn.RenderLightmaps(volumnsAll);
        }
    }
}

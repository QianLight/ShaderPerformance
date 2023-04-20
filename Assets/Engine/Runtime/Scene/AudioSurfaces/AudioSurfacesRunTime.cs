using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CFClient;

namespace CFEngine
{
    public class AudioSurfacesRunTime : MonoBehaviour
    {
        public float UnitMeter = 0.2f;

        private int nWidth = 20000000;

        public List<RunTimeData> allRunTimeData = new List<RunTimeData>();

        private Dictionary<int, EnumAudioSurfacesDefines> allRunTimeDataDic = new Dictionary<int, EnumAudioSurfacesDefines>();

        private Dictionary<EnumAudioSurfacesDefines, string> allTypeStrDic = new Dictionary<EnumAudioSurfacesDefines, string>();

        public float layer1 = 1;
        public float layer2 = 1;
        public float layer3 = 1;
        public float layer4 = 1;

        public float useOriBlend = 0;
        
        public void Awake()
        {
            layer_mask = (1 << LayerMask.NameToLayer("Terrain") | 1 << LayerMask.NameToLayer("BigGuy") |
                          1 << LayerMask.NameToLayer("Default"));
            //allRunTimeDataDic = new Dictionary<int, EnumAudioSurfacesDefines>(allRunTimeData.Count);
            allRunTimeDataDic.Clear();

            for (int i = 0; i < allRunTimeData.Count; i++)
            {
                RunTimeData itm = allRunTimeData[i];

                allRunTimeDataDic.Add(itm.nId, (EnumAudioSurfacesDefines) itm.type);
            }


            //allTypeStrDic = new Dictionary<EnumAudioSurfacesDefines, string>();
            allTypeStrDic.Clear();
            foreach (int myCode in Enum.GetValues(typeof(EnumAudioSurfacesDefines)))
            {
                string strName = Enum.GetName(typeof(EnumAudioSurfacesDefines), myCode); //获取名称
                EnumAudioSurfacesDefines type = (EnumAudioSurfacesDefines) myCode;
                if (type == EnumAudioSurfacesDefines.rock
                    || type == EnumAudioSurfacesDefines.ice
                    || type == EnumAudioSurfacesDefines.lava
                    || type == EnumAudioSurfacesDefines.cloud)
                {
                    allTypeStrDic.Add(type, string.Empty);
                }
                else
                {
                    allTypeStrDic.Add(type, "Footstep/" + strName.ToLower());
                }
            }
        }



        [Serializable]
        public class RunTimeData
        {
            // public int nX;
            //public int nZ;
            public int nId;
            public byte type;
        }

        public int GetPosToID(Vector3 vPos)
        {
            int nX = (int) (vPos.x / UnitMeter);
            int nZ = (int) (vPos.z / UnitMeter);

            return GetXZTOID(nX, nZ);
        }

        public int GetXZTOID(int nX, int nZ)
        {
            return nX * nWidth + nZ;
        }


        public bool isDebug;

        void Update()
        {
#if UNITY_EDITOR
            if (isDebug) DebugPos();
#endif

            EngineContext engineContext = EngineContext.instance;
            if (engineContext == null || engineContext.mainRole == null) return;
            UpdateSurfaceType(engineContext.mainRole.Position);
        }


        private int layer_mask;

        private void DebugPos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;
            bool bHit = Physics.Raycast(ray, out hitInfo, 10000.0f, layer_mask);

            if (!bHit) return;

            if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                UpdateSurfaceType(hitInfo.point);

                Debug.Log(pointSurfaceType, hitInfo.transform.gameObject);
            }
        }


        private Vector3 lastPos = new Vector3(-10000, 0, 0);

        public EnumAudioSurfacesDefines pointSurfaceType;

        public EnumAudioSurfacesDefines pointSurfaceTypeLast;

        /// <summary>
        /// 更新地表音效类型
        /// </summary>
        /// <param name="vPos"></param>
        private void UpdateSurfaceType(Vector3 vPos)
        {
            if (Mathf.Abs(vPos.x - lastPos.x) < UnitMeter && Mathf.Abs(vPos.z - lastPos.z) < UnitMeter) return;

            lastPos = vPos;
            
            pointSurfaceType =GetType(vPos);

            if (pointSurfaceTypeLast == pointSurfaceType) return;
            pointSurfaceTypeLast = pointSurfaceType;

            if (allTypeStrDic.ContainsKey(pointSurfaceType))
            {
                XAudioMgr.singleton.SetSurfacesType(allTypeStrDic[pointSurfaceType]);
            }
        }

        public EnumAudioSurfacesDefines GetType(Vector3 vPos)
        {
            int nId = GetPosToID(vPos);
            if (!allRunTimeDataDic.ContainsKey(nId)) return EnumAudioSurfacesDefines.Default;

            return allRunTimeDataDic[nId];
        }

    }


}
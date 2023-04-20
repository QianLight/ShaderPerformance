using System.Collections;
using System.Collections.Generic;
using CFClient;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using CFEngine;

namespace CFEngine.WorldStreamer
{
    public class OpenWorld : MonoBehaviour
    {
        [Header("世界偏移")] public Vector2 worldOffset;
        [Header("宽高距离")] public Vector2 m_WorldWidthHeight = new Vector2(6000, 8000);
        [Header("宽高格子数")] public Vector2Int m_CeilWidthHeight = new Vector2Int(6, 8);

        [HideInInspector]
        public float m_CeliWidthLength;
        [HideInInspector]
        public float m_CeliHeightLength;

        [Header("所有场景数据")] public StreamerPrefab[] m_StreamerPrefabs;


        public  EnumStreamerLODType m_EnumSteamerLODType= EnumStreamerLODType.LOD0;

        public CullingStreamerByCullingGroup m_CullingStreamerByCullingGroup;

       // private bool m_IsGame = false;
        
        public static OpenWorld Instance { get; private set; }
        
        void Awake()
        {
            Instance = this;
            
            EngineUtility.RegisterFollowPointCallBack(SetGameTarget);
            
#if UNITY_EDITOR
            StreamerLoader.IsEditorMonitor = true;
            if (GameObject.FindObjectOfType<XScript>() != null)
            {
                StreamerLoader.IsEditorMonitor = false;
            }
            else
            {
                m_TargetTf = Camera.main.transform;
                InitByCamera();
                for (int i = 0; i < m_StreamerPrefabs.Length; i++)
                {
                    m_StreamerPrefabs[i].Show(false);
                }

                EnvironmentExtra ee = GameObject.FindObjectOfType<CFEngine.EnvironmentExtra>();
                if (ee != null)
                    ee.enabled = false;

                Camera.main.gameObject.AddComponent<FreeCameraBehaviour>();
                
                SceneViewCameraFollower svcf = Camera.main.gameObject.GetComponent<SceneViewCameraFollower>();
                if (svcf != null)
                {
                    svcf.on= false;
                }

            }
#endif
            
        }

        private void SetGameTarget(Transform tf)
        {
            m_TargetTf = tf;
            InitByCamera(true);
        }

        public void InitByCamera(bool isGame = false)
        {
            if (ReferenceEquals(m_TargetTf, null)) return;

            if (ReferenceEquals( Camera.main, null)) return;


            if (isGame)
                LightmapManager.Init();

            InitConfig();

            m_CullingStreamerByCullingGroup = new CullingStreamerByCullingGroup();
            
            m_CullingStreamerByCullingGroup.InitCullingData(m_StreamerPrefabs);
            
        }

        public void InitConfig()
        {
            m_CeliWidthLength = m_WorldWidthHeight.x / m_CeilWidthHeight.x;
            m_CeliHeightLength = m_WorldWidthHeight.y / m_CeilWidthHeight.y;

            if(m_StreamerPrefabs==null) return;
            
            for (int i = 0; i < m_StreamerPrefabs.Length; i++)
            {
                m_StreamerPrefabTmp = m_StreamerPrefabs[i];
                m_StreamerPrefabTmp.InitData();
            }
        }

        

        public Transform m_TargetTf;

        private StreamerPrefab m_StreamerPrefabTmp;
        void Update()
        {

            if(m_TargetTf==null) return;
            
            Vector3 targetPos = m_TargetTf.position;
            targetPos.y = 0;

            for (int i = 0; i < m_StreamerPrefabs.Length; i++)
            {
                m_StreamerPrefabTmp = m_StreamerPrefabs[i];
                m_StreamerPrefabTmp.UpdateState(targetPos);
            }
        }

        private void OnDestroy()
        {
            m_CullingStreamerByCullingGroup.OnDestory();
            LightmapManager.Destory();
        }

        public void SendAllStreamerAction(EnumSteamerActionType type, EnumStreamerLODType lodType)
        {
            for (int i = 0; i < m_StreamerPrefabs.Length; i++)
            {
                m_StreamerPrefabs[i].SendStreamerAction(type,lodType);
            }
        }


#if UNITY_EDITOR
        public bool bDebug = false;
        public Color gizmosCeilColor=Color.green;
        public bool bDebugArea = false;
        void OnDrawGizmos()
        {
            if (!bDebug) return;

            Color lastColor = Gizmos.color;
            Color lastColor1 = Handles.color;
            Gizmos.color = gizmosCeilColor;
            Handles.color = gizmosCeilColor;

            GUIStyle textStyle = new GUIStyle(EditorStyles.textField);
            textStyle.contentOffset = new Vector2(1.4f, 1.4f);
            textStyle.normal.textColor = gizmosCeilColor;

            for (int x = 0; x < m_CeilWidthHeight.x; x++)
            {
                for (int y = 0; y < m_CeilWidthHeight.y; y++)
                {
                    float xPos = worldOffset.x + x * m_CeliWidthLength;
                    float yPos = worldOffset.y + y * m_CeliHeightLength;

                    Gizmos.DrawWireCube(new Vector3(xPos, 0, yPos),
                       new Vector3(m_CeliWidthLength, 0.1f, m_CeliHeightLength));

                    //Vector3 position = new Vector3(xPos, 0, yPos) + Vector3.up * 2f;
                    //float size = 2f;
                    //float pickSize = size * 2f;

                    //if (Handles.Button(position, Quaternion.identity, size, pickSize, Handles.RectangleHandleCap))
                    //    Debug.Log("The button was pressed!");

                    //if (Handles.Button(new Vector3(xPos, 10, yPos), Quaternion.identity, 100, 100, Handles.SphereHandleCap))
                    //{
                    //    Debug.Log("" + x + "," + y);
                    //}

                    Handles.Label(new Vector3(xPos, 10, yPos), "" + x + "," + y, textStyle);
                }
            }

            if (m_StreamerPrefabs != null)
            {
                for (int i = 0; i < m_StreamerPrefabs.Length; i++)
                {
                    StreamerPrefab sp = m_StreamerPrefabs[i];
                    if(sp==null|| !sp.enabled) continue;
                    
                    sp.InitData();
                    
                    BoundingSphere bs = sp.GetBoundingSphere();

                    Gizmos.DrawWireSphere(bs.position, bs.radius);

                    if(bDebugArea)
                    {
                        for (int j = 0; j < sp.m_StreamerLoader.Length; j++)
                        {
                            StreamerLoader sd = sp.m_StreamerLoader[j];
                            
                            int nIndex = (int) sd.m_StreamerData.m_EnumSteamerLODType;
                            Color color1 = nIndex <= gizmosCeilColors.Length ? gizmosCeilColors[nIndex - 1] : Color.black;
                            Gizmos.color = color1;
                            textStyle.normal.textColor = color1;
                            Gizmos.DrawWireCube(sd.m_AreaBounds.center, sd.m_AreaBounds.size);
                        }
                    }
   
                   
                    Handles.Label(bs.position, sp.ShowName+"_"+sp.transform.name, textStyle);
                }
            }


            Gizmos.color = lastColor;
            Handles.color = lastColor1;
        }
        
        public Color[] gizmosCeilColors=new Color[3]{Color.blue, Color.green, Color.red};
#endif

    }
}
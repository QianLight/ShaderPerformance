using System.Collections;
using System.Collections.Generic;
using System.IO;
using CFUtilPoolLib;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using XLevel;

namespace CFEngine
{
    public class AudioSurfaceRecognition
    {
        private static AudioSurfaceRecognition _Instance;

        public static AudioSurfaceRecognition Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new AudioSurfaceRecognition();
                }

                return _Instance;
            }
        }

        [MenuItem("GameObject/地表类型预览", false, 0)]
        public static void AutoAudioSurfacePreivewImpostor()
        {
            BIsPreivewAudioSurface = true;
            ImpostorHelper.PreivewImpostor();
        }
        
        private static string isPreivewAudioSurface = "isPreivewAudioSurface";

        public static bool BIsPreivewAudioSurface
        {
            get
            {
                if (PlayerPrefs.HasKey(isPreivewAudioSurface) && PlayerPrefs.GetInt(isPreivewAudioSurface) == 1)
                {
                    return true;
                }

                return false;
            }

            set { PlayerPrefs.SetInt(isPreivewAudioSurface, value ? 1 : 0); }
        }


        [InitializeOnLoadMethod]
        static void InitAutoAudioSurface()
        {

            EditorApplication.playModeStateChanged += ChangedPlaymodeState;
        }

        static void ChangedPlaymodeState(PlayModeStateChange obj)
        {

            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:

                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    if (BIsPreivewAudioSurface)
                    {
                        BIsPreivewAudioSurface = false;
                        
                        AudioSurfacesRunTime _AudioSurfacesRunTime =
                            GameObject.FindObjectOfType<AudioSurfacesRunTime>();

                        if (_AudioSurfacesRunTime != null)
                        {
                            _AudioSurfacesRunTime.isDebug = true;
                            Instance.InitMeshCollider();
                            Instance.ClearGridRoot();
                            EnvironmentExtra _EnvironmentExtra = GameObject.FindObjectOfType<EnvironmentExtra>();
                            _EnvironmentExtra.enabled = false;

                        }

                    }

                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private void ClearGridRoot()
        {
            GameObject root = GameObject.Find("GridRoot");
            if (root)
            {
                GameObject.DestroyImmediate(root);
            }
        }

        private List<AudioSurfaces> allAudioSurfaces;

        private Dictionary<GameObject, bool> _EditorScene_EnableDic;

        private void SaveEnableDic()
        {
            _EditorScene_EnableDic = new Dictionary<GameObject, bool>();

            AddDisableObj("Collider");
            AddDisableObj("DynamicObjects");
            AddDisableObj("Enverinment");
            AddDisableObj("MeshTerrain");

            GameObject obj = _EditorScene.transform.Find("StaticPrefabs/MainScene").gameObject;
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                Transform tf = obj.transform.GetChild(i);
                _EditorScene_EnableDic.Add(tf.gameObject, tf.gameObject.activeInHierarchy);
                tf.gameObject.SetActive(true);
            }
        }

        private void AddDisableObj(string name1)
        {
            GameObject obj = _EditorScene.transform.Find(name1).gameObject;
            if (obj == null) return;
            _EditorScene_EnableDic.Add(obj, obj.activeInHierarchy);
            obj.SetActive(false);
        }

        private void RecoverEnableDic()
        {
            foreach (var itm in _EditorScene_EnableDic)
            {
                itm.Key.SetActive(itm.Value);
            }
        }


        /// <summary>
        /// 初始化特图类型
        /// </summary>
        public void InitAllAudioSurfaces()
        {
            allAudioSurfaces = new List<AudioSurfaces>();

            string[] files = Directory.GetFiles("Assets/BundleRes/AudioSurfaces");

            foreach (string str in files)
            {
                if (str.Contains(".meta")) continue;

                AudioSurfaces s = AssetDatabase.LoadAssetAtPath<AudioSurfaces>(str);
                allAudioSurfaces.Add(s);
            }

            GameObject _EditorScene = GameObject.Find("EditorScene");
            _AudioSurfacesRunTime = GameObject.FindObjectOfType<AudioSurfacesRunTime>();
            if (_AudioSurfacesRunTime == null)
            {
                GameObject _AudioSurfaces = new GameObject("AudioSurfaces");
                _AudioSurfaces.transform.SetParent(_EditorScene.transform);
                _AudioSurfacesRunTime = _AudioSurfaces.AddComponent<AudioSurfacesRunTime>();
            }

            InitShader();
        }

        private Texture lastTypeTexture;
        private AudioSurfaces lastAudioSurfaces;

        /// <summary>
        /// 获取图片处类型
        /// </summary>
        /// <param name="tex"></param>
        /// <returns></returns>
        private AudioSurfaces GetTextureType(Texture tex)
        {
            if (tex == null) return null;

            if (lastTypeTexture != null && lastAudioSurfaces != null && tex == lastTypeTexture) //避免多次循环
            {
                return lastAudioSurfaces;
            }

            for (int i = 0; i < allAudioSurfaces.Count; i++)
            {
                AudioSurfaces itm = allAudioSurfaces[i];
                if (itm.CheckHasTex(tex))
                {
                    lastTypeTexture = tex;
                    lastAudioSurfaces = itm;
                    return itm;
                }

            }

            Debug.LogError("GetTextureType 未配置的地表图片类型:" + tex, tex);

            return null;
        }

        private AudioSurfaces GetSurfacesDataType(EnumAudioSurfacesDefines type)
        {
            for (int i = 0; i < allAudioSurfaces.Count; i++)
            {
                AudioSurfaces itm = allAudioSurfaces[i];
                if (itm.sufaceType == type)
                {
                    return itm;
                }
            }

            return null;
        }


        private AudioSurfacesRunTime _AudioSurfacesRunTime;

        public static float UnitMeter
        {
            get { return _Instance._AudioSurfacesRunTime.UnitMeter; }

            set { _Instance._AudioSurfacesRunTime.UnitMeter = value; }
        }

        public static float layer1
        {
            get { return _Instance._AudioSurfacesRunTime.layer1; }

            set { _Instance._AudioSurfacesRunTime.layer1 = value; }
        }
        
        public static float layer2
        {
            get { return _Instance._AudioSurfacesRunTime.layer2; }

            set { _Instance._AudioSurfacesRunTime.layer2 = value; }
        }
        
        public static float layer3
        {
            get { return _Instance._AudioSurfacesRunTime.layer3; }

            set { _Instance._AudioSurfacesRunTime.layer3 = value; }
        }
        
        public static float layer4
        {
            get { return _Instance._AudioSurfacesRunTime.layer4; }

            set { _Instance._AudioSurfacesRunTime.layer4 = value; }
        }

        public static float useOriBlend
        {
            get { return _Instance._AudioSurfacesRunTime.useOriBlend; }

            set { _Instance._AudioSurfacesRunTime.useOriBlend = value; }
        }

        GameObject _EditorScene;

        /// <summary>
        /// 初始化碰撞体
        /// </summary>
        private void InitMeshCollider()
        {
            _EditorScene = GameObject.Find("EditorScene");
            SaveEnableDic();

            tmpData.Clear();

            GameObject _StaticPrefabs = _EditorScene.transform.Find("StaticPrefabs").gameObject;
            AudioSurfaceLayer[] _MeshColliders = _StaticPrefabs.transform.GetComponentsInChildren<AudioSurfaceLayer>();
            for (int i = 0; i < _MeshColliders.Length; i++)
            {
                AudioSurfaceLayer itm = _MeshColliders[i];

                MeshCollider col = itm.gameObject.GetComponent<MeshCollider>();

                TerrainTmpData tmp = new TerrainTmpData();
                if (col == null)
                {
                    col = itm.gameObject.AddComponent<MeshCollider>();
                    col.enabled = false;
                    tmp.isNew = true;
                }

                tmp.collider = col;
                tmp.enable = col.enabled;
                tmp.layer = itm.gameObject.layer;
                tmpData.Add(tmp);

                col.enabled = true;
                itm.gameObject.layer = LayerMask.NameToLayer("Terrain");

            }





        }

        private List<TerrainTmpData> tmpData = new List<TerrainTmpData>();

        public class TerrainTmpData
        {
            public bool isNew;
            public bool enable;
            public int layer;
            public MeshCollider collider;
        }

        /// <summary>
        /// 还原场景之前的状态
        /// </summary>
        public void UnInitAllAudioSurfaces()
        {
            RecoverEnableDic();

            for (int i = 0; i < tmpData.Count; i++)
            {
                TerrainTmpData tmp = tmpData[i];
                tmp.collider.enabled = tmp.enable;
                tmp.collider.gameObject.layer = tmp.layer;
                if (tmp.isNew)
                {
                    GameObject.DestroyImmediate(tmp.collider);
                }
            }

            DeleteAllTextures();
        }

        /// <summary>
        /// 根据所有网格数据进行显示
        /// </summary>
        /// <param name="m_data"></param>
        public void ShowAllLevelMapData(LevelMapData m_data, bool isPreview)
        {

            InitMeshCollider();

            AudioSurfacesDefines.ClearFlagColorDic();

            if (!isPreview)
                _AudioSurfacesRunTime.allRunTimeData.Clear();
            else
            {
                _AudioSurfacesRunTime.Awake();
            }

            DeleteAllTextures();
            allIds.Clear();

            for (int i = 0; i < m_data.m_Blocks.Count; ++i)
            {
                DrawBlock(m_data.m_Blocks[i], isPreview);
            }

            UnInitAllAudioSurfaces();

        }


        public static int layer_mask = (1 << LayerMask.NameToLayer("Terrain") | 1 << LayerMask.NameToLayer("BigGuy") |
                                        1 << LayerMask.NameToLayer("Default"));

        /// <summary>
        /// 绘制某块区域
        /// </summary>
        private List<int> allIds = new List<int>();
        
        private void DrawBlock(LevelBlock block, bool isPreview)
        {
            for (int i = 0; i < block.m_Grids.Count; ++i)
            {
                QuadTreeElement grid = block.m_Grids[i];

                if (!isPreview)
                {
                    Vector3 h1 = grid.pos + new Vector3(0, 10.1f, 0);

                    Ray r = new Ray(h1, Vector3.down);
                    RaycastHit hitInfo;
                    bool bHit = Physics.Raycast(r, out hitInfo, 10000.0f, layer_mask);

                    if (!bHit) continue;

                    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                    {
                        AudioSurfaces typeSur = GetSurfaceType(hitInfo, r);
                        if (typeSur == null) continue;

                        grid.info = AudioSurfacesDefines.GetSurfacesFlag(typeSur);

                        AudioSurfacesRunTime.RunTimeData newItemData = new AudioSurfacesRunTime.RunTimeData();
                        newItemData.type = (byte) typeSur.sufaceType;
                        newItemData.nId = _AudioSurfacesRunTime.GetPosToID(grid.pos);

                        if (!allIds.Contains(newItemData.nId))
                        {
                            allIds.Add(newItemData.nId);
                            _AudioSurfacesRunTime.allRunTimeData.Add(newItemData);
                        }
                        else
                        {
                            int nIndex = allIds.IndexOf(newItemData.nId);
                            grid.info = _AudioSurfacesRunTime.allRunTimeData[nIndex].type;
                            //Debug.LogError("Repeat Id:" + newItemData.nId + "  " + grid.pos);
                        }
                    }
                }
                else
                {
                    AudioSurfaces typeSur = GetSurfacesDataType(_AudioSurfacesRunTime.GetType(grid.pos));
                    if (typeSur)
                        grid.info = AudioSurfacesDefines.GetSurfacesFlag(typeSur);
                }

            }
        }


        private void InitShader()
        {
            strLayerShader = Shader.Find("URP/Scene/Layer");
            strUberShader = Shader.Find("URP/Scene/Uber");
            strWaterShader = Shader.Find("URP/Scene/UniformWater");

            waterAudioSurfaces = new AudioSurfaces();
            waterAudioSurfaces.sufaceType = EnumAudioSurfacesDefines.water;
            waterAudioSurfaces.debugColor = Color.cyan;
        }

        private Shader strLayerShader;
        private Shader strUberShader;
        private Shader strWaterShader;
        private AudioSurfaces waterAudioSurfaces;

        private bool _IsDebug = false;

        public AudioSurfaces GetSurfaceType(RaycastHit hitInfo,Ray ray, bool isDebug = false)
        {
            _IsDebug = isDebug;
            GameObject obj = hitInfo.collider.gameObject;

            if (isDebug)
                Debug.Log("GetSurfaceType:" + obj, obj);

            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (mr == null) return null;
            Material mat = mr.sharedMaterial;
            if (mat == null) return null;

            Texture tex = null;
            if (mat.shader.Equals(strLayerShader))
            {
                tex = MaterialLayer_GetTexture(mat, hitInfo,ray);
            }
            else if (mat.shader.Equals(strUberShader))
            {
                tex = MaterialUber_GetTexture(mat, hitInfo);
            }
            else if (mat.shader.Equals(strWaterShader))
            {
                return waterAudioSurfaces;
            }

            AudioSurfaces texType = GetTextureType(tex);

            if (isDebug)
                Debug.Log(
                    "GetSurfaceType:" + (texType != null ? texType.sufaceType : EnumAudioSurfacesDefines.Default) +
                    "  tex:" + tex + "  point:" + hitInfo.point, tex);

            return texType;

        }


        private Texture MaterialUber_GetTexture(Material mat, RaycastHit hitInfo)
        {
            return mat.mainTexture;
        }

        private float _BlendThreshold;
        private float _Blendscale;

        private Texture MaterialLayer_GetTexture(Material mat, RaycastHit hitInfo, Ray ray)
        {
            int _VCMode = mat.GetInt("_VCMode");

            List<string> shaderKeyWords = new List<string>();
            shaderKeyWords.AddRange(mat.shaderKeywords);

            float[] allAlphas = new float[4];
            allAlphas[0] = 1;

            Texture _MainTexOri = (mat.GetTexture("_MainTex"));
            Texture _MainTexOri1 = (mat.GetTexture("_MainTex1"));
            Texture _MainTexOri2 = (mat.GetTexture("_MainTex2"));
            Texture _MainTexOri3 = (mat.GetTexture("_MainTex3"));

            Texture2D _MainTex = duplicateTexture(_MainTexOri);
            Texture2D _MainTex1 = duplicateTexture(_MainTexOri1);
            Texture2D _MainTex2 = duplicateTexture(_MainTexOri2);
            Texture2D _MainTex3 = duplicateTexture(_MainTexOri3);

            Color _Color0 = mat.GetColor("_Color0");
            Color _Color1 = mat.GetColor("_Color1");
            Color _Color2 = mat.GetColor("_Color2");
            Color _Color3 = mat.GetColor("_Color3");

            Vector4 _TextureIntensity = mat.GetVector("_TextureIntensity");

            _BlendThreshold = mat.GetFloat("_BlendThreshold");
            _Blendscale = mat.GetFloat("_BlendScale");


            Color blend = new Color(1, 1, 1);
            Vector2 pixelUV = hitInfo.textureCoord;

            _Rot01 = mat.GetVector("_Rot01");
            _Rot23 = mat.GetVector("_Rot23");

            _Layer0UVST = mat.GetVector("_Layer0UVST");
            _Layer1UVST = mat.GetVector("_Layer1UVST");
            _Layer2UVST = mat.GetVector("_Layer2UVST");
            _Layer3UVST = mat.GetVector("_Layer3UVST");

            Vector2 uv0 = Vector2.Scale(GetRotUV0(pixelUV), new Vector2(_Layer0UVST.x, _Layer0UVST.y)) +
                          new Vector2(_Layer0UVST.z, _Layer0UVST.w);
            Vector2 uv1 = Vector2.Scale(GetRotUV1(pixelUV), new Vector2(_Layer1UVST.x, _Layer1UVST.y)) +
                          new Vector2(_Layer1UVST.z, _Layer1UVST.w);
            Vector2 uv2 = Vector2.Scale(GetRotUV2(pixelUV), new Vector2(_Layer2UVST.x, _Layer2UVST.y)) +
                          new Vector2(_Layer2UVST.z, _Layer2UVST.w);
            Vector2 uv3 = Vector2.Scale(GetRotUV3(pixelUV), new Vector2(_Layer3UVST.x, _Layer3UVST.y)) +
                          new Vector2(_Layer3UVST.z, _Layer3UVST.w);

            if (_VCMode == 0)
            {
                Texture2D _BlendTex = mat.GetTexture("_BlendTex") as Texture2D;
                Texture2D _BlendTexCopy = duplicateTexture(_BlendTex);
                pixelUV.x *= _BlendTex.width;
                pixelUV.y *= _BlendTex.height;
                blend = _BlendTexCopy.GetPixel(Mathf.FloorToInt(pixelUV.x), Mathf.FloorToInt(pixelUV.y));
            }
            else
            {
                MeshRenderObject mro = hitInfo.collider.gameObject.GetComponent<MeshRenderObject>();
                MeshFilter mf = hitInfo.collider.gameObject.GetComponent<MeshFilter>();
                int[] OriTriangles = mf.sharedMesh.GetTriangles(0);
                MeshRenderer mr = hitInfo.collider.gameObject.GetComponent<MeshRenderer>();
                int nIndex = OriTriangles[hitInfo.triangleIndex * 3];
                int nIndex1 = OriTriangles[hitInfo.triangleIndex * 3 + 1];
                int nIndex2 = OriTriangles[hitInfo.triangleIndex * 3 + 2];

                //int nIndex = GetNearestPoint(hitInfo.point, mro);
                //int nIndex = RayCastMesh(ray, hitInfo.collider.gameObject);

                if (mr.additionalVertexStreams == null)
                {
                    Debug.LogError("additionalVertexStreams null！" + mr, mr);
                    return null;
                }

                Color[] colors = mr.additionalVertexStreams.colors;

                //blend = (colors[nIndex]+colors[nIndex1]+colors[nIndex2])/3;


                Vector3 hitLocalPoint = mro.transform.InverseTransformPoint(hitInfo.point);
                blend = AudioSurfacesDefines.GetColorByBarycentric(hitInfo.triangleIndex, hitLocalPoint, mf.sharedMesh,
                    mr.additionalVertexStreams);

                if (_IsDebug)
                {
                    Vector3[] postions = mf.sharedMesh.vertices;
                    Vector3 postionIndex = postions[nIndex];
                    Debug.Log("postionIndex:" + mro.transform.TransformPoint(postionIndex) + " hitLocalPoint:" +
                              hitLocalPoint + "   nIndex:" + nIndex +
                              "   nIndex1:" + nIndex1 + "   nIndex2:" + nIndex2 +
                              "   blend:" + blend + "  colors0:" + colors[nIndex] + "  colors1:" + colors[nIndex + 1] +
                              "   colors2:" + colors[nIndex + 2]);
                }

            }





            Color color0Tmp = (_MainTex).GetPixelBilinear(uv0.x, (uv0.y));
            float color0 = color0Tmp.a * _Color0.a *
                           _TextureIntensity.x;
            color0 = Mathf.Pow(color0, _Color0.a);

            float color1, color2 = 0, color3 = 0;
            bool b2X = shaderKeyWords.Contains("_SPLAT_2X");
            bool b3X = shaderKeyWords.Contains("_SPLAT_3X");
            bool b4X = shaderKeyWords.Contains("_SPLAT_4X");

            if (b2X || b3X || b4X)
            {
                color1 = _MainTex1.GetPixelBilinear((uv1.x), (uv1.y)).a * _Color1.a *
                         _TextureIntensity.y;
                color1 = Mathf.Pow(color1, _Color1.a);

                Vector4 blendColorAlpha = Vector4.zero;

                if (b2X)
                {
                    blendColorAlpha = BlendColor2(color0, color1, blend);
                }

                if (b3X)
                {
                    color2 = _MainTex2.GetPixelBilinear((uv2.x), (uv2.y)).a *
                             _Color2.a * _TextureIntensity.z;
                    color2 = Mathf.Pow(color2, _Color2.a);

                    blendColorAlpha = BlendColor3(color0, color1, color2, blend);
                }


                if (b4X)
                {
                    color3 = _MainTex3.GetPixelBilinear((uv3.x), (uv3.y)).a *
                             _Color3.a * _TextureIntensity.z;
                    color3 = Mathf.Pow(color3, _Color3.a);

                    blendColorAlpha = BlendColor4(color0, color1, color2, color3, blend);
                }

                allAlphas[0] = (blendColorAlpha[0] + blend[0] * useOriBlend) * layer1;
                allAlphas[1] = (blendColorAlpha[1] + blend[1] * useOriBlend) * layer2;
                allAlphas[2] = (blendColorAlpha[2] + blend[2] * useOriBlend) * layer3;
                allAlphas[3] = blendColorAlpha[3] * layer4;
            }




            int nShowIndex = 0;
            float blendValue = 0;
            List<Texture> shaderAllTexs = new List<Texture>();
            shaderAllTexs.Add(_MainTexOri);
            shaderAllTexs.Add(_MainTexOri1);
            shaderAllTexs.Add(_MainTexOri2);
            shaderAllTexs.Add(_MainTexOri3);

            for (int i = 0; i < allAlphas.Length; i++)
            {
                float f = allAlphas[i];
                if (f > blendValue)
                {
                    blendValue = f;
                    nShowIndex = i;
                }
            }

            if (_IsDebug)
            {
                Debug.Log("nShowIndex:" + nShowIndex + "  blendValue:" + blendValue + "  allAlphas:" +
                          string.Join(",", allAlphas));
            }

            return shaderAllTexs[nShowIndex];
        }

        private Vector4 _Rot01;
        private Vector4 _Rot23;
        private Vector4 _Layer0UVST;
        private Vector4 _Layer1UVST;
        private Vector4 _Layer2UVST;
        private Vector4 _Layer3UVST;
        
        Vector2 GetRotUV0(Vector2 uv)
        {
            return new Vector2(uv.x * _Rot01.x - uv.y * _Rot01.y, uv.x * _Rot01.y + uv.y * _Rot01.x);
        }

        Vector2 GetRotUV1(Vector2 uv)
        {
            return new Vector2(uv.x * _Rot01.z - uv.y * _Rot01.w, uv.x * _Rot01.w + uv.y * _Rot01.z);
        }

        Vector2 GetRotUV2(Vector2 uv)
        {
            return new Vector2(uv.x * _Rot23.x - uv.y * _Rot23.y, uv.x * _Rot23.y + uv.y * _Rot23.x);
        }

        Vector2 GetRotUV3(Vector2 uv)
        {
            return new Vector2(uv.x * _Rot23.z - uv.y * _Rot23.w, uv.x * _Rot23.w + uv.y * _Rot23.z);
        }
        
        
        private int GetNearestPoint(Vector3 pointHit, MeshRenderObject mro)
        {
            Vector3 point = mro.transform.InverseTransformPoint(pointHit);
            //point.y = 0;

            float fMin = float.MaxValue;
            MeshFilter mf = mro.gameObject.GetComponent<MeshFilter>();
            Vector3[] vertices = mf.sharedMesh.vertices;
            int[] triangles = mf.sharedMesh.triangles;
            int a, b, c;
            int nIndex = 0;

            for (int i = 0; i < triangles.Length; i = i + 3)
            {
                a = triangles[i];
                b = triangles[i + 1];
                c = triangles[i + 2];

                float d1 = Vector3.Distance(point, vertices[a]);
                float d2 = Vector3.Distance(point, vertices[b]);
                float d3 = Vector3.Distance(point, vertices[c]);

               float f = (d1 + d2 + d3) / 3;
               
                Debug.Log(i + "  point: " + point + "   vertices:" + vertices[a] + "  f:" + f + "  fMin:" + fMin);

                if (f < fMin)
                {
                    fMin = f;
                    nIndex = a;

                    Debug.Log("mintriangles: " + i);
                }
            }

            return nIndex;
        }


        /// <summary>
        /// Returns the distance to the mesh from the raycast origin. Returns float.NaN if no hit was found.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="objectWithMeshFilter"></param>
        /// <param name="maxDistance"></param>
        /// <returns>Distance as float or float.NaN</returns>
        private static int RayCastMesh(Ray ray, GameObject objectWithMeshFilter, float maxDistance = 1000f)
        {
            int nIndex = 0;
            if (objectWithMeshFilter != null)
            {
                float distance = float.NaN;
                // check all triangles for hit with raycast
                var meshFilter = objectWithMeshFilter.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    int[] triangles = meshFilter.sharedMesh.triangles;
                    Vector3[] vertices = meshFilter.sharedMesh.vertices;
                    int a, b, c;
                    for (int i = 0; i < triangles.Length; i = i + 3)
                    {
                        a = triangles[i];
                        b = triangles[i + 1];
                        c = triangles[i + 2];
                        /*
                        Debug.DrawLine(meshFilter.transform.TransformPoint(vertices[a]), meshFilter.transform.TransformPoint(vertices[b]), Color.red, 3.0f);
                        Debug.DrawLine(meshFilter.transform.TransformPoint(vertices[b]), meshFilter.transform.TransformPoint(vertices[c]), Color.red, 3.0f);
                        Debug.DrawLine(meshFilter.transform.TransformPoint(vertices[c]), meshFilter.transform.TransformPoint(vertices[a]), Color.red, 3.0f); //*/
                        distance = IntersectRayTriangle(
                            ray,
                            meshFilter.transform.TransformPoint(vertices[a]),
                            meshFilter.transform.TransformPoint(vertices[b]),
                            meshFilter.transform.TransformPoint(vertices[c]));
                        nIndex = a;
                        if (!float.IsNaN(distance))
                        {
                            break;
                        }
                    }
                }

                if (float.IsNaN(distance) == false)
                {
                    if (distance < maxDistance)
                    {
                        return nIndex;
                    }
                }
            }

            return nIndex;
        }

        const float kEpsilon = 0.000001f;

        /// <summary>
        /// Thanks to: https://answers.unity.com/questions/861719/a-fast-triangle-triangle-intersection-algorithm-fo.html
        /// Ray-versus-triangle intersection test suitable for ray-tracing etc.
        /// Port of Möller–Trumbore algorithm c++ version from:
        /// https://en.wikipedia.org/wiki/Möller–Trumbore_intersection_algorithm
        /// </summary>
        /// <returns><c>The distance along the ray to the intersection</c> if one exists, <c>NaN</c> if one does not.</returns>
        /// <param name="ray">the ray</param>
        /// <param name="v0">A vertex 0 of the triangle.</param>
        /// <param name="v1">A vertex 1 of the triangle.</param>
        /// <param name="v2">A vertex 2 of the triangle.</param>
        public static float IntersectRayTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            // edges from v1 & v2 to v0.
            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;

            Vector3 h = Vector3.Cross(ray.direction, e2);
            float a = Vector3.Dot(e1, h);
            if ((a > -kEpsilon) && (a < kEpsilon))
            {
                return float.NaN;
            }

            float f = 1.0f / a;

            Vector3 s = ray.origin - v0;
            float u = f * Vector3.Dot(s, h);
            if ((u < 0.0f) || (u > 1.0f))
            {
                return float.NaN;
            }

            Vector3 q = Vector3.Cross(s, e1);
            float v = f * Vector3.Dot(ray.direction, q);
            if ((v < 0.0f) || (u + v > 1.0f))
            {
                return float.NaN;
            }

            float t = f * Vector3.Dot(e2, q);
            if (t > kEpsilon)
            {
                return t;
            }
            else
            {
                return float.NaN;
            }
        }

        Vector3 BlendColor2(float color0, float color1,
            Color blend)
        {
            Vector3 blendFactor = Vector3.zero;
            blendFactor.x = color0 * Mathf.Pow(Mathf.Abs(blend.r), _Blendscale);
            blendFactor.y = color1 * Mathf.Pow(Mathf.Abs(blend.g), _Blendscale);

            float maxValue = Mathf.Max(blendFactor.x, blendFactor.y);

            float transition = Mathf.Max(_BlendThreshold * maxValue, 0.0001f);
            float threshold = maxValue - transition;
            float scale = 1 / (transition);

            Vector3 vDelta = blendFactor - new Vector3(threshold, threshold, threshold);
            vDelta = Vector3Saturate(vDelta);
            blendFactor = Vector3.Scale(vDelta, new Vector3(scale, scale, scale));

            blendFactor = blendFactor * (1 / (blendFactor.x + blendFactor.y));

            blendFactor.z = 0;

            return blendFactor;
        }

        private Vector3 Vector3Saturate(Vector3 v)
        {
            return new Vector3(Mathf.Clamp01(v.x), Mathf.Clamp01(v.y), Mathf.Clamp01(v.z));
        }


        Vector3 BlendColor3(float color0, float color1, float color2,
            Color blend)
        {
            Vector3 blendFactor = Vector3.zero;
            blendFactor.x = color0 * Mathf.Pow(Mathf.Abs(blend.r), _Blendscale);
            blendFactor.y = color1 * Mathf.Pow(Mathf.Abs(blend.g), _Blendscale);
            blendFactor.z = color2 * Mathf.Pow(Mathf.Abs(blend.b), _Blendscale);

            float maxValue = Mathf.Max(blendFactor.x, Mathf.Max(blendFactor.y, blendFactor.z));


            blendFactor = Vector3.Scale(Vector3.Max(blendFactor - new Vector3(maxValue, maxValue, maxValue)
                                                    + new Vector3(_BlendThreshold, _BlendThreshold, _BlendThreshold),
                    Vector3.zero)
                , new Vector3(blend.r, blend.g, blend.b));

            return blendFactor / (blendFactor.x + blendFactor.y + blendFactor.z);
        }


        Vector4 BlendColor4(float color0, float color1, float color2, float color3,
            Color blend)
        {
            Vector4 blendFactor = Vector4.zero;
            blendFactor.x = color0 * Mathf.Pow(Mathf.Abs(blend.r), _Blendscale);
            blendFactor.y = color1 * Mathf.Pow(Mathf.Abs(blend.g), _Blendscale);
            blendFactor.z = color2 * Mathf.Pow(Mathf.Abs(blend.b), _Blendscale);
            blendFactor.w = color2 * Mathf.Pow(Mathf.Abs(blend.a), _Blendscale);

            float maxValue = Mathf.Max(blendFactor.x,
                Mathf.Max(blendFactor.y, Mathf.Max(blendFactor.z, blendFactor.w)));


            blendFactor = Vector4.Scale(Vector4.Max(blendFactor - new Vector4(maxValue, maxValue, maxValue, maxValue)
                                                    + new Vector4(_BlendThreshold, _BlendThreshold, _BlendThreshold,
                                                        _BlendThreshold),
                    Vector4.zero)
                , new Vector4(blend.r, blend.g, blend.b, blend.a));

            return blendFactor / (blendFactor.x + blendFactor.y + blendFactor.z + blendFactor.w);
        }

        private Dictionary<Texture2D, Texture2D> tex2dDic = new Dictionary<Texture2D, Texture2D>();


        Texture2D duplicateTexture(Texture source1)
        {
            if (source1 == null) return null;

            Texture2D source = source1 as Texture2D;
            if (tex2dDic.ContainsKey(source)) return tex2dDic[source];

            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            tex2dDic.Add(source, readableText);
            return readableText;
        }

        private void DeleteAllTextures()
        {
            foreach (var itm in tex2dDic)
            {
                GameObject.DestroyImmediate(itm.Value);
            }

            tex2dDic.Clear();
        }


    }
}

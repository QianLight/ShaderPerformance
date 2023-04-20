using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace CFEngine.Editor
{
    public class SceneAtlasTool : CommonToolTemplate
    {
        enum OpType
        {
            OpNone,
            OpScanePrefab,
            OpRefreshCombineInfo,
            OpAddMateralCombine,
            OpAnalyzeMaterial,
            OpCombineMat,
            OpCreateCombineNormal,
            OpCombineNormal,
            OpCreateTexArray,
            OpUpdateAtlas,
            OpUpdateAllAtlas,
            OpPreview,
            OpCombineMesh
        }

        // class MatMeshInfo
        // {
        //     public SceneAtlas.SpriteInfo spriteInfo;
        //     public Mesh mesh;
        // }

        class AtlasContext
        {
            public Dictionary<Material, List<Mesh>> materialMap = new Dictionary<Material, List<Mesh>> ();
            public List<MeshRenderer> tmpMeshRender = new List<MeshRenderer> ();
            public Transform matTrans;
            public Transform matCombineTrans;
        }
        class CombineContext
        {
            public List<MaterialCombine> materialCombineObjs = new List<MaterialCombine> ();
            public MaterialCombine materialCombine = null;
            public int selectSpriteIndex = -1;
            public Rect pickupRect;
            public Vector2 pickupOffset;

            public string atlasName;
            public void Clear ()
            {
                materialCombine = null;
                selectSpriteIndex = -1;
                pickupRect = Rect.zero;
                pickupOffset = Vector2.zero;
            }
        }
        class CombineNormalContext
        {
            public NormalCombine normalCombine;
        }
        // class GroupInfo {
        //     public List<SceneAtlas.PrefabInfo> prefabList;
        //     public int texCount = 0;
        //     public bool hasAlpha;
        //     public bool folder = false;

        //     public Vector2 prefabScroll;
        // }

        private OpType opType = OpType.OpNone;
        private PrimitiveType primitiveType = PrimitiveType.Sphere;
        private AtlasContext atlasContext = new AtlasContext ();
        private Vector2 matCombineScroll = Vector2.zero;
        private CombineContext combineContext = new CombineContext ();

        private Material previewMat;
        private ToolsUtility.GridContext gridContext;

        private CombineNormalContext combineNormalContext = new CombineNormalContext ();

        // private SceneAtlas sceneAtlas;
        // private Vector2 scanObjectScroll = Vector2.zero;

        // private bool scanFolder = true;
        //scan
        // private Vector2 prefabScroll = Vector2.zero;
        // //prefabGroup
        // private string prefabGroupName = "";
        // private SceneAtlas.PrefabGroup selectPg = null;
        // private Vector2 groupPrefabScroll = Vector2.zero;
        // //prefab atlas
        // private int editAtlasIndex = -1;
        // private SceneAtlas.AtlasInfo editAtlas = null;

        // private SceneAtlas.PrefabInfo selectPrefab = null;
        // //tmp
        // private List<Renderer> tmpRender = new List<Renderer>();
        // private List<Texture> tmpTexList = new List<Texture>();
        // private List<SceneAtlas.PrefabInfo> tmpPrefabList = new List<SceneAtlas.PrefabInfo>();
        // // private List<CombineInstance> combineInstance = new List<CombineInstance> ();
        // private List<SceneAtlas.SpriteInfo> tmpSprite = new List<SceneAtlas.SpriteInfo>();
        // private Dictionary<GameObject, SceneAtlas.PrefabInfo> prefabMap = new Dictionary<GameObject, SceneAtlas.PrefabInfo>();

        // private List<SceneAtlas.PrefabSet> prefabSetList = new List<SceneAtlas.PrefabSet>();
        // public Dictionary<Texture2D, List<SceneAtlas.PrefabInfo>> texPrefabMap = new Dictionary<Texture2D, List<SceneAtlas.PrefabInfo>>();
        // private HashSet<GameObject> processedPrefabMap = new HashSet<GameObject>();

        // private Transform editGameObject;
        // private List<Transform> selectEditGameObject = new List<Transform>();

        public override void OnInit ()
        {
            base.OnInit ();
            Prepare ();
        }

        public override void OnUninit () { }

        private void Prepare ()
        {
            gridContext = new ToolsUtility.GridContext ();
            gridContext.drawDragRect = false;
            GameObject atlas = GameObject.Find ("Atlas");
            if (atlas == null)
            {
                atlas = new GameObject ("Atlas");
            }
            GameObject atlasMat = GameObject.Find ("Atlas/Materials");
            if (atlasMat == null)
            {
                atlasMat = new GameObject ("Materials");
                atlasMat.transform.parent = atlas.transform;
            }
            atlasContext.matTrans = atlasMat.transform;
            GameObject matCombine = GameObject.Find ("Atlas/MatCombine");
            if (matCombine == null)
            {
                matCombine = new GameObject ("MatCombine");
                matCombine.transform.parent = atlas.transform;
            }
            atlasContext.matCombineTrans = matCombine.transform;
            if (previewMat == null)
                previewMat = new Material (AssetsConfig.instance.PreviewTex);
            opType = OpType.OpRefreshCombineInfo;
        }

        private void CalcTexScaleWidth (Texture2D tex, ref Vector2 previewSize)
        {
            if (tex != null)
            {
                if (tex.width > tex.height)
                {
                    previewSize.x = 64;
                    previewSize.y = (int) (64 * ((float) tex.height / tex.width));

                }
                else
                {
                    previewSize.x = (int) (64 * ((float) tex.width / tex.height));
                    previewSize.y = 64;
                }
                // si.drawSize.x = si.scalSize.x / 4;
                // si.drawSize.y = si.scalSize.y / 4;
                // si.atlasRect.width = si.drawSize.x;
                // si.atlasRect.height = si.drawSize.y;
            }

        }
        public override void DrawGUI (ref Rect rect)
        {
            // z_GUILayout.Header(z_GUI.TempContent("Scene Atlas", ""));
            OnGameobjectScanGUI ();
            OnMatrialCombineGUI ();
            OnAtlasCombineGUI ();
            OnEventProcessGUI ();
            OnNormalCombineGUI ();
        }

        private void OnGameobjectScanGUI ()
        {
            GUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Scan", GUILayout.MaxWidth (100)))
            {
                opType = OpType.OpScanePrefab;
            }
            if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (100)))
            {
                opType = OpType.OpRefreshCombineInfo;
            }

            GUILayout.EndHorizontal ();
            GUILayout.BeginHorizontal ();
            combineContext.atlasName = EditorGUILayout.TextField ("AtlasName:", combineContext.atlasName, GUILayout.MaxWidth (400));
            if (GUILayout.Button ("Add", GUILayout.MaxWidth (100)))
            {
                opType = OpType.OpAddMateralCombine;
            }
            GUILayout.EndHorizontal ();
        }

        private void OnMatrialCombineGUI ()
        {
            // if (combineContext.matCombineTrans != null)
            {
                int count = combineContext.materialCombineObjs.Count > 10 ? 10 : combineContext.materialCombineObjs.Count;
                if (count > 0)
                {
                    matCombineScroll = GUILayout.BeginScrollView (matCombineScroll, GUILayout.MinHeight (count * 20 + 10));
                    for (int k = 0; k < combineContext.materialCombineObjs.Count; ++k)
                    {
                        MaterialCombine mc = combineContext.materialCombineObjs[k];
                        if (mc != null)
                        {
                            GUILayout.BeginHorizontal ();
                            EditorGUILayout.LabelField (string.Format ("Atlas:{0} MatCount:{1}", mc.name, mc.materialTex2D.Count));
                            if (GUILayout.Button ("Edit", GUILayout.MaxWidth (100)))
                            {

                                if (combineContext.materialCombine != mc)
                                {
                                    combineContext.Clear ();
                                    combineContext.materialCombine = mc;
                                    opType = OpType.OpAnalyzeMaterial;
                                }
                                else
                                {
                                    combineContext.Clear ();
                                }
                            }
                            if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (100)))
                            {
                                opType = OpType.OpAnalyzeMaterial;
                            }
                            GUILayout.EndHorizontal ();
                        }
                    }
                    GUILayout.EndScrollView ();
                }
            }

        }

        private void OnDrawTexPreviewGUI (Rect r, MaterialCombineTex2D mci)
        {
            var mmg = mci.mmgRef;
            if (mmg != null)
            {
                if (mmg.tex0 != null)
                {
                    Rect rect = r;
                    rect.width = mmg.previewSize.x;
                    rect.height = mmg.previewSize.y;
                    mmg.previewRect = rect;
                    EditorGUI.DrawPreviewTexture (rect, mmg.tex0);
                    if (mci.atlasPos.x >= 0 && mci.atlasPos.y >= 0)
                    {
                        Handles.DrawSolidRectangleWithOutline (rect, Color.clear, new Color (1, 0.5f, 0, 1));
                    }
                    // si.rect = r;

                    rect.x += 74;
                    rect.y += 5;
                    rect.width = 80;
                    rect.height = 15;

                    rect.y += 16;
                    rect.width = 200;
                    rect.height = 15;
                    EditorGUI.LabelField (rect, mmg.tex0.name);

                    // rect.y += 16;
                    // int scale = (int)si.scale;
                    // float realScale = 1.0f / Mathf.Pow(2, scale);
                    // EditorGUI.LabelField(rect, string.Format("texture scale:{0}:{1}x{2}", realScale, si.tex.width, si.tex.height));

                    // rect.y += 16;
                    // rect.width = 180;

                    // si.scale = GUI.HorizontalSlider(rect, si.scale, 0, 2);
                    // int newScale = (int)si.scale;
                    // if (scale != newScale)
                    // {
                    //     realScale = 1.0f / Mathf.Pow(2, newScale);
                    //     CalcTexScaleWidth(si, realScale);
                    // }

                }
            }

        }
        private void OnAtlasCombineGUI ()
        {
            if (combineContext.materialCombine != null)
            {
                var mc = combineContext.materialCombine;
                int spritePerLine = 3;
                int lineCount = mc.materialTex2D.Count / spritePerLine;
                if (mc.materialTex2D.Count % spritePerLine > 0)
                    lineCount += 1;

                for (int y = 0; y < lineCount; ++y)
                {
                    GUILayout.Space (80);
                    Rect r = GUILayoutUtility.GetLastRect ();
                    r.x += 50;
                    r.y += 10;
                    r.width = 64;
                    r.height = 64;
                    for (int x = 0; x < spritePerLine; ++x)
                    {
                        int index = y * spritePerLine + x;
                        if (index < mc.materialTex2D.Count)
                        {
                            var mci = mc.materialTex2D[index];
                            OnDrawTexPreviewGUI (r, mci);
                            r.x += 274;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                GUILayout.Space (20);

                mc.textureArray = EditorGUILayout.Toggle ("UseTexArray", mc.textureArray);
                GUILayout.Space (20);
                if (mc.textureArray)
                {
                    OnAtlasTexArrayCombineGUI (mc);
                }
                else
                {

                    OnAtlasTex2DCombineGUI (mc);
                }
            }
        }
        private void OnAtlasTex2DCombineGUI (MaterialCombine mc)
        {
            EditorGUILayout.BeginHorizontal ();
            mc.atlasWidth = (AtlasSize) EditorGUILayout.EnumPopup ("Width", mc.atlasWidth, GUILayout.MaxWidth (300));
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            mc.atlasHeight = (AtlasSize) EditorGUILayout.EnumPopup ("Height", mc.atlasHeight, GUILayout.MaxWidth (300));
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();

            if (GUILayout.Button ("Combine", GUILayout.MaxWidth (100)))
            {
                opType = OpType.OpCombineMat;
            }
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            int width = mc.atlasWidth.GetAtlasSize ();
            int height = mc.atlasHeight.GetAtlasSize ();
            EditorGUILayout.LabelField (string.Format ("size:{0}x{1}", width, height, GUILayout.MaxWidth (300)));

            EditorGUILayout.EndHorizontal ();
            GUILayout.Space (20);
            int spriteSize = mc.spriteSize.GetSpriteSize ();
            int widthCount = mc.atlasWidth.GetAtlasSize () / spriteSize;
            int heightCount = mc.atlasHeight.GetAtlasSize () / spriteSize;
            ToolsUtility.DrawGrid (gridContext, widthCount, heightCount, 64, 64);
        }

        private void OnAtlasTexArrayCombineGUI (MaterialCombine mc)
        {
            EditorGUILayout.BeginHorizontal ();
            mc.spriteSize = (SpriteSize) EditorGUILayout.EnumPopup ("SpriteSize", mc.spriteSize, GUILayout.MaxWidth (300));
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Create", GUILayout.MaxWidth (100)))
            {
                opType = OpType.OpCreateTexArray;
            }
            EditorGUILayout.EndHorizontal ();

        }

        private void OnNormalCombineGUI ()
        {
            EditorGUILayout.Space ();
            if (combineNormalContext.normalCombine == null)
            {
                GUILayout.BeginHorizontal ();
                if (GUILayout.Button ("CreateCombineNormal", GUILayout.MaxWidth (200)))
                {
                    opType = OpType.OpCreateCombineNormal;
                }

                GUILayout.EndHorizontal ();
            }
            else
            {
                NormalCombine nc = combineNormalContext.normalCombine;
                GUILayout.BeginHorizontal ();
                nc.normal0 = EditorGUILayout.ObjectField ("Normal0", nc.normal0, typeof (Texture2D), false, GUILayout.MaxWidth (200)) as Texture2D;
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                nc.normal1 = EditorGUILayout.ObjectField ("Normal1", nc.normal1, typeof (Texture2D), false, GUILayout.MaxWidth (200)) as Texture2D;
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                nc.normalSize = (SpriteSize) EditorGUILayout.EnumPopup ("NormalSize", nc.normalSize, GUILayout.MaxWidth (200));
                GUILayout.EndHorizontal ();

                GUILayout.Space (10);
                GUILayout.BeginHorizontal ();
                nc.normal2 = EditorGUILayout.ObjectField ("Normal2", nc.normal2, typeof (Texture2D), false, GUILayout.MaxWidth (200)) as Texture2D;
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                nc.blend = EditorGUILayout.ObjectField ("Blend", nc.blend, typeof (Texture2D), false, GUILayout.MaxWidth (200)) as Texture2D;
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                if (GUILayout.Button ("CombineNormal", GUILayout.MaxWidth (100)))
                {
                    opType = OpType.OpCombineNormal;
                }

                GUILayout.EndHorizontal ();
            }

        }
        private void OnEventProcessGUI ()
        {
            if (combineContext.materialCombine != null)
            {
                var mc = combineContext.materialCombine;
                if (mc.textureArray)
                {

                }
                else
                {

                    OnEventProcessTex2DGUI (mc);
                }

            }
        }
        private void OnEventProcessTex2DGUI (MaterialCombine mc)
        {
            var e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                gridContext.doubleClick = e.clickCount == 2;
                bool leftMouse = e.button == 0;
                // bool rightMouse = e.button == 1;
                if (leftMouse)
                {
                    for (int x = 0; x < mc.materialTex2D.Count; ++x)
                    {
                        var mci = mc.materialTex2D[x];
                        if (mci.mmgRef != null)
                        {
                            Rect previewRect = mci.mmgRef.previewRect;
                            if (mci.atlasPos.x >= 0 && mci.atlasPos.y >= 0)
                            {
                                Rect startRect = gridContext.innerRect;
                                previewRect.xMin = startRect.xMin + 64 * mci.atlasPos.x;
                                previewRect.yMin = startRect.yMax - 64 * mci.atlasPos.y - 64;
                                previewRect.xMax = previewRect.xMin + 64 * mci.atlasSize.x;
                                previewRect.yMax = previewRect.yMin + 64 * mci.atlasSize.y;
                            }
                            if (previewRect.Contains (e.mousePosition))
                            {
                                combineContext.selectSpriteIndex = x;
                                combineContext.pickupOffset.x = e.mousePosition.x - previewRect.xMin;
                                combineContext.pickupOffset.y = e.mousePosition.y - previewRect.yMin;
                                combineContext.pickupRect = previewRect;
                                SceneTool.DoRepaint ();
                                break;
                            }
                        }

                    }
                }
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (combineContext.selectSpriteIndex >= 0)
                {
                    var mci = mc.materialTex2D[combineContext.selectSpriteIndex];

                    combineContext.pickupRect.xMin = e.mousePosition.x - combineContext.pickupOffset.x;
                    combineContext.pickupRect.yMin = e.mousePosition.y - combineContext.pickupOffset.y;
                    combineContext.pickupRect.xMax = combineContext.pickupRect.xMin + 64 * mci.atlasSize.x;
                    combineContext.pickupRect.yMax = combineContext.pickupRect.yMin + 64 * mci.atlasSize.y;
                    SceneTool.DoRepaint ();
                }

            }
            else if (e.type == EventType.MouseUp)
            {
                if (combineContext.selectSpriteIndex >= 0)
                {
                    var mci = mc.materialTex2D[combineContext.selectSpriteIndex];
                    if (gridContext.innerRect.Contains (e.mousePosition))
                    {
                        mci.atlasPos = ToolsUtility.CalcGridIndex (gridContext, e.mousePosition);

                    }
                    else
                    {
                        mci.atlasPos = new Vector2Int (-1, -1);
                    }
                    combineContext.selectSpriteIndex = -1;
                }

            }
            if (e.type == EventType.Repaint)
            {
                if (combineContext.selectSpriteIndex >= 0)
                {
                    var mci = mc.materialTex2D[combineContext.selectSpriteIndex];
                    if (mci.mmgRef != null && mci.mmgRef.tex0 != null)
                    {
                        EditorGUI.DrawPreviewTexture (combineContext.pickupRect, mci.mmgRef.tex0, previewMat, ScaleMode.ScaleAndCrop);
                        Handles.color = Color.white * (GUI.enabled ? 1f : 0.5f);
                        Handles.DrawSolidRectangleWithOutline (combineContext.pickupRect, Color.clear, gridContext.rectOutlineColor);
                    }

                }
                for (int x = 0; x < mc.materialTex2D.Count; ++x)
                {
                    var mci = mc.materialTex2D[x];
                    if (mci.mmgRef != null && mci.mmgRef.tex0 != null)
                    {
                        if (mci.atlasPos.x >= 0 && mci.atlasPos.y >= 0)
                        {
                            Rect rect = gridContext.innerRect;
                            rect.xMin = rect.xMin + 64 * mci.atlasPos.x;
                            rect.yMin = rect.yMax - 64 * mci.atlasPos.y - 64;
                            rect.xMax = rect.xMin + 64 * mci.atlasSize.x;
                            rect.yMax = rect.yMin + 64 * mci.atlasSize.y;
                            EditorGUI.DrawPreviewTexture (rect, mci.mmgRef.tex0);
                        }
                    }
                }
            }
        }

        #region atlas op
        private void ScanGameObject ()
        {
            EditorCommon.EnumTransform fun = null;
            fun = (trans, param) =>
            {
                AtlasContext context = param as AtlasContext;
                if (EditorCommon.IsPrefabOrFbx (trans.gameObject))
                {
                    context.tmpMeshRender.Clear ();
                    trans.GetComponentsInChildren<MeshRenderer> (true, context.tmpMeshRender);
                    for (int i = 0; i < context.tmpMeshRender.Count; ++i)
                    {
                        var mr = context.tmpMeshRender[i];
                        Material mat = mr.sharedMaterial;
                        if (mat != null)
                        {
                            List<Mesh> meshs;
                            if (!context.materialMap.TryGetValue (mat, out meshs))
                            {
                                meshs = new List<Mesh> ();
                                context.materialMap[mat] = meshs;
                            }
                            MeshFilter mf = mr.GetComponent<MeshFilter> ();
                            if (mf != null && mf.sharedMesh != null)
                            {
                                if (!meshs.Contains (mf.sharedMesh))
                                {
                                    meshs.Add (mf.sharedMesh);
                                }
                            }
                        }

                    }
                }
                else
                {
                    EditorCommon.EnumChildObject (trans, context, fun);
                }
            };

            if (atlasContext.matTrans != null)
                EditorCommon.DestroyChildObjects (atlasContext.matTrans.gameObject);

            for (int i = (int) EditorSceneObjectType.Prefab; i <= (int) EditorSceneObjectType.Instance; ++i)
            {
                string path = AssetsConfig.EditorGoPath[0] + "/" + AssetsConfig.EditorGoPath[i];
                GameObject go = GameObject.Find (path);
                if (go != null)
                {
                    EditorCommon.EnumTargetObject (path, (trans, param) =>
                    {
                        fun (trans, atlasContext);
                    });
                }
            }
            int widthCount = 5;
            int size = 2;
            int x = 0;
            int z = 0;
            var it = atlasContext.materialMap.GetEnumerator ();
            while (it.MoveNext ())
            {
                var kvp = it.Current;
                var mat = kvp.Key;
                bool find = false;
                for (int k = 0; k < combineContext.materialCombineObjs.Count; ++k)
                {
                    MaterialCombine mc = combineContext.materialCombineObjs[k];
                    if (mc != null)
                    {
                        for (int j = 0; j < mc.materialTex2D.Count; ++j)
                        {
                            var mi = mc.materialTex2D[k];
                            if (mi != null && mi.mmgRef != null && mi.mmgRef.mat == mat)
                            {
                                find = true;
                            }
                        }
                    }
                }
                if (!find)
                {
                    GameObject matGo = ObjectFactory.CreatePrimitive (primitiveType);
                    Collider c = matGo.GetComponent<Collider> ();
                    UnityEngine.Object.DestroyImmediate (c);
                    matGo.name = mat.name;
                    MaterialMeshGroup mci = matGo.AddComponent<MaterialMeshGroup> ();
                    mci.meshs.AddRange (kvp.Value);
                    mci.mat = mat;
                    MeshRenderer mr = matGo.GetComponent<MeshRenderer> ();
                    mr.sharedMaterial = mat;
                    matGo.transform.parent = atlasContext.matTrans;
                    matGo.transform.position = new Vector3 (x * size, -size, z * size);
                    x++;
                    if (x > widthCount)
                    {
                        x = 0;
                        z++;
                    }
                }

            }

        }

        private void RefreshCombineInfo ()
        {
            if (atlasContext.matCombineTrans != null)
            {
                combineContext.materialCombineObjs.Clear ();
                for (int i = 0; i < atlasContext.matCombineTrans.childCount; ++i)
                {
                    Transform child = atlasContext.matCombineTrans.GetChild (i);
                    MaterialCombine mc = child.GetComponent<MaterialCombine> ();
                    if (mc != null)
                    {
                        combineContext.materialCombineObjs.Add (mc);
                    }
                }
            }
        }

        private void AddMaterialCombine ()
        {
            if (atlasContext.matCombineTrans != null && !string.IsNullOrEmpty (combineContext.atlasName))
            {
                GameObject matGo = ObjectFactory.CreatePrimitive (primitiveType);
                Collider c = matGo.GetComponent<Collider> ();
                UnityEngine.Object.DestroyImmediate (c);
                matGo.name = combineContext.atlasName;
                MaterialCombine mc = matGo.AddComponent<MaterialCombine> ();
                MeshRenderer mr = matGo.GetComponent<MeshRenderer> ();
                Material mat = new Material (AssetsConfig.instance.ScenePreview);
                mat.name = combineContext.atlasName;
                mat = CommonAssets.CreateAsset<Material> (AssetsConfig.instance.AtlasDirStr, mat.name, ".mat", mat);
                mr.sharedMaterial = mat;
                matGo.transform.parent = atlasContext.matCombineTrans;
                combineContext.atlasName = "";
                combineContext.materialCombineObjs.Add (mc);
            }
        }

        private void AnalyzeMaterial ()
        {
            if (combineContext.materialCombine != null)
            {
                List<MaterialCombineTex2D> tmp = new List<MaterialCombineTex2D> ();
                Transform t = combineContext.materialCombine.transform;
                for (int i = 0; i < t.childCount; ++i)
                {
                    var child = t.GetChild (i);
                    var mmg = child.GetComponent<MaterialMeshGroup> ();
                    var mci = combineContext.materialCombine.materialTex2D.Find ((m) => { return m.mmgRef == mmg; });
                    if (mci == null)
                    {
                        mci = new MaterialCombineTex2D ();
                        mci.mmgRef = mmg;
                        combineContext.materialCombine.materialTex2D.Add (mci);
                    }
                    tmp.Add (mci);
                }
                combineContext.materialCombine.materialTex2D.Clear ();
                combineContext.materialCombine.materialTex2D.AddRange (tmp);
                int size = 2;
                int x = 0;
                int z = 0;
                int widthCount = 5;
                for (int i = 0; i < combineContext.materialCombine.materialTex2D.Count; ++i)
                {
                    var mci = combineContext.materialCombine.materialTex2D[i];
                    if (mci.mmgRef != null && mci.mmgRef.mat != null)
                    {

                        MaterialContext context = MaterialContext.GetContext ();
                        // MaterialAnalyzeModify mam = new MaterialAnalyzeModify();
                        // MaterialShaderAssets.GetDefaultMatProperty(mci.mmgRef.mat, ref context,
                        //     AssetsConfig.GlobalAssetsConfig.matShaderType,
                        //     AssetsConfig.GlobalAssetsConfig.commonShaderProperty,
                        //     AssetsConfig.GlobalAssetsConfig.shaderPropertyKey,
                        // ref mam);
                        if (context.textureValue.Count > 0)
                        {
                            var stpv = context.textureValue[0];
                            mci.mmgRef.tex0 = stpv.value as Texture2D;
                            if (context.textureValue.Count > 1)
                            {
                                stpv = context.textureValue[1];
                                mci.mmgRef.tex1 = stpv.value as Texture2D;
                            }
                        }

                        CalcTexScaleWidth (mci.mmgRef.tex0, ref mci.mmgRef.previewSize);
                        mci.mmgRef.transform.localPosition = new Vector3 (x * size, -size, z * size);
                        x++;
                        if (x > widthCount)
                        {
                            x = 0;
                            z++;
                        }
                    }
                }
            }
        }

        private void CombineMaterial ()
        {
            if (combineContext.materialCombine != null)
            {

                if (combineContext.materialCombine.materialTex2D.Count > 0)
                {
                    int width = combineContext.materialCombine.atlasWidth.GetAtlasSize ();
                    int height = combineContext.materialCombine.atlasHeight.GetAtlasSize ();

                    RenderTexture rt0 = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
                    {
                        name = "Pack Tex",
                        hideFlags = HideFlags.DontSave,
                        filterMode = FilterMode.Bilinear,
                        wrapMode = TextureWrapMode.Clamp,
                        anisoLevel = 0,
                        autoGenerateMips = false,
                        useMipMap = false
                    };
                    rt0.Create ();

                    for (int i = 0; i < combineContext.materialCombine.materialTex2D.Count; ++i)
                    {
                        var mci = combineContext.materialCombine.materialTex2D[i];
                        if (mci.mmgRef != null && mci.mmgRef.tex0 != null)
                        {
                            int spriteWidth = mci.spriteWidth.GetSpriteSize ();
                            int spriteHeight = mci.spriteHeight.GetSpriteSize ();

                            int offsetX = spriteWidth * mci.atlasPos.x;
                            int offsetY = spriteHeight * mci.atlasPos.y;
                            TextureAssets.PackAtlas (rt0, mci.mmgRef.tex0, offsetX, offsetY, spriteWidth * mci.atlasSize.x, spriteHeight * mci.atlasSize.y);
                        }
                    }

                    // Texture2D finalTex = new Texture2D(width, height, TextureFormat.ARGB32, true);

                    combineContext.materialCombine.atlas0 = CommonAssets.CreateAsset<Texture2D> (AssetsConfig.instance.AtlasDirStr, combineContext.materialCombine.name, ".png", rt0);

                    UnityObject.DestroyImmediate (rt0);
                }
            }
        }
        private void CreateTexArray ()
        {
            if (combineContext.materialCombine != null)
            {
                var mc = combineContext.materialCombine;

                if (mc.materialTex2D.Count > 0)
                {
                    int size = mc.spriteSize.GetSpriteSize ();
                    Texture2DArray tex2DArray = new Texture2DArray (size, size, mc.materialTex2D.Count, TextureFormat.ASTC_5x5, true);

                    for (int i = 0; i < combineContext.materialCombine.materialTex2D.Count; ++i)
                    {
                        var mci = combineContext.materialCombine.materialTex2D[i];
                        if (mci.mmgRef != null && mci.mmgRef.tex0 != null)
                        {
                            for (int j = 0; j < mci.mmgRef.tex0.mipmapCount; ++j)
                            {
                                Graphics.CopyTexture (mci.mmgRef.tex0, 0, j, tex2DArray, i, j);
                            }
                        }
                    }
                    tex2DArray.Apply (false, true);
                    AssetDatabase.CreateAsset (tex2DArray, "Assets/t2d.asset");

                }
            }
        }
        private void CreateCombineNormal ()
        {
            GameObject go = GameObject.Find ("CombineNormal");
            if (go == null)
            {
                go = new GameObject ("CombineNormal");
            }
            combineNormalContext.normalCombine = go.GetComponent<NormalCombine> ();
            if (combineNormalContext.normalCombine == null)
            {
                combineNormalContext.normalCombine = go.AddComponent<NormalCombine> ();
            }
        }
        private void CombineNormal ()
        {
            NormalCombine nc = combineNormalContext.normalCombine;
            if (nc.normal0 != null && nc.normal1 != null)
            {
                string path = "";
                string name = "";
                if (nc.combineNormal != null)
                {
                    path = AssetDatabase.GetAssetPath (nc.combineNormal);
                    name = nc.combineNormal.name;
                    path = Path.GetDirectoryName (path);
                }
                else
                {
                    name = nc.normal0.name + "_" + nc.normal1.name;
                    path = AssetDatabase.GetAssetPath (nc.normal0);
                    path = Path.GetDirectoryName (path);
                }
                Material tmp = new Material (AssetsConfig.instance.CombineNormal);
                tmp.SetTexture ("_NormalMap01", nc.normal0);
                tmp.SetTexture ("_NormalMap02", nc.normal1);
                int size = nc.GetNormalSize ();
                RenderTexture rt0 = new RenderTexture (size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
                {
                    name = "NormalCombine Tex",
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    anisoLevel = 0,
                    autoGenerateMips = false,
                    useMipMap = false
                };
                rt0.Create ();
                TextureAssets.BeginDrawRT ();
                TextureAssets.DrawRT (rt0, tmp);
                TextureAssets.EndDrawRT ();
                nc.combineNormal = CommonAssets.CreateAsset<Texture2D> (path, name, ".png", rt0);
                UnityObject.DestroyImmediate (rt0);
                UnityEngine.Object.DestroyImmediate (tmp);

            }
        }

        #endregion

        public override void Update ()
        {
            switch (opType)
            {
                case OpType.OpScanePrefab:
                    ScanGameObject ();
                    break;
                case OpType.OpAnalyzeMaterial:
                    AnalyzeMaterial ();
                    break;
                case OpType.OpRefreshCombineInfo:
                    RefreshCombineInfo ();
                    break;

                case OpType.OpAddMateralCombine:
                    AddMaterialCombine ();
                    break;
                case OpType.OpCombineMat:
                    CombineMaterial ();
                    break;
                case OpType.OpCreateTexArray:
                    CreateTexArray ();
                    break;
                case OpType.OpCreateCombineNormal:
                    CreateCombineNormal ();
                    break;
                case OpType.OpCombineNormal:
                    CombineNormal ();
                    break;

                    // case OpType.OpUpdateAtlas:
                    //     opType = OnUpateAtlas();
                    //     break;
                    // case OpType.OpUpdateAllAtlas:
                    //     opType = OnUpateAllAtlas ();
                    //     break;
                    // case OpType.OpPreview:
                    //     opType = PreviewPrefab ();
                    //     break;
                    // case OpType.OpCombineMesh:
                    //     opType = CombineMesh ();
                    //     break;
            }
            opType = OpType.OpNone;
        }

        // private Mesh ProcessMesh (Mesh mesh, SceneAtlas.SpriteInfo si, SceneAtlas.AtlasInfo ai)
        // {
        //     Mesh newMesh = UnityEngine.Object.Instantiate (mesh);
        //     int uvCount = newMesh.uv.Length;
        //     Color[] colors = new Color[uvCount];
        //     Vector4 uvst = new Vector4 ((float) (si.scalSize.x - 1) / ai.size - 0.001f,
        //         (float) (si.scalSize.y - 1) / ai.size - 0.001f, (si.atlasPos.x * 256.0f) / ai.size, (float) ((si.atlasPos.y) * 256) / ai.size + 0.002f);
        //     for (int i = 0; i < uvCount; ++i)
        //     {
        //         colors[i] = new Color (uvst.x, uvst.y, uvst.z, uvst.w);
        //     }
        //     newMesh.colors = colors;

        //     return newMesh;

        // }

        // private void CalcAABB (Mesh mesh, out Vector3 min, out Vector3 max)
        // {
        //     min = new Vector3 (10000, 10000, 10000);
        //     max = new Vector3 (-10000, -10000, -10000);
        //     Vector3[] vertices = mesh.vertices;
        //     int vertexCount = vertices.Length;
        //     for (int i = 0; i < vertexCount; ++i)
        //     {
        //         Vector3 vector = vertices[i];
        //         if (vector.x < min.x)
        //         {
        //             min.x = vector.x;
        //         }
        //         if (vector.y < min.y)
        //         {
        //             min.y = vector.y;
        //         }
        //         if (vector.z < min.z)
        //         {
        //             min.z = vector.z;
        //         }
        //         if (vector.x > max.x)
        //         {
        //             max.x = vector.x;
        //         }
        //         if (vector.y > max.y)
        //         {
        //             max.y = vector.y;
        //         }
        //         if (vector.z > max.z)
        //         {
        //             max.z = vector.z;
        //         }
        //     }
        // }
        // private void PreviewPrefab (SceneAtlas.PrefabGroup prefabGroup, SceneAtlas.PrefabInfo prefabInfo)
        // {
        //     Dictionary<SceneAtlas.AtlasInfo, List<MatMeshInfo>> meshInfo = new Dictionary<SceneAtlas.AtlasInfo, List<MatMeshInfo>> ();
        //     tmpRender.Clear ();
        //     prefabInfo.prefab.GetComponentsInChildren<Renderer> (tmpRender);
        //     for (int i = 0; i < tmpRender.Count; ++i)
        //     {
        //         Renderer render = tmpRender[i];
        //         if (render != null)
        //         {
        //             MeshFilter mf = render.GetComponent<MeshFilter> ();
        //             if (mf != null && mf.sharedMesh != null)
        //             {
        //                 for (int j = 0; j < prefabInfo.mats.Count; ++j)
        //                 {
        //                     SceneAtlas.MatInfo mi = prefabInfo.mats[j];
        //                     if (mi.mat == render.sharedMaterial)
        //                     {
        //                         SceneAtlas.SpriteInfo si;
        //                         SceneAtlas.AtlasInfo ai = FindAtlas (prefabGroup, mi, 0, out si);
        //                         if (ai != null)
        //                         {
        //                             List<MatMeshInfo> meshList = null;
        //                             if (!meshInfo.TryGetValue (ai, out meshList))
        //                             {
        //                                 meshList = new List<MatMeshInfo> ();
        //                                 meshInfo.Add (ai, meshList);
        //                             }
        //                             bool find = false;
        //                             for (int k = 0; k < meshList.Count; ++k)
        //                             {
        //                                 var mmi = meshList[k];
        //                                 if (mmi.mesh == mf.sharedMesh)
        //                                 {
        //                                     find = true;
        //                                     break;
        //                                 }
        //                             }
        //                             if (!find)
        //                             {
        //                                 meshList.Add (new MatMeshInfo () { mesh = mf.sharedMesh, spriteInfo = si });
        //                             }

        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //     }

        //     if (meshInfo.Count > 0)
        //     {
        //         string meshDir = string.Format ("{0}/{1}", SceneAssets.scene_directory, AssetsConfig.GlobalAssetsConfig.AtlasDirStr);
        //         var it = meshInfo.GetEnumerator ();
        //         while (it.MoveNext ())
        //         {
        //             var kvp = it.Current;
        //             var ai = kvp.Key;
        //             var meshList = kvp.Value;
        //             Mesh mesh = null;
        //             Vector4 uvst = new Vector4 (1, 1, 0, 0);
        //             Shader shader = null;
        //             if (meshList.Count == 1)
        //             {
        //                 mesh = UnityEngine.Object.Instantiate (meshList[0].mesh);
        //                 SceneAtlas.SpriteInfo si = meshList[0].spriteInfo;
        //                 float offsetX = si.atlasPos.x * 256.0f / ai.size;
        //                 float offsetY = (float) ((si.atlasPos.y) * 256 + 1) / ai.size;
        //                 uvst = new Vector4 ((float) (si.scalSize.x - 1) / ai.size, (float) (si.scalSize.y - 1) / ai.size, offsetX, offsetY);
        //                 shader = AssetsConfig.GlobalAssetsConfig.ScenePreview;
        //             }
        //             else if (meshList.Count > 1)
        //             {
        //                 CombineInstance[] ciArray = new CombineInstance[meshList.Count];
        //                 for (int i = 0; i < meshList.Count; ++i)
        //                 {
        //                     CombineInstance ci = new CombineInstance ();
        //                     ci.mesh = ProcessMesh (meshList[i].mesh, meshList[i].spriteInfo, ai);
        //                     ciArray[i] = ci;
        //                 }
        //                 mesh = new Mesh ();
        //                 mesh.name = prefabInfo.prefab.name;
        //                 mesh.CombineMeshes (ciArray, true, false, false);
        //                 mesh.UploadMeshData (true);
        //                 string meshPath = string.Format ("{0}/{1}.asset", meshDir, mesh.name);
        //                 if (System.IO.File.Exists (meshPath))
        //                 {
        //                     AssetDatabase.DeleteAsset (meshPath);
        //                 }
        //                 mesh = CommonAssets.CreateAsset<Mesh> (meshDir, mesh.name, ".asset", mesh);
        //                 shader = AssetsConfig.GlobalAssetsConfig.ScenePreview;
        //             }
        //             if (mesh != null)
        //             {
        //                 GameObject go = new GameObject (mesh.name);
        //                 MeshFilter mf = go.AddComponent<MeshFilter> ();
        //                 mf.sharedMesh = mesh;
        //                 MeshRenderer mr = go.AddComponent<MeshRenderer> ();
        //                 mr.sharedMaterial = new Material (shader);
        //                 if (meshList.Count == 1)
        //                 {
        //                     mr.sharedMaterial.SetVector ("_AtlasUVST", uvst);

        //                 }
        //                 else
        //                 {
        //                     mr.sharedMaterial.EnableKeyword ("_ATLASINCOLOR");
        //                 }
        //                 mr.sharedMaterial.SetTexture ("_BaseTex", ai.atlas);
        //                 mr.sharedMaterial.EnableKeyword ("_PBS_NO_IBL");
        //                 mr.sharedMaterial.EnableKeyword ("_PBS_FROM_PARAM");
        //                 mr.sharedMaterial.EnableKeyword ("_ATLAS");
        //                 mr.sharedMaterial.EnableKeyword ("SYSTEM_LIGHTINFO");
        //                 Vector3 min;
        //                 Vector3 max;
        //                 CalcAABB (mesh, out min, out max);
        //                 prefabInfo.aabb.center = (min + max) * 0.5f;
        //                 prefabInfo.aabb.size = (max - min);
        //                 ObjectCombine oc = go.AddComponent<ObjectCombine> ();
        //                 oc.aabb = prefabInfo.aabb;
        //             }

        //         }
        //     }
        // }

        // private OpType PreviewPrefab ()
        // {
        //     if (selectPg != null && selectPrefab != null)
        //     {
        //         PreviewPrefab (selectPg, selectPrefab);
        //     }
        //     return OpType.OpNone;
        // }

        // // private void CombinePrefab (Transform trans) {
        // //     int childCount = trans.childCount;
        // //     for (int i = 0; i < childCount; ++i) {
        // //         Transform child = trans.GetChild (i);

        // //     }
        // // }

        // private OpType CombineMesh ()
        // {

        //     int count = 0;
        //     for (int i = 0; i < sceneAtlas.groups.Count; ++i)
        //     {
        //         var pg = sceneAtlas.groups[i];
        //         for (int j = 0; j < pg.prefabSets.Count; ++j)
        //         {
        //             var ps = pg.prefabSets[j];
        //             count += ps.prefabs.Count;
        //         }
        //     }
        //     float index = 0;
        //     for (int i = 0; i < sceneAtlas.groups.Count; ++i)
        //     {
        //         var pg = sceneAtlas.groups[i];
        //         for (int j = 0; j < pg.prefabSets.Count; ++j)
        //         {
        //             var ps = pg.prefabSets[j];
        //             for (int k = 0; k < ps.prefabs.Count; ++k)
        //             {
        //                 var pi = ps.prefabs[k];
        //                 EditorUtility.DisplayProgressBar ("UpdateAtlas", string.Format ("Group:{0} Prefab:{1}", pg.name, pi.prefab.name), index / count);
        //                 PreviewPrefab (pg, pi);
        //                 index++;
        //             }
        //         }
        //     }
        //     EditorUtility.ClearProgressBar ();

        //     return OpType.OpNone;
        // }

        #region LOD

        // private bool bLODEditorShow = false;
        // private int nrOfLevels = 3;
        // private float[] compression = new float[5] { 0.25f, 0.5f, 1f, 1.5f, 2f };
        // private bool recalcNormals = true;
        // private float smallObjectsValue = 1f;
        // private float useValueForProtectNormals = 1f;
        // private float useValueForProtectUvs = 1f;
        // private float useValueForProtectBigTriangles = 1f;
        // private float useValueForProtectSubMeshesAndSharpEdges = 1f;
        // private int useValueForNrOfSteps = 1;
        // private string CreateLODSwitcherWithMeshes (GameObject aGo)
        // {
        //     if (aGo == null) return "No gameObject selected";
        //     string path = StoragePathUsing1stMeshAndSubPath (aGo, "LODMeshes");
        //     if (path != null)
        //     {
        //         float[] useCompressions = new float[nrOfLevels];
        //         Mesh[] meshes = null;
        //         MakeBackup (aGo, true);
        //         for (int i = 0; i < nrOfLevels; i++) useCompressions[i] = compression[i];
        //         try
        //         {
        //             meshes = aGo.SetUpLODLevelsWithLODSwitcher (GetDftLodScreenSizes (nrOfLevels), useCompressions, recalcNormals, smallObjectsValue, useValueForProtectNormals, useValueForProtectUvs, useValueForProtectSubMeshesAndSharpEdges, useValueForProtectBigTriangles, useValueForNrOfSteps);
        //         }
        //         catch (Exception e)
        //         {
        //             Debug.LogError (e);
        //             return e.Message;
        //         }
        //         if (meshes != null)
        //         {
        //             string sizeStr = "";
        //             sizeStr = "LOD 0: " + meshes[0].vertexCount + " vertices, " + (meshes[0].triangles.Length / 3) + " triangles";
        //             path = path + "/" + aGo.name + "_LOD";
        //             for (int i = 1; i < meshes.Length; i++)
        //             {
        //                 sizeStr = sizeStr + "\nLOD " + i + ": " + meshes[i].vertexCount + " vertices, " + (meshes[i].triangles.Length / 3) + " triangles";
        //                 if (meshes[i] != null && meshes[i].vertexCount > 0)
        //                 {
        //                     string meshPath = AssetDatabase.GenerateUniqueAssetPath (path + i + ".asset");
        //                     AssetDatabase.CreateAsset (meshes[i], meshPath);
        //                     AssetDatabase.SaveAssets ();
        //                 }
        //             }
        //             Resources.UnloadUnusedAssets ();
        //             return "Finished! LOD meshes were saved under " + path + "[1..." + meshes.Length + "].\n" + sizeStr;
        //         }
        //     }
        //     return "No mesh found in gameobject";
        // }

        // private string StoragePathUsing1stMeshAndSubPath (GameObject aGo, string subPath)
        // {
        //     //Mesh firstMesh = aGo.Get1stSharedMesh();
        //     //if (firstMesh != null)
        //     //{
        //     //    string[] pathSegments = new string[3] { "Assets", "SimpleLOD", "WillBeIgnored" };
        //     //    string assetPath = AssetDatabase.GetAssetPath(firstMesh);
        //     //    if (assetPath != null && assetPath.Length > 0 && assetPath.StartsWith("Assets/")) pathSegments = assetPath.Split(new Char[] { '/' });
        //     //    if (pathSegments.Length > 0)
        //     //    {
        //     //        string path = "";
        //     //        for (int i = 0; i < pathSegments.Length - 1; i++)
        //     //        {
        //     //            if (pathSegments[i] != "MergedMeshes" && pathSegments[i] != "CleanedMeshes" && pathSegments[i] != "LODMeshes" && pathSegments[i] != "SimplifiedMeshes" && pathSegments[i] != "AtlasTextures" && pathSegments[i] != "AtlasMaterials" && pathSegments[i] != "AtlasMeshes")
        //     //            {
        //     //                if (i > 0 && (!Directory.Exists(path + "/" + pathSegments[i]))) AssetDatabase.CreateFolder(path, pathSegments[i]);
        //     //                if (i > 0) path = path + "/";
        //     //                path = path + pathSegments[i];
        //     //            }
        //     //        }
        //     //        if (!Directory.Exists(path + "/" + subPath)) AssetDatabase.CreateFolder(path, subPath);
        //     //        return path + "/" + subPath;
        //     //    }
        //     //}
        //     //return null;
        //     return "Assets/Scenes/modlelib/LOD";
        // }

        // private static void MakeBackup (GameObject aGO, bool onlyIfNotExists)
        // {
        //     if (onlyIfNotExists && GetBackup (aGO) != null) return;
        //     aGO = GameObjectToBackup (aGO);
        //     if (aGO == null) return;
        //     if (aGO.FindParentWithName ("_SimpleLOD_backups_delete_when_ready") != null) return; // no backup of a backup
        //     if (aGO.name.IndexOf ("_$LodGrp", 0) >= 0) return;
        //     GameObject bcpTopGO = GameObject.Find ("_SimpleLOD_backups_delete_when_ready");
        //     if (bcpTopGO == null)
        //     {
        //         bcpTopGO = new GameObject ("_SimpleLOD_backups_delete_when_ready");
        //         bcpTopGO.transform.position = Vector3.zero;
        //     }
        //     else
        //     {
        //         GameObject oldBcp = GetBackup (aGO);
        //         if (oldBcp != null) UnityEngine.Object.DestroyImmediate (oldBcp);
        //     }
        //     GameObject bcpGO = (GameObject) GameObject.Instantiate (aGO);
        //     bcpGO.name = bcpGO.name.Replace ("(Clone)", "");

        //     bcpGO.transform.SetParent (bcpTopGO.transform);

        //     bcpGO.SetActive (false);
        // }

        // private float[] GetDftLodScreenSizes (int aNrOflevels)
        // {
        //     switch (nrOfLevels)
        //     {
        //         case 1:
        //             return new float[1] { 0.5f };
        //         case 2:
        //             return new float[2] { 0.6f, 0.3f };
        //         case 3:
        //             return new float[3] { 0.6f, 0.3f, 0.15f };
        //         case 4:
        //             return new float[4] { 0.75f, 0.5f, 0.25f, 0.13f };
        //         default:
        //             return new float[5] { 0.8f, 0.6f, 0.4f, 0.2f, 0.1f };
        //     }
        // }

        // private static GameObject GetBackup (GameObject aGO)
        // {
        //     aGO = GameObjectToBackup (aGO);
        //     if (aGO == null) return null;
        //     GameObject bcpTopGO = GameObject.Find ("_SimpleLOD_backups_delete_when_ready");
        //     if (bcpTopGO != null)
        //     {
        //         foreach (Transform child in bcpTopGO.transform)
        //         {
        //             if (child.gameObject.name == aGO.name) return child.gameObject;
        //         }
        //     }
        //     return null;
        // }

        // private static GameObject GameObjectToBackup (GameObject aGO)
        // {
        //     // check if there is a skinned mesh renderer
        //     // if yes, then find the mutual parent of the smr and the bones
        //     // and return that instead
        //     // because otherwise the link breaks when you restore the backup
        //     if (aGO == null) return null;
        //     if (aGO.name.IndexOf ("_$Lod:", 0) >= 0)
        //     {
        //         if (aGO.transform.parent == null) return null;
        //         aGO = aGO.transform.parent.gameObject;
        //     }
        //     if (aGO.name.IndexOf ("_$LodGrp", 0) >= 0)
        //     {
        //         int pos = aGO.name.IndexOf ("_$LodGrp", 0);
        //         if (pos <= 0) return null;
        //         string name = aGO.name.Substring (0, pos);
        //         aGO = aGO.FindFirstChildWithName (name);
        //     }

        //     SkinnedMeshRenderer[] smr = aGO.GetComponentsInChildren<SkinnedMeshRenderer> (true);
        //     if (smr != null && smr.Length > 0)
        //     {
        //         Transform[] bones = smr[0].bones;
        //         if (bones != null && bones.Length > 1)
        //         {
        //             GameObject mutualParent = aGO.FindMutualParent (bones[0].gameObject);
        //             if (mutualParent != null) return mutualParent;
        //         }
        //     }
        //     return aGO;
        // }
        #endregion
    }
}
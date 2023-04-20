using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class BrushRep
    {
        int m_Size;
        float[] m_Strength;
        Texture2D m_CurMask = null;
        internal const int kMinBrushSize = 3;

        static float UnpackRG16ToFloat (float r, float g)
        {
            return (r + g * 256.0f) / 257.0f;
        }

        public float GetStrengthInt (int ix, int iy)
        {
            ix = Mathf.Clamp (ix, 0, m_Size - 1);
            iy = Mathf.Clamp (iy, 0, m_Size - 1);

            float s = m_Strength[iy * m_Size + ix];

            return s;
        }

        public void CreateFromBrush (Texture2D pMask, int size)
        {
            if (size == m_Size && m_Strength != null && m_CurMask == pMask)
            {
                return;
            }

            Texture2D mask = pMask;
            if (mask != null)
            {
                Texture2D readableTexture = null;
                if (!mask.isReadable)
                {
                    readableTexture = new Texture2D (mask.width, mask.height, mask.format, mask.mipmapCount > 1);
                    Graphics.CopyTexture (mask, readableTexture);
                    readableTexture.Apply ();
                }
                else
                {
                    readableTexture = mask;
                }

                float fSize = size;
                m_Size = size;
                m_Strength = new float[m_Size * m_Size];
                if (m_Size > kMinBrushSize)
                {
                    for (int y = 0; y < m_Size; ++y)
                    {
                        float v = y / fSize;
                        for (int x = 0; x < m_Size; ++x)
                        {
                            float u = x / fSize;
                            Color texel = readableTexture.GetPixelBilinear (u, v);
                            if (readableTexture.format == TextureFormat.RG16)
                            {
                                m_Strength[y * m_Size + x] = UnpackRG16ToFloat (texel.r, texel.g);
                            }
                            else
                            {
                                m_Strength[y * m_Size + x] = texel.a;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < m_Strength.Length; i++)
                        m_Strength[i] = 1.0F;
                }

                if (readableTexture != mask)
                    Object.DestroyImmediate (readableTexture);
            }
            else
            {
                m_Strength = new float[1];
                m_Strength[0] = 1.0F;
                m_Size = 1;
            }

            m_CurMask = pMask;
        }
    }
    public class BrushModeTexture : BrushMode
    {
        enum OpType
        {
            None,
            BakeTex,
        }
        private OpType opType = OpType.None;
        private List<Texture2D> m_BaseColorMap = new List<Texture2D> ();
        private Rect BaseColorRect;
        private List<Texture2D> m_PbsMap = new List<Texture2D> ();
        private Rect PbsRect;
        private int m_SelectedLayerIndex = 0;
        private BlendTexConfig m_blendTexConfig = null;
        private Texture2D m_LayerMaskEdit = null;
        private Texture2D m_LayerMask = null;
        private int m_LayerCount = 3;
        private bool editConfig = false;
        private bool editAlpha = false;

        private float lastMovePosX = -10000;
        private BrushRep m_BrushRep;
        private BrushConfig config;
        private Rect previewRect;
        private Vector2 pickupOffset;
        private bool isSelect = false;
        private Material previewMat;

        private bool bPreDictUV = true;
        private float RadiusOnRT = 0.0f;

     //   public bool isEditing = false;

        //VC
        private Mesh addMesh = null;
        private Color[] colorData = null;
        private Vector3[] vertices = null;
        private MeshRenderObject mro;
        
        private bool isPaintChange;
        public override void OnEnable ()
        {
            config = BrushConfig.instance;
            config.Init ();
            if (previewMat == null)
                previewMat = new Material (AssetsConfig.instance.PreviewTex);
            previewMat.SetFloat ("_Alpha", 0.5f);
            OnSelectionChanged ();
        }

        public override void OnSelectObjChange()
        {
            base.OnSelectObjChange();
            CheckHaveBake();
        }

        private void CheckHaveBake()
        {
            if (isPaintChange)
            {
                EditorUtility.DisplayDialog("警告", m_CurMaterial.name + " 的贴图尚未Bake，运行时不会生效，请Bake后在切换", "好的");
                isPaintChange = false;
            }
        }

        public override void OnDisable ()
        {
            CheckHaveBake();
            
            if (m_CurMaterial != null && m_LayerMask != null && config != null)
            {
                m_CurMaterial.SetTexture(config.blendTexID, m_LayerMask);
            }
            m_CurMaterial = null;
            
            if (config != null)
            {
                config.Uninit ();
            }
            if (previewMat == null)
            {
                UnityEngine.Object.DestroyImmediate (previewMat);
                previewMat = null;
            }
            if (mro != null)
            {
                mro.editingBrush = false;
                if (m_SelObject != null)
                {
                    m_SelObject.layer = mro.layer;
                }
            }
        }

        public override void OnPrePaint (RaycastHit hit)
        {
            if (m_LayerMaskEdit != null)
            {
                Undo.RegisterCompleteObjectUndo (m_LayerMaskEdit, "Paint LayerMask");
            }

            if (!config.vcMode && bPreDictUV)
                PredictUVMap (hit);
        }

        private void PredictUVMap (RaycastHit hit)
        {
            Vector3[] directions = new Vector3[4] { Vector3.up, Vector3.down, Vector3.right, Vector3.left };
            RadiusOnRT = 0.0f;
            float maxL = float.MaxValue;

            for (int i = 0; i < 4; ++i)
            {
                Vector3 center = hit.point;
                Vector3 tangent = GetTangent (hit.normal, directions[i]);

                Vector3 v = center + hit.normal + tangent * 0.5f;

                Ray testRay = new Ray (v, -hit.normal);

                RaycastHit testHit;
                Physics.Raycast (testRay, out testHit, float.MaxValue, BrushMode.LayerEditMask);

                if (testHit.transform == hit.transform)
                {
                    Vector2 deltaUV = hit.textureCoord - testHit.textureCoord;

                    float l = deltaUV.magnitude;
                    if (l < maxL) maxL = l;
                    //
                }
            }
            if(m_LayerMaskEdit != null)
            {
                RadiusOnRT = m_LayerMaskEdit.width * maxL * (config.Radius) * 0.1f;
            }
            

            // {
            //     Vector3 center = hit.point;
            //     Vector3 tangent = GetTangent (hit.normal, Vector3.up);

            //     Vector3 v = center + hit.normal + tangent * config.Radius * 0.5f * 0.01f;

            //     Ray testRay = new Ray (v, -hit.normal);

            //     RaycastHit testHit;
            //     Physics.Raycast (testRay, out testHit, float.MaxValue, BrushMode.LayerEditMask);

            //     if (testHit.transform == hit.transform)
            //     {
            //         Vector2 deltaUV = hit.textureCoord - testHit.textureCoord;

            //         float l = deltaUV.magnitude;
            //         RadiusOnRT = m_LayerMaskEdit.width * l;
            //     }
            // }
        }

        private Vector3 GetTangent (Vector3 normal, Vector3 dir)
        {
            Vector3 tangent = Vector3.Cross (normal, Vector3.forward);

            if (tangent.magnitude == 0)
            {
                tangent = Vector3.Cross (normal, dir);
            }

            return tangent;
        }

        public override void OnEndPaint ()
        {

        }
        private void EditMat ()
        {
            if (!isEditing)
            {
                m_CurMaterial.SetTexture(config.blendTexID, m_LayerMask);
            //    m_CurMaterial.DisableKeyword ("_EDITING");
                //if (mro != null)
                //{
                //    mro.editingBrush = false;
                //}
                //MeshCollider mc;
                //if (mro.TryGetComponent (out mc))
                //{
                //    mc.enabled = false;
                //}
            }
            else
            {
                m_CurMaterial.SetTexture(config.blendTexID, m_LayerMaskEdit);
            //    m_CurMaterial.EnableKeyword ("_EDITING");
                if (config.vcMode && mro != null)
                {
                    mro.editingBrush = true;
                    MeshCollider mc;
                    if (!mro.TryGetComponent (out mc))
                    {
                        mc = mro.gameObject.AddComponent<MeshCollider> ();
                    }
                    mc.enabled = true;
                }
            }
            if (config.vcMode)
            {
                m_CurMaterial.EnableKeyword ("_VCMODE_ON");
                m_CurMaterial.SetInt("_VCMode", 1);
            }
            else
            {
                m_CurMaterial.DisableKeyword ("_VCMODE_ON");
                m_CurMaterial.SetInt("_VCMode", 0);
            }
            if (m_LayerCount == 2)
            {
                m_CurMaterial.EnableKeyword ("_SPLAT_2X");
                m_CurMaterial.DisableKeyword ("_SPLAT_3X");
                m_CurMaterial.DisableKeyword ("_SPLAT_4X");
            }
            else if(m_LayerCount == 3)
            {
                m_CurMaterial.DisableKeyword ("_SPLAT_2X");
                m_CurMaterial.EnableKeyword ("_SPLAT_3X");
                m_CurMaterial.DisableKeyword ("_SPLAT_4X");
            }
            else
            {
                m_CurMaterial.DisableKeyword ("_SPLAT_2X");
                m_CurMaterial.DisableKeyword ("_SPLAT_3X");
                m_CurMaterial.EnableKeyword ("_SPLAT_4X");
            }
            m_CurMaterial.SetInt("_SPLAT", m_LayerCount - 1);
        }
        private void LoadOrCreateLayerMaskTex(bool create)
        {
            string assetPath;
            string dir = AssetsPath.GetAssetDir(m_CurMaterial, out assetPath);
            string blendConfigPath = string.Format("{0}/{1}_Blend.asset", dir, m_CurMaterial.name);
            m_blendTexConfig = AssetDatabase.LoadAssetAtPath<BlendTexConfig>(blendConfigPath);
            if (m_blendTexConfig == null && create)
            {
                m_blendTexConfig = ScriptableObject.CreateInstance<BlendTexConfig>();
                m_blendTexConfig = EditorCommon.CreateAsset<BlendTexConfig>(blendConfigPath, ".asset", m_blendTexConfig);
            }
            if (m_blendTexConfig != null)
            {
                string blendTex = string.Format("{0}/{1}_Blend_Editor.tga", dir, m_CurMaterial.name);
                if (create)
                {
                    m_LayerMaskEdit = EditorCommon.CreateAsset<Texture2D>(blendTex, ".tga", m_LayerMaskEdit);
                    m_blendTexConfig.editTex = m_LayerMaskEdit;
                    EditorCommon.SaveAsset(m_blendTexConfig);

                    SetTextureFormat(m_LayerMaskEdit);
                }
                else
                {
                    m_LayerMaskEdit = m_blendTexConfig.editTex;
                    m_LayerMask = m_blendTexConfig.runtimeTex;
                }
            }
        }

        private void SetTextureFormat(Texture2D tex)
        {
            if (tex == null) return;
            string path = AssetDatabase.GetAssetPath(tex);
            TextureImporter tImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            if (tImporter == null) return;

            tImporter.isReadable = true;
            tImporter.textureCompression = TextureImporterCompression.Uncompressed;
            tImporter.SaveAndReimport();
        }

        private void CreateLayerMaskTex (BlendTexSize width, BlendTexSize height)
        {
            int w = BrushConfig.blendTexSize[(int) width];
            int h = BrushConfig.blendTexSize[(int) height];
            if (m_LayerMaskEdit == null || m_LayerMaskEdit.width != w || m_LayerMaskEdit.height != h)
            {
                m_LayerMaskEdit = new Texture2D (w, h, TextureFormat.RGBA32, 0, true);
                for (int z = 0; z < h; ++z)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        m_LayerMaskEdit.SetPixel (x, z, new Color (1, 0, 0, 0));
                    }
                }

                LoadOrCreateLayerMaskTex (true);
                EditMat ();
            }
        }

        private void BakeAddMeshBindInfo (Mesh mesh, Mesh addMesh, string dir, string name)
        {
            if (mesh.vertexCount == addMesh.vertexCount)
            {
                string path = string.Format ("{0}/{1}_ext.bytes", dir, name);
                using (FileStream fs = new FileStream (path, FileMode.Create))
                {
                    BinaryWriter bw = new BinaryWriter (fs);
                    var vertices = mesh.vertices;
                    bw.Write (vertices.Length);
                    for (int i = 0; i < vertices.Length; ++i)
                    {
                        ref var pos = ref vertices[i];
                        int x = (int) (pos.x * 100);
                        int y = (int) (pos.y * 100);
                        int z = (int) (pos.z * 100);
                        bw.Write (x);
                        bw.Write (y);
                        bw.Write (z);
                    }
                }
                AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate);

            }
        }

        public override void DrawBrushSettings ()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(isEditing ? "绘制状态" : "非绘制状态", GUILayout.MaxWidth(160)))
            {
                isEditing = !isEditing;
              if (Selection.activeObject == null)
                {
                    isEditing = false;
                    EditorUtility.DisplayDialog("提示", "没有选择任何一个物体！", "确定");
                    return;
                }                 
                EditMat();
            }
            EditorGUILayout.LabelField("切换编辑状态！");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("切换编辑物体，按住C键同时点击选择物体！");
            EditorGUILayout.LabelField("不在编辑状态，如果选不中物体，按A键恢复！");
       
            EditorGUILayout.Space(20);
            if (!isEditing)
                return;

            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("EditConfig", GUILayout.MaxWidth (80)))
            {
                editConfig = !editConfig;
            }
            if (GUILayout.Button ("Save", GUILayout.MaxWidth (80)))
            {
                config.Save ();
                config.Init ();
            }
            EditorGUILayout.EndHorizontal ();
            if (editConfig)
                config.OnConfigGUI ();
            config.OnGUI ();

            EditorGUILayout.BeginHorizontal ();
        //    isEditing = m_CurMaterial.IsKeywordEnabled ("_EDITING");
     
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.ObjectField (m_LayerMaskEdit, typeof (Texture), false, GUILayout.MaxWidth (300));
            if (GUILayout.Button ("Create", GUILayout.MaxWidth (80)))
            {
                CreateLayerMaskTex (config.texWidth, config.texHeight);
            }
            if (config.vcMode)
            {
                if (GUILayout.Button ("Bake", GUILayout.MaxWidth (80)))
                {
                    if (mro != null && addMesh != null)
                    {
                        var mesh = mro.GetMesh ();
                        string assetPath;
                        string dir = AssetsPath.GetAssetDir (mesh, out assetPath);
                        string fbxname;
                        if (AssetsPath.GetFileName (assetPath, out fbxname))
                        {
                            addMesh.name = string.Format ("{0}_{1}_vc", fbxname, mesh.name);
                            string meshPath = string.Format ("{0}/{1}.asset", dir, addMesh.name);
                            BakeAddMeshBindInfo (mesh, addMesh, dir, addMesh.name);
                            string addmeshpath = AssetDatabase.GetAssetPath(addMesh);
                            CommonAssets.CreateAsset<Mesh> (meshPath, ".asset", addMesh);
                            FBXAssets.ProcessFbx (assetPath, null, EditorMessageType.Dialog, true);
                        }               
                    }
                }
            }
            else
            {
                if (m_LayerMaskEdit != null)
                {

                    if (GUILayout.Button ("Bake", GUILayout.MaxWidth (80)))
                    {
                        opType = OpType.BakeTex;
                    }
                }
            }

            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            EditorGUI.BeginChangeCheck ();
            EditorGUILayout.LabelField ("LayerCount", GUILayout.MaxWidth (100));
            m_LayerCount = EditorGUILayout.IntSlider ("", m_LayerCount, 2, 4, GUILayout.MaxWidth (300));
            if (EditorGUI.EndChangeCheck ())
            {
                m_CurMaterial.DisableKeyword("_SPLAT_1X");
                m_CurMaterial.DisableKeyword ("_SPLAT_2X");
                m_CurMaterial.DisableKeyword("_SPLAT_3X");
                m_CurMaterial.DisableKeyword("_SPLAT_4X");
                if (m_LayerCount == 2)
                {
                    m_CurMaterial.EnableKeyword ("_SPLAT_2X");
                }
                else if (m_LayerCount == 3)
                {
                    m_CurMaterial.EnableKeyword ("_SPLAT_3X");
                }
                else
                {
                    m_CurMaterial.EnableKeyword ("_SPLAT_4X");
                }
            }
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();

            config.OnBrushAlphaGUI ();
            editAlpha = config.weightNormal || config.orientedNormal;

            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.LabelField ("BaseColorMap");
            int oldSelectIndex = m_SelectedLayerIndex;
            m_SelectedLayerIndex = LayeredBrushLayout.ChannelField (m_SelectedLayerIndex, m_BaseColorMap, m_LayerCount, null, out BaseColorRect);
            if (oldSelectIndex != m_SelectedLayerIndex)
            {
                editAlpha = config.weightNormal = config.orientedNormal = false;
            }

            EditorGUILayout.LabelField ("PBSMap");
            LayeredBrushLayout.ChannelField (m_SelectedLayerIndex, m_PbsMap, m_LayerCount, null, out PbsRect);

            EditorGUILayout.LabelField ("TexList");
            config.OnTexListGUI ();
            OnEventProcess ();
        }

        #region textureMode
        Color NormalizeA (float[] color)
        {
            float totalAlpha = 0.0F;
            int alphaMaps = m_LayerCount;
            for (int a = 0; a < alphaMaps; a++)
            {
                totalAlpha += color[a];
            }

            if (totalAlpha > 1.0f)
            {
                for (int a = 0; a < alphaMaps; a++)
                {
                    color[a] /= totalAlpha;
                }
            }

            return new Color (color[0], color[1], color[2], color[3]);
        }
        Color Normalize (int splatIndex, float[] color)
        {
            float newAlpha = color[splatIndex];
            float totalAlphaOthers = 0.0F;
            int alphaMaps = m_LayerCount;
            for (int a = 0; a < alphaMaps; a++)
            {
                if (a != splatIndex)
                {
                    totalAlphaOthers += color[a];
                }
            }

            if (totalAlphaOthers > 0.01)
            {
                float adjust = (1.0F - newAlpha) / totalAlphaOthers;
                for (int a = 0; a < alphaMaps; a++)
                {
                    if (a != splatIndex)
                    {
                        color[a] *= adjust;
                    }
                }
            }
            else
            {
                for (int a = 0; a < alphaMaps; a++)
                {
                    color[a] = a == splatIndex ? 1.0f : 0.0f;
                }
            }

            return new Color (color[0], color[1], color[2], color[3]);
        }

        private void Draw (RaycastHit hit)
        {

            if (m_BrushRep == null)
            {
                m_BrushRep = new BrushRep ();
            }
            if (m_LayerMaskEdit == null) return;
            int nPaintSize = (int) config.Radius;

            int layerWidth = m_LayerMaskEdit.width;
            int layerHeight = m_LayerMaskEdit.height;

            int xCenter = Mathf.FloorToInt (hit.textureCoord.x * layerWidth);
            int yCenter = Mathf.FloorToInt (hit.textureCoord.y * layerHeight);

            //int intRadius = Mathf.RoundToInt (nPaintSize) / 2;
            int intRadius = RadiusOnRT > 0 ? Mathf.RoundToInt (RadiusOnRT) : Mathf.RoundToInt (nPaintSize) / 2;
            int intFraction = Mathf.RoundToInt (nPaintSize) % 2;

            int xmin = Mathf.Clamp (xCenter - intRadius, 0, layerWidth - 1);
            int ymin = Mathf.Clamp (yCenter - intRadius, 0, layerHeight - 1);

            int xmax = Mathf.Clamp (xCenter + intRadius + intFraction, 0, layerWidth);
            int ymax = Mathf.Clamp (yCenter + intRadius + intFraction, 0, layerHeight);

            int width = xmax - xmin;
            int height = ymax - ymin;

            //创建alpha数组m_Strength
            m_BrushRep.CreateFromBrush (config.BrushList[config.SelectedBrushIndex], intRadius * 2);

            //Vector2 size = new Vector2 (nPaintSize, nPaintSize) * 0.5f;
            Vector2 size = new Vector2 (intRadius, intRadius);
            float[] newColor = new float[4];
            if (config.PaintMode == PaintMode.Brush || config.PaintMode == PaintMode.Fill)
            {
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        int xBrushOffset = (xmin + x) - (xCenter - intRadius + intFraction);
                        int yBrushOffset = (ymin + y) - (yCenter - intRadius + intFraction);
                        float opa = config.PaintMode == PaintMode.Fill ? 1.0f : m_BrushRep.GetStrengthInt (xBrushOffset, yBrushOffset) * config.Strength;

                        Color c = m_LayerMaskEdit.GetPixel (xmin + x, ymin + y);
                        newColor[0] = c.r;
                        newColor[1] = c.g;
                        newColor[2] = c.b;
                        newColor[3] = c.a;

                        if (editAlpha)
                        {
                            newColor[3] = opa > 0 ? config.NormalStrength : newColor[3];
                        }
                        else
                        {
                            newColor[m_SelectedLayerIndex] = Mathf.Clamp01 (newColor[m_SelectedLayerIndex] += opa);
                        }

                        m_LayerMaskEdit.SetPixel (xmin + x, ymin + y, Normalize (m_SelectedLayerIndex, newColor));
                    }
                }
            }
            else if (config.PaintMode == PaintMode.Flood)
            {
                for (int y = 0; y < layerHeight; ++y)
                {
                    for (int x = 0; x < layerWidth; ++x)
                    {
                        int _x = (int) (layerWidth * hit.textureCoord.x - size.x) + x;
                        int _y = (int) (layerHeight * hit.textureCoord.y - size.y) + y;

                        _x = _x < 0 ? _x + layerWidth : (_x > layerWidth ? _x % layerWidth : _x);
                        _y = _y < 0 ? _x + layerHeight : (_y > layerHeight ? _y % layerHeight : _y);

                        Color c = m_LayerMaskEdit.GetPixel (_x, _y);
                        newColor[0] = c.r;
                        newColor[1] = c.g;
                        newColor[2] = c.b;
                        newColor[3] = c.a;
                        if (editAlpha)
                        {
                            newColor[3] = config.NormalStrength;
                        }
                        else
                        {
                            for (int j = 0; j < 3; ++j)
                            {
                                if (j == m_SelectedLayerIndex) newColor[j] = 1;
                                else newColor[j] = 0;
                            }
                        }

                        var newsplat = Normalize (m_SelectedLayerIndex, newColor);

                        m_LayerMaskEdit.SetPixel (_x, _y, newsplat);
                    }
                }
            }
            m_LayerMaskEdit.Apply (false);
            isPaintChange = true;
        }
        #endregion

        #region vc mode
        private void DrawVC (ref RaycastHit hit)
        {
            if (addMesh == null)
                return;
            float scale = 1.0f / Mathf.Abs (m_SelObject.transform.lossyScale.x);

            float bz = scale * config.Radius;
            bz *= bz;

            Vector3 point = hit.point;
            point = m_SelObject.transform.worldToLocalMatrix.MultiplyPoint3x4 (point);

            float falloff_mag = Mathf.Max ((config.Radius - config.Radius * config.Falloff), 0.00001f);

            int nVertexCount = colorData.Length;
            float[] newColor = new float[4];
            if (config.PaintMode == PaintMode.Brush || config.PaintMode == PaintMode.Fill)
            {
                for (int i = 0; i < nVertexCount; ++i)
                {
                    var p = vertices[i];
                    float x = point.x - p.x;
                    float y = point.y - p.y;
                    float z = point.z - p.z;
                    float dist = x * x + y * y + z * z;

                    if (dist < bz)
                    {
                        var color = colorData[i];
                        newColor[0] = color.r;
                        newColor[1] = color.g;
                        newColor[2] = color.b;
                        newColor[3] = color.a;
                        if (editAlpha)
                        {
                            newColor[3] = config.NormalStrength;
                        }
                        if (config.PaintMode == PaintMode.Brush)
                        {
                            if (!editAlpha)
                            {
                                float weight = Mathf.Clamp (config.curve.Evaluate (1f - Mathf.Clamp ((config.Radius - Mathf.Sqrt (dist)) / falloff_mag, 0f, 1f)), 0f, 1f);
                                if (m_SelectedLayerIndex < m_LayerCount)
                                {
                                    newColor[m_SelectedLayerIndex] = Mathf.Clamp01 (newColor[m_SelectedLayerIndex] += config.Strength * weight);
                                }
                            }
                            colorData[i] = NormalizeA (newColor);
                        }
                        else if (config.PaintMode == PaintMode.Fill)
                        {
                            if (!editAlpha)
                            {
                                if (m_SelectedLayerIndex < m_LayerCount)
                                {
                                    newColor[m_SelectedLayerIndex] = config.Strength;

                                }
                            }
                            colorData[i] = NormalizeA (newColor);
                        }
                    }
                }
            }
            else if (config.PaintMode == PaintMode.Flood)
            {
                for (int i = 0; i < nVertexCount; ++i)
                {
                    var color = colorData[i];
                    newColor[0] = color.r;
                    newColor[1] = color.g;
                    newColor[2] = color.b;
                    newColor[3] = color.a;
                    if (editAlpha)
                    {
                        newColor[3] = config.NormalStrength;
                    }
                    else
                    {
                        if (m_SelectedLayerIndex < m_LayerCount)
                        {
                            // newColor[m_SelectedLayerIndex] = config.Strength;
                            for (int j = 0; j < 4; ++j)
                            {
                                if (j == m_SelectedLayerIndex) newColor[j] = 1;
                                else newColor[j] = 0;
                            }
                        }
                    }

                    colorData[i] = NormalizeA (newColor);
                }
            }

            addMesh.colors = colorData;
            addMesh.UploadMeshData (false);
            mro.UpdateAddMesh();
        }

        #endregion
        public override void OnPaint (RaycastHit hit)
        {
            if (m_SupportsLayered && m_CurMaterial != null && hit.transform != null /*&& isEditing*/)
            {
                if (config.vcMode)
                {
                    DrawVC (ref hit);
                }
                else
                {
                    Draw (hit);
                }

            }
        }

        public override void Refresh ()
        {
            LoadOrCreateLayerMaskTex (false);
            // string assetPath;
            // string dir = AssetsPath.GetAssetDir (m_CurMaterial, out assetPath);
            // string blendTex = string.Format ("{0}/{1}_Blend_Editor.tga", dir, m_CurMaterial.name);
            // m_LayerMaskEdit = AssetDatabase.LoadAssetAtPath<Texture2D> (blendTex);
            // blendTex = string.Format ("{0}/{1}_Blend.tga", dir, m_CurMaterial.name);
            // m_LayerMask = AssetDatabase.LoadAssetAtPath<Texture2D> (blendTex);
            m_BaseColorMap.Clear ();
            m_PbsMap.Clear ();
            for (int i = 0; i < 4; ++i)
            {
                if (m_CurMaterial.HasProperty (config.BaseColorMapID[i]))
                {
                    m_BaseColorMap.Add (m_CurMaterial.GetTexture (config.BaseColorMapID[i]) as Texture2D);
                }
                if (m_CurMaterial.HasProperty (config.PbsMapID[i]))
                {
                    m_PbsMap.Add (m_CurMaterial.GetTexture (config.PbsMapID[i]) as Texture2D);
                }
            }
            if (m_CurMaterial.IsKeywordEnabled ("_SPLAT_3X"))
                m_LayerCount = 3;
            else if(m_CurMaterial.IsKeywordEnabled ("_SPLAT_4X"))
                m_LayerCount = 4;
            else
                m_LayerCount = 2;
            config.vcMode = m_CurMaterial.IsKeywordEnabled ("_VCMODE_ON");

            if (mro != null)
            {
                mro.editingBrush = false;
            }
            mro = null;
            if (m_SelObject.TryGetComponent(out mro))
            {
                var mesh = mro.GetMesh();
                if (mesh != null)
                {
                 //   Debug.LogError(mesh);
                    vertices = mesh.vertices;
                    addMesh = mro.additionalVertexStreamMesh;
                    if (addMesh == null)
                    {
                     //   Debug.LogError("addMesh == null:" + addMesh);
                        colorData = new Color[vertices.Length];
                        addMesh = new Mesh();
                        addMesh.name = "_AddMesh";
                        addMesh.vertices = vertices;
                        mro.additionalVertexStreamMesh = addMesh;

                    }
                    else
                    {
                   //     Debug.LogError(addMesh);
                        colorData = addMesh.colors;
                        if (colorData.Length != vertices.Length)
                        {
                            colorData = new Color[vertices.Length];
                        }
                    }
                    mro.UpdateAddMesh();
                    EditMat();
                }
            }
            else
            {
                vertices = null;
                colorData = null;
            }
        }

        public override void DrawGizmos (Event e)
        {
            RaycastHit hit = MainRay ();
            if (!m_SupportsLayered ||
                hit.transform == null ||
                m_SelObject == null ||
                m_SelObject.transform != hit.transform
            )
            {

                return;
            }
            if (!config.vcMode && m_LayerMaskEdit == null)
            {
                return;
            }
            if (isEditing)
            {
                Vector3 point = hit.point;
                Vector3 normal = hit.normal;

                Vector3 p = point; //matrix.MultiplyPoint3x4(point);
                Vector3 n = normal; //matrix.MultiplyVector(normal).normalized;

                float radius = config.Radius;
                if (!config.vcMode)
                {
                    //radius = config.Radius * 4 / (m_LayerMaskEdit.width * 1.0f);
                    radius = (config.Radius * 0.5f * 10) / 100.0f;
                }

                /// radius
                Handles.color = editAlpha? new Color (0.227f, 0.227f, 1.0f, 1.0f) : new Color (0.227f, 1.0f, 0.227f, 1.0f);
                Handles.DrawWireDisc (p, n, radius);

                if (config.vcMode)
                {
                    /// falloff
                    if (m_SelectedLayerIndex == 0)
                    {
                        Handles.color = new Color (1, 0, 0, config.Strength);
                    }
                    else if (m_SelectedLayerIndex == 1)
                    {
                        Handles.color = new Color (0, 1, 0, config.Strength);
                    }
                    else if (m_SelectedLayerIndex == 2)
                    {
                        Handles.color = new Color (0, 0, 1f, config.Strength);
                    }
                    Handles.DrawSolidDisc (p, n, radius * config.Falloff);

                }

                Handles.color = new Color (Mathf.Abs (n.x), Mathf.Abs (n.y), Mathf.Abs (n.z), 1f);
                Handles.DrawLine (p, p + n.normalized * HandleUtility.GetHandleSize (p));

                if (config.vcMode)
                {
                    Matrix4x4 matrix = hit.transform.localToWorldMatrix;
                    DrawVertexPoints (matrix, point);
                }
            }

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Space)
                {
               //     EditMat ();
                    SceneTool.DoRepaint ();
                }
                else if (e.keyCode == KeyCode.Alpha3)
                {
                    m_SelectedLayerIndex = 0;
                    SceneTool.DoRepaint ();
                }
                else if (e.keyCode == KeyCode.Alpha4)
                {
                    if (m_LayerCount == 2)
                    {
                        m_SelectedLayerIndex = 1;
                        SceneTool.DoRepaint ();
                    }
                }
                else if (e.keyCode == KeyCode.Alpha5)
                {
                    if (m_LayerCount == 3)
                    {
                        m_SelectedLayerIndex = 2;
                        SceneTool.DoRepaint ();
                    }
                }
                else if (e.keyCode == KeyCode.Alpha6)
                {
                    if (m_LayerCount == 4)
                    {
                        m_SelectedLayerIndex = 3;
                        SceneTool.DoRepaint ();
                    }
                }
                else if (e.keyCode == KeyCode.G)
                {
                    config.Strength -= 0.1f;
                    if (config.Strength < 0)
                    {
                        config.Strength = 0;
                    }
                }
                else if (e.keyCode == KeyCode.H)
                {
                    config.Strength += 0.1f;
                    if (config.Strength > 1)
                    {
                        config.Strength = 1;
                    }
                }
            }
            else if (e.type == EventType.MouseMove)
            {
                if (e.control)
                {
                    if (lastMovePosX > 0)
                    {
                        if (e.mousePosition.x > lastMovePosX)
                        {
                            config.Radius += config.RadiusInc;
                        }
                        else
                        {
                            config.Radius -= config.RadiusInc;
                            if (config.Radius < 0)
                                config.Radius = 0;
                        }
                        SceneTool.DoRepaint ();
                    }
                    lastMovePosX = e.mousePosition.x;
                }
                else
                {
                    lastMovePosX = -10000;
                }
            }
            SceneView.RepaintAll ();
        }
        void DrawVertexPoints (Matrix4x4 matrix, Vector3 point)
        {
            if (vertices != null)
            {
                float scale = 1.0f / Mathf.Abs (m_SelObject.transform.lossyScale.x);

                float bz = scale * config.Radius;
                bz *= bz;

                point = m_SelObject.transform.worldToLocalMatrix.MultiplyPoint3x4 (point);

                int nVertexCount = vertices.Length;
                for (int i = 0; i < nVertexCount; ++i)
                {
                    var p = vertices[i];
                    float x = point.x - p.x;
                    float y = point.y - p.y;
                    float z = point.z - p.z;
                    float dist = x * x + y * y + z * z;

                    if (dist < bz)
                    {
                        Handles.color = config.showVertexColor;
                        Handles.SphereHandleCap (0, matrix.MultiplyPoint (p), Quaternion.identity, config.showVertexSize * 0.02f, EventType.Repaint);
                    }
                }
            }

        }

        private bool DrapTexInSlot (Vector2 pos, ref Rect slotRect,
            List<Texture2D> texSlot, List<string> texID)
        {
            Rect rect = new Rect ();
            int index = LayeredBrushLayout.FindTexIndex (pos, slotRect, m_BaseColorMap, ref rect);
            if (index >= 0)
            {
                if (m_CurMaterial.HasProperty (texID[index]))
                {
                    var tex = config.GetTexture ();
                    m_CurMaterial.SetTexture (texID[index], tex);
                    texSlot[index] = tex;
                }

                return true;
            }
            return false;
        }
        private void OnEventProcess ()
        {
            var e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                bool leftMouse = e.button == 0;
                if (leftMouse)
                {
                    Rect pickupRect = new Rect ();
                    config.TestMousePos (e.mousePosition, ref pickupRect);
                    pickupOffset.x = e.mousePosition.x - pickupRect.xMin;
                    pickupOffset.y = e.mousePosition.y - pickupRect.yMax + 64;
                    isSelect = true;
                }
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (config.SelectedTexIndex >= 0 && isSelect)
                {
                    previewRect.xMin = e.mousePosition.x - pickupOffset.x;
                    previewRect.yMin = e.mousePosition.y - pickupOffset.y;
                    previewRect.xMax = previewRect.xMin + 64;
                    previewRect.yMax = previewRect.yMin + 64;

                    SceneTool.DoRepaint ();
                }

            }
            else if (e.type == EventType.MouseUp)
            {
                if (!DrapTexInSlot (e.mousePosition, ref BaseColorRect,
                        m_BaseColorMap, config.BaseColorMapID))
                {
                    DrapTexInSlot (e.mousePosition, ref PbsRect,
                        m_PbsMap, config.PbsMapID);
                }
                isSelect = false;
                SceneTool.DoRepaint ();
            }

            if (e.type == EventType.Repaint)
            {
                if (config.SelectedTexIndex >= 0 && isSelect)
                {
                    EditorGUI.DrawPreviewTexture (previewRect, Texture2D.whiteTexture, previewMat, ScaleMode.ScaleAndCrop);
                }
            }
        }

        public override void Update ()
        {
            switch (opType)
            {
                case OpType.BakeTex:
                    {
                        RenderTexture rt = new RenderTexture (m_LayerMaskEdit.width, m_LayerMaskEdit.height, 0,
                            RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                        {
                            name = "blendRt",
                            hideFlags = HideFlags.DontSave,
                            filterMode = FilterMode.Point,
                            wrapMode = TextureWrapMode.Clamp,
                            anisoLevel = 0,
                            autoGenerateMips = false,
                            useMipMap = false
                        };
                        rt.Create ();
                        TextureAssets.BlitTex2RT (m_LayerMaskEdit, rt);
                        string assetPath;
                        string dir = AssetsPath.GetAssetDir (m_LayerMaskEdit, out assetPath);
                        string texName = m_LayerMaskEdit.name.Replace ("_Editor", "");
                        string blendTex = string.Format ("{0}/{1}.tga", dir, texName);
                        EditorCommon.CreateAsset<Texture2D> (blendTex, ".tga", rt);
                        rt.Release ();
                        m_LayerMask = AssetDatabase.LoadAssetAtPath<Texture2D> (blendTex);
                        m_blendTexConfig.runtimeTex = m_LayerMask;
                        EditorCommon.SaveAsset (m_blendTexConfig);
                        string _maskedit = AssetDatabase.GetAssetPath(m_LayerMaskEdit);
                        if (!string.IsNullOrEmpty(_maskedit))
                        {
                            byte[] bytes = m_LayerMaskEdit.EncodeToTGA();
                            File.WriteAllBytes(_maskedit, bytes);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                        isPaintChange = false;
                    }
                    break;
            }
            opType = OpType.None;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public enum BlendTexSize
    {
        _64x64,
        _128x128,
        _256x256,
        _512x512,
        _1024x1024,
        _2048x2048
    }

    public enum PaintMode
    {
        Brush,
        Fill,
        Flood
    }

    public enum NormalMode
    {
        Weight,
        Oriented,
    }

    [System.Serializable]
    public class FolderTexture
    {
        public string folder;
        public bool show = true;
        [System.NonSerialized]
        public Rect texListRect;
        public List<Texture2D> TextureList = new List<Texture2D> ();
    }

    [System.Serializable]
    public class BrushConfig : AssetBaseConifg<BrushConfig>
    {
        public bool vcMode = true;
        public float RadiusInc = 0.01f;
        //texture
        public float Radius = 10;
        public float Strength = 1.0f;

        //vc
        public float Falloff = 0.5f;
        public float brushRadiusMin = 0.001f;
        public float brushRadiusMax = 5.0f;
        public AnimationCurve curve = new AnimationCurve (new Keyframe (0.0f, 1.0f), new Keyframe (1.0f, 0.0f, -3.0f, -3.0f));
        public float showVertexSize = 1.0f;
        public Color showVertexColor = Color.white;

        public List<string> BaseColorMapID = new List<string> (
            new string[]
            {
                "_MainTex",
                "_MainTex1",
                "_MainTex2",
                "_MainTex3",
            });
        public List<string> PbsMapID = new List<string> (
            new string[]
            {
                "_ProcedureTex0",
                "_ProcedureTex1",
                "_ProcedureTex2",
                "_ProcedureTex3",
            });
        public List<string> BrushTexture = new List<string> (
            new string[]
            {
                "Brush/Brush_0",
                "Brush/Brush_1",
                "Brush/Brush_2",
                "Brush/Brush_3",
            });
        public string blendTexID = "_BlendTex";
        [System.NonSerialized]
        public List<Texture2D> BrushList = new List<Texture2D> ();
        [System.NonSerialized]
        public int SelectedBrushIndex = 0;
        private Material preViewBrushMat = null;
        public List<string> TextureFolder = new List<string> ();
        public FolderTexture customList = new FolderTexture ()
        {
            folder = "Custom"
        };
        [System.NonSerialized]
        public List<FolderTexture> TextureList = new List<FolderTexture> ();
        [System.NonSerialized]
        public int SelectedFolderIndex = 0;
        [System.NonSerialized]
        public int SelectedTexIndex = 0;

        public BlendTexSize texWidth = BlendTexSize._512x512;
        public BlendTexSize texHeight = BlendTexSize._512x512;
        public PaintMode PaintMode = PaintMode.Brush;
        public static int[] blendTexSize = new int[] { 64, 128, 256, 512, 1024, 2048 };
        private GUIContent m_GCBrushSize = new GUIContent ("Brush Size", "Brush Size.");
        private GUIContent m_GCRadiusInc = new GUIContent ("Brush Size Inc", "Brush Size.");
        private GUIContent m_GCStrength = new GUIContent ("Strength(G/H)", "Strength: The effectiveness of this brush.");
        private GUIContent m_GCRadiusMin = new GUIContent ("Brush Radius Min", "The minimum value the brush radius slider can access");
        private GUIContent m_GCRadiusMax = new GUIContent ("Brush Radius Max", "The maximum value the brush radius slider can access");
        private GUIContent m_GCFalloff = new GUIContent ("Inner Radius", "Inner Radius: The distance from the center of a brush at which the strength begins to linearly taper to 0.  This value is normalized, 1 means the entire brush gets full strength, 0 means the very center point of a brush is full strength and the edges are 0.\n\nShortcut: 'Shift + Mouse Wheel'");
        private GUIContent m_GCFalloffCurve = new GUIContent ("Falloff Curve", "Falloff: Sets the Falloff Curve.");

        private GUIContent[] modeIcons = new GUIContent[]
        {
            new GUIContent ("Brush", "Brush"), new GUIContent ("Fill", "Fill"), new GUIContent ("Flood", "Flood")
        };

        public bool weightNormal = false;
        public bool orientedNormal = false;
        public GUIStyleState m_GUIStyleButton1 = new GUIStyleState();
        public GUIStyleState m_GUIStyleButton2 = new GUIStyleState();
        public GUIStyle m_AreaButtonStyle = null;
        public float NormalStrength = 1.0f;

        public void OnGUI ()
        {
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("VCMode", GUILayout.MaxWidth (80));
            vcMode = EditorGUILayout.Toggle ("", vcMode, GUILayout.MaxWidth (80));
            EditorGUILayout.EndHorizontal ();
            RadiusInc = LayeredBrushLayout.FloatFieldWithSlider (m_GCRadiusInc, RadiusInc, 0.01f, 1);
            if (vcMode)
            {
                using (new EditorGUI.IndentLevelScope ())
                {
                    using (new GUILayout.VerticalScope ())
                    {
                        brushRadiusMin = LayeredBrushLayout.FloatField (m_GCRadiusMin, brushRadiusMin);
                        brushRadiusMin = Mathf.Clamp (brushRadiusMin, .0001f, Mathf.Infinity);

                        brushRadiusMax = LayeredBrushLayout.FloatField (m_GCRadiusMax, brushRadiusMax);
                        brushRadiusMax = Mathf.Clamp (brushRadiusMax, brushRadiusMin + .001f, Mathf.Infinity);
                    }
                }
                Radius = LayeredBrushLayout.FloatFieldWithSlider (m_GCBrushSize, Radius, brushRadiusMin, brushRadiusMax);
                Falloff = LayeredBrushLayout.FloatFieldWithSlider (m_GCFalloff, Falloff, 0f, 1f);
                Strength = LayeredBrushLayout.FloatFieldWithSlider (m_GCStrength, Strength, 0f, 1f);
                using (new GUILayout.HorizontalScope ())
                {
                    EditorGUILayout.LabelField (m_GCFalloffCurve, GUILayout.Width (100));
                    EditorGUI.BeginChangeCheck ();
                    curve = EditorGUILayout.CurveField (curve, GUILayout.MinHeight (22));
                    if (EditorGUI.EndChangeCheck ())
                    {
                        ModifyCurve ();
                    }
                }

                EditorGUILayout.Space ();
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField ("VertexDebug", GUILayout.Width (100));
                showVertexSize = EditorGUILayout.Slider (showVertexSize, 0.2f, 10);
                showVertexColor = EditorGUILayout.ColorField (showVertexColor, GUILayout.Width (40));
                EditorGUILayout.EndHorizontal ();
            }
            else
            {
                Radius = LayeredBrushLayout.IntFieldWithSlider (m_GCBrushSize, (int) Radius, 1, 100);
                Strength = LayeredBrushLayout.FloatFieldWithSlider (m_GCStrength, Strength, 0.0f, 1.0f);
                Rect rect;
                SelectedBrushIndex = LayeredBrushLayout.ChannelField (SelectedBrushIndex, BrushList, BrushList.Count, preViewBrushMat, out rect);

            }
            PaintMode = (PaintMode) GUILayout.Toolbar ((int) PaintMode, modeIcons);

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Width", GUILayout.MaxWidth (80));
            texWidth = (BlendTexSize) EditorGUILayout.EnumPopup ("", texWidth, GUILayout.MaxWidth (80));
            EditorGUILayout.LabelField ("Height", GUILayout.MaxWidth (80));
            texHeight = (BlendTexSize) EditorGUILayout.EnumPopup ("", texHeight, GUILayout.MaxWidth (80));
            EditorGUILayout.EndHorizontal ();
        }

        private void OnStrListGUI (List<string> strList, string name, bool folderStr = false)
        {
            if (GUILayout.Button (name, GUILayout.MaxWidth (100)))
            {
                strList.Add ("");
            }
            int removeIndex = -1;
            for (int i = 0; i < strList.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal ();
                strList[i] = EditorGUILayout.TextField ("", strList[i], GUILayout.MaxWidth (200));
                if (folderStr)
                {
                    EditorGUI.BeginChangeCheck ();
                    Texture asset = null;
                    asset = EditorGUILayout.ObjectField ("", asset, typeof (Texture), false, GUILayout.MaxWidth (50)) as Texture;
                    if (EditorGUI.EndChangeCheck ())
                    {
                        strList[i] = AssetDatabase.GetAssetPath (asset);
                    }
                }
                if (GUILayout.Button ("Delete", GUILayout.MaxWidth (80)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal ();
            }
            if (removeIndex >= 0)
            {
                strList.RemoveAt (removeIndex);
            }
            GUILayout.Space (8);
        }
        public void OnConfigGUI ()
        {
            OnStrListGUI (BrushTexture, "AddBrush");
            OnStrListGUI (TextureFolder, "AddFolder", true);
            OnStrListGUI (BaseColorMapID, "BaseKey");
            OnStrListGUI (PbsMapID, "PbsKey");
            EditorGUILayout.BeginHorizontal ();
            blendTexID = EditorGUILayout.TextField ("", blendTexID, GUILayout.MaxWidth (200));
            EditorGUILayout.EndHorizontal ();

        }

        private void OnTexListGUI (FolderTexture ft)
        {
            EditorGUILayout.BeginHorizontal ();
            ft.show = EditorGUILayout.Foldout (ft.show, ft.folder);
            EditorGUILayout.EndHorizontal ();
            if (ft.show)
                LayeredBrushLayout.ChannelField2 (SelectedTexIndex, ft.TextureList, ft.TextureList.Count, null, out ft.texListRect);
        }

        public void OnTexListGUI ()
        {
            EditorGUILayout.BeginHorizontal ();
            Texture2D tex = GetTexture ();
            EditorGUILayout.LabelField (tex != null?tex.name: "", GUILayout.MaxWidth (200));
            EditorGUILayout.ObjectField (tex, typeof (Texture2D), false, GUILayout.MaxWidth (300));
            EditorGUILayout.EndHorizontal ();

            OnTexListGUI (customList);

            for (int i = 0; i < TextureList.Count; ++i)
            {
                var ft = TextureList[i];
                OnTexListGUI (ft);
            }

        }

        public void OnBrushAlphaGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("法线叠加:  ", GUILayout.MaxWidth(80));

            m_AreaButtonStyle.normal = weightNormal ? m_GUIStyleButton1 : m_GUIStyleButton2;
            if (GUILayout.Button("  权重叠加", m_AreaButtonStyle))
            {
                weightNormal = !weightNormal;
                if (weightNormal)
                {
                    //CachedStrength = Strength;
                    NormalStrength = 1;
                    orientedNormal = false;
                }
            }

            m_AreaButtonStyle.normal = orientedNormal ? m_GUIStyleButton1 : m_GUIStyleButton2;
            if (GUILayout.Button("叠加到第一层", m_AreaButtonStyle))
            {
                orientedNormal = !orientedNormal;
                if (orientedNormal)
                {
                    NormalStrength = 0;
                    weightNormal = false;
                }
            }

            EditorGUILayout.EndHorizontal();
        }


        private bool TestMousePos (ref Vector2 mousePos, ref Rect selectRect, FolderTexture ft)
        {
            SelectedTexIndex = LayeredBrushLayout.FindTexIndex (mousePos, ft.texListRect, ft.TextureList, ref selectRect);
            if (SelectedTexIndex >= 0)
            {
                return true;
            }
            return false;
        }
        public void TestMousePos (Vector2 mousePos, ref Rect selectRect)
        {
            if (TestMousePos (ref mousePos, ref selectRect, customList))
            {
                SelectedFolderIndex = 10000;
            }
            else
            {
                for (int i = 0; i < TextureList.Count; ++i)
                {
                    var tf = TextureList[i];
                    if (TestMousePos (ref mousePos, ref selectRect, tf))
                    {
                        if (SelectedTexIndex >= 0)
                        {
                            SelectedFolderIndex = i;
                            return;
                        }
                    }
                }
            }

        }
        private Texture2D GetTexture (FolderTexture ft)
        {
            if (SelectedTexIndex >= 0 && SelectedTexIndex < ft.TextureList.Count)
                return ft.TextureList[SelectedTexIndex];
            return null;
        }

        public Texture2D GetTexture ()
        {
            if (SelectedFolderIndex == 10000)
            {
                return GetTexture (customList);
            }
            if (SelectedFolderIndex >= 0 && SelectedFolderIndex < TextureList.Count)
            {
                var ft = TextureList[SelectedFolderIndex];
                return GetTexture (ft);
            }
            return null;
        }

        private bool approx (float lhs, float rhs)
        {
            return Mathf.Abs (lhs - rhs) < .0001f;
        }
        private void ModifyCurve ()
        {
            Keyframe[] keys = curve.keys;

            if ((approx (keys[0].time, 0f) && approx (keys[0].value, 0f) && approx (keys[1].time, 1f) && approx (keys[1].value, 1f)))
            {
                Keyframe[] rev = new Keyframe[keys.Length];

                for (int i = 0; i < keys.Length; i++)
                {
                    rev[keys.Length - i - 1] = new Keyframe (1f - keys[i].time, keys[i].value, -keys[i].outTangent, -keys[i].inTangent);
                }

                curve = new AnimationCurve (rev);
            }
        }
        private bool EnumTexFolder (Texture2D tex, TextureImporter importer, string path, System.Object context)
        {
            var tf = context as FolderTexture;
            tf.TextureList.Add (tex);
            return false;
        }

        public void Init ()
        {
            BrushList.Clear ();
            for (int i = 0; i < BrushTexture.Count; ++i)
            {
                BrushList.Add (IconUtility.GetIcon (BrushTexture[i]));
            }
            if (preViewBrushMat == null)
            {
                preViewBrushMat = new Material (Shader.Find ("Hidden/Custom/Editor/Preview_Brush"));
            }
            TextureList.Clear ();
            CommonAssets.enumTex2D.cb = EnumTexFolder;
            for (int i = 0; i < TextureFolder.Count; ++i)
            {
                string folder = TextureFolder[i];
                var tf = new FolderTexture ();
                tf.folder = folder;
                TextureList.Add (tf);
                CommonAssets.EnumAsset<Texture2D> (CommonAssets.enumTex2D, "", folder, tf, false, true);
            }

            m_GUIStyleButton1.background = EditorGUIUtility.Load("Tools/editor_btn_0.png") as Texture2D;
            m_GUIStyleButton1.textColor = Color.white;
            m_GUIStyleButton2.background = EditorGUIUtility.Load("Tools/editor_btn_1.png") as Texture2D;
            m_GUIStyleButton2.textColor = Color.white;

            m_AreaButtonStyle = new GUIStyle();
            m_AreaButtonStyle.padding.left = 4;
            m_AreaButtonStyle.padding.top = 4;
            m_AreaButtonStyle.padding.right = 4;
            m_AreaButtonStyle.padding.bottom = 4;
            m_AreaButtonStyle.margin = new RectOffset(4, 4, 4, 4);
            m_AreaButtonStyle.fixedHeight = 22;
            m_AreaButtonStyle.fixedWidth = 80;
        }
        public void Uninit ()
        {
            if (preViewBrushMat != null)
            {
                UnityEngine.Object.DestroyImmediate (preViewBrushMat);
                preViewBrushMat = null;
            }
        }
    }
}
// // Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static CFEngine.Editor.MaterialShaderAssets;

namespace CFEngine.Editor
{
    public class PBSShaderGUI : ShaderGUI
    {

        static Shader rampShader;
        static Material rampMaterial;
        static Color[] gradientColors = new Color[32];
        static float[] gradientColorTimes = new float[32];

        public class RampPropertyInstance
        {
            public Texture2D tempTexture;
            public Gradient gradient = new Gradient ();
            public bool OnEdit;
            public Material material;
            public String MatHash = "";
            public RenderTexture RT = new RenderTexture (256, 4, 24);
            public String TexPath = "";
        }
        private static List<RampPropertyInstance> rpiList = new List<RampPropertyInstance> ();

        public class ShaderPropertyInstance
        {
            public ShaderFeature shaderFeature;
            public MaterialProperty property;
        }

        public class ShaderPropertyBlockInstance
        {
            public ShaderFeatureBlock block;
            public List<ShaderPropertyInstance> spiList = new List<ShaderPropertyInstance> ();
        }
        public class ShaderPropertyGropuInstance
        {
            public ShaderFeatureGroup sfg;
            public List<ShaderPropertyBlockInstance> spbList = new List<ShaderPropertyBlockInstance> ();
        }

        internal delegate void LightShadowGUI ();
        public class CustomShaderInfo
        {
            public List<ShaderPropertyGropuInstance> spgList = new List<ShaderPropertyGropuInstance> ();
            internal LightShadowGUI lightShadowGui;
        }

        public struct DrawPropertyContext
        {
            public Material material;
            // public MaterialEditor materialEditor;
            public HashSet<string> hasDepency;
            public ShaderPropertyInstance spi;
            public bool dirty;
        }

        private static class Styles
        {
            public static string renderingMode = "Rendering Mode";
            public static string rimText = "Rim";
            public static string skinText = "Skin";
            public static string debugText = "Debug";
            public static string debugMode = "Debug Mode";
            public static readonly string[] blendNames = Enum.GetNames (typeof (BlendMode));

        }

        static Color[] mipMapColors = new Color[]
        {
            Color.black, //mipmap 0 black
            Color.red, //mipmap 1 red
            new Color (1.0f, 0.5f, 0.0f, 1.0f), //mipmap 2 orange
            Color.yellow, //mipmap 3 yellow
            Color.green, //mipmap 4 green
            Color.cyan, //mipmap 5 cyan
            Color.blue, //mipmap 6 blue
            Color.magenta, //mipmap 7 magenta
            Color.gray, //mipmap 8 gray
            Color.white, //mipmap 9 white
        };

        public delegate void DrawFun (ref DrawPropertyContext context);
        Dictionary<string, DrawFun> customFunc = new Dictionary<string, DrawFun> ();
        static DrawFun[] drawPropertyFun = new DrawFun[]
        {
            null,
            DrawTexFun,
            DrawColorFun,
            DrawVectorFun,
            DrawKeyWordFun,
            DrawCustomFun,
            DrawValueToggleFun,
            DrawRenderQueueFun,
            DrawGradientFun
        };
        CustomShaderInfo csi;
        HashSet<string> hasDepency = new HashSet<string> ();
        // protected MaterialEditor m_MaterialEditor;
        // MaterialProperty baseColorMp;
        protected Material m_Material;
        private string matLocation = "";
        private static bool m_DisableKeywordModify = false;
        DebugData dd;
        bool m_FirstTimeApply = true;
        List<string> matKeywords = new List<string> ();
        string lastkeywords = "";
        string paramName = "";
        private MaterialPropertyBlock mpb;
        private Renderer r;
        private DebugRenderData drd;
        private bool addUndoRefresh = false;
        
        static Dictionary<string, CustomShaderInfo> shaderInfoCache = new Dictionary<string, CustomShaderInfo> ();
        private bool FillCustomShaderInfo (MaterialProperty[] props, CustomShaderInfo csi, string shaderName)
        {
            var sc = ShaderConfig.instance;
            csi.spgList.Clear ();
            for (int i = 0; i < sc.shaderGUI.configRefs.Count; ++i)
            {
                var config = sc.shaderGUI.configRefs[i].config;
                if (config != null &&
                    (config.shader != null && config.shader.name == shaderName))
                {
                    switch (config.lsType)
                    {
                        case LightShadowType.Scene:
                            {
                                csi.lightShadowGui = SceneLightShadowGUI;
                            }
                            break;
                        case LightShadowType.Role:
                            {
                                csi.lightShadowGui = RoleLightShadowGUI;
                            }
                            break;
                        default:
                            csi.lightShadowGui = null;
                            break;
                    }

                    for (int j = 0; j < config.shaderFeatures.Count; ++j)
                    {
                        var sfi = config.shaderFeatures[j];
                        var sfg = sfi.sfg;
                        if (sfg != null)
                        {
                            var spgi = new ShaderPropertyGropuInstance ()
                            {
                            sfg = sfg,
                            };
                            csi.spgList.Add (spgi);
                            for (int k = 0; k < sfg.shaderFeatureBlocks.Count; ++k)
                            {
                                var block = sfg.shaderFeatureBlocks[k];
                                ShaderPropertyBlockInstance spbi = null;
                                for (int ii = 0; ii < block.shaderFeatures.Count; ++ii)
                                {
                                    var sf = block.shaderFeatures[ii];
                                    sf.key = string.Format ("{0}_{1}",
                                        sfg.groupName, sf.name);
                                    if (sfi.shaderFeatures.Contains (sf.key))
                                    {
                                        if (spbi == null)
                                        {
                                            spbi = new ShaderPropertyBlockInstance ()
                                            {
                                            block = block,
                                            };
                                            spgi.spbList.Add (spbi);
                                        }
                                        ShaderPropertyInstance spi = new ShaderPropertyInstance ()
                                        {
                                            shaderFeature = sf,
                                        };
                                        //spi.property = FindProperty (sf.propertyName, props, false);
                                        //RefreshDepency (sf, spi.property);
                                        spbi.spiList.Add (spi);
                                    }
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private void RefreshDepency (ShaderFeature sf, MaterialProperty property)
        {
            bool lastEnable = hasDepency.Contains (sf.key);
            bool enable = false;
            if (sf.type == ShaderPropertyType.Keyword)
            {
                enable = m_Material.IsKeywordEnabled (sf.propertyName);
            }
            else if (sf.type == ShaderPropertyType.ValueToggle && property != null)
            {
                var scp = sf.customProperty[0];
                Vector4 value = property.vectorValue;
                if (property.type == MaterialProperty.PropType.Color)
                {
                    value = property.colorValue;
                }
                else if (property.type == MaterialProperty.PropType.Float ||
                    property.type == MaterialProperty.PropType.Range)
                {
                    value.x = property.floatValue;
                }
                var cmpFun = ShaderCustomProperty.cmpFun[(int) scp.valueCmpType];
                int index = scp.index >= 0 ? scp.index : 0;
                enable = cmpFun (value[index], scp.thresholdValue);
            }
            if (lastEnable != enable)
            {
                if (enable)
                {
                    hasDepency.Add (sf.key);
                }
                else
                {
                    hasDepency.Remove (sf.key);
                }
            }
        }

        private void RefreshDep (MaterialProperty[] props, string[] shaderKeywords)
        {
            hasDepency.Clear ();
            if (csi != null)
            {
                for (int i = 0; i < csi.spgList.Count; ++i)
                {
                    var spg = csi.spgList[i];
                    for (int j = 0; j < spg.spbList.Count; ++j)
                    {
                        var spb = spg.spbList[j];
                        for (int k = 0; k < spb.spiList.Count; ++k)
                        {
                            var spi = spb.spiList[k];
                            var sf = spi.shaderFeature;
                            if (sf != null)
                            {
                                if (sf.type == ShaderPropertyType.Keyword ||
                                    sf.type == ShaderPropertyType.ValueToggle)
                                {
                                    RefreshDepency (sf, spi.property);
                                }
                            }
                        }

                    }
                }
            }
        }

        private void RefreshProperty (MaterialProperty[] props)
        {
            if (csi != null)
            {
                for (int i = 0; i < csi.spgList.Count; ++i)
                {
                    var spg = csi.spgList[i];
                    for (int j = 0; j < spg.spbList.Count; ++j)
                    {
                        var spb = spg.spbList[j];
                        for (int k = 0; k < spb.spiList.Count; ++k)
                        {
                            var spi = spb.spiList[k];
                            var sf = spi.shaderFeature;
                            if (sf != null)
                            {
                                spi.property = FindProperty (sf.propertyName, props, false);
                            }
                        }

                    }
                }
            }
        }
        protected void FindProperties (MaterialProperty[] props, bool force = false)
        {
            matLocation = AssetDatabase.GetAssetPath (m_Material);
            Shader shader = m_Material.shader;
            csi = null;
            if (force || !shaderInfoCache.TryGetValue (shader.name, out csi))
            {
                hasDepency.Clear ();
                CustomShaderInfo newCsi = new CustomShaderInfo ();
                if (FillCustomShaderInfo (props, newCsi, shader.name))
                {
                    shaderInfoCache[shader.name] = newCsi;
                    csi = newCsi;
                }
            }
            mpb = null;
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                if (go.TryGetComponent (out r))
                {
                    go.TryGetComponent (out drd);
                    if (r.HasPropertyBlock ())
                    {
                        mpb = new MaterialPropertyBlock ();
                        r.GetPropertyBlock (mpb);
                    }
                }
            }
            RefreshProperty (props);
            RefreshDep (props, m_Material.shaderKeywords);
            customFunc.Clear ();
            customFunc.Add ("BlendMode", BlendModePopup);
            customFunc.Add ("Debug", DoDebugArea);
            AssetsConfig.RefreshShaderDebugNames (false);
            // GameObject go = Selection.activeGameObject;
            // if (go != null)
            //     dd = go.GetComponent<DebugData> ();
        }

        public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader)
        {
            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (m_Material != null)
            {
                if (material.HasProperty ("_MainTex"))
                {
                    m_Material.SetTexture ("_MainTex", material.GetTexture ("_MainTex"));
                }
                if (material.HasProperty ("_MainTex"))
                {
                    m_Material.SetTexture ("_MainTex", material.GetTexture ("_MainTex"));
                }
                if (material.HasProperty ("_BumpMap"))
                {
                    m_Material.SetTexture ("_ProcedureTex0", material.GetTexture ("_BumpMap"));
                }
            }

            base.AssignNewShaderToMaterial (material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains ("Legacy Shaders/"))
            {
                MaterialShaderAssets.SetupMaterialWithBlendMode (material, MaterialShaderAssets.GetBlendMode (material));
                return;
            }

            MaterialChanged (material);
        }

        public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
        {
            OnGUI (materialEditor.target as Material, props);
            if (!addUndoRefresh)
            {
                Undo.undoRedoPerformed += LazyUndoRedoRefresh;
                addUndoRefresh = true;
            }
        }
        public void Reset ()
        {
            m_FirstTimeApply = true;
            addUndoRefresh = false;
        }
        public void OnGUI (Material material, MaterialProperty[] props, bool disableKeywordModify = false)
        {
            m_Material = material;
            m_DisableKeywordModify = disableKeywordModify;
            if (m_FirstTimeApply)
            {
                FindProperties (props);
                MaterialChanged (m_Material);
                m_FirstTimeApply = false;
            }
            ShaderPropertiesGUI (props);
        }
        private static void DrawGroupBegin (string name, bool bold = false)
        {
            if (!string.IsNullOrEmpty (name))
            {
                EditorGUI.indentLevel++;
                if (bold)
                    EditorGUILayout.LabelField (name, EditorStyles.boldLabel);
                else
                    EditorGUILayout.LabelField (name);
            }

        }
        private static void DrawGroupEnd (string name)
        {
            if (!string.IsNullOrEmpty (name))
            {
                EditorGUI.indentLevel--;
            }
        }
        private static void RegisterPropertyChangeUndo (string label, Material mat)
        {
            Undo.RecordObject (mat, "Modify " + label + " of " + mat.name);
        }
        private bool NeedShow (string groupName, string blockName, ShaderFeature sf)
        {
            if (!sf.HasFlag (ShaderFeature.Flag_Hide))
            {
                bool show = true;

                if (sf.dependencyPropertys != null &&
                    sf.dependencyPropertys.dependencyShaderProperty.Count > 0)
                {
                    bool hasFeature = sf.dependencyPropertys.dependencyType == DependencyType.And?true : false;
                    for (int k = 0; k < sf.dependencyPropertys.dependencyShaderProperty.Count; ++k)
                    {
                        string featureName = sf.dependencyPropertys.dependencyShaderProperty[k];
                        featureName = string.Format ("{0}_{1}", groupName, featureName);
                        if (sf.dependencyPropertys.dependencyType == DependencyType.Or)
                        {
                            hasFeature |= hasDepency.Contains (featureName);
                            if (hasFeature)
                                break;
                        }
                        else if (sf.dependencyPropertys.dependencyType == DependencyType.And)
                        {
                            hasFeature &= hasDepency.Contains (featureName);
                            if (!hasFeature)
                                break;
                        }
                    }
                    if (sf.dependencyPropertys.isNor)
                    {
                        hasFeature = !hasFeature;
                    }
                    show = hasFeature;
                }
                return show;
            }
            return false;
        }

        private void LazyUndoRedoRefresh()
        {
            EditorCommon.needRefreshShaderGui = true;
        }
        
        protected virtual void ShaderPropertiesGUI (MaterialProperty[] props)
        {
            EditorGUIUtility.labelWidth = 0f;

            EditorGUI.BeginChangeCheck ();
            {
                EditorGUILayout.BeginHorizontal ();
                if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (100)) ||
                    EditorCommon.needRefreshShaderGui)
                {
                    if (csi != null)
                    {
                        FindProperties (props, true);
                    }
                    EditorCommon.needRefreshShaderGui = false;
                }
                if (!string.IsNullOrEmpty (matLocation))
                {
                    if (GUILayout.Button ("Save", GUILayout.MaxWidth (100)))
                    {
                        MaterialShaderAssets.ClearMat (m_Material);
                        CommonAssets.SaveAsset (m_Material);
                    }
                }
                if (GUILayout.Button ("Analyze", GUILayout.MaxWidth (100)))
                {
                    MaterialShaderAssets.ScanMaterial (m_Material);
                }
                if (GUILayout.Button ("CopyMat", GUILayout.MaxWidth (100)))
                {
                    copyMat = m_Material;
                }
                if (copyMat != null)
                {
                    if (GUILayout.Button ("PasteMat", GUILayout.MaxWidth (100)))
                    {
                        CopyTo (m_Material);
                        EditorCommon.needRefreshShaderGui = true;
                    }
                }
                if (GUILayout.Button ("Reset", GUILayout.MaxWidth (100)))
                {
                    if (m_Material.HasProperty (ShaderManager._Param))
                        m_Material.SetVector (ShaderManager._Param, Vector4.zero);
                    if (m_Material.HasProperty (ShaderManager._ShaderKeyColor))
                        m_Material.SetVector (ShaderManager._ShaderKeyColor, Vector4.one);
                    if (r != null)
                    {
                        r.SetPropertyBlock (null);
                    }
                }

                EditorGUILayout.EndHorizontal ();

                m_Material.renderQueue = EditorGUILayout.IntSlider("Queue", m_Material.renderQueue, 2000, 4000);

                EditorGUILayout.LabelField("Shader Pass");
                EditorGUI.indentLevel++;
                for (int i = 0; i < m_Material.passCount; i++)
                {
                    string passName = m_Material.GetPassName(i);
                    EditorGUI.BeginChangeCheck();
                    bool passEnabled = EditorGUILayout.Toggle(passName, m_Material.GetShaderPassEnabled(passName));
                    if (EditorGUI.EndChangeCheck())
                        m_Material.SetShaderPassEnabled(passName, passEnabled);
                }
                EditorGUI.indentLevel--;

                if (csi != null)
                {
                    EditorGUI.BeginChangeCheck ();
                    DrawPropertyContext context = new DrawPropertyContext ();
                    {
                        for (int i = 0; i < csi.spgList.Count; ++i)
                        {
                            var spg = csi.spgList[i];
                            EditorGUILayout.LabelField (spg.sfg.groupName, EditorStyles.boldLabel);
                            for (int j = 0; j < spg.spbList.Count; ++j)
                            {
                                var spb = spg.spbList[j];
                                DrawGroupBegin (spb.block.bundleName);
                                EditorGUI.indentLevel++;
                                for (int k = 0; k < spb.spiList.Count; ++k)
                                {
                                    var spi = spb.spiList[k];
                                    var sf = spi.shaderFeature;
                                    if (NeedShow (spg.sfg.groupName, spb.block.bundleName, sf))
                                    {
                                        if (sf.type == ShaderPropertyType.CustomFun)
                                        {
                                            DrawFun drawFun = null;
                                            if (customFunc.TryGetValue (sf.name, out drawFun))
                                            {
                                                context.material = m_Material;
                                                context.hasDepency = hasDepency;
                                                context.spi = spi;
                                                drawFun (ref context);
                                            }
                                        }
                                        else
                                        {
                                            var drawFun = drawPropertyFun[(int) sf.type];
                                            if (drawFun != null)
                                            {
                                                context.material = m_Material;
                                                context.hasDepency = hasDepency;
                                                context.spi = spi;
                                                drawFun (ref context);
                                            }
                                        }
                                    }
                                }
                                EditorGUI.indentLevel--;
                                DrawGroupEnd (spb.block.bundleName);
                            }
                        }
                    }
                    if (context.dirty)
                    {
                        RefreshDep (props, m_Material.shaderKeywords);
                    }
                    if (EditorGUI.EndChangeCheck ())
                    {
                        if (EngineContext.IsRunning && drd != null && r != null)
                        {
                            drd.Refresh (r);
                        }
                    }
                }

                string[] keywords = m_Material.shaderKeywords;
                if (matKeywords.Count != keywords.Length)
                {
                    lastkeywords = "KeyWords:";
                    matKeywords.Clear();
                    int index = 0;
                    for (int i = 0; i < keywords.Length; ++i)
                    {
                        matKeywords.Add(keywords[i]);
                        if (!string.IsNullOrEmpty(keywords[i]))
                        {
                            lastkeywords += keywords[i];
                            if (i != keywords.Length - 1)
                            {
                                if (index < 3)
                                {
                                    lastkeywords += "|";
                                    index++;

                                }
                                else
                                {
                                    lastkeywords += "\r\n";
                                    index = 0;
                                }

                            }
                        }
                    }
                }
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextArea(lastkeywords);
                EditorGUI.EndDisabledGroup();
                if (csi != null && csi.lightShadowGui != null)
                {
                    csi.lightShadowGui ();
                }
            }
            if (EditorGUI.EndChangeCheck ())
            {

            }
        }

        private void SceneLightShadowGUI ()
        {
            if (m_Material.HasProperty (ShaderManager._Param))
            {
                Vector4 param = Vector4.zero;
                bool readOnly = false;
                if (mpb != null)
                {
                    param = mpb.GetVector (ShaderManager._Param);
                    readOnly = true;
                }
                else
                {
                    param = m_Material.GetVector (ShaderManager._Param);
                }

                param.x = EditorGUILayout.Toggle ("DynamicLight", param.x > 0.5f) ? 1 : 0;
                param.y = EditorGUILayout.Toggle ("LightMapEnable", param.y > 0.5f) ? 1 : 0;
                param.z = EditorGUILayout.Toggle ("ExtraShadowEnable", param.z > 0.5f) ? 1 : 0;
                if (!readOnly)
                {
                    m_Material.SetVector (ShaderManager._Param, param);
                }
            }
            paramName = EditorGUILayout.TextField (paramName);
            if (m_Material.HasProperty (paramName))
            {
                Vector4 param = Vector4.zero;
                if (mpb != null)
                {
                    param = mpb.GetVector (paramName);
                }
                else
                {
                    param = m_Material.GetVector (paramName);
                }
                EditorGUILayout.Vector4Field ("", param);
            }
        }

        private void RoleLightShadowGUI ()
        {
            if (m_Material.HasProperty (ShaderManager._Param))
            {
                Vector4 param = Vector4.zero;
                bool readOnly = false;
                if (mpb != null)
                {
                    param = mpb.GetVector (ShaderManager._Param);
                    readOnly = true;
                }
                else
                {
                    param = m_Material.GetVector (ShaderManager._Param);
                }

                param.x = EditorGUILayout.Toggle ("DynamicLight", param.x > 0.5f) ? 1 : 0;
                param.w = EditorGUILayout.Toggle ("SelfShadowEnable", param.w > 0.5f) ? 1 : 0;
                if (!readOnly)
                {
                    m_Material.SetVector (ShaderManager._Param, param);
                }
            }
            paramName = EditorGUILayout.TextField (paramName);
            if (m_Material.HasProperty (paramName))
            {
                Vector4 param = Vector4.zero;
                if (mpb != null)
                {
                    param = mpb.GetVector (paramName);
                }
                else
                {
                    param = m_Material.GetVector (paramName);
                }
                EditorGUILayout.Vector4Field ("", param);
            }
        }

        static GUIContent sharedContent = null;
        static Color ColorProperty (MaterialProperty prop, string label, bool showAlpha, ref Color defaultColor)
        {
            if (sharedContent == null)
                sharedContent = new GUIContent (label);
            sharedContent.text = label;
            EditorGUILayout.BeginHorizontal ();
            EditorGUI.BeginChangeCheck ();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            bool isHDR = ((prop.flags & MaterialProperty.PropFlags.HDR) != 0);
            float alpha = prop.colorValue.a;
            Color newValue = EditorGUILayout.ColorField (sharedContent, prop.colorValue, true, showAlpha, isHDR);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck ())
            {

                if (!showAlpha)
                {
                    newValue.a = alpha;
                }
                prop.colorValue = newValue;
            }
            if (GUILayout.Button ("R", GUILayout.MaxWidth (20)))
            {
                if (!showAlpha)
                {
                    newValue.a = alpha;
                }
                prop.colorValue = defaultColor;
            }
            EditorGUILayout.EndHorizontal ();
            return prop.colorValue;
        }

        static Vector4 VectorProperty (MaterialProperty prop, string label)
        {
            if (sharedContent == null)
                sharedContent = new GUIContent (label);
            sharedContent.text = label;
            EditorGUI.BeginChangeCheck ();
            EditorGUI.showMixedValue = prop.hasMixedValue;

            // We want to make room for the field in case it's drawn on the same line as the label
            // Set label width to default width (zero) temporarily
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0f;

            Vector4 newValue = EditorGUILayout.Vector4Field (label, prop.vectorValue);

            EditorGUIUtility.labelWidth = oldLabelWidth;

            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck ())
                prop.vectorValue = newValue;

            return prop.vectorValue;
        }

        static float GetDefaultPropertyHeight (MaterialProperty prop)
        {
            return 64 + 6f;
        }
        static Rect GetPropertyRect (MaterialProperty prop, string label)
        {
            float handlerHeight = 0f;

            return EditorGUILayout.GetControlRect (true, handlerHeight + GetDefaultPropertyHeight (prop), EditorStyles.layerMaskField);
        }
        static internal System.Type GetTextureTypeFromDimension (TextureDimension dim)
        {
            switch (dim)
            {
                case TextureDimension.Tex2D:
                    return typeof (Texture); // common use case is RenderTextures too, so return base class
                case TextureDimension.Cube:
                    return typeof (Cubemap);
                case TextureDimension.Tex3D:
                    return typeof (Texture3D);
                case TextureDimension.Tex2DArray:
                    return typeof (Texture2DArray);
                case TextureDimension.CubeArray:
                    return typeof (CubemapArray);
                case TextureDimension.Any:
                    return typeof (Texture);
                default:
                    return null; // Unknown, None etc.
            }
        }
        // static UnityEngine.Object TextureValidator(UnityEngine.Object[] references, 
        //     System.Type objType, SerializedProperty property, 
        //     EditorGUI.ObjectFieldValidatorOptions options)
        // {
        //     foreach (Object i in references)
        //     {
        //         var t = i as Texture;
        //         if (t)
        //         {
        //             if (t.dimension == m_DesiredTexdim || m_DesiredTexdim == UnityEngine.Rendering.TextureDimension.Any)
        //                 return t;
        //         }
        //     }
        //     return null;
        // }
        static Texture TexturePropertyBody (Rect position, MaterialProperty prop)
        {
            if (prop.type != MaterialProperty.PropType.Texture)
            {
                throw new ArgumentException (string.Format ("The MaterialProperty '{0}' should be of type 'Texture' (its type is '{1})'", prop.name, prop.type));
            }

            System.Type t = GetTextureTypeFromDimension (prop.textureDimension);

            // Why are we disabling the GUI in Animation Mode here?
            // If it's because object references can't be changed, shouldn't it be done in ObjectField instead?
            bool wasEnabled = GUI.enabled;

            EditorGUI.BeginChangeCheck ();
            if ((prop.flags & MaterialProperty.PropFlags.PerRendererData) != 0)
                GUI.enabled = false;
            if ((prop.flags & MaterialProperty.PropFlags.NonModifiableTextureData) != 0)
                GUI.enabled = false;

            EditorGUI.showMixedValue = prop.hasMixedValue;
            // int controlID = GUIUtility.GetControlID (12354, FocusType.Keyboard);
            var newValue = EditorGUI.ObjectField (position, prop.textureValue, t, false) as Texture;
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck ())
                prop.textureValue = newValue;

            GUI.enabled = wasEnabled;

            return prop.textureValue;
        }
        static Texture TextureProperty (MaterialProperty prop, string label)
        {
            Rect r = GetPropertyRect (prop, label);
            if (sharedContent == null)
                sharedContent = new GUIContent (label);
            sharedContent.text = label;
            // sharedContent.tooltip = tooltip;
            // Label
            EditorGUI.PrefixLabel (r, sharedContent);

            // Texture slot
            r.height = 64;
            Rect texPos = r;
            texPos.xMin = texPos.xMax - 80;
            Texture value = TexturePropertyBody (texPos, prop);
            return value;
        }

        private static void DrawTexFun (ref DrawPropertyContext context)
        {
            if (context.spi.property != null)
            {
                if (context.spi.shaderFeature.HasFlag (ShaderFeature.Flag_ReadOnly))
                {
                    EditorGUILayout.ObjectField (context.spi.property.displayName, context.spi.property.textureValue, typeof (Texture), false);
                }
                else
                {
                    EditorGUI.BeginChangeCheck ();
                    TextureProperty (context.spi.property, context.spi.shaderFeature.name);
                    //context.materialEditor.TextureProperty (context.spi.property, context.spi.property.displayName, false);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        RegisterPropertyChangeUndo (context.spi.shaderFeature.name, context.material);
                    }

                    ////////////////////////////////////////////////////////////////////---------Ramp图在编辑器中制作-----------////////////////////////////////////////////////////////////////////////////////////////

                    if (context.spi.shaderFeature.HasFlag (ShaderFeature.Flag_IsRamp))
                    {
                        if (rpiList.Count == 0)
                        {
                            RampPropertyInstance rpi = new RampPropertyInstance ();
                            rpi.material = context.material;
                            rpi.TexPath = AssetDatabase.GetAssetPath (context.material.GetTexture ("_ProcedureTex2"));
                            rpiList.Add (rpi);

                        }
                        else
                        {
                            bool needAddList = true;
                            for (int i = 0; i < rpiList.Count; i++)
                            {
                                if (rpiList[i].material.Equals (context.material))
                                {
                                    needAddList = false;
                                }
                            }
                            if (needAddList)
                            {
                                RampPropertyInstance tempRPI = new RampPropertyInstance ();
                                tempRPI.material = context.material;
                                rpiList.Add (tempRPI);
                            }

                        }

                        GUIStyle gUIStyle = new GUIStyle ("AC Button");
                        // gUIStyle.border = new RectOffset(12, 12, 12, 12);
                        gUIStyle.margin = new RectOffset (0, 0, 3, 0);
                        gUIStyle.padding = new RectOffset (0, 0, 0, 0);
                        //gUIStyle.overflow = new RectOffset(5, 5,5, 5);
                        gUIStyle.fixedWidth = 50;
                        gUIStyle.fixedHeight = 19;
                        // gUIStyle.alignment = TextAnchor.MiddleCenter;
                        // gUIStyle.fontSize = 12;
                        // gUIStyle.fontStyle=FontStyle.Bold;
                        // gUIStyle.richText = true;

                        GUIStyle gUIStyle_on = new GUIStyle (gUIStyle);
                        // GUIStyle gUIStylenode_3_on=new GUIStyle("AC Button");
                        gUIStyle_on.normal.background = gUIStyle.focused.background;

                        foreach (RampPropertyInstance rpi in rpiList)
                        {
                            if (context.material.Equals (rpi.material))
                            {

                                if (!rpi.OnEdit)
                                {
                                    GUILayout.BeginHorizontal ();
                                    EditorGUILayout.Space ();
                                    if (GUILayout.Button ("Edit", GUILayout.Width (50)))
                                    {
                                        rpi.OnEdit = true;
                                        rampShader = Shader.Find ("Hidden/CreatRampTex");
                                        if (rampShader != null)
                                        {
                                            rampMaterial = new Material (rampShader);
                                            rpi.tempTexture = context.spi.property.textureValue as Texture2D;
                                            rpi.TexPath = AssetDatabase.GetAssetPath (rpi.tempTexture);
                                            GradientPalette.Get(rpi.TexPath, out rpi.gradient);

                                            for (int i = 0; i < rpi.gradient.colorKeys.Length; i++)
                                            {
                                                gradientColors[i] = rpi.gradient.colorKeys[i].color;
                                                gradientColorTimes[i] = rpi.gradient.colorKeys[i].time;
                                            }
                                            if (rampMaterial != null)
                                            {
                                                rampMaterial.SetFloatArray ("_gradientColorTimes", gradientColorTimes);
                                                rampMaterial.SetColorArray ("_gradientColors", gradientColors);
                                                rampMaterial.SetInt ("_gradientColorsLength", rpi.gradient.colorKeys.Length);
                                                Graphics.Blit (null, rpi.RT, rampMaterial,0);
                                            }
                                        }

                                    }
                                    GUILayout.EndHorizontal ();

                                }
                                else
                                {

                                    context.spi.property.textureValue = rpi.RT;
                                    GUILayout.BeginHorizontal ();
                                    EditorGUILayout.Space ();
                                    if (GUILayout.Button ("Edit", gUIStyle_on))
                                    {
                                        rpi.OnEdit = false;
                                        context.spi.property.textureValue = rpi.tempTexture;
                                    }
                                    EditorGUILayout.EndHorizontal ();

                                    GUIStyle guiStyleGroup = new GUIStyle ("MeTransitionSelect");
                                    // guiStyleGroup.border = new RectOffset(12, 12, 12, 12);
                                    guiStyleGroup.margin = new RectOffset (0, 0, 0, 0);
                                    guiStyleGroup.padding = new RectOffset (0, 0, 0, 0);
                                    guiStyleGroup.overflow = new RectOffset (-25, 1, -3, 60);
                                    //guiStyleGroup.fixedWidth=50;
                                    guiStyleGroup.fixedHeight = 5;

                                    GUILayout.Box ("", guiStyleGroup);
                                    EditorGUI.BeginChangeCheck ();
                                    GUILayout.BeginHorizontal ();
                                    rpi.gradient = EditorGUILayout.GradientField ("CustomGradient", rpi.gradient, GUILayout.Height (35));

                                    if (EditorGUI.EndChangeCheck ())
                                    {

                                        for (int i = 0; i < rpi.gradient.colorKeys.Length; i++)
                                        {
                                            gradientColors[i] = rpi.gradient.colorKeys[i].color;
                                            gradientColorTimes[i] = rpi.gradient.colorKeys[i].time;
                                        }
                                        if (rampMaterial != null)
                                        {
                                            rampMaterial.SetFloatArray ("_gradientColorTimes", gradientColorTimes);
                                            rampMaterial.SetColorArray ("_gradientColors", gradientColors);
                                            rampMaterial.SetInt ("_gradientColorsLength", rpi.gradient.colorKeys.Length);
                                            Graphics.Blit (null, rpi.RT, rampMaterial,0);
                                        }
                                    }
                                    if (GUILayout.Button ("Save", GUILayout.Width (40)))
                                    {
                                        String DirectoryAndFileName = "";

                                        String DefaultDiretory = "Assets/BundleRes/Character/RampTextures/";
                                        DirectoryAndFileName = rpi.TexPath.Split ('.') [0];
                                        if (!DirectoryAndFileName.Contains ("/") && !DirectoryAndFileName.Contains ("\\"))
                                        {
                                            DirectoryAndFileName = DefaultDiretory + DirectoryAndFileName;
                                        }
                                        if (DirectoryAndFileName.EndsWith ("/") || DirectoryAndFileName.EndsWith ("\\"))
                                        {
                                            DirectoryAndFileName = DirectoryAndFileName + "Temp_ramp";
                                        }
                                        else
                                        {
                                            if (!DirectoryAndFileName.EndsWith ("_ramp"))
                                            {
                                                DirectoryAndFileName = DirectoryAndFileName + "_ramp";
                                            }

                                        }

                                        if (!Directory.Exists (DefaultDiretory))
                                        {
                                            Directory.CreateDirectory (DefaultDiretory);
                                        }
                                        String TexPath = DirectoryAndFileName + ".tga";

                                        RenderTexture tempRT=new RenderTexture (256, 4, 24);
                                        if (rampMaterial != null)
                                        {
                                        //rampMat.SetTexture("_MainTex",rpi.RT);
                                        Graphics.Blit (rpi.RT, tempRT, rampMaterial,1);
                                        }
                                        
                                        Texture2D rampT = CreateFrom (tempRT);
                                        var bytes = rampT.EncodeToTGA ();
                                        System.IO.File.WriteAllBytes (TexPath, bytes);
                                        AssetDatabase.Refresh ();

                                        GradientPalette.Set(TexPath, rpi.gradient);

                                        TextureImporter import = AssetImporter.GetAtPath (TexPath) as TextureImporter;
                                        import.sRGBTexture = true;
                                        import.mipmapEnabled = false;
                                        import.wrapMode = TextureWrapMode.Clamp;

                                        TextureImporterPlatformSettings TPS_PC = import.GetPlatformTextureSettings ("PC");
                                        TextureImporterPlatformSettings TPS_iPhone = import.GetPlatformTextureSettings ("iPhone");
                                        TextureImporterPlatformSettings TPS_Android = import.GetPlatformTextureSettings ("Android");

                                        TPS_PC.format = TextureImporterFormat.RGB24;
                                        TPS_iPhone.format = TextureImporterFormat.RGB24;
                                        TPS_Android.format = TextureImporterFormat.RGB24;
                                        TPS_PC.overridden = true;
                                        TPS_iPhone.overridden = true;
                                        TPS_Android.overridden = true;
                                        import.SetPlatformTextureSettings (TPS_PC);
                                        import.SetPlatformTextureSettings (TPS_iPhone);
                                        import.SetPlatformTextureSettings (TPS_Android);

                                        import.SaveAndReimport ();

                                        Texture RampTex = AssetDatabase.LoadAssetAtPath<Texture> (TexPath);
                                        

                                        AssetDatabase.Refresh ();
                                        EditorGUIUtility.PingObject (RampTex);

                                    }

                                    GUILayout.EndHorizontal ();

                                    GUILayout.BeginHorizontal ();
                                    rpi.TexPath = EditorGUILayout.TextField ("BakeTexturePath:", rpi.TexPath);
                                    GUILayout.EndHorizontal ();

                                }
                            }
                        }
                    }
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
            }

        }
        public static Texture2D CreateFrom (RenderTexture renderTexture)
        {
            Texture2D texture2D = new Texture2D (renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            var previous = RenderTexture.active;
            RenderTexture.active = renderTexture;

            texture2D.ReadPixels (new Rect (0, 0, renderTexture.width, renderTexture.height), 0, 0);

            RenderTexture.active = previous;

            texture2D.Apply ();

            return texture2D;
        }

        private static void DrawColorFun (ref DrawPropertyContext context)
        {
            if (context.spi.property != null)
            {
                if (context.spi.shaderFeature.HasFlag (ShaderFeature.Flag_ReadOnly))
                {
                    EditorGUILayout.ColorField (context.spi.shaderFeature.name, context.spi.property.colorValue);
                }
                else
                {
                    EditorGUI.BeginChangeCheck ();
                    ColorProperty (context.spi.property, context.spi.shaderFeature.name, context.spi.shaderFeature.HasFlag (ShaderFeature.Flag_ShowAlpha), ref context.spi.shaderFeature.defaultColor);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        RegisterPropertyChangeUndo (context.spi.shaderFeature.name, context.material);
                    }
                }
            }
        }

        private static void DrawVectorFun (ref DrawPropertyContext context)
        {
            if (context.spi.property != null)
            {
                if (context.spi.shaderFeature.HasFlag (ShaderFeature.Flag_ReadOnly))
                {
                    EditorGUILayout.Vector4Field (context.spi.shaderFeature.name, context.spi.property.vectorValue);
                }
                else
                {
                    EditorGUI.BeginChangeCheck ();
                    VectorProperty (context.spi.property, context.spi.shaderFeature.name);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        RegisterPropertyChangeUndo (context.spi.shaderFeature.name, context.material);
                    }
                }
            }
        }

        private static void DrawGradientFun (ref DrawPropertyContext context)
        {
            if (context.spi.property != null)
            {
                if (context.spi.shaderFeature.HasFlag (ShaderFeature.Flag_ReadOnly))
                {
                    Gradient gradient = new Gradient ();
                    EditorGUILayout.GradientField (gradient);
                }
            }
        }

        private static void DrawKeyWordFun (ref DrawPropertyContext context)
        {
            bool isEnable = context.material.IsKeywordEnabled (context.spi.shaderFeature.propertyName);
            if (context.spi.shaderFeature.HasFlag (ShaderFeature.Flag_ReadOnly) || m_DisableKeywordModify)
            {
                EditorGUILayout.Toggle (context.spi.shaderFeature.name, isEnable);
            }
            else
            {

                EditorGUI.BeginChangeCheck ();
                bool enable = EditorGUILayout.Toggle (context.spi.shaderFeature.name, isEnable);
                if (EditorGUI.EndChangeCheck ())
                {
                    if (isEnable != enable)
                    {
                        SetKeyword (context.material, context.spi.shaderFeature.propertyName, enable);
                    }
                    if (enable && !context.hasDepency.Contains (context.spi.shaderFeature.name))
                        context.hasDepency.Add (context.spi.shaderFeature.name);
                    else if (!enable && context.hasDepency.Contains (context.spi.shaderFeature.name))
                        context.hasDepency.Remove (context.spi.shaderFeature.name);
                    context.dirty = true;
                    RegisterPropertyChangeUndo (context.spi.shaderFeature.name, context.material);
                }
            }
        }

        private static void DrawCustomFun (ref DrawPropertyContext context)
        {
            if (context.spi.property != null)
            {
                if (context.spi.shaderFeature.HasFlag (ShaderFeature.Flag_ReadOnly))
                {
                    EditorGUILayout.Vector4Field (context.spi.property.displayName, context.spi.property.vectorValue);
                }
                else
                {
                    EditorGUI.BeginChangeCheck ();
                    CustomPropertyDrawer.OnGUI (context.spi.property, context.material, context.spi.shaderFeature);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        RegisterPropertyChangeUndo (context.spi.property.displayName, context.material);
                    }
                }
            }
        }
        private static void DrawValueToggleFun (ref DrawPropertyContext context)
        {
            if (context.spi.property != null)
            {
                EditorGUI.BeginChangeCheck ();
                CustomPropertyDrawer.OnToggleValueGUI (
                    context.spi.property,
                    context.material,
                    context.spi.shaderFeature,
                    context.spi.shaderFeature.HasFlag (ShaderFeature.Flag_ReadOnly));
                if (EditorGUI.EndChangeCheck ())
                {
                    RegisterPropertyChangeUndo (context.spi.property.displayName, context.material);
                    context.dirty = true;
                }
            }
        }

        private static void DrawRenderQueueFun (ref DrawPropertyContext context)
        {
            if (context.spi.shaderFeature != null)
            {
                if (context.spi.shaderFeature.HasFlag (ShaderFeature.Flag_ReadOnly))
                {
                    EditorGUILayout.IntField (context.spi.shaderFeature.name, context.material.renderQueue);
                }
                else
                {
                    EditorGUI.BeginChangeCheck ();
                    int renderQueue = EditorGUILayout.IntField (context.spi.shaderFeature.name, context.material.renderQueue);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        renderQueue = Mathf.Clamp (renderQueue, -1, 5000);
                        context.material.renderQueue = renderQueue;
                        RegisterPropertyChangeUndo (context.spi.property.displayName, context.material);
                    }
                }
            }
        }

        protected void BlendModePopup (ref DrawPropertyContext context)
        {
            BlendMode blendMode = MaterialShaderAssets.GetBlendMode (m_Material);

            EditorGUI.BeginChangeCheck ();
            int index = MaterialShaderAssets.BlendMode2Index (blendMode);
            index = EditorGUILayout.Popup (Styles.renderingMode, index, Styles.blendNames);

            if (EditorGUI.EndChangeCheck ())
            {
                var mode = MaterialShaderAssets.Index2BlendMode (index);
                if (mode != blendMode)
                {
                    MaterialShaderAssets.SetupMaterialWithBlendMode (m_Material, mode);
                }
            }

        }

        protected void DoDebugArea (ref DrawPropertyContext context)
        {
            var debugNames = AssetsConfig.shaderDebugContext.debugNames;
            if (debugNames != null)
            {
                if (context.spi.property != null)
                {
                    //GUILayout.Label (Styles.debugText, EditorStyles.boldLabel);
                    EditorGUI.BeginChangeCheck ();
                    var mode = context.spi.property.floatValue;
                    mode = EditorGUILayout.Popup (Styles.debugMode, (int) mode, debugNames);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        RegisterPropertyChangeUndo ("Debug Mode", m_Material);
                        context.spi.property.floatValue = mode;
                    }
                    string debugName = debugNames[(int) mode];
                    if (debugName == "CubeMipmap")
                    {
                        GUILayout.BeginHorizontal ();
                        int swatchSize = 22;
                        int viewWidth = (int) EditorGUIUtility.currentViewWidth - 12;
                        int swatchesPerRow = viewWidth / (swatchSize + 4);
                        swatchSize += (viewWidth % (swatchSize + 4)) / swatchesPerRow;
                        for (int i = 0; i < mipMapColors.Length; ++i)
                        {
                            GUI.backgroundColor = mipMapColors[i].linear;
                            GUILayout.Button (i.ToString (),
                                GUILayout.MinWidth (swatchSize),
                                GUILayout.MaxWidth (swatchSize),
                                GUILayout.MinHeight (swatchSize),
                                GUILayout.MaxHeight (swatchSize));
                        }
                        GUI.backgroundColor = Color.white;
                        GUILayout.EndHorizontal ();
                    }
                }
                EditorGUI.BeginChangeCheck ();
                int globalDebugMode = EditorGUILayout.Popup ("Global Debug Mode", (int) GlobalContex.globalDebugMode, debugNames);
                if (EditorGUI.EndChangeCheck ())
                {
                    RegisterPropertyChangeUndo ("Global Debug Mode", m_Material);
                    GlobalContex.globalDebugMode = globalDebugMode;
                }
            }
        }

        public static void SetupMaterialWithDebugMode (Material material, float debugMode)
        {
            material.SetFloat ("_DebugMode", (int) debugMode);
        }

        protected void MaterialChanged (Material material)
        {
            MaterialShaderAssets.SetupMaterialWithBlendMode (material, MaterialShaderAssets.GetBlendMode (material), false);
            if (material.HasProperty ("_DebugMode"))
                SetupMaterialWithDebugMode (material, material.GetFloat ("_DebugMode"));
        }

        static void SetKeyword (Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword (keyword);
            else
                m.DisableKeyword (keyword);
        }

        ///copy paste mat
        public static Material copyMat = null;
        private static List<ShaderValue> shaderValue = new List<ShaderValue> ();
        public static BlendMode blendMode = BlendMode.Opaque;
        public static void CopyTo (Material newMat)
        {
            if (newMat == copyMat)
            {
                Debug.LogError ("Error: the same material.");
                return;
            }
            if (newMat.shader != copyMat.shader)
            {
                Debug.LogError ("Error: different shader.");
                return;
            }

            //read
            Shader shader = copyMat.shader;
            int count = ShaderUtil.GetPropertyCount (shader);
            shaderValue.Clear ();
            for (int i = 0; i < count; ++i)
            {
                ShaderValue sv = null;
                string name = ShaderUtil.GetPropertyName (shader, i);
                ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType (shader, i);
                switch (type)
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                        sv = new ShaderColorValue (name, type, copyMat);
                        break;
                    case ShaderUtil.ShaderPropertyType.Vector:
                        sv = new ShaderVectorValue (name, type, copyMat);
                        break;
                    case ShaderUtil.ShaderPropertyType.Float:
                        sv = new ShaderFloatValue (name, type, copyMat);
                        break;
                    case ShaderUtil.ShaderPropertyType.Range:
                        sv = new ShaderFloatValue (name, type, copyMat);
                        break;
                    case ShaderUtil.ShaderPropertyType.TexEnv:
                        sv = new ShaderTexValue (name, type, copyMat);
                        break;
                }
                shaderValue.Add (sv);
            }
            List<string> keywords = copyMat.shaderKeywords.ToList ();
            blendMode = GetBlendMode (copyMat);

            //write
            for (int i = 0; i < count; ++i)
                shaderValue[i].SetValue (newMat);
            for (int i = 0; i < newMat.shaderKeywords.Length; i++)
            {
                string str = newMat.shaderKeywords[i];
                if (keywords.Contains (str))
                    newMat.EnableKeyword (str);
                else
                    newMat.DisableKeyword (str);
            }
            for (int i = 0; i < keywords.Count; i++)
            {
                string str = keywords[i];
                newMat.EnableKeyword (str);
            }
            SetupMaterialWithBlendMode (newMat, blendMode, true, copyMat.renderQueue);
            newMat.mainTextureScale = copyMat.mainTextureScale;
            newMat.mainTextureOffset = copyMat.mainTextureOffset;
        }
    }
} // namespace UnityEditor
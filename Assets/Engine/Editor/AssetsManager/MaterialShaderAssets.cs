using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public struct ShaderPropertyValue
    {
        public int shaderID;
        public string shaderKeyName;
        public Vector4 value;
        public byte paramType;
    }

    public class ShaderTexPropertyValue
    {
        public int shaderID;
        public string shaderKeyName;
        public Texture value;
        public byte texType;
        public string path;
        public string physicPath;
    }

    public struct MaterialContext
    {
        public List<ShaderTexPropertyValue> textureValue;
        public BlendMode blendMode;

        public uint flag;

        public int renderQueue;

        public List<ShaderPropertyValue> shaderIDs;
        public List<DummyMaterialInfo> materialInfos;
        public int matType;
        public uint matHash;

        public void SetFlag (uint f, bool add)
        {
            if (add)
            {
                flag |= f;
            }
            else
            {
                flag &= ~(f);
            }
        }

        public bool HasFlag (uint f)
        {
            return (flag & f) != 0;
        }
        public void Init ()
        {
            textureValue = new List<ShaderTexPropertyValue> ();
            shaderIDs = new List<ShaderPropertyValue> ();
            flag = 0;
        }

        public static MaterialContext GetContext ()
        {
            MaterialContext context = new MaterialContext ();
            context.Init ();
            return context;
        }
    }
    public delegate void MatAddResCb (string path, Texture tex, MatSaveData msd);
    public delegate void MatAddStrCb (string str, MatSaveData msd);
    public class MatInfo
    {
        public Material mat;
        public MaterialContext context;
        public uint hash;
    }

    public class MatSaveData
    {
        public List<MatInfo> matInfo = new List<MatInfo> ();
        public ResAssets resAsset = null;
        public ushort FindMatIndex (Material mat)
        {
            for (ushort i = 0; i < matInfo.Count; ++i)
            {
                var mi = matInfo[i];
                if (mi.mat == mat)
                {
                    return i;
                }
            }
            return ushort.MaxValue;
        }

        public bool AnalyzeMaterial (Material mat, ref MaterialContext context)
        {
            Material tmpMat = new Material (mat);
            MaterialShaderAssets.ClearMat (tmpMat);

            string matInfo =  mat.name + "  Shader:" + (mat.shader != null ? mat.shader.name : " null");

            if (!MaterialShaderAssets.ResolveMatProperty(
                    tmpMat,
                    ref context))
            {
                if (mat.name.Contains("URP"))
                {
                    Debug.LogError("Not Config Material:" + matInfo, mat);
                }
            }
            else
            {
                Debug.Log(matInfo, mat);
            }

            UnityEngine.Object.DestroyImmediate (tmpMat);
            return true;
        }
        private void AddResName (string path, Texture tex)
        {
            if (resAsset != null)
            {
                string fullpath = AssetDatabase.GetAssetPath (tex);
                if (fullpath.ToLower ().StartsWith (LoadMgr.singleton.BundlePath))
                {
                    //not redirect
                    path = AssetsPath.GetPath (fullpath, out var ext);
                    resAsset.AddResName (path);
                }
                else
                {
                    resAsset.AddResName(path);
                    resAsset.AddResReDirct(tex, path);
                }
            }

        }
        private void AddStr (string name)
        {
            if (resAsset != null)
            {
                resAsset.AddResName (name);
            }
        }
        private MatInfo AddMatInfo (Material mat)
        {
            MatInfo newmi = new MatInfo ();
            newmi.mat = mat;
            
            newmi.context.Init ();
            if (AnalyzeMaterial (mat, ref newmi.context))
            {
                newmi.hash = newmi.context.matHash;
                for (int j = 0; j < newmi.context.textureValue.Count; ++j)
                {
                    var stpv = newmi.context.textureValue[j];
                    AddResName (stpv.path, stpv.value);
                    if (stpv.shaderID == ShaderManager._ShaderKeyEffectKey.Length)
                    {
                        AddStr (stpv.shaderKeyName);
                    }
                }
                for (int j = 0; j < newmi.context.shaderIDs.Count; ++j)
                {
                    var stpv = newmi.context.shaderIDs[j];
                    if (stpv.shaderID == ShaderManager._ShaderKeyEffectKey.Length)
                    {
                        AddStr (stpv.shaderKeyName);
                    }
                }
                matInfo.Add (newmi);
                return newmi;
            }
            return null;
        }

        public bool FindOrAddMatInfo (Material mat, out int id, out uint hash)
        {
            id = -1;
            hash = 0;
            if (mat == null)
                return false;
            for (int i = 0; i < matInfo.Count; ++i)
            {
                var mi = matInfo[i];
                if (mi.mat == mat)
                {
                    hash = mi.hash;
                    id = i;
                    return false;
                }
            }

            var newmi = AddMatInfo (mat);
            if (newmi != null)
            {
                hash = newmi.hash;
                id = matInfo.Count - 1;
                return true;
            }
            return false;
        }

        //public bool FindOrAddMatInfo (Material mat, out uint hash)
        //{
        //    hash = 0;
        //    if (mat == null)
        //        return false;
        //    for (int i = 0; i < matInfo.Count; ++i)
        //    {
        //        var mi = matInfo[i];
        //        if (mi.mat == mat)
        //        {
        //            hash = mi.hash;
        //            return false;
        //        }
        //    }
            
        //    var newmi = AddMatInfo (mat);
        //    if (newmi != null)
        //    {
        //        hash = newmi.hash;
        //        return true;
        //    }
        //    return false;            
        //}
        public void Sort ()
        {
            matInfo.Sort ((x, y) => { return x.context.renderQueue.CompareTo (y.context.renderQueue); });
        }
        public void SaveMaterial (BinaryWriter bw, bool writeMatType = false)
        {
            ushort materialGroupCount = (ushort) matInfo.Count;
            bw.Write (materialGroupCount);
            for (ushort j = 0; j < materialGroupCount; ++j)
            {
                var mi = matInfo[j];
                var context = mi.context;
                bw.Write(mi.hash);
                if (writeMatType)
                {
                    bw.Write((byte)context.matType);
                }
                bw.Write(context.flag);
                bw.Write(context.matHash);
                byte resCount = (byte) context.textureValue.Count;
                bw.Write (resCount);
                for (int i = 0; i < resCount; ++i)
                {
                    var stpv = context.textureValue[i];
                    byte index = (byte) stpv.shaderID;
                    bw.Write (index);
                    if (stpv.shaderID >= ShaderManager._ShaderKeyEffectKey.Length)
                    {
                        resAsset.SaveStringIndex (bw, stpv.shaderKeyName);
                    }
                    bw.Write (stpv.texType);
                    resAsset.SaveStringIndex (bw, stpv.path);

                }
                // long tmpPos = bw.BaseStream.Position - ssci.startPos;
                byte shaderPropertyCount = (byte) context.shaderIDs.Count;
                bw.Write (shaderPropertyCount);
                for (int i = 0; i < shaderPropertyCount; ++i)
                {
                    var spv = context.shaderIDs[i];
                    byte index = (byte) spv.shaderID;
                    bw.Write (index);
                    if (index >= ShaderManager._ShaderKeyEffectKey.Length)
                    {
                        resAsset.SaveStringIndex (bw, spv.shaderKeyName);
                    }
                    bw.Write (spv.paramType);
                    EditorCommon.WriteVector (bw, spv.value);
                    //ssci.debugLogStream.AppendFormatStr (string.Format ("shader property type:{0}_{1}", i, bw.BaseStream.Position - ssci.startPos));
                }

                //ssci.debugLogStream.AppendFormatStr (string.Format ("head mat:{0}_{1}:{2}:{3}", j, mi.mat.name, tmpPos, bw.BaseStream.Position - ssci.startPos));
            }

            // SaveStringIndex (bw, ssci, ssci.context.name + "_SDF");
        }

        public void OutputLog ()
        {
            for (int i = 0; i < matInfo.Count; ++i)
            {
                var mi = matInfo[i];
                DebugLog.AddEngineLog (mi.mat.name);
            }
        }
    }

    internal class MaterialShaderAssets
    {

        internal class ShaderValue
        {
            public ShaderValue (string n, ShaderUtil.ShaderPropertyType t)
            {
                name = n;
                type = t;
            }
            public string name = "";
            public ShaderUtil.ShaderPropertyType type = ShaderUtil.ShaderPropertyType.Float;

            public virtual void SetValue (Material mat)
            {

            }

            public static void GetShaderValue (Material mat, List<ShaderValue> shaderValueLst)
            {
                Shader shader = mat.shader;
                int count = ShaderUtil.GetPropertyCount (shader);
                for (int i = 0; i < count; ++i)
                {
                    ShaderValue sv = null;
                    string name = ShaderUtil.GetPropertyName (shader, i);
                    ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType (shader, i);
                    switch (type)
                    {
                        case ShaderUtil.ShaderPropertyType.Color:
                            sv = new ShaderColorValue (name, type, mat);
                            break;
                        case ShaderUtil.ShaderPropertyType.Vector:
                            sv = new ShaderVectorValue (name, type, mat);
                            break;
                        case ShaderUtil.ShaderPropertyType.Float:
                            sv = new ShaderFloatValue (name, type, mat);
                            break;
                        case ShaderUtil.ShaderPropertyType.Range:
                            sv = new ShaderFloatValue (name, type, mat);
                            break;
                        case ShaderUtil.ShaderPropertyType.TexEnv:
                            sv = new ShaderTexValue (name, type, mat);
                            break;
                    }
                    shaderValueLst.Add (sv);
                }
                ShaderKeyWordValue keyword = new ShaderKeyWordValue (mat);
                shaderValueLst.Add (keyword);
            }

        }

        internal class ShaderIntValue : ShaderValue
        {
            public ShaderIntValue (string n, ShaderUtil.ShaderPropertyType t, Material mat) : base (n, t)
            {
                value = mat.GetInt (n);
            }
            public int value = 0;
            public override void SetValue (Material mat)
            {
                mat.SetInt (name, value);
            }
        }

        internal class ShaderFloatValue : ShaderValue
        {
            public ShaderFloatValue (string n, ShaderUtil.ShaderPropertyType t, Material mat) : base (n, t)
            {
                value = mat.GetFloat (n);
            }
            public float value = 0;
            public override void SetValue (Material mat)
            {
                mat.SetFloat (name, value);
            }
        }

        internal class ShaderVectorValue : ShaderValue
        {
            public ShaderVectorValue (string n, ShaderUtil.ShaderPropertyType t, Material mat) : base (n, t)
            {
                value = mat.GetVector (n);
            }
            public Vector4 value = Vector4.zero;
            public override void SetValue (Material mat)
            {
                mat.SetVector (name, value);
            }
        }
        internal class ShaderColorValue : ShaderValue
        {
            public ShaderColorValue (string n, ShaderUtil.ShaderPropertyType t, Material mat) : base (n, t)
            {
                value = mat.GetColor (n);
            }
            public Color value = Color.black;
            public override void SetValue (Material mat)
            {
                mat.SetColor (name, value);
            }
        }
        internal class ShaderTexValue : ShaderValue
        {
            public ShaderTexValue (string n, ShaderUtil.ShaderPropertyType t, Material mat) : base (n, t)
            {
                value = mat.GetTexture (n);
                offset = mat.GetTextureOffset (n);
                scale = mat.GetTextureScale (n);
            }
            public Texture value = null;
            public Vector2 offset = Vector2.zero;
            public Vector2 scale = Vector2.one;

            public override void SetValue (Material mat)
            {
                mat.SetTexture (name, value);
                //mat.SetTextureOffset(name, offset);
                //mat.SetTextureScale(name, scale);
            }
        }
        internal class ShaderKeyWordValue : ShaderValue
        {
            public ShaderKeyWordValue (Material mat) : base ("", ShaderUtil.ShaderPropertyType.Float)
            {
                if (mat.shaderKeywords != null)
                {
                    string[] tmp = mat.shaderKeywords;
                    keywordValue = new string[tmp.Length];

                    for (int i = 0; i < tmp.Length; ++i)
                    {
                        keywordValue[i] = tmp[i];
                    }
                }
                blendMode = GetBlendMode (mat);
            }
            public string[] keywordValue = null;
            public BlendMode blendMode = BlendMode.Opaque;
            public override void SetValue (Material mat)
            {
                var keywordData = ShaderKeywordConfig.instance.keywordData;
                for (int i = 0; i < keywordValue.Length; ++i)
                {
                    var str = keywordValue[i];
                    if (keywordData.ContainKeyIndex (str))
                    {
                        mat.EnableKeyword (str);
                    }
                    else
                    {
                        mat.DisableKeyword (str);
                    }
                }
                SetupMaterialWithBlendMode (mat, blendMode);
            }
        }

        internal class RenderQueueValue : ShaderValue
        {
            public RenderQueueValue (Material mat) : base ("", ShaderUtil.ShaderPropertyType.Float)
            {
                if (mat.shader.name == "Custom/PBS/Role" && mat.shader.name.EndsWith ("_upper"))
                {
                    renderQueue = 2451;
                }
            }
            public int renderQueue = -1;
            public override void SetValue (Material mat)
            {
                mat.renderQueue = renderQueue;
            }
        }

        internal static List<ShaderValue> shaderValue = new List<ShaderValue> ();

        [MenuItem ("Assets/Tool/Material_Clear")]
        static void ClearMat ()
        {
            CommonAssets.enumMat.cb = (mat, path, context) =>
            {
                ClearMat (mat);
            };
            CommonAssets.EnumAsset<Material> (CommonAssets.enumMat, "ClearMat");
        }

        public static void ClearMat (Material mat)
        {
            shaderValue.Clear ();
            ShaderValue.GetShaderValue (mat, shaderValue);
            Material emptyMat = new Material (mat.shader);
            mat.CopyPropertiesFromMaterial (emptyMat);
            UnityEngine.Object.DestroyImmediate (emptyMat);
            for (int i = 0; i < shaderValue.Count; ++i)
            {
                ShaderValue sv = shaderValue[i];
                sv.SetValue (mat);
            }
        }

        internal delegate void ExtractMatCb (Material material);
        internal static void ExtractMaterialsFromAsset (ModelImporter modelImporter, string destinationPath, bool createMat, ExtractMatCb cb)
        {
            if (AssetsConfig.instance.defaultRoleShader != null)
            {
                SerializedObject serializedObject = new UnityEditor.SerializedObject (modelImporter);
                SerializedProperty externalObjects = serializedObject.FindProperty ("m_ExternalObjects");
                SerializedProperty materials = serializedObject.FindProperty ("m_Materials");

                Dictionary<string, Material> preMaterial = new Dictionary<string, Material> ();
                for (int i = 0; i < materials.arraySize; ++i)
                {
                    SerializedProperty arrayElementAtIndex = materials.GetArrayElementAtIndex (i);
                    string stringValue = arrayElementAtIndex.FindPropertyRelative ("name").stringValue;

                    for (int j = 0; j < externalObjects.arraySize; ++j)
                    {
                        SerializedProperty arrayElementAtIndex2 = externalObjects.GetArrayElementAtIndex (j);
                        if (arrayElementAtIndex2.FindPropertyRelative ("first.name").stringValue == stringValue)
                        {
                            preMaterial.Add (stringValue, arrayElementAtIndex2.FindPropertyRelative ("second").objectReferenceValue as Material);
                        }
                    }
                }

                externalObjects.arraySize = 0;
                for (int i = 0; i < materials.arraySize; ++i)
                {
                    SerializedProperty arrayElementAtIndex = materials.GetArrayElementAtIndex (i);

                    string stringValue = arrayElementAtIndex.FindPropertyRelative ("name").stringValue;
                    Material mat = null;
                    //UnityEngine.Object o = arrayElementAtIndex2.FindPropertyRelative("second").objectReferenceValue;
                    if (createMat)
                    {
                        mat = new Material (AssetsConfig.instance.defaultRoleShader);
                        mat.name = stringValue;

                        if (preMaterial.ContainsKey (stringValue))
                            mat = preMaterial[stringValue];
                        else
                        {
                            if (!File.Exists (destinationPath + "\\" + stringValue + ".mat"))
                            {
                                mat = CommonAssets.CreateAsset<Material> (destinationPath, stringValue, ".mat", mat);
                            }
                            else
                            {
                                mat = AssetDatabase.LoadAssetAtPath<Material> (string.Format ("{0}/{1}.mat", destinationPath, stringValue));
                            }
                        }
                    }
                    else
                    {
                        mat = AssetDatabase.LoadAssetAtPath<Material> (string.Format ("{0}/{1}.mat", destinationPath, stringValue));
                    }
                    if (cb != null)
                    {
                        cb (mat);
                    }

                    string stringValue2 = arrayElementAtIndex.FindPropertyRelative ("type").stringValue;
                    string stringValue3 = arrayElementAtIndex.FindPropertyRelative ("assembly").stringValue;
                    externalObjects.arraySize++;
                    SerializedProperty arrayElementAtIndex2 = externalObjects.GetArrayElementAtIndex (i);
                    arrayElementAtIndex2.FindPropertyRelative ("first.name").stringValue = stringValue;
                    arrayElementAtIndex2.FindPropertyRelative ("first.type").stringValue = stringValue2;
                    arrayElementAtIndex2.FindPropertyRelative ("first.assembly").stringValue = stringValue3;
                    arrayElementAtIndex2.FindPropertyRelative ("second").objectReferenceValue = mat;
                }
                serializedObject.ApplyModifiedProperties ();
                modelImporter.SearchAndRemapMaterials (ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.Local);
                AssetDatabase.WriteImportSettingsIfDirty (modelImporter.assetPath);
            }

        }

        public static void SetupMaterialWithBlendMode (Material material, BlendMode blendMode, bool resetRenderQueue = true, int renderQueue = -1)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag ("RenderType", "");
                    if (material.HasProperty ("_SrcBlend"))
                        material.SetInt ("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                    if (material.HasProperty ("_DstBlend"))
                        material.SetInt ("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                    if (material.HasProperty ("_ZWrite"))
                        material.SetInt ("_ZWrite", 1);
                    material.DisableKeyword ("_ALPHA_TEST");
                    material.DisableKeyword ("_ALPHA_BLEND");
                    if (resetRenderQueue)
                    {
                        if (renderQueue != -1)
                            material.renderQueue = renderQueue;
                        else
                            material.renderQueue = -1;
                    }

                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag ("RenderType", "TransparentCutout");
                    if (material.HasProperty ("_SrcBlend"))
                        material.SetInt ("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                    if (material.HasProperty ("_DstBlend"))
                        material.SetInt ("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                    if (material.HasProperty ("_ZWrite"))
                        material.SetInt ("_ZWrite", 1);
                    material.EnableKeyword ("_ALPHA_TEST");
                    material.DisableKeyword ("_ALPHA_BLEND");
                    if (resetRenderQueue)
                    {
                        if (renderQueue != -1)
                            material.renderQueue = renderQueue;
                        else
                            material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
                    }
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag ("RenderType", "Transparent");
                    if (material.HasProperty ("_SrcBlend"))
                        material.SetInt ("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                    if (material.HasProperty ("_DstBlend"))
                        material.SetInt ("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    if (material.HasProperty ("_ZWrite"))
                        material.SetInt ("_ZWrite", 0);
                    material.DisableKeyword ("_ALPHA_TEST");
                    material.EnableKeyword ("_ALPHA_BLEND");
                    if (resetRenderQueue)
                    {
                        if (renderQueue != -1)
                            material.renderQueue = renderQueue;
                        else
                            material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                    }
                    break;
                case BlendMode.DepthTransparent:
                    material.SetOverrideTag ("RenderType", "TransparentCutout");
                    if (material.HasProperty ("_SrcBlend"))
                        material.SetInt ("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                    if (material.HasProperty ("_DstBlend"))
                        material.SetInt ("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    if (material.HasProperty ("_ZWrite"))
                        material.SetInt ("_ZWrite", 1);
                    material.DisableKeyword ("_ALPHA_TEST");
                    material.EnableKeyword ("_ALPHA_BLEND");
                    if (resetRenderQueue)
                    {
                        if (renderQueue != -1)
                            material.renderQueue = renderQueue;
                        else
                            material.renderQueue = 2600;
                    }
                    break;
            }
        }

        internal static BlendMode GetBlendMode (Material material)
        {
            return EditorCommon.GetBlendMode (material);
        }
        internal static int BlendMode2Index (BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    return 0;
                case BlendMode.Cutout:
                    return 1;
                case BlendMode.Transparent:
                    return 3;
                case BlendMode.DepthTransparent:
                    return 4;
            }
            return 0;
        }
        internal static BlendMode Index2BlendMode (int index)
        {
            switch (index)
            {
                case 0:
                    return BlendMode.Opaque;
                case 1:
                    return BlendMode.Cutout;
                case 3:
                    return BlendMode.Transparent;
                case 4:
                    return BlendMode.DepthTransparent;
            }
            return BlendMode.Opaque;
        }

        // struct KeywordInfo
        // {
        //     public bool comp;
        //     public KeywordFlags flag;
        //     public string str;
        // }

        static string[] shaderNameKeys = null;
        // internal static void FindMatKeyWord (string key, ref uint keywordFlag)
        // {
        //     for (int i = 0; i < keyWordInfo.Length; ++i)
        //     {
        //         KeywordInfo kf = keyWordInfo[i];
        //         if (kf.str == key && kf.idKeyword)
        //         {
        //             keywordFlag |= (uint) kf.flag;
        //             return;
        //         }
        //     }
        // }
        internal static bool AddMaterialProperty (
            ref MaterialContext context,
            Material material,
            ShaderProperty sp, string propertyName,
            Dictionary<string, ShaderFeature> sfMap)
        {
            bool enable = true;
            if (!string.IsNullOrEmpty (sp.depComp))
            {
                ShaderFeature sf;
                if (sfMap.TryGetValue (sp.depComp, out sf))
                {
                    if (sf.type == ShaderPropertyType.Keyword ||
                        sf.type == ShaderPropertyType.ValueToggle)
                    {
                        if (sf.type == ShaderPropertyType.Keyword)
                        {
                            enable = material.IsKeywordEnabled (sf.propertyName);
                        }
                        else if (sf.type == ShaderPropertyType.ValueToggle)
                        {
                            var scp = sf.customProperty[0];
                            Vector4 value = material.GetVector (sf.propertyName);
                            var cmpFun = ShaderCustomProperty.cmpFun[(int) scp.valueCmpType];
                            int index = scp.index >= 0 ? scp.index : 0;
                            enable = cmpFun (value[index], scp.thresholdValue);
                        }

                    }
                }
            }
            if (enable)
            {
                if (sp.dataType == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    Texture tex = material.GetTexture (propertyName);
                    if (tex != null)
                    {
                        byte texType = 0;
                        string ext;
                        string path = AssetsPath.GetAssetFullPath (tex, out ext);
                        ext = ext.ToLower ();
                        if (!string.IsNullOrEmpty (sp.relativeStr))
                        {
                            tex = null;
                            int index = path.LastIndexOf (sp.relativeStr);
                            if (index >= 0)
                            {
                                path = path.Substring (0, index) + ext;
                                if (File.Exists (path))
                                {
                                    tex = AssetDatabase.LoadAssetAtPath<Texture> (path);
                                }
                            }
                            if (tex == null)
                            {
                                DebugLog.AddEngineLog2 ("relative tex not find:{0} propertyName:{1}", path, propertyName);
                                return false;
                            }
                        }
                        else
                        {

                        }
                        if (tex is Texture2D)
                        {
                            if (ext.EndsWith (".tga"))
                            {
                                texType = ResObject.Tex_2D;
                            }
                            else if (ext.EndsWith (".png"))
                            {
                                texType = ResObject.Tex_2D_PNG;
                            }
                            else if (ext.EndsWith (".exr"))
                            {
                                texType = ResObject.Tex_2D_EXR;
                            }
                            else
                            {
                                DebugLog.AddEngineLog2 ("unknow tex ext:{0} mat:{1}", path, material.name);
                                return false;
                            }

                        }
                        else if (tex is Cubemap)
                        {
                            texType = ext.EndsWith(".tga") ? ResObject.Tex_Cube : ResObject.Tex_Cube_PNG;
                            if (ext.EndsWith(".exr"))
                            {
                                texType = ResObject.Tex_Cube_EXR;
                            }
                        }
                        else if (tex is Texture3D)
                        {
                            texType = ResObject.Tex_3D;
                        }
                        string physicPath = path;
                        if (path.StartsWith (AssetsConfig.instance.ResourcePath))
                        {
                            path = AssetsPath.GetPath (path, out ext).ToLower();
                        }
                        else
                        {
                            path = tex.name.ToLower();
                        }
                        
                        ShaderTexPropertyValue stpv = new ShaderTexPropertyValue ()
                        {
                            shaderID = sp.shaderID,
                            value = tex,
                            path = path,
                            texType = texType,
                            shaderKeyName = propertyName,
                            physicPath = physicPath
                        };
                        context.textureValue.Add (stpv);
                        return true;
                    }
                    else
                    {
                        DebugLog.AddEngineLog2 ("null tex property:{0} mat:{1}", propertyName, material.name);
                        return false;
                    }
                }
                else if (sp.dataType == ShaderUtil.ShaderPropertyType.Vector)
                {
                    Vector4 param = material.GetVector (propertyName);
                    ShaderPropertyValue spv = new ShaderPropertyValue ()
                    {
                        shaderID = sp.shaderID,
                        value = param,
                        shaderKeyName = propertyName,
                        paramType = ResObject.Vector,
                    };
                    context.shaderIDs.Add (spv);
                    return true;
                }
                else if (sp.dataType == ShaderUtil.ShaderPropertyType.Color)
                {
                    Color param = material.GetColor (propertyName);
                    var paramType = ResObject.Color;
                    var property = MaterialEditor.GetMaterialProperty (new UnityEngine.Object[] { material }, propertyName);
                    if (property != null)
                    {
                        if ((property.flags & MaterialProperty.PropFlags.HDR) != 0)
                        {
                            paramType = ResObject.Color_HDR;
                        }
                    }
                    // Vector4 v = param;
                    // Vector4 v1 = param.gamma;
                    // Vector4 v2 = param.linear;
                    ShaderPropertyValue spv = new ShaderPropertyValue ()
                    {
                        shaderID = sp.shaderID,
                        value = new Vector4 (param.r, param.g, param.b, param.a),
                        shaderKeyName = propertyName,
                        paramType = paramType,
                    };

                    //[Gamma] color.linear
                    //[HDR] color
                    //none color.linear
                    // DebugLog.AddEngineLog2 ("COLOR:{0} {1} gamma:{2} hdr:{3}", propertyName,
                    //     v.ToString ("F2"), v1.ToString ("F2"), v2.ToString ("F2"));
                    context.shaderIDs.Add (spv);
                    return true;
                }
                else if (sp.dataType == ShaderUtil.ShaderPropertyType.Float ||
                    sp.dataType == ShaderUtil.ShaderPropertyType.Range)
                {
                    float param = material.GetFloat (propertyName);
                    ShaderPropertyValue spv = new ShaderPropertyValue ()
                    {
                        shaderID = sp.shaderID,
                        value = new Vector4 (param, 0, 0, 0),
                        shaderKeyName = propertyName,
                        paramType = ResObject.Float,
                    };
                    context.shaderIDs.Add (spv);
                    return true;
                }
            }
            return false;

        }
        internal static bool AddMaterialProperty (
            ref MaterialContext context,
            Material material,
            ShaderProperty sp,
            Dictionary<string, ShaderFeature> sfMap)
        {
            if (shaderNameKeys == null)
            {
                shaderNameKeys = System.Enum.GetNames (typeof (EShaderKeyID));
                for (int i = 0; i < shaderNameKeys.Length; ++i)
                {
                    shaderNameKeys[i] = shaderNameKeys[i];
                }
            }
            if (sp.shaderID >= 0 && sp.shaderID < (shaderNameKeys.Length - 1))
            {
                string keyName = shaderNameKeys[sp.shaderID];
                if (material.HasProperty (keyName))
                {
                    return AddMaterialProperty (ref context, material, sp, keyName, sfMap);
                }
            }
            else if (!string.IsNullOrEmpty (sp.customName) &&
                material.HasProperty (sp.customName))
            {
                return AddMaterialProperty (ref context, material, sp, sp.customName, sfMap);
            }
            return false;
        }
        internal static void CalcMatHash(Material material,ref MaterialContext context)
        {
            var matShader = material.shader;
            if (matShader == null)
                return;
            string tag = material.GetTag("RenderType", false);
            if (tag == "Transparent")
            {
                context.blendMode = BlendMode.Transparent;
            }
            else
            {
                Shader shader = material.shader;
                if (shader != null && shader.name.Contains("Cutout"))
                    context.blendMode = BlendMode.Cutout;
                else
                    context.blendMode = BlendMode.Opaque;
            }
            ShaderKeywordConfig skc = ShaderKeywordConfig.instance;
            string[] keywords = material.shaderKeywords;
            uint featureKeywordFlag = skc.keywordData.GetShaderKey(keywords);


            var sc = ShaderConfig.instance;

            List<DummyMaterialInfo> matList = null;
            for (int i = 0; i < sc.shaderGUI.configRefs.Count; ++i)
            {
                var configRef = sc.shaderGUI.configRefs[i];
                var guiConfig = configRef.config;
                if (guiConfig != null &&
                    guiConfig.shader == matShader)
                {
                    if (guiConfig.dummyMatType == EDummyMatType.SceneMat)
                    {
                        matList = EditorMaterialConfig.instance.sceneMat.materialInfos;
                        context.matType = 0;
                    }
                    else if (guiConfig.dummyMatType == EDummyMatType.DynamicMat)
                    {
                        matList = EditorMaterialConfig.instance.dynamicMat.materialInfos;
                        context.matType = 1;
                    }
                    else if (guiConfig.dummyMatType == EDummyMatType.EffectMat)
                    {
                        matList = EditorMaterialConfig.instance.effectMat.materialInfos;
                        context.matType = 2;
                    }
                    break;
                }
            }
            context.materialInfos = matList;
            if (matList != null)
            {
                int findIndex = -1;
                int firstSameIndex = -1;
                for (int i = 0; i < matList.Count; ++i)
                {
                    var dmi = matList[i];
                    if (material.shader == dmi.shader && context.blendMode == dmi.blendType)
                    {
                        uint keywordFlag = skc.keywordData.GetShaderKey(dmi.keywords);
                        if (keywordFlag == featureKeywordFlag)
                        {
                            findIndex = i;
                            break;
                        }
                        else if ((keywordFlag == 0 || (keywordFlag & featureKeywordFlag) != 0))
                        {
                            firstSameIndex = i;
                        }
                    }
                }
                if (findIndex == -1)
                {
                    if (firstSameIndex != -1)
                    {
                        findIndex = firstSameIndex;
                    }
                    else
                    {
                        return;
                    }

                }

                var finddmi = matList[findIndex];
                context.matHash = EngineUtility.XHashLowerRelpaceDot(0, finddmi.hashID);
            }
        }
        
        internal static bool ResolveMatProperty (Material material,
            ref MaterialContext context)
        {
            var matShader = material.shader;
            if (matShader == null)
                return false;
            string tag = material.GetTag ("RenderType", false);
            if (tag == "Transparent")
            {
                context.blendMode = BlendMode.Transparent;
            }
            else
            {
                Shader shader = material.shader;
                if (shader != null && shader.name.Contains ("Cutout"))
                    context.blendMode = BlendMode.Cutout;
                else
                    context.blendMode = BlendMode.Opaque;
            }

            //ShaderKeywordConfig skc = ShaderKeywordConfig.instance;
            //hashID = string.Format("{0}_{1}", shader.name, blendType.ToString());
            //skc.keywordData.GetShaderKeyStr(keywords, ref hashID);

            ShaderKeywordConfig skc = ShaderKeywordConfig.instance;
            string[] keywords = material.shaderKeywords;
            uint featureKeywordFlag = skc.keywordData.GetShaderKey (keywords);

            
            var sc = ShaderConfig.instance;
            Dictionary<string, ShaderFeature> sfMap = new Dictionary<string, ShaderFeature> ();
            for (int i = 0; i < sc.shaderFeature.groupRefs.Count; ++i)
            {
                var fgRef = sc.shaderFeature.groupRefs[i];
                if (fgRef.sfg != null)
                {
                    for (int ii = 0; ii < fgRef.sfg.shaderFeatureBlocks.Count; ++ii)
                    {
                        var block = fgRef.sfg.shaderFeatureBlocks[ii];
                        for (int jj = 0; jj < block.shaderFeatures.Count; ++jj)
                        {
                            var sf = block.shaderFeatures[jj];
                            sf.key = string.Format ("{0}_{1}",
                                fgRef.sfg.groupName, sf.name);
                            sfMap[sf.key] = sf;
                        }
                    }
                }
            }
           
            List<DummyMaterialInfo> matList = null;
            for (int i = 0; i < sc.shaderGUI.configRefs.Count; ++i)
            {
                var configRef = sc.shaderGUI.configRefs[i];
                var guiConfig = configRef.config;
                if (guiConfig != null &&
                    guiConfig.shader == matShader)
                {
                    if (guiConfig.dummyMatType == EDummyMatType.SceneMat)
                    {
                        matList = EditorMaterialConfig.instance.sceneMat.materialInfos;
                        context.matType = 0;
                    }
                    else if (guiConfig.dummyMatType == EDummyMatType.DynamicMat)
                    {
                        matList = EditorMaterialConfig.instance.dynamicMat.materialInfos;
                        context.matType = 1;
                    }
                    else if (guiConfig.dummyMatType == EDummyMatType.EffectMat)
                    {
                        matList = EditorMaterialConfig.instance.effectMat.materialInfos;
                        context.matType = 2;
                    }
                    break;
                }
            }
            context.materialInfos = matList;
            if (matList != null)
            {
                int findIndex = -1;
                int firstSameIndex = -1;
                for (int i = 0; i < matList.Count; ++i)
                {
                    var dmi = matList[i];
                    if (material.shader == dmi.shader && context.blendMode == dmi.blendType)
                    {
                        uint keywordFlag = skc.keywordData.GetShaderKey (dmi.keywords);
                        if (keywordFlag == featureKeywordFlag)
                        {
                            findIndex = i;
                            break;
                        }
                        else if ((keywordFlag == 0 || (keywordFlag & featureKeywordFlag) != 0))
                        {
                            firstSameIndex = i;
                        }
                    }
                }
                if (findIndex == -1)
                {
                    if (firstSameIndex != -1)
                    {
                        findIndex = firstSameIndex;
                    }
                    else
                    {
                        return false;
                    }

                }

                var finddmi = matList[findIndex];
                context.flag = (uint) findIndex;

                context.matHash = EngineUtility.XHashLowerRelpaceDot(0, finddmi.hashID);


                for (int j = 0; j < finddmi.shaderPropertys.Count; ++j)
                {
                    var sp = finddmi.shaderPropertys[j];
                    AddMaterialProperty (ref context, material, sp, sfMap);
                }

                int renderQueue = material.renderQueue;
                if (renderQueue == -1)
                {
                    switch (context.blendMode)
                    {
                        case BlendMode.Opaque:
                            renderQueue = 2000;
                            break;
                        case BlendMode.Cutout:
                            renderQueue = 2450;
                            break;
                        case BlendMode.Transparent:
                            renderQueue = 3000;
                            break;
                        case BlendMode.DepthTransparent:
                            renderQueue = 2600;
                            break;
                    }
                }
                context.renderQueue = renderQueue;
                if (context.blendMode == BlendMode.Cutout)
                {
                    context.SetFlag (MatProperty.IsCutout, true);
                }
                else if (context.blendMode == BlendMode.Transparent)
                {
                    context.SetFlag (MatProperty.IsTransparent, true);
                }
                int passCount = material.passCount;
                for (int i = 0; i < passCount; ++i)
                {
                    if (material.GetPassName (i) == "OUTLINE")
                    {
                        context.SetFlag (MatProperty.Outline, true);
                        break;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        static StringBuilder keywordSB = new StringBuilder ();
        public static string GetKeyWords (List<string> keyWords)
        {
            keywordSB.Length = 0;
            for (int i = 0; i < keyWords.Count; ++i)
            {
                string key = keyWords[i];
                if (keywordSB.Length > 0)
                    keywordSB.Append ("|");
                keywordSB.Append (key);
            }
            return keywordSB.ToString ();
        }

        public static string GetKeyWords (Material mat)
        {
            keywordSB.Length = 0;
            string[] shaderKeywords = mat.shaderKeywords;

            for (int i = 0; i < shaderKeywords.Length; ++i)
            {
                if (keywordSB.Length > 0)
                    keywordSB.Append ("|");
                keywordSB.Append (shaderKeywords[i]);
            }
            return keywordSB.ToString ();
        }

        public static string GetMatPath (string folder)
        {
            string path = "";
            if (string.IsNullOrEmpty (folder))
            {
                path = string.Format ("{0}{1}",
                    LoadMgr.singleton.EngineResPath,
                    AssetsConfig.instance.DummyMatFolder);
            }
            else
            {
                path = string.Format ("{0}{1}/{2}",
                    LoadMgr.singleton.EngineResPath,
                    AssetsConfig.instance.DummyMatFolder, folder);
            }
            return path;
        }
        public static Material GetDummyMat (string name, string folder = "")
        {
            string path = string.Format ("{0}/{1}.mat",
                GetMatPath (folder), name);
            return AssetDatabase.LoadAssetAtPath<Material> (path);

        }
        public static void RefeshMat (Material mat, BlendMode blendMode, MaterialVariant mv, List<string> keywords)
        {
            if (mat != null)
            {
                mat.shaderKeywords = null;
                for (int i = 0; i < keywords.Count; ++i)
                {
                    string key = keywords[i];
                    mat.EnableKeyword (key);
                }
                for (int i = 0; i < mv.keywords.Count; ++i)
                {
                    string key = mv.keywords[i];
                    mat.EnableKeyword (key);
                }

                SetupMaterialWithBlendMode (mat, blendMode);
                for (int i = 0; i < mv.param.Count; ++i)
                {
                    var mvp = mv.param[i];
                    int shaderID = 0;
                    if (mvp.shaderID >= (int) EShaderKeyID.Num)
                    {
                        shaderID = Shader.PropertyToID (mvp.shaderKey);
                    }
                    else
                    {
                        shaderID = ShaderManager._ShaderKeyEffectKey[mvp.shaderID];
                    }

                    switch (mvp.vt)
                    {
                        case ShaderValueType.Vec:
                            mat.SetVector (shaderID, mvp.value);
                            break;
                        case ShaderValueType.Float:
                            mat.SetFloat (shaderID, mvp.value.x);
                            break;
                        case ShaderValueType.Int:
                            mat.SetInt (shaderID, (int) mvp.value.x);
                            break;
                    }

                }
            }
        }

        public static void CreateDummyMat (string folder, string name, Shader shader,
            BlendMode blendMode, DummyMaterialInfo dmi, MaterialVariant mv)
        {
            if (shader != null)
            {
                Material mat = new Material (shader);
                RefeshMat (mat, blendMode, mv, dmi.keywords);
                CommonAssets.CreateAsset<Material> (GetMatPath (folder), name, ".mat", mat);
            }
        }

        public static void DefaultMat (DummyMaterialInfo dmi, int index, string folder = "")
        {
            if (dmi.shader != null)
            {
                string name = dmi.name;
                if (index == -1)
                {
                    dmi.SetHashID ();
                    for (int i = 0; i < dmi.matVariants.Count; ++i)
                    {
                        var mv = dmi.matVariants[i];
                        CreateDummyMat (folder, name + mv.suffix, dmi.shader, dmi.blendType, dmi, mv);
                    }
                }
                else if (index >= 0 && index < dmi.matVariants.Count)
                {
                    var mv = dmi.matVariants[index];
                    CreateDummyMat (folder, name + mv.suffix, dmi.shader, dmi.blendType, dmi, mv);
                }
            }
        }
        public static void DefaultRefeshMat (DummyMaterialInfo dmi, int index)
        {
            if (index == -1)
            {
                for (int i = 0; i < dmi.matVariants.Count; ++i)
                {
                    var mv = dmi.matVariants[i];
                    RefeshMat (mv.mat, dmi.blendType, mv, dmi.keywords);
                }
            }
            else if (index >= 0 && index < dmi.matVariants.Count)
            {
                var mv = dmi.matVariants[index];
                RefeshMat (mv.mat, dmi.blendType, mv, dmi.keywords);
            }
        }

        public static void ScanMaterial (Material mat)
        {
            MaterialContext context = MaterialContext.GetContext ();
            Material tmpMat = new Material (mat);
            ClearMat (tmpMat);
            bool find = ResolveMatProperty (
                tmpMat,
                ref context);
            if (find)
            {
                uint matIndex = context.flag & EngineContext.matCountMask;
                if (context.materialInfos != null && (matIndex < context.materialInfos.Count))
                {
                    var mdi = context.materialInfos[(int) matIndex];

                    DebugLog.AddEngineLog2 ("mat:{0} type:{1} resCount:{2} flag:{3}",
                        mat.name,
                        mdi.name,
                        context.textureValue.Count.ToString (),
                        context.flag.ToString ());
                }

            }
            else
            {
                DebugLog.AddErrorLog2 ("Not Config Shader:{0}", mat.name);
            }

            UnityEngine.Object.DestroyImmediate (tmpMat);
        }

        public static void ScanMaterial (GameObject go)
        {
            List<Renderer> renders = EditorCommon.GetRenderers (go);
            for (int i = 0; i < renders.Count; ++i)
            {
                Renderer r = renders[i];
                if (r != null && r.sharedMaterial != null)
                {
                    ScanMaterial (r.sharedMaterial);
                }
            }
        }

        [MenuItem ("Assets/Tool/Mat_ScanMaterial")]
        public static void Mat_ScanMaterial ()
        {
            CommonAssets.enumPrefab.cb = (prefab, path, context) =>
            {
                ScanMaterial (prefab);
            };
            CommonAssets.EnumAsset<GameObject> (CommonAssets.enumPrefab, "ScanMaterial");
        }
        // public static void DefaultEffectMat (AssetsConfig.DummyMaterialInfo dmi, bool multiMat)
        // {
        //     if (dmi.shader != null)
        //     {
        //         string name = dmi.name;
        //         if (multiMat)
        //         {
        //             if ((dmi.blendType & EBlendType.Opaque) != 0)
        //                 CreateDummyMat (name, dmi.shader, BlendMode.Opaque, (KeywordFlags) dmi.flag, false, false);
        //             if ((dmi.blendType & EBlendType.Cutout) != 0)
        //                 CreateDummyMat (name + dmi.ext1, dmi.shader, BlendMode.Cutout, (KeywordFlags) dmi.flag, false, false);
        //             if ((dmi.blendType & EBlendType.CutoutTransparent) != 0)
        //                 CreateDummyMat (name + dmi.ext2, dmi.shader, BlendMode.CutoutTransparent, (KeywordFlags) dmi.flag, false, false);
        //         }
        //         else
        //         {
        //             CreateDummyMat (name, dmi.shader, GetBlendMode (dmi.blendType), (KeywordFlags) dmi.flag, false, false);
        //         }
        //     }
        // }

        static void CreateRefMat (string shaderName, string namePrefix, string dir, int count, float step, Color c, Mesh mesh, Vector3 pos)
        {
            GameObject group = new GameObject (namePrefix);
            Transform gt = group.transform;
            gt.position = pos;
            Shader s = Shader.Find (shaderName);
            float startX = (int) (count / 2) * -2;
            float startY = startX;
            float x = startX;
            float y = startY;
            for (int m = 0; m < count; ++m)
            {
                float metallic = m * step;
                x = startX;
                for (int r = 0; r < count; ++r)
                {
                    float roughness = r * step;
                    Material mat = new Material (s);
                    mat.SetColor ("_Color0", c);
                    mat.EnableKeyword ("_BASE_FROM_COLOR");
                    mat.EnableKeyword ("_PBS_FROM_PARAM");
                    mat.SetVector ("_Param0", new Vector4 (0, 0, roughness, metallic));
                    string name = string.Format ("{0}_M{1:N2}_R{2:N2}", namePrefix, metallic, roughness);
                    mat.name = name;
                    mat = CommonAssets.CreateAsset<Material> (dir, name, ".mat", mat);
                    GameObject go = new GameObject (name);
                    MeshRenderer mr = go.AddComponent<MeshRenderer> ();
                    mr.sharedMaterial = mat;
                    MeshFilter mf = go.AddComponent<MeshFilter> ();
                    mf.sharedMesh = mesh;
                    Transform t = go.transform;
                    t.parent = gt;
                    t.localPosition = new Vector3 (x, y, 0);
                    x += 2;
                }
                y += 2;
            }
        }
        static void RefreshRefMat (string shaderName, string namePrefix, string dir, int count, float step,
            Color c, Mesh mesh, Vector3 pos, int snap = 2)
        {
            GameObject group = GameObject.Find (namePrefix);
            if (group == null)
                group = new GameObject (namePrefix);
            Transform gt = group.transform;
            gt.position = pos;
            Shader s = Shader.Find (shaderName);
            float startX = (int) (count / snap) * -snap;
            float startY = startX;
            float x = startX;
            float y = startY;
            for (int m = 0; m < count; ++m)
            {
                float metallic = m * step;
                x = startX;
                for (int r = 0; r < count; ++r)
                {
                    float roughness = r * step;
                    string name = string.Format ("{0}_M{1:N2}_R{2:N2}", namePrefix, metallic, roughness);
                    name = EditorCommon.GetReplaceStr (name);
                    string path = string.Format ("{0}/{1}.mat", dir, name);
                    Material mat = AssetDatabase.LoadAssetAtPath<Material> (path);
                    if (mat == null)
                    {
                        mat = new Material (s);
                        mat.name = name;
                        mat.SetColor ("_Color0", c);
                        mat.EnableKeyword ("_BASE_FROM_COLOR");
                        mat.EnableKeyword ("_PBS_FROM_PARAM");
                        mat.SetVector ("_Param0", new Vector4 (0, 0, roughness, metallic));
                        mat.SetVector ("_Param1", new Vector4 (1, 0.5f, 1, 0.5f));
                        mat = CommonAssets.CreateAsset<Material> (dir, name, ".mat", mat);
                    }
                    else
                    {
                        mat.SetColor ("_Color0", c);
                        mat.EnableKeyword ("_BASE_FROM_COLOR");
                        mat.EnableKeyword ("_PBS_FROM_PARAM");
                        mat.SetVector ("_Param0", new Vector4 (0, 0, roughness, metallic));
                        mat.SetVector ("_Param1", new Vector4 (1, 0.5f, 1, 0.5f));
                    }

                    GameObject go = GameObject.Find (name);
                    if (go == null)
                    {
                        go = new GameObject (name);
                    }
                    MeshRenderer mr = go.GetComponent<MeshRenderer> ();
                    if (mr == null)
                    {
                        mr = go.AddComponent<MeshRenderer> ();
                    }
                    mr.sharedMaterial = mat;

                    MeshFilter mf = go.GetComponent<MeshFilter> ();
                    if (mf == null)
                    {
                        mf = go.AddComponent<MeshFilter> ();
                    }
                    mf.sharedMesh = mesh;
                    Transform t = go.transform;
                    t.parent = gt;
                    t.localPosition = new Vector3 (x, y, 0);
                    x += snap;
                }
                y += snap;
            }
        }

        [MenuItem ("Assets/Tool/Mat_RefreshRefMat")]
        static void RefreshRefMat ()
        {
            Mesh m = AssetsConfig.instance.sphereMesh;
            RefreshRefMat ("Custom/Editor/Scene/Uber", "Mat_White", "Assets/Engine/LookDev/Res/RefMats",
                5, 0.25f, Color.white, m, new Vector3 (64 - 10, 6, 64));
            RefreshRefMat ("Custom/Editor/Scene/Uber", "Mat_Gray", "Assets/Engine/LookDev/Res/RefMats",
                5, 0.25f, Color.gray, m, new Vector3 (64, 6, 64));
            RefreshRefMat ("Custom/Editor/Scene/Uber", "Mat_Black", "Assets/Engine/LookDev/Res/RefMats",
                5, 0.25f, Color.black, m, new Vector3 (64 + 10, 6, 64));
        }

        static void RefreshObjMat (string groupName, List<Material> mats, Mesh mesh, Vector3 pos, int snap = 2)
        {
            GameObject group = GameObject.Find (groupName);
            if (group == null)
                group = new GameObject (groupName);
            Transform gt = group.transform;
            gt.position = pos;
            int count = mats.Count;
            int lineCount = snap * 7 + 1;
            int yCount = count / lineCount;
            float startY = snap;
            int index = 0;
            for (int y = 0; y < yCount && index < count; ++y)
            {
                float yPos = startY + y * snap;
                float startX = -snap * 7;
                for (int x = 0; x < lineCount && index < count; ++x)
                {
                    float xPos = startX + x * snap;
                    Material mat = mats[index];
                    GameObject go = GameObject.Find (mat.name);
                    if (go == null)
                    {
                        go = new GameObject (mat.name);
                    }
                    MeshRenderer mr = go.GetComponent<MeshRenderer> ();
                    if (mr == null)
                    {
                        mr = go.AddComponent<MeshRenderer> ();
                    }
                    mr.sharedMaterial = mat;

                    MeshFilter mf = go.GetComponent<MeshFilter> ();
                    if (mf == null)
                    {
                        mf = go.AddComponent<MeshFilter> ();
                    }
                    mf.sharedMesh = mesh;
                    Transform t = go.transform;
                    t.parent = gt;
                    t.localPosition = new Vector3 (xPos, yPos, 0);
                    index++;
                }
            }
        }
        static List<Material> objMats = new List<Material> ();
        [MenuItem ("Assets/Tool/Mat_RefreshOjbMat")]
        static void RefreshOjbMat ()
        {
            // Mesh m = AssetsConfig.instance.sphereMesh;
            // CommonAssets.enumMat.cb = (mat, path, context) =>
            // {
            //     objMats.Add (mat);
            // };

            // CommonAssets.EnumAsset<Material> (CommonAssets.enumMat, "ObjMaterial", "Assets/Engine/LookDev/Res/ObjMats");
            // RefreshObjMat ("MatLib", objMats, m, new Vector3 (64, 0, 62));
            // objMats.Clear ();

            CommonAssets.enumMat.cb = (mat, path, context) =>
            {
                if(mat.HasProperty("_UseDepthFade"))
                {
                    float depthFade = mat.GetFloat("_UseDepthFade");
                    if(depthFade>0.5f)
                    {
                        DebugLog.AddErrorLog2("depth fade:{0}", path);
                    }
                }
                //string shaderName = mat.shader.name;
                //if ((shaderName == "Custom/Scene/Uber" ||
                //        shaderName == "Custom/Scene/TreeTrunk" ||
                //        shaderName == "Custom/Scene/Layer") &&
                //    mat.HasProperty ("_Param0"))
                //{
                //    var p0 = mat.GetVector ("_Param0");
                //    p0.z = 0;
                //    mat.SetVector ("_Param0", p0);

                //}
            };

            CommonAssets.EnumAsset<Material> (CommonAssets.enumMat, "ObjMaterial");
        }

        [MenuItem("Assets/Tool/Mat_RefreshSceneMat")]
        static void RefreshSceneMat()
        {
            CommonAssets.enumMat.cb = (mat, path, context) =>
            {
                string shaderName = mat.shader.name;
                //mat.EnableKeyword("_DEBUG_APP");
                //mat.DisableKeyword("_ADD_LIGHT");
                //mat.DisableKeyword("_SHADOW_MAP");
                //mat.DisableKeyword("_EXTRA_SHADOW");
                //mat.DisableKeyword("_NORMALMAP");                
                //mat.EnableKeyword("_OD0");
            };

            CommonAssets.EnumAsset<Material>(CommonAssets.enumMat, "RefreshSceneMat");
        }

        [MenuItem("Assets/Tool/Mat_SetProperty")]
        public static void SetProperty()
        {
            string[] directories = new string[] { "Assets/Creatures" };
            HashSet<Shader> shaderNames = new HashSet<Shader>
            {
                //Shader.Find("Custom/Role/Disappear"),
                //Shader.Find("Custom/Role/CartoonLaser"),
                //Shader.Find("Custom/Role/CartoonDissolve"),
                //Shader.Find("Custom/Role/Cartoon"),
                Shader.Find("Custom/Role/StyleHair"),
                Shader.Find("Custom/Role/CartoonFabric"),
                Shader.Find("Custom/Role/Hair"),
            };
            string filter = "t:material";
            string propertyName = "_Param0";

            List<Material> targets = new List<Material>();

            string[] guids = AssetDatabase.FindAssets(filter, directories);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material && material.shader && shaderNames.Contains(material.shader) && material.HasProperty(propertyName))
                {
                    targets.Add(material);
                }
            }

            for (float i = 0; i < targets.Count; i++)
            {
                Material material = targets[(int)i];
                EditorUtility.DisplayProgressBar($"Setting property", material.ToString(), i / targets.Count);
                Vector4 lastValue = material.GetVector(propertyName);
                Vector4 newValue = new Vector4(lastValue.x, 1, lastValue.z, lastValue.w);
                material.SetVector(propertyName, newValue);
                Debug.Log($"Set material property : {material}.{propertyName} = {lastValue} => {newValue}");
                EditorUtility.SetDirty(material);
            }
            EditorUtility.ClearProgressBar();

            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/Tool/Mat_RefreshMatMash")]
        static void RefreshMatMash()
        {
            AssetDatabase.StartAssetEditing();
            DirectoryInfo di = new DirectoryInfo(string.Format("{0}/Prefab", LoadMgr.singleton.editorResPath));
            var files = di.GetFiles("*.asset");
            for (int i = 0; i < files.Length; ++i)
            {
                var file = files[i];
                string path = file.FullName.Replace("\\", "/");
                int index = path.IndexOf(LoadMgr.singleton.editorResPath);
                path = path.Substring(index);
                CFEngine.PrefabRes pr = AssetDatabase.LoadAssetAtPath<CFEngine.PrefabRes>(path);
                if (pr != null && pr.meshes != null)
                {
                    for (int j = 0; j < pr.meshes.Length; ++j)
                    {
                        var emi = pr.meshes[j];
                        if (!emi.isSfx)
                        {
                            var mat = AssetDatabase.LoadAssetAtPath<Material>(emi.matPath);
                            if (mat != null)
                            {
                                MaterialContext context = new MaterialContext();
                                CalcMatHash(mat, ref context);
                                emi.matHash = context.matHash;
                                
                            }
                        }
                    }
                    EditorCommon.SaveAsset(pr);
                }
            }
            AssetDatabase.StopAssetEditing();
        }
    }
}
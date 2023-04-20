using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using ShaderUtility = CFEngine.ShaderUtility;

public class SFXMaterialReplace
{
    private class Reference
    {
        public Material material;
        public Shader shader;
        public HashSet<string> set = new HashSet<string>();

        public bool Get(Material material)
        {
            if (!material)
            {
                Debug.LogError("Material is null.");
                return false;
            }

            var shader = material.shader;
            if (!shader)
            {
                Debug.LogError("Shader is null.");
                return false;
            }

            this.material = material;
            this.shader = shader;
            set.Clear();

            return true;
        }
    }

    private static readonly Reference src = new Reference();
    private static readonly Reference dst = new Reference();
    private static readonly SavedBool sync = new SavedBool($"{nameof(SFXMaterialReplace)}.{nameof(sync)}");

    public static void Replace(Material srcMaterial, Material dstMateiral)
    {
        // Setup go
        if (src.Get(srcMaterial) && dst.Get(dstMateiral))
        {
            // transfer properties
            TransferProperties();

            // Do logs
            LogMissedProperties();

            Debug.Log($"Material Replace success, raw:{srcMaterial}, new:{dstMateiral}");
        }
        else
        {
            Debug.LogError("Material Replace fail.");
        }
    }

    public static void SyncPropertyToggle()
    {
        sync.Value = !sync.Value;
        Debug.Log("Sync " + (sync.Value ? "On" : "Off"));
    }

    private static void IgnoreProperty(Reference reference, string desc)
    {
        reference.set.Remove(desc);
    }

    private static void TransferProperties()
    {
        InitPropertySet(src);
        InitPropertySet(dst);

        // 打算干掉_MainRamp和_Contrast
        IgnoreProperty(src, "_UseMainRamp");
        IgnoreProperty(src, "_MainRamp.Texture");
        IgnoreProperty(src, "_MainRamp.Scale");
        IgnoreProperty(src, "_MainRamp.Offset");
        // 空属性
        IgnoreProperty(src, "_texcoord.Texture");
        IgnoreProperty(src, "_texcoord.Scale");
        IgnoreProperty(src, "_texcoord.Offset");
        IgnoreProperty(dst, "_Param4.w");

        // Shader mode
        bool materialMode = GetBool(src, "_ShaderMode");
        SetBool(dst, "_UseCustomData0.x", !materialMode);
        SetBool(dst, "_UseCustomData0.y", !materialMode);
        SetBool(dst, "_UseCustomData0.z", !materialMode);
        SetBool(dst, "_UseCustomData0.w", !materialMode);

        // Base Property
        float depthMode = GetFloat(src, "_DepthMode");
        CopyFloat("_BlendMode", "_BlendMode");
        CopyFloat("_CullMode", "_CullMode");
        CopyFloat("_ZTest", "_ZTest");
        SetFloat(dst, "_DepthMode", depthMode);
        CopyBool("_AlphaR", "_Param2.y");
        if (depthMode > 0)
        {
            CopyFloat("_CUTOUT", "_Param2.z");
        }
        else
        {
            MarkUsed(src, "_CUTOUT");
            SetFloat(dst, "_Param2.z", 0);
        }


        // Main Color
        // Note: 原shader的_BackColor.a不生效，逻辑上说属于BUG。
        CopyTexWithScaleOffset("_MainTex", "_MainTex");
        CopyColor("_MainColor", "_FrontColor");
        Color backColor = GetColor(src, "_BackColor");
        backColor.a = 1;
        SetColor(dst, "_BackColor", backColor);
        CopyBool("_UseBackColor", "_Param2.x");
        CopyFloat("_Brightness", "_Param1.w");
        CopyFloat("_Alpha", "_Param4.x");

        float contrast = GetFloat(src, "_Contrast");
        SetBool(dst, "_LegacyParam.x", contrast != 1);
        SetFloat(dst, "_Param4.z", contrast);

        // Mask
        bool useMask = GetBool(src, "_UseMask");
        SetBool(dst, "_Param1.z", useMask);
        CopyTexWithScaleOffset("_MaskTex", "_MaskTex");

        // Camera fade
        bool enableCameraFade = GetBool(src, "_CameraFade");
        string fadeIn = "_Param0.y";
        string fadeOut = "_Param0.z";
        if (enableCameraFade)
        {
            float offset = GetFloat(src, "_CameraFadeOffset");
            float length = GetFloat(src, "_CameraFadeLength");
            SetFloat(dst, fadeIn, offset);
            SetFloat(dst, fadeOut, offset + length);
        }
        else
        {
            MarkUsed(src, "_CameraFadeOffset");
            MarkUsed(src, "_CameraFadeLength");
            SetFloat(dst, fadeIn, 0);
            SetFloat(dst, fadeOut, -1);
        }

        // Ramp
        CopyBool("_UseRamp", "_Param1.y");
        Color rampColor0 = GetColor(src, "_Color0");
        Color rampColor1 = GetColor(src, "_Color1");
        Color rampColor2 = GetColor(src, "_Color2");
        rampColor0.a = 1;
        rampColor1.a = 1;
        rampColor2.a = 1;
        SetColor(dst, "_RampColor0", rampColor0);
        SetColor(dst, "_RampColor1", rampColor1);
        SetColor(dst, "_RampColor2", rampColor2);
        float rampParam0 = GetFloat(src, "_RampParam.x");
        float rampParam1 = GetFloat(src, "_RampParam.y");
        float rampParam2 = GetFloat(src, "_RampParam.z");
        float rampParam3 = GetFloat(src, "_RampParam.w");
        SetFloat(dst, "_RampShape.x", rampParam0);
        SetFloat(dst, "_RampShape.y", (rampParam1 - rampParam0));
        SetFloat(dst, "_RampShape.z", rampParam2);
        SetFloat(dst, "_RampShape.w", (rampParam3 - rampParam2));

        // Depth fade
        bool depthFadeEnable = GetBool(src, "_UseDepthFade");
        if (depthFadeEnable)
        {
            CopyFloat("_FadeLength", "_Param0.x");
        }
        else
        {
            MarkUsed(src, "_FadeLength");
            SetFloat(dst, "_Param0.x", 0);
        }

        // Fresnel -> Rim
        bool fresnelEnable = GetBool(src, "_Usefresnel");
        if (fresnelEnable)
        {
            CopyFloat("_fresnelpower", "_Param0.w");
            bool flip = GetBool(src, "_Flip");
            SetBool(dst, "_Param1.x", !flip);
            SetColor(dst, "_RimColor", new Color(1, 1, 1, 0));
            CopyFloat("_fresnelmultiply", "_Param3.x");
            SetVector(dst, "_RimMask", new Vector4(0, 0, 0, 1));
        }
        else
        {
            MarkUsed(src, "_fresnelpower");
            bool flip = GetBool(src, "_Flip");
            SetBool(dst, "_Param1.x", !flip);
            SetBool(dst, "_Param0.w", false);
            CopyFloat("_fresnelmultiply", "_Param3.x");
            SetColor(dst, "_RimColor", new Color(1, 1, 1, 0));
            SetVector(dst, "_RimMask", Vector4.one);
        }

        TextureWithScaleOffset turbulenceTex = GetTextureWithScaleOffset(src, "_TurbulenceTex");

        // Distortion
        bool enableDistortion = GetBool(src, "_UseTurbulence");
        SetBool(dst, "_Param2.w", enableDistortion);
        SetTextureWithScaleOffset(dst, "_DistortionTex", turbulenceTex);
        float distortionIntensity = GetFloat(src, "_DistortPower") * 0.5f;
        SetFloat(dst, "_Param3.y", distortionIntensity);
        float distoprtionUSpeed = GetFloat(src, "_PowerU");
        float distoprtionVSpeed = GetFloat(src, "_PowerV");
        SetFloat(dst, "_DistortionSpeed.x", distoprtionUSpeed);
        SetFloat(dst, "_DistortionSpeed.y", distoprtionVSpeed);
        SetFloat(dst, "_DistortionSpeed.z", distoprtionUSpeed);
        SetFloat(dst, "_DistortionSpeed.w", distoprtionVSpeed);

        // UseClip
        bool dissolutionEnable = GetBool(src, "_UseClip") || !materialMode;
        if (dissolutionEnable)
        {
            float progress = GetFloat(src, "_Dissolve");
            SetFloat(dst, "_DissolutionParam.x", progress);
            SetTextureWithScaleOffset(dst, "_DissolutionTex", turbulenceTex);
        }
        else
        {
            MarkUsed(src, "_Dissolve");
            SetFloat(dst, "_DissolutionParam.x", 1);
            SetTextureWithScaleOffset(dst, "_DissolutionTex", TextureWithScaleOffset.Empty);
        }
        float dissolutionHardness = GetFloat(src, "_Hardness");
        SetFloat(dst, "_DissolutionParam.y", dissolutionHardness);
        SetFloat(dst, "_DissolutionParam.w", dissolutionHardness);
        CopyFloat("_EdgeWidth", "_DissolutionParam.z");
        SetBool(dst, "_Param4.y", true);
        CopyColor("_WidthColor", "_DissolutionEdgeColor");
        CopyFloat("_MainPannerX", "_Param3.z");
        CopyFloat("_MainPannerY", "_Param3.w");

        EditorUtility.SetDirty(dst.material);
    }

    private struct TextureWithScaleOffset
    {
        public Texture texture;
        public Vector2 scale;
        public Vector2 offset;

        public static readonly TextureWithScaleOffset Empty = new TextureWithScaleOffset();
    }

    #region Getter and setter

    private static void SetBool(Reference reference, string desc, bool value, Func<bool, float> setter = null)
    {
        MaterialUtility.SetBool(reference.material, desc, value, setter);
        MarkUsed(reference, desc);
    }

    private static bool GetBool(Reference reference, string desc, Func<float, bool> getter = null)
    {
        MarkUsed(reference, desc);
        return MaterialUtility.GetBool(reference.material, desc, getter);
    }

    private static void SetFloat(Reference reference, string desc, float value)
    {
        MarkUsed(reference, desc);
        MaterialUtility.SetFloat(reference.material, desc, value);
    }

    private static float GetFloat(Reference reference, string desc)
    {
        MarkUsed(reference, desc);
        return MaterialUtility.GetFloat(reference.material, desc);
    }

    private static void SetVector(Reference reference, string desc, Vector4 value)
    {
        reference.material.SetVector(desc, value);
        MarkUsed(reference, desc);
    }

    private static Vector4 GetVector(Reference reference, string desc)
    {
        MarkUsed(reference, desc);
        return reference.material.GetVector(desc);
    }

    private static void SetColor(Reference reference, string desc, Color value)
    {
        reference.material.SetColor(desc, value);
        MarkUsed(reference, desc);
    }

    private static Color GetColor(Reference reference, string desc)
    {
        MarkUsed(reference, desc);
        return reference.material.GetColor(desc);
    }

    private static Vector2 GetTextureScale(Reference reference, string desc)
    {
        MarkUsed(reference, desc + ".Scale");
        return reference.material.GetTextureScale(desc);
    }

    private static Vector2 GetTextureOffset(Reference reference, string desc)
    {
        MarkUsed(reference, desc + ".Offset");
        return reference.material.GetTextureOffset(desc);
    }

    private static Texture GetTexture(Reference reference, string desc)
    {
        MarkUsed(reference, desc + ".Texture");
        return reference.material.GetTexture(desc);
    }

    private static TextureWithScaleOffset GetTextureWithScaleOffset(Reference reference, string name)
    {
        TextureWithScaleOffset info = new TextureWithScaleOffset();
        info.texture = GetTexture(reference, name);
        info.scale = GetTextureScale(reference, name);
        info.offset = GetTextureOffset(reference, name);
        return info;
    }

    private static void SetTextureScale(Reference reference, string desc, Vector2 value)
    {
        MarkUsed(reference, desc + ".Scale");
        reference.material.SetTextureScale(desc, value);
    }

    private static void SetTextureOffset(Reference reference, string desc, Vector2 value)
    {
        MarkUsed(reference, desc + ".Offset");
        reference.material.SetTextureOffset(desc, value);
    }

    private static void SetTexture(Reference reference, string desc, Texture value)
    {
        MarkUsed(reference, desc + ".Texture");
        reference.material.SetTexture(desc, value);
    }

    private static void SetTextureWithScaleOffset(Reference reference, string name, TextureWithScaleOffset value)
    {
        SetTexture(reference, name, value.texture);
        SetTextureScale(reference, name, value.scale);
        SetTextureOffset(reference, name, value.offset);
    }

    #endregion

    #region Copy

    private static void CopyBool(string srcName, string dstName, Func<float, bool> getter = null, Func<bool, float> setter = null)
    {
        bool value = GetBool(src, srcName, getter);
        SetBool(dst, dstName, value, setter);
    }

    private static void CopyColor(string srcName, string dstName)
    {
        Color value = GetColor(src, srcName);
        SetColor(dst, dstName, value);
    }

    private static void CopyFloat(string srcName, string dstName)
    {
        float value = GetFloat(src, srcName);
        SetFloat(dst, dstName, value);
    }

    private static void CopyVector(string srcName, string dstName)
    {
        Vector4 value = GetVector(src, srcName);
        SetVector(dst, dstName, value);
    }

    private static void CopyTexWithScaleOffset(string srcName, string dstName)
    {
        TextureWithScaleOffset info = GetTextureWithScaleOffset(src, srcName);
        SetTextureWithScaleOffset(dst, dstName, info);
    }

    #endregion

    #region Collect unused properties

    private static void InitPropertySet(Reference reference)
    {
        HashSet<string> set = reference.set;
        Material mat = reference.material;
        set.Clear();
        var shader = mat.shader;
        for (int pIndex = 0; pIndex < ShaderUtil.GetPropertyCount(shader); pIndex++)
        {
            string propertyName = ShaderUtil.GetPropertyName(shader, pIndex);
            var type = ShaderUtil.GetPropertyType(shader, pIndex);
            if (type == ShaderUtil.ShaderPropertyType.Vector)
            {
                set.Add(propertyName + ".x");
                set.Add(propertyName + ".y");
                set.Add(propertyName + ".z");
                set.Add(propertyName + ".w");
            }
            else if (type == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                set.Add(propertyName + ".Texture");
                set.Add(propertyName + ".Scale");
                set.Add(propertyName + ".Offset");
            }
            else
            {
                set.Add(propertyName);
            }
        }
    }

    private static void MarkUsed(Reference reference, string desc)
    {
        Shader shader = reference.shader;
        HashSet<string> set = reference.set;
        int dotIndex = desc.IndexOf('.');
        bool propertyExist = ShaderUtility.GetPropertyInfo(shader, desc, out ShaderUtility.PropertyInfo pInfo);
        bool isVector = propertyExist && pInfo.type == ShaderUtil.ShaderPropertyType.Vector;
        if (isVector && dotIndex < 0)
        {
            RemoveFromSet(shader, desc + ".x", set);
            RemoveFromSet(shader, desc + ".y", set);
            RemoveFromSet(shader, desc + ".z", set);
            RemoveFromSet(shader, desc + ".w", set);
        }
        else
        {
            RemoveFromSet(shader, desc, set);
        }
    }

    private static void RemoveFromSet(Shader shader, string desc, HashSet<string> set)
    {
        if (!set.Remove(desc))
        {
            Debug.LogError($"Property not exist, {shader.name}.{desc}");
        }
    }

    private static void LogMissedProperties()
    {
        LogMissedProperties(src);
        LogMissedProperties(dst);
    }

    private static void LogMissedProperties(Reference reference)
    {
        Shader shader = reference.shader;
        HashSet<string> set = reference.set;
        Dictionary<ShaderUtility.PropertyInfo, int> componentMap = new Dictionary<ShaderUtility.PropertyInfo, int>();
        List<ShaderUtility.PropertyInfo> properties = new List<ShaderUtility.PropertyInfo>();
        foreach (string desc in set)
        {
            int dotIndex = desc.IndexOf('.');
            if (dotIndex > 0)
            {
                string[] temp = desc.Split('.');
                string propertyName = temp[0];
                ShaderUtility.GetPropertyInfo(shader, propertyName, out var pInfo);
                if (pInfo.type == ShaderUtil.ShaderPropertyType.Vector)
                {
                    componentMap.TryGetValue(pInfo, out int bitMask);
                    if (!MaterialUtility.GetComponentIndex(temp[1][0], out int component))
                    {
                        Debug.LogError($"Unknow component type \"{temp[1]}\"");
                        continue;
                    }
                    bitMask |= 1 << component;
                    componentMap[pInfo] = bitMask;
                    if (!properties.Contains(pInfo))
                        properties.Add(pInfo);
                }
                else if (pInfo.type == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    componentMap.TryGetValue(pInfo, out int bitMask);
                    if (temp[1] == "Texture")
                    {
                        bitMask |= 1 << 4;
                    }
                    else if (temp[1] == "Scale")
                    {
                        bitMask |= 1 << 5;
                    }
                    else if (temp[1] == "Offset")
                    {
                        bitMask |= 1 << 6;
                    }
                    else
                    {
                        Debug.LogError($"Unknow component type \"{temp[1]}\"");
                        continue;
                    }
                    componentMap[pInfo] = bitMask;
                    if (!properties.Contains(pInfo))
                        properties.Add(pInfo);
                }
                else
                {
                    Debug.LogError($"Unknow property type with component: type = {pInfo.type}, component = \"{temp[1]}\"");
                    continue;
                }
            }
            else
            {
                ShaderUtility.GetPropertyInfo(shader, desc, out var pInfo);
                if (!properties.Contains(pInfo))
                    properties.Add(pInfo);
            }
        }

        properties.Sort((a, b) => a.index.CompareTo(b.index));

        if (properties.Count > 0)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Material({reference.material}) Shader({shader}) Missed property list ({properties.Count}):");
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                string[] componentNames = new string[]
                {
                    "x", "y", "z", "w", "Texture", "Scale", "Offset"
                };
                if (componentMap.TryGetValue(property, out int componentMask))
                {
                    string componentsDesc = ".";
                    int lastIndex = -1;
                    for (int componentIndex = 0; componentIndex < componentNames.Length; componentIndex++)
                    {
                        if ((componentMask & (1 << componentIndex)) > 0)
                        {
                            if (lastIndex >= 4 && componentIndex >= 4)
                            {
                                componentsDesc += ",";
                            }
                            componentsDesc += componentNames[componentIndex];
                            lastIndex = componentIndex;
                        }
                    }
                    if (property.type == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        stringBuilder.AppendLine($"[{i}/{properties.Count}] : {property.name}{componentsDesc}");
                    }
                    stringBuilder.AppendLine($"[{i}/{properties.Count}] : {property.name}{componentsDesc}");
                }
                else
                {
                    stringBuilder.AppendLine($"[{i}/{properties.Count}] : {property.name}");
                }
            }
            Debug.Log(stringBuilder.ToString());
        }
    }

    #endregion
}

using System;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 材质解析器。
/// * 使用后记得调用Apply。
/// * 解析器只用来查询和擦除原有数据。
/// * Set功能请使用默认接口。
/// </summary>
public class MaterialDeserizer
{
    public SerializedObject serializedObject;
    private const string LIGHTMAP_FLAGS = "m_LightmapFlags";
    private const string ENABLE_INSTANCING_VARIANTS = "m_EnableInstancingVariants";
    private const string DOUBLE_SIDED_GI = "m_DoubleSidedGI";
    private const string CUSTOM_RENDER_QUEUE = "m_CustomRenderQueue";
    private const string STRING_TAG_MAP = "stringTagMap";
    private const string DISABLED_SHADER_PASSES = "disabledShaderPasses";
    private const string PROPERTIES = "m_SavedProperties";
    private const string TEXTURES = PROPERTIES + ".m_TexEnvs";
    private const string FLOATS = PROPERTIES + ".m_Floats";
    private const string COLOR_VECTORS = PROPERTIES + ".m_Colors";
    private const string KEYWORDS = "m_ShaderKeywords";
    private const string SHADER = "m_Shader";
    private const string FIRST = "first";
    private const string SECOND = "second";

    [Flags]
    public enum ClearFlag : uint
    {
        Shader = 1 << 0,
        Keywords = 1 << 1,
        Textures = 1 << 2,
        ColorVectors = 1 << 3,
        Floats = 1 << 4,
        LightmapFlags = 1 << 5,
        EnableInstancingVariants = 1 << 6,
        DoubleSidedGI = 1 << 7,
        CustomRenderQueue = 1 << 8,
        StringTagMap = 1 << 9,
        DisabledShaderPasses = 1 << 10,

        None = 0,
        Properties = Textures | ColorVectors | Floats,
        All = ~0u,
    }

    public MaterialDeserizer(Material material)
    {
        serializedObject = new SerializedObject(material);
    }

    public void ReplaceName(string newPropertyName, string oldPropertyName)
    {
        bool result = GetFloat(oldPropertyName, out int index, out float value);
    }

    public int GetTextureCount()
    {
        return serializedObject.FindProperty(TEXTURES).arraySize;
    }

    public bool GetTextureData<T>(string propertyName, out int index, out T texture, out Vector2 scale,
        out Vector2 offset) where T : Texture
    {
        bool result = GetProperty(TEXTURES, propertyName, out index, out var second);
        GetTextureData(result, second, out texture, out scale, out offset);
        return result;
    }

    public bool GetTextureData<T>(int index, out string propertyName, out T texture, out Vector2 scale,
        out Vector2 offset) where T : Texture
    {
        bool result = GetProperty(TEXTURES, index, out propertyName, out var second);
        GetTextureData(result, second, out texture, out scale, out offset);
        return result;
    }

    private static void GetTextureData<T>(bool result, SerializedProperty second, out T texture, out Vector2 scale,
        out Vector2 offset) where T : Texture
    {
        texture = result ? second.FindPropertyRelative("m_Texture").objectReferenceValue as T : default;
        scale = result ? second.FindPropertyRelative("m_Scale").vector2Value : default;
        offset = result ? second.FindPropertyRelative("m_Offset").vector2Value : default;
    }

    public int GetVectorCount()
    {
        return serializedObject.FindProperty(COLOR_VECTORS).arraySize;
    }

    public bool GetVector4(string propertyName, out int index, out Vector4 value)
    {
        bool result = GetProperty(COLOR_VECTORS, propertyName, out index, out var second);
        value = result ? second.vector4Value : default;
        return result;
    }

    public bool GetVector4(int index, out string propertyName, out Vector4 value)
    {
        bool result = GetProperty(COLOR_VECTORS, index, out propertyName, out var second);
        value = result ? second.vector4Value : default;
        return result;
    }

    public bool GetVector3(string propertyName, out int index, out Vector3 value)
    {
        bool result = GetProperty(COLOR_VECTORS, propertyName, out index, out var second);
        value = result ? second.vector3Value : default;
        return result;
    }

    public bool GetVector3(int index, out string propertyName, out Vector3 value)
    {
        bool result = GetProperty(COLOR_VECTORS, index, out propertyName, out var second);
        value = result ? second.vector3Value : default;
        return result;
    }

    public bool GetVector2(string propertyName, out int index, out Vector2 value)
    {
        bool result = GetProperty(COLOR_VECTORS, propertyName, out index, out var second);
        value = result ? second.vector2Value : default;
        return result;
    }

    public bool GetVector2(int index, out string propertyName, out Vector2 value)
    {
        bool result = GetProperty(COLOR_VECTORS, index, out propertyName, out var second);
        value = result ? second.vector2Value : default;
        return result;
    }

    public int GetFloatCount()
    {
        return serializedObject.FindProperty(FLOATS).arraySize;
    }

    public bool GetFloat(string propertyName, out int index, out float value)
    {
        bool result = GetProperty(FLOATS, propertyName, out index, out var second);
        value = result ? second.floatValue : default;
        return result;
    }

    public bool GetFloat(int index, out string propertyName, out float value)
    {
        bool result = GetProperty(FLOATS, index, out propertyName, out var second);
        value = result ? second.floatValue : default;
        return result;
    }

    private bool GetProperty(string arrayPath, int index, out string propertyName, out SerializedProperty second)
    {
        var array = serializedObject.FindProperty(arrayPath);
        if (index > 0 && index < array.arraySize)
        {
            var property = array.GetArrayElementAtIndex(index);
            propertyName = property.FindPropertyRelative(FIRST).stringValue;
            second = property.FindPropertyRelative(SECOND);
            return true;
        }

        propertyName = default;
        second = default;
        return false;
    }

    private bool GetProperty(string arrayPath, string propertyName, out int index, out SerializedProperty second)
    {
        var array = serializedObject.FindProperty(arrayPath);
        for (int i = 0; i < array.arraySize; i++)
        {
            var property = array.GetArrayElementAtIndex(i);
            if (property.FindPropertyRelative(FIRST).stringValue == propertyName)
            {
                index = i;
                second = property.FindPropertyRelative(SECOND);
                return true;
            }
        }

        index = default;
        second = default;
        return false;
    }

    public bool RemoveProperty(string propertyName)
    {
        if (TryRemovePropertyInArray(propertyName, TEXTURES))
            return true;
        if (TryRemovePropertyInArray(propertyName, FLOATS))
            return true;
        if (TryRemovePropertyInArray(propertyName, COLOR_VECTORS))
            return true;
        return false;
    }

    private bool TryRemovePropertyInArray(string propertyName, string arrayPath)
    {
        SerializedProperty array = serializedObject.FindProperty(arrayPath);
        for (int i = 0; i < array.arraySize; i++)
        {
            var property = array.GetArrayElementAtIndex(i);
            var first = property.FindPropertyRelative(FIRST);
            if (first != null && first.stringValue == propertyName)
            {
                array.DeleteArrayElementAtIndex(i);
                return true;
            }
        }

        return false;
    }

    public void RemoveUnusedProperties()
    {
        void RemoveUnusedPropertyInArray(Dictionary<string, ShaderUtility.PropertyInfo> map, string arrayName,
            params ShaderUtil.ShaderPropertyType[] types)
        {
            var array = serializedObject.FindProperty(arrayName);
            for (int i = 0; i < array.arraySize; i++)
            {
                var element = array.GetArrayElementAtIndex(i);
                if (!map.TryGetValue(element.FindPropertyRelative(FIRST).stringValue, out var pInfo)
                    || Array.IndexOf(types, pInfo.type) < 0)
                {
                    array.DeleteArrayElementAtIndex(i--);
                }
            }
        }

        Shader shader = serializedObject.FindProperty(SHADER).objectReferenceValue as Shader;
        if (ShaderUtility.GetShaderInfo(shader, out ShaderUtility.ShaderInfo info))
        {
            RemoveUnusedPropertyInArray(info.properties, COLOR_VECTORS, ShaderUtil.ShaderPropertyType.Color,
                ShaderUtil.ShaderPropertyType.Vector);
            RemoveUnusedPropertyInArray(info.properties, FLOATS, ShaderUtil.ShaderPropertyType.Float,
                ShaderUtil.ShaderPropertyType.Range);
            RemoveUnusedPropertyInArray(info.properties, TEXTURES, ShaderUtil.ShaderPropertyType.TexEnv);
        }
    }

    public void Clear(ClearFlag flags = ClearFlag.All)
    {
        if ((flags & ClearFlag.Shader) > 0)
            serializedObject.FindProperty(SHADER).objectReferenceValue = null;
        if ((flags & ClearFlag.Keywords) > 0)
            serializedObject.FindProperty(KEYWORDS).stringValue = default;
        if ((flags & ClearFlag.Textures) > 0)
            serializedObject.FindProperty(TEXTURES).ClearArray();
        if ((flags & ClearFlag.ColorVectors) > 0)
            serializedObject.FindProperty(COLOR_VECTORS).ClearArray();
        if ((flags & ClearFlag.Floats) > 0)
            serializedObject.FindProperty(FLOATS).ClearArray();
        if ((flags & ClearFlag.LightmapFlags) > 0)
            serializedObject.FindProperty(LIGHTMAP_FLAGS).floatValue = default;
        if ((flags & ClearFlag.EnableInstancingVariants) > 0)
            serializedObject.FindProperty(ENABLE_INSTANCING_VARIANTS).boolValue = default;
        if ((flags & ClearFlag.DoubleSidedGI) > 0)
            serializedObject.FindProperty(DOUBLE_SIDED_GI).boolValue = default;
        if ((flags & ClearFlag.CustomRenderQueue) > 0)
            serializedObject.FindProperty(CUSTOM_RENDER_QUEUE).floatValue = -1;
        if ((flags & ClearFlag.StringTagMap) > 0)
            serializedObject.FindProperty(STRING_TAG_MAP).ClearArray();
        if ((flags & ClearFlag.DisabledShaderPasses) > 0)
            serializedObject.FindProperty(DISABLED_SHADER_PASSES).ClearArray();
    }

    public void Apply()
    {
        serializedObject.ApplyModifiedProperties();
    }
}
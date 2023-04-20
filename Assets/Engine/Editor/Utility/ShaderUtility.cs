using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using PropertyType = UnityEditor.ShaderUtil.ShaderPropertyType;
using TextureDimension = UnityEngine.Rendering.TextureDimension;

namespace CFEngine
{
    public struct ShaderKey
    {
        public string name;
        public Shader shader;

        public static implicit operator ShaderKey(Shader shader)
        {
            return new ShaderKey()
            {
                shader = shader,
                name = shader.name
            };
        }

        public static implicit operator Shader(ShaderKey shader)
        {
            return shader.shader;
        }

        public static implicit operator ShaderKey(string name)
        {
            return new ShaderKey()
            {
                name = name,
                shader = Shader.Find(name)
            };
        }

        public static implicit operator string(ShaderKey shader)
        {
            return shader.name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is Shader)
            {
                return (obj as Shader) == shader;
            }

            if (obj is string)
            {
                return (obj as string) == name;
            }

            if (obj is ShaderKey)
            {
                return ((ShaderKey)obj).shader == shader;
            }

            return false;
        }

        public static implicit operator bool(ShaderKey key)
        {
            return key.shader;
        }
    }

    public static class ShaderUtility
    {
        public static readonly Dictionary<ShaderKey, ShaderInfo> map = new Dictionary<ShaderKey, ShaderInfo>();
        public static event Action<Shader> onShaderReimport;

        public class ShaderInfo
        {
            public string name;
            public Shader shader;
            public PropertyType type;
            public Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

            private HashSet<string> _keywords;

            public HashSet<string> keywords
            {
                get
                {
                    if (_keywords == null && !ParseKeywords(shader, out _keywords))
                        return null;
                    return _keywords;
                }
            }
        }

        public class PropertyInfo
        {
            public string name;
            public PropertyType type;
            public int index;
            public TextureDimension dimiension;
        }

        public static bool GetShaderInfo(ShaderKey shader, out ShaderInfo info)
        {
            if (!shader)
            {
                info = null;
                return false;
            }

            if (!map.TryGetValue(shader, out info))
            {
                info = new ShaderInfo()
                {
                    shader = shader,
                    name = shader.name
                };

                map[shader] = info;
                info.name = shader.name;
                info.shader = shader;

                int count = ShaderUtil.GetPropertyCount(shader.shader);
                for (int pIndex = 0; pIndex < count; pIndex++)
                {
                    PropertyInfo pInfo = new PropertyInfo();
                    pInfo.name = ShaderUtil.GetPropertyName(shader, pIndex);
                    pInfo.type = ShaderUtil.GetPropertyType(shader, pIndex);
                    if (pInfo.type == PropertyType.TexEnv)
                        pInfo.dimiension = ShaderUtil.GetTexDim(shader, pIndex);
                    pInfo.index = pIndex;
                    if (info.properties.ContainsKey(pInfo.name))
                    {
                        Debug.LogError($"Shader属性重复：{shader.name} : {pInfo.name}");
                    }
                    else
                    {
                        info.properties.Add(pInfo.name, pInfo);
                    }
                }
            }

            return true;
        }

        public static bool GetPropertyInfo(ShaderKey shader, string property, out PropertyInfo result)
        {
            if (GetShaderInfo(shader, out ShaderInfo sInfo))
            {
                return sInfo.properties.TryGetValue(property, out result);
            }
            result = default;
            return false;
        }

        public static bool ParseKeywords(Shader shader, out HashSet<string> result)
        {
            result = null;
            string path = AssetDatabase.GetAssetPath(shader);
            if (!path.StartsWith("Assets/"))
                return false;

            result = new HashSet<string>();
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                line = line.Trim();
                int cutIndex = line.IndexOf("//");
                if (cutIndex >= 0)
                    line = line.Substring(0, cutIndex);
                const string pragma = "#pragma";
                if (!line.StartsWith(pragma))
                    continue;
                line = line.Substring(pragma.Length, line.Length - pragma.Length);
                line = line.Trim();
                string[] tokens = line.Split(' ', '\t');
                if (tokens.Length < 2)
                    continue;
                if (!tokens[0].StartsWith("multi_compile") && !tokens[0].StartsWith("shader_feature"))
                    continue;
                
                for (int j = 1; j < tokens.Length; j++)
                {
                    string token = tokens[j]; 
                    if (token == "_" || token == "__")
                        continue;
                    result.Add(token);
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"ParseShaderKeyword({shader}):");
            foreach (string keyword in result)
            {
                stringBuilder.Append(keyword);
                stringBuilder.Append(", ");
            }
            Debug.Log(stringBuilder.ToString());

            return true;
        }
        
        private class ShaderPostProcessor : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                if (!EngineUtility.AutoAssetPostprocessor) return;

                foreach (string assetPath in importedAssets)
                {
                    if (assetPath.EndsWith(".shader"))
                    {
                        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
                        if (shader)
                        {
                            map.Remove(shader.name);
                            onShaderReimport?.Invoke(shader);
                        }
                    }
                }
            }
        }
    }
}

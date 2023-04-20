using System;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine.Editor
{
    public class MaterialReplaceConfig : ScriptableObject
    {
        public Shader srcShader;
        public Shader dstShader;
        public List<PropertyReplaceConfig> properties = new List<PropertyReplaceConfig>();
        public List<string> refDirectories = new List<string>();
        public List<MaterialLocation> matReplaceLocations = new List<MaterialLocation>();
        
        [SerializeField]
        private string processorType;
        private MaterialReplaceProcessor processor;
        public MaterialReplaceProcessor Processor
        {
            get
            {
                if (processor != null && processor.GetType().FullName == processorType)
                    return processor;
                if (string.IsNullOrEmpty(processorType))
                    return null;
                var type = GetType().Assembly.GetType(processorType);
                if (type == null)
                    return null;
                return Activator.CreateInstance(type) as MaterialReplaceProcessor;
            }
            set
            {
                processor = value;
                processorType = processor == null ? string.Empty : processor.GetType().FullName;
            }
        }
    }

    [System.Serializable]
    public class PropertyReplaceConfig
    {
        public string srcName = string.Empty;
        public string dstName = string.Empty;
        public string srcDesc = string.Empty;
        public string dstDesc = string.Empty;
    }

    [System.Serializable]
    public class MaterialLocation
    {
        public string matPath;
        public string assetPath;
        public string componentPath;
        public int materialIndex;

        public MaterialLocation(string matPath, string assetPath, string componentPath, int materialIndex)
        {
            this.matPath = matPath;
            this.assetPath = assetPath;
            this.componentPath = componentPath;
            this.materialIndex = materialIndex;
        }

        public override string ToString()
        {
            return $"material:{matPath}, referece:{assetPath}, component:{componentPath}, index:{materialIndex}";
        }
    }
}

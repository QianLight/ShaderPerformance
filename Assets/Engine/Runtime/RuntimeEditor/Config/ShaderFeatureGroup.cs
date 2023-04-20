#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace CFEngine
{
    [System.Serializable]
    public class ShaderFeatureGroup : ScriptableObject
    {

        public List<ShaderFeatureBlock> shaderFeatureBlocks = new List<ShaderFeatureBlock> ();

        [NonSerialized]
        public int index = 0;
        private string sfgName = "";
        public string groupName
        {
            get
            {
                if (string.IsNullOrEmpty (sfgName))
                {
                    sfgName = name;
                    int index = sfgName.LastIndexOf ("_");
                    if (index >= 0)
                    {
                        sfgName = sfgName.Substring (index + 1);
                    }
                }
                return sfgName;
            }
        }
        public void Export (XmlDocument doc, XmlElement ShaderFeatureGroupRef)
        {
            for (int i = 0; i < shaderFeatureBlocks.Count; ++i)
            {
                XmlElement ShaderFeatureBundle = doc.CreateElement ("ShaderFeatureBundle");
                var sfBundle = shaderFeatureBlocks[i];
                sfBundle.Export (doc, ShaderFeatureBundle);
                ShaderFeatureGroupRef.AppendChild (ShaderFeatureBundle);
            }
        }
        public void Import (XmlElement ShaderFeatureGroupRef)
        {
            XmlNodeList childs = ShaderFeatureGroupRef.ChildNodes;
            shaderFeatureBlocks.Clear ();

            for (int i = 0; i < childs.Count; ++i)
            {
                XmlElement sfBundle = childs[i] as XmlElement;
                ShaderFeatureBlock sfb = new ShaderFeatureBlock ();
                sfb.Import (sfBundle);
                shaderFeatureBlocks.Add (sfb);
            }
        }
    }
}

#endif
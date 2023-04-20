#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public enum BlendMode
    {
        Opaque = 0x0001,
        Cutout = 0x0002,
        //CutoutTransparent = 0x0004,
        Transparent = 0x0008, // Physically plausible transparency mode, implemented as alpha pre-multiply
        DepthTransparent = 0x0010,
    }

    public enum EShaderCustomValueToggle
    {
        None,
        ValueToggle,
        Keyword,
        Toggle,
        Min,
        Max,
    }

    public delegate bool FloatCompFun (float v0, float v1);
    public enum EValueCmpType
    {
        Equel,
        NotEquel,
        Greater,
        GEquel,
        Less,
        LEquel,
    }

    public enum EVectorValueIndex
    {
        X,
        Y,
        Z,
        W
    }
    public enum ShaderPropertyType
    {
        CustomFun = 0,
        Tex,
        Color,
        Vector,
        Keyword, //Keyword
        Custom,
        ValueToggle,
        CustomExpression,
        // RenderQueue,
    }
    public enum DependencyType
    {
        Or,
        And,
    }

    public enum DebugDisplayType
    {
        Full,
        Split,
    }

    [System.Serializable]
    public class ShaderDebugContext
    {
        public DebugDisplayType debugDisplayType = DebugDisplayType.Full;
        public float splitLeft = -1;
        public float splitRight = 1;
        [CFRange (-45, 45, 0)]
        public float splitAngle = 0;
        public float splitPos = 0;
        [NonSerialized]
        public int debugMode = 0;
        [NonSerialized]
        public int convertDebugMode = 0;
        [NonSerialized]
        public bool modeModify = false;
        [NonSerialized]
        public bool typeModify = false;
        [NonSerialized]
        public bool angleModify = false;
        [NonSerialized]
        public bool posModify = false;
        [NonSerialized]
        public int[] shaderID = null;
        public void Reset ()
        {
            debugMode = 0;
            convertDebugMode = 0;
            splitAngle = 0;
            splitPos = 0;
            modeModify = true;
            typeModify = true;
            angleModify = true;
            posModify = true;
        }
        public void Refresh ()
        {
            if (shaderID != null)
            {
                if (angleModify)
                    CalcSplitLeftRight ();
                if (modeModify)
                {
                    Shader.SetGlobalFloat (shaderID[0], (float) convertDebugMode);
                    modeModify = false;
                }
                if (typeModify)
                {
                    Shader.SetGlobalInt (shaderID[1], (int) debugDisplayType);
                    typeModify = false;
                }
                if (angleModify)
                {
                    float k = Mathf.Tan (Mathf.Deg2Rad * (90 - splitAngle));
                    Shader.SetGlobalVector (shaderID[2], new Vector2 (k, k < 0 ? -1 : 1));
                    angleModify = false;
                }
                if (posModify)
                {
                    float k = Mathf.Tan (Mathf.Deg2Rad * (90 - splitAngle));
                    float b = -k * splitPos;
                    Shader.SetGlobalFloat (shaderID[3], b);
                    posModify = false;
                }
            }
        }

        private void CalcSplitLeftRight ()
        {
            float k = Mathf.Tan (Mathf.Deg2Rad * (90 - splitAngle));
            float b = 1 + k;
            splitLeft = -b / k;
            splitRight = -splitLeft;

        }
    }

    public static class ShaderHDRVisualize
    {
        public static readonly Vector4[] visualizeColors = new Vector4[8]
        {
            new Vector4(0.0f, 1.0f , 0.0f, 1.0f),
            new Vector4(0.5f, 1.0f , 0.0f, 1.0f),
            new Vector4(1.0f, 1.0f , 0.0f, 1.0f),
            new Vector4(1.0f, 0.5f , 0.0f, 1.0f),
            new Vector4(1.0f, 0.0f , 0.0f, 1.0f),
            new Vector4(1.0f, 0.0f , 1.0f, 1.0f),
            new Vector4(0.0f, 0.0f , 1.0f, 1.0f),
            new Vector4(0.0f, 1.0f , 1.0f, 1.0f),
        };
        private static float[] visualizeGraphData;
        private static float numberAlpha = 1;

        public static float[] VisualizeGraphData
        {
            get
            {
                if (visualizeGraphData == null)
                {
                    int[][] visualizeGraphRawData = new int[][]
                    {
                        #region Visualize Raw Data
                        new int[256]
                        {
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,
                            0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,0,
                            0,0,1,1,1,0,0,0,0,0,0,1,1,1,0,0,
                            0,0,1,1,1,0,0,0,0,0,0,1,1,1,0,0,
                            0,0,1,1,1,0,0,0,0,0,0,1,1,1,0,0,
                            0,0,1,1,1,0,0,0,0,0,0,1,1,1,0,0,
                            0,0,1,1,1,0,0,0,0,0,0,1,1,1,0,0,
                            0,0,1,1,1,0,0,0,0,0,0,1,1,1,0,0,
                            0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,
                            0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        },
                        new int[256]
                        {
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,0,1,1,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,1,1,1,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,1,1,1,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        },
                        new int[256]
                        {
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,
                            0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,
                            0,0,1,1,1,1,1,0,0,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,0,
                            0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,
                            0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,
                            0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,
                            0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,
                            0,0,0,0,0,1,1,1,1,1,1,0,0,0,0,0,
                            0,0,0,1,1,1,1,1,1,0,0,0,0,0,0,0,
                            0,0,1,1,1,1,1,0,0,0,0,0,0,0,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        },
                        new int[256]
                        {
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,
                            0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,
                            0,0,1,1,1,1,1,0,0,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,0,
                            0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,
                            0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,
                            0,0,0,0,0,1,1,1,1,1,1,0,0,0,0,0,
                            0,0,0,0,0,1,1,1,1,1,1,0,0,0,0,0,
                            0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,
                            0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,
                            0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,0,
                            0,0,1,1,1,1,0,0,0,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,0,0,0,
                            0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        },
                        new int[256]
                        {
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,0,0,1,1,1,0,1,1,1,0,0,0,0,0,
                            0,0,0,0,1,1,1,0,1,1,1,0,0,0,0,0,
                            0,0,0,0,1,1,1,0,1,1,1,0,0,0,0,0,
                            0,0,0,1,1,1,1,0,1,1,1,0,0,0,0,0,
                            0,0,0,1,1,1,0,0,1,1,1,0,0,0,0,0,
                            0,0,1,1,1,1,0,0,1,1,1,0,0,0,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,
                            0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,
                            0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,
                            0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,
                            0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        },
                        new int[256]
                        {
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,1,1,1,0,1,1,1,1,1,1,1,0,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,
                            0,0,1,1,1,0,0,0,0,0,0,1,1,1,1,0,
                            0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,
                            0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        },
                        new int[256]
                        {
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,
                            0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,
                            0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,
                            0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,
                            0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,
                            0,0,0,1,1,1,1,1,1,1,1,1,0,0,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,0,0,0,
                            0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,0,
                            0,0,1,1,1,0,0,0,0,0,0,1,1,1,0,0,
                            0,0,1,1,1,0,0,0,0,0,0,1,1,1,0,0,
                            0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,0,
                            0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,
                            0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        },
                        new int[256]
                        {
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,
                            0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,
                            0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,
                            0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,
                            0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,
                            0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,
                            0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,
                            0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,
                            0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,
                            0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,
                            0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,
                            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        },
                        #endregion
                    };

                    int[] temp = new int[256];
                    for (int index = 0; index < 256; index++)
                    {
                        for (int exp = 0; exp < 8; exp++)
                        {
                            temp[index] |= visualizeGraphRawData[exp][index] * (1 << exp);
                        }
                    }

                    visualizeGraphData = new float[256];
                    for (int i = 0; i < 256; i++)
                    {
                        visualizeGraphData[i] = temp[i];
                    }
                }
                return visualizeGraphData;
            }
        }

        [InitializeOnLoadMethod]
        public static void RefreshVisualizeColor()
        {
            Shader.SetGlobalVectorArray("_HDRVisualizeColors", visualizeColors);
            Shader.SetGlobalFloatArray("_HDRVisualizeGraph", VisualizeGraphData);
            Shader.SetGlobalFloat("_HDRVisualizeAlpha", numberAlpha);
        }

        public static void SetNumberAlpha(float alpha)
        {
            numberAlpha = alpha;
            Shader.SetGlobalFloat("_HDRVisualizeAlpha", numberAlpha);
        }
    }

    [System.Serializable]
    public class ShaderPropertyDependency
    {
        public bool isNor = false;
        public DependencyType dependencyType = DependencyType.Or;
        public List<string> dependencyShaderProperty = new List<string> ();

        public void Clone (ShaderPropertyDependency src)
        {
            isNor = src.isNor;
            dependencyType = src.dependencyType;
            dependencyShaderProperty.Clear ();
            dependencyShaderProperty.AddRange (src.dependencyShaderProperty);
        }
        public void Export (XmlDocument doc, XmlElement ShaderPropertyDependency)
        {
            ShaderPropertyDependency.SetAttribute ("isNor", isNor? "1": "0");
            ShaderPropertyDependency.SetAttribute ("dependencyType", ((int) dependencyType).ToString ());
            for (int i = 0; i < dependencyShaderProperty.Count; ++i)
            {
                XmlElement d = doc.CreateElement ("dependencyShaderProperty");
                var dsp = dependencyShaderProperty[i];
                d.SetAttribute ("Property", dsp);
                ShaderPropertyDependency.AppendChild (d);
            }
        }
        public void Import (XmlElement ShaderPropertyDependency)
        {
            isNor = ShaderPropertyDependency.GetAttribute ("isNor") == "1";
            dependencyType = (DependencyType) int.Parse (ShaderPropertyDependency.GetAttribute ("dependencyType"));
            XmlNodeList childs = ShaderPropertyDependency.ChildNodes;
            dependencyShaderProperty.Clear ();
            for (int i = 0; i < childs.Count; ++i)
            {
                XmlElement d = childs[i] as XmlElement;
                string dep = d.GetAttribute ("Property");
                dependencyShaderProperty.Add (dep);
            }
        }
    }

    [System.Serializable]
    public class ShaderCustomProperty
    {
        public bool valid = false;
        public string desc;
        public float defaultValue = 0.0f;
        public float min = 0.0f;
        public float max = 1.0f;
        public int index = -1;
        public bool intValue = false;
        public EShaderCustomValueToggle toggleType = EShaderCustomValueToggle.None;

        public string valueName = "";
        public float disableValue = 0;
        public float enableValue = 1;
        public EValueCmpType valueCmpType = EValueCmpType.Equel;
        public float thresholdValue = 0;
        public static FloatCompFun[] cmpFun = new FloatCompFun[]
        {
            delegate (float v0, float v1) { return v0 == v1; },
                delegate (float v0, float v1) { return v0 != v1; },
                delegate (float v0, float v1) { return v0 > v1; },
                delegate (float v0, float v1) { return v0 >= v1; },
                delegate (float v0, float v1) { return v0 < v1; },
                delegate (float v0, float v1) { return v0 <= v1; }
        };

        public void Clone (ShaderCustomProperty src)
        {
            valid = src.valid;
            desc = src.desc;
            defaultValue = src.defaultValue;
            min = src.min;
            max = src.max;
            index = src.index;
            toggleType = src.toggleType;
            valueName = src.valueName;
            disableValue = src.disableValue;
            enableValue = src.enableValue;
            valueCmpType = src.valueCmpType;
            thresholdValue = src.thresholdValue;
        }
        public void Export (XmlDocument doc, XmlElement ShaderCustomProperty)
        {
            ShaderCustomProperty.SetAttribute ("Valid", valid? "1": "0");
            ShaderCustomProperty.SetAttribute ("Desc", desc);
            ShaderCustomProperty.SetAttribute ("DefaultValue", defaultValue.ToString ());
            ShaderCustomProperty.SetAttribute ("Min", min.ToString ());
            ShaderCustomProperty.SetAttribute ("Max", max.ToString ());
            ShaderCustomProperty.SetAttribute ("Index", index.ToString ());
            ShaderCustomProperty.SetAttribute ("ToggleType", ((int) toggleType).ToString ());
            ShaderCustomProperty.SetAttribute ("ValueName", valueName);
            ShaderCustomProperty.SetAttribute ("DisableValue", disableValue.ToString ());
            ShaderCustomProperty.SetAttribute ("EnableValue", enableValue.ToString ());
            ShaderCustomProperty.SetAttribute ("ValueCmpType", ((int) valueCmpType).ToString ());
            ShaderCustomProperty.SetAttribute ("ThresholdValue", thresholdValue.ToString ());
        }
        public void Import (XmlElement ShaderCustomProperty)
        {
            valid = ShaderCustomProperty.GetAttribute ("Valid") == "1";
            desc = ShaderCustomProperty.GetAttribute ("Desc");
            defaultValue = float.Parse (ShaderCustomProperty.GetAttribute ("DefaultValue"));
            min = float.Parse (ShaderCustomProperty.GetAttribute ("Min"));
            max = float.Parse (ShaderCustomProperty.GetAttribute ("Max"));
            index = int.Parse (ShaderCustomProperty.GetAttribute ("Index"));
            toggleType = (EShaderCustomValueToggle) int.Parse (ShaderCustomProperty.GetAttribute ("ToggleType"));
            valueName = ShaderCustomProperty.GetAttribute ("ValueName");
            disableValue = float.Parse (ShaderCustomProperty.GetAttribute ("DisableValue"));
            enableValue = float.Parse (ShaderCustomProperty.GetAttribute ("EnableValue"));
            valueCmpType = (EValueCmpType) int.Parse (ShaderCustomProperty.GetAttribute ("ValueCmpType"));
            thresholdValue = float.Parse (ShaderCustomProperty.GetAttribute ("ThresholdValue"));
        }
    }

    [System.Serializable]
    public class ShaderFeature : BaseCopy<ShaderFeature>, IFolderHash
    {
        public string name = "empty";
        public string propertyName = "";
        public ShaderPropertyType type = ShaderPropertyType.Custom;
        public Color defaultColor = Color.white;
        public uint flag = 0;
        public static uint Flag_Hide = 0x00000001;
        public static uint Flag_ReadOnly = 0x00000002;
        public static uint Flag_ShowAlpha = 0x00000004;
        public static uint Flag_IsRamp = 0x00000008;

        [NonSerialized]
        public int height = 21;
        [NonSerialized]
        public string key = "";

        public ShaderCustomProperty[] customProperty = new ShaderCustomProperty[4]
        {
            new ShaderCustomProperty (),
            new ShaderCustomProperty (),
            new ShaderCustomProperty (),
            new ShaderCustomProperty (),
        };
        public ShaderPropertyDependency dependencyPropertys = new ShaderPropertyDependency ();
        public override void Copy (ShaderFeature src)
        {
            name = src.name;
            propertyName = src.propertyName;
            type = src.type;
            defaultColor = src.defaultColor;
            flag = src.flag;

            for (int i = 0; i < customProperty.Length; ++i)
            {
                var scp = customProperty[i];
                var srcScp = src.customProperty[i];
                scp.Clone (srcScp);
            }
            dependencyPropertys.Clone (src.dependencyPropertys);
        }

        public string hash = "";
        public string GetHash ()
        {
            if (string.IsNullOrEmpty (hash))
            {
                hash = FolderConfig.Hash ();
            }
            return hash;
        }
        public bool HasFlag (uint f)
        {
            return (flag & f) != 0;
        }

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

        public void Export (XmlDocument doc, XmlElement ShaderFeature)
        {
            ShaderFeature.SetAttribute ("name", name);
            ShaderFeature.SetAttribute ("flag", flag.ToString ());
            ShaderFeature.SetAttribute ("propertyName", propertyName);
            ShaderFeature.SetAttribute ("type", ((int) type).ToString ());
            Color32 c32 = defaultColor;
            ShaderFeature.SetAttribute ("ColorR", c32.r.ToString ());
            ShaderFeature.SetAttribute ("ColorG", c32.g.ToString ());
            ShaderFeature.SetAttribute ("ColorB", c32.b.ToString ());
            ShaderFeature.SetAttribute ("ColorA", c32.a.ToString ());
            for (int i = 0; i < customProperty.Length; ++i)
            {
                XmlElement ShaderCustomProperty = doc.CreateElement ("ShaderCustomProperty");
                var scp = customProperty[i];
                scp.Export (doc, ShaderCustomProperty);
                ShaderFeature.AppendChild (ShaderCustomProperty);
            }
            XmlElement dps = doc.CreateElement ("DependencyPropertys");
            dependencyPropertys.Export (doc, dps);
            ShaderFeature.AppendChild (dps);
        }
        public void Import (XmlElement ShaderFeature)
        {
            name = ShaderFeature.GetAttribute ("name");
            flag = uint.Parse (ShaderFeature.GetAttribute ("flag"));
            propertyName = ShaderFeature.GetAttribute ("propertyName");
            type = (ShaderPropertyType) int.Parse (ShaderFeature.GetAttribute ("type"));
            int r = int.Parse (ShaderFeature.GetAttribute ("ColorR"));
            int g = int.Parse (ShaderFeature.GetAttribute ("ColorG"));
            int b = int.Parse (ShaderFeature.GetAttribute ("ColorB"));
            int a = int.Parse (ShaderFeature.GetAttribute ("ColorA"));
            defaultColor = new Color32 ((byte) r, (byte) g, (byte) b, (byte) a);

            XmlNodeList childs = ShaderFeature.ChildNodes;
            for (int i = 0; i < customProperty.Length; ++i)
            {
                XmlElement ShaderCustomProperty = childs[i] as XmlElement;
                ref var scp = ref customProperty[i];
                scp.Import (ShaderCustomProperty);
            }
            XmlElement dp = childs[customProperty.Length] as XmlElement;
            dependencyPropertys.Import (dp);

        }
    }

    [System.Serializable]
    public class ShaderFeatureBlock : BaseFolderHash
    {
        public string bundleName;
        public List<ShaderFeature> shaderFeatures = new List<ShaderFeature> ();

        public void Export (XmlDocument doc, XmlElement sfBundle)
        {
            sfBundle.SetAttribute ("bundleName", bundleName);
            for (int i = 0; i < shaderFeatures.Count; ++i)
            {
                XmlElement shaderFeature = doc.CreateElement ("ShaderFeature");
                var sf = shaderFeatures[i];
                sf.Export (doc, shaderFeature);
                sfBundle.AppendChild (shaderFeature);
            }
        }
        public void Import (XmlElement sfBundle)
        {
            bundleName = sfBundle.GetAttribute ("bundleName");
            XmlNodeList childs = sfBundle.ChildNodes;
            shaderFeatures.Clear ();
            for (int i = 0; i < childs.Count; ++i)
            {
                XmlElement shaderFeature = childs[i] as XmlElement;
                ShaderFeature sf = new ShaderFeature ();
                sf.Import (shaderFeature);
                shaderFeatures.Add (sf);
            }
        }
    }

    [System.Serializable]
    public class ShaderFeatureGroupRef : BaseFolderHash
    {
        public string name = "";
        public ShaderFeatureGroup sfg;
        public void Export (XmlDocument doc, XmlElement ShaderFeatureGroupRef)
        {
            ShaderFeatureGroupRef.SetAttribute ("name", name);
            if (sfg != null)
            {
                sfg.Export (doc, ShaderFeatureGroupRef);
            }
        }

        public void Import (XmlElement ShaderFeatureGroupRef)
        {
            name = ShaderFeatureGroupRef.GetAttribute ("name");
            string path = string.Format ("Assets/Engine/Editor/EditorResources/ShaderConfig/ShaderFeatureGroup_{0}.asset", name);
            sfg = EditorCommon.LoadAsset<ShaderFeatureGroup> (path);
            sfg.Import (ShaderFeatureGroupRef);
            EditorCommon.SaveAsset (path, sfg);
        }

    }

    [System.Serializable]
    public class ShaderFeatureData : BaseAssetConfig
    {
        public List<ShaderFeatureGroupRef> groupRefs = new List<ShaderFeatureGroupRef> ();
        public override IList GetList () { return groupRefs; }

        public override Type GetListType () { return typeof (List<ShaderFeatureGroupRef>); }

        public override void OnAdd () { groupRefs.Add (new ShaderFeatureGroupRef ()); }

        public void Export (string path)
        {
            XmlDocument doc = new XmlDocument ();
            XmlElement ShaderFeatureGroups = doc.CreateElement ("ShaderFeatureGroups");
            doc.AppendChild (ShaderFeatureGroups);
            for (int i = 0; i < groupRefs.Count; ++i)
            {
                XmlElement ShaderFeatureGroupRef = doc.CreateElement ("ShaderFeatureGroupRef");
                var sfgRef = groupRefs[i];
                sfgRef.Export (doc, ShaderFeatureGroupRef);
                ShaderFeatureGroups.AppendChild (ShaderFeatureGroupRef);
            }
            doc.Save (path);
        }
        public void Import (string path)
        {
            XmlDocument doc = new XmlDocument ();
            doc.Load (path);
            XmlElement ShaderFeatureGroups = doc.DocumentElement;
            XmlNodeList childs = ShaderFeatureGroups.ChildNodes;
            for (int i = 0; i < childs.Count; ++i)
            {
                XmlElement ShaderFeatureGroup = childs[i] as XmlElement;
                ShaderFeatureGroupRef sfgr = new ShaderFeatureGroupRef ();
                sfgr.Import (ShaderFeatureGroup);
                groupRefs.Add (sfgr);
            }
        }
    }

    [System.Serializable]
    public class ShaderFeatureInstance : BaseFolderHash
    {
        public ShaderFeatureGroup sfg;
        public List<string> shaderFeatures = new List<string> ();
    }

    public class ShaderFeatureState
    {
        public ShaderFeatureGroupRef groupRef;
        public bool enable;
    }

    [System.Serializable]
    public class ShaderGUIConfigRef : BaseFolderHash
    {
        public ShaderGUIConfig config;
        [NonSerialized]
        public Shader shader;
        [NonSerialized]
        public bool add;

        [NonSerialized]
        public List<ShaderFeatureState> sfState = new List<ShaderFeatureState> ();
    }

    [System.Serializable]
    public class ShaderGUIData : BaseAssetConfig
    {
        public List<ShaderGUIConfigRef> configRefs = new List<ShaderGUIConfigRef> ();

        public override IList GetList () { return configRefs; }

        public override Type GetListType () { return typeof (List<ShaderGUIConfigRef>); }

        public override void OnAdd () { configRefs.Add (new ShaderGUIConfigRef ()); }

    }
    public enum ShaderValueType
    {
        Vec,
        Float,
        Int,
    }
    //Material/////////////////////////////////////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class MaterialVariantParam
    {
        public int shaderID;
        public string shaderKey;
        public ShaderValueType vt = ShaderValueType.Vec;
        public Vector4 value;
    }

    [System.Serializable]
    public class MaterialVariant : BaseFolderHash
    {
        [NonSerialized]
        public bool editKey = false;
        public Material mat;
        public string suffix = "";
        public List<string> keywords = new List<string> ();
        public List<MaterialVariantParam> param = new List<MaterialVariantParam> ();
        public void Copy (MaterialVariant src)
        {
            for (int i = 0; i < src.param.Count; ++i)
            {
                var srcmvp = src.param[i];
                param.Add (new MaterialVariantParam ()
                {
                    shaderID = srcmvp.shaderID,
                        value = srcmvp.value,
                });
            }
            keywords.Clear ();
            keywords.AddRange (src.keywords);
        }
    }

    [System.Serializable]
    public class MaterialLodGroup : BaseFolderHash
    {
        public string groupName = "";
        public List<Material> lodMat = new List<Material> ();
    }

    [System.Serializable]
    public class ShaderProperty
    {
        public ShaderUtil.ShaderPropertyType dataType; //0 vec 1 color 2 tex
        public int shaderID;
        public string customName;
        public string depComp;
        public string relativeStr = "";

    }

    [System.Serializable]
    public class DepPair
    {
        public string propertyName;
        public string dep;
        public bool relative;
    }

    [System.Serializable]
    public class IncludeProperty
    {
        public string property;
        public bool include;
    }

    [System.Serializable]
    public class DummyMaterialInfo : BaseCopy<DummyMaterialInfo>, IFolderHash
    {
        [NonSerialized]
        public float height = 0;
        [NonSerialized]
        public bool editKey = false;
        public string name = "";
        public string hashID = "";
        public Shader shader;
        public List<MaterialVariant> matVariants = new List<MaterialVariant> ();
        public MaterialLodGroup[] lodGroup = new MaterialLodGroup[3]
        {
            new MaterialLodGroup () { groupName = "LodHigh" },
            new MaterialLodGroup () { groupName = "LodMedium" },
            new MaterialLodGroup () { groupName = "LodLow" }
        };
        public int selectFunIndex = 0;
        public uint keywordFlag = 0;
        public BlendMode blendType = BlendMode.Opaque;
        public List<string> keywords = new List<string> ();
        public List<IncludeProperty> includeParam = new List<IncludeProperty> ();
        public List<DepPair> depParam = new List<DepPair> ();
        public List<ShaderProperty> shaderPropertys = new List<ShaderProperty> ();

        public override void Copy (DummyMaterialInfo src)
        {
            if (shader == null)
                shader = src.shader;
            matVariants.Clear ();
            for (int i = 0; i < src.matVariants.Count; ++i)
            {
                var srcmv = src.matVariants[i];
                var mv = new MaterialVariant ();
                mv.suffix = srcmv.suffix;
                mv.Copy (srcmv);
                matVariants.Add (mv);
            }
            selectFunIndex = src.selectFunIndex;
            blendType = src.blendType;
            shaderPropertys.Clear ();
            for (int i = 0; i < src.shaderPropertys.Count; ++i)
            {
                var srcsp = src.shaderPropertys[i];
                var sp = new ShaderProperty ();
                sp.dataType = srcsp.dataType;
                sp.shaderID = srcsp.shaderID;
                shaderPropertys.Add (sp);
            }
            keywords.Clear ();
            keywords.AddRange (src.keywords);

        }

        public Material FindMat (string suffix)
        {
            for (int i = 0; i < matVariants.Count; ++i)
            {
                var mv = matVariants[i];
                if (mv.suffix == suffix)
                {
                    return mv.mat;
                }
            }
            return null;
        }

        public void SetHashID ()
        {
            if (shader != null)
            {
                ShaderKeywordConfig skc = ShaderKeywordConfig.instance;
                hashID = string.Format ("{0}_{1}", shader.name, blendType.ToString ());
                keywords.Sort();
                skc.keywordData.GetShaderKeyStr (keywords, ref hashID);

            }
            else
            {
                hashID = "";
            }
        }

        public string hash = "";
        public string GetHash ()
        {
            if (string.IsNullOrEmpty (hash))
            {
                hash = FolderConfig.Hash ();
            }
            return hash;
        }
    }

    [System.Serializable]
    public class DummyMatData : BaseAssetConfig
    {
        public static string[] BatchFun = new string[(int)EBuildBatchType.Num]
        {
            "DefaultScene",
            "Simple",
            "Terrain",
            "DefaultRole",
            "Outline",
            "SceneLod3",
            "RoleShow",
        };
        public List<DummyMaterialInfo> materialInfos = new List<DummyMaterialInfo> ();

        [NonSerialized]
        public string[] matNames;
        public override IList GetList () { return materialInfos; }

        public override Type GetListType () { return typeof (List<DummyMaterialInfo>); }

        public override void OnAdd () { materialInfos.Add (new DummyMaterialInfo ()); }

        public override float GetHeight (int index) { return materialInfos[index].height; }

        public override void SetHeight (int index, float height) { materialInfos[index].height = height; }
        public void Copy (DummyMatData src)
        {
            materialInfos.Clear ();
            for (int i = 0; i < src.materialInfos.Count; ++i)
            {
                var mi = new DummyMaterialInfo ();
                var misrc = src.materialInfos[i];
                mi.Copy (misrc);
                materialInfos.Add (mi);
            }
        }
    }

    [System.Serializable]
    public class MatKeywordConfig : BaseFolderHash
    {
        public string suffix;
        public List<string> keywords = new List<string> ();
    }

    [System.Serializable]
    public class MatLodConfig
    {
        public string matDesc = "";
        public string findSuffixLod0 = "";
        public string findSuffixLod1 = "";
        public string findSuffixLod2 = "";
    }

    [System.Serializable]
    public class MatKeywordConfigs : BaseFolderHash
    {
        public List<MatKeywordConfig> configs = new List<MatKeywordConfig> ();
    }

    [System.Serializable]
    public class MatLodConfigs : BaseFolderHash
    {
        public List<MatLodConfig> configs = new List<MatLodConfig> ();

        [NonSerialized]
        private string[] matNames;

        public string[] GetNames ()
        {
            if (matNames == null || matNames.Length != configs.Count)
            {
                if (configs.Count > 0)
                {
                    matNames = new string[configs.Count];
                    for (int i = 0; i < configs.Count; ++i)
                    {
                        matNames[i] = configs[i].matDesc;
                    }
                }

            }
            return matNames;
        }
    }

    [System.Serializable]
    public class DefaultMatConfig : BaseFolderHash
    {
        public MatKeywordConfigs configs = new MatKeywordConfigs ();
        public MatLodConfigs lodConfigs = new MatLodConfigs ();
    }
}

#endif

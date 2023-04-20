// #if UNITY_EDITOR
using System;
using System.Diagnostics;
using UnityEngine;
namespace CFEngine
{

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EnvAttribute : Attribute
    {
        public readonly string menuItem;
        public readonly bool isVirtualEffect;
        internal EnvAttribute (Type renderer, string menuItem, bool isVirtualEffect = false)
        {
            this.menuItem = menuItem;
            this.isVirtualEffect = isVirtualEffect;
        }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public class BasePropertyAttribute : PropertyAttribute
    {
        public BasePropertyAttribute () { }
    }

    // [Conditional ("UNITY_EDITOR")]
    // [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    // public sealed class CFMaxAttribute : BasePropertyAttribute
    // {
    //     public readonly float max;

    //     public CFMaxAttribute (float max, bool canRecord = true) : base (canRecord)
    //     {
    //         this.max = max;
    //     }
    // }

    // [Conditional ("UNITY_EDITOR")]
    // [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    // public sealed class CFMinAttribute : BasePropertyAttribute
    // {
    //     public readonly float min;

    //     public CFMinAttribute (float min, bool canRecord = true) : base (canRecord)
    //     {
    //         this.min = min;
    //     }
    // }

    // [Conditional ("UNITY_EDITOR")]
    // [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    // public sealed class CFMinMaxAttribute : BasePropertyAttribute
    // {
    //     public readonly float min;
    //     public readonly float max;

    //     public CFMinMaxAttribute (float min, float max, bool canRecord = true) : base (canRecord)
    //     {
    //         this.min = min;
    //         this.max = max;
    //     }
    // }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public class CFRangeAttribute : BasePropertyAttribute
    {
        public readonly float min;
        public readonly float max;
        public readonly float v;
        public CFRangeAttribute (float min, float max, float defalut) : base ()
        {
            this.min = min;
            this.max = max;
            this.v = defalut;
        }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public class CFResPathAttribute : BasePropertyAttribute
    {
        public readonly Type type;
        public readonly string defaultPath;
        public readonly int resOffset;
        public readonly bool redirectRes;
        public CFResPathAttribute (Type type, string defaultPath, int resOffset, bool redirectRes) : base ()
        {
            this.type = type;
            this.defaultPath = defaultPath;
            this.resOffset = resOffset;
            this.redirectRes = redirectRes;
        }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public class CFLightingAttribute : BasePropertyAttribute
    {
        public readonly string name;
        public CFLightingAttribute (string lightName) : base ()
        {
            this.name = lightName;
        }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public class CFDynamicLightingAttribute : BasePropertyAttribute
    {
        public readonly bool addLight;
        public readonly int lightCount;
        public readonly int minLightCount;
        public readonly int maxLightCount;
        public CFDynamicLightingAttribute (bool addLight, int lightCount, int minLightCount, int maxLightCount) : base ()
        {
            this.addLight = addLight;
            this.lightCount = lightCount;
            this.minLightCount = minLightCount;
            this.maxLightCount = maxLightCount;
        }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public class CFObjRefAttribute : BasePropertyAttribute
    {
        public readonly string name;
        public CFObjRefAttribute (string name) : base ()
        {
            this.name = name;
        }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CFColorUsageAttribute : BasePropertyAttribute
    {

        public readonly bool showAlpha;
        public readonly bool hdr;
        public readonly Color v;
        public CFColorUsageAttribute (bool showAlpha, bool hdr, float r, float g, float b, float a) : base ()
        {
            this.showAlpha = showAlpha;

            this.hdr = hdr;

            this.v = new Color (r, g, b, a);
        }
    }

    public enum C4DataType
    {
        None,
        FloatRange,
        IntRange,
        Float,
        Int,
        /// <summary>
        /// 打开为max，关闭为min，确保max > min + 0.01f。
        /// </summary>
        Bool,
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CFParam4Attribute : BasePropertyAttribute
    {
        public readonly string v0Str;
        public readonly float default0;
        public readonly float min0;
        public readonly float max0;
        public readonly float scale0;
        public readonly C4DataType type0;
        public readonly string v1Str;
        public readonly float default1;
        public readonly float min1;
        public readonly float max1;
        public readonly float scale1;
        public readonly C4DataType type1;
        public readonly float default2;
        public readonly string v2Str;
        public readonly float min2;
        public readonly float max2;
        public readonly float scale2;
        public readonly C4DataType type2;
        public readonly string v3Str;
        public readonly float default3;
        public readonly float min3;
        public readonly float max3;
        public readonly float scale3;
        public readonly C4DataType type3;

        public CFParam4Attribute (
            string v0, float default0, float min0, float max0, float scale0, C4DataType type0,
            string v1, float default1, float min1, float max1, float scale1, C4DataType type1,
            string v2, float default2, float min2, float max2, float scale2, C4DataType type2,
            string v3, float default3, float min3, float max3, float scale3, C4DataType type3) : base ()
        {
            this.v0Str = v0;
            this.default0 = default0;
            this.min0 = min0;
            this.max0 = max0;
            this.scale0 = scale0;
            this.type0 = type0;

            this.v1Str = v1;
            this.default1 = default1;
            this.min1 = min1;
            this.max1 = max1;
            this.scale1 = scale1;
            this.type1 = type1;

            this.v2Str = v2;
            this.default2 = default2;
            this.min2 = min2;
            this.max2 = max2;
            this.scale2 = scale2;
            this.type2 = type2;

            this.v3Str = v3;
            this.default3 = default3;
            this.min3 = min3;
            this.max3 = max3;
            this.scale3 = scale3;
            this.type3 = type3;
        }
    }

    // [Conditional ("UNITY_EDITOR")]
    // [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    // public sealed class CFLayerAttribute : BasePropertyAttribute
    // {
    //     public readonly bool multi;
    //     public CFLayerAttribute (bool multi, bool canRecord = true) : base (canRecord)
    //     {
    //         this.multi = multi;
    //     }
    // }

    // [Conditional ("UNITY_EDITOR")]
    // [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    // public sealed class CFRotAttribute : BasePropertyAttribute
    // {
    //     public readonly string title;

    //     public CFRotAttribute (string title, bool canRecord = true) : base (canRecord)
    //     {
    //         this.title = title;
    //     }
    // }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CFEnumAttribute : BasePropertyAttribute
    {
        public readonly int v;
        public readonly Type enumType;

        public CFEnumAttribute (Type enumType, int v) : base ()
        {
            this.v = v;
            this.enumType = enumType;
        }
    }

    // [Conditional ("UNITY_EDITOR")]
    // [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    // public sealed class CFSpaceAttribute : PropertyAttribute
    // {
    //     public readonly float height;
    //     public readonly string str;
    //     public CFSpaceAttribute (float height, string str)
    //     {
    //         this.height = height;
    //         this.str = str;
    //     }
    // }
    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CFCustomOverrideAttribute : PropertyAttribute
    {
        public CFCustomOverrideAttribute () { }
    }

    public interface ICustomDraw
    {
        void OnDrawInspector ();
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CFCustomDrawAttribute : PropertyAttribute
    {
        public CFCustomDrawAttribute () { }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CFNoSerializedAttribute : Attribute
    {
        internal CFNoSerializedAttribute () { }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CFSerializedAttribute : Attribute
    {
        internal CFSerializedAttribute () { }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CFDisplayNameAttribute : Attribute
    {
        public readonly string displayName;

        public CFDisplayNameAttribute (string displayName)
        {
            this.displayName = displayName;
        }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CFTrackballAttribute : Attribute
    {
        public readonly int mode;
        public readonly Vector4 defaultValue;

        public CFTrackballAttribute (int mode, float x, float y, float z, float w)
        {
            this.mode = mode;
            this.defaultValue = new Vector4 (x, y, z, w);
        }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public class CFTooltipAttribute : Attribute
    {
        public readonly string tooltip;
        public CFTooltipAttribute (string tooltip)
        {
            this.tooltip = tooltip;
        }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public class CFSHAttribute : PropertyAttribute
    {
        public readonly Color flatColor;
        public readonly Color skyColor;
        public readonly Color equatorColor;

        public readonly Color groundColor;
        public float skyIntensity;
        public CFSHAttribute (
            float flatColorR, float flatColorG, float flatColorB,
            float skyColorR, float skyColorG, float skyColorB,
            float equatorColorR, float equatorColorG, float equatorColorB,
            float groundColorR, float groundColorG, float groundColorB,
            float skyIntensity)
        {
            this.flatColor = new Color (flatColorR / 255, flatColorR / 255, flatColorR / 255, 1);
            this.skyColor = new Color (skyColorR / 255, skyColorG / 255, skyColorB / 255, 1);
            this.equatorColor = new Color (equatorColorR / 255, equatorColorG / 255, equatorColorB / 255, 1);
            this.groundColor = new Color (groundColorR / 255, groundColorG / 255, groundColorB / 255, 1);
            this.skyIntensity = skyIntensity;
        }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage (AttributeTargets.Field, AllowMultiple = false)]
    public class CFTextureCurveAttribute : PropertyAttribute
    {
        public readonly uint keyCount;
        public readonly byte curveType;
        public readonly bool loop;
        public readonly int background;
        public readonly Color color;
        public CFTextureCurveAttribute (
            uint keyCount,
            byte curveType,
            bool loop,
            int background,
            float r, float g, float b)
        {
            this.keyCount = keyCount;
            this.curveType = curveType;
            this.loop = loop;
            this.background = background;
            this.color = new Color(r, g, b, 1);
        }
    }

    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CFFogAttribute : PropertyAttribute
    {
        public float start;
        public float end;
        public float intensityMin;
        public float intensityMax;
        public float intensityScale;
        public float fallOff;

        public CFFogAttribute(float start, float end, float intensityMin, float intensityMax, float intensityScale, float fallOff)
        {
            this.start = start;
            this.end = end;
            this.intensityMin = intensityMin;
            this.intensityMax = intensityMax;
            this.intensityScale = intensityScale;
            this.fallOff = fallOff;
        }
    }

}
// #endif
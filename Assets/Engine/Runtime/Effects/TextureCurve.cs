using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CFEngine
{
    [Serializable]
    public class TextureCurveParam : ParamOverride<AnimationCurve, TextureCurveParam>
    {
        public byte curveType;
        public bool loop;
        private bool texDirty = true;

        static int k_Precision = 128; // Edit LutBuilder3D if you change this value
        static float k_Step = 1f / k_Precision;
        static Color[] pixels = new Color[k_Precision];
        public static byte CurveTyp_Linear01 = 0;
        public static byte CurveTyp_ConstHalf = 1;
        public static byte CurveTyp_Custom = 2;
        public bool forceLinear01 = false;
#if UNITY_EDITOR
        public byte defaultCurveType;
        private bool refreshCurve = false;
        private AnimationCurve loopCurve;
        private static Keyframe[] defaultCurve = new [] { new Keyframe (0f, 0f, 1f, 1f), new Keyframe (1f, 1f, 1f, 1f) };
#endif
        public override void SetValue (ParamOverride parameter, bool shallowCopy)
        {
            TextureCurveParam src = parameter as TextureCurveParam;
            if (shallowCopy)
            {
                texDirty |= value != src.value;
                value = src.value;
                curveType = src.curveType;
#if UNITY_EDITOR
                if (!parameter.overrideState)
                {
                    curveType = defaultCurveType;
                }
                loopCurve = src.loopCurve;
                refreshCurve = src.refreshCurve;
                src.refreshCurve = false;
#endif
            }
#if UNITY_EDITOR
            else
            {
                curveType = src.curveType;
                loop = src.loop;
            }
#endif
        }

        protected override void InnerInterp (
            ParamOverride<AnimationCurve, TextureCurveParam> from,
            ParamOverride<AnimationCurve, TextureCurveParam> to, float t)
        {
            if (t > 0.5f)
            {
                SetValue (to, true);
            }
            else
            {
                SetValue (from, true);
            }
        }

        public override void Destroy ()
        {
            if (value != null)
            {
                CommonObject<AnimationCurve>.Release (value);
                value = null;
            }
#if UNITY_EDITOR
            if (loopCurve != null)
            {
                CommonObject<AnimationCurve>.Release (loopCurve);
                loopCurve = null;
            }
#endif
            CommonObject<TextureCurveParam>.Release (this);
        }

        public void Load (CFBinaryReader reader)
        {
            curveType = reader.ReadByte ();
            int count = reader.ReadByte ();
            if (count > 0)
            {
                if (value == null)
                {
                    value = CommonObject<AnimationCurve>.Get ();
                }
#if UNITY_EDITOR
                if (loopCurve == null && loop)
                {
                    loopCurve = CommonObject<AnimationCurve>.Get ();
                }
#endif
                for (int i = 0; i < count; ++i)
                {
                    Keyframe key = new Keyframe ();
                    key.time = reader.ReadSingle ();
                    key.value = reader.ReadSingle ();
                    key.inTangent = reader.ReadSingle ();
                    key.outTangent = reader.ReadSingle ();
                    value.AddKey (key);

                    if (loop)
                    {
                        AnimationCurve lc = value;
#if UNITY_EDITOR
                        lc = loopCurve;
#endif
                        lc.AddKey (key);
                        if (i == 0)
                        {
                            key.time += 1;
                            lc.AddKey (key);
                        }
                        else if (i == (count - 1))
                        {
                            key.time -= 1;
                            lc.AddKey (key);
                        }
                    }

                }
            }
        }

        #region envParam
        protected override void InnerLoadFromData (IGetEnvValue valueHandler, EnvParam envParam)
        {
#if UNITY_EDITOR           
            if (EngineContext.IsRunning)
#endif
            {
                int dataOffset = envParam.dataOffset;
                if ((envParam.valueMask & MaskX) > 0)
                {
                    curveType = (byte) valueHandler.GetValue (dataOffset++);
                    int count = (int) valueHandler.GetValue (dataOffset++);
                    if (count > 0)
                    {
                        if (value == null)
                        {
                            value = CommonObject<AnimationCurve>.Get ();
                        }
#if UNITY_EDITOR
                        var runtime = envParam.runtimeParam.runtime as TextureCurveParam;
                        if (loopCurve == null && runtime.loop)
                        {
                            loopCurve = CommonObject<AnimationCurve>.Get ();
                        }
#endif
                        for (int i = 0; i < count; ++i)
                        {
                            Keyframe key = new Keyframe ();
                            key.time = valueHandler.GetValue (dataOffset++);
                            key.value = valueHandler.GetValue (dataOffset++);
                            key.inTangent = valueHandler.GetValue (dataOffset++);
                            key.outTangent = valueHandler.GetValue (dataOffset++);
                            value.AddKey (key);

                            if (loop)
                            {
                                AnimationCurve lc = value;
#if UNITY_EDITOR
                                lc = loopCurve;
#endif
                                lc.AddKey (key);
                                if (i == 0)
                                {
                                    key.time += 1;
                                    lc.AddKey (key);
                                }
                                else if (i == (count - 1))
                                {
                                    key.time -= 1;
                                    lc.AddKey (key);
                                }
                            }

                        }
                    }
                }

            }
        }

        #endregion
        #region runtime
        static TextureFormat GetTextureFormat ()
        {
            if (SystemInfo.SupportsTextureFormat (TextureFormat.RHalf))
                return TextureFormat.RHalf;
            if (SystemInfo.SupportsTextureFormat (TextureFormat.R8))
                return TextureFormat.R8;

            return TextureFormat.ARGB32;
        }
        public void ResetTex (RenderContext rc, int slot, byte curve)
        {
            ref var tex = ref rc.colorCurveCache[slot];
            if (tex != null)
            {
                if (curve == CurveTyp_Linear01)
                {
                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i].r = 1.0f * i * k_Step;
                }
                else if (curve == CurveTyp_ConstHalf)
                {
                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i].r = 0.5f;
                }
                tex.SetPixels (pixels);
                tex.Apply (false, false);
            }
        }

        public Texture2D GetTexture (RenderContext rc, int slot)
        {
            ref var tex = ref rc.colorCurveCache[slot];
            if (tex == null)
            {
                tex = new Texture2D (k_Precision, 1, GetTextureFormat (), false, true);
#if UNITY_EDITOR
                tex.name = name;
#endif
                tex.hideFlags = HideFlags.HideAndDontSave;
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.Apply (false, false);
            }
#if UNITY_EDITOR
            texDirty |= refreshCurve;
            refreshCurve = false;
#endif
            //if (texDirty)
            {
                if (forceLinear01 || curveType == CurveTyp_Linear01)
                {
                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i].r = 1.0f * i * k_Step;
                }
                else if (curveType == CurveTyp_ConstHalf)
                {
                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i].r = 0.5f;
                }
                else if (curveType == CurveTyp_Custom)
                {
                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i].r = Evaluate (i * k_Step);
                }

                tex.SetPixels (pixels);
                tex.Apply (false, false);
                texDirty = false;
            }

            return tex;
        }

        private float Evaluate (float time)
        {
            if (value == null || value.length < 1)
                return 0.5f;
            var c = value;
#if UNITY_EDITOR
            if (loop)
                c = loopCurve;
#endif
            return c.Evaluate (time);
        }
        #endregion

#if UNITY_EDITOR

        public void InitCurve (uint count, byte defaultCurveType, bool force = false)
        {
            if (value == null || force)
            {
                if (value == null)
                {
                    value = count == 0 ? new AnimationCurve () : new AnimationCurve (defaultCurve);
                }
                else
                {
                    if (count == 0)
                    {
                        value.keys = null;
                    }
                    else
                    {
                        value.keys = defaultCurve;
                    }
                }
                curveType = defaultCurveType;
                this.defaultCurveType = defaultCurveType;
            }
            if (value.length > 0 && loop)
            {
                InitProfileLoopCurve ();
            }
            refreshCurve = true;
        }

        private void InitProfileLoopCurve ()
        {
            if (loopCurve == null)
            {
                loopCurve = new AnimationCurve ();
            }
            var keys = value.keys;
            loopCurve.keys = keys;
            var k0 = keys[0];
            k0.time += 1;
            loopCurve.AddKey (k0);
            var k1 = keys[keys.Length - 1];
            k1.time -= 1;
            loopCurve.AddKey (k1);
        }

        public void SetDirty (bool modify = true)
        {
            refreshCurve = true;
            if (value != null && value.length > 0)
            {
                if (loop)
                    InitProfileLoopCurve ();
            }
            if (modify)
                curveType = TextureCurveParam.CurveTyp_Custom;
        }

        public void Save (BinaryWriter bw)
        {
            bw.Write (curveType);
            int count = 0;
            if (curveType == CurveTyp_Custom)
            {
                count = value != null?value.length : 0;
            }
            bw.Write ((byte) count);
            if (count > 0)
            {
                var keys = value.keys;
                for (int i = 0; i < count; ++i)
                {
                    ref var key = ref keys[i];
                    bw.Write (key.time);
                    bw.Write (key.value);
                    bw.Write (key.inTangent);
                    bw.Write (key.outTangent);
                }
            }
        }
#endif
    }
}
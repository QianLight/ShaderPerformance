using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif

namespace CFEngine
{
    [Serializable]
    public sealed class SHParam : ParamOverride<SHInfo, SHParam>
    {
        [NonSerialized]
        public AnimContext<SHProperty> anim;
        [NonSerialized]
        public SHProperty data;
#if UNITY_EDITOR
        //public AnimContextSH curve = new AnimContextSH() { mask = MaskX };
#endif
        protected override void InnerInterp (ParamOverride<SHInfo, SHParam> from, ParamOverride<SHInfo, SHParam> to, float t)
        {
            value.shAr = Vector4.Lerp (from.value.shAr, to.value.shAr, t);
            value.shAg = Vector4.Lerp (from.value.shAg, to.value.shAg, t);
            value.shAb = Vector4.Lerp (from.value.shAb, to.value.shAb, t);

            value.shBr = Vector4.Lerp (from.value.shBr, to.value.shBr, t);
            value.shBg = Vector4.Lerp (from.value.shBg, to.value.shBg, t);
            value.shBb = Vector4.Lerp (from.value.shBb, to.value.shBb, t);

            value.shC = Vector4.Lerp (from.value.shC, to.value.shC, t);
        }

        protected override void InnerLoadFromData (IGetEnvValue valueHandler, EnvParam envParam)
        {
#if UNITY_EDITOR
            if (EngineContext.IsRunning)
#endif
            {
                if ((envParam.valueMask & MaskX) > 0)
                {
                    int dataOffset = envParam.dataOffset;

                    value.shAr = new Vector4 (
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++));
                    value.shAg = new Vector4 (
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++));
                    value.shAb = new Vector4 (
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++));
                    value.shBr = new Vector4 (
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++));
                    value.shBg = new Vector4 (
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++));
                    value.shBb = new Vector4 (
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++));
                    value.shC = new Vector4 (
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++),
                        valueHandler.GetValue (dataOffset++));
                }
            }
        }

        public override void BindAnimData (AnimEnvParam aep, ref AnimPack animPack)
        {
            if (animPack.SHCurve != null)
            {
                int indexOffset = aep.curveIndexStart;
                if ((aep.valueMask & MaskX) > 0)
                {
                    AnimPack.BindCurve (ref animPack, ref indexOffset, ref anim);
                }
            }
        }

        public override void SetAnimValue(float t, AnimEnvParam aep, ref AnimPack animPack)
        {
            base.SetAnimValue(t, aep, ref animPack);
#if UNITY_EDITOR
            if (!EngineContext.IsRunning && !AnimEnvParam.test)
            {
                var envParam = aep.envParam;
                if (envParam != null)
                {
                    var p = envParam.param as SHParam;
                    if ((envParam.valueMask & MaskX) > 0)
                    {
                      //  p.curve.Evaluate(t, ref value);                        
                       // p.curve.envParam = envParam;
                        p.value = value;
                    }
                }
            }
            else
#endif
            {
                if ((aep.valueMask & MaskX) > 0)
                {
                    anim.Evaluate (t, animPack.SHCurve, ref data);
                    value.Copy (ref data);
                }
            }
        }

        public void Load(CFBinaryReader reader, int index)
        {
            reader.ReadVector(ref value.shAr);
            reader.ReadVector(ref value.shAg);
            reader.ReadVector(ref value.shAb);

            reader.ReadVector(ref value.shBr);
            reader.ReadVector(ref value.shBg);
            reader.ReadVector(ref value.shBb);

            reader.ReadVector(ref value.shC);

#if UNITY_EDITOR
            BindSHDebugColor(index);
#endif
        }

#if UNITY_EDITOR
        public override void Create(IGetEnvValue valueHandler, EnvParam envParam)
        {
            base.Create(valueHandler, envParam);
            if (EngineContext.IsRunning)
            {
                var shParam = envParam.param as SHParam;
                if (shParam != null)
                {
                    if (this.name == "sceneSH")
                    {
                        shParam.BindSHDebugColor(EnvBlock.envObjIndex);
                    }
                    else
                    {
                        shParam.BindSHDebugColor(EnvBlock.envObjIndex + 100);
                    }
                }
            }
        }
        public void Save (BinaryWriter bw)
        {
            EditorCommon.WriteVector (bw, value.shAr);
            EditorCommon.WriteVector (bw, value.shAg);
            EditorCommon.WriteVector (bw, value.shAb);

            EditorCommon.WriteVector (bw, value.shBr);
            EditorCommon.WriteVector (bw, value.shBg);
            EditorCommon.WriteVector (bw, value.shBb);
            value.shC.w = (int) value.ambientMode;
            EditorCommon.WriteVector (bw, value.shC);
        }
        private void Save(ref Vector4 v, IGetEnvValue valueHandler)
        {
            valueHandler.SetValue(v.x);
            valueHandler.SetValue(v.y);
            valueHandler.SetValue(v.z);
            valueHandler.SetValue(v.w);
        }
        public override void SaveAnim(AnimEnvParam aep, ref AnimPack animPack)
        {
            aep.curveIndexStart = animPack.index.Count;
            if ((aep.valueMask & MaskX) > 0)
            {
                int start = animPack.SH.Count;
                animPack.index.Add(start);

               // animPack.SH.AddRange(curve.GetRuntimeData());

                int end = animPack.SH.Count;
                animPack.index.Add(end);
            }
        }
        public override void Serialize(EnvParam envParam, IGetEnvValue valueHandler)
        {
            envParam.dataOffset = valueHandler.GetCount();

            if ((envParam.valueMask & MaskX) > 0)
            {
                Save(ref value.shAr, valueHandler);
                Save(ref value.shAg, valueHandler);
                Save(ref value.shAb, valueHandler);
                Save(ref value.shBr, valueHandler);
                Save(ref value.shBg, valueHandler);
                Save(ref value.shBb, valueHandler);
                Save(ref value.shC, valueHandler);

                if (envParam.Name == "sceneSH")
                {
                    AddSHColorDebug(this, EnvBlock.envObjIndex);
                }
                else
                {
                    AddSHColorDebug(this, EnvBlock.envObjIndex + 100);
                }
            }
        }

        public override void SetCurveValue(float t, EnvParam envParam)
        {
            if ((envParam.valueChangeMask & MaskX) > 0)
            {
                //curve.SetKeyValue(t, ref value);
            }
        }
        public override void GetEditingCurve(List<AnimContextBase> editAnim, EnvParam envParam)
        {
            if ((envParam.valueMask & MaskX) > 0 &&
                (envParam.animMask & MaskX) > 0)
            {
               // editAnim.Add(curve);
            }
        }

        public static void AddSHColorDebug(SHParam shParam, int index)
        {
            if(!Ambient.shColorDebug.ContainsKey(index))
            {
                SHColorDebug colorDebug = new SHColorDebug()
                {
                    ambientMode = shParam.value.ambientMode,
                    flatColor = shParam.value.flatColor,
                    skyColor = shParam.value.skyColor,
                    equatorColor = shParam.value.equatorColor,
                    groundColor = shParam.value.groundColor,
                    skyCube = shParam.value.skyCube,
                    skyIntensity = shParam.value.skyIntensity,
                };
                Ambient.shColorDebug.Add(index, colorDebug);
            }
        }

        private void BindSHDebugColor(int index)
        {
            EngineContext context = EngineContext.instance;
            if (context.shColorDebug.TryGetValue(index, out var debugColor))
            {
                value.ambientMode = debugColor.ambientMode;
                value.flatColor = debugColor.flatColor;
                value.skyColor = debugColor.skyColor;
                value.equatorColor = debugColor.equatorColor;
                value.groundColor = debugColor.groundColor;
                value.skyCube = debugColor.skyCube;
                value.skyIntensity = debugColor.skyIntensity;
            }
        }
#endif
    }
}
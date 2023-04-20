using System;
using System.IO;
using UnityEngine;

namespace CFEngine
{
    [Serializable]
    public sealed class FogParam : ParamOverride<FogInfo, FogParam>
    {
        [NonSerialized]
        public FogProperty data;

        protected override void InnerInterp(ParamOverride<FogInfo, FogParam> from, ParamOverride<FogInfo, FogParam> to, float t)
        {
            value.start = Mathf.Lerp(from.value.start, to.value.start, t);
            value.end = Mathf.Lerp(from.value.end, to.value.end, t);
            value.intensityMin = Mathf.Lerp(from.value.intensityMin, to.value.intensityMin, t);
            value.intensityMax = Mathf.Lerp(from.value.intensityMax, to.value.intensityMax, t);
            value.intensityScale = Mathf.Lerp(from.value.intensityScale, to.value.intensityScale, t);
            value.fallOff = Mathf.Lerp(from.value.fallOff, to.value.fallOff, t);
        }

        protected override void InnerLoadFromData(IGetEnvValue valueHandler, EnvParam envParam)
        {
#if UNITY_EDITOR
            if (EngineContext.IsRunning)
#endif
            {
                if ((envParam.valueMask & MaskX) > 0)
                {
                    int dataOffset = envParam.dataOffset;
                    value.start = valueHandler.GetValue(dataOffset++);
                    value.end = valueHandler.GetValue(dataOffset++);
                    value.intensityMin = valueHandler.GetValue(dataOffset++);
                    value.intensityMax = valueHandler.GetValue(dataOffset++);
                    value.intensityScale = valueHandler.GetValue(dataOffset++);
                    value.fallOff = valueHandler.GetValue(dataOffset++);
                }
            }
        }

        public void Load(CFBinaryReader reader)
        {
            value.start = reader.ReadSingle();
            value.end = reader.ReadSingle();
            value.intensityMin = reader.ReadSingle();
            value.intensityMax = reader.ReadSingle();
            value.intensityScale = reader.ReadSingle();
            value.fallOff = reader.ReadSingle();
        }

#if UNITY_EDITOR

        public void Save(BinaryWriter bw)
        {
            bw.Write(value.start);
            bw.Write(value.end);
            bw.Write(value.intensityMin);
            bw.Write(value.intensityMax);
            bw.Write(value.intensityScale);
            bw.Write(value.fallOff);
        }

        public override void Serialize(EnvParam envParam, IGetEnvValue valueHandler)
        {
            envParam.dataOffset = valueHandler.GetCount();

            if ((envParam.valueMask & MaskX) > 0)
            {
                valueHandler.SetValue(value.start);
                valueHandler.SetValue(value.end);
                valueHandler.SetValue(value.intensityMin);
                valueHandler.SetValue(value.intensityMax);
                valueHandler.SetValue(value.intensityScale);
                valueHandler.SetValue(value.fallOff);
            }
        }

#endif // UNITY_EDITOR
    }
}
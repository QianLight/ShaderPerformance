#if UNITY_EDITOR
using System;
using UnityEngine;

namespace CFEngine
{
    public class SFXLightAnalyze : SFXAnalyze
    {
        private LightRender lr;

        public override byte ComponentType
        {
            get { return SFXComponent.SFX_COMPONENT_POINTLIGHT; }
        }

        public override void Analyze(ref SFXContext context,
               ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            base.Analyze(ref context, ref parentContext, ref tmpContext);
            lr = Prepare<LightRender>(context.trans);
            if (lr != null)
            {
                context.componentType = ComponentType;
            }
        }

        public override void CreateComponent(ref SFXContext context, ref SFXTmpContext tmpContext)
        {
            base.CreateComponent(ref context, ref tmpContext);
            if (lr != null)
            {
                var sfxLight = context.comp as SFXLight;

                sfxLight.delay = lr.delay;
                sfxLight.fadeInLength = lr.fadeInLength;
                sfxLight.loopLength = lr.loopLength;
                sfxLight.fadeOutLength = lr.fadeOutLength;
                sfxLight.loopTimes = lr.loopTimes;
                sfxLight.softness = lr.softness;

                sfxLight.fadeIn = AnalyzeAnimPack(lr.fadeIn);
                sfxLight.loop = AnalyzeAnimPack(lr.loop);
                sfxLight.fadeOut = AnalyzeAnimPack(lr.fadeOut);
            }
        }

        private LightAnimPack AnalyzeAnimPack(CLightProperty data)
        {
            LightAnimPack pack = SharedObjectPool<LightAnimPack>.Get();

            pack.color.Analyze(data.color);
            pack.ambient.Analyze(data.ambient);
            pack.range.Analyze(data.range);
            pack.intensity.Analyze(data.intensity);

            return pack;
        }

        public override void Reset(ref SFXContext context, bool save)
        {
            Remove(ref lr, save);
        }
    }
}
#endif
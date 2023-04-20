#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Playables;

namespace CFEngine
{
    public class DummyAnalyze : SFXAnalyze
    {
        private AllowDisableAnimator allowDisableAnimator = null;

        public override byte ComponentType
        {
            get { return SFXComponent.SFX_COMPONENT_DUMMY; }
        }

        public override void PreAnalyze(ref SFXContext context,
               ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            tmpContext.hasTimeline = Prepare<PlayableDirector>(context.trans) != null;
            if (tmpContext.hasTimeline)
                return;
            if (!parentContext.hasAnimator)
            {
                var anim = Prepare<Animator>(context.trans);
                allowDisableAnimator = Prepare<AllowDisableAnimator>(context.trans);
                tmpContext.hasAnimator = anim != null && (anim.enabled || allowDisableAnimator != null);
                tmpContext.ator = anim;
            }
            if (!parentContext.hasAnimation)
            {
                var anim = Prepare<Animation>(context.trans);
                tmpContext.hasAnimation = anim != null;
                tmpContext.anim = anim;
            }
            if(tmpContext.hasAnimator || tmpContext.hasAnimation)
            {
                context.componentType = ComponentType;
            }
        }

        public override void Reset(ref SFXContext context, bool save)
        {
            Remove(ref allowDisableAnimator, save);
        }
    }
}
#endif
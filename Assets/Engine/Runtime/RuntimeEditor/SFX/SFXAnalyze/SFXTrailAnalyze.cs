#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;
using System.Text;
using UnityEngine.CFUI;
using UnityObject = UnityEngine.Object;
using System.IO;
namespace CFEngine
{

    public class SFXTrailAnalyze : SFXAnalyze
    {
        private TrailRenderer trail;
        public override byte ComponentType
        {
            get { return SFXComponent.SFX_COMPONENT_TRAIL; }
        }

        public override void Analyze (ref SFXContext context,
               ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            base.Analyze(ref context, ref parentContext, ref tmpContext);
            trail = Prepare<TrailRenderer>(context.trans);
            AnalyzeRender(ref context, trail,ref tmpContext);
            if (trail != null && !context.stateFlag.HasFlag(SFXContext.Flag_RemoveRender))
            {
                context.componentType = ComponentType;
            }
        }

        public override void CreateComponent (ref SFXContext context, ref SFXTmpContext tmpContext)
        {
            base.CreateComponent(ref context, ref tmpContext);
            if (trail != null)
                context.comp.flag.SetFlag (SFXComp.Flag_RenderDisable, !trail.enabled);
        }
        public override void Reset(ref SFXContext context, bool save)
        {
            if (context.stateFlag.HasFlag(SFXContext.Flag_RemoveRender))
            {
                Remove(ref trail, save);
            }
            trail = null;
        }
    }
}
#endif
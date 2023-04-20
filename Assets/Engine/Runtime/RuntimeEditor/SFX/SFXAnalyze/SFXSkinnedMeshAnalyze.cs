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
    public class SFXSkinnedMeshAnalyze : SFXAnalyze
    {
        SkinnedMeshRenderer smr;
        public override byte ComponentType
        {
            get { return SFXComponent.SFX_COMPONENT_MESH; }
        }
        public override void Analyze (ref SFXContext context,
               ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            smr = Prepare<SkinnedMeshRenderer>(context.trans);
            if (smr != null)
            {
                AnalyzeRender(ref context, smr, ref tmpContext);
                if (!context.stateFlag.HasFlag(SFXContext.Flag_RemoveRender))
                {
                    context.componentType = ComponentType;
                }
            }
        }

        public override void CreateComponent(ref SFXContext context, ref SFXTmpContext tmpContext)
        {
            base.CreateComponent(ref context, ref tmpContext);
            if (smr != null)
            {
                var sharedMaterial = smr.sharedMaterial;
                context.comp.flag.SetFlag(SFXRender.Flag_IsOpaque, sharedMaterial != null && sharedMaterial.renderQueue<=2000);
            }
        }

        public override void Reset(ref SFXContext context, bool save)
        {
            if (context.stateFlag.HasFlag(SFXContext.Flag_RemoveRender))
            {
                Remove(ref smr, save);
            }
            smr = null;
        }
    }

}
#endif
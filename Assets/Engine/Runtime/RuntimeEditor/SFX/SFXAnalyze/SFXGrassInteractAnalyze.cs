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
    public class SFXGrassInteractAnalyze : SFXAnalyze
    {
         private GrassInteract IB;
        public override byte ComponentType
        {
            get { return SFXComponent.SFX_COMPONENT_GRASSINTERACT; }
        }

        public override void Analyze(ref SFXContext context,
               ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            base.Analyze(ref context, ref parentContext, ref tmpContext);
            IB = Prepare<GrassInteract>(context.trans);
            if (IB != null)
            {
                context.componentType = ComponentType;
            }
          
        }


        public override void CreateComponent(ref SFXContext context, ref SFXTmpContext tmpContext)
        {
            base.CreateComponent(ref context, ref tmpContext);
            if (IB != null)
            {
                var sfxGa = context.comp as SFXGrassInteract;

            }
        }

        public override void Reset(ref SFXContext context, bool save)
        {
            Remove(ref IB, save);
        }
    }
}
#endif

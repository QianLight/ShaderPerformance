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
    public class SFXDistortionAnalyze : SFXAnalyzeDep
    {
        private DistortionControl dc;
        public override byte ComponentType
        {
            get { return 255; }
        }
        public override void Analyze (ref SFXContext context,
               ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            base.Analyze(ref context, ref parentContext, ref tmpContext);
            dc = PrepareMono<DistortionControl>(ref context);
        }

        public override void CreateComponent (ref SFXContext context, ref SFXTmpContext tmpContext)
        {
            base.CreateComponent(ref context, ref tmpContext);
            if (dc != null)
            {
                context.comp.flag.SetFlag (SFXComp.Flag_Distortion, true);
            }
        }

        public override void Reset (ref SFXContext context, bool save)
        {
            Remove(ref dc, save);
        }
    }
}
#endif
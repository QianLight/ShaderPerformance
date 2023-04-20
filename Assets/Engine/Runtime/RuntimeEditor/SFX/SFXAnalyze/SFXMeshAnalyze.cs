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
    public class SFXMeshAnalyze : SFXAnalyze
    {
        private MeshFilter mf;
        private MeshRenderer mr;
        private UIOffset uioffset;
        private SfxMesh sss;
        public override byte ComponentType
        {
            get { return SFXComponent.SFX_COMPONENT_MESH; }
        }
        public override bool HasDepAnalyze { get { return true; } }
        public override void Analyze (ref SFXContext context,
               ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            base.Analyze(ref context, ref parentContext, ref tmpContext);
            mf = Prepare<MeshFilter>(context.trans);
            mr = Prepare<MeshRenderer>(context.trans);
            sss = Prepare<SfxMesh>(context.trans);
            if (mf != null && mr != null)
            {
                var mesh = mf.sharedMesh;
                if (mesh == null)
                {
                    context.stateFlag.SetFlag(SFXContext.Flag_RemoveRender, true);
                }
                AnalyzeRender(ref context, mr, ref tmpContext);
                
                if (!context.stateFlag.HasFlag(SFXContext.Flag_RemoveRender))
                {
                    uioffset.Process(ref context);
                    if (uioffset.pc != null ||
                        tmpContext.HasAnim() && !mr.enabled ||
                        sss != null)
                    {
                        context.componentType = ComponentType;
                    }
                }
                
            }
            else
            {
                if (mf == null && mr != null)
                {
                    context.Log("", "null meshFilter");
                    context.stateFlag.SetFlag(SFXContext.Flag_RemoveRender, true);
                }
                if (mf != null && mr == null)
                {
                    context.Log("", "null meshRender");
                    context.stateFlag.SetFlag(SFXContext.Flag_RemoveRender, true);
                }
            }
        }

        public override void CreateComponent (ref SFXContext context, ref SFXTmpContext tmpContext)
        {
            base.CreateComponent(ref context, ref tmpContext);
            if (mr != null)
            {
                context.comp.flag.SetFlag(SFXComp.Flag_RenderDisable, !mr.enabled);
                if(mr.enabled)
                {
                    var sharedMaterial = mr.sharedMaterial;
                    context.comp.flag.SetFlag(SFXRender.Flag_IsOpaque, sharedMaterial != null && sharedMaterial.renderQueue<=2000);
                }

                if (uioffset.pc != null)
                {
                    var meshComp = context.comp as SFXRender;
                    meshComp.flag.SetFlag(SFXRender.Flag_UISort, true);
                    meshComp.uiOffset = uioffset.uiOffset;
                }
            }
                
        }

        public override void Reset(ref SFXContext context, bool save)
        {
            if (context.stateFlag.HasFlag(SFXContext.Flag_RemoveRender))
            {
                Remove(ref mf, save);
                Remove(ref mr, save);
            }
            Remove(ref uioffset.pc, save);
            Remove(ref sss, save);
            uioffset.Reset();
            mf = null;
            mr = null;
        }
    }
}
#endif
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
    public class SFXGpuAnimationAnalyze : SFXAnalyze
    {
        private GpuAnimation ga;
        public override byte ComponentType
        {
            get { return SFXComponent.SFX_COMPONENT_GPUANIMATION; }
        }

        public override void Analyze (ref SFXContext context,
               ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            base.Analyze(ref context, ref parentContext, ref tmpContext);
            ga = Prepare<GpuAnimation>(context.trans);
            if (ga != null)
            {
                if(ga.mesh != null && ga.mat != null)
                {
                    context.componentType = ComponentType;
                }
                else
                {
                    context.stateFlag.SetFlag(SFXContext.Flag_Destroy, true);
                }
            }
        }

        public override void CreateComponent(ref SFXContext context, ref SFXTmpContext tmpContext)
        {
            base.CreateComponent(ref context, ref tmpContext);
            if (ga != null)
            {
                var sfxGa = context.comp as SFXGpuAnimation;
                sfxGa.mesh = ga.mesh;
                sfxGa.mat = ga.mat;
                sfxGa.tmpData.Clear();
                sfxGa.tmpParam.Clear();
                ga.FindChild (context.trans, sfxGa.tmpData, sfxGa.tmpParam);
                sfxGa.duration = ga.duration;
                sfxGa.indexCount = ga.mesh.GetIndexCount (0);
                sfxGa.size = ga.aabb.size;
                context.AddAsset (ga.mesh, ResObject.ResExt_Asset);
                context.AddAsset (ga.mat, ResObject.ResExt_Mat,false);

                for (int i = context.trans.childCount - 1; i >= 0; --i)
                {
                    UnityObject.DestroyImmediate(context.trans.GetChild(i).gameObject);
                }
                
            }
        }

        public override void Reset(ref SFXContext context, bool save)
        {
            Remove(ref ga, save);
        }
    }
}
#endif
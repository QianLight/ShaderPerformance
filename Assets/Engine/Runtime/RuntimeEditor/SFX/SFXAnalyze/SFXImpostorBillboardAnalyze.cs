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
    public class SFXImpostorBillboardAnalyze : SFXAnalyze
    {
        private ImpostorBillboard IB;
        public override byte ComponentType
        {
            get { return SFXComponent.SFX_COMPONENT_IMPOSTORBILLBOARD; }
        }

        public override void Analyze(ref SFXContext context,
               ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            base.Analyze(ref context, ref parentContext, ref tmpContext);
            IB = Prepare<ImpostorBillboard>(context.trans);
            if (IB != null)
            {
                if (IB.mesh != null && IB.mat != null)
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
            if (IB != null)
            {
                var sfxGa = context.comp as SFXImpostorBillboard;
                sfxGa.mesh = IB.mesh;
                sfxGa.mat = IB.mat;
              //  sfxGa.tmpData.Clear();
                sfxGa.tmpParam.Clear();
               IB.FindChild(sfxGa.tmpParam);
             //   sfxGa.duration = IB.duration;
                sfxGa.indexCount = IB.mesh.GetIndexCount(0);
                sfxGa.aabb = IB.aabb;
           //     context.AddAsset(IB.mesh, ResObject.ResExt_Asset);
                context.AddAsset(IB.mat, ResObject.ResExt_Mat, false);

                for (int i = context.trans.childCount - 1; i >= 0; --i)
                {
                    UnityObject.DestroyImmediate(context.trans.GetChild(i).gameObject);
                }
            }
        }

        public override void Reset(ref SFXContext context, bool save)
        {
            Remove(ref IB, save);
        }
    }
}
#endif
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
    public struct UIOffset
    {
        public int uiOffset;
        public CFParticleControl pc;

        public void Reset()
        {
            pc = null;
        }
        public void Process(ref SFXContext context)
        {
            pc = SFXAnalyze.PrepareMono<CFParticleControl>(ref context);
            if (pc != null)
            {
                uiOffset = pc.renderQueueOffset;
            }
        }
    }
    public class ParticleAnalyze : SFXAnalyze
    {
        private ParticleSystemRenderer r;
        private ParticleSystem ps;
        private SFXCustomData[] customData;
        private UIOffset uioffset;
        public override byte ComponentType
        {
            get { return SFXComponent.SFX_COMPONENT_PARTICLE; }
        }
        public override bool HasDepAnalyze { get { return true; } }
        public override void Analyze(ref SFXContext context,
               ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            base.Analyze(ref context, ref parentContext, ref tmpContext);
            ps = Prepare<ParticleSystem>(context.trans);
            if (ps != null)
            {
                r = Prepare<ParticleSystemRenderer>(context.trans);
                if (r != null)
                {
                    // if (r.enabled)
                    // {
                        var mats = r.sharedMaterials;
                        // if (mats != null)
                        // {
                            if (mats.Length > 1)
                            {
                                //log
                                context.Log("{0} mats", mats.Length.ToString());
                            }
                            var subEmmitor = ps.subEmitters;
                            var mat = mats[0];
                            var trail = ps.trails;
                            if (mat == null || mat.shader == null)
                            {
                                context.Log("", "null mat or shader");
                                if (!r.enabled &&!subEmmitor.enabled && !trail.enabled)
                                {
                                    context.stateFlag.SetFlag(SFXContext.Flag_RemoveRender, true);
                                }
                                else
                                {
                                    if (trail.enabled)
                                    {
                                        r.renderMode = ParticleSystemRenderMode.None;
                                    }
                                }
                            }
                            if (r.renderMode == ParticleSystemRenderMode.None && !subEmmitor.enabled && !trail.enabled)
                            {
                                context.stateFlag.SetFlag(SFXContext.Flag_RemoveRender, true);
                            }
                        // }
                        // else
                        // {
                        //     context.stateFlag.SetFlag(SFXContext.Flag_RemoveRender, true);
                        // }
                    // }
                    // else
                    // {
                    //     
                    // }
                    if (!r.enabled && !subEmmitor.enabled && !parentContext.HasAnim())
                    {
                        context.Log("", "disable render");
                        context.stateFlag.SetFlag(SFXContext.Flag_RemoveRender, true);
                    }
                    context.componentType = ComponentType;
                }
            }
            uioffset.Process(ref context);
        }

        public override void CreateComponent(ref SFXContext context, ref SFXTmpContext tmpContext)
        {
            base.CreateComponent(ref context, ref tmpContext);
            ResolveRender(ref context, r);
            var mats = r.sharedMaterials;
            if (mats != null)
            {
                if (!ps.trails.enabled)
                {
                    if(mats.Length>1)mats[1] = null;
                    r.sharedMaterials = mats;
                }
                if (r.renderMode == ParticleSystemRenderMode.None)
                {
                    for (int i = 0; i < mats.Length; ++i)
                    {
                        if (i == 1 && ps.trails.enabled) break;
                        mats[i] = null;
                    }
                    r.sharedMaterials = mats;
                    r.sharedMaterial = null;
                }
                else
                {
                    // var customModule = ps.customData;
                    // if (customModule.enabled)
                    // {
                    //     customData = new SFXCustomData[6]
                    //     {
                    //         new SFXCustomData () { scale = 1.0f },
                    //         new SFXCustomData () { scale = 1.0f },
                    //         new SFXCustomData () { scale = 1.0f },
                    //         new SFXCustomData () { scale = 1.0f },
                    //         new SFXCustomData () { scale = 1.0f },
                    //         new SFXCustomData () { scale = 1.0f },
                    //     };
                    //     ParticleData.CopyFromParticle (customData, ref customModule);
                    //     customModule.SetMode (ParticleSystemCustomData.Custom1, ParticleSystemCustomDataMode.Disabled);
                    //     customModule.SetMode (ParticleSystemCustomData.Custom2, ParticleSystemCustomDataMode.Disabled);
                    //     customModule.enabled = false;
                    // }
                }
            }

            var psComp = context.comp as SFXRender;
            psComp.customData = customData;
            var subEmmitor = ps.subEmitters;
            psComp.flag.SetFlag(SFXRender.Flag_HasSubEmmit, subEmmitor.enabled);
            psComp.flag.SetFlag(SFXComp.Flag_RenderDisable, !r.enabled);
            if (r.enabled)
            {
                psComp.flag.SetFlag(SFXRender.Flag_NeedWaveCam, r.sharedMaterial != null && r.sharedMaterial.shader.Equals(Shader.Find("WaveParticle_Soft")));
                psComp.flag.SetFlag(SFXRender.Flag_IsOpaque, r.sharedMaterial != null && r.sharedMaterial.renderQueue<=2000);
            }
            if (uioffset.pc != null)
            {
                psComp.flag.SetFlag(SFXRender.Flag_UISort, true);
                psComp.uiOffset = uioffset.uiOffset;
            }
        }

        public override void Reset(ref SFXContext context, bool save)
        {
            if (context.stateFlag.HasFlag(SFXContext.Flag_RemoveRender))
            {
                Remove(ref r, save);
                Remove(ref ps, save);
            }
            Remove(ref uioffset.pc, save);
            uioffset.Reset();
            r = null;
            ps = null;
            customData = null;
        }
    }
}
#endif
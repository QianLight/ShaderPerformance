using UnityEngine.Rendering.Universal.Internal;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal
{
    public sealed class Tempora
    {
        public delegate void EnqueuePass(ScriptableRenderPass pass);
        public static readonly ProfilingSampler TAAProfilingSampler = new ProfilingSampler("TaaFrustumJitter");
        public const string RTAName = "_CameraColorTextureA";
        public const string RTBName = "_CameraColorTextureB";

        public bool UseTAA = false;

        private FilterMode filterMode = FilterMode.Bilinear;
        private bool init = true;
        private bool useRTA = false;

        private RenderTexture rtA, rtB, last, current;
        private EnqueuePass enqueuePassHandle;
        private JitterPass jitterPass = null;
        private TemporaAASetting taaSetting = null;
        private Matrix4x4 CurrP, PrevVP;
        private RenderTargetIdentifier[] mrt;
        public Tempora()
        {
            jitterPass = new JitterPass();
            mrt = new RenderTargetIdentifier[2];
        }
        
        public bool CanDo
        {
            get 
            {
                return jitterPass.CanWork; 
            }
        }
        public void PostProcessDrawMesh(CommandBuffer cmd, RenderTextureDescriptor baseDescriptor, RenderTargetIdentifier source, RenderTargetIdentifier cameraTarget, Material mat)
        {
            int defaultTarget = cameraTarget == BuiltinRenderTextureType.CameraTarget ? 1 : 0;
            //if(defaultTarget == 1)
            {
                //baseDescriptor.graphicsFormat = Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB;
            }
            GetRT(baseDescriptor, filterMode);

            if (last == null)
                cmd.SetGlobalTexture("_LastTex", source);
            else
                cmd.SetGlobalTexture("_LastTex", last);

            if(taaSetting.MixAA)
                cmd.EnableShaderKeyword(ShaderKeywordStrings.MixAA);
            else
                cmd.DisableShaderKeyword(ShaderKeywordStrings.MixAA);

            CurrP = GL.GetGPUProjectionMatrix(jitterPass.PM, false);
            PrevVP = CurrP * jitterPass.PrevV;
            mat.SetVector("_ProjectionExtents", jitterPass.ProjectionExtents);
            mat.SetMatrix("_PrevVP", PrevVP);

            mat.SetVector("_Parameter", new Vector4(taaSetting.FeedbackMin, taaSetting.FeedbackMax, taaSetting.FeedbackSpeed, taaSetting.Sharpness));
            mat.SetVector("_Parameter2", new Vector4(jitterPass.JitterSample.x, jitterPass.JitterSample.y, defaultTarget, taaSetting.AABBSize));
            mat.SetVector("_Parameter3", new Vector4(taaSetting.MotionFallOff.x, taaSetting.MotionFallOff.y, taaSetting.MotionBlurStrength, 0));

            //mat.EnsureKeyword("MINMAX_3X3", taaSetting.Neighborhood == NeighborhoodType.MinMax3x3);
            //mat.EnsureKeyword("MINMAX_3X3_ROUNDED", taaSetting.Neighborhood == NeighborhoodType.MinMax3x3Rounded);
            //mat.EnsureKeyword("MINMAX_4TAP_VARYING", taaSetting.Neighborhood == NeighborhoodType.MinMax4TapVarying);
            //mat.EnsureKeyword("USE_OPTIMIZATIONS", taaSetting.OptimizationsClipAABB);
            //mat.EnsureKeyword("UNJITTER_COLORSAMPLES", taaSetting.UnJitterColorSamples);
            //mat.EnsureKeyword("USE_DILATION_5TAP", taaSetting.Dilation == DepthDilation.Tap5);
            //mat.EnsureKeyword("USE_DILATION_3X3", taaSetting.Dilation == DepthDilation.Tap9);
            //mat.EnsureKeyword("USE_YCOCG", taaSetting.UseYCoOCg);
            //mat.EnsureKeyword("USE_CLIPPING", taaSetting.UseClipping);
            //mat.EnsureKeyword("USE_MOTION_BLUR", taaSetting.UseMotionBlur);
            //mat.EnsureKeyword("USE_ANTI_FLICKERING", taaSetting.UseFlickering);

            //cmd.SetViewProjectionMatrices(frustum.CurrV, frustum.PM);
            //cmd.SetViewProjectionMatrices(lastCamera.worldToCameraMatrix, lastCamera.projectionMatrix);
            //lastCamera.projectionMatrix = frustum.DefaultPM;
            //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, mat);
            mrt[0] = cameraTarget;
            mrt[1] = current;
            //cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.SetRenderTarget(mrt, cameraTarget);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, mat);
            //cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, jitterPass.PM);
            jitterPass.CanWork = false;
        }
        void DefaultRtState()
        {
            last = null;
            current = rtB;
            useRTA = false;
        }
        public ScriptableRenderPass Swap(bool useTAA, TemporaAASetting setting)
        {
            taaSetting = setting;
            UseTAA = useTAA;
            if (UseTAA)
            {
                useRTA = !useRTA;
                if (init)
                {
                    init = false;

                    DefaultRtState();
                }
                else
                {
                    if (useRTA)
                    {
                        current = rtA;
                        last = rtB;
                    }
                    else
                    {
                        current = rtB;
                        last = rtA;
                    }
                }
            }
            else
            {
                ClearRT();
            }
            if(UseTAA)
            {
                jitterPass.Setup(taaSetting);
                return jitterPass;
            }
            return null;
        }

        void GetRT(RenderTextureDescriptor desc, FilterMode filter)
        {
            if (rtA == null)
            {
                 CreatRT(desc, filter);
            }
            else
            {
                if(desc.width != rtA.width || desc.graphicsFormat != rtA.graphicsFormat)
                {
                    ClearRT();
                    CreatRT(desc, filter);
                    DefaultRtState();
                }
            }
        }

        void CreatRT(RenderTextureDescriptor desc, FilterMode filter)
        {
            initRT(ref rtA, desc, RTAName, filter);
            initRT(ref rtB, desc, RTBName, filter);
        }
        void ClearRT()
        {
            deleteRT(ref rtA);
            deleteRT(ref rtB);
        }
        void initRT(ref RenderTexture rt, RenderTextureDescriptor desc, string name, FilterMode filter)
        {
            //rt = RenderTexture.GetTemporary(desc);
            rt = new RenderTexture(desc);
            rt.name = name;
            rt.filterMode = filter;
        }
        void deleteRT(ref RenderTexture rt)
        {
            if (rt != null)
            {
                //RenderTexture.ReleaseTemporary(rt);
                GameObject.DestroyImmediate(rt);
                rt = null;
            }
        }
        public void Cleanup()
        {
            ClearRT();
        }
    }
}

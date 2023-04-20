using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    public abstract class SparkRenderer
    {
        protected RenderTargetBinding rtBinding;
        protected RenderTargetIdentifier[] colorRTSetup;
        protected RenderBufferStoreAction[] storeSetup;
        protected RenderBufferLoadAction[] loadSetup;

        public virtual bool InitCamera (ref RenderingData rd, Camera camera)
        {
            ref var cameraData = ref rd.cameraData;
            cameraData.camera = camera;
            cameraData.targetTexture = camera.targetTexture;
            bool hasHSRGPU = SystemInfo.hasHiddenSurfaceRemovalOnGPU;
            bool canSkipFrontToBackSorting = (camera.opaqueSortMode == OpaqueSortMode.Default && hasHSRGPU) ||
                camera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;
            var noFrontToBackOpaqueFlags = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
            cameraData.defaultOpaqueSortFlags = canSkipFrontToBackSorting ? noFrontToBackOpaqueFlags : SortingCriteria.CommonOpaque;
            return true;
        }

        public virtual void Setup (ref ScriptableRenderContext context, ref ScriptableCullingParameters cullingParameters, ref RenderingData rd)
        {
            cullingParameters.cullingOptions = 0;
        }
        public virtual void ExecuteBeforeRendering (
            ref ScriptableRenderContext context,
            ref RenderingData rd)
        {
            // cullingParameters.cullingOptions = 0;
            // context.SetupCameraProperties (rd.cameraData.camera);

        }

        public virtual void ExecuteAfterRendering (ref ScriptableRenderContext context, ref RenderingData rd)
        {

        }
        protected virtual void ExecuteCameraRT (ref ScriptableRenderContext context, ref RenderingData rd)
        {

        }
        protected virtual void ExecuteOpaque (ref ScriptableRenderContext context, ref RenderingData rd)
        {

        }

        protected virtual void ExecuteTransparent (ref ScriptableRenderContext context, ref RenderingData rd)
        {

        }

        protected virtual void FinishRendering (ref ScriptableRenderContext context, ref RenderingData rd)
        {

        }

        protected virtual void ExecuteDistortion(ref ScriptableRenderContext context, ref RenderingData rd)
        {

        }

        public void ExecuteStack (ref ScriptableRenderContext context, ref RenderingData rd)
        { 
            Camera camera = rd.cameraData.camera;

            // Opaque blocks...
            ExecuteOpaque (ref context, ref rd);
            // Transparent blocks...
            ExecuteTransparent (ref context, ref rd);
            // Draw Gizmos...
            DrawGizmos (ref context, camera, GizmoSubset.PreImageEffects);
            //post process
            // ExecuteAfterRendering (ref context, ref rd);
            // Draw Gizmos...
            DrawGizmos (ref context, camera, GizmoSubset.PostImageEffects);
            FinishRendering (ref context, ref rd);
        }

        public void Execute (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            Camera camera = rd.cameraData.camera;

            // ExecuteBeforeRendering (ref context, ref rd);
            ExecuteCameraRT (ref context, ref rd);
            // Opaque blocks...
            ExecuteOpaque (ref context, ref rd);
            // Transparent blocks...
            ExecuteTransparent (ref context, ref rd);
            // Draw Gizmos...
            //DrawGizmos (ref context, camera, GizmoSubset.PreImageEffects);
            //post process
            //ExecuteAfterRendering (ref context, ref rd);
            // Draw Gizmos...
            DrawGizmos (ref context, camera, GizmoSubset.PostImageEffects);
            FinishRendering (ref context, ref rd);
        }

        protected ClearFlag GetClearFlag (Camera camera)
        {
            ClearFlag flag = ClearFlag.None;
            if (camera.clearFlags == CameraClearFlags.Skybox)
                flag = ClearFlag.All;
            else if (camera.clearFlags == CameraClearFlags.Depth)
                flag = ClearFlag.Depth;
            else if (camera.clearFlags == CameraClearFlags.SolidColor)
                flag = ClearFlag.Color;
            return flag;
        }

        [Conditional ("UNITY_EDITOR")]
        void DrawGizmos (ref ScriptableRenderContext context, Camera camera, GizmoSubset gizmoSubset)
        {
#if UNITY_EDITOR
            if (UnityEditor.Handles.ShouldRenderGizmos ())
                context.DrawGizmos (camera, gizmoSubset);
#endif
        }

        protected void DrawRender(ref ScriptableRenderContext context,
           ref OrderQueue drawCalls, CommandBuffer cmd)
        {
            if (drawCalls.count > 0)
            {
                var it = drawCalls.BeginGet();
                while (drawCalls.Get<DrawBatch>(ref it, out var db))
                {
                    if (db.mesh != null)
                    {
                        cmd.DrawMesh(db.mesh, db.matrix, db.mat, 0, 0);
                    }
                    else if (db.render != null)
                    {
                        cmd.DrawRenderer(db.render, db.mat, 0, 0);
                    }
                }
                RuntimeUtilities.CommitCmd(ref context, cmd);
            }
        }
        //         [Conditional ("DEBUG")]
        //         void DrawRuntimeGizmos (ref ScriptableRenderContext context, Camera camera, GizmoSubset gizmoSubset)
        //         {
        // #if DEBUG
        //             if (EngineContext.drawRuntimeGizmo)
        //                 context.DrawGizmos (camera, gizmoSubset);
        //             else
        //             {
        //                 DrawGizmos (ref context, camera, gizmoSubset);
        //             }
        // #endif
        //         }
    }
}
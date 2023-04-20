using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityObject = UnityEngine.Object;

namespace CFEngine.Editor
{
    public partial class SceneEditTool : CommonToolTemplate
    {
        class PreviewCBContext
        {
            public CommandBuffer chunkLodLevelCB;
            public CommandBuffer terrainGridCB;
            public CommandBuffer collisionVexelCB;
            public CommandBuffer lightProbeEditCB;
            public void Reset ()
            {
                if (chunkLodLevelCB != null)
                {
                    chunkLodLevelCB.Release ();
                    chunkLodLevelCB = null;
                }
                if (terrainGridCB != null)
                {
                    terrainGridCB.Release ();
                    terrainGridCB = null;
                }
                if (collisionVexelCB != null)
                {
                    collisionVexelCB.Release ();
                    collisionVexelCB = null;
                }
                if (lightProbeEditCB != null)
                {
                    lightProbeEditCB.Release ();
                    lightProbeEditCB = null;
                }
            }
        }
        class PreviewContext
        {

            public List<ComputeBuffer> cbCache = new List<ComputeBuffer> ();
            public Dictionary<int, InstanceBox[]> instanceBox = new Dictionary<int, InstanceBox[]> ();

            public byte[] groundFlags;
            public float[] h;
            // public ComputeBuffer terrainMeshHeightBuffer;
            // public ComputeBuffer terrainMeshPointBuffer;
            public bool drawTerrainCollider = false;
            

            public void Reset ()
            {
                for (int i = 0; i < cbCache.Count; ++i)
                {
                    cbCache[i].Dispose ();
                }
                cbCache.Clear ();
            }
        }

        private MaterialPropertyBlock[] previewColor = new MaterialPropertyBlock[4];

        private PreviewContext previewContext;
        private PreviewCBContext cbContext;

        private void PostInitPreview ()
        {
            previewContext = new PreviewContext ();
            cbContext = new PreviewCBContext ();
            previewColor[0] = new MaterialPropertyBlock ();
            previewColor[0].SetColor ("_Color", Color.cyan);
            previewColor[1] = new MaterialPropertyBlock ();
            previewColor[1].SetColor ("_Color", Color.magenta);
            previewColor[2] = new MaterialPropertyBlock ();
            previewColor[2].SetColor ("_Color", Color.blue);
            previewColor[3] = new MaterialPropertyBlock ();
            previewColor[3].SetColor ("_Color", Color.yellow);
        }
        private void UnInitPreview ()
        {
            if (SceneView.lastActiveSceneView != null &&
                SceneView.lastActiveSceneView.camera != null)
            {
                SceneView.lastActiveSceneView.camera.RemoveAllCommandBuffers ();
            }
            if (cbContext != null)
                cbContext.Reset ();
            if (previewContext != null)
                previewContext.Reset ();
        }

        private CommandBuffer GetPreviewCB (ref CommandBuffer cb, bool clear)
        {
            if (cb == null)
            {
                cb = new CommandBuffer () { name = "Temp CB" };
            }
            if (clear)
                cb.Clear ();
            return cb;
        }

        private void AddCB2SceneView (CommandBuffer cb, bool add, CameraEvent e = CameraEvent.BeforeImageEffects)
        {
            Camera c = Camera.main;
            c = SceneView.lastActiveSceneView != null?SceneView.lastActiveSceneView.camera : null;
            if (c != null && cb != null)
            {
                if (add)
                    c.AddCommandBuffer (e, cb);
                else
                    c.RemoveCommandBuffer (e, cb);
            }
        }
    }
}
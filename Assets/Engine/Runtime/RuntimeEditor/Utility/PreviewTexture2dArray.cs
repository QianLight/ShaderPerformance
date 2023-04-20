#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{

    public enum Preview2DArrayType
    {
        None,
        Slice,
        Atlas,
    }
    public class PreviewTexture2dArray
    {
        private RenderTexture rt;
        private RenderTexture tex2DArray;
        private int lineCount = 1;
        private int pageCount = 1;

        private int page = 0;

        public Preview2DArrayType previewType = Preview2DArrayType.None;

        private Material sliceMat;
        private Material atlasMat;

        public void Init (RenderTexture tex, int lineCount)
        {
            tex2DArray = tex;
            if (tex2DArray != null)
            {
                this.lineCount = lineCount;
                ResetLayout ();
                if (rt == null)
                {
                    rt = new RenderTexture (1024, 1024, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                    {
                    name = tex.name + "_Preview",
                    hideFlags = HideFlags.DontSave,
                    anisoLevel = 0,
                    };
                    rt.Create ();
                }
            }
        }
        public void ResetLayout ()
        {
            if (tex2DArray != null)
            {
                int texCountPrePage = lineCount * lineCount;
                pageCount = tex2DArray.depth / texCountPrePage + texCountPrePage % texCountPrePage > 0 ? 1 : 0;
            }
        }
        public void Uninit ()
        {
            if (rt != null)
            {
                EngineUtility.Destroy (rt);
                rt = null;
            }
        }
        public void Refresh ()
        {
            if (tex2DArray != null && rt != null)
            {
                CommandBuffer cb = new CommandBuffer () { name = "Temp CB" };
                cb.SetRenderTarget (rt);
                Material mat = new Material (AssetsConfig.instance.TextureArrayBake);
                mat.SetTexture ("_2DArrayTex", tex2DArray);
                int texCount = tex2DArray.depth;
                int texStart = page * (lineCount * lineCount);
                float uvSize = 1.0f / lineCount;
                for (int y = 0; y < lineCount; ++y)
                {
                    for (int x = 0; x < lineCount; ++x)
                    {
                        int index = texStart + x + y * lineCount;
                        if (index < texCount)
                        {

                            Rect viewPort = new Rect ();
                            viewPort.xMin = x * uvSize;
                            viewPort.xMax = viewPort.xMin + uvSize;
                            viewPort.xMin = viewPort.xMin * 2 - 1.0f;
                            viewPort.xMax = viewPort.xMax * 2 - 1.0f;

                            viewPort.yMin = y * uvSize;
                            viewPort.yMax = viewPort.yMin + uvSize;

                            viewPort.yMin = viewPort.yMin * 2 - 1.0f;
                            viewPort.yMax = viewPort.yMax * 2 - 1.0f;
                            Mesh mesh = RuntimeUtilities.GetScreenMesh (viewPort);
                            mat.SetInt ("_Slice", index);
                            cb.DrawMesh (mesh, Matrix4x4.identity, mat, 0, 1);
                            UnityEngine.Object.DestroyImmediate (mesh);
                        }
                    }
                }

                Graphics.ExecuteCommandBuffer (cb);
                cb.Release ();
                UnityEngine.Object.DestroyImmediate (mat);
            }
        }

        public void OnDraw ()
        {
            if (tex2DArray != null)
            {
                previewType = (Preview2DArrayType) EditorGUILayout.EnumPopup ("PreviewType", previewType);
                if (previewType == Preview2DArrayType.Slice)
                {
                    page = EditorGUILayout.IntSlider ("Index", page, 0, tex2DArray.volumeDepth - 1);
                    RuntimeUtilities.DrawInstpectorTex (
                        null,
                        AssetsConfig.instance.PreviewTex2dArray,
                        ref sliceMat);
                    sliceMat.SetInt ("_Slice", page);
                    sliceMat.SetTexture ("_TextureArray", tex2DArray);
                }
                else if (previewType == Preview2DArrayType.Atlas)
                {

                    EditorGUI.BeginChangeCheck ();
                    lineCount = EditorGUILayout.IntSlider ("LineCount", lineCount, 1, 4);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        ResetLayout ();
                    }
                    page = EditorGUILayout.IntSlider ("Page", page, 0, pageCount);
                    if (GUILayout.Button ("Refresh", GUILayout.Width (100)))
                    {
                        Refresh ();
                    }
                    RuntimeUtilities.DrawInstpectorTex (
                        rt,
                        AssetsConfig.instance.PreviewTex,
                        ref atlasMat);
                }
            }
        }
    }

}
#endif
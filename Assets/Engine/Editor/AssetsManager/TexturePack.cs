using System.Collections.Generic;
using System.IO;
using CFEngine;
using UnityEditor;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CFEngine.Editor
{
    enum ChanelMask
    {
        None = -1,
        R = 0,
        G = 1,
        B = 2,
        A = 3,
    }
    internal class TexturePack : EditorWindow
    {
        class TexPackInfo
        {
            public Texture2D tex;
            public Vector4Int chanelMask = new Vector4Int (0, 0, 0, 0);
            public Vector4Int targetChanel = new Vector4Int (-1, -1, -1, -1);
        }
        private List<TexPackInfo> texList = new List<TexPackInfo> ();
        private SpriteSize size = SpriteSize.E512x512;
        private string path = "";
        private string texName = "";
        [MenuItem (@"Assets/Tool/Tex_Pack")]
        private static void Tex_Pack ()
        {
            ShowWindow ();
        }
        public static void ShowWindow ()
        {
            TexturePack window = EditorWindow.GetWindow<TexturePack> (false);
            window.Show ();
        }

        void OnGUI ()
        {
            GUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Add", GUILayout.MaxWidth (160)))
            {
                texList.Add (new TexPackInfo ());
            }
            GUILayout.EndHorizontal ();
            GUILayout.BeginHorizontal ();
            size = (SpriteSize) EditorGUILayout.EnumPopup ("Size", (SpriteSize) size, GUILayout.MaxWidth (400));
            GUILayout.EndHorizontal ();
            GUILayout.BeginHorizontal ();
            path = EditorGUILayout.TextField ("Path", path, GUILayout.MaxWidth (400));
            texName = EditorGUILayout.TextField ("Name", texName, GUILayout.MaxWidth (400));
            GUILayout.EndHorizontal ();
            int removeIndex = -1;
            for (int i = 0; i < texList.Count; ++i)
            {
                var texInfo = texList[i];
                GUILayout.BeginHorizontal ();
                texInfo.tex = EditorGUILayout.ObjectField (texInfo.tex, typeof (Texture2D), false) as Texture2D;

                if (GUILayout.Button ("Remove", GUILayout.MaxWidth (160)))
                {
                    removeIndex = i;
                }

                GUILayout.EndHorizontal ();
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal ();
                texInfo.chanelMask.x = EditorGUILayout.Toggle ("R", texInfo.chanelMask.x > 0) ? 1 : 0;
                ChanelMask maskR = (ChanelMask) EditorGUILayout.EnumPopup ("", (ChanelMask) texInfo.targetChanel.x, GUILayout.MaxWidth (200));
                texInfo.targetChanel.x = (int) maskR;
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                texInfo.chanelMask.y = EditorGUILayout.Toggle ("G", texInfo.chanelMask.y > 0) ? 1 : 0;
                ChanelMask maskG = (ChanelMask) EditorGUILayout.EnumPopup ("", (ChanelMask) texInfo.targetChanel.y, GUILayout.MaxWidth (200));
                texInfo.targetChanel.y = (int) maskG;
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                texInfo.chanelMask.z = EditorGUILayout.Toggle ("B", texInfo.chanelMask.z > 0) ? 1 : 0;
                ChanelMask maskB = (ChanelMask) EditorGUILayout.EnumPopup ("", (ChanelMask) texInfo.targetChanel.z, GUILayout.MaxWidth (200));
                texInfo.targetChanel.z = (int) maskB;
                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();
                texInfo.chanelMask.w = EditorGUILayout.Toggle ("A", texInfo.chanelMask.w > 0) ? 1 : 0;
                ChanelMask maskA = (ChanelMask) EditorGUILayout.EnumPopup ("", (ChanelMask) texInfo.targetChanel.w, GUILayout.MaxWidth (200));
                texInfo.targetChanel.w = (int) maskA;
                GUILayout.EndHorizontal ();

                EditorGUI.indentLevel--;
            }
            if (removeIndex >= 0)
            {
                texList.RemoveAt (removeIndex);
            }
            bool pack = false;
            GUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Pack", GUILayout.MaxWidth (160)))
            {
                pack = true;
            }
            if (GUILayout.Button ("Cancel", GUILayout.MaxWidth (160)))
            {
                this.Close ();
            }
            GUILayout.EndHorizontal ();

            if (pack)
            {
                Material tmp = new Material (AssetsConfig.instance.BakeTexChanel);
                int s = size.GetSize ();
                RenderTexture rt0 = new RenderTexture (s, s, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                {
                    name = "Pack Tex",
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    anisoLevel = 0,
                    autoGenerateMips = false,
                    useMipMap = false
                };
                rt0.Create ();
                RenderTexture rt1 = new RenderTexture (s, s, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                {
                    name = "Pack Tex back",
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    anisoLevel = 0,
                    autoGenerateMips = false,
                    useMipMap = false
                };
                rt0.Create ();
                var srcRT = rt0;
                var desRT = rt1;
                TextureAssets.BeginDrawRT ();
                for (int i = 0; i < texList.Count; ++i)
                {
                    var texInfo = texList[i];
                    if (texInfo.tex != null)
                    {
                        tmp.SetTexture (ShaderIDs.MainTex, texInfo.tex);
                        tmp.SetVector ("_ChanelMask", new Vector4 (texInfo.chanelMask.x, texInfo.chanelMask.y, texInfo.chanelMask.z, texInfo.chanelMask.w));
                        tmp.SetVector ("_TargetChanel", new Vector4 (texInfo.targetChanel.x, texInfo.targetChanel.y, texInfo.targetChanel.z, texInfo.targetChanel.w));
                        tmp.SetTexture ("_LastRT", desRT);
                        TextureAssets.DrawRT (srcRT, tmp);
                        TextureAssets.BlitRT (srcRT, desRT);
                        var tmpRT = desRT;
                        desRT = srcRT;
                        srcRT = tmpRT;
                    }
                }

                TextureAssets.EndDrawRT ();
                CommonAssets.CreateAsset<Texture2D> (path, texName, ".png", srcRT);
                UnityObject.DestroyImmediate (rt0);
                UnityObject.DestroyImmediate (rt1);
                UnityEngine.Object.DestroyImmediate (tmp);
            }
        }
    }
}
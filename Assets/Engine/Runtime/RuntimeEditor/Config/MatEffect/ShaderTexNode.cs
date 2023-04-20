#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class ShaderTexture : MatEffectNode
    {
        [Editable ("key")] public string key;

        public override MatEffectTemplate GetEffectTemplate ()
        {
            if (met == null)
            {
                met = new MatEffectTemplate ()
                {
                effectType = (int) MatEffectType.Texture,
                };
            }
            FillData(met);
            met.keyID = Shader.PropertyToID (key);
            met.shaderKey = key;
            return met;
        }

        private void RefreshRes (MatEffectData data)
        {
            if (data.asset == null)
            {
                data.path = "";
            }
            else
            {
                string fullpath = AssetsPath.GetAssetFullPath(data.asset, out var ext);
                data.path = AssetsPath.GetDir(fullpath) + "/";
                ext = ext.ToLower();
                if (ext.EndsWith(ResObject.ResExt_TGA))
                {
                    data.x = ResObject.Tex_2D;
                }
                else if (ext.EndsWith (ResObject.ResExt_PNG))
                {
                    data.x = ResObject.Tex_2D_PNG;
                }
                else
                {
                    DebugLog.AddErrorLog ("only support tga png tex");
                    data.asset = null;
                    data.path = "";
                }
            }
        }

        public override void OnGUI (MatEffectData data)
        {
            EditorGUILayout.BeginHorizontal ();
            // EditorGUILayout.LabelField ("useBundleResPath", GUILayout.MaxWidth (200));
            // bool use = EditorGUILayout.Toggle ("", data.y > 0.5f, GUILayout.MaxWidth (300));
            // data.y = use?1 : 0;
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Tex", GUILayout.MaxWidth (100));
            EditorGUI.BeginChangeCheck ();
            data.asset = EditorGUILayout.ObjectField ("", data.asset, typeof (Texture), false, GUILayout.MaxWidth (300)) as Texture;
            if (EditorGUI.EndChangeCheck ())
            {
                RefreshRes (data);
            }

            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField (data.path);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField (ResObject.GetExt ((byte) data.x), GUILayout.MaxWidth (100));
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (80)))
            {
                RefreshRes (data);
            }
            EditorGUILayout.EndHorizontal ();

        }
    }
}
#endif
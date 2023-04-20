#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Spine;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static CFEngine.EditorComponentGUI;
using static UnityEditor.EditorGUILayout;
using Object = UnityEngine.Object;

namespace CFEngine
{
    public class RolePhotographer :EditorComponetSelectObject<RolePhotographer>
    {
        private RenderTexture _rt;
        private Texture2D _texture;
        private GameObject _ob;
        private GameObject _camObj;
        private GameObject _tmpOB;
        private Camera _cam;
        [SerializeField]private RoleToolBoxConfig _config;

        private string _name = "pic";
        public override void Init(GameObject selectedGameObject = null)
        {
             _ob = selectedGameObject;
             _config = AssetDatabase.LoadAssetAtPath<RoleToolBoxConfig>("Assets/Engine/Runtime/RoleToolBoxConfig.Asset");
             if (_config == null)
             { 
                 _config = ScriptableObject.CreateInstance<RoleToolBoxConfig>();
                 AssetDatabase.CreateAsset(_config,"Assets/Engine/Runtime/RoleToolBoxConfig.Asset");
             }
        }

        public override void DrawGUI()
        {
            DrawEditorField("Width", ref _config.RolePhotographer.width, IntField);
            DrawEditorField("Height", ref _config.RolePhotographer.height, IntField);
            DrawEditorField("Save Path", ref _config.RolePhotographer.path, TextField);
            DrawEditorField("Background", ref _config.RolePhotographer.color, ColorField);
            DrawEditorField("Post Processing", ref _config.RolePhotographer.usingPostProcess, Toggle);

            DrawButton("Shot", Shot);
            
            if (_texture!=null)
            {
                DrawHorizontal(PreviewImage);
                DrawEditorField("Name for Save", ref _name, TextField);
                DrawHorizontal(SaveAndDiscard);
            }
            
            Undo.RegisterCompleteObjectUndo(_config,"RoleToolBoxConfigChang");
        }

        void PreviewImage()
        {
            float ratio = (((float) _config.RolePhotographer.width / _config.RolePhotographer.height));
            bool h = (1080 * ratio) > 2160;
            int height = h ? (int)((float)440 / ratio) : (int)(443 * ((float) 1080 / 2160));
            GUILayout.Box(_texture, GUILayout.Height(height), GUILayout.Width(440));
        }
        
        void SaveAndDiscard()
        {
            DrawButton("Save PNG", Save);
            DrawButton("Discard", Discard);
        }

        void Shot()
        {
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(_config.RolePhotographer.width,_config.RolePhotographer.height,GraphicsFormat.R8G8B8A8_SRGB,24);
            descriptor.msaaSamples = 8;
            if (_rt == null)
            {
                _rt = RenderTexture.GetTemporary(descriptor);
            }
            _rt.name = "Screen_Shot";
            Object.DestroyImmediate(_tmpOB);
            _tmpOB = Object.Instantiate(_ob);
            _tmpOB.layer = 25;
            Renderer[] renderers = _tmpOB.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.gameObject.layer = 25;
            }

            if (_camObj == null)
            {
                _camObj = new GameObject();
                _cam = _camObj.AddComponent<Camera>();
            }
            _cam.CopyFrom(Camera.main);
            _cam.cullingMask = 1 << 25;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = _config.RolePhotographer.color;
            var uacd = _cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            uacd.renderPostProcessing = _config.RolePhotographer.usingPostProcess;
            _cam.targetTexture = _rt;

            UnityEngine.Rendering.RenderPipelineManager.endFrameRendering -= GetPic;
            UnityEngine.Rendering.RenderPipelineManager.endFrameRendering += GetPic;

        }

        private void GetPic(ScriptableRenderContext arg1, Camera[] arg2)
        {
            _texture = new Texture2D(_config.RolePhotographer.width, _config.RolePhotographer.height,TextureFormat.RGBA32,false);
            RenderTexture activeRT = RenderTexture.active;
            RenderTexture.active = _rt;
            _texture.ReadPixels(new Rect(0,0,_config.RolePhotographer.width,_config.RolePhotographer.height),0,0);
            Color[] colors = _texture.GetPixels();

            if (!_config.RolePhotographer.usingPostProcess)
            {
                // FIX GAMMA
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i].r = math.pow(colors[i].r, 0.45f);
                    colors[i].g = math.pow(colors[i].g, 0.45f);
                    colors[i].b = math.pow(colors[i].b, 0.45f);
                }
            }
            
            _texture.SetPixels(colors);
            _texture.Apply();
            RenderTexture.active = activeRT;
            _cam.targetTexture = null;
            Clear();
            UnityEngine.Rendering.RenderPipelineManager.endFrameRendering -= GetPic;
        }

        void Save()
        {
            
            Directory.CreateDirectory(_config.RolePhotographer.path);
            File.WriteAllBytes(_config.RolePhotographer.path+"/"+_name+".png",_texture.EncodeToPNG());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Discard();
        }

        void Discard()
        {
            _texture.EditorDestroy();
        }

        void Clear()
        {
            _tmpOB.EditorDestroy();
            _camObj.EditorDestroy();
            RenderTexture.ReleaseTemporary(_rt);
            AssetDatabase.SaveAssets(); 
        }
        public override void Destroy()
        {
            Clear();
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssetIfDirty(_config);
        }

        public override string Name()
        {
            return "Role Photographer";
        }
    }

}
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CFEngine.Editor
{
    public class SfxResScanPolicy : ScanPolicy
    {
        public override string ScanType
        {
            get { return "SFX"; }
        }
        public override string ResExt
        {
            get { return "*.prefab"; }
        }

        private List<Renderer> renders = new List<Renderer>();
        MaterialContext context = new MaterialContext();
        public override ResItem Scan(string name, string path, OrderResList result, ResScanConfig config)
        {
            var sfx = result.Add(null, name, path);
            try
            {
                if(name.Contains(" "))
                {
                    result.sb.AppendLine(string.Format("name contains space {0}", name));
                }
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                go.GetComponentsInChildren(true, renders);
                if (renders.Count > config.sfxRenderCount)
                {
                    result.sb.AppendLine(string.Format("too many render {0} {1}", renders.Count.ToString(), path));
                }
                foreach(var r in renders)
                {
                    Material mat = r.sharedMaterial;
                    if (mat != null)
                    {
                        context.Init();
                        MaterialShaderAssets.ResolveMatProperty(mat, ref context);
                    }
                    else
                    {
                        if (r is ParticleSystemRenderer)
                        {
                            r.gameObject.TryGetComponent<ParticleSystem>(out var ps);
                            if(!ps.subEmitters.enabled)
                            {
                                result.sb.AppendLine(string.Format("null mat {0} {1}", r.name, path));
                            }
                        }
                        else
                        {
                            result.sb.AppendLine(string.Format("null mat {0} {1}", r.name, path));
                        }
                        
                    }
  
                    for (int j = 0; j < context.textureValue.Count; ++j)
                    {
                        var stpv = context.textureValue[j];
                        string texExt = ResObject.GetExt(stpv.texType);
                        result.Add(sfx, stpv.path + texExt, stpv.physicPath + texExt);
                    }
                    Mesh mesh = null;
                    if (r is MeshRenderer)
                    {
                        r.gameObject.TryGetComponent<MeshFilter>(out var mf);
                        mesh = mf.sharedMesh;
                    }
                    else if (r is ParticleSystemRenderer)
                    {
                        var psr = r as ParticleSystemRenderer;
                        if (psr.renderMode == ParticleSystemRenderMode.Mesh)
                        {
                            mesh = psr.mesh;
                        }
                        //r.gameObject.TryGetComponent<ParticleSystem>(out var ps);
                        //if (ps.main.maxParticles > config.sfxMaxParticleCount)
                        //{
                        //    result.sb.AppendLine(string.Format("too many par {0} {1}", r.name, path));
                        //}
                        //var emmit = ps.emission;
                        //if(emmit.enabled)
                        //{

                        //}
                    }
                    if (mesh != null)
                    {
                        string meshPath = AssetDatabase.GetAssetPath(mesh);
                        if (!string.IsNullOrEmpty(meshPath))
                        {
                            result.Add(sfx, mesh.name + ".asset", meshPath);
                        }
                    }
       
                }
                renders.Clear();
            }
            catch (Exception e)
            {
                DebugLog.AddErrorLog(name + "_sfx_" + e.StackTrace);
            }
            return sfx;
        }
    }

    
}
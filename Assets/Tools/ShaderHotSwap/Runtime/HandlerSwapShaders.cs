using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{

    public static class HandlerSwapShaders
    {

        public static Action<Shader, string> HandlerSwapShadersAction;
        
        public static string HandlerMain(string jsonRequest)
        {
            try
            {
                MemoryLogger.Log("[HandlerSwapShaders] Swap. req:{0}", jsonRequest.Substring(0, 128));
                
                var req = JsonUtility.FromJson<SwapShadersReq>(jsonRequest);
                var res = new SwapShadersRes();
                SwapShaders(req, res);

                return OkString(res);
            }
            catch( System.Exception e )
            {
                return ErrorString(e.ToString());
            }
        }

        private class TargetMaterial
        {
            public Material mat;
            public List<TargetRenderer> renderers;
        }
        
        private class TargetRenderer
        {
            public Renderer render;
            public int matIndex;
        }


        private static List<Material> m_SpecialMaterialsList = new List<Material>();

        public static void AddSpecialMaterials(Material mat)
        {
            m_SpecialMaterialsList.Add(mat);
        }

        private static void SwapShaders(SwapShadersReq req, SwapShadersRes res)
        {
            var assetBundle = LoadAssetBundle(req.assetBundleBase64);
            
            MemoryLogger.Log("[HandlerSwapShaders] AssetBundle Loaded = " + assetBundle);
            
            var renderers = Resources.FindObjectsOfTypeAll<Renderer>().Where(x => ! IsAsset(x.gameObject));
            
            res.shaders = new List<SwappedShader>();
            
            ThrowIf( req.shaders == null,  "req.shaders == null");


            GPUInstancingRoot instanceTree = GameObject.FindObjectOfType<GPUInstancingRoot>();
            
            foreach (var remoteShader in req.shaders)
            {
                ThrowIf(remoteShader == null, "remoteShader == null");

                var shaderName = remoteShader.name;
                var shaderLoadName = remoteShader.guid;
                var shader = assetBundle.LoadAsset<Shader>(shaderLoadName);

                ThrowIf(shader == null, "shader == null");

                MemoryLogger.Log("[HandlerSwapShaders] Shader Loaded = " + shader);

                var targetMaterials = new List<TargetMaterial>();

                // Pass 1 : Find Materials & Renderers
                foreach (var renderer in renderers)
                {
                    var mats = renderer.sharedMaterials;
                    for (int i = 0; i < mats.Length; ++i)
                    {
                        var mat = mats[i];

                        if (mat == null)
                            continue;

                        if (mat.shader.name == shaderName)
                        {
                            var foundMat = targetMaterials.FirstOrDefault(x => x.mat == mat);
                            if (foundMat == null)
                            {
                                foundMat = new TargetMaterial();
                                foundMat.mat = mat;
                                foundMat.renderers = new List<TargetRenderer>();
                                targetMaterials.Add(foundMat);
                            }

                            var render = new TargetRenderer();
                            render.render = renderer;
                            render.matIndex = i;

                            foundMat.renderers.Add(render);
                        }
                    }
                }

                // Pass 2 : Swap Materials

                foreach (var targetMat in targetMaterials)
                {
                    var newMat = new Material(targetMat.mat);
                    newMat.shader = shader;
                    newMat.name += " (HotSwapped)";

                    foreach (var targetRender in targetMat.renderers)
                    {
                        MemoryLogger.Log(
                            "[HandlerSwapShaders] Replace Material. Shader:{0}, Material:{1}, Renderer:{2}, Material Index:{3}",
                            shader.name, targetMat.mat.name, targetRender.render.name, targetRender.matIndex);

                        var matLength = targetRender.render.sharedMaterials.Length;
                        if (matLength > 1 && targetRender.matIndex > 0)
                        {
                            var mats = new Material[matLength];
                            for (int i = 0; i < matLength; ++i)
                            {
                                if (i == targetRender.matIndex)
                                    mats[i] = newMat;
                                else
                                    mats[i] = targetRender.render.sharedMaterials[i];
                            }

                            targetRender.render.materials = mats;
                        }
                        else
                        {
                            targetRender.render.material = newMat;
                        }
                    }
                }


                if (instanceTree != null)
                {
                    foreach (GPUInstancingGroup group in instanceTree.groups)
                    {
                        if (group.material != null && group.material.shader.name == shaderName)
                        {
                            group.material.shader = shader;
                            group.material.name += " (HotSwapped)";
                        }
                    }
                }

                if (RenderSettings.skybox != null && RenderSettings.skybox.shader.name == shaderName)
                {
                    var newMat = new Material(RenderSettings.skybox);
                    newMat.shader = shader;
                    newMat.name += " (HotSwapped)";
                    RenderSettings.skybox = newMat;
                }


                if (HandlerSwapShadersAction != null)
                {
                    HandlerSwapShadersAction(shader, shaderName);
                }

                Application.targetFrameRate = 1000;
                
                // Pass 3 : Log Result

                var swappedShader = new SwappedShader();
                swappedShader.shader = remoteShader;
                swappedShader.materials = new List<SwappedMaterial>();
                res.shaders.Add(swappedShader);

                foreach (var targetMat in targetMaterials)
                {
                    var swappedMaterial = new SwappedMaterial();
                    swappedMaterial.material = new RemoteMaterial();
                    swappedMaterial.material.name = targetMat.mat.name;
                    swappedMaterial.material.instanceID = targetMat.mat.GetInstanceID();
                    swappedMaterial.renderers = new List<RemoteRenderer>();

                    foreach (var targetRender in targetMat.renderers)
                    {
                        var remoteRenderer = new RemoteRenderer();
                        remoteRenderer.name = targetRender.render.name;
                        remoteRenderer.instanceID = targetRender.render.GetInstanceID();

                        swappedMaterial.renderers.Add(remoteRenderer);
                    }

                    swappedShader.materials.Add(swappedMaterial);
                }


            }

            assetBundle.Unload(false);
            Resources.UnloadUnusedAssets();
        }

        private static void ThrowIf(bool condition, string msg)
        {
            if (condition) throw new Exception("[HandlerSwapShaders] " + msg);
        }

        private static bool IsAsset(GameObject go)
        {
            return go.scene.rootCount == 0;
        }

        private static AssetBundle LoadAssetBundle(string assetBundleBase64)
        {
            var bytes = Convert.FromBase64String(assetBundleBase64);
            var assetBundle = AssetBundle.LoadFromMemory(bytes);
            return assetBundle;
        }
        

        static string OkString(SwapShadersRes res)
        {
            res.error = string.Empty;
            res.log = MemoryLogger.Flush();
            return JsonUtility.ToJson(res);
        }
        
        static string ErrorString(string reason)
        {
            var res = new SwapShadersRes();;
            res.error = reason;
            res.log = MemoryLogger.Flush();
            return JsonUtility.ToJson(res);
        }
    }


}
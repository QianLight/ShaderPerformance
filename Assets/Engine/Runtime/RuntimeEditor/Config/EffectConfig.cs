#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VirtualSkill;

namespace CFEngine
{
    public class EffectPreviewContext : IEntityHandler
    {
        public XGameObject xGameObject;
        public Transform t;
        public float height = 2;
        public Vector3 localPos;
        public bool entityInit = false;
        public ulong fromId;
        private MatEffectConfig matEffectConfig;
        public bool GetComponent(uint type, out int index, out ComponentFactory factory)
        {
            index = 0;
            factory = null;
            return true;
        }

        public XGameObject GetObject()
        {
            return xGameObject;
        }

        public uint PresentID { get { return 0; } }
        public float SpeedRatio { get { return SkillHoster.GetHoster!=null ? SkillHoster.GetHoster.GetEntityRatio(fromId) : 1.0f; } }
        //EntityType GetEntityType();
        public void GetEntityPos(ref Vector4 pos)
        {
            pos = t.position;
            pos.y += height * 0.5f;
            pos.w = height;
        }

        public void GetCachedShaderVector(ref Vector4 vec)
        {
            // TODO
            vec.x = localPos.x;
            vec.y = localPos.y;
            vec.z = localPos.z;
        }

        public void GetHugeColliderData(ref List<Vector3> position, ref List<Vector3> size)
        {

        }
        public GameObject GetEditorGo()
        {
            return t != null ? t.gameObject : null;
        }
        public float GetBaseScale()
        {
            return 1;
        }
        public void SetScale(float s)
        {
            if (t != null)
            {
                t.localScale = new Vector3(s, s, s);
            }
        }
        public void BeginEffect(MatEffectConfig mec)
        {
            matEffectConfig = null;
            var effectInst = RenderEffectSystem.CreateEffectInstance(this, mec.priority, mec.effectTime, mec.fadeIn, mec.fadeOut, false);
            if (effectInst != null)
            {
                matEffectConfig = mec;
                effectInst.e = this;
                for (int k = 0; k < mec.meData.Count; ++k)
                {
                    var med = mec.meData[k];
                    if (med.nodeIndex.node != null)
                    {
                        var template = med.nodeIndex.node.GetEffectTemplate();
                        template.flag.SetFlag(med.flag.flag, true);
                        EffectObject eo = SharedObjectPool<EffectObject>.Get();
                        eo.template = template;

                        eo.partMask = med.partMask;
                        eo.keywordMask = med.param;
                        med.nodeIndex.node.UpdateEffect(this, med);
                        eo.targetValue.x = med.x;
                        eo.targetValue.y = med.y;
                        eo.targetValue.z = med.z;
                        eo.targetValue.w = med.w;
                        eo.currentValue = eo.targetValue;
                        if (!string.IsNullOrEmpty(med.path))
                        {
                            if (med.asset != null)
                            {
                                eo.path = string.Format("{0}{1}", med.path, med.asset.name);
                                LoadMgr.GetAssetHandler(ref eo.asset, eo.path,
                                    ResObject.GetExt((byte)med.x));
                                eo.targetValue.y = EffectData.useRedirectPath;
                            }
                            else
                            {
                                eo.path = med.path;
                            }

                        }
                        RenderEffectSystem.Setup(eo);
                        effectInst.effectData.Push(eo);
                    }

                }
            }
        }

        public void Update()
        {
            EngineContext context = EngineContext.instance;
            if (context != null)
            {
                if (t != null && xGameObject != null)
                {
                    if (matEffectConfig != null)
                    {
                        for (int k = 0; k < matEffectConfig.meData.Count; ++k)
                        {
                            var med = matEffectConfig.meData[k];
                            if (med.nodeIndex != null && med.nodeIndex.node != null)
                            {
                                med.nodeIndex.node.UpdateEffect(this, med);
                            }

                        }
                    }


                    xGameObject.Rotation = t.rotation;
                    int index;
                    var riIt = xGameObject.BeginGetRender(out index);
                    RendererInstance ri;
                    while (xGameObject.GetRender(ref riIt, ref index, out ri))
                    {
                        ri.RefreshMpb();
                    }
                    if (!Application.isPlaying)
                    {
                        RenderEffectSystem.Update(context);
                    }
                    MaterialBaseEffect.PostUpdate(xGameObject);
                }
            }
        }
        public void EndEffect()
        {
            RenderEffectSystem.EndEffect();
        }

        public void SetLocalPRS(ref Vector3 pos, ref Quaternion rot, ref Vector3 scale, bool enableDynamicBone)
        {
        }
    }

    [Serializable]
    public class TestGameObject
    {
        public GameObject gameObject;
        public float height;
        public string partTag = "";
    }

    [Serializable]
    public class MatEffectData
    {
        public string path;
        public UnityEngine.Object asset;
        public float x;
        public float y;
        public float z;
        public float w;
        public uint param;
        public uint partMask = 0;
        [NonSerialized]
        public NodeIndex nodeIndex;
        [NonSerialized]
        public FlagMask flag;
    }

    [Serializable]
    public class MatEffectConfig : BaseFolderHash
    {
        public string desc = "";
        public string templateID;
        public int uniqueID = 0;
        public int priority = 0;
        public float fadeIn = -1;
        public float fadeOut = -1;
        public float effectTime = -1;
        public string partTags = "";
        public List<MatEffectData> meData = new List<MatEffectData>();
    }

    public class MatEffectConfigList
    {
        public Vector2 scroll = Vector2.zero;
        public EffectTemplate et;
        public List<NodeIndex> nodeList;
        public List<MatEffectConfig> effects = new List<MatEffectConfig>();
    }

    public class MatEffectNodeInfo
    {
        public EffectTemplate et;
        public MatEffectNode node;
    }

    public class EffectConfig : AssetBaseConifg<EffectConfig>
    {
        //public EditorEffectData[] effectGroup = new EditorEffectData[(int) EffectType.Num];

        // public PartList parts = new PartList ();

        public List<TestGameObject> testGO = new List<TestGameObject>();

        public List<MatEffectConfig> matEffects = new List<MatEffectConfig>();

        //public Dictionary<string, MatEffectConfigList> effectMap = new Dictionary<string, MatEffectConfigList> ();
        [NonSerialized]
        public List<MatEffectConfigList> effectMap = new List<MatEffectConfigList>();

        private MatEffectGraph meGraph;
        private Dictionary<short, MatEffectNodeInfo> templateNodes = new Dictionary<short, MatEffectNodeInfo>();
        public static void InitEffect(EffectPreviewContext context, GameObject entity,
            EffectConfig config = null, float h = 2)
        {
            context.entityInit = false;
            context.height = h;
            if (entity != null)
            {
                context.t = entity.transform;
                if (context.xGameObject == null)
                {
                    context.xGameObject = new XGameObject();
                    context.xGameObject.flag.SetFlag(XGameObject.Flag_LoadFinish, true);
                }
                context.xGameObject.PrefabLoadFinish(entity);
                context.xGameObject.InitEditorAtor();
                while (context.xGameObject.subRes.Pop(out RendererInstance r)) { }
                context.xGameObject.renderResCount = 0;
                List<Renderer> tmpRender = EditorCommon.GetRenderers(entity);
                for (int i = 0; i < tmpRender.Count; i++)
                {
                    Renderer render = tmpRender[i];
                    if ((render is MeshRenderer || render is SkinnedMeshRenderer))
                    {
                        RendererInstance ri = SharedObjectPool<RendererInstance>.Get();
                        ri.flag.SetFlag(RendererInstance.Flag_PartActive, render.enabled);
                        ri.render = render;
                        if (render is SkinnedMeshRenderer)
                        {
                            ri.t = (render as SkinnedMeshRenderer).rootBone;
                        }
                        else
                        {
                            ri.t = render.transform;
                        }
                        ri.mi = new MaterialInstance();
                        //var mpb = CommonObject<MaterialPropertyBlock>.Get ();
                        var mat = render.sharedMaterial;
                        var effectMat = new Material(mat);
                        render.sharedMaterial = effectMat;
                        //EngineUtility.SetMatValue (mat, mpb);
                        //ri.mi.mpb = mpb;
                        ri.mi.renderMat = effectMat;
                        ri.mi.mpRef = new MatProperty()
                        {
                            srcMat = mat,
                        };
                        ri.mi.mpRef.flag.SetFlag(MatProperty.NotUnLoad, true);
                        context.xGameObject.renderResCount++;
                        context.xGameObject.subRes.Push(ri);
                    }
                }
                context.entityInit = true;
            }
        }

        public static void PostInit(EffectPreviewContext context, string partTag)
        {
            if (context.xGameObject != null)
            {
                var partConfig = PartConfig.instance;
                var pi = partConfig.GetPartInfo(partTag);
                int index;
                var riIt = context.xGameObject.BeginGetRender(out index);
                RendererInstance ri;
                while (context.xGameObject.GetRender(ref riIt, ref index, out ri))
                {
                    string n = ri.render.name.ToLower();
                    ri.partMask = PartConfig.commonMask; //all mask
                    ri.flag.SetFlag(RendererInstance.Flag_IsShadow, n.Contains("_sd_"));
                    for (int j = pi.parts.Length - 1; j >= 0; --j)
                    {
                        if (n.EndsWith(pi.parts[j]))
                        {
                            ri.partMask = (uint)pi.partsFlags[j];
                            break;
                        }
                    }
                }
            }

        }
        public void LoadEffectTemplate(List<string> names)
        {
            meGraph = AssetDatabase.LoadAssetAtPath<MatEffectGraph>(AssetsConfig.instance.MatEffectPath);
            meGraph.Init();
            templateNodes.Clear();
            for (int i = 0; i < meGraph.templates.Count; ++i)
            {
                var et = meGraph.effectTemplate[i];
                if (et.valid)
                {
                    var t = meGraph.templates[i];
                    if (names != null)
                        names.Add(et.effectName);
                    for (int j = 0; j < t.nodeList.Count; ++j)
                    {
                        var node = t.nodeList[j];
                        if (node.index >= 0 && node.index < meGraph.nodes.Count)
                        {
                            node.node = meGraph.nodes[node.index] as MatEffectNode;
                            templateNodes.Add(node.uniqueID, new MatEffectNodeInfo()
                            {
                                et = et,
                                node = node.node
                            });
                        }
                    }
                }

            }
        }

        public MatEffectGraphNodes FindEffectGraph(string effectName)
        {
            if (meGraph != null)
            {
                for (int i = 0; i < meGraph.templates.Count; ++i)
                {
                    var et = meGraph.effectTemplate[i];
                    if (et.valid && et.effectName == effectName)
                        return meGraph.templates[i];
                }
            }
            return null;
        }
        public MatEffectGraphNodes FindEffectGraph(int groupID)
        {
            if (meGraph != null)
            {
                for (int i = 0; i < meGraph.templates.Count; ++i)
                {
                    var et = meGraph.effectTemplate[i];
                    if (et.valid && et.groupID == groupID)
                        return meGraph.templates[i];
                }
            }
            return null;
        }
        public MatEffectNodeInfo FindNode(short uniqueID)
        {
            if (templateNodes.TryGetValue(uniqueID, out var node))
            {
                return node;
            }
            return null;
        }
        public MatEffectConfig FindMatEffectConfig(int uniqueID)
        {
            for (int i = 0; i < matEffects.Count; ++i)
            {
                if (matEffects[i].uniqueID == uniqueID) return matEffects[i];
            }
            return null;
        }

        public static void InitResData(MatEffectData med, string name, string ext)
        {
            if (!string.IsNullOrEmpty(med.path))
            {
                uint flag = LoadMgr.GetLoadFlag();
                string assetPath = string.Format("{0}{1}", med.path, name);
                if (ext == ResObject.ResExt_Mat)
                {
                    med.asset = LoadMgr.singleton.LoadAssetImmediate<Material>(assetPath, ext, flag);
                    LoadMgr.singleton.DestroyImmediate();
                }
                else if (ext == ResObject.ResExt_TGA || ext == ResObject.ResExt_PNG)
                {
                    med.asset = LoadMgr.singleton.LoadAssetImmediate<Texture>(assetPath, ext, flag);
                    LoadMgr.singleton.DestroyImmediate();
                }
            }
        }
    }
    public class MatEffectGraphHelp : CFSingleton<MatEffectGraphHelp>
    {
        MatEffectGraph meGraph;

    }
}
#endif
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;
using System.Text;
using UnityEngine.CFUI;
using UnityObject = UnityEngine.Object;
using System.IO;
namespace CFEngine
{
    public enum SFXFilterType
    {
        None,
        Mono,
        Collider,
        Skin,
        Mesh,
        LineRender,
        TrailRender,
        Shader,
        Depth,
        Fbx,
        SubEmmit,
        PSNoise,
        PSShapeTexture,
    }
    public struct SFXTmpContext
    {
        public bool hasTimeline;
        public bool hasAnimator;
        public bool hasAnimation;
        public Animator ator;
        public Animation anim;

        public bool HasAnim()
        {
            return hasAnimator || hasAnimation;
        }
    }
    
    public struct SFXContext
    {
        public int renderCount;
        public int psCount;
        public int emitCount;
        public string filterStr;
        public SFXFilterType filterType;
        public bool hasFilterResult;
        public StringBuilder sb;

        public HashSet<string> monos;
        public HashSet<Shader> sfxShader;

        public bool fatelError;
        public bool customData;

        //analyze
        public List<ResPathRedirect> resPath;
        public HashSet<string> resNames;
        public Stack<int> layerIndex;

        public Transform trans;
        public byte componentType;
        public SFXComp comp;
        public List<SFXComp> compList;
        public FlagMask flag;
        public static uint Flag_Log = 0x00000001;
        public static uint Flag_Fbx = 0x00000002;
        public FlagMask stateFlag;
        public static uint Flag_HasComponent = 0x00000001;
        public static uint Flag_RemoveRender = 0x00000002;
        public static uint Flag_Destroy = 0x00000004;
        public void Reset ()
        {
            if (sb != null)
            {
                sb.Clear ();
            }
            if (layerIndex == null)
            {
                layerIndex = new Stack<int> ();
            }
            hasFilterResult = false;
            renderCount = 0;
            psCount = 0;
            emitCount = 0;

            fatelError = false;
            customData = false;
            if (compList == null)
            {
                compList = new List<SFXComp> ();
            }
            compList.Clear ();
            if (resPath == null)
                resPath = new List<ResPathRedirect> ();
            resPath.Clear ();
            if (resNames == null)
            {
                resNames = new HashSet<string> ();
            }
            resNames.Clear ();
        }

        public void Init (Transform t)
        {
            trans = t;
            stateFlag.Reset ();
            comp = null;
            componentType = 255;
        }

        public void AddAsset (UnityEngine.Object asset, string ext,bool buildBundle = true)
        {
            string nameWithExt = (asset.name + ext).ToLower ();
            if (!resNames.Contains (nameWithExt))
            {
                string dir = AssetsPath.GetAssetDir (asset, out var path) + "/";
                resPath.Add (new ResPathRedirect ()
                {
                    name = asset.name.ToLower (),
                        physicPath = dir,
                        ext = ext,
                        buildBundle = buildBundle
                });
                resNames.Add (nameWithExt);
            }

        }
        // public void Copy (ref SFXContext src)
        // {
        //     renderCount = src.renderCount;
        //     psCount = src.psCount;
        //     emitCount = src.emitCount;
        //     animator = src.animator;
        //     fatelError = src.fatelError;
        // }

        public void Log (string format, string log)
        {
            if (flag.HasFlag (SFXContext.Flag_Log))
            {

            }
        }
    }

    // public interface ISFXAnayze
    // {
    //     byte ComponentType { get; }
    //     void Analyze (ref SFXContext context);
    //     void Resolve (ref SFXContext context);
    //     void CreateComponent (ref SFXContext context);
    //     void Reset ();
    // }

    public abstract class SFXAnalyze // : ISFXAnayze
    {
        private static int[] depth = new int[8];
        public virtual byte ComponentType
        {
            get { return 255; }
        }
        public virtual bool HasDepAnalyze { get { return false; } }

        public static bool CalculateDepth (ref SFXContext context, Transform trans)
        {
            depth[0] = depth[1] = depth[2] = depth[3] = depth[4] = depth[5] = depth[6] = depth[7] = 255;
            var it = context.layerIndex.GetEnumerator ();
            int index = 0;
            bool tooDeep = false;
            while (it.MoveNext ())
            {
                if (index < depth.Length)
                {
                    depth[index] = it.Current;
                }
                else
                {
                    context.fatelError = true;
                    tooDeep = true;
                    DebugLog.AddErrorLog2 ("too deep hierarchical {0} {1} from {2}", index.ToString (), trans.name, context.trans.gameObject.name);
                }

                index++;
            }
            return tooDeep;
        }
        protected static void CalculateDepth (ref SFXContext context, SFXComp comp)
        {
            comp.layerIndex.Reset ();
            CalculateDepth (ref context, context.trans);
            int index = 0;
            for (int i = depth.Length - 1; i >= 0; --i)
            {
                int d = depth[i];
                if (d != 255)
                {
                    switch (index)
                    {
                        case 0:
                            comp.layerIndex.layer0 = (byte) d;
                            index++;
                            break;
                        case 1:
                            comp.layerIndex.layer1 = (byte) d;
                            index++;
                            break;
                        case 2:
                            comp.layerIndex.layer2 = (byte) d;
                            index++;
                            break;
                        case 3:
                            comp.layerIndex.layer3 = (byte) d;
                            index++;
                            break;
                        case 4:
                            comp.layerIndex.layer4 = (byte) d;
                            index++;
                            break;
                        case 5:
                            comp.layerIndex.layer5 = (byte) d;
                            index++;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        protected static void AnalyzeRender (ref SFXContext context, Renderer r, ref SFXTmpContext tmpContext)
        {
            if (r != null)
            {
                if (r.enabled)
                {
                    var mats = r.sharedMaterials;
                    if (mats != null)
                    {
                        if (mats.Length > 1)
                        {
                            //log
                            context.Log ("{0} mats\r\n", mats.Length.ToString ());
                        }
                        for (int i = 0; i < mats.Length; ++i)
                        {
                            var mat = mats[i];
                            if (mat == null || mat.shader == null)
                            {
                                context.Log ("", "null mat or shader");
                                context.stateFlag.SetFlag (SFXContext.Flag_RemoveRender, true);
                                break;
                            }
                        }
                    }
                    else
                    {
                        context.stateFlag.SetFlag (SFXContext.Flag_RemoveRender, true);
                    }
                }
                else if (!tmpContext.HasAnim())
                {
                    context.stateFlag.SetFlag (SFXContext.Flag_RemoveRender, true);
                }
            }
        }

        protected static void ResolveRender (ref SFXContext context, Renderer r)
        {
            if (r != null)
            {
                r.allowOcclusionWhenDynamic = false;
                // r.reflectionProbeUsage = ReflectionProbeUsage.Off;
                if (r.sharedMaterial != null)
                {
                    if (!r.sharedMaterial.shader.name.Equals("URP/Scene/Uber"))
                    {
                        r.lightProbeUsage = LightProbeUsage.Off;
                        r.shadowCastingMode = ShadowCastingMode.Off;
                        r.reflectionProbeUsage = ReflectionProbeUsage.Off;
                    }
                }
            }
        }

        public static void OnCreateComponent (ref SFXContext context)
        {
            if (context.componentType < SFXComp.compFactory.Length)
            {
                var factory = SFXComp.compFactory[context.componentType];
                context.comp = factory.Create ();
                context.comp.type = context.componentType;
                CalculateDepth (ref context, context.comp);
                context.compList.Add (context.comp);
            }
        }
        public static void OnPreAnalyze(ref SFXContext context,
            ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {
            if (!parentContext.HasAnim() &&
                !context.trans.gameObject.activeSelf)
            {
                context.stateFlag.SetFlag (SFXContext.Flag_Destroy, true);
            }

            //if (context.flag.HasFlag (SFXContext.Flag_Log))
            //{
            //    var prefab = PrefabUtility.GetCorrespondingObjectFromSource (context.trans.gameObject) as GameObject;
            //    if (prefab != null)
            //    {
            //        var type = PrefabUtility.GetPrefabAssetType (prefab);
            //        if (type == PrefabAssetType.Model)
            //        {
            //            context.flag.SetFlag (SFXContext.Flag_Fbx, true);
            //        }
            //    }
            //}
        }

        public static void OnSave(ref SFXContext context, GameObject gameObject)
        {
            string name = gameObject.name;

            string path = AssetsConfig.instance.sfxDir + name + ".bytes";

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(SFXComp.VersionLatest);
                byte compCount = (byte) context.compList.Count;
                bw.Write(compCount);
                for (int i = 0; i < context.compList.Count; ++i)
                {
                    var comp = context.compList[i];
                    comp.Save(bw);
                    if (comp.type < SFXComp.compFactory.Length)
                    {
                        var factory = SFXComp.compFactory[comp.type];

                        factory.Destroy(comp);
                    }

                }
            }

            AddBytes(gameObject, path);

            //AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate);
        }

        public static void AddBytes(GameObject gameObject,string path)
        {
            if(!File.Exists(path)) return;
            
            BytesRes bts = gameObject.GetComponent<BytesRes>();
            if (bts == null)
            {
                bts = gameObject.AddComponent<BytesRes>();
            }

            gameObject.TryGetComponent(out SFXPriority pri);
            if (pri != null) bts.lodConfig = pri;
            gameObject.TryGetComponent(out SFXWarningZone warningZone);
            if (warningZone != null) bts.warningZoneConfig = warningZone;
            Byte[] bytes = File.ReadAllBytes(path);
            bts.bytes = bytes;
            bts.GetAllAnimatorManager();
            File.Delete(path);
        }


        public virtual void PreAnalyze(ref SFXContext context,
            ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {

        }
        public virtual void Analyze (ref SFXContext context, 
            ref SFXTmpContext parentContext, ref SFXTmpContext tmpContext)
        {

        }

        public static bool PreResolve(ref SFXContext context)
        {
            if (context.trans.childCount == 0 && context.stateFlag.HasFlag(SFXContext.Flag_RemoveRender))
            {
                context.stateFlag.SetFlag(SFXContext.Flag_Destroy, true);
            }
            if (context.stateFlag.HasFlag(SFXContext.Flag_Destroy))
            {
                if (context.trans != null)
                {
                    UnityObject.DestroyImmediate(context.trans.gameObject);
                    context.trans = null;
                    return false;
                }
            }
            else
            {
                Remove<Collider>(context.trans);
            }
            return true;
        }

        public virtual void CreateComponent (ref SFXContext context, ref SFXTmpContext tmpContext) 
        {
            context.comp.flag.SetFlag(SFXComp.Flag_Animator, tmpContext.hasAnimator);           
            context.comp.flag.SetFlag(SFXComp.Flag_Animation, tmpContext.hasAnimation);
            EditorCommon.RemoveMissingScript(context.trans);
        }

        public virtual void Reset (ref SFXContext context, bool save)
        {

        }
        // public virtual bool CreateComponent (ref SFXContext context,
        //     SFXEditorAsset asset,
        //     Transform trans,
        //     MonoBehaviour mono, ref bool destroyed)
        // {
        //     if (asset != null)
        //     {
        //         if (r != null || hasAnimation || hasAnimator)
        //         {
        //             config = ScriptableObject.CreateInstance<SFXComponent> ();
        //             config.componetType = ComponentType;
        //             PostCreateComponent (ref context, asset, trans, mono, config);
        //         }
        //         else if (trans.childCount == 0)
        //         {
        //             UnityObject.DestroyImmediate (trans);
        //         }
        //     }
        //     return true;
        // }
        // public void CreateBase (Transform trans)
        // {
        //     hasAnimator = SFXAnalyze.Prepare<Animator> (trans) != null;
        //     hasAnimation = SFXAnalyze.Prepare<Animation> (trans) != null;
        // }

        // public static SFXAnalyze CreateBase (Transform trans,
        //     ref SFXContext context,
        //     StringBuilder sb,
        //     bool createSFX)
        // {
        //     var hasAnimator = SFXAnalyze.Prepare<Animator> (trans) != null;
        //     var hasAnimation = SFXAnalyze.Prepare<Animation> (trans) != null;
        //     if (hasAnimator || hasAnimation)
        //     {
        //         SFXAnalyze sa = new SFXAnalyze ()
        //         {
        //             hasAnimator = hasAnimator,
        //             hasAnimation = hasAnimation
        //         };
        //         return sa;
        //     }
        //     return null;

        // }

        // public void PostCreateComponent (ref SFXContext context,
        //     SFXEditorAsset asset,
        //     Transform trans,
        //     MonoBehaviour mono,
        //     SFXComponent config)
        // {

        //     if (config != null)
        //     {
        //         if (mono is billboard)
        //         {
        //             config.flag.SetFlag (SFXComponent.Flag_Billboard, true);
        //             config.flag.SetFlag (SFXComponent.Flag_RandomZRotation, (mono as billboard).RandomZ_Rotation);
        //             UnityObject.DestroyImmediate (mono);
        //         }
        //         else if (mono is CFParticleControl)
        //         {
        //             var pc = mono as CFParticleControl;
        //             config.flag.SetFlag (SFXComponent.Flag_UISort, true);
        //             config.uiOffset = pc.renderQueueOffset;
        //             UnityObject.DestroyImmediate (mono);
        //             // if (r != null)
        //             // {
        //             //     var mat = r.sharedMaterial;
        //             //     int queue = 3000 + pc.renderQueueOffset;
        //             //     if (queue != mat.renderQueue)
        //             //         mat.renderQueue = queue;
        //             // }
        //         }
        //         else if (mono is DistortionControl)
        //         {
        //             var pc = mono as DistortionControl;
        //             config.flag.SetFlag (SFXComponent.Flag_Distortion, true);
        //             UnityObject.DestroyImmediate (mono);
        //         }
        //         config.flag.SetFlag (SFXComponent.Flag_Animator, hasAnimator);
        //         config.flag.SetFlag (SFXComponent.Flag_Animation, hasAnimation);
        //         if (r != null)
        //             config.flag.SetFlag (SFXComponent.Flag_RenderDisable, !r.enabled);
        //         CalculateDepth (ref context, config, trans);
        //         config.name = trans.name;
        //         // if (context.ator == trans)
        //         // {
        //         //     if (context.animator)
        //         //         comp.flag.SetFlag (SFXComponent.Flag_Animator, true);
        //         //     else
        //         //         comp.flag.SetFlag (SFXComponent.Flag_Animation, true);
        //         // }
        //         asset.components.Add (config);
        //         if (r != null)
        //         {
        //             r.allowOcclusionWhenDynamic = false;
        //             r.reflectionProbeUsage = ReflectionProbeUsage.Off;
        //             r.lightProbeUsage = LightProbeUsage.Off;
        //             r.shadowCastingMode = ShadowCastingMode.Off;
        //         }
        //     }
        // }

        public static T Prepare<T> (Transform trans) where T : Component
        {
            T v = default (T);
            trans.TryGetComponent<T> (out v);
            return v;
        }

        public static void Remove<T> (Transform trans) where T : Component
        {
            T v = default (T);
            if (trans.TryGetComponent<T> (out v))
            {
                UnityObject.DestroyImmediate (v);
            }
        }
        public static void Remove<T>(ref T v, bool save) where T : Component
        {
            if (v != null && save)
                UnityObject.DestroyImmediate(v);
            v = null;
        }
        public static T PrepareMono<T> (ref SFXContext context) where T : Component
        {
            T v = default (T);
            context.trans.TryGetComponent<T> (out v);
            context.Log ("Script:{0}", typeof (T).Name);
            return v;
        }
    }
    public abstract class SFXAnalyzeDep : SFXAnalyze
    {
    }

    public partial class SFXWrapper : MonoBehaviour
    {
        public static SFXContext context;
        public static StringBuilder sb;
        public static HashSet<string> monos;
        public static HashSet<Shader> sfxShader;
        static List<SFXAnalyze> sfxList = new List<SFXAnalyze> ();
        static List<Component> compList = new List<Component> ();
        static List<SFXAnalyze> sfxAnalyze = new List<SFXAnalyze> ();
        static List<SFXAnalyze> sfxAnalyzeDep = new List<SFXAnalyze>();
        public static void InitContext ()
        {
            if (sb == null)
            {
                sb = new StringBuilder ();
            }
            context.sb = sb;
            if (monos == null)
            {
                monos = new HashSet<string> ();
            }
            context.monos = monos;
            monos.Clear ();
            if (sfxShader == null)
            {
                sfxShader = new HashSet<Shader> ();
            }
            sfxShader.Clear ();
            context.sfxShader = sfxShader;
            context.Reset ();

        }

        public static void FilterSfx (
            Transform trans,
            bool testFbx = true)
        {
            switch (context.filterType)
            {
                case SFXFilterType.Mono:
                    {
                        if (context.filterStr == "missing")
                        {
                            compList.Clear ();
                            trans.GetComponents (compList);
                            for (int i = compList.Count - 1; i >= 0; --i)
                            {
                                if (compList[i] == null)
                                {
                                    sb.AppendFormat ("missing script:{0}", context.filterStr, trans.name);
                                    sb.AppendLine ();
                                    context.hasFilterResult = true;
                                }
                            }
                        }
                        else
                        {
                            MonoBehaviour mono;
                            if (trans.TryGetComponent (out mono))

                            {
                                if (mono.GetType ().Name == context.filterStr)
                                {
                                    sb.AppendFormat ("mono_{0}:{1}", context.filterStr, trans.name);
                                    sb.AppendLine ();
                                    context.hasFilterResult = true;
                                }
                            }
                        }

                    }
                    break;
                case SFXFilterType.Collider:
                    {
                        Collider collider;
                        if (trans.TryGetComponent (out collider))
                        {
                            sb.AppendFormat ("Collider:{0}", trans.name);
                            sb.AppendLine ();
                            context.hasFilterResult = true;
                        }
                    }
                    break;
                case SFXFilterType.LineRender:
                    {
                        LineRenderer lr;
                        if (trans.TryGetComponent (out lr))
                        {
                            sb.AppendFormat ("LineRenderer:{0}", trans.name);
                            sb.AppendLine ();
                            context.hasFilterResult = true;
                        }
                    }
                    break;
                case SFXFilterType.TrailRender:
                    {
                        TrailRenderer tr;
                        if (trans.TryGetComponent (out tr))
                        {
                            sb.AppendFormat ("TrailRender:{0}", trans.name);
                            sb.AppendLine ();
                            context.hasFilterResult = true;
                        }
                    }
                    break;

                case SFXFilterType.Shader:
                    {
                        Renderer r;
                        if (trans.TryGetComponent (out r))
                        {
                            Material[] mats = r.sharedMaterials;
                            if (mats != null)
                            {
                                foreach (var mat in mats)
                                {
                                    if (mat != null && mat.shader != null &&
                                        mat.shader.name == context.filterStr)
                                    {
                                        sb.AppendFormat ("shader_{0}:{1}", context.filterStr, trans.name);
                                        sb.AppendLine ();
                                        context.hasFilterResult = true;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case SFXFilterType.Depth:
                    {
                        Renderer r;
                        if (trans.TryGetComponent (out r))
                        {
                            if (SFXAnalyze.CalculateDepth (ref context, trans))
                            {
                                context.hasFilterResult = true;
                            }
                        }
                    }
                    break;
                case SFXFilterType.Fbx:
                    {
                        if (testFbx)
                        {
                            var prefab = PrefabUtility.GetCorrespondingObjectFromSource (trans.gameObject) as GameObject;
                            if (prefab != null)
                            {
                                var type = PrefabUtility.GetPrefabAssetType (prefab);
                                if (type == PrefabAssetType.Model)
                                {
                                    testFbx = false;
                                    context.hasFilterResult = true;
                                    DebugLog.AddErrorLog2 ("error use fbx:{0}", trans.name);
                                }
                            }
                        }
                    }
                    break;
                case SFXFilterType.SubEmmit:
                    {
                        if (trans.TryGetComponent (out ParticleSystem ps))
                        {
                            var subEmmit = ps.subEmitters;
                            if (subEmmit.enabled)
                            {
                                context.hasFilterResult = true;
                            }
                        }
                    }
                    break;
                case SFXFilterType.PSNoise:
                    {
                        if (trans.TryGetComponent (out ParticleSystem ps))
                        {
                            var noise = ps.noise;
                            if (noise.enabled)
                            {
                                context.hasFilterResult = true;
                            }
                        }
                    }
                    break;
                case SFXFilterType.PSShapeTexture:
                    {
                        if (trans.TryGetComponent (out ParticleSystem ps))
                        {
                            var shape = ps.shape;
                            if (shape.enabled && shape.texture != null)
                            {
                                context.hasFilterResult = true;
                            }
                        }
                    }
                    break;

            }
            for (int i = 0; i < trans.childCount; ++i)
            {
                context.layerIndex.Push (i);
                FilterSfx (trans.GetChild (i), testFbx);
                context.layerIndex.Pop ();
            }
        }

        // public static void PreFilterSfx (
        //     Transform src,
        //     Transform trans,
        //     bool testFbx = true)
        // {
        //     Animator animator;
        //     Animation animation;
        //     if (trans.TryGetComponent (out animator))
        //     {
        //         // context.ator = trans;
        //         context.animator = true;
        //         sb.Append ("ator\r\n");
        //     }
        //     if (trans.TryGetComponent (out animation))
        //     {
        //         // context.ator = trans;
        //         sb.Append ("animation\r\n");
        //     }

        //     if (testFbx)
        //     {
        //         var prefab = PrefabUtility.GetCorrespondingObjectFromSource (src.gameObject) as GameObject;
        //         if (prefab != null)
        //         {
        //             var type = PrefabUtility.GetPrefabAssetType (prefab);
        //             if (type == PrefabAssetType.Model)
        //             {
        //                 testFbx = false;
        //                 DebugLog.AddErrorLog2 ("error use fbx:{0}", trans.name);
        //             }
        //         }
        //     }

        //     for (int i = 0; i < trans.childCount; ++i)
        //     {
        //         PreFilterSfx (src.GetChild (i), trans.GetChild (i), testFbx);
        //     }
        // }

        public static void AnalyzeSfx (
            Transform trans,
            bool save,
            ref SFXTmpContext parentContext)
        {

            context.Init (trans);
            SFXAnalyze currentAnalyze = null;
            SFXTmpContext tmpContext = new SFXTmpContext();
            for (int i = 0; i < sfxAnalyze.Count; ++i)
            {
                var analyze = sfxAnalyze[i];
                analyze.PreAnalyze(ref context, ref parentContext, ref tmpContext);
                if(context.componentType != 255)
                {
                    currentAnalyze = analyze;
                }
            }
            if (tmpContext.hasTimeline)
                return;
            
            for (int i = 0; i < sfxAnalyze.Count; ++i)
            {
                var analyze = sfxAnalyze[i];
                SFXAnalyze.OnPreAnalyze(ref context, ref parentContext, ref tmpContext);                
                if (context.stateFlag.HasFlag(SFXContext.Flag_Destroy))
                {
                    break;
                }
                analyze.Analyze(ref context, ref parentContext, ref tmpContext);
                if (context.componentType != 0 &&
                    context.componentType != 255)
                {
                    if(analyze.HasDepAnalyze)
                    {
                        for (int j = 0; j < sfxAnalyzeDep.Count; ++j)
                        {
                            sfxAnalyzeDep[j].Analyze(ref context, ref parentContext, ref tmpContext);
                        }
                    }
                    currentAnalyze = analyze;
                    break;
                }
            }

            if (save)
            {
                if (SFXAnalyze.PreResolve(ref context) && currentAnalyze != null)
                {
                    SFXAnalyze.OnCreateComponent(ref context);
                    if (context.comp != null)
                    {
                        currentAnalyze.CreateComponent(ref context, ref tmpContext);
                        if (currentAnalyze.HasDepAnalyze)
                        {
                            for (int j = 0; j < sfxAnalyzeDep.Count; ++j)
                            {
                                sfxAnalyzeDep[j].CreateComponent(ref context, ref tmpContext);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < sfxAnalyze.Count; ++i)
            {
                var analyze = sfxAnalyze[i];
                analyze.Reset(ref context, save);
            }
            for (int i = 0; i < sfxAnalyze.Count; ++i)
            {
                var analyze = sfxAnalyze[i];
                analyze.Reset(ref context, save);
            }

            if (!context.stateFlag.HasFlag(SFXContext.Flag_Destroy))
            {
                tmpContext.hasAnimation |= parentContext.hasAnimation;
                tmpContext.hasAnimator |= parentContext.hasAnimator;
                for (int i = 0; i < trans.childCount; ++i)
                {
                    Transform child = trans.GetChild(i);
                    if (trans != null)
                    {
                        if (i < trans.childCount)
                        {
                            child = trans.GetChild (i);
                        }
                        else
                        {
                            return;
                        }
                    }
                    context.layerIndex.Push(i);
                    AnalyzeSfx(child, save, ref tmpContext);
                    context.layerIndex.Pop ();
                }
            }
        }

        public static SFXEditorAsset GetSFXEditorAsset (string name, GameObject prefab = null)
        {
            string editorAssetPath = string.Format ("{0}SFX/{1}.asset", LoadMgr.singleton.editorResPath, name);
            var asset = AssetDatabase.LoadAssetAtPath<SFXEditorAsset> (editorAssetPath);
            if (asset == null)
            {
                if (prefab != null)
                {
                    asset = SFXEditorAsset.CreateInstance<SFXEditorAsset> ();
                    asset.srcSFX = prefab;
                    EditorCommon.CreateAsset<SFXEditorAsset> (editorAssetPath, ".asset", asset);
                }
            }
            else if (prefab != null)
            {
                asset.srcSFX = prefab;
                EditorCommon.SaveAsset (asset);
                asset = AssetDatabase.LoadAssetAtPath<SFXEditorAsset> (editorAssetPath);
            }
            return asset;
        }

        public void Process (GameObject prefab, bool jianxiu = false)
        {
            void AvoidExtraEffect(GameObject o)
            {
                o.TryGetComponent(out SFXPriority lodConfig);
                if (jianxiu)
                {
                    var sfxwrapper = o.GetComponent<SFXWrapper>();
                    for (int i = 0; i < sfxwrapper.avoidToei.Count; i++)
                    {
                        DestroyImmediate(sfxwrapper.avoidToei[i]);
                    }

                    if (lodConfig != null) lodConfig.CleanEmpty();
                }
            }

            try
            {
                string path = AssetDatabase.GetAssetPath (prefab);
                if (!path.StartsWith (AssetsConfig.instance.sfxDir))
                {
                    if (sfxAnalyze.Count == 0)
                    {
                        var types = EngineUtility.GetAssemblyType (typeof (SFXAnalyze));
                        foreach (var t in types)
                        {
                            var process = Activator.CreateInstance (t) as SFXAnalyze;
                            if (process != null)
                            {
                                if(process is SFXAnalyzeDep)
                                {
                                    sfxAnalyzeDep.Add(process);
                                }
                                else
                                {
                                    sfxAnalyze.Add(process);
                                }
                                
                            }
                        }
                    }
                   
                    bool activeState = prefab.activeSelf;
                    prefab.SetActive(true);
                    // //编辑器下旧文件
                    // InitContext ();
                    // Transform transOrigin = InstanceToSave(prefab);
                    // string prefabOriginPath = AssetsConfig.instance.sfxDir + transOrigin.name + ".prefab";
                    // SaveTmp(transOrigin, prefabOriginPath);
                    prefab.TryGetComponent(out SFXPriority lodConfig);
                    
                    
                    //编辑器与监修预览版本======================
                    InitContext ();
                    Transform trans = InstanceToSave(prefab);
                    AvoidExtraEffect(trans.gameObject);
                    SFXAnalyze.Remove<SFXWrapper>(trans);
                    string prefabPath = (jianxiu ? AssetsConfig.instance.sfxJianxiuDir : AssetsConfig.instance.sfxDir) + trans.name + ".prefab";
                    SaveTmp(trans, prefabPath);
                    //===================================
                    if (lodConfig != null)
                    {
                        if(!lodConfig.isSceneSFX)
                        {
                            //新分档
                            // SetLOD(prefab.name, lodConfig);
                            for (int i = 0; i < lodConfig.priorityGroups.Length; i++)
                            {
                                bool empty = true;
                                for (int j = lodConfig.priorityGroups.Length - 1; j >= i; j--)
                                {
                                    if (lodConfig.priorityGroups[j].effects.Length > 0 || lodConfig.priorityGroups[j].meshes.Length > 0)
                                    {
                                        empty = false;
                                        break;
                                    }
                                }
                                if (empty)
                                {
                                    break;
                                }
                                
                                InitContext ();
                                Transform transLOD = InstanceToSave(prefab);
                               
                                AvoidExtraEffect(transLOD.gameObject);
                                SFXAnalyze.Remove<SFXWrapper>(transLOD);
                                string tmpName = transLOD.name;
                                var tmpLODConfig = transLOD.GetComponent<SFXPriority>();
                               
                                if (i != 0)
                                {
                                    for (int k = 0; k < i; k++)
                                    {
                                        for (int j = 0; j < tmpLODConfig.priorityGroups[k].effects.Length; j++)
                                        {
                                            if (tmpLODConfig.priorityGroups[k].effects[j] != null)
                                            {
                                                SFXAnalyze.Remove<CFParticleControl>(tmpLODConfig.priorityGroups[k].effects[j].transform);
                                                SFXAnalyze.Remove<ParticleSystemRenderer>(tmpLODConfig.priorityGroups[k].effects[j].transform);
                                                SFXAnalyze.Remove<ParticleSystem>(tmpLODConfig.priorityGroups[k].effects[j].transform);
                                            }
                                            // DestroyImmediate(tmpLODConfig.priorityGroups[k].effects[j].gameObject);
                                        }
                                
                                        for (int j = 0; j < tmpLODConfig.priorityGroups[k].meshes.Length; j++)
                                        {
                                            if(tmpLODConfig.priorityGroups[k].meshes[j].gameObject!=null)DestroyImmediate(tmpLODConfig.priorityGroups[k].meshes[j].gameObject);
                                        }
                                    }
                                }
                                if (i != 3 && tmpLODConfig.priorityGroups.Length == 4 && tmpLODConfig.replaceMode)
                                {
                                    for (int j = 0; j < tmpLODConfig.priorityGroups[3].effects.Length; j++)
                                    {
                                        if (tmpLODConfig.priorityGroups[3].effects[j] != null)
                                        {
                                            SFXAnalyze.Remove<CFParticleControl>(tmpLODConfig.priorityGroups[3].effects[j]
                                                .transform);
                                            SFXAnalyze.Remove<ParticleSystemRenderer>(tmpLODConfig.priorityGroups[3]
                                                .effects[j].transform);
                                            SFXAnalyze.Remove<ParticleSystem>(tmpLODConfig.priorityGroups[3].effects[j]
                                                .transform);
                                        }
                                        // if(tmpLODConfig.priorityGroups[k].effects[j]!=null)DestroyImmediate(tmpLODConfig.priorityGroups[k].effects[j].gameObject);
                                    }
                                
                                    for (int j = 0; j < tmpLODConfig.priorityGroups[3].meshes.Length; j++)
                                    {
                                        if (tmpLODConfig.priorityGroups[3].meshes[j] != null)
                                            DestroyImmediate(tmpLODConfig.priorityGroups[3].meshes[j].gameObject);
                                    }
                                }
                                
                                SFXAnalyze.Remove<SFXPriority>(transLOD);
                                prefabPath = (jianxiu ? AssetsConfig.instance.sfxJianxiuDir : AssetsConfig.instance.sfxDir) + transLOD.name + PrefabSuffix(i) + ".prefab";
                                SaveTmp(transLOD, prefabPath);
                            }
                        }
                    }
                    // else
                    // {
                    //     InitContext ();
                    //     trans = InstanceToSave(prefab);
                    //     prefabPath = AssetsConfig.instance.sfxDir + trans.name + ".prefab";
                    //     SaveTmp(trans, prefabPath);
                    // }
                    prefab.SetActive(activeState);
                }
            }
            catch (Exception e)
            {
                string path = AssetDatabase.GetAssetPath (prefab);
                DebugLog.AddErrorLog (path + e.StackTrace);
            }
        }

        private static string PrefabSuffix(int i)
        {
            switch (i)
            {
                case 0: return "_H";
                case 1: return "_M";
                case 2: return "_L";
                case 3: return "_Base";
            }

            return "";
        }
        private static void SaveTmp(Transform trans, string prefabPath)
        {
            SFXAnalyze.OnSave(ref context, trans.gameObject);
            PrefabUtility.SaveAsPrefabAsset(trans.gameObject, prefabPath);
            if (context.resPath.Count > 0)
            {
                string configPath = string.Format("{0}Config/SFX/{1}.asset",
                    LoadMgr.singleton.editorResPath, trans.name);
                if (File.Exists(configPath))
                {
                    var rrConfig = AssetDatabase.LoadAssetAtPath<ResRedirectConfig>(configPath);
                    rrConfig.resPath.Clear();
                    rrConfig.resPath.AddRange(context.resPath);
                    EditorCommon.SaveAsset(rrConfig);
                }
                else
                {
                    var rrConfig = ResRedirectConfig.CreateInstance<ResRedirectConfig>();
                    rrConfig.name = trans.name;
                    rrConfig.resPath.AddRange(context.resPath);
                    rrConfig = EditorCommon.CreateAsset<ResRedirectConfig>(configPath, ".asset", rrConfig);
                }
            }

            UnityObject.DestroyImmediate(trans.gameObject);
        }

        private Transform InstanceToSave(GameObject prefab)
        {
            GameObject newSfx = GameObject.Instantiate(prefab);
            newSfx.name = prefab.name;
            Transform trans = newSfx.transform;
            SFXTmpContext tmpContext = new SFXTmpContext();
            AnalyzeSfx(trans, true, ref tmpContext);
            
            return trans;
        }

        public static void SetLOD(string name, SFXPriority lodConfig)
        {
            const string configPath = "Assets/BundleRes/Config/SFXLodConfigList.asset";
            SFXPrefabLodConfig config = AssetDatabase.LoadAssetAtPath<SFXPrefabLodConfig>(configPath);

            bool hasConfig = false;
            SFXPrefabLodItem item = new SFXPrefabLodItem();
            for (int i = 0; i < config.items.Count; i++)
            {
                var info = config.items[i];
                if (string.Equals(info.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    hasConfig = true;
                    item = info;
                    EditorUtility.SetDirty(config);
                }
            }
            if (hasConfig)
            {
                config.items.Remove(item);
            }

            if (!hasConfig)
            {
                item = new SFXPrefabLodItem()
                {
                    name = name.ToLower(),
                };
            }
            if( lodConfig.priorityGroups.Length > 0)item.SetLod(name, lodConfig.priorityGroups.Length - 1);
            config.items.Add(item);
            
            EditorUtility.SetDirty(config);
            
            if (EditorUtility.IsDirty(config))
                config.items.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            AssetDatabase.SaveAssetIfDirty(config);
        }

        // #endif

    }
    // #if UNITY_EDITOR
    [CustomEditor(typeof(SFXWrapper))]
    public class SFXWrapperEditor : UnityEngineEditor
    {
        SerializedProperty notControl;

        SerializedProperty customSample;

        private SerializedProperty avoidToei;

        //SerializedProperty exString;
        private string physicPath = "";
        private GameObject srcGo;
        private string sfxName = "";

        private void OnEnable()
        {
            notControl = serializedObject.FindProperty("notControl");
            customSample = serializedObject.FindProperty("customSample");
            avoidToei = serializedObject.FindProperty("avoidToei");
            //exString = serializedObject.FindProperty("exString");
            SFXWrapper sfx = target as SFXWrapper;

            // if (sfx.data == null)
            // {
            //     GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(sfx.gameObject) as GameObject;
            //     sfxName = prefab != null?prefab.name : sfx.name;
            //     string path = string.Format ("Assets/BundleRes/Runtime/SFX/{0}.bytes",
            //         sfxName);
            //     if (File.Exists (path))
            //     {
            //         sfx.data = AssetDatabase.LoadAssetAtPath<TextAsset> (path);
            //     }
            // }
            // if (sfx.asset == null)
            // {
            //     GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource (sfx.gameObject) as GameObject;
            //     string sfxName = prefab != null?prefab.name : sfx.name;
            //     sfx.asset = AssetDatabase.LoadAssetAtPath<SFXAsset> (
            //         string.Format ("Assets/BundleRes/Runtime/SFX/{0}.asset",
            //             sfxName));
            // }
            physicPath = EditorCommon.GetPrefabOrignalPath(sfx.gameObject, out var n, out srcGo);
        }

        public override void OnInspectorGUI()
        {
            SFXWrapper sfx = target as SFXWrapper;
            serializedObject.Update();
            EditorGUILayout.PropertyField(notControl);
            EditorGUILayout.PropertyField(customSample);
            EditorGUILayout.IntField("AreaMask", (int) sfx.areaMask);
            //EditorGUILayout.PropertyField(exString);
            //EditorGUILayout.ObjectField ("", sfx.data, typeof (TextAsset), false);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(physicPath);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal("box");
            
            if (GUILayout.Button("Save", GUILayout.MaxWidth(80)))
            {
                sfx.gameObject.TryGetComponent(out SFXPriority sfxp);
                if (sfxp is null)
                {
                    DebugLog.AddErrorLog("特效未做分级,非技能特效请无视（如果此log出现在运行时说明错误调用了Editor版本的特效，需要修改Prefab配置）");
                }
                Save(sfx, false);
            }
            if (GUILayout.Button("Save(监修版)", GUILayout.MaxWidth(80)))
            {
                sfx.gameObject.TryGetComponent(out SFXPriority sfxp);
                if (sfxp is null)
                {
                    DebugLog.AddErrorLog("特效未做分级,非技能特效请无视（如果此log出现在运行时说明错误调用了Editor版本的特效，需要修改Prefab配置）");
                }
                Save(sfx, true);
            }
            if (GUILayout.Button("Test", GUILayout.MaxWidth(80)))
            {
                if (string.IsNullOrEmpty(sfxName))
                {
                    GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(sfx.gameObject) as GameObject;
                    sfxName = prefab != null ? prefab.name : sfx.name;
                }

                if (!string.IsNullOrEmpty(sfxName))
                {
                    string path = string.Format("Assets/BundleRes/Runtime/SFX/{0}.prefab",
                        sfxName);
                    //string configPath = string.Format ("Assets/BundleRes/Runtime/SFX/{0}.bytes", sfxName);
                    if (File.Exists(path)) // && File.Exists(configPath))
                    {
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        sfx.sfx = prefab != null ? prefab.transform : null;
                        if (sfx.config == null)
                            sfx.config = SharedObjectPool<SFXConfig>.Get();
                        sfx.config.Reset();
                        sfx.config.name = sfxName;
                        //LoadMgr.loadContext.Init(null, sfx.config);
                        //configPath = string.Format ("Runtime/SFX/{0}", sfxName);
                        //LoadMgr.singleton.LoadData (configPath, SFXMgr.loadConfigCb);
                        SFXMgr.InitBytesConfig(sfx.sfx.gameObject, sfx.config);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(avoidToei);
            
            if (sfx.config != null && sfx.sfx != null)
            {
                EditorCommon.BeginGroup(string.Format("Componts:{0}", sfx.config.components.count.ToString()));
                var it = sfx.config.components.BeginGet();
                while (sfx.config.components.Get<SFXComp>(ref it, out var comp))
                {
                    comp.OnGUI(sfx.sfx);
                }

                EditorCommon.EndGroup();
            }

            EditorCommon.BeginGroup("Componts");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(80)))
            {
                sfx.Refresh();
                //SequenceEditor.ShowWindow(sfx.sequence);
            }

            if (GUILayout.Button(sfx.isPlaying ? "Stop" : "Play", GUILayout.MaxWidth(80)))
            {
                sfx.Play();
            }

            EditorGUILayout.EndHorizontal();
            sfx.OnCompGUI();
            EditorCommon.EndGroup();
            serializedObject.ApplyModifiedProperties();
        }

        public void Save(SFXWrapper sfx, bool jianxiu = false)
        {
            physicPath = EditorCommon.GetPrefabOrignalPath(sfx.gameObject, out var sfxName, out srcGo);
            sfx.Process(srcGo, jianxiu);
            if (sfx.data == null)
                sfx.data = AssetDatabase.LoadAssetAtPath<TextAsset>(
                    string.Format("Assets/BundleRes/Runtime/SFX/{0}.bytes", sfxName));
            // if (SFXWrapper.context.sb.Length > 0)
            //     DebugLog.AddEngineLog (SFXWrapper.context.sb.ToString ());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/引擎/Bytes/Effects")]
        public static void Bytes_Effects()
        {
            string[] guids = AssetDatabase.FindAssets("t:prefab", new string[] {"Assets/BundleRes/Runtime/SFX"});
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string bytesPath = path.Replace(".prefab", ".bytes");
                if (!File.Exists(bytesPath)) continue;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab)
                {
                    continue;
                }

                GameObject newObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                SFXAnalyze.AddBytes(newObj, bytesPath);
                PrefabUtility.ApplyPrefabInstance(newObj, InteractionMode.UserAction);
                DestroyImmediate(newObj);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    [CanEditMultipleObjects, CustomEditor (typeof (SFXAsset))]
    public class SFXAssetEditor : UnityEngineEditor
    {
        public override void OnInspectorGUI ()
        {
            var sfxAsset = target as SFXAsset;
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Test", GUILayout.MaxWidth (80)))
            {
                SFXMgr.singleton.Create (sfxAsset.name);
            }
            EditorGUILayout.EndHorizontal ();
            base.OnInspectorGUI ();
        }
    }

    // #endif
}
#endif
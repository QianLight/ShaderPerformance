using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using a;
using CFEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;



public class SFXPrefabCleaner: EditorWindow
{
    // public string filterType;
    // public Type type;
    public Object folder;
    public bool SearchChild;
    public ResultType Result;

    public bool Empty;
    public bool RuntimeClean;
    public bool MatClean;
    public bool GPUInstancingSet;
    public bool SceneSFXmark;
        public bool markTarget;
    public bool PriorityNoneCheck;
    public bool LODConfigPatch;
        public bool forceReplace;
    public bool probeSearcher;
    public bool GrassComponentSeacher;
    public bool HideSearcher;
    public bool HeavyFXFinder;
        public int thresholdLayerAmount;
    public bool TimelineEditorFXChecker;
    public bool LoopChecker;
        public bool OpenedLoop;
    
    // public bool AddMesh;

    public bool SubEmitterCheck;
    // public Material mat;
    public string pathFolder;
    public string[] indicators = new[]
        {"multi_compile", "shader_feature"};
    public List<GameObject> objects;
    private Vector2Int partNum = Vector2Int.zero;
    
    private static SFXPrefabCleaner instance;
    private string[] oldKeywords;
    private List<string> shaderKeywords;
    private List<string> newKeywords;
    // private string oldList;
    // private string shaList;
    // private string newList;
    private bool foldout = false;
    private Object _folder;
    private int index = 0;
    private string errorResult = "";
    private string[] AssetPaths;
    private string changelist;
    private event Func<GameObject, string, bool> realProcess;
    private event Action beforeProcess;
    private event Action<bool> endProcess;
    private SFXPrefabLodConfig config;

    private Vector2 _resultPosition;

    public enum ResultType
    {
        NormalSave = 0,
        PrefabSave = 1,
        OutputList = 2
    }
    #region staticfunction

    [MenuItem("Tools/特效/特效Prefab清理")]
    public static void ShowWindow()
    {
        if(!instance)instance = GetWindow<SFXPrefabCleaner>("特效Prefab清理");
        instance.Focus();
    }
    
    public void OnGUI()
    {
        folder = EditorGUILayout.ObjectField ("Drag Folder Here", folder, typeof (UnityEngine.Object), false, GUILayout.MaxWidth (300)) as UnityEngine.Object;
        Empty = EditorGUILayout.Toggle("仅重新保存", Empty);
        RuntimeClean = EditorGUILayout.Toggle("Runtime冗余组件清理", RuntimeClean);
        MatClean = EditorGUILayout.Toggle("隐藏冗余材质球清理", MatClean);
        GPUInstancingSet = EditorGUILayout.Toggle("粒子GPU Instance分级", GPUInstancingSet);
        using (new EditorGUILayout.HorizontalScope())
        {
            SceneSFXmark = EditorGUILayout.Toggle("场景特效标记", SceneSFXmark);
            markTarget = EditorGUILayout.Toggle("标记为场景特效", markTarget);
        }

        PriorityNoneCheck = EditorGUILayout.Toggle("分级存空检测", PriorityNoneCheck);
        SubEmitterCheck = EditorGUILayout.Toggle("SubEmitter检测", SubEmitterCheck);
        using (new EditorGUILayout.HorizontalScope())
        {
            LODConfigPatch = EditorGUILayout.Toggle("LOD 配置补全", LODConfigPatch);
            forceReplace = EditorGUILayout.Toggle("强制同级重刷", forceReplace);
        }
        probeSearcher = EditorGUILayout.Toggle("物件lightprobe开启", probeSearcher);
        GrassComponentSeacher = EditorGUILayout.Toggle("草地组件搜索", GrassComponentSeacher);
        HideSearcher = EditorGUILayout.Toggle("隐藏粒子搜索", HideSearcher);
        using (new EditorGUILayout.HorizontalScope())
        {
            HeavyFXFinder = EditorGUILayout.Toggle("多层特效搜索", HeavyFXFinder);
            thresholdLayerAmount = EditorGUILayout.IntField("阈值(大于等于此数量)", thresholdLayerAmount);
        }
        TimelineEditorFXChecker = EditorGUILayout.Toggle("检查timeline内错误版本特效（与上述功能不兼容）", TimelineEditorFXChecker);
        using (new EditorGUILayout.HorizontalScope())
        {
            LoopChecker = EditorGUILayout.Toggle("Loop模式检查", LoopChecker);
            OpenedLoop = EditorGUILayout.Toggle("查询开启的资源", OpenedLoop);
        }
        
        EditorGUILayout.Space(10);
        
        SearchChild = EditorGUILayout.Toggle("需要遍历子对象", SearchChild);
        Result = (ResultType)EditorGUILayout.Popup("结果处理",(int)Result, new []{"常规特效保存","Prefab保存", "输出列表"});

        #region staticInspector

        if (_folder)
        {
            int count = objects.Count;
            using (new EditorGUILayout.HorizontalScope())
            {
                foldout = EditorGUILayout.Foldout(foldout, "Object");
                EditorGUILayout.LabelField($"{index*10}-{index*10+10}/{count}");
                if (GUILayout.Button("<-"))
                {
                    index--;
                    if (index < 0) index = 0;
                }

                if (GUILayout.Button("->"))
                {
                    index ++;
                    if (index > count / 10) index = count / 10;
                }
            }
            
            if (foldout)
            {
                for (int i = index * 10; i < Mathf.Min(index * 10 + 10, objects.Count); i++)
                {
                    EditorGUILayout.ObjectField($"Target{i}", objects[i], typeof(GameObject), true);
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            partNum = EditorGUILayout.Vector2IntField("区间[a, b)，从0开始", partNum);
            if (objects !=null && objects.Count > 0)
            {
                partNum.x = Mathf.Max(0, partNum.x);
                partNum.x = Mathf.Min(partNum.x, objects.Count - 1);
                partNum.y = Mathf.Max(partNum.x, partNum.y);
                partNum.y = Mathf.Min(partNum.y, objects.Count);
            }
            if (GUILayout.Button("Clean"))
            {
                SetFunc();
                int index = partNum.x;
                errorResult = "";
                changelist = "";
                bool hasChanged = false;
                AssetDatabase.StartAssetEditing();
                if (beforeProcess != null) beforeProcess();
                EditorApplication.update = delegate()
                {
                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("清理中", $"{objects[index].name}({index}/{objects.Count})", (float)index / (float)objects.Count);
                    hasChanged |= Process(objects[index]);
                    index++;
                    if (isCancel || index >= partNum.y)
                    {
                        EditorUtility.ClearProgressBar();
                        EditorApplication.update = null;
                        index = 0;
                        if (endProcess != null) endProcess(hasChanged);
                        AssetDatabase.StopAssetEditing();
                        AssetDatabase.Refresh();
                    }
                    
                };
                EditorApplication.update.Invoke();
                EditorGUILayout.TextArea(changelist);
            }
        }
        // mat = EditorGUILayout.ObjectField("Material", mat, typeof(Material)) as Material;

        if (errorResult.Length>0)
        {
            if (GUILayout.Button("Clear Result"))
            {
                errorResult = "";
            }

            _resultPosition = GUILayout.BeginScrollView(_resultPosition);
            EditorGUILayout.TextArea(errorResult);
            GUILayout.EndScrollView();
        }
        if (EditorGUI.EndChangeCheck ())
        {
            if (_folder != folder)
            {
                _folder = folder;
                if (_folder != null)
                {
                    objects = new List<GameObject>();
                    pathFolder = AssetDatabase.GetAssetPath(_folder);
                    AssetPaths = AssetDatabase.FindAssets($"t:Prefab",new[]{pathFolder});
                    for (int i = 0; i < AssetPaths.Length; i++)
                    {
                        objects.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(AssetPaths[i]), typeof(GameObject)) as GameObject);
                    }

                    partNum.x = 0;
                    partNum.y = objects.Count;
                }
            }
        }
        #endregion
    }

    public void SetFunc()
    {
        realProcess = default;
        if (RuntimeClean) realProcess += RuntimeCleaner;
        if (MatClean) realProcess += RedundanceMatCleaner;
        if (GPUInstancingSet) realProcess += GPUInstancingCleaner;
        if (SceneSFXmark) realProcess += SceneSFXMarker;
        if (PriorityNoneCheck) realProcess += PriorityNoneChecker;
        if (SubEmitterCheck) realProcess += SubEmitterChecker;
        if (LODConfigPatch)
        {
            beforeProcess += () =>
            {
                const string configPath = "Assets/BundleRes/Config/SFXLodConfigList.asset";
                config = AssetDatabase.LoadAssetAtPath<SFXPrefabLodConfig>(configPath);
            };
            realProcess += LODConfigPatcher;
            endProcess += (changed) =>
            {
                if (changed)
                {
                    DebugLog.AddLog("去重并重新排序");
                    IEqualityComparer<SFXPrefabLodItem> cp = new SFXPrefabLodItemComparer();
                    config.items = config.items.Distinct(cp).ToList();
                    config.items.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssetIfDirty(config);
                }
            };
        }
        if (probeSearcher) realProcess += ProbeSearcher;
        if (GrassComponentSeacher) realProcess += GrassComponentSearch;
        if (HideSearcher) realProcess += HideParticleSearch;
        if (HeavyFXFinder) realProcess += HeavyFXSearch;
        if (TimelineEditorFXChecker) realProcess += TimelineEditorFXCheck;
        if (LoopChecker) realProcess += LoopCheck;
        
        if (Empty) realProcess += EmptyChange;
        // if (AddMesh) realProcess += MeshAdd;
    }

    


    public bool Process(GameObject obj, bool isRoot = true, string rootname = "")
    {
        if (isRoot) rootname = obj.name;
        GameObject go;
        bool hasChanged = false;
        if (isRoot)
        {
            go = PrefabUtility.InstantiatePrefab(obj) as GameObject;
            // PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        else
        {
            go = obj;
        }
        
        //*****************************开始清理*****************************************
        if(realProcess!= default)hasChanged = realProcess(go, rootname);
        //**********************清理结束*************************
        if (SearchChild)
        {
            if (go.transform.childCount != 0)
            {
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    GameObject child = go.transform.GetChild(i).gameObject;
                    hasChanged |= Process(child, false, rootname);
                }
            }
        }
        if(hasChanged)DebugLog.AddLog($"{go} from root{rootname} changed");

        if (isRoot)
        {
            if (hasChanged)
            {
                if (Result == ResultType.NormalSave)
                {
                    go.TryGetComponent(out SFXWrapper sfxWrapper);
                    if (sfxWrapper != null)
                    {
                        try
                        {
                            SFXWrapperEditor editor = Editor.CreateEditor(sfxWrapper) as SFXWrapperEditor;
                            MethodInfo mi = typeof(SFXWrapperEditor).GetMethod("Save", BindingFlags.Instance | BindingFlags.Public);
                            object[] parameters = ArrayPool<object>.Get(2);
                            parameters[0] = sfxWrapper;
                            parameters[1] = false;
                            mi.Invoke(editor, parameters);
                            parameters[0] = null;
                            parameters[1] = null;
                            ArrayPool<object>.Release(parameters);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                       
                    }
                }

                if (Result != ResultType.OutputList)
                {
                    try
                    {
                        PrefabUtility.ApplyPrefabInstance(go, InteractionMode.AutomatedAction);
                        // PrefabUtility.SavePrefabAsset(go);
                   
                        Debug.Log($"处理并保存了Prefab {go.name}");
                    }
                    catch
                    {
                        Debug.Log($"Can't save {go}");
                    }
                }
                errorResult += $"{go.name}\n";
            }
            
            DestroyImmediate(go);
        }
        return hasChanged;
    }
    #endregion

    public bool LoopCheck(GameObject go, string name)
    {
        go.TryGetComponent(out ParticleSystem ps);
        if (ps != null)
        {
            return OpenedLoop ? ps.main.loop : !ps.main.loop;
        }
        return false;
    }

    public bool MeshAdd(GameObject go, string name)
    {
        go.TryGetComponent(out MeshRenderer mr);
        go.TryGetComponent(out SkinnedMeshRenderer smr);
        go.TryGetComponent(out SfxMesh sm);
        if (mr != null || smr != null)
        {
            if (sm == null)
            {
                go.AddComponent<SfxMesh>();
                return true;
            }
        }
        return false;
    }
    public bool EmptyChange(GameObject go, string name)
    {
        return true;
    }
    public bool RuntimeCleaner(GameObject go, string name)
    {
        go.TryGetComponent(out SFXWrapper wrapper);
        if (wrapper != null)
        {
            DestroyImmediate(wrapper);
            Debug.Log($"{go.name}残余SFXWrapper被清理");
            return true;
        }
            
        go.TryGetComponent(out DistortionControl dc);
        if (dc != null)
        {
            DestroyImmediate(dc);
            Debug.Log($"{go.name}残余DistortionControl被清理");
            return true;
        }
        return false;
    }

    public bool GPUInstancingCleaner(GameObject go, string name)
    {
        go.TryGetComponent(out ParticleSystem ps);
        if (ps != null)
        {
            go.TryGetComponent(out ParticleSystemRenderer psr);
            if (psr != null)
            {
                if (psr.renderMode == ParticleSystemRenderMode.Mesh && psr.enableGPUInstancing)
                {
                    errorResult += $"{name}\n";
                    if (!go.TryGetComponent(out SFXMeshInitialize smi))
                    {
                        go.AddComponent<SFXMeshInitialize>();
                        Debug.Log($"{go.name}增加脚本以控制关闭GPUInstancing");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool RedundanceMatCleaner(GameObject go, string name)
    {
        go.TryGetComponent(out ParticleSystem ps);
        if (ps != null)
        {
            go.TryGetComponent(out ParticleSystemRenderer psr);
            var componentTrails = ps.trails;
            if (componentTrails.enabled == false)
            {
                if (psr != null)
                {
                    var materials = psr.sharedMaterials;
                    List<Material> mats = new List<Material>();
                    if (materials.Length > 2)
                    {
                    
                        foreach (var mat in materials)
                        {
                            if(mat!=null) mats.Add(mat);
                        }

                        if (mats.Count > 1)
                        {
                            mats = mats.Distinct().ToList();
                        }
                    
                        psr.materials = mats.ToArray();
                        Debug.Log($"{name} 可能修改了材质");
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool SceneSFXMarker(GameObject go, string name)
    {
        go.TryGetComponent(out SFXPriority sfxPriority);
        if (sfxPriority != null)
        {
            sfxPriority.isSceneSFX = markTarget;
            return true;
        }
        return false;
    }

    public bool PriorityNoneChecker(GameObject go, string name)
    {
        go.TryGetComponent(out SFXPriority sfxPriority);
        if (sfxPriority != null)
        {
            for (int i = 0; i < sfxPriority.priorityGroups.Length; i++)
            {
                for (int j = 0; j < sfxPriority.priorityGroups[i].effects.Length; j++)
                {
                    if (sfxPriority.priorityGroups[i].effects[j] == null)
                    {
                        errorResult += $"{name}\n";
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public bool LODConfigPatcher(GameObject go, string name)
    {
        go.TryGetComponent(out SFXPriority sfxPriority);
        if (sfxPriority != null)
        {
            bool hasOld = false;
            SFXPrefabLodItem current;
            for (int i = 0; i < config.items.Count; i++)
            {
                if(config.items[i].name.Equals(go.name.ToLower()) && sfxPriority.priorityGroups.Length > 0)
                {
                    //匹配到旧数据但是是场景特效
                    if(sfxPriority.isSceneSFX)
                    {
                        config.items.RemoveAt(i);
                        return true;
                    }

                    if (!forceReplace && config.items[i].lodName.Length != sfxPriority.priorityGroups.Length ||
                        forceReplace)
                    {
                        current = config.items[i];
                        int emptyLevel = 4;
                        for (int j = sfxPriority.priorityGroups.Length - 1; j >= 0; j--)
                        {
                            if (sfxPriority.priorityGroups[j].effects?.Length == 0 &&
                                sfxPriority.priorityGroups[j].meshes?.Length == 0)
                            {
                                emptyLevel = j;
                            }
                            else
                            {
                                break;
                            }
                        }
                        current.SetLod(current.name, sfxPriority.priorityGroups.Length-1, emptyLevel);
                        config.items[i] = current;
                        return true;
                    }
                   
                }
            }
            //匹配不到旧数据
            if( sfxPriority.priorityGroups.Length > 0 && !sfxPriority.isSceneSFX)
            {
                current = new SFXPrefabLodItem();
                int emptyLevel = 4;
                for (int i = sfxPriority.priorityGroups.Length - 1; i >= 0; i--)
                {
                    if (sfxPriority.priorityGroups[i].effects?.Length == 0 &&
                        sfxPriority.priorityGroups[i].meshes?.Length == 0)
                    {
                        emptyLevel = i;
                    }
                    else
                    {
                        break;
                    }
                }
                current.SetLod(go.name.ToLower(), sfxPriority.priorityGroups.Length-1, emptyLevel);
                config.items.Add(current);
                return true;
            }
            // if (config.Exist(go.name.ToLower()))
            // {
            //     var oldConfig = config.items.Find(item => item.name.Equals(go.name.ToLower()));
            //     
            //     if (sfxPriority.priorityGroups.Length!=0 && !oldConfig.lodFlag.HasFlag((uint)(sfxPriority.priorityGroups.Length-1)))
            //     {
            //         oldConfig.lodFlag.SetFlag(0u, true);
            //         oldConfig.lodFlag.SetFlag(PrefabLodItem.Flag_Lod1, sfxPriority.priorityGroups.Length>1);
            //         oldConfig.lodFlag.SetFlag(PrefabLodItem.Flag_Lod2, sfxPriority.priorityGroups.Length>2);
            //         return true;
            //     }
            // }
            // else
            // {
            //     var newConfig = new PrefabLodItem();
            //     newConfig.name = go.name;
            //     newConfig.lodFlag.SetFlag(0u, true);
            //     newConfig.lodFlag.SetFlag(PrefabLodItem.Flag_Lod1, sfxPriority.priorityGroups.Length>1);
            //     newConfig.lodFlag.SetFlag(PrefabLodItem.Flag_Lod2, sfxPriority.priorityGroups.Length>2);
            //     config.items.Add(newConfig);
            //     return true;
            // }
        }
        return false;
    }
    
    public bool SubEmitterChecker(GameObject go, string name)
    {
        go.TryGetComponent(out ParticleSystem ps);
        if (ps != null)
        {
            if (ps.subEmitters.enabled)
            {
                DebugLog.AddLog($"{name} enabled");
            }
            return ps.subEmitters.enabled;
        }
        return false;
    }

    public bool ProbeSearcher(GameObject go, string name)
    {
        go.TryGetComponent(out ParticleSystem ps);
        if (ps != null)
        {
            go.TryGetComponent(out ParticleSystemRenderer psr);
            if (psr != null)
            {
                var mat = psr.sharedMaterial;
                if (mat == null)
                {
                    if (psr.enabled)
                    {
                        DebugLog.AddLog($"{name}有材质球丢失");
                    }
                    return false;
                }
                bool changed = false;
                if (mat.shader.name.Equals("URP/Scene/Uber"))
                {
                    if (psr.shadowCastingMode != ShadowCastingMode.On)
                    {
                        psr.shadowCastingMode = ShadowCastingMode.On;
                        changed = true;
                    }

                    if (psr.lightProbeUsage != LightProbeUsage.BlendProbes)
                    {
                        psr.lightProbeUsage = LightProbeUsage.BlendProbes;
                        changed = true;
                    }

                    if (psr.reflectionProbeUsage != ReflectionProbeUsage.BlendProbesAndSkybox)
                    {
                        psr.reflectionProbeUsage = ReflectionProbeUsage.BlendProbesAndSkybox;
                        changed = true;
                    }
                }
                else
                {
                    
                    if (psr.shadowCastingMode != ShadowCastingMode.Off)
                    {
                        psr.shadowCastingMode = ShadowCastingMode.Off;
                        changed = true;
                    }

                    if (psr.lightProbeUsage != LightProbeUsage.Off)
                    {
                        psr.lightProbeUsage = LightProbeUsage.Off;
                        changed = true;
                    }

                    if (psr.reflectionProbeUsage != ReflectionProbeUsage.Off)
                    {
                        psr.reflectionProbeUsage = ReflectionProbeUsage.Off;
                        changed = true;
                    }
                }
                return changed;
            }
        }
        return false;

    }
    
    public bool GrassComponentSearch(GameObject go, string name)
    {
        go.TryGetComponent(out GrassInteract ps);
        if (ps != null)
        {
            return true;
        }
        return false;
    }

    public bool HideParticleSearch(GameObject go, string name)
    {
        go.TryGetComponent(out ParticleSystem ps);
        if (ps != null && !go.activeSelf)
        {
            return true;
        }

        return false;
    }

    private bool TimelineEditorFXCheck(GameObject timeline, string timelineName)
    {
        var editSFX = timeline.GetComponentsInChildren<SFXWrapper>();
        return editSFX == null;
    }

    private bool HeavyFXSearch(GameObject arg1, string arg2)
    {
        var trans = arg1.transform;
        var pss = trans.GetComponentsInChildren<ParticleSystem>();
        return pss.Length >= thresholdLayerAmount;
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace CFEngine.Editor
{
    public class PrefabConvertContext
    {
        public int index = -1;
        public int prefabIndex = -1;
        public EditorPrefabData config;
        public PrefabPathConfig ppc;

    }
    public class PrefabConfigTool : BaseConfigTool<EditorPrefabData>
    {
        enum OpPrefabConfigType
        {
            OpNone,
            OpRefresh,
            // OpGenPrefabRes,
            // OpCreateFbxConfig,
            // OpExport,
            // OpExportConfig,
            OpCleanPrefab,
            OpCheckValid,
            OpSavePrefabData,
        }
        private OpPrefabConfigType opPrefabType = OpPrefabConfigType.OpNone;
        private bool showBytesConfig = false;
        private Vector2 bytesScroll = Vector2.zero;
        private Vector2 resScroll = Vector2.zero;
        private bool showResRedirect = false;

        private PrefabConvertContext context = new PrefabConvertContext ();

        private Dictionary<byte, ResStatistic> resStatistic = new Dictionary<byte, ResStatistic> ();

        public override void OnInit ()
        {
            base.OnInit ();
            config = EditorPrefabData.instance;
            RefreshPrefabFolder (config, context, false);
        }

        protected override void OnConfigGui (ref Rect rect)
        {
            if (config.folder.FolderGroup ("PrefabList", "PrefabList", rect.width))
            {
                EditorGUILayout.BeginHorizontal ();
                if (GUILayout.Button ("Add", GUILayout.MaxWidth (80)))
                {
                    config.prefabFolder.Add (new PrefabPathConfig ());
                }
                if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (80)))
                {
                    opPrefabType = OpPrefabConfigType.OpRefresh;
                }

                if (GUILayout.Button ("SavePrefabData", GUILayout.MaxWidth (120)))
                {
                    opPrefabType = OpPrefabConfigType.OpSavePrefabData;
                }

                bool refreshByteList = false;
                EditorGUI.BeginChangeCheck ();
                EditorGUILayout.LabelField ("ShowBytesConfig", GUILayout.MaxWidth (100));
                showBytesConfig = EditorGUILayout.Toggle ("", showBytesConfig, GUILayout.MaxWidth (120));
                EditorGUILayout.EndHorizontal ();
                if (EditorGUI.EndChangeCheck ())
                {
                    refreshByteList = showBytesConfig;
                }

                if (showBytesConfig)
                {
                    BytesPrefabListGUI (ref rect, refreshByteList);
                }

                int removeIndex = -1;

                for (int i = 0; i < config.prefabFolder.Count; ++i)
                {
                    var ppc = config.prefabFolder[i];
                    EditorGUILayout.BeginHorizontal ();
                    ToolsUtility.FolderSelect (ref ppc.srcPath);
                    string folderPath = ppc.GetHash ();
                    bool isFolder = config.folder.IsFolder (folderPath);
                    if (GUILayout.Button (isFolder ? "Hide" : "Show", GUILayout.MaxWidth (80)))
                    {
                        isFolder = !isFolder;
                        config.folder.SetFolder (folderPath, isFolder);
                    }
                    if (GUILayout.Button ("Delete", GUILayout.MaxWidth (80)))
                    {
                        removeIndex = i;
                    }
                    EditorGUILayout.EndHorizontal ();
                    EditorGUILayout.BeginHorizontal ();

                    // if (GUILayout.Button ("GenPrefabRes", GUILayout.MaxWidth (80)))
                    // {
                    //     context.index = i;
                    //     context.prefabIndex = -1;
                    //     opPrefabType = OpPrefabConfigType.OpGenPrefabRes;
                    // }
                    // if (GUILayout.Button ("CleanUnusesd", GUILayout.MaxWidth (120)))
                    // {
                    //     context.index = i;
                    //     opPrefabType = OpPrefabConfigType.OpCleanPrefab;
                    // }
                    // if (GUILayout.Button ("Check", GUILayout.MaxWidth (120)))
                    // {
                    //     context.index = i;
                    //     opPrefabType = OpPrefabConfigType.OpCheckValid;
                    // }
                    // if (ppc.isSfx)
                    // {

                    // }
                    // else
                    // {
                    //     // if (GUILayout.Button ("CreateFBXConfig", GUILayout.MaxWidth (120)))
                    //     // {
                    //     //     context.index = i;
                    //     //     opPrefabType = OpPrefabConfigType.OpCreateFbxConfig;
                    //     // }
                    //     // if (GUILayout.Button ("ExportAll", GUILayout.MaxWidth (120)))
                    //     // {
                    //     //     context.index = i;
                    //     //     opPrefabType = OpPrefabConfigType.OpExport;
                    //     // }
                    //     // if (GUILayout.Button ("MakeConfig", GUILayout.MaxWidth (120)))
                    //     // {
                    //     //     context.index = i;
                    //     //     opPrefabType = OpPrefabConfigType.OpExportConfig;
                    //     // }
                    // }
                    EditorGUILayout.EndHorizontal ();
                    if (isFolder)
                    {
                        EditorGUI.indentLevel++;
                        PrefabListGUI (ppc, i, ref rect);
                        EditorGUI.indentLevel--;
                    }

                }
                if (removeIndex >= 0)
                {
                    config.prefabFolder.RemoveAt (removeIndex);
                }
                EditorCommon.EndFolderGroup ();
            }
        }

        private void PrefabListGUI (PrefabPathConfig ppc, int index, ref Rect rect)
        {
            EditorCommon.BeginScroll (ref ppc.scroll, ppc.configs.Count, 20, 600, rect.width - 20);
            for (int i = 0; i < ppc.configs.Count; ++i)
            {
                var c = ppc.configs[i];
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField (string.Format ("{0}.src", i.ToString ()), GUILayout.MaxWidth (60));
                EditorGUILayout.ObjectField ("", c.src, typeof (GameObject), false, GUILayout.MaxWidth (300));
                EditorGUILayout.LabelField ("des", GUILayout.MaxWidth (40));
                EditorGUILayout.ObjectField ("", c.des, typeof (GameObject), false, GUILayout.MaxWidth (300));

                if (GUILayout.Button ("Test", GUILayout.MaxWidth (80)))
                {
                    PrefabTestTool.Init (c);
                }

                EditorGUILayout.EndHorizontal ();
            }
            EditorCommon.EndScroll ();
        }
        private void RefreshBytesInfo ()
        {
            var pc = PrefabConfig.singleton;
            var i = pc.prefabs.GetEnumerator ();
            while (i.MoveNext ())
            {
                var current = i.Current;
                var rb = current.Value;
                if (rb is PrefabAsset)
                {
                    var pa = rb as PrefabAsset;
                    if (pa.asset == null)
                    {
                        string path = string.Format ("{0}{1}.prefab", LoadMgr.singleton.BundlePath, pa.res.path);
                        pa.asset = AssetDatabase.LoadAssetAtPath<GameObject> (path);
                    }
                    for (int ii = 0; ii < pa.meshCount; ++ii)
                    {
                        ref var mi = ref pc.meshInfo[pa.meshIndex + ii];
                        if (mi.mesh == null)
                        {
                            mi.mesh = AssetDatabase.LoadAssetAtPath<Mesh> (mi.res.path + ResObject.ResExt_Asset);
                        }
                    }
                    for (int ii = 0; ii < pa.subResCount; ++ii)
                    {
                        ref var sr = ref pc.subRes[pa.subResIndex + ii];
                        if (sr.sfx == null && !string.IsNullOrEmpty (sr.resName))
                        {
                            sr.sfx = AssetDatabase.LoadAssetAtPath<SFXAsset> (
                                string.Format ("{0}runtime/sfx/{1}.asset",
                                    LoadMgr.singleton.BundlePath, sr.resName));
                        }
                    }
                }
            }
            resStatistic.Clear ();

            var j = LoadMgr.resPathInEditor.GetEnumerator ();
            while (j.MoveNext ())
            {
                var current = j.Current;
                byte type = ResObject.Mesh;
                string typeStr = "Mesh";
                if (current.Value.name.EndsWith (ResObject.ResExt_Asset))
                {
                    type = ResObject.Mesh;
                    typeStr = "Mesh";
                }
                else
                {
                    type = ResObject.Tex_2D;
                    typeStr = "Texture";
                }

                ResStatistic rs;
                if (!resStatistic.TryGetValue (type, out rs))
                {
                    rs = new ResStatistic ()
                    {
                        type = typeStr
                    };
                    resStatistic.Add (type, rs);
                }
                rs.res.Add (current.Value);

            }
        }
        private void BytesPrefabListGUI (ref Rect rect, bool refresh = false)
        {
            var pc = PrefabConfig.singleton;
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Reload", GUILayout.MaxWidth (80)))
            {
                pc.Init ();
                RefreshBytesInfo ();
            }
            EditorGUILayout.EndHorizontal ();

            if (refresh)
            {
                RefreshBytesInfo ();
            }
            EditorCommon.BeginScroll (ref bytesScroll, pc.prefabs.Count, 20, 600, rect.width - 20);
            var it = pc.prefabs.GetEnumerator ();
            int index = 0;
            while (it.MoveNext ())
            {
                var current = it.Current;
                var rb = current.Value;
                if (rb is PrefabAsset)
                {
                    var pa = rb as PrefabAsset;
                    EditorCommon.BeginGroup ("", true, rect.width - 50);
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.LabelField (string.Format ("{0}.{1}(part:{2} subres:{3})",
                            index.ToString (),
                            pa.res.path,
                            pa.meshCount.ToString (),
                            pa.subResCount.ToString ()),
                        GUILayout.MaxWidth (500));
                    index++;

                    EditorGUILayout.ObjectField ("", pa.asset, typeof (UnityEngine.Object), false, GUILayout.MaxWidth (300));
                    EditorGUILayout.EndHorizontal ();

                    EditorGUI.indentLevel++;
                    for (int i = 0; i < pa.meshCount; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();
                        int id = pa.meshIndex + i;
                        ref var mi = ref pc.meshInfo[id];
                        EditorGUILayout.LabelField(string.Format("Index:{0} {1}", id.ToString(), mi.res.path), GUILayout.MaxWidth(rect.width - 60));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField ("", mi.mesh, typeof (UnityEngine.Object), false, GUILayout.MaxWidth (300));
                        EditorGUILayout.EndHorizontal ();
                        if (mi.mp != null)
                        {
                            EditorGUILayout.BeginHorizontal ();
                            EditorGUILayout.ObjectField ("", mi.mp.GetSrcMat (), typeof (Material), false, GUILayout.MaxWidth (300));
                            string matType = "";
                            Material mat = mi.mp.GetMat(0);
                            //if (mi.mp is SceneMatProperty)
                            //{
                            //    matType = "Scene";
                            //    mat = WorldSystem.GetMat (mi.mp.matOffsetIndex, 0);
                            //}
                            //else
                            //{
                            //    matType = "Dynamic";
                            //    mat = WorldSystem.GetDynamicMat (mi.mp.matOffsetIndex, 0);
                            //}

                            EditorGUILayout.LabelField (string.Format ("MatType:{0} {1}", matType, mat != null?mat.name: "empty"));
                            EditorGUILayout.EndHorizontal ();
                        }
                    }
                    for (int i = 0; i < pa.subResCount; ++i)
                    {
                        EditorGUILayout.BeginHorizontal ();
                        ref var sr = ref pc.subRes[pa.subResIndex + i];
                        EditorGUILayout.LabelField (sr.resName, GUILayout.MaxWidth (400));
                        EditorGUILayout.ObjectField ("", sr.sfx, typeof (UnityEngine.Object), false, GUILayout.MaxWidth (300));
                        EditorGUILayout.EndHorizontal ();
                    }
                    EditorGUI.indentLevel--;
                    EditorCommon.EndGroup ();
                    EditorGUILayout.Space ();
                }
            }
            EditorCommon.EndScroll ();

            EditorGUILayout.LabelField ("ShowResRedirect", GUILayout.MaxWidth (100));
            showResRedirect = EditorGUILayout.Toggle ("", showResRedirect, GUILayout.MaxWidth (120));
            if (showResRedirect)
            {
                EditorCommon.BeginScroll (ref resScroll, resStatistic.Count, 10, 600, rect.width - 20);
                var itRes = resStatistic.GetEnumerator ();
                while (itRes.MoveNext ())
                {
                    var v = itRes.Current.Value;
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.LabelField (v.type, GUILayout.MaxWidth (100));
                    EditorGUILayout.EndHorizontal ();

                    EditorGUI.indentLevel++;
                    for (int i = 0; i < v.res.Count; ++i)
                    {
                        EditorGUILayout.BeginHorizontal ();
                        var res = v.res[i];
                        EditorGUILayout.LabelField ("Name:" + res.name, GUILayout.MaxWidth (300));
                        EditorGUILayout.LabelField ("Path:" + res.path, GUILayout.MaxWidth (500));
                        EditorGUILayout.EndHorizontal ();
                    }
                    EditorGUI.indentLevel--;
                }
                EditorCommon.EndScroll ();
            }

        }
        protected override void OnConfigUpdate ()
        {

            switch (opPrefabType)
            {
                case OpPrefabConfigType.OpRefresh:
                    RefreshPrefabFolder (config, context);
                    break;
                    // case OpPrefabConfigType.OpGenPrefabRes:
                    //     GenPrefabRes ();
                    //     break;
                case OpPrefabConfigType.OpCleanPrefab:
                    CleanAsset (config, context);
                    break;
                case OpPrefabConfigType.OpCheckValid:
                    CheckValid ();
                    break;
                    // case OpPrefabConfigType.OpCreateFbxConfig:
                    //     CreateFBXConfig ();
                    //     break;
                    // case OpPrefabConfigType.OpExport:
                    //     ExportMesh ();
                    //     break;
                    // case OpPrefabConfigType.OpExportConfig:
                    //     ExportConfig ();
                    //     break;
                case OpPrefabConfigType.OpSavePrefabData:
                    SavePrefabData (config);
                    break;

            }
            opPrefabType = OpPrefabConfigType.OpNone;
        }

        public static string GetPrefabPath ()
        {
            string prefabType = EditorPrefabData.prefabPath[(int) PrefabType.Prefab];
            return string.Format ("{0}/Runtime/{1}", AssetsConfig.instance.ResourcePath, prefabType);
        }

        private static void RefreshPrefab(PrefabPathConfig ppc, GameObject prefab, out GameObject target)
        {
            string prefabName = prefab.name.ToLower();
            string assetPath;
            GetBandposeDir(prefab, out assetPath, out var pr);
            string prefabDir = GetPrefabPath();
            var targetPath = string.Format("{0}/{1}.prefab", prefabDir, prefabName);
            ppc.targetPaths.Add(prefabName.ToLower());

            if (!File.Exists(targetPath) && prefabNamePathDic.ContainsKey(prefabName))
            {
                targetPath = string.Format("{0}/{1}.prefab", prefabDir, prefabNamePathDic[prefabName]);
            }

            target = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
        }

        private  static  Dictionary<string,string> prefabNamePathDic=new Dictionary<string, string>();

        private static void MakePrefabPathInfo()
        {
            string prefabDir = GetPrefabPath();
            prefabDir = Application.dataPath.Replace("/Assets", "/" + prefabDir);

            prefabNamePathDic.Clear();
            string[] files = Directory.GetFiles(prefabDir, "*.prefab", SearchOption.AllDirectories);
            foreach (var filePath in files)
            {
                string fileUse = filePath.Replace(@"\", "/").Replace(prefabDir + "/", string.Empty);

                fileUse = fileUse.Replace(".prefab", string.Empty);

                int nStartIndex = fileUse.LastIndexOf("/");
                string fileName = fileUse;
                if (nStartIndex >= 0)
                    fileName = fileUse.Substring(nStartIndex + 1);

                prefabNamePathDic.Add(fileName.ToLower(), fileUse.ToLower());
            }
        }

        private static void RefreshPrefabList (EditorPrefabData config, PrefabPathConfig ppc)
        {
            ppc.targetPaths.Clear ();
            for (int i = 0; i < ppc.configs.Count; ++i)
            {
                var pc = ppc.configs[i];
                if (pc.src != null)
                {
                    GameObject target = null;
                    RefreshPrefab (ppc, pc.src, out target);
                    pc.des = target;
                }
            }
        }

        private static void RefreshPrefabFolder (EditorPrefabData config, PrefabPathConfig ppc, PrefabConvertContext context)
        {
            CommonAssets.enumPrefab.cb = (prefab, path, param) =>
            {
                var c = param as PrefabConvertContext;
                string prefabName = prefab.name.ToLower ();
                bool ignore = false;
                for (int i = 0; i < c.config.ignoreName.Length; ++i)
                {
                    if (prefabName.Contains (c.config.ignoreName[i]))
                    {
                        ignore = true;
                        break;
                    }
                }
                if (!ignore)
                {
                    GameObject target = null;
                    RefreshPrefab (c.ppc, prefab, out target);
                    c.ppc.configs.Add (new PrefabRes ()
                    {
                        src = prefab,
                            des = target,
                    });

                }
            };
            context.config = config;
            context.ppc = ppc;
            ppc.Clear ();
            CommonAssets.EnumAsset<GameObject> (CommonAssets.enumPrefab, "FindPrfab", ppc.srcPath, context, false);
        }

        public static void RefreshPrefabFolder (EditorPrefabData config, PrefabConvertContext context, bool deep = true)
        {
            MakePrefabPathInfo();
            
            if (config != null)
            {
                for (int i = 0; i < config.prefabFolder.Count; ++i)
                {
                    var ppc = config.prefabFolder[i];
                    if (deep)
                    {
                        context.index = i;
                        RefreshPrefabFolder (config, ppc, context);
                    }
                    else
                        RefreshPrefabList (config, ppc);
                }
                for (int i = 0; i < config.prefabFolder.Count; ++i)
                {
                    var ppc = config.prefabFolder[i];
                    if (deep)
                    {
                        context.index = i;
                        CleanAsset (config, context);
                    }
                }
            }
        }

        private static string GetBandposeDir (GameObject prefab, out string assetPath,
            out CFEngine.PrefabRes pr)
        {
            assetPath = "";
            pr = null;
            if (prefab != null)
            {
                pr = LoadPrefabVersion (prefab.name);
                if (pr != null)
                {
                    // var bd = pr.prefabConfig as PrefabData;
                    // pr.configDir = AssetsPath.GetAssetDir (bd, out assetPath);
                    // if(pr.meshes!=null)
                    // {
                    //     for(int i=0;i<pr.meshes.Length;++i)
                    //     {
                    //         var m = pr.meshes[i];
                    //         if(m.mat!=null)
                    //         {
                    //             m.matPath = AssetDatabase.GetAssetPath(m.mat);
                    //         }
                    //     }
                    // }
                    // CommonAssets.SaveAsset(pr);
                    return pr.configDir;
                }
                else
                {
                    var renders = EditorCommon.GetRenderers (prefab);
                    for (int i = 0; i < renders.Count; ++i)
                    {
                        var r = renders[i];
                        MeshFilter mf;
                        if (r.gameObject.TryGetComponent (out mf))
                        {
                            var m = mf.sharedMesh;
                            if (m != null)
                                return AssetsPath.GetAssetDir (m, out assetPath);
                        }
                    }
                }
            }
            return "";
        }

        public static GameObject MakeRunTimePrefab (GameObject src, string targetPath)
        {
            var go = GameObject.Instantiate (src);
            go.name = src.name;
            Animator ator;
            if (go.TryGetComponent (out ator))
            {
                ator.avatar = null;
            }
            var renders = EditorCommon.GetRenderers (go);
            for (int j = 0; j < renders.Count; ++j)
            {
                var r = renders[j];
                //r.allowOcclusionWhenDynamic = false;
                //r.reflectionProbeUsage = ReflectionProbeUsage.Off;
                //r.lightProbeUsage = LightProbeUsage.Off;
                //r.shadowCastingMode = ShadowCastingMode.Off;
                r.enabled = false;
                
                /*if (r.sharedMaterial != null)
                {
                    var mats = r.sharedMaterials;
                    if (mats.Length > 1)
                    {
                        DebugLog.AddErrorLog2 ("multi materials:{0}", r.name);
                        r.sharedMaterials = new Material[1];
                    }
                    r.sharedMaterial = null;

                }
                else
                {
                    DebugLog.AddErrorLog2 ("null mat in prefab:{0}--{1}", go.name, r.name);
                }*/
                
                // if (r is SkinnedMeshRenderer)
                // {
                //     var smr = r as SkinnedMeshRenderer;
                //     var mesh = smr.sharedMesh;
                //     if (mesh != null)
                //     {
                //         smr.SetSharedMesh(null);
                //     }
                // }
                // else if (r is MeshRenderer)
                // {
                //     MeshFilter mf;
                //     if (r.TryGetComponent (out mf))
                //     {
                //         mf.SetSharedMesh(null);
                //     }
                // }
                // else
                // {
                //     DebugLog.AddErrorLog2 ("not support render type in prefab:{0}--{1},{2}", go.name, r.name, r.GetType ().Name);
                //     if (go != r.gameObject)
                //         GameObject.DestroyImmediate (r.gameObject);
                // }
            }
            string path = GetPrefabPath ();
            if (string.IsNullOrEmpty(targetPath))
            {

                MakePrefabPathInfo();
                string prefabName = src.name.ToLower();

                if (prefabNamePathDic.ContainsKey(prefabName))
                {
                    targetPath = string.Format("{0}/{1}.prefab", path, prefabNamePathDic[prefabName]);
                    Debug.Log("MakeRunTimePrefab has old :" + targetPath);
                }
                else
                {
                    targetPath = string.Format("{0}/{1}.prefab", path, go.name);
                }
            }

            if (File.Exists (targetPath))
            {
                AssetDatabase.DeleteAsset (targetPath);
            }
            var des = PrefabUtility.SaveAsPrefabAsset (go, targetPath);
            DebugLog.AddEngineLog2 ("Make RuntimePrefab:{0}", targetPath);
            GameObject.DestroyImmediate (go);
            return des;
        }

        private static void CleanAsset (EditorPrefabData config, PrefabConvertContext context)
        {
            CommonAssets.enumPrefab.cb = (prefab, path, param) =>
            {
                var c = param as PrefabConvertContext;
                var p = c.ppc;
                if (!p.targetPaths.Contains (prefab.name.ToLower ()))
                {
                    AssetDatabase.DeleteAsset (path);
                    DebugLog.AddEngineLog2 ("res destroyed:{0}", path);
                }
            };
            context.config = config;
            var ppc = config.prefabFolder[context.index];
            context.ppc = ppc;
            string dir = GetPrefabPath ();
            CommonAssets.EnumAsset<GameObject> (CommonAssets.enumPrefab, "CleanPrfab", dir, context, false);
        }

        private bool CompareTransform (Transform t0, Transform t1, string parentName, StringBuilder sb)
        {
            var pos0 = t0.position;
            var pos1 = t1.position;
            var rot0 = t0.rotation;
            var rot1 = t1.rotation;
            var scale0 = t0.localScale;
            var scale1 = t1.localScale;
            if (EngineUtility.CompareVector (ref pos0, ref pos1) &&
                EngineUtility.CompareQuation (ref rot0, ref rot1) &&
                EngineUtility.CompareVector (ref scale0, ref scale1))
            {
                return true;
            }
            else
            {
                sb.AppendFormat ("trans value not same,name :{0}{1} pos0:{2:F2} pos1:{3:F2} rot0:{4:F2} rot1:{5:F2} scale0:{6:F2} scale1:{7:F2}",
                    parentName,
                    t0.name,
                    pos0, pos1,
                    rot0, rot1,
                    scale0, scale1);
                return false;
            }
        }
        private int CompareSkinRender (Transform t0, Transform t1, string parentName, StringBuilder sb)
        {
            SkinnedMeshRenderer smr0;
            t0.TryGetComponent (out smr0);
            SkinnedMeshRenderer smr1;
            t1.TryGetComponent (out smr1);
            if (smr0 != null && smr1 != null)
            {
                var bone0 = smr0.bones;
                var bone1 = smr1.bones;
                bool same = true;
                if (bone0 != null && bone1 != null && bone0.Length == bone1.Length)
                {
                    for (int i = 0; i < bone0.Length; ++i)
                    {
                        var b0 = bone0[i];
                        var b1 = bone1[i];
                        if (b0.transform.name != b1.transform.name)
                        {
                            same = false;
                            break;
                        }
                    }
                }
                else
                {
                    same = false;
                }
                if (same)
                {
                    return 0;
                }
                sb.AppendFormat ("skin bone not same,name :{0}{1} ",
                    parentName,
                    t0.name);
                return 1;
            }
            else
            {
                if (smr0 == smr1)
                {
                    //null
                    return 2;
                }
                sb.AppendFormat ("skin render not same,name :{0}{1} smr0:{2} smr1:{2}",
                    parentName,
                    t0.name,
                    smr0 != null?smr0.name: "empty",
                    smr1 != null?smr1.name: "empty");
                return 1;
            }
        }
        private bool CompareMeshRender (Transform t0, Transform t1, string parentName, StringBuilder sb)
        {
            MeshRenderer mr0;
            t0.TryGetComponent (out mr0);
            MeshRenderer mr1;
            t1.TryGetComponent (out mr1);
            if (mr0 != null && mr0 != null)
            {
                return true;
            }
            if (mr0 == mr1)
                return true;
            sb.AppendFormat ("mesh render not same,name :{0}{1} smr0:{2} smr1:{2}",
                parentName,
                t0.name,
                mr0 != null?mr0.name: "empty",
                mr1 != null?mr1.name: "empty");
            return false;
        }
        private bool CompareComponent (Transform t0, Transform t1, string parentName, StringBuilder sb)
        {
            if (t0 != null && t1 != null)
            {

                if (t0.name.ToLower () == t1.name.ToLower () && t0.childCount == t1.childCount)
                {
                    var result = CompareSkinRender (t0, t1, parentName, sb);
                    if (result != 1)
                    {
                        if (result == 2)
                        {
                            return CompareMeshRender (t0, t1, parentName, sb);
                        }
                        else
                        {
                            for (int i = 0; i < t0.childCount; ++i)
                            {
                                var child0 = t0.GetChild (i);
                                var child1 = t1.GetChild (i);
                                var r = CompareComponent (child0, child1, parentName + t0.name + "/", sb);
                                if (!r)
                                {
                                    return false;
                                }
                            }
                            return true;
                        }

                    }
                    else
                    {
                        return false;
                    }
                }
            }
            sb.AppendFormat ("trans not same: src:{0}{1} des:{0}{2}",
                parentName,
                t0 != null?t0.name: "empty",
                t1 != null?t1.name: "empty");
            return false;
        }
        private void CheckValid ()
        {
            var pc = PrefabConfig.singleton;
            PrefabConfig.singleton.Init ();
            var ppc = config.prefabFolder[context.index];
            var sb = new StringBuilder ();
            for (int i = 0; i < ppc.configs.Count; ++i)
            {
                var c = ppc.configs[i];
                if (c.src != null && c.des != null)
                {
                    // ResBundle asset;
                    // uint hash = EngineUtility.XHashLowerRelpaceDot (0, c.src.name);
                    // if (pc.prefabs.TryGetValue (hash, out asset))
                    // {
                    //     sb.Clear ();
                    //     int result = 0;
                    //     string versionfilename = c.src.name.ToLower () + "_version";
                    //     PrefabVersion pv;
                    //     if (pc.prefabVersions.TryGetValue (versionfilename, out pv))
                    //     {
                    //         if (pv.uid != asset.uid)
                    //         {
                    //             sb.AppendLine (string.Format ("prefab {0} uid out of data,editor:{1} runtime:{2}", c.src.name, asset.uid, pv.uid));
                    //             result++;
                    //         }
                    //     }

                    //     var r = CompareComponent (c.src.transform, c.des.transform, c.src.name + "/", sb);
                    //     if (!r)
                    //         result++;
                    //     if (result > 0)
                    //     {
                    //         DebugLog.AddErrorLog (sb.ToString ());
                    //     }
                    // }
                    // else
                    // {
                    //     DebugLog.AddErrorLog2 ("not config prefab:{0}", c.src.name);
                    // }
                }
            }
        }

        class MeshMat
        {
            public string path = "";
            public float lodDist = 16;
            public int matIndex = -1;
            public string transPath = "";
            public bool isSkinMesh = true;
            public uint partMask = 0;
            public FlagMask flag;
        }

        public static void SavePrefabData (EditorPrefabData config)
        {
            //mat
            MatSaveData matSaveData = new MatSaveData ();
            var resAsset = new ResAssets ();
            matSaveData.resAsset = resAsset;

            List<MeshMat> meshes = new List<MeshMat> ();
            List<SubResInfo> subRes = new List<SubResInfo> ();
            Dictionary<int, uint> redirectMatHash = new Dictionary<int, uint>();
            // List<MeshMat> tmpMeshes = new List<MeshMat> ();
            // List<PrefabAsset> prefabs = new List<PrefabAsset> ();
            // List<SFXAsset> sfxs = new List<SFXAsset> ();

            List<CFEngine.PrefabRes> resList = new List<CFEngine.PrefabRes> ();
            for (int i = 0; i < config.prefabFolder.Count; ++i)
            {
                var ppc = config.prefabFolder[i];
                for (int j = 0; j < ppc.configs.Count; ++j)
                {
                    var c = ppc.configs[j];
                    if (c.src != null && c.des != null)
                    {
                        CFEngine.PrefabRes pr;
                        string assetPath;
                        string dir = GetBandposeDir(c.src, out assetPath, out pr);
                        if (pr != null && !resList.Contains(pr))
                        {
                            resList.Add(pr);
                        }
                    }
                    else
                    {
                        Debug.LogError("SavePrefabData: src: " + c.src + "   des:" + c.des);
                    }
                }

                if (ppc.configs.Count != resList.Count)
                {
                    Debug.LogError("prefab文件数:" + ppc.configs.Count + "   有效配置数:" + resList.Count);
                }
            }


            var sb1 = new System.Text.StringBuilder ();
            for (int i = 0; i < resList.Count; ++i)
            {
                var res = resList[i];
                sb1.AppendFormat ("prefab:{0}\r\n", res.name);
                var prefab = PrefabAsset.CreateInstance<PrefabAsset> ();
                res.asset = prefab;
                prefab.uid = res.uid;
                prefab.flag.SetFlag (PrefabAsset.Flag_FadeEffect, res.fadeEffect);
                prefab.meshIndex = meshes.Count;
                prefab.subResIndex = subRes.Count;
                prefab.res.path = res.name;
                resAsset.AddResName (res.name.ToLower ());
                int partCount = res.meshes != null?res.meshes.Length : 0;
                for (int j = 0; j < partCount; ++j)
                {
                    var part = res.meshes[j];
                    string assetName;
                    string dir = AssetsPath.GetDir (part.meshPath, out assetName);
                    assetName = assetName.ToLower ();
                    if (part.isSfx)
                    {
                        var sfx = new SubResInfo ();
                        sfx.resName = assetName;
                        resAsset.AddResName (assetName);
                        sfx.pos = part.pos;
                        sfx.rot = part.rot;
                        sfx.scale = part.scale;
                        subRes.Add (sfx);
                        prefab.subResCount++;
                    }
                    else
                    {
                        var meshMat = new MeshMat ();
                        var mat = string.IsNullOrEmpty (part.matPath) ? null : AssetDatabase.LoadAssetAtPath<Material> (part.matPath);
                        matSaveData.FindOrAddMatInfo (mat, out int matIndex,out uint matHash);

                        meshMat.path = assetName;
                        int resIndex = resAsset.AddResName (assetName);
                        resAsset.AddResReDirct (dir + "/", meshMat.path + ResObject.ResExt_Asset, ReDirectRes.LogicPath_Common);
                        sb1.AppendFormat ("\tmesh:{0}/{1} index {2} resindex {3}\r\n", dir, meshMat.path, meshes.Count, resIndex.ToString ());
                        meshMat.matIndex = matIndex;
                        meshMat.transPath = "";

                        resAsset.AddResName (meshMat.transPath);
                        meshMat.isSkinMesh = part.isSkin;

                        meshMat.partMask = part.partMask;
                        meshMat.flag.SetFlag (RendererInstance.Flag_PartActive, part.active);
                        bool isShadowPart = part.meshPath != null && Path.GetFileNameWithoutExtension(part.meshPath).Contains("_sd_");
                        meshMat.flag.SetFlag (RendererInstance.Flag_IsShadow, isShadowPart);
                        meshes.Add (meshMat);

                        prefab.meshCount++;
                    }
                }
            }

            try
            {
                DirectoryInfo di = new DirectoryInfo(string.Format("{0}/Config", 
                    LoadMgr.singleton.editorResPath));
                var files = di.GetFiles("*.asset", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; ++i)
                {
                    var file = files[i];
                    string configPath = file.FullName.Replace("\\", "/");
                    int index = configPath.IndexOf(LoadMgr.singleton.editorResPath);
                    configPath = configPath.Substring(index);
                    ResRedirectConfig rrc = AssetDatabase.LoadAssetAtPath<ResRedirectConfig>(configPath);
                    if (rrc != null)
                    {
                        for (int j = 0; j < rrc.resPath.Count; ++j)
                        {
                            var rp = rrc.resPath[j];
                            if (rp.ext == ResObject.ResExt_Mat)
                            {
                                string matPath = string.Format("{0}{1}.mat", rp.physicPath, rp.name);
                                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                                if (mat != null)
                                {
                                    matSaveData.FindOrAddMatInfo(mat, out int matIndex, out uint matHash);
                                    var matNameHash = EngineUtility.XHashLowerRelpaceDot(0, mat.name);
                                    if (!redirectMatHash.ContainsKey(matIndex))
                                        redirectMatHash.Add(matIndex, matNameHash);
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                DebugLog.AddErrorLog(e.StackTrace);
            }
            // File.WriteAllText("Assets/a.txt",sb1.ToString ());
            DebugLog.AddEngineLog (sb1.ToString ());

            string path = string.Format ("{0}Config/prefabconfig.bytes",
                LoadMgr.singleton.BundlePath);
            using (FileStream fs = new FileStream (path, FileMode.Create))
            {
                var sb = new System.Text.StringBuilder ();
                BinaryWriter bw = new BinaryWriter (fs);
                resAsset.SaveHeadString (bw);
                //mat
                matSaveData.SaveMaterial (bw, true);

                //redirect mat
                int redirectMatCount = redirectMatHash.Count;
                bw.Write(redirectMatCount);
                var matIt = redirectMatHash.GetEnumerator();
                while(matIt.MoveNext())
                {
                    var kvp = matIt.Current;
                    bw.Write(kvp.Key);
                    bw.Write(kvp.Value);
                }
                //mesh
                ushort meshCount = (ushort) meshes.Count;
                bw.Write (meshCount);
                for (int i = 0; i < meshCount; ++i)
                {
                    var mm = meshes[i];
                    bw.Write ((ushort) mm.matIndex);

                    int nameIndex = resAsset.SaveStringIndex (bw, mm.path);
                    sb.AppendFormat ("\tmesh:{0} index {1} resIndex {2}\r\n", mm.path, i.ToString (), nameIndex.ToString ());
                    bw.Write (mm.isSkinMesh);
                    bw.Write (mm.lodDist);
                    resAsset.SaveStringIndex (bw, mm.transPath);
                    bw.Write (mm.partMask);
                    bw.Write (mm.flag.flag);
                }
                DebugLog.AddEngineLog (sb.ToString ());
                //sfx
                ushort sfxCount = (ushort) subRes.Count;
                bw.Write (sfxCount);
                for (int i = 0; i < sfxCount; ++i)
                {
                    var sfx = subRes[i];
                    resAsset.SaveStringIndex (bw, sfx.resName);
                    EditorCommon.WriteVector (bw, sfx.pos);
                    EditorCommon.WriteQuaternion (bw, sfx.rot);
                    EditorCommon.WriteVector (bw, sfx.scale);
                }

                //prefab
                ushort prefabCount = (ushort) resList.Count;
                bw.Write (prefabCount);
                for (int i = 0; i < prefabCount; ++i)
                {
                    var prefab = resList[i].asset;
                    bw.Write(prefab.uid);
                    bw.Write(prefab.flag.flag);
                    bw.Write((ushort) prefab.meshIndex);
                    bw.Write((byte) prefab.meshCount);
                    bw.Write((ushort) prefab.subResIndex);
                    bw.Write((byte) prefab.subResCount);
                    resAsset.SaveStringIndex(bw, prefab.res.path.ToLower());
                    string filePath = prefabNamePathDic[prefab.res.path.ToLower()];
                    prefab.res.path = filePath;
                    bw.Write(filePath);
                }


                Dictionary<uint, int> meshInfoIndex = new Dictionary<uint, int>();
                var pc = PrefabConfig.singleton;
                var prefabDiy = LoadMgr.singleton.LoadAssetImmediate<PrefabDIY>("EditorAssetRes/Config/PrefabDIY", ".prefab");
                if (prefabDiy != null)
                {                   
                    for (int i = 0; i < prefabDiy.prefabs.Count; ++i)
                    {
                        var prefab = prefabDiy.prefabs[i];
                        if (prefab.go != null)
                        {
                            string name = prefab.go.name.ToLower();
                            var res = resList.Find((x) => x.name == name);
                            if (res != null&& res.asset!=null)
                            {
                                int end = res.asset.meshIndex + res.asset.meshCount;
                                for (int j = res.asset.meshIndex; j < end; ++j)
                                {
                                    var mi = meshes[j];
                                    uint h = EngineUtility.XHashLowerRelpaceDot(0, mi.path);
                                    if (!meshInfoIndex.TryGetValue(h, out var index))
                                    {
                                        meshInfoIndex[h] = j;
                                    }
                                }
                            }
                            pc.AddParts(prefab.go.name);
                        }
                    }
                }
                LoadMgr.singleton.DestroyImmediate();
                bw.Write(meshInfoIndex.Count);
                var it = meshInfoIndex.GetEnumerator();
                while (it.MoveNext())
                {
                    bw.Write(it.Current.Key);
                    bw.Write(it.Current.Value);
                }
            }
            AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate);
            path = string.Format ("{0}Config/PrefabConfig.resbytes", LoadMgr.singleton.editorResPath);
            using (FileStream fs = new FileStream (path, FileMode.Create))
            {
                BinaryWriter bw = new BinaryWriter (fs);
                int count = resAsset.editorResReDirect.Count;
                bw.Write (count);
                var it = resAsset.editorResReDirect.GetEnumerator ();
                // var sb = new System.Text.StringBuilder ();
                while (it.MoveNext ())
                {
                    var kvp = it.Current;
                    bw.Write (kvp.Key);
                    var redirectRes = kvp.Value;
                    bw.Write (redirectRes.physicDir);
                    bw.Write (redirectRes.logicPathType);
                    // sb.AppendLine (string.Format ("res:{0} path:{1}", kvp.Key, redirectRes.physicDir));
                }

                var materialGroupCount = matSaveData.matInfo.Count;
                bw.Write (materialGroupCount);
                for (int j = 0; j < materialGroupCount; ++j)
                {
                    var mi = matSaveData.matInfo[j];
                    var context = mi.context;
                    if (mi.mat != null)
                    {
                        string matPath = AssetDatabase.GetAssetPath (mi.mat);
                        bw.Write (matPath);
                    }
                    else
                    {
                        bw.Write ("");
                    }
                }
                // DebugLog.AddEngineLog (sb.ToString ());
            }
            AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate);
            CFEngine.PrefabConfig.singleton.init = false;
            AssetDatabase.Refresh ();
        }

        public static CFEngine.PrefabRes SaveEditorPrefabRes (string name, ScriptableObject config, List<EditorMeshInfo> eMeshInfo, bool fadeEffect = false)
        {
            string versionPath = string.Format ("{0}Prefab/{1}.asset",
                LoadMgr.singleton.editorResPath, name.ToLower ());
            var pr = AssetDatabase.LoadAssetAtPath<CFEngine.PrefabRes> (versionPath);
            if (pr == null)
            {
                pr = ScriptableObject.CreateInstance<CFEngine.PrefabRes> ();
                pr = EditorCommon.CreateAsset<CFEngine.PrefabRes> (versionPath, ".asset", pr);
            }
            pr.uid++;
            // pr.prefabConfig = config;
            pr.meshes = eMeshInfo.ToArray ();
            pr.fadeEffect = fadeEffect;
            EditorCommon.SaveAsset (pr);
            return pr;
        }

        public static void SavePrefabVersion (string name, ScriptableObject config)
        {
            string versionPath = string.Format ("{0}Prefab/{1}_version.asset",
                LoadMgr.singleton.editorResPath, name.ToLower ());
            PrefabVersion pv = AssetDatabase.LoadAssetAtPath<PrefabVersion> (versionPath);
            if (pv == null)
            {
                pv = ScriptableObject.CreateInstance<PrefabVersion> ();
                pv = EditorCommon.CreateAsset<PrefabVersion> (versionPath, ".asset", pv);
            }
            pv.uid++;
            pv.prefabConfig = config;
            EditorCommon.SaveAsset (pv);
        }
        public static CFEngine.PrefabRes LoadPrefabVersion (string name)
        {
            string versionPath = string.Format ("{0}Prefab/{1}.asset",
                LoadMgr.singleton.editorResPath, name.ToLower ());
            var pr = AssetDatabase.LoadAssetAtPath<CFEngine.PrefabRes> (versionPath);
            if (pr == null)
            {
                DebugLog.AddErrorLog2 ("{0} not exist.", versionPath);
            }
            return pr;
        }
    }
}
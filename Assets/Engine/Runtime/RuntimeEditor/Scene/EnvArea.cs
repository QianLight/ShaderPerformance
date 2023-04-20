#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    [System.Serializable]
    public class EnvObject
    {
        public string envName;
        
        public int areaID = -1;
        public Color color = Color.red;
        public Vector3 pos = Vector3.zero;
        public FlagMask flag;
        public EnvAreaProfile profile;
        // [NonSerialized]
        // public EnvBlock envBlock = new EnvBlock ();
        // [NonSerialized]
        public List<LightingInfo> lights = new List<LightingInfo>();
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class EnvArea : MonoBehaviour, IEnvContainer
    {
        [NonSerialized]
        public float blinkTime = 0.1f;
        [Range (-1, 31)]
        public int areaID = -1;
        public Color color = Color.red;
        public bool isActive = true;
        public EnvAreaProfile profile;
        
        public List<Transform> volumns = new List<Transform>();
        public List<Transform> instances = new List<Transform>();
        public List<Transform> effects = new List<Transform>();
        public List<Transform> multiLayers = new List<Transform>();
        public bool dirtyCameraFov;
        public Transform lookAtTarget;
        public bool manualTrigger;
        private bool init = false;
        private Transform trans = null;

        private static readonly HashSet<(string, string)> rolePaths = new HashSet<(string, string)>()
        {
            (nameof(Lighting), nameof(Lighting.roleLightColorV2)),
            (nameof(Ambient), nameof(Ambient.roleSHV2)),
            (nameof(Shadow), nameof(Shadow.shadowMisc)),
            (nameof(Shadow), nameof(Shadow.roleShadowColor)),
        };

        public void CreatLoadProfile (string scenePath)
        {
            string path = string.Format ("{0}/Config/{1}.asset", scenePath, this.name);
            if (File.Exists (path))
            {
                profile = AssetDatabase.LoadAssetAtPath<EnvAreaProfile> (path);
            }
            else
            {
                EnvAreaProfile eap = EnvAreaProfile.CreateInstance<EnvAreaProfile> ();
                profile = EditorCommon.CreateAsset<EnvAreaProfile> (path, ".asset", eap);
            }
        }
        public Transform GetTransform()
        {
            if (trans == null)
            {
                trans = this.transform;
            }
            return trans;
        }

        private static void BindEnvParam (EnvBlock envBlock, EnvParam envParam, RuntimeParamOverride runtimeParam, Transform owner)
        {
            envParam.runtimeParam = runtimeParam;
            envParam.owner = owner;
            var runtime = runtimeParam != null ? runtimeParam.runtime : null;
            if (runtime != null)
            {
                runtime.Create(envBlock, envParam);
                if (envParam.param != null)
                    envParam.Name = runtime.name;
                ClassSerializedParameterOverride spo = envParam.editorData as ClassSerializedParameterOverride;
                if (spo == null && runtimeParam.runtimeSp != null)
                {
                    spo = new ClassSerializedParameterOverride (
                        runtimeParam.runtimeSp,
                        runtimeParam.attributes,
                        runtime);
                    SerializedParameterOverride.InitProperty (spo);
                    envParam.editorData = spo;
                }
                if (spo != null)
                {
                    AttributeDecorator decorator = spo.decorator;
                    Attribute attribute = spo.decoratorAttr;
                    decorator.SetInfo (runtimeParam.envName, spo.displayName, attribute, envParam);
                }
            }
        }

        public static bool BindEnvParam (EnvBlock envBlock, EnvParam envParam, Transform owner)
        {
            if (EnvSetting.paramIndex.TryGetValue (envParam.hash, out var runtimeParam))
            {
                BindEnvParam (envBlock, envParam, runtimeParam, owner);
                return true;
            }
            return false;
        }

        public static EnvParam CreateEnvParam (EnvBlock envBlock, Transform owner, bool hasAnim, int index)
        {
            EnvParam envParam = new EnvParam ();
            var runtimeParam = EnvProfile.GetEnvRuntimeParam (index);
            envParam.hash = runtimeParam.hash;
            envParam.effectType = runtimeParam.envType;
            envParam.hasAnim = hasAnim;
            BindEnvParam (envBlock, envParam, runtimeParam, owner);
            envBlock.envParams.Add (envParam);
            var epg = RefreshEnvGroup (envBlock, envParam);
            epg.envParams.Sort ((x, y) => x.runtimeParam.index.CompareTo (y.runtimeParam.index));
            return envParam;
        }

        public static EnvParamGroup RefreshEnvGroup (EnvBlock envBlock, EnvParam envParam)
        {
            var epg = envBlock.envParamGroups[envParam.effectType];
            if (epg == null)
            {
                epg = new EnvParamGroup ();
                envBlock.envParamGroups[envParam.effectType] = epg;
            }
            epg.effectName = envParam.runtimeParam.envName;
            epg.envParams.Add (envParam);
            return epg;
        }

        public static void BindEnvBlock (EnvBlock envBlock, Transform owner, bool hasAnim = false)
        {
            for (int i = 0; i < envBlock.envParamGroups.Length; ++i)
            {
                var epg = envBlock.envParamGroups[i];
                if (epg != null)
                {
                    epg.envParams.Clear ();
                }
            }
            var mgr = RenderingManager.instance;
            for (int i = 0; i < envBlock.envParams.Count; ++i)
            {
                var envParam = envBlock.envParams[i];
                if (BindEnvParam (envBlock, envParam, owner))
                {
                    envParam.hasAnim = hasAnim;
                    RefreshEnvGroup (envBlock, envParam);
                }
            }
            for (int i = 0; i < envBlock.envParamGroups.Length; ++i)
            {
                var epg = envBlock.envParamGroups[i];
                if (epg != null)
                {
                    epg.envParams.Sort ((x, y) => x.runtimeParam.index.CompareTo (y.runtimeParam.index));
                }
            }
        }

        public static void OnEnvBlockGUI (EnvAreaProfile profile, EnvBlock envBlock,
            IEnvContainer envContainer, bool lerpTime, bool hasAnim, EnvOpContext opContext)
        {
            var width = EditorGUIUtility.currentViewWidth;
            bool addConfig = false;
            bool clear = false;
            Transform owner = envContainer != null ? envContainer.GetTransform () : null;
            EditorGUILayout.BeginHorizontal ();
            var envParamStr = EnvProfile.GetEnvParamStr ();
            if (envParamStr != null)
                envBlock.selectEnvIndex = EditorGUILayout.Popup ("", envBlock.selectEnvIndex, envParamStr, GUILayout.MaxWidth (200));
            if (GUILayout.Button ("AddConfig", GUILayout.MaxWidth (80)))
            {
                if (!envBlock.HasEnvParam (EnvProfile.GetEnvRuntimeParam (envBlock.selectEnvIndex)))
                {
                    addConfig = true;
                }
            }
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Clear", GUILayout.MaxWidth (80)))
            {
                clear = true;
            }
            if (GUILayout.Button (envBlock.isDelete? "Deleting": "Delete", GUILayout.MaxWidth (80)))
            {
                envBlock.isDelete = !envBlock.isDelete;
            }
            if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (80)))
            {
                BindEnvBlock (envBlock, owner, hasAnim);
            }
            EditorGUILayout.EndHorizontal ();
            if (lerpTime)
            {
                EditorGUILayout.BeginHorizontal ();
                envBlock.lerpTime = EditorGUILayout.Slider ("LerpTime", envBlock.lerpTime, -0.1f, 2);
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                envBlock.lerpOutTime = EditorGUILayout.Slider ("LerpOutTime", envBlock.lerpOutTime, -0.1f, 2);
                EditorGUILayout.EndHorizontal ();
            }

            EditorGUI.indentLevel++;
            EnvParam deleteParam = null;
            int deleteParamIndex = -1;

            for (int i = 0; i < envBlock.envParamGroups.Length; ++i)
            {
                var epg = envBlock.envParamGroups[i];
                if (epg != null && epg.envParams.Count > 0)
                {
                    if (EditorCommon.BeginFolderGroup (epg.effectName, ref epg.folder, width, 50 * epg.envParams.Count))
                    {
                        for (int j = 0; j < epg.envParams.Count; ++j)
                        {
                            var envParam = epg.envParams[j];
                            EditorGUILayout.BeginHorizontal ();
                            var cspo = envParam.editorData as ClassSerializedParameterOverride;
                            if (cspo != null)
                            {
                                if (envBlock.isDelete)
                                {
                                    if (GUILayout.Button ("D", GUILayout.MaxWidth (20)))
                                    {
                                        deleteParam = envParam;
                                        deleteParamIndex = j;
                                    }
                                }

                                DrawProperty(envBlock, opContext, envParam);
                            }
                            EditorGUILayout.EndHorizontal ();
                        }
                        EditorCommon.EndFolderGroup ();
                    }
                }
            }
            if (deleteParam != null)
            {
                for (int i = 0; i < envBlock.envParams.Count; ++i)
                {
                    var param = envBlock.envParams[i];
                    if (param == deleteParam)
                    {
                        envBlock.envParams.RemoveAt (i);
                        var epg = envBlock.envParamGroups[param.effectType];
                        epg.envParams.RemoveAt (deleteParamIndex);
                        if (opContext != null && opContext.onRemove != null)
                        {
                            opContext.onRemove (param);
                        }
                        break;
                    }

                }
            }

            #region Role lighting params
            GUILayout.BeginVertical("box");
            GUILayout.Box("角色光照参数集合", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Add All"))
            {
                foreach (var item in rolePaths)
                {
                    string envStr = $"{item.Item1}/{item.Item2}";
                    int index = Array.IndexOf(envParamStr, envStr);
                    if (index >= 0 && !envBlock.HasEnvParam(EnvProfile.GetEnvRuntimeParam(index)))
                    {
                        EnvParam envParam = CreateEnvParam(envBlock, owner, hasAnim, index);
                        if (opContext != null && opContext.onAdd != null)
                        {
                            opContext.onAdd(envParam);
                        }
                    }
                }
            }
            for (int i = 0; i < envBlock.envParamGroups.Length; ++i)
            {
                var epg = envBlock.envParamGroups[i];
                if (epg != null && epg.envParams.Count > 0)
                {
                    for (int j = 0; j < epg.envParams.Count; ++j)
                    {
                        var envParam = epg.envParams[j];
                        if (rolePaths.Contains((epg.effectName, envParam.Name)))
                        {
                            DrawProperty(envBlock, opContext, envParam);
                        }
                    }
                }
            }
            GUILayout.EndVertical();
            #endregion

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("LightsCount:{0}", profile.lights.Count.ToString()));
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            if (addConfig)
            {
                EnvParam envParam = CreateEnvParam (envBlock, owner, hasAnim, envBlock.selectEnvIndex);
                if (opContext != null && opContext.onAdd != null)
                {
                    opContext.onAdd (envParam);
                }
            }
            if (clear)
            {
                envBlock.envParams.Clear ();
                for (int i = 0; i < envBlock.envParamGroups.Length; ++i)
                {
                    var epg = envBlock.envParamGroups[i];
                    if (epg != null)
                    {

                        epg.envParams.Clear ();

                    }
                }
                if (opContext != null && opContext.onClear != null)
                {
                    opContext.onClear ();
                }
            }
        }

        private static void DrawProperty(EnvBlock envBlock, EnvOpContext opContext, EnvParam envParam)
        {
            var cspo = envParam.editorData as ClassSerializedParameterOverride;
            var decorator = cspo.decorator;
            if (decorator == null)
                return;

            var attribute = cspo.decoratorAttr;
            var title = RuntimeUtilities.GetContent (cspo.displayName);
            byte animMask = envParam.animMask;
            envParam.valueChangeMask = 0;
            if (decorator.OnGUI(cspo, title, attribute, envParam, 0, out var toggleChange))
            {
                if (toggleChange)
                {
                    var runtime = envParam.runtimeParam != null ? envParam.runtimeParam.runtime : null;
                    if (runtime != null)
                        runtime.Create(envBlock, envParam);
                }
                //else
                //{
                //    if (envParam.param != null)
                //        envParam.param.Save2Profile(envParam);
                //}

                if (opContext != null && opContext.onValueChange != null)
                {
                    opContext.onValueChange(envParam);
                }

                var context = EngineContext.instance;
                if (EngineContext.IsRunning && context != null)
                {
                    var env = context.envModifys[envParam.effectType];
                    if (env != null && env.modify != null)
                    {
                        env.modify.DirtySetting();
                    }
                }
            }
            if (opContext != null && opContext.onAnimMaskChange != null)
            {
                if (envParam.animMask != animMask)
                {
                    opContext.onAnimMaskChange(envParam);
                }
            }
        }

        private void InnerUpdate (EngineContext context)
        {
            if (profile != null && isActive)
            {
                if (!init)
                {
                    var mgr = RenderingManager.instance;
                    for (int i = 0; i < profile.envBlock.envParams.Count; ++i)
                    {
                        var envParam = profile.envBlock.envParams[i];
                        BindEnvParam(profile.envBlock, envParam, this.transform);
                    }
                    init = true;
                }
                profile.envBlock.flag.SetFlag(EnvBlock.Flag_DirtyCameraFov, dirtyCameraFov);
                profile.envBlock.flag.SetFlag(EnvBlock.Flag_ManualTrigger, manualTrigger);
                if (lookAtTarget != null)
                {
                    profile.envBlock.flag.SetFlag(EnvBlock.Flag_AbsShadowPos, true);
                    profile.envBlock.shadowPos = lookAtTarget.position;
                }
                else
                {
                    profile.envBlock.flag.SetFlag(EnvBlock.Flag_AbsShadowPos, false);
                }

                Vector4 box = Vector4.zero;
                Vector4 size = Vector2.zero;
                int boxDataLength = profile.areaList.Count * 8 + 1;
                profile.envBlock.data.Create (boxDataLength, "envBlock.data");
                profile.envBlock.data.Set (0, (float) profile.areaList.Count * 8);
                Transform t = this.transform;
                for (int i = 0; i < profile.areaList.Count; ++i)
                {
                    var envBox = profile.areaList[i];
                    Vector3 worldPos = envBox.center + t.position;
                    float halfY = envBox.size.y * 0.5f;
                    float yMin = worldPos.y - halfY;
                    float yMax = worldPos.y + halfY;
                    float cosA = Mathf.Cos (envBox.rotY * Mathf.Deg2Rad);
                    float sinA = Mathf.Sin (envBox.rotY * Mathf.Deg2Rad);

                    profile.envBlock.data.Set (i * 8 + 1, worldPos.x);
                    profile.envBlock.data.Set (i * 8 + 2, worldPos.z);
                    profile.envBlock.data.Set (i * 8 + 3, cosA);
                    profile.envBlock.data.Set (i * 8 + 4, sinA);
                    profile.envBlock.data.Set (i * 8 + 5, envBox.size.x * 0.5f);
                    profile.envBlock.data.Set (i * 8 + 6, envBox.size.z * 0.5f);
                    profile.envBlock.data.Set (i * 8 + 7, yMin);
                    profile.envBlock.data.Set (i * 8 + 8, yMax);
                }
                if (SceneRoamingSystem.TestEnvArea(profile.envBlock, ref context.cameraPos, ref size, ref box))
                {
                    EnvGroup.currentArea = this;
                    GenEnvParamBlock(profile.envBlock);
                    profile.OnUpdate(this);
                }    
            }
        }

        public static void GenEnvParamBlock (EnvBlock envBlock)
        {
            envBlock.root = null;
            EnvParam last = null;
            for (int i = 0; i < envBlock.envParams.Count; ++i)
            {
                var envParam = envBlock.envParams[i];
                if (last == null)
                {
                    envBlock.root = envParam;
                }
                else
                {
                    last.next = envParam;
                }
                last = envParam;
            }
            if (last != null)
                last.next = null;
        }

        public static void OnUpdate (EngineContext context)
        {
            string path = string.Format ("{0}/{1}",
                AssetsConfig.EditorGoPath[0],
                AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.Enverinment]);
            GameObject go = GameObject.Find (path);
            if (go != null)
            {
                EnvGroup eg = go.GetComponent<EnvGroup> ();
                if (eg == null)
                {
                    eg = go.AddComponent<EnvGroup> ();
                    eg.CollectAreas ();
                }
                EnvGroup.currentArea = null;
                for (int i = 0; i < eg.areas.Count; ++i)
                {
                    var area = eg.areas[i];
                    if (area != null)
                        area.InnerUpdate(context);
                }
                if (!context.logicflag.HasFlag (EngineContext.Flag_DisableEnvTest))
                {
                    if (EnvGroup.currentArea != null && EnvGroup.currentArea.profile != null)
                    {
                        context.currentEnvBlock = EnvGroup.currentArea.profile.envBlock;
                        LightingModify.envSceneLights = EnvGroup.currentArea.profile;
                    }                       
                    else
                    {
                        context.currentEnvBlock = null;
                        LightingModify.envSceneLights = null;
                    }
                        
                    SceneRoamingSystem.PostUpdateEnvTrigger (context);
                    EnvGroup.lastArea = EnvGroup.currentArea;
                }

            }
        }

        private void InnerDrawGizmos ()
        {
            if (profile != null)
            {
                if (EnvGroup.currentArea == this)
                {
                    for (int i = 0; i < profile.areaList.Count; ++i)
                    {
                        var areaBox = profile.areaList[i];

                        Color boxColor = Color.Lerp (new Color (1, 1, 1, 0.2f), Color.white, blinkTime);
                        blinkTime += Time.deltaTime;
                        if (blinkTime > 1.0f)
                        {
                            blinkTime = 0;
                        }
                        Gizmos.color = boxColor;

                        Vector3 worldPos = areaBox.center + transform.position;
                        Quaternion rot = Quaternion.Euler (0, areaBox.rotY, 0);
                        Gizmos.matrix = Matrix4x4.TRS (worldPos, rot, Vector3.one);
                        Gizmos.DrawWireCube (Vector3.zero, areaBox.size);
                        Gizmos.matrix = Matrix4x4.identity;
                    }
                }
                else
                {

                    for (int i = 0; i < profile.areaList.Count; ++i)
                    {
                        var areaBox = profile.areaList[i];
                        Gizmos.color = color;
                        Vector3 worldPos = areaBox.center + transform.position;
                        Quaternion rot = Quaternion.Euler (0, areaBox.rotY, 0);
                        Gizmos.matrix = Matrix4x4.TRS (worldPos, rot, Vector3.one);
                        Gizmos.DrawWireCube (Vector3.zero, areaBox.size);
                        Gizmos.matrix = Matrix4x4.identity;
                    }
                }

                //

            }

        }

        void OnDrawGizmosSelected ()
        {
            if (profile != null)
            {
                for (int i = 0; i < profile.envBlock.envParams.Count; ++i)
                {
                    var ep = profile.envBlock.envParams[i];
                    if (ep.param != null)
                    {
                        ep.param.OnGizmo (ep);
                    }
                }
            }
        }
        public static void DrawGizmos ()
        {
            string path = string.Format ("{0}/{1}",
                AssetsConfig.EditorGoPath[0],
                AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.Enverinment]);
            GameObject go = GameObject.Find (path);
            if (go != null)
            {
                EnvGroup eg = go.GetComponent<EnvGroup> ();
                if (eg != null)
                {
                    for (int i = 0; i < eg.areas.Count; ++i)
                    {
                        var area = eg.areas[i];
                        if (area)
                        {
                            area.InnerDrawGizmos ();
                        }
                    }
                }
            }
        }
        public void OnSave (IEnvContainer envContainer)
        {
            if (profile != null)
            {
                profile.OnSave (envContainer);
            }

        }

        static void InitEnvAreaDebug(SceneEnvDebug sed)
        {
            sed.profile = EnvAreaProfile.Clone(sed.envBlock, sed.name);
            BindEnvBlock(sed.envBlock, sed.transform);
            if(sed.envBlock.flag.HasFlag(EnvBlock.Flag_AbsShadowPos))
            {
                var tran = new GameObject("LookAtPos").transform;
                tran.position = sed.envBlock.shadowPos;
                tran.parent = sed.transform;
                sed.lookAtPos = tran;
            }
            for (int i = 0; i < sed.envBlock.lightInfo.Length; ++i)
            {
                var li = sed.envBlock.lightInfo.Get(i);
                var tran = new GameObject(string.Format("PointLight_{0}", i.ToString())).transform;
                tran.position = li.posCoverage;
                tran.parent = sed.transform;
                var eal = tran.gameObject.AddComponent<EnvAreaLight>();
                eal.specLerp = li.posCoverage.w;
                eal.range = Mathf.Sqrt(1 / li.color.w);
                eal.color = li.color;
                eal.color.a = 1;
            }
        }

        public static void InitDebug()
        {
            SceneEnvDebug.envAreaDebug = InitEnvAreaDebug;
        }

        public static void SaveProfileOnRuntime(EnvAreaProfile profile,EnvAreaProfile runtimeProfile)
        {
            if (profile != null)
            {
                var paramMap = new Dictionary<uint, EnvParam>();
                foreach (var ep in profile.envBlock.envParams)
                {
                    paramMap[ep.hash] = ep;
                }
                var envBlock = runtimeProfile.envBlock;
                var envParam = envBlock.root;
                while (envParam != null)
                {
                    if (envParam.param != null &&
                        paramMap.TryGetValue(envParam.hash, out var src))
                    {
                        if (src.param != null)
                        {
                            src.param.SetValue(envParam.param, true);
                        }

                    }
                    envParam = envParam.next as EnvParam;
                }
                EditorCommon.SaveAsset(profile);
            }
        }
    }
}
#endif
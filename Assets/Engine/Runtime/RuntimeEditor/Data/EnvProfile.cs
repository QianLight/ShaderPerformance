#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CFEngine
{
    public interface IEditorSetting { }

    [Serializable]
    public sealed class TransParam : ParamOverride<string, TransParam>
    {
        [NonSerialized]
        public Transform t;
        public void OnEnable ()
        {
            if (!string.IsNullOrEmpty (value))
            {
                GameObject go = GameObject.Find (value);
                t = go != null?go.transform : null;

            }
        }
        public override void SetValue (ParamOverride parameter, bool shallowCopy)
        {
            TransParam param = parameter as TransParam;
            value = param.value;
            t = param.t;
        }
    }

    [Serializable]
    public sealed class EnvProfile : ScriptableObject
    {
        public List<EnvSetting> settings = new List<EnvSetting> ();
        [NonSerialized]
        public ListObjectWrapper<ISceneObject> paramObjects;
        public static ListObjectWrapper<ISceneObject> emptyParamObjects;
        // Editor only, doesn't have any use outside of it
        [NonSerialized]
        public bool isDirty = true;
        public static EnvProfile activeProfile;
        private static string[] envParamStr;
        private static List<RuntimeParamOverride> runtimeEnvParam = new List<RuntimeParamOverride> ();

        public static int ParamType_Light = EnvParam.ParamType_Engine + 1;
        public static int ParamType_Ambinet = ParamType_Light + 1;
        public static int ParamType_DynamicLight = ParamType_Ambinet + 1;
        public bool HasSettings (Type type)
        {
            for (int i = 0; i < settings.Count; ++i)
            {
                var setting = settings[i];
                if (setting.GetType () == type)
                    return true;
            }

            return false;
        }

        public List<EnvSetting> GetProfileSettings ()
        {
            List<EnvSetting> s = new List<EnvSetting> ();
            for (int i = 0; i < settings.Count; ++i)
            {
                var setting = settings[i];
                if (s is IEditorSetting)
                {
                    continue;
                }
                s.Add (setting);
            }
            return s;
        }

        public T Get<T>()where T : EnvSetting
        {
            foreach (EnvSetting setting in settings)
            {
                if (setting is T)
                {
                    return setting as T;
                }
            }
            return null;
        }
            
        public void Refresh ()
        {
            ListObjectWrapper<ISceneObject>.Get (ref paramObjects);
            paramObjects.Clear ();
            EnvSetting.forceOverride = true;
            for (int i = 0; i < settings.Count; ++i)
            {
                var setting = settings[i];
                if (setting != null)
                {
                    setting.envProfile = this;
                    setting.InitParamaters (paramObjects, null);
                    setting.InitEditorParamaters (paramObjects, null, true);
                    setting.EndInitParamaters (paramObjects);
                }
            }
            EnvSetting.forceOverride = false;
        }

        public static EnvProfile CreateOrLoad (GameObject go, string key = "")
        {
            var scene = go.scene;
            if (!string.IsNullOrEmpty (scene.path))
            {
                var scenePath = Path.GetDirectoryName (scene.path);
                var assetName = string.IsNullOrEmpty (key) ? scene.name + "_Profiles.asset" : key + ".asset";
                var profileDir = scenePath + "/Config";
                if (!AssetDatabase.IsValidFolder (profileDir))
                    AssetDatabase.CreateFolder (scenePath, "Config");
                string profilePath = string.Format ("{0}/{1}", profileDir, assetName);
                if (!File.Exists (profilePath))
                {
                    var profile = ScriptableObject.CreateInstance<EnvProfile> ();
                    // profile.key = key;
                    AssetDatabase.CreateAsset (profile, profilePath);
                    AssetDatabase.SaveAssets ();
                    AssetDatabase.Refresh ();
                    return profile;
                }
                else
                {
                    return AssetDatabase.LoadAssetAtPath<EnvProfile> (profilePath);
                }
            }
            EditorUtility.DisplayDialog ("Empty Scene", "Save Scene First!", "OK");
            return null;
        }
        public static RuntimeParamOverride GetEnvRuntimeParam (int index)
        {
            return runtimeEnvParam[index];
        }
        public static string[] GetEnvParamStr ()
        {
            var context = EngineContext.instance;
            if (envParamStr == null && context != null)
            {
                List<string> paramPath = new List<string> ();
                runtimeEnvParam.Clear ();
                var envModifys = context.envModifys;
                if (envModifys != null)
                {
                    for (int i = 0; i < envModifys.Length; ++i)
                    {
                        var modify = envModifys[i];
                        if (modify != null)
                        {
                            var settings = modify.runtime;
                            if (settings.GetType().IsSubclassOf(typeof(EnvSetting)))
                            {
                                paramPath.AddRange (settings.GetRuntimeEnvParam (runtimeEnvParam));
                            }
                        }

                    }
                    envParamStr = paramPath.ToArray ();
                }
            }
            return envParamStr;
        }

        private static void AddDebugParam (RuntimeParamOverride runtime, byte mask,
            C4DataType type, float min, float max, string str, List<DebugEnvParam> paramList)
        {
            if (type != C4DataType.None)
            {
                DebugEnvParam dep = new DebugEnvParam ()
                {
                hash = runtime.hash,
                desc = str,
                valueMask = mask,
                min = min,
                max = max,
                resType = ResObject.Vector
                };
                paramList.Add (dep);
            }
        }
        private static void AddDebugParam (RuntimeParamOverride runtime, byte mask, string str, List<DebugEnvParam> paramList)
        {
            DebugEnvParam dep = new DebugEnvParam ()
            {
                hash = runtime.hash,
                desc = str,
                valueMask = mask,
                resType = ResObject.Light
            };
            paramList.Add (dep);
        }
        public static void ExportEnvConfig ()
        {
            var context = EngineContext.instance;
            if (context != null)
            {
                var envModifys = context.envModifys;
                if (envModifys != null)
                {
                    string debugEnvPath = string.Format ("{0}/Config/EnvDebug.asset", AssetsConfig.instance.ResourcePath);

                    DebugEnv de = AssetDatabase.LoadAssetAtPath<DebugEnv> (debugEnvPath);
                    if (de == null)
                    {
                        de = ScriptableObject.CreateInstance<DebugEnv> ();
                        de.name = "EnvDebug";
                        de = EditorCommon.CreateAsset<DebugEnv> (debugEnvPath, ".asset", de);
                    }
                    de.groups.Clear ();
                    for (int i = 0; i < envModifys.Length; ++i)
                    {
                        var modify = envModifys[i];
                        if (modify != null)
                        {
                            var setting = modify.runtime;

                            if (setting.runtimeEnvParam.Count > 0)
                            {
                                DebugEnvGroup deg = new DebugEnvGroup ()
                                {
                                    groupName = setting.GetType ().Name,
                                };
                                de.groups.Add (deg);
                                for (int j = 0; j < setting.runtimeEnvParam.Count; ++j)
                                {
                                    var runtimeParam = setting.runtimeEnvParam[j];
                                    foreach (var attr in runtimeParam.attributes)
                                    {
                                        if (attr is CFParam4Attribute)
                                        {
                                            var p4 = attr as CFParam4Attribute;
                                            AddDebugParam (runtimeParam, ParamOverride.MaskX, p4.type0, p4.min0, p4.max0, p4.v0Str, deg.paramList);
                                            AddDebugParam (runtimeParam, ParamOverride.MaskY, p4.type1, p4.min1, p4.max1, p4.v1Str, deg.paramList);
                                            AddDebugParam (runtimeParam, ParamOverride.MaskZ, p4.type2, p4.min2, p4.max2, p4.v2Str, deg.paramList);
                                            AddDebugParam (runtimeParam, ParamOverride.MaskW, p4.type3, p4.min3, p4.max3, p4.v3Str, deg.paramList);
                                            break;
                                        }
                                        // if (attr is CFLightingAttribute)
                                        // {
                                        //     var light = attr as CFLightingAttribute;
                                        //     AddDebugParam (runtimeParam, ParamOverride.MaskX, p4.type0, p4.min0, p4.max0, p4.v0Str, deg.paramList);
                                        //     AddDebugParam (runtimeParam, ParamOverride.MaskY, p4.type1, p4.min1, p4.max1, p4.v1Str, deg.paramList);
                                        //     AddDebugParam (runtimeParam, ParamOverride.MaskZ, p4.type2, p4.min2, p4.max2, p4.v2Str, deg.paramList);
                                        //     AddDebugParam (runtimeParam, ParamOverride.MaskW, p4.type3, p4.min3, p4.max3, p4.v3Str, deg.paramList);
                                        //     break;
                                        // }
                                        else if (attr is CFColorUsageAttribute)
                                        {
                                            var c = attr as CFColorUsageAttribute;
                                            DebugEnvParam dep = new DebugEnvParam ()
                                            {
                                                hash = runtimeParam.hash,
                                                desc = runtimeParam.runtime.name,
                                                valueMask = c.showAlpha?(byte) 1: (byte) 0,
                                                max = c.hdr?10 : 1,
                                                resType = ResObject.Color
                                            };
                                            deg.paramList.Add (dep);
                                            break;
                                        }
                                    }

                                }

                            }

                        }
                    }
                    EditorCommon.SaveAsset (de);
                }
            }
        }

        public static void Save(EnvProfile profile, EnvProfile runtimeProfile)
        {
            if (profile != null && runtimeProfile != null)
            {
                var paramMap = new Dictionary<uint, ParamOverride>();
                if (profile.paramObjects.IsValid())
                {
                    for (int i = 0; i < profile.paramObjects.Count; ++i)
                    {
                        var param = profile.paramObjects.SafeGet<ParamOverride>(i);
                        if (param != null)
                        {
                            paramMap[param.hash] = param;
                        }
                    }
                }
                var context = EngineContext.instance;
                for (int i = 0; i < context.envModifys.Length; ++i)
                {
                    var rem = context.envModifys[i];
                    var setting = rem != null ? rem.runtime : null;
                    if (setting != null)
                    {
                        int start = setting.GetParamStart();
                        int end = start + setting.GetParamCount();
                        for (int j = start; j < end; ++j)
                        {
                            var runtime = RenderingManager.instance.paramObjects.Get<RuntimeParamOverride>(j);
                            if (runtime != null && paramMap.TryGetValue(runtime.hash, out var src))
                            {
                                if (runtime.profile != null)
                                    src.SetValue(runtime.profile, true);
                            }
                        }
                    }
                }

                EditorCommon.SaveAsset(profile);
            }
        }
    }

}
#endif
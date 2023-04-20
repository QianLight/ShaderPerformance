using System;
using System.Collections.Generic;
using System.IO;
using CFEngine;
using UnityEditor;
using Cinemachine;
using Cinemachine.PostFX;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

/// <summary>
/// 新增方式：
/// 1.查找对应模块hash添加到ParamHashGroup
/// 2.新增对应数据转换函数(参考TransferDOFData,MatchDOFSettings),以生成新的数据列表
/// 3.新增赋值函数(参考CreateCinemachineDOF)以生成组件
/// </summary>
public class TimelinePPVolumeTransfer:Editor
{
    private static List<TimelineClip> vcamClips;
    private static Dictionary<TimelineClip, GameObject> vcamDict;
    private static GameObject[] vcamObjects;

    public delegate PPOverrideBlock TransferData();
    public delegate VolumeComponent MatchNewSetting(CinemachineVolumeSettings settings, GameObject vcam, PPOverrideKey currentData);
    public static TransferData TransferFunc;
    public static MatchNewSetting MatchFunc;
    
    /// <summary>
    /// 不同后处理对应hash集
    /// </summary>
    private static readonly uint[] ParamHashGroup = 
    {
        2833690986,  //DOFActive
        363884959, //DOF参数
        1686376725, //BloomActive
        3845061001, //SH
        2067170641, //Ambient
        3143514350, //Fog
        
    };

    [MenuItem("ArtTools/Timeline更新/0-全部更新")]
    public static void CreateCinemachineVolumeSettings()
    {
        CreateCinemachineDOF();
    }

    #region DOF

    [MenuItem("ArtTools/Timeline更新/更新DOF配置")]
    public static void CreateCinemachineDOF()
    {
        TransferFunc = TransferDOFData;
        MatchFunc = MatchDOFSettings;
        CreateCinemachineVolumeSettingProperty();
    }

    private static UnityEngine.Rendering.Universal.DepthOfField MatchDOFSettings(CinemachineVolumeSettings settings,
        GameObject vcam, PPOverrideKey currentData)
    {
        settings.m_Profile.TryGet(out UnityEngine.Rendering.Universal.DepthOfField newSettings);
        if (!newSettings)
        {
            newSettings = VolumeProfileFactory.CreateVolumeComponent<UnityEngine.Rendering.Universal.DepthOfField>(
                settings.m_Profile);
        }
        else
        {
            Debug.LogWarning(vcam.name+"已有DOF组件，可能有数据被覆盖");
        }
                        
        newSettings.mode = new DepthOfFieldModeParameter(DepthOfFieldMode.Easy, true);
        newSettings.active = currentData.config[0].value == 1;
        newSettings.easyMode.overrideState = currentData.config[1].isOverride;
        newSettings.easyMode.value = currentData.config[1].value == 1;
        newSettings.focusDistance.overrideState = currentData.config[2].isOverride;
        newSettings.focusDistance.value = currentData.config[2].value;
        newSettings.bokehRangeNear.overrideState = currentData.config[3].isOverride;
        newSettings.bokehRangeNear.value = currentData.config[3].value;
        newSettings.focusRangeFar.overrideState = currentData.config[4].isOverride;
        newSettings.focusRangeFar.value = currentData.config[4].value;
        return newSettings;
    }
    
    /// <summary>
    /// 0:Active 1:EasyMode 2:FocusDistance 3: BokehRangeFar 4:FocusRangeNear
    /// </summary>
    /// <returns></returns>
    private static PPOverrideBlock TransferDOFData()
    {
        var dofActiveOld = SearchOriginalConfig(0);
        var dofPropertyOld = SearchOriginalConfig(1);
        PPOverrideBlock newBlock = new PPOverrideBlock();
        newBlock.name = "EasyDOF";
        //模块所有Property数量(包括Active)
        newBlock.propertyNum = 5;
        newBlock.keys = new List<PPOverrideKey>();
        List<float> defaultSettings = new List<float>() {0, 1, 30, 1, 0};
        CheckActiveParamOverride(ref newBlock, dofActiveOld.param, 0, defaultSettings);
        CheckVector4ParamOverride(ref newBlock, dofPropertyOld.param, 1, defaultSettings);
        return newBlock;
    }
    #endregion

    #region Lighting
    public static void CreateCinemachineLight()
    {

    }
    
    /// <summary>
    /// 0:Active 1:EasyMode 2:FocusDistance 3: BokehRangeFar 4:FocusRangeNear
    /// </summary>
    /// <returns></returns>
    private static PPOverrideBlock TransferLightingData()
    {
        //获取参数列表
        //-- var dofActiveOld = SearchOriginalConfig(0);
        //-- var dofPropertyOld = SearchOriginalConfig(1);
        PPOverrideBlock newBlock = new PPOverrideBlock();
        newBlock.name = "Lighting";
        //模块所有Property数量(包括Active)
        //-- newBlock.propertyNum = 5;
        newBlock.keys = new List<PPOverrideKey>();
        //按顺序设置参数默认值
        //-- List<float> defaultSettings = new List<float>() {0, 1, 30, 1, 0};
        //按顺序进行替换
        //-- CheckActiveParamOverride(ref newBlock, dofActiveOld.param, 0, defaultSettings);
        //-- CheckVector4ParamOverride(ref newBlock, dofPropertyOld.param, 1, defaultSettings);
        return newBlock;
    }
    #endregion

    #region SharedFunction
    public static void CreateCinemachineVolumeSettingProperty()
    {
        var newData = TransferFunc();
        //获取timeline的vcam持续时间信息
        GetClipData();
        for (int i = 0; i < vcamClips.Count; i++)
        {
            Vector2 currentVCamDuration = new Vector2((float) vcamClips[i].start, (float)vcamClips[i].end);
            bool beenOverride = false;
            for (int j = 0; j < newData.keys.Count; j++)
            {
                //未赋值时按时间从小到大寻找第一个等于的key进行配置,如果第一个就大于则直接用前一个
                if ((newData.keys[j].time >= currentVCamDuration.x  || j == newData.keys.Count-1)&& !beenOverride)
                {
                    string tick = "";
                    if (newData.keys[j].time - currentVCamDuration.x > 0.001f)
                    {
                        j--;
                        tick = "有起始匹配: ";
                    }
                    else
                    {
                        tick = "无起始匹配，使用前一个参数: ";
                    }

                    if (j < 0)
                    {
                        j = 0;
                        tick = "无起始匹配且为第一个，使用第一个参数: ";
                    }
                    tick += (int)newData.keys[j].time + ":" + (int)(newData.keys[j].time % 1 * 30);
                    Debug.Log(vcamClips[i].displayName+tick);

                    
                    vcamDict.TryGetValue(vcamClips[i], out GameObject vcam);
                    var currentData = newData.keys[j];
                    if (vcam)
                    {
                        CinemachineVirtualCamera virtualCamera = vcam.GetComponent<CinemachineVirtualCamera>();
                        //获取当前vcam后处理组件
                        vcam.TryGetComponent(out CinemachineVolumeSettings settings);
                        if (!settings)
                        {
                            // settings = vcam.AddComponent<CinemachineVolumeSettings>();
                            virtualCamera.AddExtension(vcam.AddComponent<CinemachineVolumeSettings>());
                            vcam.TryGetComponent(out settings);
                        }
                        if (!settings.m_Profile)
                        {
                            string name = GameObject.Find("timeline").transform.parent.name;
                            string path = SceneManager.GetActiveScene().path;
                            path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                            string sceneName = SceneManager.GetActiveScene().name;
                            path = path + "\\" + name + "_" + settings.name+" Profile.asset";
                            var old = AssetDatabase.LoadAssetAtPath(path, typeof(VolumeProfile));
                            if (old != null)
                            {
                                settings.m_Profile = old as VolumeProfile;
                                Debug.Log("使用旧配置，来自"+path);
                            }
                            else
                            {
                                VolumeProfile vp = VolumeProfileFactory.CreateVolumeProfile(settings.gameObject.scene, name + "_" + settings.name);
                                settings.m_Profile = vp;
                                settings.m_Profile.name = name;
                            }
                        }
                        VolumeComponent newSettings = MatchFunc(settings, vcam, currentData);
                        settings.m_Profile.isDirty = true;
                    }
                    beenOverride = true;
                }
                else if (newData.keys[j].time >= currentVCamDuration.y)
                {
                    // currentStart = j;
                    break;
                }
            }

            if (!beenOverride)
            {
                
            }
        }
    }
    public static void GetClipData()
    {
        //获取当前Playable内vcam的数据(起止时间和名字)
        var playableDirector = GameObject.Find("timeline").GetComponent<PlayableDirector>();
        var timeline = playableDirector.playableAsset as TimelineAsset;
        vcamClips = new List<TimelineClip>();
        foreach (var track in timeline.GetOutputTracks())
        {
            foreach (var clip in track.GetClips())
            {
                if (clip.displayName.Contains("CM vcam"))
                {
                    vcamClips.Add(clip);
                    // Debug.Log(clip.displayName+" : "+clip.start+" - " + clip.duration);
                }
            }
        }
        CombineVCamClips();
    }
    private static void CombineVCamClips()
    {
        vcamDict = new Dictionary<TimelineClip, GameObject>();
        //获取vcam列表
        var vcamRoot = GameObject.Find("cineroot");
        if (vcamRoot is null)
        {
            Debug.LogError("当前Timeline 虚拟相机列表缺失或格式不正确");
            return;
        }

        vcamObjects = new GameObject[vcamRoot.transform.childCount];
        for (var index = 0; index < vcamObjects.Length; index++)
        {
            vcamObjects[index] = vcamRoot.transform.GetChild(index).gameObject;
            for (int i = 0; i < vcamClips.Count; i++)
            {
                if (vcamClips[i].displayName.Equals(vcamObjects[index].name))
                {
                    vcamDict.Add(vcamClips[i], vcamObjects[index]);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 根据hash值(组内序号)获取当前timeline内的原有配置
    /// </summary>
    /// <param name="paramType"></param>
    /// <returns></returns>
    private static EnvParam SearchOriginalConfig(int paramType)
    {
        var originalConfig = GameObject.Find("Env");
        if (originalConfig is null)
        {
            Debug.Log("未加载Timeline或当前Timeline未配置Env");
            return null;
        }

        originalConfig.TryGetComponent(out AnimEnv animEnv);
        if (animEnv is null)
        {
            Debug.Log("当前Timeline未使用 AnimEnv组件");
            return null;
        }

       /* var configAsset = animEnv.animEnvProfile.profile;
        var paramsList = configAsset.envBlock.envParams;
        EnvParam param = null;
        foreach (var paramBlock in paramsList)
        {
            if (paramBlock.hash.Equals(ParamHashGroup[paramType]))
            {
                param = paramBlock;
                break;
            }
        }

        if (param is null)
        {
            Debug.Log("当前timeline未修改本效果 hash:"+ParamHashGroup[paramType]);
            return null;
        }*/

        return null;
    }

    private static void CheckVector4ParamOverride(ref PPOverrideBlock ppOverrideBlock, ParamOverride oldData, int startSerial, List<float> defaultSetting = null)
    {
        Vector4Param vector4Param = oldData as Vector4Param;
        CheckFloatParamOverride(ref ppOverrideBlock, vector4Param.curve0, startSerial, defaultSetting);
        CheckFloatParamOverride(ref ppOverrideBlock, vector4Param.curve1, startSerial+1, defaultSetting);
        CheckFloatParamOverride(ref ppOverrideBlock, vector4Param.curve2, startSerial+2, defaultSetting);
        CheckFloatParamOverride(ref ppOverrideBlock, vector4Param.curve3, startSerial+3, defaultSetting);
    }

    private static void CheckActiveParamOverride(ref PPOverrideBlock ppOverrideBlock, ParamOverride oldData, int serial, List<float> defaultSetting = null)
    {
        ActiveParam activeParam = oldData as ActiveParam;
        CheckBoolParamOverride(ref ppOverrideBlock, activeParam.curve, serial, defaultSetting);
    }

    private static void CheckBoolParamOverride(ref PPOverrideBlock ppOverrideBlock, ParamOverride oldData, int serial, List<float> defaultSetting = null)
    {
        BoolParam boolParam = oldData as BoolParam;
        CheckBoolParamOverride(ref ppOverrideBlock, boolParam.curve, serial, defaultSetting);
    }
    private static void CheckFloatParamOverride(ref PPOverrideBlock ppOverrideBlock, ParamOverride oldData, int serial, List<float> defaultSetting = null)
    {
        FloatParam floatParam = oldData as FloatParam;
        CheckFloatParamOverride(ref ppOverrideBlock, floatParam.curve, serial, defaultSetting);
    }
    private static void CheckFloatParamOverride(ref PPOverrideBlock ppOverrideBlock, AnimContextFloat oldData, int serial, List<float> defaultSetting = null)
    {
        AddFloatParamOverride(ref ppOverrideBlock, oldData.data, serial, defaultSetting);
    }

    private static void CheckBoolParamOverride(ref PPOverrideBlock ppOverrideBlock, AnimContextBool oldData, int serial, List<float> defaultSetting = null)
    {
        AddBoolParamOverride(ref ppOverrideBlock, oldData.data, serial, defaultSetting);
    }

    private class TimelineKeyPair
    {
        
    }
    private class TimelineKeyFactory<T, TT>:TimelineKeyPair where T : new() where TT : BaseAnimKey, new()
    {
        public T newKey;
        public TT oldKey;
    }

    private static TimelineKeyPair[] factory = new TimelineKeyPair[]
    {
        new TimelineKeyFactory<bool, BoolAnimKey>(),
        new TimelineKeyFactory<float, FloatAnimKey>(),
        new TimelineKeyFactory<Vector3, V3AnimKey>(),
        new TimelineKeyFactory<Color, ColorAnimKey>(),
    };

    private static Type[] keyType = new Type[]
    {
        typeof(bool),
        typeof(float),
        typeof(Vector3),
        typeof(Color)
    };
    /// <summary>
    /// 向Override后处理单块增加一个参数列表的配置
    /// </summary>
    /// <param name="overrideBlock">被处理的单块</param>
    /// <param name="paramList">被处理的参数列表</param>
    /// <param name="serial">被处理的参数在该后处理单块的序号</param>
    private static void AddFloatParamOverride(ref PPOverrideBlock overrideBlock, List<FloatAnimKey> paramList, int serial, List<float> defaultSetting)
    {
        if (serial >= overrideBlock.propertyNum)
        {
            Debug.LogError("需要拷贝的参数序号超过该项的参数数量");
            return;
        }

        if (defaultSetting.Count != overrideBlock.propertyNum)
        {
            Debug.LogError("默认参数数量与组件参数数量不符");
            return;
        }
        
        //先按时间从小到大排序(防意外纠错)
        overrideBlock.keys.Sort();
        //逐参数关键帧塞入单块关键帧列表
        foreach (var pAnimKey in paramList)
        {
            bool added = false;
            //寻找匹配的塞入时间点
            for (int i = 0; i < overrideBlock.keys.Count; i++)
            {
                if (overrideBlock.keys[i].time.Equals(pAnimKey.time))
                {
                    overrideBlock.keys[i].config[serial] = new PPOverrideParam(){isOverride = true, value = pAnimKey.v};
                    added = true;
                    break;
                }

                if (overrideBlock.keys[i].time > pAnimKey.time)
                {
                    var newOverride = new PPOverrideKey();
                    newOverride.time = pAnimKey.time;
                    newOverride.config = new List<PPOverrideParam>();
                    for (int j = 0; j < overrideBlock.propertyNum; j++)
                    {
                        bool isOverride = j == serial;
                        newOverride.config.Add(new PPOverrideParam(){isOverride = isOverride, value = isOverride ? pAnimKey.v : defaultSetting[j]});
                    }
                    overrideBlock.keys.Insert(i, newOverride);
                    added = true;
                    break;
                }
            }
            if (!added)
            {
                var newOverride = new PPOverrideKey();
                newOverride.time = pAnimKey.time;
                newOverride.config = new List<PPOverrideParam>();
                for (int j = 0; j < overrideBlock.propertyNum; j++)
                {
                    bool isOverride = j == serial;
                    newOverride.config.Add(new PPOverrideParam(){isOverride = isOverride, value = isOverride ? pAnimKey.v : defaultSetting[j]});
                }
                overrideBlock.keys.Add(newOverride);
            }
        }
    }
    private static void AddBoolParamOverride(ref PPOverrideBlock overrideBlock, List<BoolAnimKey> paramList, int serial, List<float> defaultSetting = null)
    {
        
        if (serial >= overrideBlock.propertyNum)
        {
            Debug.LogError("需要拷贝的参数序号超过该项的参数数量");
            return;
        }
        if (defaultSetting.Count != overrideBlock.propertyNum)
        {
            Debug.LogError("默认参数数量与组件参数数量不符");
            return;
        }
        
        //先按时间从小到大排序(防意外纠错)
        overrideBlock.keys.Sort();
        //逐参数关键帧塞入单块关键帧列表
        foreach (var pAnimKey in paramList)
        {
            bool added = false;
            //寻找匹配的塞入时间点
            for (int i = 0; i < overrideBlock.keys.Count; i++)
            {
                //时间匹配的点
                if (overrideBlock.keys[i].time.Equals(pAnimKey.time))
                {
                    overrideBlock.keys[i].config[serial] = new PPOverrideParam(){isOverride = true, value = pAnimKey.v ? 1 : 0};
                    added = true;
                    break;
                }

                //时间不匹配的第一个点
                if (overrideBlock.keys[i].time > pAnimKey.time)
                {
                    var newOverride = new PPOverrideKey();
                    newOverride.time = pAnimKey.time;
                    newOverride.config = new List<PPOverrideParam>();
                    for (int j = 0; j < overrideBlock.propertyNum; j++)
                    {
                        bool isOverride = j == serial;
                        newOverride.config.Add(new PPOverrideParam(){isOverride = isOverride, value = isOverride ? (pAnimKey.v? 1:0) : defaultSetting[j]} );
                    }
                    overrideBlock.keys.Insert(i, newOverride);
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                var newOverride = new PPOverrideKey();
                newOverride.time = pAnimKey.time;
                newOverride.config = new List<PPOverrideParam>();
                for (int j = 0; j < overrideBlock.propertyNum; j++)
                {
                    bool isOverride = j == serial;
                    newOverride.config.Add(new PPOverrideParam(){isOverride = isOverride, value = isOverride ? (pAnimKey.v? 1:0): defaultSetting[j]});
                }
                overrideBlock.keys.Add(newOverride);
            }
        }
    }
    #endregion
    
}

/// <summary>
/// 单数据类关键帧组
/// </summary>
public class PPOverrideBlock
{
    public string name;
    public int propertyNum;
    public List<PPOverrideKey> keys;
}

/// <summary>
/// 单关键帧参数
/// </summary>
public class PPOverrideKey : IComparable<PPOverrideKey>
{
    public float time;
    public List<PPOverrideParam> config;
    public int CompareTo(PPOverrideKey other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return time.CompareTo(other.time);
    }
}

public struct PPOverrideParam
{
    public bool isOverride;
    public float value;
}

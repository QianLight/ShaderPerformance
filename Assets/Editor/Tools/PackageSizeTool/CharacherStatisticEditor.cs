using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CFUtilPoolLib;
using LevelEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using XHitData = ClientEcsData.XHitData;
using XSkillData = ClientEcsData.XSkillData;

public static class CharacherStatisticEditor
{
    class ResData
    {
        public String Guid;
        public uint DataSize;
        public String Path;
        public bool IsRepeat;
    }


    private static String tableFilepath = "Assets/BundleRes/Table";

    private class PathCharacterProperties
    {
        public String RoleCurvePathRre;
        public String RoleAnimationPathRre;
        public String RoleSkillPathPre;
        public String RoleReactPathPre;
        public String RoleHitPathPre;

        public String RolePrefabPre;
        public String RoleFXPathPre;
        public String MonsterPrefabPre;
        public String OtherPrefabPre;
        public String OtherLevelPrefabPre;
        public String OtherCutScenePrefabPre;
        public String TimeLinePathPre;
        public String VideoPathPre;
        public String Other1PrefabPre;
        public String SpecialActionPath;
    }

    private static PathCharacterProperties RolePath = new PathCharacterProperties()
    {
        RoleCurvePathRre = "Assets/BundleRes/Curve/",
        RoleAnimationPathRre = "Assets/BundleRes/Animation/",
        RoleSkillPathPre = "Assets/BundleRes/SkillPackage/",
        RoleHitPathPre = "Assets/BundleRes/HitPackage/",
        RoleReactPathPre = "Assets/BundleRes/ReactPackage/",
        RolePrefabPre = "Assets/BundleRes/Runtime/Prefab/Role/",
        MonsterPrefabPre = "Assets/BundleRes/Runtime/Prefab/Monster/",
        OtherPrefabPre = "Assets/BundleRes/Runtime/Prefab/",
        Other1PrefabPre = "Assets/BundleRes/Prefabs/",
        OtherLevelPrefabPre = "Assets/BundleRes/Runtime/Prefab/LevelObject/",
        OtherCutScenePrefabPre = "Assets/BundleRes/Runtime/Prefab/CutScene/",
        RoleFXPathPre = "Assets/BundleRes/Runtime/SFX/",
        TimeLinePathPre = "Assets/BundleRes/TimeLine/",
        VideoPathPre = "Assets/BundleRes/Video/",
        SpecialActionPath = "Assets/BundleRes/SpecialAction/"
    };

    private static Dictionary<String, String> _fxLower2UpDic;
    private static Dictionary<String, String> _timeLineLower2UpDic;
    private static Dictionary<String, String> _entityPrefabDic;
    private static Dictionary<String, String> _skillHitPathDic;
    private static Dictionary<String, Dictionary<String, ResData>> _characterData;
    private static Dictionary<String, ResData> _cacheData;
    private static HashSet<String> _sceneCache;
    private static HashSet<String> _characterCache;
    private static Dictionary<uint, XEntityPresentation.RowData> _monsterPresentationDic;
    private static Dictionary<String, Dictionary<String, ResData>> _sceneDic;
    private static Dictionary<String, LevelData> _levelDatas;
    private static Dictionary<String, Dictionary<String, ResData>> _levelDic;
    private static readonly String ScenePathPre = "Assets/BundleRes/Scene/";
    private static Dictionary<String, Dictionary<String, ResData>> _cutSceneCharacterDic;
    private static Dictionary<string, string> _SpecialRadialBlurV2Up2LowerDic;


    private class LevelData
    {
        public String SceneName;
        public List<ResData> LevelSelfData;
        public HashSet<String> MonsterSet;
        public HashSet<String> CutSceneCharacterSet;

        public LevelData()
        {
            SceneName = "";
            LevelSelfData = new List<ResData>();
            MonsterSet = new HashSet<String>();
            CutSceneCharacterSet = new HashSet<String>();
        }
    }


    private static void PreparePathUp2Lower()
    {
        _fxLower2UpDic = new Dictionary<String, String>();
        var fxfiles = GetFileByPath(RolePath.RoleFXPathPre, ".prefab");
        foreach (var file in fxfiles)
        {
            var value = file.Replace(".prefab", "");
            var key = value.ToLower();
            _fxLower2UpDic.Add(key, value);
        }

        _timeLineLower2UpDic = new Dictionary<String, String>();
        var timelineFiles = GetFileByPath(RolePath.TimeLinePathPre, ".prefab");
        foreach (var file in timelineFiles)
        {
            var value = file.Replace(".prefab", "");
            var key = value.ToLower();
            _timeLineLower2UpDic.Add(key, value);
        }

        _SpecialRadialBlurV2Up2LowerDic = new Dictionary<String, String>();
        var radiusBlur = RolePath.SpecialActionPath + "RadialBlurV2/Data/" ;
        DirectoryInfo direction = new DirectoryInfo(radiusBlur);
        DirectoryInfo[] dirs = direction.GetDirectories();
        for (int i = 0; i < dirs.Length; i++)
        {
            var path = radiusBlur +  dirs[i].Name;
            var allasset = GetFileByPath(path, ".asset");
            foreach (var asset in allasset)
            {
                var value = asset.Replace(".asset", "");
                var key =   value.ToLower();
                _SpecialRadialBlurV2Up2LowerDic.Add(key, path + "/" + asset);
            }

        }


        
    }

    // [MenuItem("Tools/统计资源/统计场景及角色资源", priority = 0)]
    // static void AllStatistic()
    // {
    //     PackAgeSceneSelectEditorWindow.Init(
    //         (List<MapList.RowData> mapList) =>
    //         {
    //             PackAgeSceneSelectEditorWindow.Init(
    //                 (HashSet<uint> characterIds) =>
    //                 {
    //                     InitPrefabPath();
    //                     PreparePathUp2Lower();
    //                     _characterData = new Dictionary<String, Dictionary<String, ResData>>();
    //                     _cacheData = new Dictionary<String, ResData>(1 << 14);
    //                     GenAllCharacter(characterIds, _cacheData, _characterData);
    //                     GetResSize(_characterData);
    //                     WriteResToFile("CharacterRes.txt", _characterData);
    //                     ClacScene(mapList);
    //                 }
    //             );
    //         }
    //     );
    // }
    //
    // [MenuItem("Tools/统计资源/统计角色资源", priority = 0)]
    // static void CharacterStatistic()
    // {
    //     PackAgeSceneSelectEditorWindow.Init(
    //         (HashSet<uint> characterIds) =>
    //         {
    //             InitPrefabPath();
    //             PreparePathUp2Lower();
    //             _characterData = new Dictionary<String, Dictionary<String, ResData>>();
    //             _cacheData = new Dictionary<String, ResData>(1 << 14);
    //             var allRoles = new HashSet<uint>() ;
    //             GenAllCharacter(allRoles, _cacheData, _characterData);
    //             GetResSize(_characterData);
    //             WriteResToFile("CharacterRes.txt", _characterData);
    //         });
    // }
    //
    // [MenuItem("Tools/统计资源/统计场景资源")]
    // static void SceneStatistic()
    // {
    //     PackAgeSceneSelectEditorWindow.Init(ClacScene);
    // }


    private static void InitPrefabPath()
    {
        #region 初始化prefab字典

        static void AddPrefab(List<String> datas, String prePath)
        {
            foreach (var data in datas)
            {
                string key = data.ToLower().Replace(".prefab", "");
                if (_entityPrefabDic.ContainsKey(key))
                {
                    continue;
                }

                _entityPrefabDic.Add(key, prePath + data);
            }
        }

        static void AddPrefabByPath(String rootPath)
        {
            var root = new DirectoryInfo(rootPath);
            DirectoryInfo[] dirs = root.GetDirectories();
            foreach (var dir in dirs)
            {
                var path = rootPath + dir.Name + "/";
                var files = GetFileByPath(path, ".prefab");
                AddPrefab(files, path);
            }
        }

        //初始化prefab字典
        _entityPrefabDic = new Dictionary<String, String>();
        _skillHitPathDic = new Dictionary<String, String>();
        var root = new DirectoryInfo(RolePath.RoleHitPathPre);
        DirectoryInfo[] dirs = root.GetDirectories();
        foreach (var dir in dirs)
        {
            var path = RolePath.RoleHitPathPre + dir.Name + "/";
            var files = GetFileByPath(path, ".bytes");
            foreach (var file in files)
            {
                var key = file.Replace(".bytes", "").ToLower();
                if (!_skillHitPathDic.ContainsKey(key))
                {
                    _skillHitPathDic.Add(key, path + file);
                }
            }
        }

        AddPrefabByPath(RolePath.RolePrefabPre);
        AddPrefabByPath(RolePath.MonsterPrefabPre);
        AddPrefabByPath(RolePath.OtherLevelPrefabPre);
        AddPrefabByPath(RolePath.OtherCutScenePrefabPre);
        var otherFiles = GetFileByPath(RolePath.OtherPrefabPre, ".prefab");
        AddPrefab(otherFiles, RolePath.OtherPrefabPre);
        var other1Files = GetFileByPath(RolePath.Other1PrefabPre, ".prefab");
        AddPrefab(other1Files, RolePath.Other1PrefabPre);

        #endregion
    }

    private static void ClacScene(List<MapList.RowData> mapList)
    {
        if (_cacheData == null)
        {
            _cacheData = new Dictionary<String, ResData>();
        }

        _sceneDic = new Dictionary<String, Dictionary<String, ResData>>();
        if (_characterData == null)
        {
            _characterData = new Dictionary<String, Dictionary<String, ResData>>();
        }

        _characterCache = new HashSet<string>();
        _sceneCache = new HashSet<string>();
        InitPrefabPath();
        PreparePathUp2Lower();

        var monsterDataList = XEntityStatisticsReader.Statistics.Table;
        _monsterPresentationDic = new Dictionary<uint, XEntityPresentation.RowData>(monsterDataList.Length);
        _cutSceneCharacterDic = new Dictionary<String, Dictionary<String, ResData>>();

        _sceneDic = new Dictionary<String, Dictionary<String, ResData>>(20);
        _levelDic = new Dictionary<String, Dictionary<String, ResData>>(20);
        foreach (var monster in monsterDataList)
        {
            var presentData = XEntityPresentationReader.GetData(monster.PresentID);
            _monsterPresentationDic.Add(monster.ID, presentData);
        }

        _levelDatas = new Dictionary<String, LevelData>(20);
        foreach (var level in mapList)
        {
            _levelDatas.Add(level.MapID + level.Comment, new LevelData());
        }

        // 与计算场景大小及内容
        PreCalcSceneBase(mapList);
        // 预计算场场景怪物内容
        PreCalcSceneMonster(mapList);
        // 计算关卡单独内容
        ClacLevel(mapList);
        // 计算最终结果
        CalcSceneRes(_levelDatas);

        GetResSize(_levelDic);
        // 写入文件
        WriteResToFile("SceneRes.txt", _levelDic, _levelDatas);
    }


    private static void CalcSceneRes(Dictionary<String, LevelData> levelDatas)
    {
        foreach (var levelData in levelDatas)
        {
            var charNames = levelData.Value.MonsterSet;
            foreach (var name in charNames)
            {
                if (_characterData.ContainsKey(name))
                {
                    var hasCache = _characterCache.Contains(name);
                    var data = _characterData[name];
                    foreach (var resData in data)
                    {
                        ResData temp = resData.Value;
                        if (hasCache)
                        {
                            ResData newdata = new ResData()
                            {
                                Path = temp.Path,
                                DataSize = temp.DataSize,
                                Guid = temp.Guid,
                                IsRepeat = true,
                            };
                            temp = newdata;
                        }

                        AddResDataToDic(levelData.Key, temp, _levelDic);
                    }

                    _characterCache.Add(name);
                }
            }

            var cutScenesCharacters = levelData.Value.CutSceneCharacterSet;
            foreach (var cut in cutScenesCharacters)
            {
                if (_cutSceneCharacterDic.ContainsKey(cut))
                {
                    var datas = _cutSceneCharacterDic[cut];
                    foreach (var resData in datas)
                    {
                        ResData temp = resData.Value;

                        ResData newdata = new ResData()
                        {
                            Path = temp.Path,
                            DataSize = temp.DataSize,
                            Guid = temp.Guid,
                            IsRepeat = true,
                        };
                        temp = newdata;
                        AddResDataToDic(levelData.Key, temp, _levelDic);
                    }
                }
                else
                {
                    GameObject prefab;
                    var data = GetPrefabResData(_entityPrefabDic[cut], out prefab);
                    if (data != null)
                    {
                        AddResDataToDic(levelData.Key, data, _levelDic);
                        AddResDataToDic(cut, data, _cutSceneCharacterDic);
                        AddCacheResDataToDic(data, _cacheData);
                    }

                    var temp1 = GetAllMesh(prefab);
                    var temp2 = GetAllMaterial(prefab);
                    AddRangeResDataToCacheDic(temp1, _cacheData);
                    AddRangeResDataToCacheDic(temp2, _cacheData);
                    AddRangeResDataToDic(cut, temp1, _cutSceneCharacterDic);
                    AddRangeResDataToDic(cut, temp2, _cutSceneCharacterDic);
                    AddRangeResDataToDic(levelData.Key, temp1, _levelDic);
                    AddRangeResDataToDic(levelData.Key, temp2, _levelDic);
                }
            }

            var sceneName = levelData.Value.SceneName;

            if (_sceneDic.ContainsKey(sceneName))
            {
                var data = _sceneDic[sceneName];
                var hasCache = _sceneCache.Contains(sceneName);
                foreach (var resData in data)
                {
                    ResData temp = resData.Value;
                    if (hasCache)
                    {
                        ResData newdata = new ResData()
                        {
                            Path = temp.Path,
                            DataSize = temp.DataSize,
                            Guid = temp.Guid,
                            IsRepeat = true,
                        };
                        temp = newdata;
                    }

                    AddResDataToDic(levelData.Key, temp, _levelDic);
                }

                _sceneCache.Add(sceneName);
            }

            AddRangeResDataToCacheDic(levelData.Value.LevelSelfData, _cacheData);
            AddRangeResDataToDic(levelData.Key, levelData.Value.LevelSelfData, _levelDic);
        }
    }

    private static void GenAllCharacter(
        HashSet<uint> partenerIds,
        Dictionary<String, ResData> cacheDic,
        Dictionary<String, Dictionary<String, ResData>> resDic
    )
    {
        foreach (var id in partenerIds)
        {
            var entity = XEntityPresentationReader.GetData(id);
            if (entity == null)
            {
                Debug.Log("cant find entity：" + id);
                continue;
            }

            var name = entity.PresentID + entity.Name;
            if (!_characterData.ContainsKey(name))
            {
                _characterData.Add(name, new Dictionary<String, ResData>(500));
            }

            if (!_entityPrefabDic.ContainsKey(entity.Prefab))
            {
                Debug.Log($"cant find prefab {entity.Prefab}");
                continue;
            }

            var meshResDatas = new List<ResData>();
            var materialResDatas = new List<ResData>();
            var meshSuffix = new List<String>() {"", "_lod1", "_lod2"};
            foreach (var suffix in meshSuffix)
            {
                var key = entity.Prefab + suffix;
                if (!_entityPrefabDic.ContainsKey(key))
                {
                    continue;
                }

                var prePath = _entityPrefabDic[key];
                GameObject prefab;
                var data = GetPrefabResData(prePath, out prefab);
                if (data == null) continue;
                AddCacheResDataToDic(data, cacheDic);
                AddResDataToDic(name, data, resDic);

                var temp1 = GetAllMesh(prefab);
                var temp2 = GetAllMaterial(prefab);
                meshResDatas.AddRange(temp1);
                materialResDatas.AddRange(temp2);
                PrefabUtility.UnloadPrefabContents(prefab);
            }

            var res = new List<ResData>();
            var skillPath = RolePath.RoleSkillPathPre + entity.SkillLocation;
            var reactPath = RolePath.RoleReactPathPre + entity.BehitLocation;
            var hitfiles = entity.BeHit;
            var fxAndTexResData = GetAllSkilll(skillPath, reactPath, hitfiles);
            var animationPath = RolePath.RoleAnimationPathRre + entity.AnimLocation;
            var animationResData = GetAllAnimation(animationPath);
            var curvepath = RolePath.RoleCurvePathRre + entity.CurveLocation;
            var curveResData = GetAllCurves(curvepath);
            
            res.AddRange(materialResDatas);
            res.AddRange(meshResDatas);
            res.AddRange(fxAndTexResData);
            res.AddRange(animationResData);
            res.AddRange(curveResData);

            AddRangeResDataToCacheDic(res, cacheDic);
            AddRangeResDataToDic(name, res, resDic);
        }
    }

    private static void PreCalcSceneMonster(List<MapList.RowData> datas)
    {
        foreach (var scene in datas)
        {
            var allCharacter = new HashSet<uint>();
            var levelDataName = scene.MapID + scene.Comment;
            var levelData = _levelDatas[levelDataName];
            String path = $"{tableFilepath}/{scene.LevelConfigFile}.cfg";
            if (!File.Exists(path))
                continue;
            var grphDatas = DataIO.DeserializeData<LevelEditorData>(path)?.GraphDataList;
            if (grphDatas == null)
                continue;
            foreach (var graphData in grphDatas)
            {
                foreach (var monster in graphData.WaveData)
                {
                    var data = _monsterPresentationDic[(uint) monster.SpawnID];
                    levelData.MonsterSet.Add(data.PresentID + data.Name);
                    allCharacter.Add(data.PresentID);
                }
            }

            ScenePartnerTable table = new ScenePartnerTable();
            var scenePartnerTable = table.GetBySceneId((int) scene.MapID);
            if (scenePartnerTable != null)
            {
                foreach (var partnerId in scenePartnerTable.Partner)
                {
                    var partner = XEntityPresentationReader.GetData(partnerId);
                    levelData.MonsterSet.Add(partner.PresentID + partner.Name);
                    allCharacter.Add(partner.PresentID);
                }

                foreach (var partnerId in scenePartnerTable.Default)
                {
                    var partner = XEntityPresentationReader.GetData(partnerId);
                    levelData.MonsterSet.Add(partner.PresentID + partner.Name);
                    allCharacter.Add(partner.PresentID);
                }
            }

            // 统计怪物
            GenAllCharacter(allCharacter, _cacheData, _characterData);
        }
    }

    static GameObject[] GetOnlyChildGameObjects(GameObject father)
    {
        var count = father.transform.childCount;
        var res = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            res[i] = father.transform.GetChild(i).gameObject;
        }

        return res;
    }

    //计算场景资源
    private static void ClacLevel(List<MapList.RowData> datas)
    {
        foreach (var scene in datas)
        {
            var levelDataKey = scene.MapID + scene.Comment;
            if (!_levelDatas.ContainsKey(levelDataKey))
            {
                Debug.Log("level not found:" + levelDataKey);
                continue;
            }

            var levelData = _levelDatas[levelDataKey];

            // 统计碰撞文件
            var blockPath = $"Assets/BundleRes/{scene.BlockFilePath}.mapheight";
            if (File.Exists(blockPath))
            {
                var data = GetFileResData(blockPath);
                if (data != null)
                {
                    levelData.LevelSelfData.Add(data);
                }
            }

            levelData.SceneName = scene.UnitySceneFile;
            String path = $"{tableFilepath}/{scene.LevelConfigFile}.cfg";
            if (!File.Exists(path))
                continue;
            var temp = GetFileResData(path);
            if (temp != null)
            {
                levelData.LevelSelfData.Add(temp);
            }

            var grphDatas = DataIO.DeserializeData<LevelEditorData>(path)?.GraphDataList;
            // 统计过场动画
            foreach (var grphData in grphDatas)
            {
                foreach (var script in grphData.ScriptData)
                {
                    if (script.Cmd != LevelScriptCmd.Level_Cmd_Cutscene)
                    {
                        continue;
                    }

                    if (script.stringParam.Count < 1)
                    {
                        Debug.Log("Cutscene id not found:" + script.stringParam);
                        continue;
                    }

                    var resRealName = script.stringParam[0];
                    var timeLinePrefab = RolePath.TimeLinePathPre + resRealName + ".prefab";
                    var timeLinePath = RolePath.TimeLinePathPre + resRealName + ".playable";
                    var timeLineVideoPath = RolePath.VideoPathPre + resRealName + ".mp4";

                    if (File.Exists(timeLineVideoPath))
                    {
                        var data = GetFileResData(timeLineVideoPath);
                        if (data != null)
                        {
                            levelData.LevelSelfData.Add(data);
                        }
                    }

                    if (!File.Exists(timeLinePath) && !File.Exists(timeLinePrefab))
                    {
                        Debug.Log("Cutscene id not found:" + script.stringParam[0]);
                        continue;
                    }

                    var timeLineData = GetFileResData(timeLinePath);
                    if (timeLineData != null)
                    {
                        levelData.LevelSelfData.Add(timeLineData);
                    }


                    var resData = GetPrefabResData(timeLinePrefab, out var gameObject);
                    if (gameObject == null)
                    {
                        Debug.Log("Cutscene id not found:" + timeLinePrefab);
                        continue;
                    }

                    levelData.LevelSelfData.Add(resData);

                    var timelinesFather = gameObject.transform.Find("timeline");
                    if (timelinesFather != null)
                    {
                        var child = GetOnlyChildGameObjects(timelinesFather.gameObject);
                        foreach (var go in child)
                        {
                            if (go != null)
                            {
                                var prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                                if (prefab.EndsWith("fbx") || prefab.EndsWith("FBX"))
                                {
                                    var data = GetFileResData(prefab);
                                    if (data != null)
                                    {
                                        levelData.LevelSelfData.Add(data);
                                        var temp2 = GetAllMaterial(go);
                                        levelData.LevelSelfData.AddRange(temp2);
                                    }

                                    continue;
                                }

                                if (prefab != null)
                                {
                                    GameObject tempGo;
                                    var data = GetPrefabResData(prefab, out tempGo);
                                    if (data != null)
                                    {
                                        levelData.LevelSelfData.Add(data);
                                        var temp1 = GetAllMesh(tempGo);
                                        var temp2 = GetAllMaterial(tempGo);
                                        levelData.LevelSelfData.AddRange(temp1);
                                        levelData.LevelSelfData.AddRange(temp2);
                                    }
                                    // PrefabUtility.UnloadPrefabContents(tempGo);
                                }
                            }
                        }
                    }

                    var vfxParent = gameObject.transform.Find("VFX");
                    if (vfxParent != null)
                    {
                        var fxNames = new HashSet<string>();
                        var child = GetOnlyChildGameObjects(timelinesFather.gameObject);
                        foreach (var go in child)
                        {
                            var fx = go.name.ToLower();
                            fxNames.Add(fx);
                        }

                        var resFx = GetFxs(fxNames);
                        levelData.LevelSelfData.AddRange(resFx);
                    }

                    var comp = gameObject.GetComponentInChildren<OrignalTimelineData>();
                    var playableDirector = gameObject.GetComponentInChildren<PlayableDirector>();
                    var trackassets = new List<TrackAsset>();
                    var fx_tracks = new List<ControlTrack>();
                    if (playableDirector != null && playableDirector.playableAsset != null)
                    {
                        var outputs = playableDirector.playableAsset.outputs;
                        foreach (var assert in outputs)
                        {
                            switch (assert.sourceObject)
                            {
                                case AnimationTrack _:
                                case ActivationTrack _:
                                case CustomAnimationTrack _:
                                case RenderEffectTrack _:
                                case TransformTweenTrack _:
                                case BoneRotateTrack _:
                                case ControlPartTrack _:
                                case CharacterShadingSettingsTrack _:
                                    var atrack = assert.sourceObject as TrackAsset;
                                    trackassets.Add(atrack);
                                    break;
                                case ControlTrack _:
                                    fx_tracks.Add(assert.sourceObject as ControlTrack);
                                    break;
                            }
                        }
                    }

                    var sceneFx = new HashSet<string>();
                    foreach (var track in fx_tracks)
                    {
                        foreach (var clip in track.GetClipList())
                        {
                            var clipData = clip.asset as ControlPlayableAsset;
                            if (clipData != null && clipData.fxData != null && File.Exists(clipData.fxData.path))
                            {
                                var fx = clipData.fxData.path.Split('/');
                                var fxName = fx[fx.Length - 1].ToLower();
                                sceneFx.Add(fxName);
                            }
                        }
                    }

                    var res = GetFxs(sceneFx);
                    levelData.LevelSelfData.AddRange(res);
                    foreach (var track in trackassets)
                    {
                        foreach (var clip in track.GetClipList())
                        {
                            var asset = clip.asset as AnimationPlayableAsset;
                            if (asset != null)
                            {
                                var aniPath = AssetDatabase.GetAssetPath(asset.clip);
                                if (File.Exists(aniPath))
                                {
                                    var data = GetAnimationResData(aniPath);
                                    if (data != null)
                                    {
                                        levelData.LevelSelfData.Add(data);
                                    }
                                }
                            }
                        }
                    }

                    foreach (var asset in trackassets)
                    {
                        var gb = playableDirector.GetGenericBinding(asset);
                        if (gb != null)
                        {
                            var prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gb);
                            if (prefab.EndsWith("fbx") || prefab.EndsWith("FBX"))
                            {
                                var data = GetFileResData(prefab);
                                if (data != null)
                                {
                                    levelData.LevelSelfData.Add(data);
                                }

                                continue;
                            }

                            if (prefab != null)
                            {
                                GameObject tempGo;
                                var data = GetPrefabResData(prefab, out tempGo);
                                if (data != null)
                                {
                                    levelData.LevelSelfData.Add(data);
                                    var temp1 = GetAllMesh(tempGo);
                                    var temp2 = GetAllMaterial(tempGo);
                                    levelData.LevelSelfData.AddRange(temp1);
                                    levelData.LevelSelfData.AddRange(temp2);
                                }
                                // PrefabUtility.UnloadPrefabContents(tempGo);
                            }
                        }
                    }

                    var chars = comp?.chars;
                    if (chars == null) continue;

                    foreach (var c in chars)
                    {
                        var roleName = c.prefab.ToLower();
                        if (!_entityPrefabDic.ContainsKey(roleName))
                        {
                            Debug.Log("Cutscene id not found:" + roleName);
                            continue;
                        }

                        levelData.CutSceneCharacterSet.Add(roleName);
                    }

                    PrefabUtility.UnloadPrefabContents(gameObject);
                }
            }
        }
    }

    private static void PreCalcSceneBase(List<MapList.RowData> datas)
    {
        foreach (var scene in datas)
        {
            var sceneBasePath = ScenePathPre + scene.UnitySceneFile + "/";
            var scenePath = sceneBasePath + scene.UnitySceneFile + ".unity";
            if (!File.Exists(scenePath)) continue;
            if (_sceneDic.ContainsKey(scene.UnitySceneFile))
                continue;
            var tempList = new List<ResData>();
            _sceneDic.Add(scene.UnitySceneFile, new Dictionary<String, ResData>());

            var sceneNow = EditorSceneManager.OpenScene(scenePath);
            GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();

            var texResData = ClacSceneLightMap();
            var meshDatas = ClacSceneMeshData(gos);
            var senceGridDir = sceneBasePath + "SplitMeshFile/";
            var meshGridDatas = ClacSenceMeshGrid(senceGridDir);
            var reflectProbData = ClacReflectProData(gos);
            var materialDatas = GetAllMaterial(gos[0]);
            var sceneResData = GetFileResData(scenePath);
            if (sceneResData != null)
            {
                tempList.Add(sceneResData);
            }

            tempList.AddRange(materialDatas);
            tempList.AddRange(texResData);
            tempList.AddRange(meshDatas);
            tempList.AddRange(meshGridDatas);
            tempList.AddRange(reflectProbData);
            AddRangeResDataToCacheDic(tempList, _cacheData);
            AddRangeResDataToDic(scene.UnitySceneFile, tempList, _sceneDic);
        }
    }

    private static List<ResData> ClacReflectProData(GameObject[] gos)
    {
        var resData = new List<ResData>();
        foreach (var go in gos)
        {
            var reflectCubes = go.GetComponentsInChildren<ReflectionProbe>(true);
            foreach (var reflectCube in reflectCubes)
            {
                var tex = reflectCube.texture;
                var data = GetTexResData(tex);
                if (data != null)
                {
                    resData.Add(data);
                }
            }
        }

        return resData;
    }

    private static List<ResData> ClacSenceMeshGrid(String senceGridDir)
    {
        var resData = new List<ResData>();
        var prefabNames = GetFileByPath(senceGridDir, "prefab");
        foreach (var prefabname in prefabNames)
        {
            var path = senceGridDir + prefabname;
            var prefabData = GetPrefabResData(path, out var go);
            var meshes = GetAllMesh(go);
            var mates = GetAllMaterial(go);
            resData.Add(prefabData);
            resData.AddRange(meshes);
            resData.AddRange(mates);
            PrefabUtility.UnloadPrefabContents(go);
        }

        return resData;
    }

    private static List<ResData> ClacSceneMeshData(GameObject[] gos)
    {
        var meshResDatas = new List<ResData>();
        foreach (var go in gos)
        {
            var temp = GetAllMesh(go);
            meshResDatas.AddRange(temp);
        }

        return meshResDatas;
    }

    private static List<ResData> ClacSceneLightMap()
    {
        var lightmaps = LightmapSettings.lightmaps;
        var res = new List<ResData>();
        foreach (var lightmap in lightmaps)
        {
            var temp = GetTexResData(lightmap.lightmapColor);
            if (temp != null)
            {
                res.Add(temp);
            }
        }

        return res;
    }

    private static List<ResData> GetAllCurves(String basePath)

    {
        var res = new List<ResData>();
        var curvePaths = GetFileByPath(basePath);
        foreach (var curvePath in curvePaths)
        {
            var path = basePath + curvePath;
            var data = GetFileResData(path);
            if (data != null)
            {
                res.Add(data);
            }
        }

        return res;
    }


    private static List<ResData> GetFxs(HashSet<String> allFx)
    {
        var res = new List<ResData>();
        foreach (var fx in allFx)
        {
            var key = fx.ToLower();
            if (!_fxLower2UpDic.ContainsKey(key))
            {
                Debug.Log("can not find fx:" + fx);
                continue;
            }

            var fxPath1 = RolePath.RoleFXPathPre + _fxLower2UpDic[key] + ".prefab";
            var fxPathH = RolePath.RoleFXPathPre + _fxLower2UpDic[key] + "_H.prefab";
            var fxPathL = RolePath.RoleFXPathPre + _fxLower2UpDic[key] + "_L.prefab";
            var fxPathM = RolePath.RoleFXPathPre + _fxLower2UpDic[key] + "_M.prefab";
            var fxPaths = new List<String>()
            {
                fxPath1,
                fxPathH,
                fxPathL,
                fxPathM,
            };
            foreach (var fxPath in fxPaths)
            {
                GameObject gb;
                var fxData = GetPrefabResData(fxPath, out gb);
                if (fxData == null) continue;
                res.Add(fxData);
                var matAndTex = GetFxMaterialAndTex(gb);
                var mesh = GetAllMesh(gb);
                res.AddRange(mesh);
                res.AddRange(matAndTex);
                PrefabUtility.UnloadPrefabContents(gb);
            }
        }

        return res;
    }

    private static List<ResData> GetAllSkilll(String skillPath, String reactPath,
        SeqListRef<String> hitFile)
    {
        var res = new List<ResData>();
        var allFX = new HashSet<String>();




        #region 计算技能特效

        var files = GetFileByPath(skillPath, ".bytes");

        foreach (var file in files)
        {
            var path = skillPath + file;
            XSkillData data = DataIO.DeserializeEcsData<XSkillData>(path);
            var data1 = GetFileResData(path);
            if (data1 != null)
            {
                res.Add(data1);
            }

            foreach (var cameraShake in data.CameraShakeData)
            {
                var d = GetFileResData("Assets/BundleRes/" + cameraShake.Path + ".asset");
                if (d != null)
                {
                    res.Add(d);
                }
            }

            foreach (var special in data.SpecialActionData)
            {
                if (special.Type == 12 && special.SubType == 5)
                {
                    var pathSmaller = special.StringParameter1;
                    var key = pathSmaller.Split('/').Last();
                    if (_SpecialRadialBlurV2Up2LowerDic.ContainsKey(key))
                    {
                        Debug.Log($"{key}");
                        var d = GetFileResData(_SpecialRadialBlurV2Up2LowerDic[key]);
                        if (d != null)
                        {
                            Debug.Log($"--{key}");
                            res.Add(d);

                        }
                    }

                }
  
            }
            foreach (var cameraMotion in data.CameraMotionData)
            {
                var d = GetAnimationResData("Assets/BundleRes/" + cameraMotion.MotionPath + ".anim");
                if (d != null)
                {
                    res.Add(d);
                }
            }



            foreach (var fx in data.BulletData)
            {
                if (fx == null || fx.BulletPath == null)
                {
                    continue;
                }

                if (_fxLower2UpDic.ContainsKey(fx.BulletPath.ToLower()))
                {
                    var fxName = _fxLower2UpDic[fx.BulletPath.ToLower()];
                    allFX.Add(fxName);
                }
            }

            foreach (var fx in data.WarningData)
            {
                if (fx == null || fx.FxPath == null)
                {
                    continue;
                }

                if (_fxLower2UpDic.ContainsKey(fx.FxPath.ToLower()))
                {
                    var fxName = _fxLower2UpDic[fx.FxPath.ToLower()];
                    allFX.Add(fxName);
                }
            }

            foreach (var fx in data.FxData)
            {
                if (fx == null || fx.FxPath == null)
                {
                    continue;
                }

                if (_fxLower2UpDic.ContainsKey(fx.FxPath.ToLower()))
                {
                    var fxName = _fxLower2UpDic[fx.FxPath.ToLower()];
                    allFX.Add(fxName);
                }
            }
        }

        #endregion

        #region 计算受击

        files = GetFileByPath(reactPath, ".bytes");
        foreach (var file in files)
        {
            var path = reactPath + file;
            if (!File.Exists(path))
                continue;
            XReactData data = DataIO.DeserializeData<XReactData>(path);
            var data1 = GetFileResData(path);
            if (data1 != null)
            {
                res.Add(data1);
            }

            if (data.Fx == null)
            {
                continue;
            }

            foreach (var fx in data.Fx)
            {
                var fxNames = fx.Fx.Split('/');
                var fxName = fxNames[fxNames.Length - 1];
                if (_fxLower2UpDic.ContainsKey(fxName.ToLower()))
                {
                    fxName = _fxLower2UpDic[fxName.ToLower()];
                    allFX.Add(fxName);
                }
            }
        }

        #endregion

        for (int i = 0; i < hitFile.count; i++)
        {
            var hit = hitFile.GetValue(i, 1);
            if (hit == null)
                continue;
            if (!_skillHitPathDic.ContainsKey(hit.ToLower()))
            {
                continue;
            }

            var hitPath = _skillHitPathDic[hit.ToLower()];
            XHitData data = DataIO.DeserializeEcsData<XHitData>(hitPath);
            var data1 = GetFileResData(hitPath);
            if (data1 != null)
            {
                res.Add(data1);
            }

            if (data.FxData == null)
            {
                continue;
            }

            foreach (var fx in data.FxData)
            {
                if (fx == null)
                {
                    continue;
                }

                if (_fxLower2UpDic.ContainsKey(fx.FxPath.ToLower()))
                {
                    var fxName = _fxLower2UpDic[fx.FxPath.ToLower()];
                    allFX.Add(fxName);
                }
            }
        }

        res.AddRange(GetFxs(allFX));

        return res;
    }

    private static List<ResData> GetFxMaterialAndTex(GameObject gb)
    {
        var res = new List<ResData>();
        var renderers = gb.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            var materials = renderer.sharedMaterials;
            foreach (var material in materials)
            {
                var matdata = GetMatResData(material);
                if (matdata == null)
                {
                    continue;
                }

                res.Add(matdata);
                var texDatas = GetCertainMaterialTexturePaths(material);
                res.AddRange(texDatas);
            }
        }

        return res;
    }

    private static ResData GetTexResData(Texture tex)
    {
        var texPath = AssetDatabase.GetAssetPath(tex);
        if (texPath == "Resources/unity_builtin_extra" || texPath == null || texPath == "") return null;
        var texGuid = AssetDatabase.AssetPathToGUID(texPath);
        var texSize = new FileInfo(texPath).Length;
        var texData = new ResData()
        {
            Guid = texGuid,
            DataSize = (uint) texSize,
            Path = texPath,
        };
        return texData;
    }

    private static ResData GetPrefabResData(String path, out GameObject gb)
    {
        if (!File.Exists(path))
        {
            gb = null;
            return null;
        }

        gb = PrefabUtility.LoadPrefabContents(path);
        var guid = AssetDatabase.AssetPathToGUID(path);
        var size = new FileInfo(path).Length;
        var data = new ResData()
        {
            Guid = guid,
            DataSize = (uint) size,
            Path = path,
        };
        return data;
    }

    private static ResData GetMatResData(Material material)
    {
        if (material == null) return null;
        var size = Profiler.GetRuntimeMemorySize(material);

        var path = AssetDatabase.GetAssetPath(material);
        if (path == "Resources/unity_builtin_extra") return null;

        var guid = AssetDatabase.AssetPathToGUID(path);
        var matdata = new ResData
        {
            Guid = guid,
            DataSize = (uint) size,
            Path = path,
        };
        return matdata;
    }

    private static ResData GetFileResData(String path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var size = new FileInfo(path).Length;
        var guid = AssetDatabase.AssetPathToGUID(path);
        var data = new ResData()
        {
            Guid = guid,
            DataSize = (uint) size,
            Path = path,
        };
        return data;
    }

    private static ResData GetAnimationResData(String path)
    {
        var animation = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        // 排除controller

        var size = Profiler.GetRuntimeMemorySize(animation);
        var guid = AssetDatabase.AssetPathToGUID(path);
        var animationData = new ResData
        {
            Guid = guid,
            DataSize = (uint) size,
            Path = path,
        };
        return animationData;
    }

    private static void WriteResToFile(String filePath, Dictionary<String, Dictionary<String, ResData>> AllDatas,
        Dictionary<String, LevelData> levelDatas)
    {
        FileStream fs = new FileStream(filePath, FileMode.Create);
        foreach (var character in AllDatas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Level:" + character.Key + "\n");
            if (levelDatas.ContainsKey(character.Key))
            {
                sb.Append("MonsterAndSwiftChar:");
                foreach (var monster in levelDatas[character.Key].MonsterSet)
                {
                    sb.Append(monster + ",");
                }
            }

            sb.Append("\n");

            if (levelDatas.ContainsKey(character.Key))
            {
                sb.Append("CutSceneChar:");
                foreach (var monster in levelDatas[character.Key].CutSceneCharacterSet)
                {
                    sb.Append(monster + ",");
                }
            }

            sb.Append("\n");

            foreach (var data in character.Value)
            {
                sb.Append(
                    $"{data.Value.Path},size:{data.Value.DataSize / (1024f * 1024f)}MB,isRepeat:{data.Value.IsRepeat}\n");
            }

            byte[] bytes = new UTF8Encoding().GetBytes(sb.ToString());
            fs.Write(bytes, 0, bytes.Length);
        }

        fs.Close();
    }

    private static void WriteResToFile(String filePath, Dictionary<String, Dictionary<String, ResData>> AllDatas)
    {
        FileStream fs = new FileStream(filePath, FileMode.Create);
        foreach (var character in AllDatas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Character:" + character.Key + "\n");
            foreach (var data in character.Value)
            {
                sb.Append(
                    $"{data.Value.Path},size:{data.Value.DataSize / (1024f * 1024f)}MB,isRepeat:{data.Value.IsRepeat}\n");
            }

            byte[] bytes = new UTF8Encoding().GetBytes(sb.ToString());
            fs.Write(bytes, 0, bytes.Length);
        }

        fs.Close();
    }

    private static void GetResSize(Dictionary<String, Dictionary<String, ResData>> allData)
    {
        foreach (var s_datas in allData)
        {
            long size = 0;
            foreach (var data in s_datas.Value)
            {
                size += data.Value.DataSize;
            }

            Debug.Log($"{s_datas.Key} size:{size / (1024f * 1024f)} M");
        }
    }

    private static List<ResData> GetAllAnimation(String basePath)

    {
        var res = new List<ResData>();
        var files = GetFileByPath(basePath);
        foreach (var file in files)
        {
            var path = basePath + file;
            if (!path.EndsWith(".anim"))
            {
                continue;
            }

            var animationData = GetAnimationResData(path);

            if (animationData != null)
            {
                res.Add(animationData);
            }
        }

        return res;
    }

    private static List<ResData> GetAllMesh(GameObject gb)
    {
        var res = new List<ResData>();
        var filters = gb.GetComponentsInChildren<MeshFilter>(true);
        var skinMeshRenders = gb.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if ((filters == null || filters.Length == 0) &&
            (skinMeshRenders == null || skinMeshRenders.Length == 0))
        {
            return res;
        }

        var meshList = new List<Mesh>();
        if (filters != null)
            foreach (var filter in filters)
            {
                var mesh = filter.sharedMesh;
                if (mesh == null) continue;
                meshList.Add(mesh);
            }

        foreach (var render in skinMeshRenders)
        {
            var mesh = render.sharedMesh;
            if (mesh == null) continue;
            meshList.Add(mesh);
        }

        foreach (var mesh in meshList)
        {
            var size = Profiler.GetRuntimeMemorySize(mesh);
            var path = AssetDatabase.GetAssetPath(mesh);
            var guid = AssetDatabase.AssetPathToGUID(path);
            var meshData = new ResData
            {
                Guid = guid,
                DataSize = (uint) size,
                Path = path,
            };
            res.Add(meshData);
        }

        return res;
    }

    private static void AddResDataToDic(String name, ResData data,
        Dictionary<String, Dictionary<String, ResData>> dstDic)
    {
        if (data == null || data.Guid == null) return;
        if (!dstDic.ContainsKey(name))
        {
            dstDic[name] = new Dictionary<String, ResData>();
        }

        if (!dstDic[name].ContainsKey(data.Guid))
        {
            dstDic[name].Add(data.Guid, data);
        }
    }

    private static void AddCacheResDataToDic(ResData data, Dictionary<String, ResData> cacheDic)
    {
        if (data == null || data.Guid == null)
        {
            return;
        }

        if (cacheDic.ContainsKey(data.Guid))
        {
            data.IsRepeat = true;
        }
        else
        {
            data.IsRepeat = false;
            cacheDic.Add(data.Guid, data);
        }
    }

    private static void AddRangeResDataToDic(String name, List<ResData> datas,
        Dictionary<String, Dictionary<String, ResData>> dstDic)
    {
        foreach (var data in datas)
        {
            AddResDataToDic(name, data, dstDic);
        }
    }

    private static void AddRangeResDataToCacheDic(List<ResData> datas, Dictionary<String, ResData> cacheDic)
    {
        foreach (var data in datas)
        {
            AddCacheResDataToDic(data, cacheDic);
        }
    }

    private static List<ResData> GetAllMaterial(GameObject go)
    {
        var res = new List<ResData>();
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0) return res;
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null) continue;
                var matdata = GetMatResData(material);
                res.Add(matdata);
                var texDatas = GetCertainMaterialTexturePaths(material);
                res.AddRange(texDatas);
            }
        }

        return res;
    }

    private static List<ResData> GetCertainMaterialTexturePaths(Material mat)
    {
        List<ResData> results = new List<ResData>();

        Shader shader = mat.shader;
        for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); ++i)
        {
            if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                String propertyName = ShaderUtil.GetPropertyName(shader, i);
                Texture tex = mat.GetTexture(propertyName);
                if (tex == null)
                {
                    continue;
                }

                var data = GetTexResData(tex);
                if (data == null) continue;
                results.Add(data);
            }
        }

        return results;
    }

    private static List<String> GetFileByPath(String path, String endWith = "")
    {
        if (!Directory.Exists(path))
        {
            return new List<String>();
        }

        var res = new List<String>();
        DirectoryInfo direction = new DirectoryInfo(path);
        FileInfo[] files = direction.GetFiles("*");
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith(".meta"))
                continue;
            if (endWith != "" && !files[i].Name.EndsWith(endWith))
                continue;
            res.Add(files[i].Name);
        }

        return res;
    }
}
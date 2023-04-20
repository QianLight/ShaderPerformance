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
using UnityEngine.CFUI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using XHitData = ClientEcsData.XHitData;
using XSkillData = ClientEcsData.XSkillData;
using PreLower2upDic =
    System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.List<System.String>>;
using GUID = System.String;

public static class ResStatisticEditor
{
    public struct NameAndFilePath
    {
        public String name;
        public String path;
    }

    private class ResNode
    {
        public GUID GUID;

        public String Path;

        private ResourcePackageTypeEnum fileKind;

        public ResourcePackageTypeEnum FileKind
        {
            get => fileKind;
            set
            {
                if (fileKind > value)
                {
                    fileKind = value;
                    foreach (var node in Child)
                    {
                        node.FileKind = value;
                    }
                }
            }
        }

        public List<ResNode> Child;

        private ResNode(GUID guid, String path, ResourcePackageTypeEnum fileKind)
        {
            this.fileKind = fileKind;
            GUID = guid;
            Path = path;
            Child = new List<ResNode>();
            AllResDictionary.Add(guid, this);
        }

        public static ResNode GetOrCreateResNode(GUID guid, String path, ResourcePackageTypeEnum FileKind)
        {
            if (AllResDictionary.ContainsKey(guid))
            {
                AllResDictionary[guid].FileKind = FileKind;
                return AllResDictionary[guid];
            }

            return new ResNode(guid, path.ToLower(), FileKind);
        }
    }

    private static Dictionary<GUID, ResNode> AllResDictionary;


    private static readonly String FxPathPre = "Assets/BundleRes/Runtime/SFX/";
    private static readonly String TimeLinePathPre = "Assets/BundleRes/TimeLine/";
    private static readonly String PrefabPre = "Assets/BundleRes/Prefabs/";
    private static readonly String RuntimePrefabPre = "Assets/BundleRes/Runtime/Prefab/";

    private static readonly String SpecialActionPath = "Assets/BundleRes/SpecialAction/";
    private static readonly String SkillPathPre = "Assets/BundleRes/SkillPackage/";
    private static readonly String ReactPathPre = "Assets/BundleRes/ReactPackage/";
    private static readonly String HitPathPre = "Assets/BundleRes/HitPackage/";
    private static readonly String AnimationPathRre = "Assets/BundleRes/Animation/";
    private static readonly String ScenePathPre = "Assets/BundleRes/Scene/";
    private static readonly String TableFilePath = "Assets/BundleRes/Table";
    private static readonly String UIBgImagePre = "Assets/BundleRes/UIBackground";


    // private stat
    private static PreLower2upDic _preFxDictionary;
    private static PreLower2upDic _preSpecialRadialBlurDictionary;
    private static PreLower2upDic _preTimelineDictionary;
    private static PreLower2upDic _prePrefabDictionary;
    private static PreLower2upDic _preSkillHitDictionary;
    private static PreLower2upDic _preUIBgImageDictionary;

    private static List<uint> _CharacterInner;
    private static List<uint> _CharacterOutter;
    private static List<uint> _LevelInner;
    private static List<uint> _LevelOutter;


    private static Dictionary<String, List<ResNode>> _CharacterRes;
    private static Dictionary<String, List<ResNode>> _SceneRes;
    private static Dictionary<String, List<ResNode>> _UIRes;

    // 预计算
    static void PreGetResDicUp2Low()
    {
        // fx Init
        _preFxDictionary = new PreLower2upDic();
        var fxdata = GetAllFileByType(FxPathPre, ".prefab");
        fxdata.ForEach(path =>
        {
            if (_preFxDictionary.ContainsKey(path.name))
            {
                _preFxDictionary[path.name].Add(path.path);
            }
            else
            {
                _preFxDictionary.Add(path.name, new List<string>() {path.path});
            }
        });

        // timeLine init
        _preTimelineDictionary = new PreLower2upDic();
        var timeData = GetAllFileByType(TimeLinePathPre, ".prefab");
        timeData.ForEach(path =>
        {
            if (_preTimelineDictionary.ContainsKey(path.name))
            {
                _preTimelineDictionary[path.name].Add(path.path);
            }
            else
            {
                _preTimelineDictionary.Add(path.name, new List<string>() {path.path});
            }
        });

        // prefab init
        _prePrefabDictionary = new PreLower2upDic();
        var prefabData = GetAllFileByType(PrefabPre, ".prefab");
        prefabData.ForEach(path =>
        {
            if (_prePrefabDictionary.ContainsKey(path.name))
            {
                _prePrefabDictionary[path.name].Add(path.path);
            }
            else
            {
                _prePrefabDictionary.Add(path.name, new List<string>() {path.path});
            }
        });

        // runtimeprefab init
        var runtimeprefabData = GetAllFileByType(RuntimePrefabPre, ".prefab");
        runtimeprefabData.ForEach(path =>
        {
            if (_prePrefabDictionary.ContainsKey(path.name))
            {
                _prePrefabDictionary[path.name].Add(path.path);
            }
            else
            {
                _prePrefabDictionary.Add(path.name, new List<string>() {path.path});
            }
        });


        // camera fx init 
        _preSpecialRadialBlurDictionary = new PreLower2upDic();
        var radiusBlur = SpecialActionPath + "RadialBlurV2/Data/";
        var radiusBlurData = GetAllFileByType(radiusBlur, ".asset");
        radiusBlurData.ForEach(path =>
        {
            if (_preSpecialRadialBlurDictionary.ContainsKey(path.name))
            {
                _preSpecialRadialBlurDictionary[path.name].Add(path.path);
            }
            else
            {
                _preSpecialRadialBlurDictionary.Add(path.name, new List<string>() {path.path});
            }
        });
        // Hit
        _preSkillHitDictionary = new PreLower2upDic();
        var hitData = GetAllFileByType(HitPathPre, ".bytes");
        hitData.ForEach(path =>
        {
            if (_preSkillHitDictionary.ContainsKey(path.name))
            {
                _preSkillHitDictionary[path.name].Add(path.path);
            }
            else
            {
                _preSkillHitDictionary.Add(path.name, new List<string>() {path.path});
            }
        });
        // UI大图
        _preUIBgImageDictionary = new PreLower2upDic();
        var bgImages = GetAllFileByType(UIBgImagePre, new List<string>() {".png", ".jpg", ".jpeg", ".tga"});
        bgImages.ForEach(path =>
        {
            if (_preUIBgImageDictionary.ContainsKey(path.name))
            {
                _preUIBgImageDictionary[path.name].Add(path.path);
            }
            else
            {
                _preUIBgImageDictionary.Add(path.name, new List<string>() {path.path});
            }
        });
    }

    public static void GenPreData(DelConfig assets)
    {
        _CharacterRes = new Dictionary<string, List<ResNode>>();
        _SceneRes = new Dictionary<string, List<ResNode>>();
        _UIRes = new Dictionary<string, List<ResNode>>();

        AllResDictionary = new Dictionary<GUID, ResNode>();
        PreGetResDicUp2Low();
        foreach (var ui in assets.uiDatas)
        {
            ClacUI(ui.Key, ui.Value);
        }

        foreach (var hit in assets.behitDatas)
        {
            var hitPath = HitPathPre + hit.Key;
            ClacCharacterHit(hitPath, hit.Value);
        }

        foreach (var skill in assets.skillDatas)
        {
            var skillPath = SkillPathPre + skill.Key;
            ClacCharacterSkill(skillPath, skill.Value);
        }

        foreach (var react in assets.reactDatas)
        {
            var reactPath = ReactPathPre + react.Key;
            ClacCharacterReact(reactPath, react.Value);
        }

        foreach (var level in assets.levelDatas)
        {
            ClacLevel(level.Key, false, level.Value);
        }

        foreach (var c in assets.characterDatas)
        {
            ClacCharacterByCharId(c.Key, false, c.Value);
        }

        foreach (var timeLine in assets.timeLineDatas)
        {
            GetTimeLineRes(timeLine.Key, timeLine.Value);
        }

        foreach (var entity in assets.entityDatas)
        {
            ClacEntityByPresentId(entity.Key, false, entity.Value);
        }

        foreach (var scene in assets.sceneDatas)
        {
            var scenepath = scene.Key;
            var sceneName = scenepath.Split('/').Last().Replace(".unity", "");
            var sceneBasePath = ScenePathPre + scenepath.Replace(sceneName + ".unity", "");
            ClacScene(sceneBasePath, sceneName, scene.Value);
        }

        WriteSampleResult("ResStaticRes/sampleRes.txt");
    }

    public static void GenPreData(PackageConfig assets)
    {
        _CharacterRes = new Dictionary<string, List<ResNode>>();
        _SceneRes = new Dictionary<string, List<ResNode>>();
        _UIRes = new Dictionary<string, List<ResNode>>();
        AllResDictionary = new Dictionary<GUID, ResNode>();
        PreGetResDicUp2Low();
        

        for (int i = 0; i <= (int) ResourcePackageTypeEnum.Unknown; i++)
        {
            foreach (var ui in assets.uiDatas)
            {
                if (ui.Value == (ResourcePackageTypeEnum)i)
                {
                    ClacUI(ui.Key, ui.Value);
                }
            }
            
            foreach (var level in assets.levelDatas)
            {
                if (level.Value == (ResourcePackageTypeEnum)i)
                {
                    ClacLevel(level.Key, true, level.Value);
                }
            }
            
            foreach (var c in assets.characterDatas)
            {
                if (c.Value == (ResourcePackageTypeEnum)i)
                {
                    ClacCharacterByCharId(c.Key, true, c.Value);
                }
            }
            
            foreach (var timeLine in assets.timeLineDatas)
            {
                if (timeLine.Value == (ResourcePackageTypeEnum) i)
                {
                    var name = timeLine.Key.Split('/').Last();
                    GetTimeLineRes(name, timeLine.Value);
                }
            }

            foreach (var entity in assets.entityDatas)
            {
                if (entity.Value == (ResourcePackageTypeEnum) i)
                {
                    ClacEntityByPresentId(entity.Key, true, entity.Value);
                }
            }
        }


        WriteSampleResult("ResStaticRes/sampleRes.txt");
    }

    private static void ClacUI(string sysName, ResourcePackageTypeEnum FileKind)
    {
        if (_UIRes.ContainsKey(sysName))
        {
            return;
        }

        var path = "Assets/BundleRes/UI/OPsystemprefab/";
        var sysPath = path + sysName;
        var uiRes = new List<ResNode>(10);
        _UIRes.Add(sysName, uiRes);
        var allPrefab = GetAllFileByType(sysPath, ".prefab");
        foreach (var prefab in allPrefab)
        {
            var res = GetPrefabRes(prefab.path, out var go, FileKind, true);
            uiRes.Add(res);
            TryUnloadPrefabContents(go);
        }
    }

    static void ClacUIs(List<String> uiFolder, ResourcePackageTypeEnum FileKind)
    {
        foreach (var sysName in uiFolder)
        {
            ClacUI(sysName, FileKind);
        }
    }

    private static void TryUnloadPrefabContents(GameObject go)
    {
        try
        {
            PrefabUtility.UnloadPrefabContents(go);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private static void GetResNode(ResNode parent, ref List<ResNode> resNodes, HashSet<GUID> cahce)
    {
        if (resNodes == null)
        {
            resNodes = new List<ResNode>(1000);
            if (parent == null)
            {
                return;
            }

            if (cahce.Contains(parent.GUID))
            {
                return;
            }

            cahce.Add(parent.GUID);
            resNodes.Add(parent);
        }


        foreach (var node in parent.Child)
        {
            if (cahce.Contains(node.GUID))
            {
                continue;
            }

            cahce.Add(node.GUID);
            resNodes.Add(node);
            GetResNode(node, ref resNodes, cahce);
        }
    }

    private static void WriteSampleResult(String filePath)
    {
        if (!System.IO.Directory.Exists("ResStaticRes"))
        {
            Directory.CreateDirectory("ResStaticRes");
        }

        FileStream fs = new FileStream(filePath, FileMode.Create);
        StringBuilder sb = new StringBuilder();
        foreach (var data in AllResDictionary.Values)
        {
            if (string.IsNullOrEmpty(data.Path)) continue;
            sb.Append($"{data.Path},{data.FileKind}\n");
            byte[] bytes = new UTF8Encoding().GetBytes(sb.ToString());
            fs.Write(bytes, 0, bytes.Length);
            sb.Clear();
        }

        fs.Close();
    }

    private static void WriteResToFile(String filePath, Dictionary<string, List<ResNode>> datas, FileMode mode)
    {
        FileStream fs = new FileStream(filePath, mode);
        foreach (var data in datas)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("---:" + data.Key + "\n");
            var cahce = new HashSet<GUID>();
            foreach (var value in data.Value)
            {
                List<ResNode> res = null;
                GetResNode(value, ref res, cahce);
                foreach (var d in res)
                {
                    sb.Append($"{d.Path},{d.FileKind}\n");
                    byte[] bytes = new UTF8Encoding().GetBytes(sb.ToString());
                    fs.Write(bytes, 0, bytes.Length);
                    sb.Clear();
                }
            }
        }

        fs.Close();
    }


    private static string GetCharacterKeyByPresentId(uint PresentId)
    {
        var entity = XEntityPresentationReader.GetData(PresentId);
        var name = entity.PresentID + entity.Name;
        return name;
    }

    private static string GetSceneKeyById(uint sceneId)
    {
        var sceneData = MapListReader.MapList.GetByMapID(sceneId);
        if (sceneData == null)
        {
            return "";
        }

        var name = sceneData.MapID + sceneData.Comment;
        return $"{name}";
    }

    private static ResNode ClacCharacterUIImage(uint presentId, ResourcePackageTypeEnum FileType)
    {
        var table = new PartnerShowTable();
        XTableReader.ReadFile(@"Table/PartnerShow", table);
        var data = table.GetByID(presentId);

        // DEL
        // MiniIconAwake
        // PersonalPartnerAnimation
        // UISceneBg
        // StarUpVoice
        // ShipRoomIdlePose
        // ShipPlottingRoomShow
        // ShipLabRoomShow
        // ShipCookRoomShow
        // SkillBg

        if (data == null)
        {
            return null;
        }

        // UI BG
        var halfImageName = data.HalfImage.Split('/').Last();
        _preUIBgImageDictionary.TryGetValue(halfImageName, out var halfImage);

        var cardImageName = data.CardImage.Split('/').Last();
        _preUIBgImageDictionary.TryGetValue(cardImageName, out var cardImage);

        var largeImageName = data.LargeImage.Split('/').Last();
        _preUIBgImageDictionary.TryGetValue(largeImageName, out var largeImage);

        var lotteryImageName = data.LotteryImage.Split('/').Last();
        _preUIBgImageDictionary.TryGetValue(lotteryImageName, out var lotteryImage);

        var mainListIconName = data.MainListIcon.Split('/').Last();
        _preUIBgImageDictionary.TryGetValue(mainListIconName, out var mainListIcon);

        var mainListIconAwakeName = data.MainListIconAwake.Split('/').Last();
        _preUIBgImageDictionary.TryGetValue(mainListIconAwakeName, out var mainListIconAwake);

        var smallSceneBgImgName = data.SmallSceneBgImg.Split('/').Last();
        _preUIBgImageDictionary.TryGetValue(smallSceneBgImgName, out var smallSceneBgImg);

        var uiShowTexName = data.UIShowTexture.Split('/').Last();
        _preUIBgImageDictionary.TryGetValue(uiShowTexName, out var uiShowTex);

        var skillRoleBgName = data.SkillRoleBg.Split('/').Last();
        _preUIBgImageDictionary.TryGetValue(skillRoleBgName, out var skillRoleBg);

        var uiBgList = new List<string>();
        if (halfImage != null)
        {
            uiBgList.AddRange(halfImage);
        }

        if (cardImage != null)
        {
            uiBgList.AddRange(cardImage);
        }

        if (largeImage != null)
        {
            uiBgList.AddRange(largeImage);
        }

        if (lotteryImage != null)
        {
            uiBgList.AddRange(lotteryImage);
        }

        if (mainListIcon != null)
        {
            uiBgList.AddRange(mainListIcon);
        }

        if (mainListIconAwake != null)
        {
            uiBgList.AddRange(mainListIconAwake);
        }

        if (smallSceneBgImg != null)
        {
            uiBgList.AddRange(smallSceneBgImg);
        }

        if (uiShowTex != null)
        {
            uiBgList.AddRange(uiShowTex);
        }

        if (skillRoleBg != null)
        {
            uiBgList.AddRange(skillRoleBg);
        }

        foreach (var ui in uiBgList)
        {
            GetFileResAndCheckIsExist(ui, out var node, FileType);
        }

        //spine
        String pre = "Assets/BundleRes/";
        var path1 = pre + data.SpineImage + ".asset";
        var path2 = pre + data.SpineImage + ".atlas";
        GetFileResAndCheckIsExist(path1, out var _, FileType);
        GetFileResAndCheckIsExist(path2, out var _, FileType);
        path1 = pre + data.SpineImageAwake + ".asset";
        path2 = pre + data.SpineImageAwake + ".atlas";
        GetFileResAndCheckIsExist(path1, out var _, FileType);
        GetFileResAndCheckIsExist(path2, out var _, FileType);
        return null;
    }

    private static void ClacEntityByPresentId(uint PresentId, bool allData, ResourcePackageTypeEnum FileKind)
    {
        var entity = XEntityPresentationReader.GetData(PresentId);
        if (entity == null)
        {
            return;
        }

        var name = GetCharacterKeyByPresentId(PresentId);
        if (!_CharacterRes.ContainsKey(name))
        {
            _CharacterRes.Add(name, new List<ResNode>(30));
        }


        // 角色animation 直接统计文件夹
        var animationPath = AnimationPathRre + entity.AnimLocation;
        var anims = GetAllFileByType(animationPath, ".anim");
        foreach (var anim in anims)
        {
            var exist = GetFileResAndCheckIsExist(anim.path, out var data, FileKind);
            if (!exist)
            {
                _CharacterRes[name].Add(data);
            }
        }

        if (allData)
        {
        }

        // 角色预制体
        var meshSuffix = new List<String>() {"", "_lod1", "_lod2"};
        foreach (var suffix in meshSuffix)
        {
            var key = entity.Prefab.ToLower() + suffix;
            if (!_prePrefabDictionary.ContainsKey(key))
            {
                continue;
            }

            var paths = _prePrefabDictionary[key];
            foreach (var path in paths)
            {
                var data = GetPrefabRes(path, out var _, FileKind);
                _CharacterRes[name].Add(data);
            }
        }

        if (allData)
        {
            // 角色技能
            var skillPath = SkillPathPre + entity.SkillLocation;

            var files = GetAllFileByType(skillPath, ".bytes");
            foreach (var file in files)
            {
                var res = ClacCharacterSkill(file.path, FileKind);
                if (res == null)
                {
                    continue;
                }

                _CharacterRes[name].Add(res);
            }

            // 角色react
            var reactPath = ReactPathPre + entity.BehitLocation;

            files = GetAllFileByType(reactPath, ".bytes");
            foreach (var file in files)
            {
                var res = ClacCharacterReact(file.path, FileKind);
                if (res == null)
                {
                    continue;
                }

                _CharacterRes[name].Add(res);
            }

            // 角色Hit
            var hitfiles = entity.BeHit;
            for (int i = 0; i < hitfiles.count; i++)
            {
                var hit = hitfiles.GetValue(i, 1);
                if (hit == null)
                    continue;
                if (!_preSkillHitDictionary.ContainsKey(hit.ToLower()))
                {
                    continue;
                }

                var hitPaths = _preSkillHitDictionary[hit.ToLower()];
                foreach (var hitPath in hitPaths)
                {
                    var res = ClacCharacterHit(hitPath, FileKind);
                    if (res == null)
                    {
                        continue;
                    }

                    _CharacterRes[name].Add(res);
                }
            }
        }
    }

    // 统计角色
    private static void ClacCharacterByCharId(uint charId, bool allData, ResourcePackageTypeEnum FileKind)
    {
        var partnerInfo = PartnerInfoReader.PartnerInfo.GetByID(charId);
        if (partnerInfo == null)
        {
            Debug.LogError(charId + " partner is null");
            return;
        }

        ClacEntityByPresentId(partnerInfo.PresentId, allData, FileKind);
        ClacCharacterUIImage(partnerInfo.PresentId, FileKind);
    }

    // 统计所有角色
    private static void ClacAllCharacter(List<uint> chars, bool divide, ResourcePackageTypeEnum FileKind)
    {
        foreach (var @char in chars)
        {
            ClacCharacterByCharId(@char, divide, FileKind);
        }
    }

    // 统计角色skill 
    private static ResNode ClacCharacterSkill(String skillPath, ResourcePackageTypeEnum FileKind)
    {
        XSkillData data;
        try
        {
            data = DataIO.DeserializeEcsData<XSkillData>(skillPath);
        }
        catch (Exception e)
        {
            return null;
        }

        var exist = GetFileResAndCheckIsExist(skillPath, out var res, FileKind);
        if (res == null)
        {
            return null;
        }


        foreach (var cameraShake in data.CameraShakeData)
        {
            exist = GetFileResAndCheckIsExist("Assets/BundleRes/" + cameraShake.Path + ".asset", out var d, FileKind);

            if (d != null)
            {
                res.Child.Add(d);
            }
        }

        foreach (var special in data.SpecialActionData)
        {
            if (special.Type == 12 && special.SubType == 5)
            {
                var pathSmaller = special.StringParameter1;
                var key = pathSmaller.Split('/').Last();
                if (_preSpecialRadialBlurDictionary.ContainsKey(key))
                {
                    var specials = _preSpecialRadialBlurDictionary[key];
                    foreach (var special1 in specials)
                    {
                        exist = GetFileResAndCheckIsExist(special1, out var d, FileKind);
                        if (d != null)
                        {
                            res.Child.Add(d);
                        }
                    }
                }
            }
        }

        foreach (var cameraMotion in data.CameraMotionData)
        {
            exist = GetFileResAndCheckIsExist("Assets/BundleRes/" + cameraMotion.MotionPath + ".anim", out var d,
                FileKind);

            if (d != null)
            {
                res.Child.Add(d);
            }
        }


        foreach (var fx in data.BulletData)
        {
            if (fx == null || fx.BulletPath == null)
            {
                continue;
            }


            var d = GetFxData(fx.BulletPath, FileKind);
            if (d != null)
            {
                res.Child.AddRange(d);
            }
        }

        foreach (var fx in data.WarningData)
        {
            if (fx == null || fx.FxPath == null)
            {
                continue;
            }

            var d = GetFxData(fx.FxPath, FileKind);
            if (d != null)
            {
                res.Child.AddRange(d);
            }
        }

        foreach (var fx in data.FxData)
        {
            if (fx == null || fx.FxPath == null)
            {
                continue;
            }

            var d = GetFxData(fx.FxPath, FileKind);
            if (d != null)
            {
                res.Child.AddRange(d);
            }
        }

        return res;
    }

    // 统计角色hit 
    private static ResNode ClacCharacterHit(String hitPath, ResourcePackageTypeEnum FileKind)
    {
        XHitData data;
        try
        {
            data = DataIO.DeserializeEcsData<XHitData>(hitPath);
        }
        catch (Exception e)
        {
            return null;
        }

        var exist = GetFileResAndCheckIsExist(hitPath, out var res, FileKind);
        if (data == null)
        {
            return null;
        }

        foreach (var fx in data.FxData)
        {
            var d = GetAllSkillRes(fx.FxPath, FileKind);
            if (d != null)
            {
                res.Child.Add(d);
            }
        }

        return res;
    }

    // 统计角色react
    private static ResNode ClacCharacterReact(String reactPath, ResourcePackageTypeEnum FileKind)
    {
        if (!File.Exists(reactPath))
            return null;
        XReactData data;
        try
        {
            data = DataIO.DeserializeData<XReactData>(reactPath);
        }
        catch (Exception e)
        {
            return null;
        }
        if (data == null)
        {
            Debug.LogError("cant find react " + reactPath);
            return null;
        }

        var exist = GetFileResAndCheckIsExist(reactPath, out var res, FileKind);

        if (res == null)
        {
            return null;
        }

        if (data.Fx == null)
        {
            return null;
        }


        foreach (var fx in data.Fx)
        {
            var d = GetAllSkillRes(fx.Fx, FileKind);
            if (d != null)
            {
                res.Child.Add(d);
            }
        }

        return res;
    }

    // 统计文件类型
    private static bool GetFileResAndCheckIsExist(string path, out ResNode resNode, ResourcePackageTypeEnum FileKind)
    {
        if (!File.Exists(path))
        {
            resNode = null;
            return true;
        }

        GUID guid = AssetDatabase.AssetPathToGUID(path);
        if (AllResDictionary.ContainsKey(guid))
        {
            AllResDictionary[guid].FileKind = FileKind;
            resNode = AllResDictionary[guid];
            return true;
        }

        resNode = ResNode.GetOrCreateResNode(guid, path, FileKind);
        return false;
    }


    // 统计关卡

    private static void ClacLevel(uint sceneId, bool allData, ResourcePackageTypeEnum FileKind)
    {
        var sceneKey = GetSceneKeyById(sceneId);
        if (_SceneRes.ContainsKey(sceneKey))
        {
            return;
        }

        var sceneRes = new List<ResNode>(1000);
        _SceneRes.Add(sceneKey, sceneRes);

        var sceneData = MapListReader.MapList.GetByMapID(sceneId);
        if (sceneData == null)
        {
            return;
        }

        // 计算场景
        var sceneBasePath = ScenePathPre + sceneData.UnitySceneFile + "/";
        var sceneResNode = ClacScene(sceneBasePath, sceneData.UnitySceneFile, FileKind);
        sceneRes.Add(sceneResNode);
        // 获取关卡怪物
        if (allData)
        {
            var chars = GetMonsterBySceneData(sceneData);
            foreach (var charId in chars)
            {
                var key = GetCharacterKeyByPresentId(charId);
                if (!_CharacterRes.ContainsKey(key))
                {
                    ClacEntityByPresentId(charId, true, FileKind);
                }

                sceneRes.AddRange(_CharacterRes[key]);
            }
        }


        // 统计碰撞文件
        var blockPath = $"Assets/BundleRes/{sceneData.BlockFilePath}.mapheight";
        if (File.Exists(blockPath))
        {
            GetFileResAndCheckIsExist(blockPath, out var res, FileKind);
            if (res != null)
            {
                sceneRes.Add(res);
            }
        }

        String configPath = $"{TableFilePath}/{sceneData.LevelConfigFile}.cfg";

        GetFileResAndCheckIsExist(configPath, out var configRes, FileKind);
        if (configRes == null)
        {
            Debug.LogError("level config not exist " + configPath);
            return;
        }

        sceneRes.Add(configRes);
        // 统计关卡脚本
        GetLevelScriptByPath(configPath, configRes, FileKind);
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

    private static ResNode GetTimeLineRes(string name, ResourcePackageTypeEnum FileKind)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var timeLinePrefabs = new List<String>();
        var ok = _preTimelineDictionary.TryGetValue(name.ToLower(), out timeLinePrefabs);
        if (!ok) return null;
        var timeLinePrefab = timeLinePrefabs[0];
        var timeLinePath = timeLinePrefab.Replace(".prefab", ".playable");
        if (!File.Exists(timeLinePath) && !File.Exists(timeLinePrefab))
        {
            Debug.Log("Cutscene id not found:" + name);
            return null;
        }

        var resData = GetPrefabRes(timeLinePrefab, out var gameObject, FileKind);
        if (gameObject == null)
        {
            Debug.Log("Cutscene id not found:" + timeLinePrefab);
            return null;
        }

        GetFileResAndCheckIsExist(timeLinePath, out var timeLineRes, FileKind);
        if (timeLineRes != null)
        {
            resData.Child.Add(timeLineRes);
        }


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
                        resData.Child.AddRange(GetAllMatAndTexFromGo(go, FileKind));
                        resData.Child.AddRange(GetAllMeshFromGo(go, FileKind));

                        continue;
                    }

                    if (prefab != null)
                    {
                        GameObject tempGo;
                        var data = GetPrefabRes(prefab, out tempGo, FileKind);
                        if (tempGo != null)
                        {
                            resData.Child.Add(data);
                            var temp1 = GetAllMeshFromGo(tempGo, FileKind);
                            var temp2 = GetAllMatAndTexFromGo(tempGo, FileKind);
                            TryUnloadPrefabContents(tempGo);

                            resData.Child.AddRange(temp1);
                            resData.Child.AddRange(temp2);
                        }
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
                var resFx = GetFxData(go.name, FileKind);
                resData.Child.AddRange(resFx);
            }
        }

        var comp = gameObject.GetComponentInChildren<OrignalTimelineData>();
        var playableDirector = gameObject.GetComponentInChildren<PlayableDirector>();
        var trackassets = new List<TrackAsset>();
        var fx_tracks = new List<ControlTrack>();
        var IterTimeline = new List<string>();
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

            TimelineAsset timelineAsset = playableDirector.playableAsset as TimelineAsset;
            if (timelineAsset != null && timelineAsset.markerTrack != null)
            {
                IEnumerable<IMarker> markers = timelineAsset.markerTrack.GetMarkers();
                foreach (var marker in markers)
                {
                    JumpTimelineSignal signal = marker as JumpTimelineSignal;
                    if (signal)
                    {
                        IterTimeline.Add(signal.m_timelineName);
                    }
                }
            }
        }

        foreach (var track in fx_tracks)
        {
            foreach (var clip in track.GetClipList())
            {
                var clipData = clip.asset as ControlPlayableAsset;
                if (clipData != null && clipData.fxData != null)
                {
                    var data = GetFxData(clipData.fxData.path, FileKind);
                    resData.Child.AddRange(data);
                }
            }
        }

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
                        GetFileResAndCheckIsExist(aniPath, out var data, FileKind);
                        if (data != null)
                        {
                            resData.Child.Add(data);
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
                    GetFileResAndCheckIsExist(prefab, out var data, FileKind);
                    if (data != null)
                    {
                        resData.Child.Add(data);
                    }

                    continue;
                }

                if (prefab != null)
                {
                    GameObject tempGo;
                    var data = GetPrefabRes(prefab, out tempGo, FileKind);

                    if (tempGo != null)
                    {
                        resData.Child.Add(data);
                        var temp1 = GetAllMeshFromGo(tempGo, FileKind);
                        var temp2 = GetAllMatAndTexFromGo(tempGo, FileKind);
                        TryUnloadPrefabContents(tempGo);

                        resData.Child.AddRange(temp1);
                        resData.Child.AddRange(temp2);
                    }
                }
            }
        }

        var chars = comp?.chars;
        if (chars == null) return resData;

        foreach (var c in chars)
        {
            var roleName = c.prefab.ToLower();
            if (!_prePrefabDictionary.ContainsKey(roleName))
            {
                Debug.Log("Cutscene id not found:" + roleName);
                continue;
            }

            var paths = _prePrefabDictionary[roleName];
            foreach (var path in paths)
            {
                var data = GetPrefabRes(path, out var go, FileKind);
                TryUnloadPrefabContents(go);

                resData.Child.Add(data);
            }
        }
        
        IterTimeline.ForEach(timelineName => resData.Child.Add(GetTimeLineRes(timelineName, FileKind)));

        TryUnloadPrefabContents(gameObject);

        return resData;
    }

    //统计关卡脚本
    private static void GetLevelScriptByPath(string configPath, ResNode parenet, ResourcePackageTypeEnum FileKind)
    {
        var res = parenet.Child;
        if (!File.Exists(configPath))
            return;
        GetFileResAndCheckIsExist(configPath, out var configRes, FileKind);

        if (configRes == null)
        {
            return;
        }

        List<LevelGraphData> grphDatas;
        try
        {
            grphDatas = DataIO.DeserializeData<LevelEditorData>(configPath)?.GraphDataList;

        }
        catch (Exception e)
        {
            return;
        }
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
                var timeLineData = GetTimeLineRes(resRealName, FileKind);
                if (timeLineData != null)
                {
                    parenet.Child.Add(timeLineData);
                }
            }
        }
    }

    private static List<uint> GetMonsterBySceneData(MapList.RowData mapData)
    {
        var resIds = new List<uint>();
        String path = $"{TableFilePath}/{mapData.LevelConfigFile}.cfg";
        List<LevelGraphData> grphDatas;
        try
        {
            grphDatas = DataIO.DeserializeData<LevelEditorData>(path)?.GraphDataList;

        }
        catch (Exception e)
        {
            return resIds;
        }
        if (grphDatas == null)
            return resIds;
        foreach (var graphData in grphDatas)
        {
            foreach (var monster in graphData.WaveData)
            {
                var staticData = XEntityStatisticsReader.Statistics.GetByID((uint) monster.SpawnID);
                if (staticData == null)
                {
                    continue;
                }

                var data = XEntityPresentationReader.GetData(staticData.PresentID);
                if (data == null)
                {
                    continue;
                }

                resIds.Add(data.PresentID);
            }
        }

        ScenePartnerTable table = new ScenePartnerTable();
        var scenePartnerTable = table.GetBySceneId((int) mapData.MapID);
        if (scenePartnerTable != null)
        {
            foreach (var partnerId in scenePartnerTable.Partner)
            {
                var partner = XEntityPresentationReader.GetData(partnerId);
                resIds.Add(partner.PresentID);
            }

            foreach (var partnerId in scenePartnerTable.Default)
            {
                var partner = XEntityPresentationReader.GetData(partnerId);
                resIds.Add(partner.PresentID);
            }
        }

        return resIds;
    }

    // 统计场景
    private static ResNode ClacScene(String sceneBasePath, String sceneName, ResourcePackageTypeEnum FileKind)
    {
        var scenePath = sceneBasePath + sceneName + ".unity";
        var exist = GetFileResAndCheckIsExist(scenePath, out var res, FileKind);

        if (exist)
        {
            return res;
        }

        // 统计光照贴图
        ClacSceneLightMap(res, FileKind);

        // 统计碰撞文件
        var senceGridDir = sceneBasePath + "SplitMeshFile/";
        var allGrads = GetAllFileByType(senceGridDir, ".prefab");
        foreach (var grad in allGrads)
        {
            var prefab = GetPrefabRes(grad.path, out var go, FileKind);
            TryUnloadPrefabContents(go);

            res.Child.Add(prefab);
        }

        // 统计关卡下模型材质与贴图
        var sceneNow = EditorSceneManager.OpenScene(scenePath);
        GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in gos)
        {
            res.Child.AddRange(GetAllMatAndTexFromGo(go, FileKind));
            res.Child.AddRange(GetAllMeshFromGo(go, FileKind));
        }

        // 反射探针
        foreach (var go in gos)
        {
            var reflectCubes = go.GetComponentsInChildren<ReflectionProbe>(true);
            foreach (var reflectCube in reflectCubes)
            {
                var tex = reflectCube.texture;
                var data = GetTexResData(tex, FileKind);
                if (data != null)
                {
                    res.Child.Add(data);
                }
            }
        }

        return res;
    }

    // 统计ligthMapData 
    private static void ClacSceneLightMap(ResNode resNode, ResourcePackageTypeEnum FileKind)
    {
        var lightmaps = LightmapSettings.lightmaps;
        foreach (var lightmap in lightmaps)
        {
            var temp = GetTexResData(lightmap.lightmapColor, FileKind);
            if (temp != null)
            {
                resNode.Child.Add(temp);
            }
        }
    }

    private static ResNode GetTexResData(Texture tex, ResourcePackageTypeEnum FileKind)
    {
        var texPath = AssetDatabase.GetAssetPath(tex);
        if (texPath == "Resources/unity_builtin_extra" || texPath == null || texPath == "") return null;
        var texGuid = AssetDatabase.AssetPathToGUID(texPath);
        return ResNode.GetOrCreateResNode(texGuid, texPath, FileKind);
    }


    // 统计预制体
    private static ResNode GetPrefabRes(string path, out GameObject outpre, ResourcePackageTypeEnum FileKind,
        bool needAltas = false)
    {
        outpre = null;

        if (!File.Exists(path))
        {
            return null;
        }


        var guid = AssetDatabase.AssetPathToGUID(path);

        if (AllResDictionary.ContainsKey(guid))
        {
            AllResDictionary[guid].FileKind = FileKind;
            return AllResDictionary[guid];
        }

        outpre = PrefabUtility.LoadPrefabContents(path);
        if (outpre == null)
        {
            return null;
        }

        var data = ResNode.GetOrCreateResNode(guid, path.ToLower(), FileKind);
        var matAndTexData = GetAllMatAndTexFromGo(outpre, FileKind);
        data.Child.AddRange(matAndTexData);
        var mesh = GetAllMeshFromGo(outpre, FileKind);
        data.Child.AddRange(mesh);
        if (needAltas)
        {
            var ImageComps = outpre.GetComponentsInChildren<CFImage>(true);
            foreach (var imgComp in ImageComps)
            {
                if (imgComp.atlasName != String.Empty)
                {
                    var bundleName = "ui/atlas/" + imgComp.atlasName + ".spriteatlas";
                    var atlas = ResNode.GetOrCreateResNode(bundleName, bundleName, FileKind);
                    data.Child.Add(atlas);
                }
            }

            var RawImageComps = outpre.GetComponentsInChildren<CFRawImage>(true);
            foreach (var imgComp in RawImageComps)
            {
                if (imgComp.m_TexPath != String.Empty)
                {
                    var tempPath = imgComp.m_TexPath;
                    var texName = tempPath.Split('/').Last();
                    _preUIBgImageDictionary.TryGetValue(texName, out var paths);
                    if (paths != null)
                    {
                        foreach (var p in paths)
                        {
                            GetFileResAndCheckIsExist(p, out var d, FileKind);
                            data.Child.Add(d);
                        }
                    }
                }
            }

            var uiFxs = outpre.GetComponentsInChildren<CFSpectorEffect>(true);
            foreach (var uifx in uiFxs)
            {
                var fxpath = uifx.assetPath;
                var fxData = GetFxData(fxpath, FileKind);
                data.Child.AddRange(fxData);
            }
        }

        return data;
    }


    // 统计meshRender
    private static List<ResNode> GetAllMatAndTexFromGo(GameObject go, ResourcePackageTypeEnum FileKind)
    {
        var res = new List<ResNode>();
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            var materials = renderer.sharedMaterials;
            foreach (var material in materials)
            {
                var resData = GetMatResData(material, FileKind);
                if (resData != null)
                {
                    res.Add(resData);
                }
            }
        }


        return res;
    }

    //统计材质
    private static ResNode GetMatResData(Material material, ResourcePackageTypeEnum FileKind)
    {
        if (material == null) return null;

        var path = AssetDatabase.GetAssetPath(material);
        if (path == "Resources/unity_builtin_extra") return null;
        var guid = AssetDatabase.AssetPathToGUID(path);
        if (AllResDictionary.ContainsKey(guid))
        {
            AllResDictionary[guid].FileKind = FileKind;
            return AllResDictionary[guid];
        }

        var data = ResNode.GetOrCreateResNode(guid, path, FileKind);
        var texDatas = GetCertainMaterialTexturePaths(material, FileKind);
        data.Child.AddRange(texDatas);
        return data;
    }

    // 统计材质贴图
    private static List<ResNode> GetCertainMaterialTexturePaths(Material mat, ResourcePackageTypeEnum FileKind)
    {
        List<ResNode> results = new List<ResNode>();

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

                var path = AssetDatabase.GetAssetPath(tex);
                if (path == "Resources/unity_builtin_extra" || path == null || path == "") continue;
                var guid = AssetDatabase.AssetPathToGUID(path);
                var data = ResNode.GetOrCreateResNode(guid, path, FileKind);
                results.Add(data);
            }
        }

        return results;
    }


    // 统计mesh
    private static List<ResNode> GetAllMeshFromGo(GameObject gb, ResourcePackageTypeEnum FileKind)
    {
        var res = new List<ResNode>();
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
            var path = AssetDatabase.GetAssetPath(mesh);
            var guid = AssetDatabase.AssetPathToGUID(path);
            res.Add(ResNode.GetOrCreateResNode(guid, path, FileKind));
        }

        return res;
    }

    // 统计角色所有技能脚本
    private static ResNode GetAllSkillRes(String path, ResourcePackageTypeEnum FileKind)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        GUID guid = AssetDatabase.AssetPathToGUID(path);
        if (!AllResDictionary.ContainsKey(guid))
        {
            AllResDictionary[guid].FileKind = FileKind;
            return AllResDictionary[guid];
        }

        var exist = GetFileResAndCheckIsExist(path, out var res, FileKind);
        if (res == null)
        {
            return null;
        }

        XSkillData data;
        try
        {
            data = DataIO.DeserializeEcsData<XSkillData>(path);
        }
        catch (Exception e)
        {
            return res;
        }

        if (data == null)
        {
            return res;
        }

        foreach (var fx in data.FxData)
        {
            if (fx == null || fx.FxPath == null)
            {
                continue;
            }

            res.Child.AddRange(GetFxData(fx.FxPath, FileKind));
        }

        // 统计镜头shake
        foreach (var cameraShake in data.CameraShakeData)
        {
            exist = GetFileResAndCheckIsExist("Assets/BundleRes/" + cameraShake.Path + ".asset", out var d, FileKind);
            if (d != null)
            {
                res.Child.Add(d);
            }
        }

        // 统计radiaBlur
        foreach (var special in data.SpecialActionData)
        {
            if (special.Type == 12 && special.SubType == 5)
            {
                var pathSmaller = special.StringParameter1;
                var key = pathSmaller.Split('/').Last();
                if (_preSpecialRadialBlurDictionary.ContainsKey(key))
                {
                    var paths = _preSpecialRadialBlurDictionary[key];
                    foreach (var p in paths)
                    {
                        exist = GetFileResAndCheckIsExist(p, out var d, FileKind);
                        if (d != null)
                        {
                            res.Child.Add(d);
                        }
                    }
                }
            }
        }

        // 镜头anim
        foreach (var cameraMotion in data.CameraMotionData)
        {
            exist = GetFileResAndCheckIsExist("Assets/BundleRes/" + cameraMotion.MotionPath + ".anim", out var d,
                FileKind);
            if (d != null)
            {
                res.Child.Add(d);
            }
        }

        // 技能子弹
        foreach (var fx in data.BulletData)
        {
            if (fx == null || fx.BulletPath == null)
            {
                continue;
            }

            res.Child.AddRange(GetFxData(fx.BulletPath, FileKind));
        }

        // 技能预警
        foreach (var fx in data.WarningData)
        {
            if (fx == null || fx.FxPath == null)
            {
                continue;
            }

            res.Child.AddRange(GetFxData(fx.FxPath, FileKind));
        }

        return res;
    }

    // 统计特效
    private static List<ResNode> GetFxData(String path, ResourcePackageTypeEnum FileKind)
    {
        var res = new List<ResNode>();
        if (string.IsNullOrEmpty(path)) return res;
        var name = path.Split('/').Last().ToLower();
        var lodPref = new List<String>() {"", "_h", "_l", "_m"};
        for (int i = 0; i < 4; i++)
        {
            var fullName = name + lodPref[i];
            if (!_preFxDictionary.ContainsKey(fullName))
            {
                continue;
            }

            var realPaths = _preFxDictionary[fullName];
            foreach (var realPath in realPaths)
            {
                var data = GetPrefabRes(realPath, out var go, FileKind);
                if (go != null)
                {
                    res.Add(data);
                    TryUnloadPrefabContents(go);
                }
            }
        }

        return res;
    }
    // 统计关卡脚本

    // 获取目录所有指定后缀文件 Name 小写
    public static List<NameAndFilePath> GetAllFileByType(String path, String endWith)
    {
        if (path.EndsWith("/"))
        {
            path = path.Substring(0, path.Length - 1);
        }

        if (!System.IO.Directory.Exists(path))
        {
            return new List<NameAndFilePath>();
        }

        var res = new List<NameAndFilePath>();
        DirectoryInfo directory = new DirectoryInfo(path);
        FileSystemInfo[] filesArray = directory.GetFileSystemInfos();
        foreach (var item in filesArray)
        {
            //是否是一个文件夹
            if (item.Attributes == FileAttributes.Directory)
            {
                res.AddRange(GetAllFileByType(path + "/" + item.Name, endWith));
            }
            else
            {
                if (item.Name.EndsWith(endWith))
                {
                    res.Add(new NameAndFilePath()
                        {name = item.Name.ToLower().Replace(endWith, ""), path = path + "/" + item.Name});
                }
            }
        }

        return res;
    }

    // 获取目录所有指定后缀文件 Name 小写
    private static List<NameAndFilePath> GetAllFileByType(String path, List<String> endWiths)
    {
        if (path.EndsWith("/"))
        {
            path = path.Substring(0, path.Length - 1);
        }

        if (!System.IO.Directory.Exists(path))
        {
            return new List<NameAndFilePath>();
        }

        var res = new List<NameAndFilePath>();
        DirectoryInfo directory = new DirectoryInfo(path);
        FileSystemInfo[] filesArray = directory.GetFileSystemInfos();
        foreach (var item in filesArray)
        {
            //是否是一个文件夹
            if (item.Attributes == FileAttributes.Directory)
            {
                res.AddRange(GetAllFileByType(path + "/" + item.Name, endWiths));
            }
            else
            {
                foreach (var endWith in endWiths)
                {
                    if (item.Name.EndsWith(endWith))
                    {
                        res.Add(new NameAndFilePath()
                            {name = item.Name.ToLower().Replace(endWith, ""), path = path + "/" + item.Name});
                    }
                }
            }
        }

        return res;
    }
}
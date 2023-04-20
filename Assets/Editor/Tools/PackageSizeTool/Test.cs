using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CFEngine.Editor;
using CFUtilPoolLib;
using ErosionBrushPlugin;
using FMOD;
using LevelEditor;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Debug = UnityEngine.Debug;

    public static class Test
    {
        private static String tableFilepath = "Assets/BundleRes/Table";
        private static String RoleCurvePathRre = "Assets/BundleRes/Curve/";
        private static String RoleAnimationPathRre = "Assets/BundleRes/Animation/";
        private static String RoleSkillPathPre = "Assets/BundleRes/SkillPackage/";
        private static String RoleHitPathPre = "Assets/BundleRes/HitPackage/";
        private static String RoleReactPathPre = "Assets/BundleRes/ReactPackage/";
        private static String RolePrefabPre = "Assets/BundleRes/Runtime/Prefab/Role/";
        private static String MonsterPrefabPre = "Assets/BundleRes/Runtime/Prefab/Monster/";
        private static String OtherPrefabPre = "Assets/BundleRes/Runtime/Prefab/";
        private static String Other1PrefabPre = "Assets/BundleRes/Prefabs/";
        private static String OtherLevelPrefabPre = "Assets/BundleRes/Runtime/Prefab/LevelObject/";
        private static String OtherCutScenePrefabPre = "Assets/BundleRes/Runtime/Prefab/CutScene/";
        private static String RoleFXPathPre = "Assets/BundleRes/Runtime/SFX/";
        private static String TimeLinePathPre = "Assets/BundleRes/TimeLine/";
        private static String VideoPathPre = "Assets/BundleRes/Video/";
        private static String SpecialActionPath = "Assets/BundleRes/SpecialAction/";
        private static Dictionary<string,ResourcePackageTypeEnum> Assets;

        private static Dictionary<string, ResourcePackageTypeEnum> PreTimeLineResTypeDic;
        private static Dictionary<string, ResourcePackageTypeEnum> PreEntityResTypeDic;
        private static Dictionary<string, ResourcePackageTypeEnum> PreCutSceneCharacterTypeDic;
        private static Dictionary<string, ResourcePackageTypeEnum> PreSceneSFXTypeDic;


        public static void Log(string str)
        {
        }

        public static void Init()
        {
            var temp = CFEngine.Editor.BuildBundleConfig.instance
                .BuildBundle("", -1, CFEngine.Editor.BuildType.PreBuild, false);
            Assets = new Dictionary<string, ResourcePackageTypeEnum>(temp.Count);
            temp.ForEach((data) => Assets.Add(data.assetBundleName, ResourcePackageTypeEnum.Unknown));
        }

        // 预统计 统计场景用到的
        // Timeline Character Monster 
        public static void PreCalc()
        {
        }

        public static void GetSceneTimeLine(MapList.RowData scene, ResourcePackageTypeEnum type)
        {
            String path = $"{tableFilepath}/{scene.LevelConfigFile}.cfg";
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
                        continue;
                    }

                    var resRealName = script.stringParam[0];
                    var timeLinePrefab = TimeLinePathPre + resRealName + ".prefab";
                    if (!File.Exists(timeLinePrefab))
                    {
                        Log("");
                        return;
                    }

                    if (!PreTimeLineResTypeDic.ContainsKey(timeLinePrefab))
                    {
                        var oldValue = PreTimeLineResTypeDic[timeLinePrefab];
                        PreTimeLineResTypeDic[timeLinePrefab] = oldValue > type ? type : oldValue;
                    }
                    else
                    {
                        PreTimeLineResTypeDic.Add(timeLinePrefab,type);
                    }
                    
                }
            }
        }

        public static void GetTimelineInnerTimeLine(string path,ResourcePackageTypeEnum type)
        {
            if (!File.Exists(path))
            {
                Log("");
                return;
            }
            var go = PrefabUtility.LoadPrefabContents(path);
            if (go is {})
            {
                Log("Load TimeLine Prefab faild " + path);
            }
            var playableDirector = go.GetComponentInChildren<PlayableDirector>();
            if (playableDirector == null || playableDirector.playableAsset == null)
            {
                return;
            }

            TimelineAsset timelineAsset = playableDirector.playableAsset as TimelineAsset;

            if (timelineAsset != null && timelineAsset.markerTrack != null)
            {
                IEnumerable<IMarker> markers = timelineAsset.markerTrack.GetMarkers();
                foreach (var marker in markers)
                {
                    JumpTimelineSignal signal = marker as JumpTimelineSignal;
                    if (signal != null)
                    {
                        var timeLine = signal.m_timelineName;
                        if (!File.Exists(timeLine))
                        {
                            Log("");
                            return;
                        }

                        if (!PreTimeLineResTypeDic.ContainsKey(timeLine))
                        {
                            var oldValue = PreTimeLineResTypeDic[timeLine];
                            PreTimeLineResTypeDic[timeLine] = oldValue > type ? type : oldValue;
                        }
                        else
                        {
                            PreTimeLineResTypeDic.Add(timeLine,type);
                        }
                    }
                }
            }
            PrefabUtility.UnloadPrefabContents(go);
        }
        
        public static void GetCutSceneCharacter(string path,ResourcePackageTypeEnum type)
        {
            
        }
        public static void GetSceneMonster()
        {
            
        }
        public static void GetTimeLineCharacter()
        {
        }
    }

using BluePrint;
using CFEngine;
using ClientEcsData;
using EcsData;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorNode
{
    enum SpaceTimeLockType
    {
        Self = 0x0001,
        Partner = 0x0002,
        Friend = 0x0004,
        Enemy = 0x0008,
        Neutral = 0x0010,
        Scene = 0x0020,
        TeamOn = 0x0040,
        TeamOff = 0x0080,
        TeamPet = 0x0100,
        PvpOn = 0x0200,
        PvpOff = 0x0400,
        PvpPet = 0x0800,
        EnemyForTeamCopy = 0x1000,
        FriendMonster = 0x2000,
    }

    enum EnvEffectType
    {
        SceneMisc,
        RoleLightDir,
        RoleLightColor,
        RoleRimLight,
        HideScene,
        RadialBlur,
        MontionBlur
    }

    enum DynamicBoneActionType
    {
        Reset,
        SetWeight,
        TweenWeight,
    }

    public class SpecialActionNode : TimeTriggerNode<XSpecialActionData>
    {
        private Material skybox;
        private TextAsset reactScript;
        private AnimCurveData curveData;
        private RadialBlurDataV2 radialBlurDataV2;

        private BaseSkillNode resultNode = null;
        private GUIContent colorText;

        const int FreezeCD = 0x7FFFFFFF;

        private static readonly SavedVector2 scrollViewPosition = new SavedVector2($"{nameof(SpecialActionNode)}.{nameof(scrollViewPosition)}");

        private static readonly string[] dynamicBoneEventTypeNames = new string[]
        {
            "Reset",
            "SetWeight",
            "StartTween",
            "StopTween",
        };

        private static readonly string[] dynamicBoneEventTypeDescs = new string[]
        {
            "\n* 重置物理效果，可以解决闪现类技能头发甩动的问题",
            "\n* 设置物理骨骼影响程度，0为关闭物理骨骼效果，1为完全打开物理效果。",
            "\n* Tween是参数渐变的意思，让物理骨骼淡出一段时间后淡入。\n        - Start ：开始淡入/结束淡出的强度。\n        - End   ：完成淡入/开始淡出的强度。\n        - FadeIn ：淡入时长。\n        - FadeOut ：淡出时长。\n        - 持续时长为LifeTime。",
            "\n* StopTween：停止物理骨骼表现的淡入淡出。",
        };

        private static long timeWarningTime;
        private const float timeWarningDurationMs = 1000;

        public override void Init (BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init (Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header9";

            if (GetRoot.NeedInitRes)
            {
                switch ((XSpecialActionType)HosterData.Type)
                {
                    case XSpecialActionType.XSkybox:
                        skybox = AssetDatabase.LoadAssetAtPath(ResourecePath + HosterData.StringParameter1 + ".mat", typeof(Material)) as Material;
                        break;
                    case XSpecialActionType.XAimMode:
                        reactScript = AssetDatabase.LoadAssetAtPath(ResourecePath + HosterData.StringParameter1 + ".bytes", typeof(TextAsset)) as TextAsset;
                        break;
                    case XSpecialActionType.XPlayCameraCurve:
                        curveData = AssetDatabase.LoadAssetAtPath(ResourecePath + HosterData.StringParameter1 + ".asset", typeof(AnimCurveData)) as AnimCurveData;
                        break;
                }
            }
        }

        public override T CopyData<T>(T data)
        {
            XSpecialActionData copy = base.CopyData(data) as XSpecialActionData;
            List<int> param = new List<int>();
            for (int i = 0; i < copy.IntParameterList.Count; ++i)
                param.Add(copy.IntParameterList[i]);
            copy.IntParameterList = param;

            return copy as T;
        }

        public override void DrawDataInspector ()
        {
            base.DrawDataInspector ();

            float height = GetRoot.editorWindow.position.height - 300;
            scrollViewPosition.Value = GUILayout.BeginScrollView(scrollViewPosition.Value, false, false, GUILayout.Width(0), GUILayout.Height(height));

            switch ((XSpecialActionType)HosterData.Type)
            {
                case XSpecialActionType.XItemDrop:
                case XSpecialActionType.XItemPick:
                    {
                        HosterData.LifeTime = 0;
                        HosterData.EndWithSkill = false;
                        HosterData.PlayerTrigger = false;
                    }
                    break;
                case XSpecialActionType.XSpaceTimeLock:
                case XSpecialActionType.XCollisionType:
                case XSpecialActionType.XTriggerQteEvent:
                case XSpecialActionType.XSkillAttackField:
                    break;
                case XSpecialActionType.XClientFeature:
                case XSpecialActionType.XCinemachineControl:
                    {
                        HosterData.LifeTime = TimeFrameField("LifeTime", HosterData.LifeTime);
                        HosterData.EndWithSkill = false;
                        HosterData.PlayerTrigger = EditorGUITool.Toggle("PlayerTrigger", HosterData.PlayerTrigger);
                    }
                    break;
                default:
                    {
                        Color c = GUI.color;
                        float alpha = Mathf.Clamp01((System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond - timeWarningTime) / timeWarningDurationMs);
                        GUI.color = Color.Lerp(Color.red, c, alpha);
                        HosterData.LifeTime = TimeFrameField("LifeTime", HosterData.LifeTime);
                        GUI.color = c;
                        if ((XSpecialActionType)HosterData.Type == XSpecialActionType.XSkybox ||
                            (XSpecialActionType)HosterData.Type == XSpecialActionType.XSceneEffect)
                            HosterData.EndWithSkill = EditorGUITool.Toggle("EndWithSkill", HosterData.EndWithSkill);
                        else HosterData.EndWithSkill = false;
                        HosterData.PlayerTrigger = EditorGUITool.Toggle("PlayerTrigger", HosterData.PlayerTrigger);
                    }
                    break;
            }

            HosterData.Type = EditorGUITool.Popup ("Type", HosterData.Type, EditorGUITool.Translate<XSpecialActionType> ());
            switch ((XSpecialActionType) HosterData.Type)
            {
                case XSpecialActionType.XSkybox:
                    {
                        skybox = EditorGUITool.ObjectField ("Skybox", skybox, typeof (Material), false) as Material;
                        if (skybox != null)
                        {
                            HosterData.StringParameter1 = AssetDatabase.GetAssetPath (skybox).Replace (ResourecePath, "");
                            HosterData.StringParameter1 = HosterData.StringParameter1.Remove (HosterData.StringParameter1.LastIndexOf ('.'));
                        }
                        else HosterData.StringParameter1 = "";
                    }
                    break;
                case XSpecialActionType.XCriticalAttack:
                case XSpecialActionType.XSpaceTimeLockEffect:
                    {
                        HosterData.FadeTime = TimeFrameField("Fade In Time", HosterData.FadeTime);
                        HosterData.FadeOutTime = TimeFrameField("Fade Out Time", HosterData.FadeOutTime);
                        OnSceneEffectGui();
                    }
                    break;
                case XSpecialActionType.XUIState:
                    {
                        HosterData.IntParameter1 = EditorGUITool.MaskField("Mask", HosterData.IntParameter1, EditorGUITool.Translate(UnityEngine.CFUI.PanelLayer.LayerNames));
                        HosterData.IntParameter2 = EditorGUITool.MaskField("BattleUI", HosterData.IntParameter2, EditorGUITool.Translate<CFClient.UI.XUITools.HideUIBySkillFlag>());
                        CFClient.UI.XUITools.HideUIBySkillFlag flag = (CFClient.UI.XUITools.HideUIBySkillFlag)HosterData.IntParameter2;
                    }
                    break;
                case XSpecialActionType.XAimMode:
                    {
                        reactScript = EditorGUITool.ObjectField ("ReactScript", reactScript, typeof (TextAsset), false) as TextAsset;
                        if (reactScript != null)
                        {
                            HosterData.StringParameter1 = AssetDatabase.GetAssetPath (reactScript).Replace (ResourecePath, "");
                            HosterData.StringParameter1 = HosterData.StringParameter1.Remove (HosterData.StringParameter1.LastIndexOf ('.'));
                        }
                        else HosterData.StringParameter1 = "";
                    }
                    break;
                case XSpecialActionType.XItemDrop:
                    {
                        HosterData.IntParameter1 = EditorGUITool.IntField ("ItemID", HosterData.IntParameter1);
                    }
                    break;
                case XSpecialActionType.XItemPick:
                    {
                        HosterData.FloatParameter1 = EditorGUITool.FloatField ("PickRange", HosterData.FloatParameter1);
                        IntListField(ref HosterData.IntParameterList, 20, "ItemID");
                    }
                    break;
                case XSpecialActionType.XSpaceTimeLock:
                    {
                        HosterData.FloatParameter1 = TimeFrameField("LifeTime", HosterData.FloatParameter1);
                        HosterData.FloatParameter2 = EditorGUITool.Slider("Ratio", HosterData.FloatParameter2, 0.01f, 3f);
                        int type = FreezeCD & EditorGUITool.MaskField("EntityType", HosterData.IntParameter1 & FreezeCD, EditorGUITool.Translate<SpaceTimeLockType>());
                        bool freezeCD = EditorGUITool.Toggle("FreezeCD", ((uint)HosterData.IntParameter1 >> 31) > 0);
                        HosterData.IntParameter1 = type + (freezeCD ? 1 << 31 : 0);
                    }
                    break;
                case XSpecialActionType.XTriggerQteEvent:
                    {
                        HosterData.IntParameter1 = EditorGUITool.IntField ("EventID", HosterData.IntParameter1);
                    }
                    break;
                case XSpecialActionType.XCollisionType:
                    {
                        HosterData.IntParameter1 = EditorGUITool.IntField ("CollisionType", HosterData.IntParameter1);
                    }
                    break;
                case XSpecialActionType.XPlayCameraCurve:
                    {
                        curveData = EditorGUITool.ObjectField ("AnimCurveData", curveData, typeof (AnimCurveData), false) as AnimCurveData;
                        if (curveData != null)
                        {
                            HosterData.StringParameter1 = AssetDatabase.GetAssetPath (curveData).Replace (ResourecePath, "");
                            HosterData.StringParameter1 = HosterData.StringParameter1.Remove (HosterData.StringParameter1.LastIndexOf ('.'));

                            HosterData.FloatParameter1 = EditorGUITool.Slider("Fov", HosterData.FloatParameter1, 0, 180);
                        }
                        else HosterData.StringParameter1 = "";

                        HosterData.IntParameter1 = EditorGUITool.IntField("AssistRoleCount", HosterData.IntParameter1 == 0 ? 3 : HosterData.IntParameter1);
                    }
                    break;
                case XSpecialActionType.XSkillAttackField:
                    {
                        GetNodeByIndex<ResultNode> (ref resultNode, ref HosterData.IntParameter1, true, "ResultNodeIndex");
                    }
                    break;
                case XSpecialActionType.XCinemachineControl:
                    {
                        HosterData.SubType = EditorGUITool.Popup("ControlType", HosterData.SubType, EditorGUITool.Translate<ClientEcsData.XScriptCinemachineControlType>());
                    }
                    break;
                case XSpecialActionType.XSceneEffect:
                    {
                        DrawLine();
                        var envType = (EnvEffectType) HosterData.SubType;
                        EditorGUI.BeginChangeCheck ();
                        envType = (EnvEffectType) EditorGUILayout.EnumPopup ("EffectType", envType);
                        if (EditorGUI.EndChangeCheck ())
                        {
                            HosterData.SubType = (int) envType;
                        }
                        
                        HosterData.FadeTime = TimeFrameField("Fade In Time", HosterData.FadeTime);
                        HosterData.FadeOutTime = TimeFrameField("Fade Out Time", HosterData.FadeOutTime);

                        switch (envType)
                        {
                            case EnvEffectType.SceneMisc:
                                OnSceneEffectGui ();
                                break;
                            case EnvEffectType.RoleLightDir:
                                OnRoleLightDirGui ();
                                break;
                            case EnvEffectType.RoleLightColor:
                                OnLightColorGui ();
                                break;
                            case EnvEffectType.RoleRimLight:
                                OnLightColorGui ();
                                break;
                            case EnvEffectType.HideScene:
                                break;
                            case EnvEffectType.RadialBlur:
                                OnRadialBlurGui();
                                break;
                            case EnvEffectType.MontionBlur:
                                OnMontionBlurGui();
                                break;
                        }
                    }
                    break;
                case XSpecialActionType.XDynamicBone:
                    {
                        HosterData.IntParameter1 = EditorGUITool.Popup("ActionType", HosterData.IntParameter1, dynamicBoneEventTypeNames);
                        DynamicBoneActionType type = (DynamicBoneActionType)HosterData.IntParameter1;
                        if (type == DynamicBoneActionType.SetWeight)
                        {
                            HosterData.FloatParameter1 = EditorGUITool.FloatField("Weight", HosterData.FloatParameter1);
                        }
                        else if (type == DynamicBoneActionType.TweenWeight)
                        {
                            HosterData.FloatParameter1 = EditorGUITool.Slider("Start", HosterData.FloatParameter1, 0, 1);
                            HosterData.FloatParameter2 = EditorGUITool.Slider("End", HosterData.FloatParameter2, 0, 1);
                            HosterData.FloatParameter3 = Mathf.Max(0, EditorGUITool.FloatField("FadeIn", HosterData.FloatParameter3));
                            HosterData.FloatParameter4 = Mathf.Max(0, EditorGUITool.FloatField("FadeOut", HosterData.FloatParameter4));
                        }
                        GUILayout.Label(dynamicBoneEventTypeDescs[HosterData.IntParameter1]);
                    }
                    break;
                case XSpecialActionType.XClientFeature:
                    {
                        DrawLine();
                        HosterData.SubType = EditorGUITool.Popup("FeatureType", HosterData.SubType, EditorGUITool.Translate<EcsData.ClientFeatureType>());
                        switch ((ClientFeatureType)HosterData.SubType)
                        {
                            case ClientFeatureType.Talk:
                                HosterData.IntParameter1 = EditorGUITool.IntField("TalkID", HosterData.IntParameter1);
                                break;
                            case ClientFeatureType.TransformSkin:
                                HosterData.IntParameter1 = EditorGUITool.IntField("PresentID", HosterData.IntParameter1);
                                break;
                        }
                    }
                    break;
                case XSpecialActionType.XHitSlotFromHeader:
                    {
                        HosterData.IntParameter1 = EditorGUITool.Popup("EffectID: ", HosterData.IntParameter1, EditorGUITool.Translate<EditorEcs.XHitSlot>());
                    }
                    break;
            }

            if (HosterData.FadeTime + HosterData.FadeOutTime > HosterData.LifeTime)
            {
                timeWarningTime = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
            }
            HosterData.FadeTime = Mathf.Clamp(HosterData.FadeTime, 0, HosterData.LifeTime);
            HosterData.FadeOutTime = Mathf.Clamp(HosterData.FadeOutTime, 0, HosterData.LifeTime - HosterData.FadeTime);

            GUILayout.EndScrollView();
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            switch ((XSpecialActionType)HosterData.Type)
            {
                case XSpecialActionType.XSkillAttackField:
                    {
                        if(HosterData.IntParameter1==-1)
                        {
                            hasError = true;
                            LogError("Node_" + HosterData.Index + "，XSkillAttackField 未配置！！！");
                            return false;
                        }
                    }
                    break;

            }

            return true;
        }

        public override void BuildDataFinish ()
        {
            base.BuildDataFinish ();

            switch ((XSpecialActionType) HosterData.Type)
            {
                case XSpecialActionType.XSkillAttackField:
                    {
                        GetNodeByIndex<ResultNode> (ref resultNode, ref HosterData.IntParameter1);
                    }
                    break;
            }
        }

        private void OnSceneEffectGui ()
        {
            EditorGUILayout.LabelField("Vignette");
            HosterData.FloatParameter4 = EditorGUILayout.Slider("Intensity", HosterData.FloatParameter4, 0f, 1f);
            HosterData.FloatParameter5 = EditorGUILayout.Slider("Smoothness", HosterData.FloatParameter5, 0.01f, 1f);
            HosterData.FloatParameter6 = EditorGUILayout.Slider("Roundness", HosterData.FloatParameter6, 0f, 1f);
            bool rounded = HosterData.FloatParameter7 < 0.5f ? false : true;
            rounded = EditorGUILayout.Toggle("Rounded", rounded);
            HosterData.FloatParameter7 = rounded ? 1 : 0;
            EditorGUILayout.LabelField("SceneColor");
            OnLightColorGui();
            bool changeRoleColor = HosterData.IntParameter1 == 0 ? true : false;
            changeRoleColor = EditorGUILayout.Toggle("ChangeRoleColor", changeRoleColor);
            HosterData.IntParameter1 = changeRoleColor ? 0 : 1;

            bool changeMainRole = HosterData.IntParameter2 == 1 ? true : false;
            changeMainRole = EditorGUILayout.Toggle("ChangeMainRole", changeMainRole);
            HosterData.IntParameter2 = changeMainRole ? 1 : 0;
        }

        private void OnRoleLightDirGui ()
        {
            HosterData.FloatParameter1 = EditorGUILayout.Slider ("MinAngle", HosterData.FloatParameter1, -45, 0f);
            HosterData.FloatParameter2 = EditorGUILayout.Slider("MaxAngle", HosterData.FloatParameter2, -80f, 45f);
            HosterData.FloatParameter3 = EditorGUILayout.Slider("RoleLightRotOffset", HosterData.FloatParameter3, 0, 360);
            RoleLightMode mode = (RoleLightMode)HosterData.IntParameter1;
            HosterData.IntParameter1 = (int)(RoleLightMode)EditorGUILayout.EnumPopup("LightMode", mode);
            HosterData.Proprity = EditorGUILayout.IntField("Proprity", HosterData.Proprity);
            if (HosterData.Proprity < 0)
                HosterData.Proprity = 0;
        }

        private void OnLightColorGui ()
        {
            Color color = new Color (HosterData.FloatParameter1, HosterData.FloatParameter2, HosterData.FloatParameter3);
            if (colorText == null)
            {
                colorText = new GUIContent ("Color");
            }
            color = EditorGUILayout.ColorField (colorText, color, false, false, true);
            HosterData.FloatParameter1 = color.r;
            HosterData.FloatParameter2 = color.g;
            HosterData.FloatParameter3 = color.b;


            HosterData.Proprity = EditorGUILayout.IntField ("Proprity", HosterData.Proprity);
            if (HosterData.Proprity < 0)
                HosterData.Proprity = 0;
        }

        private static string GetHosterPath(Object asset)
        {
            if (asset)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                int startIndex = AssetsConfig.instance.ResourcePath.Length + 1;
                int length = path.Length - startIndex - ".asset".Length;
                path = path.Substring(startIndex, length);
                path = path.ToLower();
                return path;
            }
            else
            {
                return string.Empty;
            }
        }

        private void GUIRadialBlurVersioned<T>(ref string path, ref T data, string dir) where T : ScriptableObject
        {
            if (!string.IsNullOrEmpty(path))
            {
                string fullPath = GetFullPathFromHosterPath(path);
                T temp = AssetDatabase.LoadAssetAtPath<T>(fullPath);
                if (temp)
                {
                    data = temp;
                }
                else
                {
                    path = string.Empty;
                    data = null;
                }
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            data = EditorGUILayout.ObjectField("Data", data, typeof(T), false) as T;
            if (EditorGUI.EndChangeCheck())
            {
                path = GetHosterPath(data);
            }

            bool pathInvalid = GetRoot == null || string.IsNullOrEmpty(GetRoot.DataPath);
            if (pathInvalid)
            {
                EditorGUILayout.HelpBox("无法新建，当前技能还未保存，无法获取路径。", MessageType.Warning);
            }
            else
            {
                int skillRootDirLength = Application.dataPath.Length + "/BundleRes/SkillPackage/".Length;
                string skillPath = GetRoot.DataPath.Substring(skillRootDirLength, GetRoot.DataPath.Length - skillRootDirLength - ".bytes".Length);
                string dataPath = $"Assets/BundleRes/SpecialAction/{dir}/{skillPath}.asset";
                bool pathEqual = AssetDatabase.GetAssetPath(data) == dataPath;
                EditorGUI.BeginDisabledGroup(pathInvalid || pathEqual);
                if (GUILayout.Button("新建", GUILayout.Width(35)))
                {
                    string directory = Path.GetDirectoryName(dataPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    T instance = ScriptableObject.CreateInstance<T>();
                    AssetDatabase.CreateAsset(instance, dataPath);
                    data = AssetDatabase.LoadAssetAtPath<T>(dataPath);
                    path = GetHosterPath(data);
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUI.BeginDisabledGroup(!EditorUtility.IsDirty(data));
            if (GUILayout.Button("保存", GUILayout.Width(35)))
            {
                AssetDatabase.SaveAssets();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            if (data)
            {
                Editor editor = Editor.CreateEditor(data);
                editor.OnInspectorGUI();
            }
        }

        private void OnRadialBlurGui()
        {
            HosterData.LoopTime = TimeFrameField("Loop Time", HosterData.LoopTime);

            ref int version = ref HosterData.IntParameter1;
            ref string path = ref HosterData.StringParameter1;

            float loopTimes = Mathf.Max(0, (HosterData.LifeTime - HosterData.FadeTime - HosterData.FadeOutTime) / HosterData.LoopTime);
            GUILayout.Label($"循环次数 = (LifeTime - FadeIn - FadeOut) / Loop = {loopTimes:0.00}");

            EditorGUI.BeginChangeCheck();
            version = EditorGUILayout.Popup(version, new string[] { "初版", "新版" });
            if (EditorGUI.EndChangeCheck())
            {
                path = string.Empty;
            }

            float loopDuration = HosterData.LifeTime - HosterData.FadeTime - HosterData.FadeOutTime;
            if (HosterData.LoopTime <= 0 && loopDuration > 1e-3f)
            {
                EditorGUILayout.BeginHorizontal();
                float expectedLifeTime = HosterData.FadeTime + HosterData.FadeOutTime;
                EditorGUILayout.HelpBox($"循环时长大于0({loopDuration})，但是LoopTime({HosterData.LoopTime})小于0，Loop阶段将持续无效果，请修改LifeTime为淡入和淡出时间的和({expectedLifeTime})。", MessageType.Error);
                if (GUILayout.Button("修改", GUILayout.Width(35), GUILayout.Height(38)))
                {
                    HosterData.LifeTime = expectedLifeTime;
                }
                EditorGUILayout.EndHorizontal();
            }
            HosterData.CanSkipWhenCastAoYi = EditorGUITool.Toggle("CanSkipWhenCastAoYi", HosterData.CanSkipWhenCastAoYi);
            GUIRadialBlurVersioned(ref path, ref radialBlurDataV2, "RadialBlurV2/Data");
        }

        private static string GetFullPathFromHosterPath(string parameter)
        {
            return $"{AssetsConfig.instance.ResourcePath}/{parameter}.asset";
        }

        private void OnMontionBlurGui()
        {
            bool blurByCamera = HosterData.FloatParameter2 > 0.5f;
            EditorGUI.BeginChangeCheck();
            blurByCamera = EditorGUILayout.Toggle("BlurByCamera", blurByCamera);
            if (EditorGUI.EndChangeCheck())
            {
                if (blurByCamera)
                {
                    HosterData.FloatParameter1 = 50;
                }
                else
                {
                    HosterData.FloatParameter1 = 1;
                    HosterData.FloatParameter2 = 0.2f;
                }
            }
            HosterData.FloatParameter2 = blurByCamera ? 1 : 0;

            if (blurByCamera)
            {
                HosterData.FloatParameter1 = EditorGUILayout.Slider("Intensity", HosterData.FloatParameter1, 0, 100f);
            }
            else
            {
                HosterData.FloatParameter3 = EditorGUILayout.Slider("BlurDiverge", HosterData.FloatParameter3, 0, 3);
                HosterData.FloatParameter1 = EditorGUILayout.Slider("Intensity", HosterData.FloatParameter1, 0, 1f);
            }
            HosterData.CanSkipWhenCastAoYi = EditorGUITool.Toggle("CanSkipWhenCastAoYi", HosterData.CanSkipWhenCastAoYi);
        }
    }
}

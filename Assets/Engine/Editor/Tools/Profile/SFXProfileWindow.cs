using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using a;
using CFEngine.SRP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using VirtualSkill;

namespace CFEngine.Editor
{ 
    public class SFXProfileWindow: EditorWindow {
    
        public static SFXProfileWindow instance;
        public static bool isOn;
        public GameObject currentParticleSystem;
        public string saveName;
        public bool addMode = false;

        private GameObject _cuGameObject;
        // public RenderTexture RenderTexture;
        public ParticleSystem[] _psList;
        // private int _maxParticleCount;
        // private int _maxDrawcall;
        private int _skillSerial = 0;
        private bool _startTest = false;
        private string skillPath = String.Empty;
        private string _skillPath;
        private List<string> _files;
        private int _manualSerial;
        private bool _manualLock = false;
        private List<KeyValuePair<int, SingleSkillSFXProfileData>> _errorResult;

        private Vector2 _skillListScrollViewPos;
        private Vector2 _debugViewPos;
        private string tip = string.Empty;
        
        private GUIStyle _titleStyle;
        private GUIStyle _overflowStyle;
        private GUIStyle _defaultStyle;

        private float _timer;
        // private static readonly int _sceneStandardDrawCall = 23;
        // private static readonly float _sceneStandardOverdraw = 0.977f;
        // private static readonly int _recMemory = 1000 * 1024;
        // private static readonly int _recTextureSize = 5;
        // private static readonly int _recPSSize = 10;
        // private static readonly int _recPSize = 50;
        // private static readonly int _recDC = 10;
        // private static readonly float _recFillrate = 4;
        
        
        public static void ShowWindow()
        {
            if (Application.isPlaying)
            {
                if(!instance)instance = GetWindow<SFXProfileWindow>("技能性能半自动检测工具");
                instance.Focus();
                if (!OverdrawMonitor.isOn)
                {
                    SFXMgr.singleton.createMonitor = OverdrawMonitor.Instance.StartObserveProfile;
                    SFXMgr.singleton.destoryMonitor = OverdrawMonitor.Instance.EndObserveProfile;
                    // OverdrawMonitor.isOn = true;
                }
                isOn = true;
            }
        }
        
        private void OnEnable()
        {
            _titleStyle = new GUIStyle() {fontSize = 12, normal = new GUIStyleState() {textColor = Color.white}};
            _overflowStyle = new GUIStyle() {normal = new GUIStyleState() {textColor = Color.red}};
            _defaultStyle = new GUIStyle() {normal = new GUIStyleState() {textColor = Color.white}, fontStyle = FontStyle.Bold};
            if (Application.isPlaying && SkillHoster.GetHoster!=null)
            {
                SkillHoster.GetHoster.AutoFire += ManualTest;
            }
            _errorResult = new List<KeyValuePair<int, SingleSkillSFXProfileData>>();
        }
        private void OnDisable()
        {
        
            if (OverdrawMonitor.isOn)
            {
                SFXMgr.singleton.createMonitor -= OverdrawMonitor.Instance.StartObserveProfile;
                SFXMgr.singleton.destoryMonitor -= OverdrawMonitor.Instance.EndObserveProfile;
                // OverdrawMonitor.isOn = false;
            }
            if (Application.isPlaying && SkillHoster.GetHoster!=null)
            {
                SkillHoster.GetHoster.AutoFire -= ManualTest;
            }
        }

        public static void CloseDialog()
        {
            // if (!instance) return;
            instance.Close();
            isOn = false;
        }

        private void ReadNewFile()
        {
            _skillPath = skillPath;
            _files = new List<string>();
            string totalPath = Application.dataPath + "/BundleRes/SkillPackage/" + _skillPath;
            tip = "读取路径下文件";
            var paths = Directory.GetFiles(totalPath);
            for (var index = 0; index < paths.Length; index++)
            {
                string path = paths[index];
                if (System.IO.Path.GetExtension(path) == ".bytes")
                {
                    path = Path.GetFileName(path);
                    _files.Add("/BundleRes/SkillPackage/" + _skillPath + "/" + path);
                }
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Screen", _titleStyle);
            if (OverdrawState.gameOverdrawViewMode && Application.isPlaying)
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.Space(20);
                    OverdrawMonitor.Instance.ComputeOverdrawFillrate();
                    GUILayout.Label("最大填充率\n" + OverdrawMonitor.Instance.MaxOverdraw.ToString("00.000"));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("实时填充率\n" + OverdrawMonitor.Instance.AccumulatedAverageOverdraw.ToString("00.000"));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("实时粒子系统数\n" + EngineProfiler.context.psCount);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("实时特效数\n" + EngineProfiler.context.fxCount);
                }
            }
            EditorGUILayout.Space(20);
            using (new GUILayout.VerticalScope())
            {
                
                EditorGUILayout.CurveField("Fillrate", OverdrawMonitor.Instance.isObserving ? OverdrawMonitor.Instance.RuntimeProfileData.fillrate : new AnimationCurve());
                EditorGUILayout.CurveField("drawcall", OverdrawMonitor.Instance.isObserving ? OverdrawMonitor.Instance.RuntimeProfileData.batchesCount : new AnimationCurve());
            }

            
            saveName = EditorGUILayout.TextField("保存文件名", saveName);
            

            // addMode = EditorGUILayout.Toggle("条目增量模式", addMode);
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("统计重置"))
            {
                // _psList = currentParticleSystem.GetComponentsInChildren<ParticleSystem>();
                // _maxParticleCount = 0;
                // _maxDrawcall = 0;
                // OverdrawMonitor.Instance.RuntimeProfileData = new SingleSkillSFXProfileData();
                OverdrawMonitor.Instance.ResetSceneStaticProfile();
                // OverdrawMonitor.Instance.isOn = true;
            }

            if (OverdrawMonitor.Instance.StartProfileData != null)
            {
                EditorGUILayout.IntField("StaticDrawcall", OverdrawMonitor.Instance.StartProfileData.currentBatches);
                EditorGUILayout.FloatField("StaticFillrate", OverdrawMonitor.Instance.StartProfileData.currentFillrate);
                EditorGUILayout.IntField("StaticParticleSystemCount",
                    OverdrawMonitor.Instance.StartProfileData.currentParticleSystemCount);
                EditorGUILayout.IntField("StaticParticleCount",
                    OverdrawMonitor.Instance.StartProfileData.currentParticleCount);
            }
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(tip, MessageType.Warning);
            EditorGUILayout.Space(10);
            if (GUILayout.Button("保存数据"))
            {
                ProfileAnalyze();
                Save();
            }
            if (EditorSceneManager.GetActiveScene().name.Equals("Role_screencapture"))
            {
                EditorGUILayout.LabelField("测试文件夹");
                skillPath = EditorGUILayout.TextField("/BundleRes/SkillPackage/", skillPath);
                if (GUILayout.Button("读取路径"))
                {
                    ReadNewFile();
                }

                EditorGUILayout.LabelField("强制设置释放的特效LOD等级(默认为高)");
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("高"))
                    {
                        SFXMgr.performanceLevel = 0;
                    }

                    if (GUILayout.Button("中"))
                    {
                        SFXMgr.performanceLevel = 1;
                    }

                    if (GUILayout.Button("低"))
                    {
                        SFXMgr.performanceLevel = 2;
                    }
                    if (GUILayout.Button("极低"))
                    {
                        SFXMgr.performanceLevel = 3;
                    }
                }
                if (!_startTest)
                {
                    if (GUILayout.Button("开始测试"))
                    {
                        if (string.IsNullOrEmpty(saveName))
                        {
                            tip = "请先命名目标存储的文件名";
                        }
                        else
                        {
                            if (Application.isPlaying)
                            {
                                OverdrawMonitor.Instance.PassedSkillProfileDatas = new List<SingleSkillSFXProfileData>();
                                if (_files == null || _files.Count == 0)
                                {
                                    _files = SkillHoster.GetHoster._keyboard_skill;
                                }
                                _skillSerial = 0;
                                _startTest = true;
                                SkillHoster.GetHoster.AutoFire += AutoTest;
                                OverdrawMonitor.Instance.errorList = new List<string>();
                            }
                            else
                            {
                                tip = "请在Playing模式执行测试";
                            }
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("停止测试"))
                    {
                        _startTest = false;
                        SkillHoster.GetHoster.AutoFire -= AutoTest;
                        _skillSerial = 0;
                        ProfileAnalyze();
                    }
                }
                if (GUILayout.Button("强制中断技能"))
                {
                    XEcsScriptEntity.EndSkill(SkillHoster.PlayerIndex);
                    OverdrawMonitor.Instance.SaveData();
                    OverdrawMonitor.Instance.isObserving = false;
                }
                if (GUILayout.RepeatButton("清除暂存技能"))
                {
                    OverdrawMonitor.Instance.PassedSkillProfileDatas = new List<SingleSkillSFXProfileData>();
                    tip = "已清空暂存列表";
                }
                
                bool ending = false;
                if (_files != null && saveName != String.Empty)
                {
                    ending = !_startTest 
                             && !OverdrawMonitor.Instance.isObserving 
                             && _skillSerial.Equals(_files.Count) 
                             && _skillSerial!=0;
                }

                if (ending)
                {
                    ProfileAnalyze();
                    Save();
                }
                
               
                if ( _errorResult!=null && _errorResult.Count > 0)
                {
                    string allError = "以下技能测试结果出现问题，请手动测试\n";
                    for (int i = 0; i < _errorResult.Count; i++)
                    {
                        allError += Path.GetFileName(_errorResult[i].Value.skillname) + "\n";
                    }

                    if (OverdrawMonitor.Instance.errorList.Count != 0)
                    {
                        allError += "以下技能测试时有对象未找到，请检查资源是否完整\n";
                        OverdrawMonitor.Instance.errorList = OverdrawMonitor.Instance.errorList
                            .Where((x, i) => OverdrawMonitor.Instance.errorList.FindIndex(n => n == x) == i).ToList();
                        for (int i = 0; i < OverdrawMonitor.Instance.errorList.Count; i++)
                        {
                            allError += OverdrawMonitor.Instance.errorList[i]+"\n";
                        }
                    }
                    
                    _debugViewPos = EditorGUILayout.BeginScrollView(_debugViewPos, false, true, GUILayout.MaxHeight(300), GUILayout.MinHeight(200));
                    EditorGUILayout.TextArea(allError);
                    EditorGUILayout.EndScrollView();
                }
                
                // if (GUILayout.Button("截取静态数据"))
                // {
                //     OverdrawMonitor.Instance.InitialStaticProfile();
                // }
                
                if (_files != null)
                {
                    EditorGUILayout.Space(10);
                    _skillListScrollViewPos = EditorGUILayout.BeginScrollView(_skillListScrollViewPos, false, true);
                    for (int i = 0; i < _files.Count; i++)
                    {
                        if (GUILayout.Button((i+1)+". " + Path.GetFileName(_files[i])))
                        {
                            if (Application.isPlaying)
                            {
                                _manualSerial = i;
                                _manualLock = true;
                                tip = "手动播放技能"+_files[i];
                            }
                            else
                            {
                                tip = "请在Playing模式执行测试";
                            }
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                
                if (ending)
                {
                    _skillSerial = 0;
                }
            }
            
            Repaint();
        }

        public void Save()
        {
            SkillSFXProfileAssets skillAsset = CreateInstance<SkillSFXProfileAssets>();
            skillAsset.list = OverdrawMonitor.Instance.PassedSkillProfileDatas;
            string path = "Assets/Test/SFXProfile";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (saveName == null || saveName.Length <= 0)
            {
                saveName = _skillPath;
            }
            string savePath = string.Format("{0}/{1}.asset", path, saveName);
            if (File.Exists(savePath) && addMode)
            {
                SkillSFXProfileAssets oldAsset = AssetDatabase.LoadAssetAtPath<SkillSFXProfileAssets>(savePath);
                foreach (var singleSkillSfxProfileData in oldAsset.list) skillAsset.list.Add(singleSkillSfxProfileData);
            }
            AssetDatabase.CreateAsset(skillAsset, savePath);
            tip += "已存储";
        }
        public void ProfileAnalyze()
        {
            _errorResult = new List<KeyValuePair<int, SingleSkillSFXProfileData>>();
            tip = "开始分析数据";
            
            List<SingleSkillSFXProfileData> data =  OverdrawMonitor.Instance.PassedSkillProfileDatas;
            for (var index = 0; index < data.Count; index++)
            {
                SingleSkillSFXProfileData skill = data[index];
                if (skill.totalduration <= 0.2f 
                    || skill.fillrate.length <= 2
                    || skill.batchesCount.length <= 2
                    || skill.particlesCount.length <= 2
                    || skill.particleSystemCount.length <= 2)
                {
                    _errorResult.Add(new KeyValuePair<int, SingleSkillSFXProfileData>(index, skill));
                    continue;
                }
                int keySeparate = 0;
                skill.highestProfiles = new List<HighestCurrentProfile>();
                if (skill.separateTime != 0 && Mathf.Abs(skill.separateTime - skill.totalduration) > 0.1f)
                {
                    for (int i = 0; i < skill.particlesCount.length; i++)
                    {
                        if (skill.particlesCount[i].time > skill.separateTime)
                        {
                            keySeparate = i;
                            break;
                        }
                    }

                    HighestCurrentProfile beforeSkillEndingProfile = new HighestCurrentProfile();
                    beforeSkillEndingProfile.AreaFillrate = 0;
                    for (int i = 0; i < keySeparate; i++)
                    {
                        CompareHighestProfilePair(skill, i, ref beforeSkillEndingProfile, ref skill.decreaseWarning);
                        beforeSkillEndingProfile.AreaFillrate += skill.fillrate[i].value * ((i>0)?(skill.fillrate[i].time - skill.fillrate[i-1].time):0);

                    }
                    beforeSkillEndingProfile.AvgBatches /= keySeparate;
                    beforeSkillEndingProfile.AvgFillrate /= keySeparate;
                    beforeSkillEndingProfile.AvgParticleCount /= keySeparate;
                    beforeSkillEndingProfile.AvgParticleSystemCount /= keySeparate;

                    HighestCurrentProfile afterSkillEndingProfile = new HighestCurrentProfile();
                    afterSkillEndingProfile.AreaFillrate = 0;
                    for (int i = keySeparate + 1; i < skill.particlesCount.length; i++)
                    {
                        CompareHighestProfilePair(skill, i, ref afterSkillEndingProfile, ref skill.decreaseWarning);
                        afterSkillEndingProfile.AreaFillrate += skill.fillrate[i].value * ((i>0)?(skill.fillrate[i].time - skill.fillrate[i-1].time):0);
                    }
                    int num = skill.particlesCount.length - keySeparate;
                    afterSkillEndingProfile.AvgBatches /= num;
                    afterSkillEndingProfile.AvgFillrate /= num;
                    afterSkillEndingProfile.AvgParticleCount /= num;
                    afterSkillEndingProfile.AvgParticleSystemCount /= num;

                    skill.highestProfiles.Add(beforeSkillEndingProfile);
                    skill.highestProfiles.Add(afterSkillEndingProfile);
                }
                else
                {
                    HighestCurrentProfile HighestProfile = new HighestCurrentProfile();
                    for (int i = 0; i < skill.particlesCount.length; i++)
                    {
                        CompareHighestProfilePair(skill, i, ref HighestProfile, ref skill.decreaseWarning);
                    }
                    HighestProfile.AvgBatches /= skill.particlesCount.length;
                    HighestProfile.AvgFillrate /= skill.particlesCount.length;
                    HighestProfile.AvgParticleCount /= skill.particlesCount.length;
                    HighestProfile.AvgParticleSystemCount /= skill.particleSystemCount.length;

                    skill.highestProfiles.Add(HighestProfile);
                }

            }
            
            tip = saveName + "完成分析\n";
            if (_errorResult.Count > 0)
            {
                for (int i = 0; i < _errorResult.Count; i++)
                {
                    OverdrawMonitor.Instance.PassedSkillProfileDatas.Remove(_errorResult[i].Value);
                }
            }
        }

        public void CompareHighestProfilePair(SingleSkillSFXProfileData profileData, int serial,
            ref HighestCurrentProfile result, ref bool error)
        {
            if (profileData.batchesCount[serial].value > result.Batches.value)
            {
                result.Batches = new HighestProfilePair()
                    {time = profileData.batchesCount[serial].time, value = profileData.batchesCount[serial].value};
            }
            result.AvgBatches += profileData.batchesCount[serial].value;
            if (profileData.fillrate[serial].value > result.Fillrate.value)
            {
                result.Fillrate = new HighestProfilePair()
                    {time = profileData.fillrate[serial].time, value = profileData.fillrate[serial].value};
            }
            result.AvgFillrate += profileData.fillrate[serial].value;
            if (profileData.particlesCount[serial].value > result.ParticleCount.value)
            {
                result.ParticleCount = new HighestProfilePair()
                    {time = profileData.particlesCount[serial].time, value = profileData.particlesCount[serial].value};
            }
            result.AvgParticleCount += profileData.particlesCount[serial].value;
            if (profileData.particleSystemCount[serial].value > result.ParticleSystemCount.value)
            {
                result.ParticleSystemCount = new HighestProfilePair()
                    {time = profileData.particleSystemCount[serial].time, value = profileData.particleSystemCount[serial].value};
            }
            result.AvgParticleSystemCount += profileData.particleSystemCount[serial].value;

            if (profileData.batchesCount[serial].value < 0
                || profileData.particleSystemCount[serial].value < 0
                || profileData.particlesCount[serial].value < 0)
            {
                error = true;
            }
        }
        
        private void AutoTest()
        {
            if (_startTest)
            {
                if (_files.Count == 0)
                {
                    _startTest = false;
                    SkillHoster.GetHoster.AutoFire -= AutoTest;
                    tip = "技能列表为空";
                    return;
                }
                if (!OverdrawMonitor.Instance.isObserving)
                {
                    _timer += Time.deltaTime;
                    XEcsScriptEntity.EndSkill(SkillHoster.PlayerIndex);
                    OverdrawMonitor.Instance.InitSFXCount();
                    if (_timer > 0.2f)
                    {
                        tip = "(" + (_skillSerial+1)+"/"+_files.Count+")  "+"正在测试" + Path.GetFileName(_files[_skillSerial]);
                        SkillHoster.GetHoster.FireSkill(_files[_skillSerial], SkillHoster.PlayerIndex);
                        OverdrawMonitor.Instance.currentName = _files[_skillSerial];
                        // Input.GetKeyDown(KeyCode.Alpha1 + _skillSerial);
                        // int currentSerial = 49 +_skillSerial;
                        // keybd_event((byte) currentSerial, 0, 1, 0);
                        _skillSerial++;
                        if (_skillSerial > _files.Count - 1)
                        {
                            _startTest = false;
                            SkillHoster.GetHoster.AutoFire -= AutoTest;
                            OverdrawMonitor.Instance.currentName = "";
                            tip = "技能释放完成，等待全部sfx播放结束后可以保存并分析";
                        }

                        _timer = 0;
                    }
                }
            }
        }

        private void ManualTest()
        {
            if (_manualLock && !OverdrawMonitor.Instance.isObserving)
            {
                SkillHoster.GetHoster.FireSkill(_files[_manualSerial], SkillHoster.PlayerIndex);
                OverdrawMonitor.Instance.currentName = _files[_skillSerial];
                tip = "技能被手动释放:"+_files[_manualSerial];
                _manualLock = false;
            }
        }
        // private void OnInspectorUpdate()
        // {
        //     if (!currentParticleSystem)
        //     {
        //         _maxParticleCount = 0;
        //         _maxDrawcall = 0;
        //         return;
        //     }
        //     if (!_cuGameObject)
        //     {
        //         _cuGameObject = currentParticleSystem;
        //         _psList = currentParticleSystem.GetComponentsInChildren<ParticleSystem>();
        //         _maxParticleCount = 0;
        //         _maxDrawcall = 0;
        //     }
        //     if (_cuGameObject.Equals(currentParticleSystem)) return;
        //     
        // }

        // private void ComputeParticle()
        // {
        //     
        //     int particleCount = 0;
        //     foreach (var ps in _psList)
        //     {
        //         particleCount += ps.particleCount;
        //     }
        //     if (particleCount > _maxParticleCount)
        //     {
        //         _maxParticleCount = particleCount;
        //     }
        //     int index = 0;
        //     int memoryUse;
        //     int texUse;
        //     GetParticleEffectData.DisplayRuntimeMemorySizeStr(currentParticleSystem, out memoryUse, out texUse);
        //         EditorGUILayout.Space(20);
        //     using (new GUILayout.HorizontalScope())
        //     {
        //         GUILayout.Label("内存占用\n" + EditorUtility.FormatBytes(memoryUse), (memoryUse > _recMemory) ? _overflowStyle : _defaultStyle);
        //         GUILayout.FlexibleSpace();
        //         GUILayout.Label("内存推荐占用\n" + EditorUtility.FormatBytes(_recMemory));
        //         GUILayout.FlexibleSpace();
        //         GUILayout.Label("贴图使用\n" + texUse, texUse > _recTextureSize ? _overflowStyle : _defaultStyle);
        //         GUILayout.FlexibleSpace();
        //         GUILayout.Label("贴图推荐占用\n" + _recTextureSize);
        //     }
        //     int psLength;
        //     GetParticleEffectData.DisplayParticleSystemCount(currentParticleSystem, out psLength);
        //         EditorGUILayout.Space(20);
        //     using (new GUILayout.HorizontalScope())
        //     {
        //         GUILayout.Label("粒子系统数量\n" + psLength, psLength > _recPSSize ? _overflowStyle : _defaultStyle);
        //         GUILayout.FlexibleSpace();
        //         GUILayout.Label("粒子系统推荐数量\n" + "<"+_recPSSize);
        //         GUILayout.FlexibleSpace();
        //         GUILayout.Label("最大粒子数\n" + _maxParticleCount, _maxParticleCount > _recPSize ? _overflowStyle : _defaultStyle);
        //         GUILayout.FlexibleSpace();
        //         GUILayout.Label("当前粒子数\n" + particleCount, _defaultStyle);
        //         GUILayout.FlexibleSpace();
        //         GUILayout.Label("推荐粒子数\n" +  "<"+_recPSize);
        //     }
        //     // GUILayout.Label(GetParticleEffectData.DisplayParticleCountStr(particleCount, _maxParticleCount));
        //
        //     int dcCurrent;
        //     int batchCurrent;
        //     string dcTip;
        //     dcCurrent = UnityStats.drawCalls - _sceneStandardDrawCall;//GetParticleEffectData.GetOnlyParticleEffecDrawCall();
        //     batchCurrent = UnityStats.batches - _sceneStandardDrawCall;
        //     EditorGUILayout.Space(20);
        //     using (new GUILayout.HorizontalScope())
        //     {
        //         if (dcCurrent > _maxDrawcall) _maxDrawcall = dcCurrent;
        //         GUILayout.Label("当前特效DrawCall / Batches\n" + dcCurrent + " / " + batchCurrent, _defaultStyle);
        //         GUILayout.FlexibleSpace();
        //         GUILayout.Label("实时总Drawcall / Batches\n" + UnityStats.drawCalls + " / " + UnityStats.batches,
        //             _defaultStyle);
        //     } using (new GUILayout.HorizontalScope())
        //     {
        //         GUILayout.Label("最大DrawCall\n" + _maxDrawcall, _maxDrawcall > _recDC ? _overflowStyle : _defaultStyle);
        //         GUILayout.FlexibleSpace();
        //         GUILayout.Label("推荐DrawCall\n" +  "<"+_recDC);
        //     }
        //     
        //     if (RenderContext.gameOverdrawViewMode)
        //     {
        //         using (new GUILayout.HorizontalScope())
        //         {
        //             GUILayout.Label("特效实时填充率\n" + (OverdrawMonitor.Instance.AccumulatedAverageOverdraw - _sceneStandardOverdraw).ToString("0.000"), _defaultStyle);
        //             GUILayout.FlexibleSpace();
        //         }
        //     }
        //     
        // }


    }

}

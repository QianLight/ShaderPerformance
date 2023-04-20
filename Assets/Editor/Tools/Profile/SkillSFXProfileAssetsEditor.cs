using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using a;
using CFEngine.Editor;
using KKSG;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillSFXProfileAssets))]
public class SkillSFXProfileAssetsEditor : Editor
{
    private SkillSFXProfileAssets assets;
    private string[] skillName;
    private int currentType;
        
    private int gridID;
    private string[] standardName;
    private GUIStyle bold;
    private GUIStyle normal;
    private GUIStyle avgOverflow;
    private GUIStyle highOverflow;
    private GUIStyle title;
    private string warning;

    private int s_maxCount;
    private float s_maxDelay;
    private int s_maxSystem;
    private float s_maxFillrate;
    private int s_maxBatches;
    private float s_maxFillrateArea;

    private bool isSetted = true;
    private static readonly string[] levelText = new[] {"高", "中", "低"};

    private void OnEnable()
    {
        bold = new GUIStyle() {fontStyle = FontStyle.Bold, normal = new GUIStyleState(){textColor = Color.white}, fontSize = 12, alignment = TextAnchor.MiddleLeft};
        avgOverflow = new GUIStyle() {normal = new GUIStyleState() {textColor = Color.red}, alignment = TextAnchor.MiddleLeft};
        highOverflow = new GUIStyle() {normal = new GUIStyleState() {textColor = new Color(1, 0.5f, 0)}, alignment = TextAnchor.MiddleLeft};
        title = new GUIStyle() {fontSize = 14, alignment = TextAnchor.MiddleLeft, normal = new GUIStyleState(){textColor = Color.white}};
        normal = new GUIStyle() {fontSize = 12, alignment = TextAnchor.MiddleLeft, normal = new GUIStyleState() {textColor = Color.white} };
        assets = target as SkillSFXProfileAssets;
        InitData();
    }
    
    private void InitData()
    {
        skillName = new string[assets.list.Count];
        for (int i = 0; i < skillName.Length; i++)
        {
            string[] split = assets.list[i].skillname.Split('/');
            if (split.Length > 1)
            {
                skillName[i] = split[4];
            }
            else
            {
                skillName[i] = split[0];
            }
        }

        currentType = 0;
        gridID = 0;
        RefreshStandard(assets.list[0].SFXLevel);
    }

    private void RefreshStandard(int level = 0)
    {
        currentType = 0;

        //匹配受击列表Hit
        string[] hitPath = Directory.GetFiles(Application.dataPath + "/BundleRes/Effects/Prefabs/Hit", "*", SearchOption.AllDirectories);
        for (int i = 0; i < hitPath.Length; i++)
        {
            // Debug.Log(Path.GetFileNameWithoutExtension(hitPath[i])+"    "+skillName[gridID]);
            if (Path.GetFileNameWithoutExtension(hitPath[i]).Equals(skillName[gridID], StringComparison.OrdinalIgnoreCase))
            {
                currentType = 3;
                isSetted = true;
                SetStandard(level);
                return;
            }
        }
        
        //匹配受击列表Hit_new
        string[] hitNewPath = Directory.GetFiles(Application.dataPath + "/BundleRes/Effects/Prefabs/Hit_new", "*", SearchOption.AllDirectories);
        for (int i = 0; i < hitNewPath.Length; i++)
        {
            // Debug.Log(Path.GetFileNameWithoutExtension(hitNewPath[i])+"    "+skillName[gridID]);
            if (Path.GetFileNameWithoutExtension(hitNewPath[i]).Equals(skillName[gridID], StringComparison.CurrentCultureIgnoreCase))
            {
                currentType = 3;
                isSetted = true;
                SetStandard(level);
                return;
            }
        }
        
        //匹配角色
        if (IgnoreContains(skillName[gridID],"Role_", StringComparison.CurrentCultureIgnoreCase ))
        {
            currentType = 1;
            isSetted = true;
            SetStandard(level);
            return;
        }
        
        //匹配技能设置(技能列表)
        for (int i = 0; i < EditorSFXData.instance.skillTypeByDoc.Count; i++)
        {
            if (IgnoreContains(skillName[gridID],EditorSFXData.instance.skillTypeByDoc[i].skillName, StringComparison.InvariantCultureIgnoreCase ))
            {
                currentType = EditorSFXData.instance.skillTypeByDoc[i].skillType;
                isSetted = true;
                SetStandard(level);
                return;
            }
        }
        
        //匹配技能设置(文件夹列表 缺省设置会到这一步)
        for (int i = 0; i < EditorSFXData.instance.skillTypeByDoc.Count; i++)
        {
            if (IgnoreContains(skillName[gridID],EditorSFXData.instance.skillTypeByDoc[i].skillName, StringComparison.CurrentCultureIgnoreCase ))
            {
                currentType = EditorSFXData.instance.skillTypeByDoc[i].skillType;
                isSetted = true;
                return;
            }
        }
        isSetted = false;
        SetStandard(level);
    }

    private bool IgnoreContains(string source, string value, StringComparison comparisonType)
    {
        return (source.IndexOf(value, comparisonType) >= 0);
    }
    private void SetStandard(int level = 0)
    {
        s_maxBatches = EditorSFXData.instance.settingType[currentType].profileLevels[level].batches;
        s_maxCount = EditorSFXData.instance.settingType[currentType].profileLevels[level].pCount;
        s_maxFillrateArea = EditorSFXData.instance.settingType[currentType].profileLevels[level].fillrateArea;
        s_maxFillrate = EditorSFXData.instance.settingType[currentType].profileLevels[level].fillrate;
        s_maxSystem = EditorSFXData.instance.settingType[currentType].profileLevels[level].psCount;
    }

    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();
        // string[] standardName = {"大量叠加", "中量叠加", "少量叠加", "不叠加", "少量常驻", "大量常驻"};

        // int gridID = 0;
        if (GUILayout.Button("重计算峰值"))
        {
            ProfileAnalyzeSelf();
            InitData();
        }
        int oldID = gridID;
        gridID = GUILayout.SelectionGrid(gridID, skillName, 5);
        SingleSkillSFXProfileData current = assets.list[gridID];
        if(oldID!=gridID)RefreshStandard(current.SFXLevel);

        // using (new EditorGUILayout.HorizontalScope())
        // {
        //     EditorGUILayout.LabelField("LOD等级判定标准");
        //     if (GUILayout.Button("高"))
        //     {
        //         SetStandard(0);
        //     }
        //
        //     if (GUILayout.Button("中"))
        //     {
        //         SetStandard(1);
        //     }
        //
        //     if (GUILayout.Button("低"))
        //     {
        //         SetStandard(2);
        //     }
        // }
        EditorGUILayout.Space(20);
        // standardID = GUILayout.SelectionGrid(standardID, standardName, 6);
        
        
        EditorGUILayout.LabelField(skillName[gridID],title);
        EditorGUILayout.Space(10);

        if (isSetted)
        {
            EditorGUILayout.LabelField("技能类型:" + EditorSFXData.instance.settingType[currentType].exampleInfo);
        }
        else
        {
            EditorGUILayout.LabelField("!!!!!!!!!技能类型未设置，请联系策划进行添加!!!!!!!!", avgOverflow);
        }
        EditorGUILayout.LabelField("LOD等级:" + levelText[current.SFXLevel]);
        
        EditorGUILayout.Space(10);
        warning = string.Empty;
        if (current.separateTime == 0)
        {
            EditorGUILayout.LabelField("该技能不存在滞留效果",bold);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("技能总持续时间为  "+current.totalduration.ToString("0.00")+"秒",bold);
            EditorGUILayout.LabelField("技能持续期峰值",bold);
            EditorGUILayout.Space(10);
            DrawHighestProfile(current.highestProfiles[0], current);
        }
        else
        {
            EditorGUILayout.LabelField("技能释放结束时间为  "+current.separateTime.ToString("0.00")+"秒",bold);
            EditorGUILayout.LabelField("技能总持续时间为  "+current.totalduration.ToString("0.00")+"秒",bold);
            if (current.totalduration - current.separateTime > s_maxDelay)
            {
                warning += "滞留时间较长可能造成大量特效叠加,请酌情降低滞留段的粒子数量\n";
            }
            if (Mathf.Abs(current.separateTime - current.totalduration) < 0.1f)
            {
                EditorGUILayout.LabelField("由于sfx结束时间较技能结束时间几乎无延迟故分段忽略不计");
                EditorGUILayout.Space(10);
                DrawHighestProfile(current.highestProfiles[0], current);
            }
            else
            {
                EditorGUILayout.Space(10);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Label("技能固定段", bold);
                        EditorGUILayout.Space(10);
                        DrawHighestProfile(current.highestProfiles[0], current);
                    }
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Label("技能滞留段", bold);
                        EditorGUILayout.Space(10);
                        DrawHighestProfile(current.highestProfiles[1], current);
                    }
                    
                }
            }
        }

        if (current.decreaseWarning) warning += "存在数据负增长，可能是镜头移动造成，请核查\n";
        bool[] notZero = new bool[4] {true, true, true, true};
        for (int i = 0; i < current.highestProfiles.Count; i++)
        {
            if (current.highestProfiles[i].Batches.value != 0) notZero[0] = false;
            if (current.highestProfiles[i].Fillrate.value != 0) notZero[1] = false;
            if (current.highestProfiles[i].ParticleSystemCount.value != 0) notZero[2] = false;
            if (current.highestProfiles[i].ParticleCount.value != 0) notZero[3] = false;
        }
        EditorGUILayout.CurveField("Batches", current.batchesCount);
        EditorGUILayout.CurveField("填充率", current.fillrate);
        EditorGUILayout.CurveField("粒子系统数量", current.particleSystemCount);
        EditorGUILayout.CurveField("粒子数量", current.particlesCount);
        EditorGUILayout.Space(10);

        if (warning != string.Empty)
        {
            EditorGUILayout.HelpBox(warning, MessageType.Warning);
        }
        
    }


    private void DrawHighestProfile(HighestCurrentProfile currentData, SingleSkillSFXProfileData current)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("属性\n", bold);
                GUILayout.Label("Batches\n", bold);
                GUILayout.Label("填充率\n", bold);
                GUILayout.Label("粒子系统数量\n", bold);
                GUILayout.Label("全部粒子数", bold);
            }
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("时间点\n", bold);
                GUILayout.Label(currentData.Batches.time.ToString("0.00" + "s") + "\n", normal);
                GUILayout.Label(currentData.Fillrate.time.ToString("0.00" + "s") + "\n", normal);
                GUILayout.Label(currentData.ParticleSystemCount.time.ToString("0.00" + "s") + "\n", normal);
                GUILayout.Label(currentData.ParticleCount.time.ToString("0.00"+"s")+"\n", normal);
            }
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("峰值\n", bold);

                bool dcOverflow = currentData.Batches.value > s_maxBatches;
                GUILayout.Label(currentData.Batches.value.ToString() + "\n", dcOverflow ? highOverflow : normal);
                if (dcOverflow) warning += "峰值batches较多, 期望值"+s_maxBatches+",实际值"+currentData.Batches.value+"\n";
                
                bool frOverflow = currentData.Fillrate.value > s_maxFillrate;
                GUILayout.Label(currentData.Fillrate.value.ToString("P") + "\n", frOverflow ? highOverflow: normal);
                if (frOverflow) warning += "峰值填充率/overdraw较高, 期望值"+s_maxFillrate.ToString("P")+",实际值"+currentData.Fillrate.value.ToString("P")+"\n";
                
                bool psOverflow = currentData.ParticleSystemCount.value > s_maxSystem;
                GUILayout.Label(currentData.ParticleSystemCount.value.ToString() + "\n", psOverflow ? highOverflow : normal);
                if (psOverflow) warning += "峰值粒子系统数量较高, 期望值"+s_maxSystem+",实际值"+currentData.ParticleSystemCount.value+"\n";
                
                bool pOverflow = currentData.ParticleCount.value > s_maxCount;
                GUILayout.Label(currentData.ParticleCount.value.ToString()+"\n", pOverflow ? highOverflow : normal);
                if (pOverflow) warning += "峰值粒子数量较高, 期望值"+s_maxCount+",实际值"+currentData.ParticleCount.value+"\n";
                
            }
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("均值\n", bold);

                bool dcOverflow = currentData.AvgBatches > s_maxBatches;
                GUILayout.Label(currentData.AvgBatches.ToString("0.00") + "\n", dcOverflow ? avgOverflow : normal);
                if (dcOverflow) warning += "均值batches较多, 期望值" + s_maxBatches + ",实际值" + currentData.AvgBatches.ToString("0") + "\n";
                
                bool frOverflow = currentData.AvgFillrate > s_maxFillrate;
                GUILayout.Label(currentData.AvgFillrate.ToString("P") + "\n", frOverflow ? avgOverflow: normal);
                if (frOverflow)
                    warning += "均值填充率/overdraw较高, 期望值" + s_maxFillrate.ToString("P") + ",实际值" + currentData.AvgFillrate.ToString("P") + "\n";
                
                bool psOverflow = currentData.AvgParticleSystemCount > s_maxSystem;
                GUILayout.Label(currentData.AvgParticleSystemCount.ToString("0.00") + "\n", psOverflow ? avgOverflow: normal);
                if (psOverflow) warning += "均值粒子系统数量较高, 期望值" + s_maxSystem + ",实际值" + currentData.AvgParticleSystemCount.ToString("0") + "\n";
                
                bool pOverflow = currentData.AvgParticleCount > s_maxCount;
                GUILayout.Label(currentData.AvgParticleCount.ToString("0.00")+"\n", pOverflow ? avgOverflow : normal);
                if (pOverflow) warning += "均值粒子数量较高, 期望值" + s_maxCount + ",实际值" + currentData.AvgParticleCount.ToString("0") + "\n";
                
            }

            using (new EditorGUILayout.VerticalScope())
            {

                GUILayout.Label("总值\n",bold);
                GUILayout.Label("\n",normal);
                bool areaOverflow = currentData.AreaFillrate > s_maxFillrateArea;
                GUILayout.Label((currentData.AreaFillrate).ToString("0.0000")+"\n", areaOverflow ? avgOverflow : normal);
                if (areaOverflow)
                    warning += "技能持续时间内填充量过高, 期望值" + s_maxFillrateArea + ",实际值 " + currentData.AreaFillrate;
                GUILayout.Label("\n", normal);
                GUILayout.Label("\n", normal);
            }
        }
       
    }
    
    public void ProfileAnalyzeSelf()
        {
            SkillSFXProfileAssets assets = target as SkillSFXProfileAssets;
            List<SingleSkillSFXProfileData> data =  assets.list;
            foreach (SingleSkillSFXProfileData skill in data)
            {
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
                    double tmpFillrate = 0;
                    int last = 0;
                    for (int i = 0; i < keySeparate; i++)
                    {
                        CompareHighestProfilePair(skill, i, ref beforeSkillEndingProfile, ref skill.decreaseWarning);
                        if (i != 0)
                        {
                            if (float.IsNaN(skill.fillrate[i].value))
                            {
                                continue;
                            }
                            tmpFillrate += skill.fillrate[i].value * (skill.fillrate[i].time - skill.fillrate[last].time);
                            last = i;
                        }
                    }
                    beforeSkillEndingProfile.AvgBatches /= keySeparate;
                    beforeSkillEndingProfile.AvgFillrate = (float)(tmpFillrate / keySeparate);
                    beforeSkillEndingProfile.AreaFillrate =  (float)tmpFillrate;
                    beforeSkillEndingProfile.AvgParticleSystemCount /= keySeparate;
                    beforeSkillEndingProfile.AvgParticleCount /= keySeparate;
                    
                    HighestCurrentProfile afterSkillEndingProfile = new HighestCurrentProfile();
                    afterSkillEndingProfile.AreaFillrate = 0;
                    tmpFillrate = 0;
                    last = 0;
                    for (int i = keySeparate+1; i < skill.particlesCount.length; i++)
                    {
                        CompareHighestProfilePair(skill, i, ref afterSkillEndingProfile, ref skill.decreaseWarning);
                        if (float.IsNaN(skill.fillrate[i].value)) continue;
                        tmpFillrate += skill.fillrate[i].value * (skill.fillrate[i].time - skill.fillrate[last].time);
                        last = i;
                    }
                    int num = skill.particlesCount.length - keySeparate;
                    afterSkillEndingProfile.AvgBatches /= num;
                    afterSkillEndingProfile.AvgFillrate = (float)(tmpFillrate / keySeparate);
                    afterSkillEndingProfile.AreaFillrate =  (float)tmpFillrate;
                    afterSkillEndingProfile.AvgParticleCount /= num;
                    afterSkillEndingProfile.AvgParticleSystemCount /= num;

                    skill.highestProfiles.Add(beforeSkillEndingProfile);
                    skill.highestProfiles.Add(afterSkillEndingProfile);
                }
                else
                {
                    HighestCurrentProfile HighestProfile = new HighestCurrentProfile();
                    double tmpFillrate = 0;
                    int last = 0;
                    for (int i = 0; i < skill.particlesCount.length; i++)
                    {
                        CompareHighestProfilePair(skill, i, ref HighestProfile, ref skill.decreaseWarning);
                        if (i != 0)
                        {
                            if (float.IsNaN(skill.fillrate[i].value)) continue;
                            tmpFillrate += skill.fillrate[i].value * (skill.fillrate[i].time - skill.fillrate[last].time);
                            last = i;
                        }
                    }
                    
                    HighestProfile.AvgBatches /= skill.batchesCount.length;
                    HighestProfile.AvgFillrate = (float)(tmpFillrate / skill.fillrate[skill.fillrate.keys.Length-1].time);
                    HighestProfile.AreaFillrate =  (float)tmpFillrate;
                    HighestProfile.AvgParticleCount /= skill.particlesCount.length;
                    HighestProfile.AvgParticleSystemCount /= skill.particleSystemCount.length;

                    skill.highestProfiles.Add(HighestProfile);
                }
            }

            RefreshStandard();
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
}

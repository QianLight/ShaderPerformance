using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CFUtilPoolLib;
using ErosionBrushPlugin;
using UnityEditor;
using UnityEngine;

[Serializable]
public class FileList
{
    [SerializeField] public List<string> list;
}

public static class ExplorerUtil
{
    public static void OpenExplorerFolder(string dirPath)
    {
#if UNITY_EDITOR
        System.Diagnostics.Process open = new System.Diagnostics.Process();
        open.StartInfo.FileName = "explorer";
        open.StartInfo.Arguments = @"/e /root," + dirPath.Replace("/", "\\");
        open.Start();
#endif
    }

    public static void OpenExplorerFile(string filePath)
    {
#if UNITY_EDITOR
        System.Diagnostics.Process open = new System.Diagnostics.Process();
        open.StartInfo.FileName = "explorer";
        open.StartInfo.Arguments = @"/select," + filePath.Replace("/", "\\");
        open.Start();
#endif
    }
}

public class SrcResourceData
{
    public struct Data
    {
        public String Path;
        public String Name;
        public float Size;
        private ResourcePackageTypeEnum fileKind;

        public Data(string path, string name, float size)
        {
            Path = path;
            Name = name;
            Size = size;
            fileKind = ResourcePackageTypeEnum.Unknown;
        }

        public ResourcePackageTypeEnum FileKind
        {
            get { return fileKind; }
            set { fileKind = value > fileKind ? fileKind : value; }
        }

        public String BundlePath
        {
            get { return Path.EndsWith(".unity") ? Path : Path.ToLower(); }
        }
    }

    public Dictionary<string, List<Data>> SrcResDict;

    public void InitResDict(string filePath)
    {
        SrcResDict = new Dictionary<string, List<Data>>();
        FileStream fileStream = new FileStream(filePath, FileMode.Open);
        StreamReader sr = new StreamReader(fileStream);
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            var info = line.Split('\t');

            if (info.Length != 2)
            {
                fileStream.Close();
                throw new Exception($"cant format data {line}");
            }

            string path = info[0];
            string name = info[0].Split('/').Last().ToLower();
            if (float.TryParse(info[1]+"f", out var size))
            {
                fileStream.Close();
                throw new Exception($"cant format data {line}");
            }

            if (!SrcResDict.ContainsKey(name))
            {
                SrcResDict.Add(name, new List<Data>());
            }

            SrcResDict[name].Add(new Data(path, name, size));
        }

        fileStream.Close();
    }

    public void SetFileKind(string path, ResourcePackageTypeEnum type)
    {
        var name = path.Split('/').Last().ToLower();
        if (!SrcResDict.ContainsKey(name))
        {
            return;
        }

        for (int i = 0; i < SrcResDict[name].Count; i++)
        {
            var temp = SrcResDict[name][i];
            if (temp.Path.Contains("assetres/") || path.Contains(temp.Path))
            {
                temp.FileKind = type;
            }

            SrcResDict[name][i] = temp;
        }
    }
}

public enum ResourceKindEnum
{
    levelDatas,
    characterDatas,
    entityDatas,
    uiDatas,
    timeLineDatas,
    sceneDatas,
    skillDatas,
    behitDatas,
    reactDatas
}


public abstract class ResConfigBase
{
    public abstract void InitData();

    public void DelResource(string file )
    {
        FileStream fileStream = new FileStream(file, FileMode.Open);
        StreamReader sr = new StreamReader(fileStream);
        string line;
        var paths = new List<string>();
        while ((line = sr.ReadLine()) != null)
        {
            var info = line.Split(',');
            if (info[1] == ResourcePackageTypeEnum.NoUseFile.ToString())
            {
                paths.Add(info[0]);
            }
        }

        fileStream.Close();
        PackAgeSceneSelectEditorWindow.OpenDelWindow(paths);
    }

    public void OutToTxt(ResourceKindEnum kind)
    {
        var d1 = this.GetPublicField(kind.ToString()) as Dictionary<uint, ResourcePackageTypeEnum>;
        var d2 = this.GetPublicField(kind.ToString()) as Dictionary<string, ResourcePackageTypeEnum>;
        if (d1 == null && d2 == null)
        {
            Debug.LogError(kind + "不存在");
            return;
        }

        if (!System.IO.Directory.Exists("ResStaticRes"))
        {
            Directory.CreateDirectory("ResStaticRes");
        }

        var path = "ResStaticRes/" + kind.ToString() + ".txt";
        FileStream fs = new FileStream(path, FileMode.Create);
        StringBuilder sb = new StringBuilder();
        if (d1 != null)
        {
            foreach (var data in d1)
            {
                sb.Append(data.Key + "," + ((int) data.Value) + "\n");
            }
        }

        if (d2 != null)
        {
            foreach (var data in d2)
            {
                sb.Append(data.Key + "," + ((int) data.Value) + "\n");
            }
        }

        byte[] bytes = new UTF8Encoding().GetBytes(sb.ToString());
        fs.Write(bytes, 0, bytes.Length);
        Debug.Log("写入文件成功  " + path);

        sb.Clear();
        fs.Close();
        ExplorerUtil.OpenExplorerFolder("ResStaticRes/");
    }

    public void ImportFormTxt(ResourceKindEnum kind)
    {
        var d1 = this.GetPublicField(kind.ToString()) as Dictionary<uint, ResourcePackageTypeEnum>;
        var d2 = this.GetPublicField(kind.ToString()) as Dictionary<string, ResourcePackageTypeEnum>;
        if (d1 == null && d2 == null)
        {
            Debug.LogError(kind + "不存在");
            return;
        }

        var path = "ResStaticRes/" + kind.ToString() + ".txt";
        if (!File.Exists(path))
        {
            Debug.LogError(path + " 不存在");
            return;
        }

        FileStream fileStream = new FileStream(path, FileMode.Open);
        StreamReader sr = new StreamReader(fileStream);
        string line;
        var kinds = new List<ResourcePackageTypeEnum>();
        var id = new List<string>();
        while ((line = sr.ReadLine()) != null)
        {
            line = line.Replace(" ", "");
            var info = line.Split(',');
            int k = default;
            if (info.Length != 2 || !(int.TryParse(info[1], out k)))
            {
                Debug.LogError("cant format :" + line);
                fileStream.Close();
                return;
            }

            kinds.Add((ResourcePackageTypeEnum) k);
            id.Add(info[0]);
        }

        if (d1 != null)
        {
            d1.Clear();
            for (int i = 0; i < kinds.Count; i++)
            {
                d1.CheckAdd(uint.Parse(id[i]), kinds[i]);
            }
        }

        if (d2 != null)
        {
            d2.Clear();
            for (int i = 0; i < kinds.Count; i++)
            {
                d2.CheckAdd(id[i], kinds[i]);
            }
        }

        fileStream.Close();
        Debug.Log("载入成功 ：" + path);
    }
}

public enum ResourcePackageTypeEnum
{
    ApkFile,
    FirstFile,
    SecondFile,
    FeatureFile,
    NoUseFile,
    Unknown
}

[Serializable]
public class Data
{
    public ResourcePackageTypeEnum type;
}

public class PackageConfig : ResConfigBase
{
    [SerializeField] public Dictionary<uint, ResourcePackageTypeEnum> levelDatas;
    [SerializeField] public Dictionary<uint, ResourcePackageTypeEnum> characterDatas;
    [SerializeField] public Dictionary<uint, ResourcePackageTypeEnum> entityDatas;
    [SerializeField] public Dictionary<string, ResourcePackageTypeEnum> uiDatas;
    [SerializeField] public Dictionary<string, ResourcePackageTypeEnum> timeLineDatas;

    [ContextMenu("InitData")]
    public override void InitData()
    {
        InitLevel();
        InitCharacter();
        InitUI();
        InitTimeLine();
    }

    [ContextMenu("输出到文本")]
    new void OutToTxt(ResourceKindEnum kind)
    {
        base.OutToTxt(kind);
    }

    [ContextMenu("从文本载入")]
    new void ImportFormTxt(ResourceKindEnum kind)
    {
        base.ImportFormTxt(kind);
    }


    [ContextMenu("删除无用资源文件")]
    new void DelResource(string file= "ResStaticRes/sampleRes.txt")
    {
        base.DelResource(file);
    }


    [ContextMenu("WriteNoFirstPackageToJson")]
    private void WriteNoFirstPackageToJson(
        List<ResourcePackageTypeEnum> filekinds,
        string packageFile = "ResStaticRes/sampleRes.txt",
        string resourceFile = "ResStaticRes/res1.txt",
        string dstFile = "AssetListLog/司法岛_2022-03-09-11-43-50-0131.json"
        )
    {
        FileStream fileStream = new FileStream(packageFile, FileMode.Open);
        StreamReader sr = new StreamReader(fileStream);
        var resource = new SrcResourceData();
        resource.InitResDict(resourceFile);
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            
            var info = line.Split(',');
            var ok = ResourcePackageTypeEnum.TryParse(info[1],out ResourcePackageTypeEnum fileKind);
            if (!ok)
            {
                throw new Exception($"file format err {line}");
            }
            resource.SetFileKind(info[0],fileKind);
        }
        var paths = new List<string>();
        foreach (var datas in resource.SrcResDict.Values)
        {
            foreach (var d in datas)
            {
                if (filekinds.Contains(d.FileKind))
                {
                    paths.Add(d.BundlePath);
                }
            }
        }

        fileStream.Close();
        FileList data = new FileList();
        data.list = paths;
        string str = JsonUtility.ToJson(data, true);
        FileStream dstFileStream = new FileStream(dstFile, FileMode.Create);
        byte[] bytes = new UTF8Encoding().GetBytes(str);
        dstFileStream.Write(bytes, 0, bytes.Length);
    }

    [ContextMenu("ClacAll")]
    void ClacAll()
    {
        ResStatisticEditor.GenPreData(this);
        ExplorerUtil.OpenExplorerFolder("ResStaticRes/");
    }


    private void InitTimeLine()
    {
        if (timeLineDatas == null)
        {
            timeLineDatas = new Dictionary<string, ResourcePackageTypeEnum>();
        }

        String TimeLinePathPre = "Assets/BundleRes/TimeLine/";
        var paths = ResStatisticEditor.GetAllFileByType(TimeLinePathPre, ".prefab");
        foreach (var path in paths)
        {
            if (timeLineDatas.ContainsKey(path.name))
            {
                continue;
            }

            timeLineDatas.Add(path.name, ResourcePackageTypeEnum.FirstFile);
        }
    }


    private void InitLevel()
    {
        if (levelDatas == null)
        {
            levelDatas = new Dictionary<uint, ResourcePackageTypeEnum>();
        }

        var _MapList = new MapList();
        XTableReader.ReadFile(@"Table/MapList", _MapList);
        foreach (var data in _MapList.Table)
        {
            var id = data.MapID;
            if (levelDatas.ContainsKey(id))
            {
                continue;
            }

            levelDatas.Add(id, ResourcePackageTypeEnum.FirstFile);
        }
    }

    private void InitCharacter()
    {
        if (characterDatas == null)
        {
            characterDatas = new Dictionary<uint, ResourcePackageTypeEnum>();
        }

        var _PartnerInfoTable = PartnerInfoReader.PartnerInfo.Table;
        foreach (var data in _PartnerInfoTable)
        {
            var id = data.PresentId;
            if (characterDatas.ContainsKey(id))
            {
                continue;
            }

            if (data.Open)
            {
                characterDatas.Add(id, ResourcePackageTypeEnum.FirstFile);
            }
            else
            {
                characterDatas.Add(id, ResourcePackageTypeEnum.SecondFile);
            }
        }
    }

    private void InitUI()
    {
        if (uiDatas == null)
        {
            uiDatas = new Dictionary<string, ResourcePackageTypeEnum>();
        }

        var path = "Assets/BundleRes/UI/OPsystemprefab";
        var parent = new DirectoryInfo(path);
        var dirs = parent.GetDirectories();
        foreach (var dir in dirs)
        {
            if (dir is DirectoryInfo)
            {
                var name = dir.Name;
                if (uiDatas.ContainsKey(name))
                {
                    continue;
                }

                uiDatas.Add(name, ResourcePackageTypeEnum.FirstFile);
            }
        }
    }
}

[CreateAssetMenu(menuName = "ResStatisticAssets")]
public class ResStatisticAssets
{
    [SerializeField] public PackageConfig PackageConfig;
    [SerializeField] public DelConfig DelConfig;
}

public class DelConfig : ResConfigBase
{
    [SerializeField] public Dictionary<uint, ResourcePackageTypeEnum> levelDatas;
    [SerializeField] public Dictionary<uint, ResourcePackageTypeEnum> characterDatas;
    [SerializeField] public Dictionary<uint, ResourcePackageTypeEnum> entityDatas;
    [SerializeField] public Dictionary<string, ResourcePackageTypeEnum> uiDatas;
    [SerializeField] public Dictionary<string, ResourcePackageTypeEnum> timeLineDatas;
    [SerializeField] public Dictionary<string, ResourcePackageTypeEnum> skillDatas;
    [SerializeField] public Dictionary<string, ResourcePackageTypeEnum> behitDatas;
    [SerializeField] public Dictionary<string, ResourcePackageTypeEnum> reactDatas;
    [SerializeField] public Dictionary<string, ResourcePackageTypeEnum> sceneDatas;

    [ContextMenu("InitData")]
    public override void InitData()
    {
        InitLevel();
        InitCharacter();
        InitUI();
        InitTimeLine();
        InitSkillData();
        InitBehitData();
        InitReactData();
        InitSceneData();
    }

    private void InitSceneData()
    {
        if (reactDatas == null)
        {
            reactDatas = new Dictionary<string, ResourcePackageTypeEnum>();
        }
    }

    private void InitReactData()
    {
        if (reactDatas == null)
        {
            reactDatas = new Dictionary<string, ResourcePackageTypeEnum>();
        }

        String ReactPathPre = "Assets/BundleRes/ReactPackage/";
        var allReacts = ResStatisticEditor.GetAllFileByType(ReactPathPre, ".bytes");
        foreach (var react in allReacts)
        {
            var key = react.path.Replace(ReactPathPre, "");
            if (!reactDatas.ContainsKey(key))
            {
                reactDatas.Add(key, ResourcePackageTypeEnum.FirstFile);
            }
        }
    }

    private void InitBehitData()
    {
        if (behitDatas == null)
        {
            behitDatas = new Dictionary<string, ResourcePackageTypeEnum>();
        }

        String HitPathPre = "Assets/BundleRes/HitPackage/";
        var allHitDatas = ResStatisticEditor.GetAllFileByType(HitPathPre, ".bytes");
        foreach (var hit in allHitDatas)
        {
            var key = hit.path.Replace(HitPathPre, "");
            if (!behitDatas.ContainsKey(key))
            {
                behitDatas.Add(key, ResourcePackageTypeEnum.FirstFile);
            }
        }
    }

    private void InitSkillData()
    {
        if (skillDatas == null)
        {
            skillDatas = new Dictionary<string, ResourcePackageTypeEnum>();
        }

        String SkillPathPre = "Assets/BundleRes/SkillPackage/";
        var allSkill = ResStatisticEditor.GetAllFileByType(SkillPathPre, ".bytes");
        foreach (var skill in allSkill)
        {
            var key = skill.path.Replace(SkillPathPre, "");
            if (!skillDatas.ContainsKey(key))
            {
                skillDatas.Add(key, ResourcePackageTypeEnum.FirstFile);
            }
        }
    }

    [ContextMenu("输出到文本")]
    new void OutToTxt(ResourceKindEnum kind)
    {
        base.OutToTxt(kind);
    }

    [ContextMenu("从文本载入")]
    new void ImportFormTxt(ResourceKindEnum kind)
    {
        base.ImportFormTxt(kind);
    }


    [ContextMenu("删除无用资源文件")]
    new void DelResource(string file= "ResStaticRes/sampleRes.txt")
    {
        base.DelResource(file);
    }

    [ContextMenu("ClacAll")]
    void ClacAll()
    {
        ResStatisticEditor.GenPreData(this);
        ExplorerUtil.OpenExplorerFolder("ResStaticRes/");
    }


    private void InitTimeLine()
    {
        if (timeLineDatas == null)
        {
            timeLineDatas = new Dictionary<string, ResourcePackageTypeEnum>();
        }

        String TimeLinePathPre = "Assets/BundleRes/TimeLine/";
        var paths = ResStatisticEditor.GetAllFileByType(TimeLinePathPre, ".prefab");
        foreach (var path in paths)
        {
            if (timeLineDatas.ContainsKey(path.name))
            {
                continue;
            }

            timeLineDatas.Add(path.name, ResourcePackageTypeEnum.FirstFile);
        }
    }


    private void InitLevel()
    {
        if (levelDatas == null)
        {
            levelDatas = new Dictionary<uint, ResourcePackageTypeEnum>();
        }

        var _MapList = new MapList();
        XTableReader.ReadFile(@"Table/MapList", _MapList);
        foreach (var data in _MapList.Table)
        {
            var id = data.MapID;
            if (levelDatas.ContainsKey(id))
            {
                continue;
            }

            levelDatas.Add(id, ResourcePackageTypeEnum.FirstFile);
        }
    }

    private void InitCharacter()
    {
        if (characterDatas == null)
        {
            characterDatas = new Dictionary<uint, ResourcePackageTypeEnum>();
        }

        var _PartnerInfoTable = PartnerInfoReader.PartnerInfo.Table;
        foreach (var data in _PartnerInfoTable)
        {
            var id = data.PresentId;
            if (characterDatas.ContainsKey(id))
            {
                continue;
            }

            if (data.Open)
            {
                characterDatas.Add(id, ResourcePackageTypeEnum.FirstFile);
            }
            else
            {
                characterDatas.Add(id, ResourcePackageTypeEnum.SecondFile);
            }
        }
    }

    private void InitUI()
    {
        if (uiDatas == null)
        {
            uiDatas = new Dictionary<string, ResourcePackageTypeEnum>();
        }

        var path = "Assets/BundleRes/UI/OPsystemprefab";
        var parent = new DirectoryInfo(path);
        var dirs = parent.GetDirectories();
        foreach (var dir in dirs)
        {
            if (dir is DirectoryInfo)
            {
                var name = dir.Name;
                if (uiDatas.ContainsKey(name))
                {
                    continue;
                }

                uiDatas.Add(name, ResourcePackageTypeEnum.FirstFile);
            }
        }
    }
}
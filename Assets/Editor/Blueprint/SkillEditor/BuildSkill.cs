using System;
using System.Collections.Generic;
using System.IO;
using CFEngine;
using CFEngine.Editor;
using UnityEditor;
using UnityEngine;

public partial class BuildSkill : PreBuildPreProcess
{
    class DirFileInfo
    {
        public string dirName;
        public List<string> files = new List<string> ();
    }
    private void AddRes (DirectoryInfo di, List<DirFileInfo> dirFiles, string name, string rootName, bool toLower)
    {
        string path = rootName + name;
        string destPath = toLower ? rootName + name.ToLower() : path;

        FileInfo[] fi = di.GetFiles ("*bytes", SearchOption.TopDirectoryOnly);
        if (fi != null && fi.Length > 0)
        {
            var dfi = new DirFileInfo ()
            {
            dirName = toLower ? name.ToLower() : name
            };
            for (int j = 0; j < fi.Length; ++j)
            {
                var f = fi[j];
                string filename = f.Name.Replace (".bytes", "");
                dfi.files.Add (toLower ? filename.ToLower() : filename);
                string filePath = "";
                if (!string.IsNullOrEmpty (name))
                {
                    filePath = string.Format ("{0}/{1}", path, f.Name);
                }
                else
                {
                    filePath = string.Format ("{0}{1}", path, f.Name);
                }

                string desPath = "";
                if (!string.IsNullOrEmpty(name))
                {
                    desPath = string.Format("{0}/{1}", destPath, toLower ? f.Name.ToLower() : f.Name);
                }
                else
                {
                    desPath = string.Format("{0}{1}", destPath, toLower ? f.Name.ToLower() : f.Name);
                }
                CopyFile(filePath, desPath);
            }
            dirFiles.Add (dfi);
        }
    }
    private void AddRes (string dir, List<DirFileInfo> dirFiles, string rootName, bool processRoot, bool toLower)
    {
        DirectoryInfo rootDir = new DirectoryInfo (dir);
        if (processRoot)
        {
            AddRes (rootDir, dirFiles, "", rootName, toLower);
        }
        DirectoryInfo[] subDirs = rootDir.GetDirectories ();
        for (int i = 0; i < subDirs.Length; ++i)
        {
            var subDir = subDirs[i];
            AddRes (subDir, dirFiles, subDir.Name, rootName, toLower);
        }
    }
    private void SaveRes (BinaryWriter bw, List<DirFileInfo> dirFiles, string dir)
    {
        short count = (short)dirFiles.Count;
        bw.Write(count);
        for (int i = 0; i < dirFiles.Count; ++i)
        {
            var df = dirFiles[i];
            bw.Write(df.dirName);
            short fileCount = (short)df.files.Count;
            bw.Write(fileCount);
            for (int j = 0; j < df.files.Count; ++j)
            {
                var f = df.files[j];
                bw.Write(f);
            }
        }
    }
    public override string Name { get { return "Skill"; } }
    public override int Priority
    {
        get
        {
            return 2;
        }
    }

    public override void PreProcess ()
    {
        base.PreProcess ();
//#if !UNITY_ANDROID
        if (build)
        {

            string path0 = "Assets/StreamingAssets/Bundles/assets/bundleres/skillpackage";
            string path1 = "Assets/StreamingAssets/Bundles/assets/bundleres/hitpackage";
            string path2 = "Assets/StreamingAssets/Bundles/assets/bundleres/reactpackage";

            DirectoryInfo di = new DirectoryInfo (path0);
            if (di.Exists)
            {
                di.Delete (true);
            }
            di = new DirectoryInfo (path1);
            if (di.Exists)
            {
                di.Delete (true);
            }
            di = new DirectoryInfo (path2);
            if (di.Exists)
            {
                di.Delete (true);
            }
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh ();
        }

        var skills = new List<DirFileInfo> ();
        AddRes (Application.dataPath + "/BundleRes/SkillPackage/", skills, "skillpackage/", false, true);
        var hits = new List<DirFileInfo> ();
        AddRes (Application.dataPath + "/BundleRes/HitPackage/", hits, "hitpackage/", true, true);
        var reacts = new List<DirFileInfo> ();
        AddRes (Application.dataPath + "/BundleRes/ReactPackage/", reacts, "reactpackage/", false, true);
        if (build)
        {
            try
            {
                string configPath = string.Format("{0}/Config/ScriptList.bytes", AssetsConfig.instance.ResourcePath);
                if (File.Exists(configPath))
                    AssetDatabase.DeleteAsset(configPath);
                using (FileStream fs = new FileStream(configPath, FileMode.Create))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    SaveRes(bw, skills, "skillpackage");
                    SaveRes(bw, hits, "hitpackage");
                    SaveRes(bw, reacts, "reactpackage");
                }
                AssetDatabase.ImportAsset(configPath, ImportAssetOptions.ForceUpdate);
            }
            catch (Exception e)
            {
                DebugLog.AddErrorLog(e.StackTrace);
            }
        }
        //

    }

}
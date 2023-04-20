using CFUtilPoolLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using XUpdater;

internal sealed class PatchFirst 
{

    internal const string EXT = ".7z";
    internal const string assetRes7z = "assetres.7z";

    static readonly XVersionData v0 = new XVersionData(1, 0, 0);
    static readonly XVersionData v = v0.Increment(false);


    [MenuItem("Tools/Patch/Patch-First", priority = 3)]
    public static void GenerateFirstVersion()
    {
        var tool = XBundleTools.singleton;
        if (!XBundleTools.BranchCheck(out var branch))
        {
            EditorUtility.DisplayDialog("Error", "You current branch is " + branch, "OK");
        }
        else
        {
            XBundleTools.BuildAllAssetBundlesWithList();
            AssetDatabase.Refresh();
            GenerateFirstVersionWithout();
        }
    }

    [MenuItem("Tools/Patch/Patch-First-Fastly", priority = 3)]
    public static void GenerateFirstVersionWithout()
    {
        EditorUtility.DisplayProgressBar("0/2", "processing " + v0 + ", wait patient!", 0.1f);
        var tool = XBundleTools.singleton;
        tool.LoadVersion();
        tool.CreateNewVersion(v0);
        AssetDatabase.Refresh();

        EditorUtility.DisplayProgressBar("1/2", "processing v" + v + ", wait patient!", 0.39f);
        tool.CreateNewVersion(v);
        EditorUtility.DisplayProgressBar("2/2", "generating manifest, wait patient!", 0.99f);
        tool.BuildExternal(v);
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("tip", "build first patch finish", "ok");
    }


    [MenuItem("Tools/Patch/7zPatch", priority = 3)]
    private static void MergeFolder()
    {
        XBundleTools.InitVersion();
        var dirPat = Path.Combine(HelperEditor.basepath, XBundleTools.BundleRoot, v, "animation");
       

        if (Directory.Exists(dirPat))
        {
            var dir = new DirectoryInfo(dirPat);
            var subdirs = dir.GetDirectories();
            float cnt = subdirs.Length;
            int u = 0;
            foreach (var d in subdirs)
            {
                EditorUtility.DisplayProgressBar(string.Format("{0}/{1}", u++, cnt), "processing " + d.Name, u / cnt);
                UnityEngine.Debug.Log(d.Name);
                string pat = d.FullName;
                var directory = new DirectoryInfo(pat);
                var files = directory.GetFiles("*.*", SearchOption.AllDirectories).Where(file => XBundleTools.Filter(file));
                string pat2 = pat + EXT;
                if (File.Exists(pat2)) File.Delete(pat2);
                using (FileStream joinStream = new FileStream(pat2, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    BinaryWriter binary = new BinaryWriter(joinStream, Encoding.Default);
                    binary.Write(files.Count());
                    foreach (var it in files)
                    {
                        using (FileStream readStream = new FileStream(it.FullName, FileMode.Open, FileAccess.Read))
                        {
                            byte[] buffer = new byte[readStream.Length];
                            int data = 0;
                            binary.Write(it.Name);
                            binary.Write(it.Length);
                            if ((data = readStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                binary.Write(buffer, 0, data);
                            }
                        }
                    }
                }
                directory.Delete(true);
            }
        }

        RecalManifest();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/Patch/Un7zPatch", priority = 3)]
    private static void Un7zPatch()
    {
        string dest = Path.Combine(HelperEditor.basepath, XBundleTools.BundleRoot, v, "animation");


        if (Directory.Exists(dest))
        {
            var dir = new DirectoryInfo(dest);
            var files = dir.GetFiles("*" + EXT);
            float cnt = files.Length;
            int u = 0;
            foreach (var d in files)
            {
                EditorUtility.DisplayProgressBar(string.Format("{0}/{1}", u++, cnt), "processing " + d.Name, u / cnt);
                UnityEngine.Debug.Log(d.Name);
                string name = d.Name.Replace(EXT, "");
                string target = dest + "/" + name;
                if (Directory.Exists(target)) Directory.Delete(target, true);
                Directory.CreateDirectory(target);
                using (FileStream fs = new FileStream(d.FullName, FileMode.Open))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    int count = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        string file = reader.ReadString();
                        int len = (int)reader.ReadInt64();
                        var bytes = reader.ReadBytes(len);
                        FileStream fs2 = new FileStream(target + "/" + file, FileMode.OpenOrCreate);
                        BinaryWriter writer = new BinaryWriter(fs2);
                        writer.Write(bytes);
                        writer.Close();
                        fs2.Close();
                    }
                }
                d.Delete();
            }
        }

        Un7zAssetRes();
        EditorUtility.ClearProgressBar();
    }


    private static void RecalManifest()
    {
        var tool = XBundleTools.singleton;
        var list = LoadOld(out var outAssetRes);
        AssetRes7z(outAssetRes);
        string pref = HelperEditor.basepath + "/" + XBundleTools.BundleRoot + "/" + v;
        string dest = pref + "/animation";

        if (Directory.Exists(dest))
        {
            var files = new DirectoryInfo(dest).GetFiles();
            foreach (var it in files)
            {
                if (!XBundleTools.Filter(it)) continue;

                var n = new XBundleTools.MD5Node();
                n.type = AssetType.Bundles | AssetType.MERGE;
                n.location = "animation/" + it.Name;
                n.status = ResStatus.Add;
                n.size = it.Length;
                n.md5 = XBundleTools.FileMD5(it.FullName);
                list.Add(n);
            }
        }

        dest = pref + "/" + assetRes7z;
        FileInfo finfo = new FileInfo(dest);
        var n2 = new XBundleTools.MD5Node();
        n2.type = AssetType.Bundles | AssetType.ZIP;
        n2.location = assetRes7z;
        n2.status = ResStatus.Add;
        n2.size = finfo.Length;
        n2.md5 = XBundleTools.FileMD5(finfo.FullName);
        list.Add(n2);

        list.Sort(Sort);

        tool.GenerateConfBytes(v, list);
        tool.GenerateConfLines(v, list);
    }


    private static int Sort(XBundleTools.MD5Node x, XBundleTools.MD5Node y)
    {
        AssetType ex = ~AssetType.SIGNGLE;
        int l = (int)(x.type & ex);
        int r = (int)(y.type & ex);
        return r - l;
    }


    private static void AssetRes7z(List<XBundleTools.MD5Node> list)
    {
        EditorUtility.DisplayProgressBar("AssetRes 7z", "AssetRes  processing, wait patient!", 0.99f);
        string pref = HelperEditor.basepath + "/" + XBundleTools.BundleRoot + "/" + v + "/";
        string pat = pref + assetRes7z;
        if (File.Exists(pat)) File.Delete(pat);
        using (FileStream joinStream = new FileStream(pat, FileMode.OpenOrCreate, FileAccess.Write))
        {
            BinaryWriter binary = new BinaryWriter(joinStream, Encoding.Default);
            binary.Write(list.Count);
            foreach (var itm in list)
            {
                var it = new FileInfo(pref + "/" + itm.location);
                using (FileStream readStream = new FileStream(it.FullName, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[readStream.Length];
                    int data = 0;
                    binary.Write(itm.location);
                    binary.Write(it.Length);
                    if ((data = readStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        binary.Write(buffer, 0, data);
                    }
                }
                File.Delete(it.FullName);
            }
        }
    }

    private static void Un7zAssetRes()
    {
        EditorUtility.DisplayProgressBar("Un7z AssetRes", "AssetRes  processing, wait patient!", 0.99f);
        string pref = HelperEditor.basepath + "/" + XBundleTools.BundleRoot + "/" + v + "/";
        string pat = pref + "assetres" + EXT;
        using (FileStream fs = new FileStream(pat, FileMode.Open))
        using (BinaryReader reader = new BinaryReader(fs))
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string file = reader.ReadString();
                int len = (int)reader.ReadInt64();
                var bytes = reader.ReadBytes(len);

                string target = pref + file;
                string targetFolder = target.Substring(0,target.LastIndexOf("/"));

                if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

                FileStream fs2 = new FileStream(target, FileMode.OpenOrCreate);
                BinaryWriter writer = new BinaryWriter(fs2);
                writer.Write(bytes);
                writer.Close();
                fs2.Close();
            }
        }
        File.Delete(pat);
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/Patch/Manifest", priority = 3)]
    private static List<XBundleTools.MD5Node> LoadOldManifest()
    {
        return LoadOld(out var li);
    }

    [MenuItem("Tools/Patch/CleanBar", priority = 3)]
    private static void CleanBar()
    {
        EditorUtility.ClearProgressBar();
    }


    private static List<XBundleTools.MD5Node> LoadOld(out List<XBundleTools.MD5Node> outAssetRes)
    {
        outAssetRes = new List<XBundleTools.MD5Node>();
        var list = new List<XBundleTools.MD5Node>();
        var path = XBundleTools.BundleRoot + v + "/manifest.bytes";
        using (FileStream stream = new FileStream(path, FileMode.Open))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            var md5 = reader.ReadBytes(XBundleTools.Md5Length);
            int len = reader.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                var n = new XBundleTools.MD5Node();
                n.location = reader.ReadString();
                n.status = reader.ReadChar();
                n.size = reader.ReadInt64(); //size
                n.md5 = reader.ReadString();
                short t = reader.ReadInt16();
                n.type = (AssetType)t;
                if (!n.location.StartsWith("animation"))
                {
                    if (n.size < 10 * (1 << 10)) //10K
                    {
                        XDebug.singleton.AddLog(n.location);
                        outAssetRes.Add(n);
                    }
                    else if (n.location != assetRes7z)
                    {
                        list.Add(n);
                    }
                }
            }
        }
        return list;
    }

}
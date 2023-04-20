using CFEngine.Editor;
using CFUtilPoolLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using XEditor;
using XUpdater;

public class XBundleTools : XSingleton<XBundleTools>
{
    public class UpdatedFile
    {
        public string name;
        public string location;
        public char status;
        public long size;

        public string path { get { return "Assets/" + location + name; } }

        public string Size { get { return GetCapacityValue(size); } }

        public static string GetCapacityValue(long capacity)
        {
            float value;
            if (capacity < 1024 * 1024)
            {
                value = capacity / 1024.0f;
                return value == 0 ? "0K" : string.Format("{0}K", value.ToString("F2"));
            }
            else
            {
                value = capacity / 1024.0f / 1024.0f;
                return string.Format("{0}M", value.ToString("F2"));
            }
        }
    }

    public static string BundleRoot = "Bundle/Android/";
    public static string ResRoot = "Assets/Resources/";
    public static string BundleManifest = "";
    private static MD5CryptoServiceProvider _md5Generator;
    internal const string stream_pref = "StreamingAssets/Bundles/assets/bundleres/";
    internal const string pref = @"Bundles/assets/bundleres/";
    internal const int Md5Length = 16;
    internal const string dll = "CFClient.dll";
    internal static readonly string[] ignoreFiles = { "Bundles", "Bundles.manifest" };

    private XVersionData _current = null;
    private List<UpdatedFile> _update_files_bundle = new List<UpdatedFile>();
    private List<UpdatedFile> _update_files_native = new List<UpdatedFile>();

    public XVersionData CurrentVersion { get { return _current; } }
    public XVersionData NextVersion { get { return _current.Increment(Rebuild); } }
    public List<UpdatedFile> BundleUpdateFiles { get { return _update_files_bundle; } }
    public List<UpdatedFile> NativeUpdateFiles { get { return _update_files_native; } }
    public bool Rebuild { get; set; }


    static public bool InitVersion()
    {
        ResRoot = "Assets/Resources/version.bytes";
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case UnityEditor.BuildTarget.Android:
                BundleRoot = "Bundle/Android/";
                break;
            case UnityEditor.BuildTarget.iOS:
                BundleRoot = "Bundle/IOS/";
                break;
            default:
                EditorUtility.DisplayDialog("Error", "You current platform is " + EditorUserBuildSettings.activeBuildTarget, "OK");
                return false;
        }
        return true;
    }

    public XBundleTools()
    {
        Rebuild = false;
        InitVersion();
        if (!Directory.Exists(BundleRoot)) Directory.CreateDirectory(BundleRoot);
        BundleManifest = BundleRoot + "manifest.bytes";
    }


    public bool OnInit()
    {
        if (!InitVersion()) return false;

        if (!BranchCheck(out var branch))
        {
            EditorUtility.DisplayDialog("Error", "You current branch is " + branch, "OK");
            return false;
        }
        if (!Directory.Exists(BundleRoot))
        {
            XEditorPath.BuildPath(BundleRoot.Substring(7, BundleRoot.Length - 7), "Assets");
        }
        if (!LoadVersion()) return false;
        FetchNewlyUpdate();
        return true;
    }
    
    public static void BuildAllAssetBundlesWithList()
    {
        //tmp code 
        //BuildBundleConfig.instance.BuildBundle("", -1, BuildType.ABBuild);

        BuildBundleConfig.instance.BuildBundle();
    }

    public void CreateNewVersion(XVersionData v)
    {
        CreateVersionFolder(v);
        WriteTmpVersion(v);
        GenerateHashManifest(v);
        XGitExtractor.Push(v);
        XGitExtractor.TagSrc(v);
    }

    public static bool BranchCheck(out string branch)
    {
        branch = XGitExtractor.CurrentBranch();
        return branch.StartsWith("release") || branch.StartsWith("patch") || branch.StartsWith("master");
    }

    public bool LoadVersion()
    {
        if (!File.Exists(ResRoot))
        {
            File.WriteAllBytes(ResRoot, Encoding.ASCII.GetBytes(XUpdater.XUpdater.Major_Version + ".0.0"));
        }
        _current = ASCIIEncoding.ASCII.GetString(File.ReadAllBytes(ResRoot));
        return _current != null;
    }

    public void UpdateVersion(string version)
    {
        File.WriteAllBytes(ResRoot, Encoding.ASCII.GetBytes(version));
        LoadVersion();
    }

    public void FetchNewlyUpdate()
    {
        _update_files_bundle.Clear();
        _update_files_native.Clear();
        if (!Rebuild)
        {
            XGitExtractor.Run(CurrentVersion);
            CalculateVersionDiff();
        }
    }
    
    public void BuildExternal(XVersionData version)
    {
        var pat = "Assets/Editor/Patch/external.txt";
        var pref = "Assets/" + stream_pref;
        HashSet<string> conf = new HashSet<string>();
        using (FileStream fs = new FileStream(pat, FileMode.Open))
        using (StreamReader reader = new StreamReader(fs))
        {
            while (true)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;
                else
                {
                    var dir = pref + line;
                    if (Directory.Exists(dir))
                    {
                        var directory = new DirectoryInfo(dir);
                        var files = directory.GetFiles("*.*", SearchOption.AllDirectories).Where(file => Filter(file));
                        foreach (var it in files)
                        {
                            int idx = it.FullName.IndexOf(@"Bundles");
                            if (idx > 0)
                            {
                                var name = it.FullName.Substring(idx);
                                conf.Add(name);
                            }
                        }
                    }
                }
            }
            var list = Trans2UpdatedFile(conf, ResStatus.Add);
            Copy2Target(conf, version);
            GenerateConfBytes(version, list);
            GenerateConfLines(version, list);
        }
    }
    
    public bool TransFile(string path, char status, out UpdatedFile uf)
    {
        uf = new UpdatedFile();
        int idx = path.LastIndexOf('/');
        uf.name = path.Substring(idx + 1, path.Length - idx - 1);
        int indx = path.LastIndexOf('/');
        uf.location = path.Remove(indx + 1);
        uf.status = status;
        var physic = "Assets/" + path;
        uf.size = File.Exists(physic) ? new FileInfo(physic).Length : 0;
        return !uf.name.EndsWith(".manifest");
    }

    private void FilterList(UpdatedFile uf)
    {
        if (uf.location.StartsWith("Lib/"))
        {
            if (EditorUserBuildSettings.activeBuildTarget != UnityEditor.BuildTarget.iOS)
                _update_files_native.Add(uf);
        }
        else if (IsInRes(uf))
        {
            _update_files_bundle.Add(uf);
        }
        else if (uf.location.StartsWith("StreamingAssets/"))
        {
            if (uf.name.EndsWith(".bank") || uf.name.EndsWith(".lua.txt"))
            {
                _update_files_native.Add(uf);
            }
        }
    }

    public void CalculateVersionDiff()
    {
        foreach (string m in XGitExtractor.M_files)
        {
            if (!TransFile(m, ResStatus.Mody, out var uf)) continue;
            FilterList(uf);
        }
        foreach (string m in XGitExtractor.A_files)
        {
            if (!TransFile(m, ResStatus.Add, out var uf)) continue;
            FilterList(uf);
        }
        foreach (string m in XGitExtractor.D_files)
        {
            if (!TransFile(m, ResStatus.Delt, out var uf)) continue;
            FilterList(uf);
        }
    }

    private bool IsInRes(UpdatedFile uf)
    {
        bool ret = false;
        if (uf.location.StartsWith("Creatures") ||
            uf.location.StartsWith("Effects") ||
            uf.location.StartsWith("Scenes") ||
            uf.location.StartsWith("BundleRes/"))
            ret = true;
        return ret;
    }

    private void CopyFile(string orig, string dest)
    {
        FileInfo file = new FileInfo(dest);
        var dir = file.Directory;
        if (!dir.Exists) dir.Create();
        File.Copy(orig, dest, true);
    }

    private void CreateVersionFolder(XVersionData version, bool deleteOld = false)
    {
        string target = BundleRoot + version;
        if (deleteOld && Directory.Exists(target))
        {
            Directory.Delete(target, true);
        }
        Directory.CreateDirectory(target);
    }

    private void FilterPath(string it, out List<string> locations, out List<string> paths, out List<AssetType> types)
    {
        paths = new List<string>();
        locations = new List<string>();
        types = new List<AssetType>();
        paths.Add("Assets/StreamingAssets/" + it);
        if (it.EndsWith(".lua.txt") || it.EndsWith(".bank"))
        {
            locations.Add(it);
            types.Add(it.EndsWith(".lua.txt") ? AssetType.Lua : AssetType.Bank);
        }
        else if (it.Equals(dll) || it.Equals("CFClient.bytes"))
        {
            locations.Add(it.Replace(".dll", ".bytes"));
            paths[0] = @"Assets/Lib/" + dll;
            types.Add(AssetType.Dll);
        }
        else if (it.StartsWith(@"Bundles"))
        {
            locations.Add(it);
            types.Add(AssetType.Bundles);
            if (it == @"Bundles/Bundles")
                locations[0] = "Bundles";
            if (UsedInLua(it, out var location))
            {
                string loc2 = ReplacePath(it, "Bundles/assets/bundleres/", "");
                locations.Add(location);
                string pat2 = "Assets/BundleRes/" + loc2;
                paths.Add(pat2);
                types.Add(AssetType.LuaTable);
            }
        }
    }

    private string ReplacePath(string target, string oldV, string newV)
    {
        target = target.Replace(oldV, newV);
        oldV = oldV.Replace("/", "\\");
        target = target.Replace(oldV, newV);
        return target;
    }

    public List<MD5Node> Trans2UpdatedFile(HashSet<string> list, char status)
    {
        List<MD5Node> ret = new List<MD5Node>();
        foreach (var it in list)
        {
            FilterPath(it, out var locations, out var path, out var types);
            for (int i = 0; i < locations.Count; i++)
            {
                MD5Node f = new MD5Node();
                f.status = status;
                f.location = locations[i].Replace(@"\", @"/");
                f.size = File.Exists(path[i]) ? new FileInfo(path[i]).Length : 0;
                f.md5 = FileMD5(path[i]);
                f.type = types[i];
                ret.Add(f);
            }
        }
        return ret;
    }


    public class MD5Node
    {
        public string location;
        public char status;
        public long size;
        public string md5;
        public AssetType type;
        public string Size { get { return UpdatedFile.GetCapacityValue(size); } }
    }

    public void GenerateVersion(XVersionData version)
    {
        CreateVersionFolder(version, true);
        GenerateHashManifest(version);
        AnalyVersionDiff(version, out var add, out var mody, out var del);

        List<string> addOrMody = new List<string>();
        addOrMody.AddRange(add);
        addOrMody.AddRange(mody);
        Copy2Target(addOrMody, version);
        var v1 = Trans2UpdatedFile(add, ResStatus.Add);
        var v2 = Trans2UpdatedFile(mody, ResStatus.Mody);
        var v3 = Trans2UpdatedFile(del, ResStatus.Delt);
        List<MD5Node> conf = new List<MD5Node>();
        conf.AddRange(v1);
        conf.AddRange(v2);
        conf.AddRange(v3);
        WriteTmpVersion(version);
        GenerateConfBytes(version, conf);
        GenerateConfLines(version, conf);
    }

    public void Copy2Target(ICollection<string> addOrMody, XVersionData version)
    {
        foreach (var it in addOrMody)
        {
            FilterPath(it, out var locations, out var path, out var type);
            for (int i = 0; i < locations.Count; i++)
            {
                string pref = @"Bundles\assets\bundleres\"; // windows
                string pref2 = @"Bundles/assets/bundleres/"; // macos
                var loc = locations[i].Replace(pref, string.Empty).Replace(pref2, string.Empty);
                string dest = BundleRoot + version + "/" + loc;
                CopyFile(path[i], dest);
            }
        }
    }

    private void WriteTmpVersion(XVersionData v)
    {
        var path = "Shell/version.txt";
        var lines = File.ReadAllLines(path);
        if (EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
        {
            lines[1] = v;
        }
        else if (EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
        {
            lines[0] = v;
        }
        File.WriteAllLines(path, lines);
    }


    private string TrimLoc(MD5Node it)
    {
        if (it.type == AssetType.Bundles)
        {
            string loc = it.location.Replace(pref, string.Empty);
            return loc;
        }
        return it.location;
    }

    public void GenerateConfLines(XVersionData version, List<MD5Node> conf)
    {
        var path = BundleRoot + version + "/manifest.txt";
        if (File.Exists(path)) File.Delete(path);
        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
        using (StreamWriter writer = new StreamWriter(fs))
        {
            writer.WriteLine(conf.Count);
            foreach (var it in conf)
            {
                writer.WriteLine(TrimLoc(it));
                string header = it.status + "-" + it.type + "-" + it.Size;
                writer.WriteLine(header);
                writer.WriteLine(it.md5);
            }
        }
    }

    public void GenerateConfBytes(XVersionData version, List<MD5Node> conf)
    {
        var path = BundleRoot + version + "/manifest.bytes";
        if (File.Exists(path)) File.Delete(path);
        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            writer.Write(conf.Count);
            foreach (var it in conf)
            {
                writer.Write(TrimLoc(it));
                writer.Write(it.status);
                writer.Write(it.size);
                writer.Write(it.md5);
                short t = (short)(it.type);
                writer.Write(t);
            }
        }
        AttachMD5(path);
    }

    public static bool Filter(FileInfo file)
    {
        return !file.FullName.Trim().EndsWith(".meta")
            && !file.FullName.Trim().EndsWith(".manifest")
            && !file.FullName.Trim().EndsWith(".DS_Store")
            && !ignoreFiles.Contains(file.Name);
    }

    public void GenerateHashManifest(XVersionData v)
    {
        DirectoryInfo directory = new DirectoryInfo("Assets/StreamingAssets");
        var files = directory.GetFiles("*.*", SearchOption.AllDirectories).
            Where(file => Filter(file));

        string target = BundleRoot + v + "/hash.txt";
        using (FileStream fs = new FileStream(target, FileMode.OpenOrCreate))
        using (StreamWriter writer = new StreamWriter(fs))
        {
            writer.WriteLine(files.Count() + 1);
            FileInfo dll = new FileInfo("Assets/Lib/CFClient.dll");
            writer.WriteLine("CFClient.bytes");
            string hash = FileMD5(dll.FullName);
            writer.WriteLine(hash);

            foreach (var file in files)
            {
                string name = file.FullName;
                int idx = name.IndexOf("StreamingAssets");
                if (idx > 0)
                {
                    name = name.Substring(idx + 16);
                }
                writer.WriteLine(name);
                hash = FileMD5(file.FullName);
                writer.WriteLine(hash);
            }
        }
    }

    private bool AnalyVersionDiff(XVersionData v, out HashSet<string> add_files, out HashSet<string> mody_files, out HashSet<string> dele_files)
    {
        add_files = new HashSet<string>();
        mody_files = new HashSet<string>();
        dele_files = new HashSet<string>();
        if (v.Minor_Version > 0)
        {
            XVersionData pre = new XVersionData();
            pre.VersionCopy(v);
            pre.Minor_Version--;
            Dictionary<string, string> info1, info2;
            LoadHashManifest(v, out info1);
            LoadHashManifest(pre, out info2);

            foreach (var it in info1)
            {
                if (info2.ContainsKey(it.Key))
                {
                    if (info2[it.Key] != it.Value) mody_files.Add(it.Key);
                }
                else
                {
                    add_files.Add(it.Key);
                }
            }
            foreach (var it in info2)
            {
                if (!info1.ContainsKey(it.Key)) dele_files.Add(it.Key);
            }
            return true;
        }
        return false;
    }


    private void LoadHashManifest(XVersionData v, out Dictionary<string, string> infos)
    {
        infos = new Dictionary<string, string>();
        string target = BundleRoot + v + "/hash.txt";
        using (FileStream fs = new FileStream(target, FileMode.Open))
        using (StreamReader reader = new StreamReader(fs))
        {
            int cnt = int.Parse(reader.ReadLine().Trim());
            for (int i = 0; i < cnt; i++)
            {
                string name = reader.ReadLine().Trim();
                string hash = reader.ReadLine().Trim();
                if (!string.IsNullOrEmpty(name)) infos.Add(name, hash);
            }
        }
    }


    private void AttachMD5(string path)
    {
        if (_md5Generator == null)
            _md5Generator = new MD5CryptoServiceProvider();
        var bytes = File.ReadAllBytes(path);
        byte[] hash = _md5Generator.ComputeHash(bytes);
        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            writer.Write(hash);
            writer.Write(bytes);
        }
    }


    public static string FileMD5(string file)
    {
        if (_md5Generator == null)
            _md5Generator = new MD5CryptoServiceProvider();
        if (File.Exists(file))
        {
            var bytes = File.ReadAllBytes(file);
            byte[] hash = _md5Generator.ComputeHash(bytes);
            return System.BitConverter.ToString(hash);
        }
        return "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00";
    }

    [MenuItem("Assets/开发/MD5")]
    static void SelectionMD5()
    {
        var pt = AssetDatabase.GetAssetPath(Selection.activeObject);
        XDebug.singleton.AddLog(FileMD5(pt));
    }

    [MenuItem("Tools/Patch/Manifest")]
    static void CheckManifest()
    {
        var path = Path.Combine(Application.persistentDataPath, "manifest.bytes");
        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(fs))
            {
                Debug.Log("version: " + reader.ReadString());
                int cnt = reader.ReadInt32();
                Debug.Log("cnt: " +cnt);
                for(int i=0;i<cnt;i++)
                {
                    Debug.Log(i + ": " + reader.ReadString());
                }
            }
        }
    }

    public void CleanEnv(XVersionData v)
    {
        CleanStreaming();
        CleanVersion(v);
    }


    private void CleanStreaming()
    {
        var p = UnityEngine.Application.streamingAssetsPath;
        var p1 = p + "/bytes";
        var p2 = p + "/Bundles";
        if (Directory.Exists(p1))
        {
            Directory.Delete(p1, true);
        }
        if (Directory.Exists(p2))
        {
            Directory.Delete(p2, true);
        }
        AssetDatabase.Refresh();
    }

    private void CleanVersion(XVersionData v)
    {
        string dest = BundleRoot + v;
        if (Directory.Exists(dest))
        {
            Directory.Delete(dest, true);
        }
    }

    private static List<string> _lua_table;
    const string luatable = "Assets/StreamingAssets/lua/table";

    public static List<string> lua_table
    {
        get
        {
            if (_lua_table == null)
            {
                _lua_table = new List<string>();
                DirectoryInfo dirinfo = new DirectoryInfo(luatable);
                FileInfo[] files = dirinfo.GetFiles("*.lua.txt");
                foreach (var file in files)
                {
                    var st = file.FullName.ToLower();
                    st = st.Replace(".lua.txt", ".bytes").Replace("\\", "/");
                    _lua_table.Add(st);
                }
            }
            return _lua_table;
        }
    }

    // table's bytes referenced by lua
    public bool UsedInLua(string loc, out string location)
    {
        location = loc;
        if (loc.EndsWith(".bytes")&& loc.Replace("\\", "/").Contains(@"Bundles/assets/bundleres/table/"))
        {
            loc = loc.Replace("\\", "/").Replace(@"Bundles/assets/bundleres/table/", "");
            XDebug.singleton.AddLog("loc: " + loc + " table[0]: " + lua_table[0]);
            for(int i=0;i<lua_table.Count;i++)
            {
                if(lua_table[i].Contains(loc))
                {
                    DirectoryInfo dirinfo = new DirectoryInfo(luatable);
                    FileInfo[] files = dirinfo.GetFiles("*.lua.txt");
                    location = "table/" + files[i].Name.Replace(".lua.txt", ".bytes");
                    XDebug.singleton.AddLog("loca: " + location + " loc: " +loc);
                    return true;
                }
            }
        }
        return false;
    }

}
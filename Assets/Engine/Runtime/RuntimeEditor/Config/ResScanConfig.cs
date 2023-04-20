#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using System.Xml;
using System.IO;
using System.Text;

namespace CFEngine
{
    [Serializable]
    public class ScanJob
    {
        public string folder = "";
        public int scanType = -1;
    }

    public abstract class ScanPolicy
    {
        public virtual string ScanType
        {
            get { return ""; }
        }
        public virtual string ResExt
        {
            get { return ""; }
        }
        public virtual void Prepare()
        {

        }
        public virtual ResItem Scan(string name, string path, OrderResList result, ResScanConfig config)
        {
            return null;
        }

        public virtual void PostProcess()
        {

        }
    }

     
    public class ResItem
    {
        public string path;
        public string nameWithExt;
        public string nameWithExtLow;
        public byte resType = 0;

        public UnityEngine.Object res;
        public long size = 0;
        public long totalSize = 0;
        public int count = 0;
        public string stateStr = "";

        public RelativeRes relative;
        public List<ResItem> parents = null;
        public bool isRoot = false;
        public FlagMask flag;
        public static uint Flag_IsReadable = 0x00000001;
        public static uint Flag_IOSFormatOverride = 0x00000002;
        public static uint Flag_AndroidFormatOverride = 0x00000004;

        public void UpdateResData(ResScanConfig config, StringBuilder sb)
        {
            flag.Reset();
            if (!string.IsNullOrEmpty(path))
            {
                res = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (res != null)
                {
                    size = Profiler.GetRuntimeMemorySizeLong(res);

                    if (res is Mesh)
                    {
                        size /= 2;
                        Mesh m = res as Mesh;
                        if (m.isReadable)
                        {
                            flag.SetFlag(Flag_IsReadable, true);
                        }

                        if (size >= config.meshSizeThreahold || flag.HasFlag(Flag_IsReadable))
                        {
                            sb.AppendFormat("mesh {0}", path);
                            if (size >= config.meshSizeThreahold)
                            {
                                sb.AppendFormat(" size too long {0}", size);
                            }
                            if (flag.HasFlag(Flag_IsReadable))
                            {
                                sb.Append(" isReadable");
                            }
                            sb.Append("\r\n");
                        }
                    }
                    else if (res is Texture)
                    {
                        size /= 2;
                        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                        if (ti != null)
                        {
                            var iosSetting = ti.GetPlatformTextureSettings("iPhone");
                            flag.SetFlag(Flag_IOSFormatOverride, iosSetting!=null&& iosSetting.overridden);

                            var androidSetting = ti.GetPlatformTextureSettings("Android");
                            flag.SetFlag(Flag_AndroidFormatOverride, androidSetting != null && androidSetting.overridden);
                        }
                       
                        if (size >= config.texSizeThreahold || !flag.HasFlag(Flag_IOSFormatOverride) || !flag.HasFlag(Flag_AndroidFormatOverride))
                        {
                            sb.AppendFormat("tex {0}", path);
                            if (size >= config.texSizeThreahold)
                            {
                                sb.AppendFormat(" size too long {0}", size);
                            }
                            if (!flag.HasFlag(Flag_IOSFormatOverride))
                            {
                                sb.Append(" iOS not override");
                            }
                            if (!flag.HasFlag(Flag_AndroidFormatOverride))
                            {
                                sb.Append(" android not override");
                            }
                            sb.Append("\r\n");
                        }
                           
                    }
                    AssetsConfig.GetResType(res, path, ref resType);
                }
            }
        }

        public void UpdateStateStr()
        {
            totalSize = size + (relative != null ? relative.size : 0);
            stateStr = string.Format("{0} ({1} Bytes) ref {2}",
            EditorUtility.FormatBytes(totalSize),
            size.ToString(), count.ToString());
        }
        public void Save(XmlDocument doc, XmlElement parent, OrderResList root)
        {
            var e = doc.CreateElement("item");
            e.SetAttribute("nameWithExt", nameWithExt);
            e.SetAttribute("path", path);
            e.SetAttribute("resType", resType.ToString());
            e.SetAttribute("count", count.ToString());
            e.SetAttribute("isRoot", isRoot?"1":"0");
            if (relative != null)
            {
                var p = doc.CreateElement("childs");
                e.AppendChild(p);
                int i = 0;
                foreach (var ri in relative.sortList)
                {
                    int index = root.res.IndexOf(ri);
                    var c = doc.CreateElement(string.Format("child_{0}", i.ToString()));
                    c.SetAttribute("index", index.ToString());
                    p.AppendChild(c);
                    i++;
                }
            }
            if (parents != null)
            {
                var p = doc.CreateElement("parents");
                e.AppendChild(p);
                for (int i = 0; i < parents.Count; ++i)
                {
                    var ri = parents[i];
                    int index = root.res.IndexOf(ri);
                    var c = doc.CreateElement(string.Format("parent_{0}", i.ToString()));
                    c.SetAttribute("index", index.ToString());
                    p.AppendChild(c);
                }
            }

            
            parent.AppendChild(e);
        }

        public void Load(XmlElement elem)
        {
            nameWithExt = elem.GetAttribute("nameWithExt");
            nameWithExtLow = nameWithExt.ToLower();
            path = elem.GetAttribute("path");
            string resTypeStr = elem.GetAttribute("resType");
            byte.TryParse(resTypeStr, out resType);
            string countStr = elem.GetAttribute("count");
            int.TryParse(countStr, out count);
            isRoot = elem.GetAttribute("isRoot") == "1";
        }

        public void LoadRelative(XmlElement elem, OrderResList root)
        {
            if (elem.HasChildNodes)
            {
                var childs = elem.ChildNodes;
                for (int i = 0; i < childs.Count; ++i)
                {
                    XmlElement e = childs[i] as XmlElement;
                    if (e.Name == "childs")
                    {
                        var indexChilds = e.ChildNodes;
                        for (int j = 0; j < indexChilds.Count; ++j)
                        {
                            var c = indexChilds[j] as XmlElement;
                            string indexStr = c.GetAttribute("index");
                            if(int.TryParse(indexStr, out var index))
                            {
                                if (relative == null)
                                {
                                    relative = new RelativeRes();
                                }
                                if (index >= 0 && index < root.res.Count)
                                {
                                    relative.sortList.Add(root.res[index]);
                                }
                            }
                        }
                    }
                    else if (e.Name == "parents")
                    {
                        var indexChilds = e.ChildNodes;
                        for (int j = 0; j < indexChilds.Count; ++j)
                        {
                            var c = indexChilds[j] as XmlElement;
                            string indexStr = c.GetAttribute("index");
                            if (int.TryParse(indexStr, out var index))
                            {
                                if (parents == null)
                                {
                                    parents = new List<ResItem>();
                                }
                                if (index >= 0 && index < root.res.Count)
                                {
                                    parents.Add(root.res[index]);
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void Sort(List<ResItem> sortList, ESortType sorttype)
        {
            switch (sorttype)
            {
                case ESortType.ResType:
                    sortList.Sort(
                        (x, y) =>
                        {
                            int result = x.resType.CompareTo(y.resType);
                            if (result == 0)
                            {
                                result = y.totalSize.CompareTo(x.totalSize);
                                if (result == 0)
                                {
                                    result = x.nameWithExtLow.CompareTo(y.nameWithExtLow);
                                }
                            }
                            return result;
                        }
                        );
                    break;
                case ESortType.Size:
                    sortList.Sort(
                        (x, y) =>
                        {
                            int result = y.totalSize.CompareTo(x.totalSize);
                            if (result == 0)
                            {
                                result = x.resType.CompareTo(y.resType);
                                if (result == 0)
                                {
                                    result = x.nameWithExtLow.CompareTo(y.nameWithExtLow);
                                }
                            }
                            return result;
                        }
                        );
                    break;
            }
        }


    }
    public enum ESortType
    {
        ResType,
        Size,
    }
    public class RelativeRes
    {
        public long size = 0;
        private Dictionary<string, ResItem> childs = new Dictionary<string, ResItem>();
        public List<ResItem> sortList = new List<ResItem>();
        public void Add(ResItem item)
        {
            if(!childs.ContainsKey(item.nameWithExtLow))
            {
                childs.Add(item.nameWithExtLow, item);
                sortList.Add(item);
            }
            item.count++;
        }

        public void UpdateResData()
        {
            size = 0;
            foreach(var ri in sortList)
            {
                size += ri.size;
            }
        }
    }

    public class OrderResList
    {
        public List<ResItem> res = new List<ResItem> ();
        private Dictionary<string, int> resMap = new Dictionary<string, int> ();
        public long size = 0;
        public StringBuilder sb = new StringBuilder();
        public int Count
        {
            get
            {
                return res.Count;
            }
        }
        public ResItem GetItem (int index)
        {
            if (index >= 0 && index < res.Count)
            {
                return res[index];
            }
            return null;
        }

        public void Clear ()
        {
            res.Clear ();
            resMap.Clear ();
            sb.Clear();
        }


        public ResItem Add (ResItem parent, string nameWithExt, string path)
        {
            ResItem resItem;
            int index;
            var nameWithExtLow = nameWithExt.ToLower();
            if (resMap.TryGetValue (nameWithExtLow, out index))
            {
                resItem = res[index];
            }
            else
            {
                resItem = new ResItem ();
                resItem.nameWithExt = nameWithExt;
                resItem.nameWithExtLow = nameWithExtLow;
                resItem.path = path;
                resItem.isRoot = parent == null;
                resMap.Add (nameWithExtLow, res.Count);
                res.Add (resItem);
            }
            if (parent != null)
            {
                if (resItem.parents == null)
                {
                    resItem.parents = new List<ResItem> ();
                }
                if (!resItem.parents.Contains (parent))
                    resItem.parents.Add (parent);

                if (parent.relative == null)
                {
                    parent.relative = new RelativeRes();
                }
                parent.relative.Add(resItem);
            }
            return resItem;
        }
        public void BeginCalcSize()
        {
            size = 0;
        }
        
        public ResItem CalcSize0(int index, ResScanConfig config)
        {
            var ri = res[index];
            ri.UpdateResData(config,sb);
            size += ri.size;
            return ri;
        }

        public ResItem CalcSize1(int index)
        {
            var ri = res[index];
            if (ri.relative != null)
                ri.relative.UpdateResData();
            ri.UpdateStateStr();
            return ri;
        }
        public void OutputLog()
        {
            DirectoryInfo di = new DirectoryInfo("Assets/../Dump");
            if (!di.Exists)
            {
                di.Create();
            }
            var now = System.DateTime.Now;
            string log = string.Format("{0}/ResStatis_{1}-{2}-{3}_{4}-{5}.txt", di.FullName,
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            File.WriteAllText(log, sb.ToString());
            sb.Clear();
        }
        
        public void Save()
        {
            DirectoryInfo di = new DirectoryInfo("Assets/../Dump");
            if (!di.Exists)
            {
                di.Create();
            }
            var now = System.DateTime.Now;
            string target = string.Format("{0}/ResStatis_{1}-{2}-{3}_{4}-{5}.xml", di.FullName,
               now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            XmlDocument doc = new XmlDocument();
            XmlElement resList = doc.CreateElement("Res");
            doc.AppendChild(resList);
            for (int i = 0; i < res.Count; ++i)
            {
                var ri = res[i];
                ri.Save(doc, resList, this);
            }
            doc.Save(target);

        }

        public void Load(string path, List<ResItem> scanRoot)
        {
            scanRoot.Clear();
            res.Clear();
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlElement resList = doc.DocumentElement;
            XmlNodeList childs = resList.ChildNodes;
            for (int i = 0; i < childs.Count; ++i)
            {
                XmlElement riElem = childs[i] as XmlElement;
                ResItem ri = new ResItem();
                res.Add(ri);
                ri.Load(riElem);
                if (ri.isRoot)
                    scanRoot.Add(ri);
            }

            for (int i = 0; i < childs.Count; ++i)
            {
                XmlElement riElem = childs[i] as XmlElement;
                var ri = res[i];
                ri.LoadRelative(riElem, this);
            }
        }
    }

    public class ResScanConfig : AssetBaseConifg<ResScanConfig>
    {
        public List<ScanJob> jobs = new List<ScanJob> ();

        public long meshSizeThreahold = 1024 * 1024;
        public long texSizeThreahold = 1024 * 1024;
        public int sfxRenderCount = 10;
        public int sfxMaxParticleCount = 10;
    }

}
#endif
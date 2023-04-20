using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;
using CFEngine;

namespace CFEngine.Editor
{
    public abstract partial class PreBuildPreProcess : IDisposable
    {
        public abstract int Priority { get; }
        public static bool build;
        public static int count;
        public List<string> files = new List<string> ();
        public List<string> invalidFiles = new List<string> ();
        public bool folder = false;

        public abstract string Name { get; }
        public void Clear ()
        {

            files.Clear ();
            invalidFiles.Clear ();
        }
        public virtual void PreProcess ()
        {
            Clear ();
        }

        protected void CopyFile (string relativeSrc, string relativeDes)
        {
//#if !UNITY_ANDROID
            string src = string.Format ("{0}/{1}", AssetsConfig.instance.ResourcePath, relativeSrc);

            string des = string.Format ("Assets/StreamingAssets/Bundles/assets/bundleres/{0}", relativeDes);

            des = des.Replace ("\\", "/");
            if (build)
            {
                int index = des.LastIndexOf ("/");
                if (index >= 0)
                {
                    string dir = des.Substring (0, index);
                    EditorCommon.CreateDir (dir);
                }
                if (File.Exists (src))
                {
                    File.Copy (src, des, true);
                }
                else
                {
                    DebugLog.AddErrorLog2 ("file not exist:{0}", src);
                }
            }
            if (build)
            {
                if (!File.Exists (des))
                {
                    invalidFiles.Add (des);
                }
                else
                {
                    files.Add (des);
                }
            }
            else
            {
                files.Add (des);
            }

            count++;
//#endif
        }

        protected void ProcessFolder(string dirPath,string listName)
        {
            if (build)
            {
                string dir = "Assets/StreamingAssets/Bundles/assets/bundleres/" + dirPath;
                DirectoryInfo targetDi = new DirectoryInfo(dir);
                if (targetDi.Exists)
                {
                    targetDi.Delete(true);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            List<string> path = new List<string>();
            DirectoryInfo di = new DirectoryInfo(string.Format("{0}/{1}", 
                AssetsConfig.instance.ResourcePath, dirPath));
            FileInfo[] files = di.GetFiles("*.bytes", SearchOption.TopDirectoryOnly);
            if (files != null && files.Length > 0)
            {
                for (int i = 0; i < files.Length; ++i)
                {
                    var fi = files[i];

                    string filename = fi.Name.Replace(".bytes", "").ToLower();
                    path.Add(filename);
                    string filePath = string.Format("{0}/{1}", dirPath,fi.Name);
                    filePath = filePath.ToLower();
                    CopyFile(filePath, filePath);
                }
            }

            if (build)
            {
                try
                {
                    string configPath = string.Format("{0}/Config/{1}.bytes", 
                        AssetsConfig.instance.ResourcePath, listName);

                    using (FileStream fs = new FileStream(configPath, FileMode.Create))
                    {
                        BinaryWriter bw = new BinaryWriter(fs);
                        short count = (short)path.Count;
                        bw.Write(count);
                        for (int i = 0; i < path.Count; ++i)
                        {
                            bw.Write(path[i].ToLower());
                        }
                    }
                    AssetDatabase.ImportAsset(configPath, ImportAssetOptions.ForceUpdate);
                }
                catch (System.Exception e)
                {
                    DebugLog.AddErrorLog(e.Message);
                }
            }
        }

        private DateTime now;

        public PreBuildPreProcess()
        {
            now = DateTime.Now;
        }

        void IDisposable.Dispose()
        {
            var last = DateTime.Now - now;
            DebugLog.AddEngineLog2("======================{0}:{1}s======================", GetType().FullName, last.TotalSeconds);
        }
    }
}

namespace JTools
{
    public abstract class TimeCalculator : IDisposable
    {
        private string tag;
        private DateTime now;

        public TimeCalculator(string tag = null)
        {
            this.tag = tag;
            now = DateTime.Now;
        }

        void IDisposable.Dispose()
        {
            var last = DateTime.Now - now;
            DebugLog.AddEngineLog2("======================{0}:{1}s======================", tag ?? GetType().FullName, last.TotalSeconds);
        }
    }
}
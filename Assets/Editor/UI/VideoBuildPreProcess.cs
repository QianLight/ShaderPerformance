using System;
using System.Collections.Generic;
using System.IO;
using CFEngine;
using CFEngine.Editor;
using Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace CFEngine.Editor
{
   
   public partial class BuildVideo : PreBuildPreProcess
   {
       public override string Name { get { return "Video"; } }

       public override int Priority
       {
           get
           {
               return 4;
           }
       }
       public override void PreProcess()
       {
           base.PreProcess();
            string dir = "Assets/StreamingAssets/Bundles/assets/bundleres/video";
            DirectoryInfo targetDi = new DirectoryInfo(dir);
            if (targetDi.Exists)
            {
                targetDi.Delete(true);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            List<string> path = new List<string>();
           DirectoryInfo di = new DirectoryInfo(string.Format("{0}/video", AssetsConfig.instance.ResourcePath));
           FileInfo[] files = di.GetFiles("*.mp4", SearchOption.TopDirectoryOnly);
           if (files != null && files.Length > 0)
           {
               for (int i = 0; i < files.Length; ++i)
               {
                   var fi = files[i];
                   string filename = fi.Name.ToLower();
                   path.Add(filename);
                   string filePath = string.Format("video/{0}", fi.Name);
                   filePath = filePath.ToLower();
                   CopyFile(filePath, filePath);
               }
           }
       }

   }
}

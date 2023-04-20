/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using Zeus.Build;

namespace Zeus.Framework.Http
{
    public class UnityHttpDownloaderBuildHelper
    {
#if TEST_HTTPDOWNLOADER
        [UnityEditor.MenuItem("test/test_httpdownloader")]
        public static void Test()
        {
            UnityHttpDownloaderBuildHelper helper = new UnityHttpDownloaderBuildHelper();
            helper.OnBeforeBuild(BuildTarget.Android);
            helper.OnBeforeBuild(BuildTarget.iOS);
            helper.OnBeforeBuild(BuildTarget.StandaloneWindows);
        }
#endif

        /// <summary>
        /// 此函数对应IBeforeBuild接口，应该在其对应的阶段调用
        /// </summary>
        /// <param name="target"></param>
        public void OnBeforeBuild(BuildTarget target)
        {
            try
            {
                string packagePath = PackageUtility.GetPackageFullPath("com.pwrd.zeus-framework.http");
                string pluginsDir = packagePath + "/Runtime/UnityHttpDownloader/Plugins";
                string[] libraryPaths = null;
                if (target == BuildTarget.Android)
                {
                    string dir = pluginsDir + "/Android";
                    libraryPaths = System.IO.Directory.GetFiles(dir, "*.aar");
                }
                else if (target == BuildTarget.iOS)
                {
                    string dir = pluginsDir + "/iOS";
                    libraryPaths = System.IO.Directory.GetFiles(dir, "*.a");
                }
                if (libraryPaths != null && libraryPaths.Length > 1)
                {
                    Dictionary<string, System.DateTime> dic = new Dictionary<string, System.DateTime>();
                    for (int i = 0; i < libraryPaths.Length; i++)
                    {
                        string[] temp = System.IO.Path.GetFileName(libraryPaths[i]).Split('-');
                        System.DateTime buildDate;
                        if (temp.Length >= 2 && System.DateTime.TryParse(temp[1], out buildDate))
                        {
                            dic.Add(libraryPaths[i], buildDate);
                        }
                        else
                        {
                            dic.Add(libraryPaths[i], System.DateTime.MinValue);
                        }
                    }
                    string maxPath = null;
                    System.DateTime maxDate = System.DateTime.MinValue;
                    foreach (var item in dic)
                    {
                        if (item.Value >= maxDate)
                        {
                            if (!string.IsNullOrEmpty(maxPath))
                            {
                                System.IO.File.Delete(maxPath);
                            }
                            maxPath = item.Key;
                            maxDate = item.Value;
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("Delete Duplicate Plugin:" + item.Key);
                            System.IO.File.Delete(item.Key);
                        }
                    }
                    AssetDatabase.Refresh();
                }

            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
        }
    }
}
#endif